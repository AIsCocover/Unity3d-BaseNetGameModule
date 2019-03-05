using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Reflection;

namespace Server
{
	public class ServNet
	{
		public static ServNet instance;
		public ServNet()
		{
			instance = this;
		}

		public ProtocolBase proto;

		public Socket listenfd;
		public Conn[] conns;
		public int maxConn = 50;

		// Timer
		public long heartBeatTime = 180;
		System.Timers.Timer timer = new System.Timers.Timer(1000);

		// Msg Dispatch
		public HandleConnMsg handleConnMsg = new HandleConnMsg();
		public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
		public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();

		#region Help Function
		/// <summary>
		/// Find a position from conns and return its index.
		/// </summary>
		/// <returns>The index.</returns>
		int NewIndex()
		{
			if (conns == null)
				return -1;
			
			for (int i = 0; i < conns.Length; i++) {
				Conn conn = conns [i];
				if (conn == null) {
					conn = new Conn ();
					return i;
				}
				if (conn.isUse == false)
					return i;
			}

			return -1;
		}
		/// <summary>
		/// Check all the conn and disconnect it which appear heartbeat.
		/// </summary>
		void HeartBeat()
		{
			long nowTime = Sys.GetTimeStamp ();

			for (int i = 0; i < conns.Length; i++) {
				Conn conn = conns [i];
				if (conn == null || conn.isUse == false)
					continue;
				
				if (nowTime - conn.lastTickTime > heartBeatTime) {
					Console.WriteLine ("[ServNet.HeartBeat] HeartBeat case disconnect with " + conn.GetAddress ());
					lock (conn) {
						conn.Close ();
					}
				}
			}
		}

		/// <summary>
		/// Main timer handler.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandleMainTimer(object sender, System.Timers.ElapsedEventArgs e)
		{
			HeartBeat ();
			timer.Start ();
		}
		#endregion

		#region ServNet Operation
		/// <summary>
		/// Start a server with host and port.
		/// </summary>
		/// <param name="host">Host.</param>
		/// <param name="port">Port.</param>
		public void Start(string host, int port)
		{
			// Timer
			timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
			timer.AutoReset = false;
			timer.Enabled = true;

			conns = new Conn[maxConn];
			for (int i = 0; i < conns.Length; i++) {
				conns [i] = new Conn ();
			}

			listenfd = new Socket (
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp);
		
			IPAddress ipAdr = IPAddress.Parse (host);
			IPEndPoint ipEp = new IPEndPoint (ipAdr, port);
			listenfd.Bind (ipEp);
			listenfd.Listen (maxConn);

			listenfd.BeginAccept (
				AcceptCb,
				null);
			Console.WriteLine ("[ServNet.Start] Server Start...");
		}

		/// <summary>
		/// Close the server.
		/// </summary>
		public void Close()
		{
			for (int i = 0; i < conns.Length; i++) {
				Conn conn = conns [i];
				if (conn == null || conn.isUse == false)
					continue;

				lock (conn) {
					conn.Close ();
				}
			}
		}

		/// <summary>
		/// Handle the protocol and send to client by conn.
		/// </summary>
		/// <param name="conn">Conn.</param>
		/// <param name="protoBase">Proto base.</param>
		public void Send(Conn conn, ProtocolBase protoBase)
		{
			byte[] strBytes = protoBase.Encode ();
			byte[] lenBytes = BitConverter.GetBytes (strBytes.Length);
			byte[] sendBytes = lenBytes.Concat (strBytes).ToArray ();
		
			try {
				conn.socket.Send(sendBytes);
			} catch (Exception ex) {
				Console.WriteLine ("[ServNet.Send] Send bytes fail. " + ex.Message);
			}
		}
		/// <summary>
		/// Broadcast protoBase.
		/// </summary>
		/// <param name="protoBase">Proto base.</param>
		public void Broadcast(ProtocolBase protoBase)
		{
			for (int i = 0; i < conns.Length; i++) {
				Conn conn = conns [i];
				if (conn == null || conn.isUse == false)
					continue;

				Send (conn, protoBase);
			}
		}

