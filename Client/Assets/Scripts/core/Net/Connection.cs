using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;

public class Connection {

	private static int BUFFER_SIZE = 1024;

	public enum Status
	{
		None,
		Connected,
	};   
	public Status status = Status.None;

	public ProtocolBase proto;
	public MsgDistribution msgDist = new MsgDistribution ();
	public float lastTickTime = 0;                    
	public float heartBeatTime = 120;

	private Socket socket;
	private byte[] readBuff = new byte[BUFFER_SIZE];
	private int buffCount = 0;

	// Handle data package
	private Int32 msgLength;
	private byte[] lenBuff = new byte[sizeof(Int32)];

	#region Operation
	public bool Connect(string host, int port)
	{
		try {
			socket = new Socket (
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp);
			socket.Connect (host, port);

			socket.BeginReceive (
				readBuff,
				buffCount,
				BUFFER_SIZE-buffCount,
				SocketFlags.None,
				ReceiveCb,
				readBuff);
			status = Status.Connected;
			Console.WriteLine ("[Connection.Connect] Connected to " + socket.RemoteEndPoint.ToString ());
			return true;
		} catch (Exception ex) {
			Console.WriteLine ("[Connection.Connect] Can not connect to " + host + ":" + port.ToString () + "." + ex.Message);
			status = Status.None;
			return false;
		}
	}

	public bool Disconnect()
	{
		try {
			socket.Close();
			return true;
		} catch (Exception ex) {
			Console.WriteLine ("[Connection.Disconnect] Can not disconnected to " + socket.RemoteEndPoint.ToString () + ex.Message);
			return false;
		}
	}

	public bool Send(ProtocolBase protoBase)
	{
		if (status != Status.Connected) {
			Console.WriteLine ("[Connection.Send] Connection is not exist.");
			return false;
		}
			
		byte[] strBytes = protoBase.Encode ();
		byte[] lenBytes = BitConverter.GetBytes (strBytes.Length);
		byte[] sendBytes = lenBytes.Concat (strBytes).ToArray ();

		socket.Send (sendBytes);
		Console.WriteLine ("[Connection.Send] Send protocol: " + protoBase.GetDesc ());
		return true;
	}

	public bool Send(ProtocolBase protoBase, string cbName, MsgDistribution.Delegate cb)
	{
		if (status != Status.Connected)
			return false;
		msgDist.AddOnceListener (cbName, cb);
		return Send (protoBase);
	}

	public bool Send(ProtocolBase protoBase, MsgDistribution.Delegate cb)
	{
		string cbName = protoBase.GetName ();
		return Send (protoBase, cbName, cb);
	}

	public void Update()
	{
		msgDist.Update ();

		if (status == Status.Connected) {
			if (Time.time - lastTickTime > heartBeatTime) {
				ProtocolBase protocol = NetMgr.GetHeartBeatProtocol ();
				Send (protocol);
				lastTickTime = Time.time;
			}
		}
	}
	#endregion

	#region Help Function
	void ReceiveCb(IAsyncResult ar)
	{
		try {
			int count = socket.EndReceive(ar);
			if(count>0)
			{
				buffCount += count;
				ProcessData();
			}
			socket.BeginReceive (
				readBuff,
				buffCount,
				BUFFER_SIZE-buffCount,
				SocketFlags.None,
				ReceiveCb,
				readBuff);
		} catch (Exception ex) {
			Console.WriteLine ("[Connection.ReceiveCb] Receive data fail. " + ex.Message);
			status = Status.None;	
		}
	}

	void ProcessData()
	{
		if (buffCount < sizeof(Int32))
			return;

		Array.Copy (readBuff, lenBuff, sizeof(Int32));
		msgLength = BitConverter.ToInt32 (lenBuff, 0);
		if (buffCount < sizeof(Int32) + msgLength)
			return;

		ProtocolBase protocol = proto.Decode (readBuff, sizeof(Int32), msgLength);
		lock (msgDist.msgList) {
			msgDist.msgList.Add (protocol);
		}

		int count = buffCount - sizeof(Int32) - msgLength;
		Array.Copy (readBuff, sizeof(Int32) + msgLength, readBuff, 0, count);
		buffCount = count;
		if (buffCount > 0) {
			ProcessData ();
		}
	}
	#endregion
}