		public void Print()
		{
			Console.WriteLine ("=============== Server login info ===============");
			for (int i = 0; i < conns.Length; i++) {
				Conn conn = conns [i];
				if (conn == null || conn.isUse == false)
					continue;
				string str = "[" + conn.GetAddress () + "] ";
				if (conn.player != null)
					str += conn.player.id + " ";
				Console.WriteLine (str);
			}
		}
		#endregion

		#region Listen-Cycle
		void AcceptCb(IAsyncResult ar)
		{
			try {
				Socket socket = listenfd.EndAccept(ar);
				int index = NewIndex();

				if(index < 0)
				{
					Console.WriteLine("[ServNet.AcceptCb] conns is full.");
				} else {
					Conn conn = conns[index];
					conn.Init(socket);

					conn.socket.BeginReceive(
						conn.readBuff,
						conn.buffCount,
						conn.BufferRemain(),
						SocketFlags.None,
						ReceiveCb,
						conn);
				}
				listenfd.BeginAccept(
					AcceptCb,
					null);
			} catch (Exception ex) {
				Console.WriteLine ("[ServNet.AcceptCb] Accept socket fail. " + ex.Message);
			}
		}

		void ReceiveCb(IAsyncResult ar)
		{
			Conn conn = (Conn)ar.AsyncState;
	
			try {
				int count = conn.socket.EndReceive(ar);
				if(count > 0){
					conn.buffCount += count;
					ProcessData(conn);
				}
			
				conn.socket.BeginReceive(
					conn.readBuff,
					conn.buffCount,
					conn.BufferRemain(),
					SocketFlags.None,
					ReceiveCb,
					conn);
			} catch (Exception ex) {
				Console.WriteLine ("[ServNet.ReceiveCb] Receive byte fail." + ex.Message);
				conn.Close ();
			}
		}
	
		void ProcessData(Conn conn)
		{
			if (conn.buffCount < sizeof(Int32))
				return;

			Array.Copy (conn.readBuff, conn.lenBuff, sizeof(Int32));
			conn.msgLength = BitConverter.ToInt16 (conn.lenBuff, 0);
			if (conn.buffCount < sizeof(Int32) + conn.msgLength)
				return;

			ProtocolBase protocol = proto.Decode (conn.readBuff, sizeof(Int32), conn.msgLength);
			HandleMsg (conn, protocol);

			int count = conn.buffCount - sizeof(Int32) - conn.msgLength;
			Array.Copy (conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
			conn.buffCount = count;
			if (conn.buffCount > 0) {
				ProcessData (conn);
			}
		}
		/// <summary>
		/// According to the protocol name, call different function to handle problem.
		/// </summary>
		/// <param name="conn">Conn.</param>
		/// <param name="protoBase">Proto base.</param>
		void HandleMsg(Conn conn, ProtocolBase protoBase)
		{
			string name = protoBase.GetName ();
			string methodName = "Msg" + name;

			if (conn.player == null || name == "HeartBeat" || name == "Logout") {
				MethodInfo mm = handleConnMsg.GetType ().GetMethod (methodName);
				if (mm == null) {
					Console.WriteLine ("[ServNet.HandleMsg] ConnMsgHandler has no appropriate function.");
					return;
				}
				Object[] obj = new object[]{ conn, protoBase };
				Console.WriteLine ("[ServNet.HandleMsg] Handle ConnMsg : " + conn.GetAddress () + " [" + name + "]");
				mm.Invoke (handleConnMsg, obj);
			} else {
				MethodInfo mm = handlePlayerMsg.GetType ().GetMethod (methodName);
				if (mm == null) {
					Console.WriteLine ("[ServNet.HandleMsg] PlayerMsgHandler has no appropriate function.");
					return;
				}
				Object[] obj = new object[]{ conn.player, protoBase };
				mm.Invoke (handlePlayerMsg, obj);
			}
		}
		#endregion
	}
}

