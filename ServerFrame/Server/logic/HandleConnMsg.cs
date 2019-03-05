using System;

namespace Server
{
	public partial class HandleConnMsg
	{
		// Protocol: HeartBeat
		// Rec Args: None
		// Return: None
		public void MsgHeartBeat(Conn conn, ProtocolBase protoBase)
		{
			conn.lastTickTime = Sys.GetTimeStamp ();
			Console.WriteLine ("[HandleConnMsg.MsgHeartBeat] Update HeartBeat Time: " + conn.GetAddress () + " : " + conn.lastTickTime.ToString ());
		}

		// Protocol: Register
		// Rec Args: string[id] string[pw]
		// Return: int[0-success] int[-1-fail]
		public void MsgRegister(Conn conn, ProtocolBase protoBase)
		{
			ProtocolBytes protocol = (ProtocolBytes)protoBase;
			int start = 0;
			string protoName = protocol.GetString (start, ref start);
			string id = protocol.GetString (start, ref start);
			string pw = protocol.GetString (start, ref start);
			Console.WriteLine ("[HandleConnMsg.MsgRegister] Receive Register protocol: id[" + id + "] pw[" + pw + "]");

			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("Register");
			if (!DataMgr.instance.Register (id, pw)) {
				protocolRet.AddInt32 (-1);
			} else {
				protocolRet.AddInt32 (0);
			}

			DataMgr.instance.CreatePlayer (id);
			conn.Send (protocolRet);
		}

		// Protocol: Login
		// Rec Args: string[id] string[pw]
		// Return: int[0-success] int[-1-fail]
		public void MsgLogin(Conn conn, ProtocolBase protoBase)
		{
			ProtocolBytes protocol = (ProtocolBytes)protoBase;
			int start = 0;
			string protoName = protocol.GetString (start, ref start);
			string id = protocol.GetString (start, ref start);
			string pw = protocol.GetString (start, ref start);
			Console.WriteLine ("[HandleConnMsg.MsgLogin] Receive Login protocol: id[" + id + "] pw[" + pw + "]");


			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("Login");

			if (!DataMgr.instance.CheckPassword (id, pw)) {
				protocolRet.AddInt32 (-1);
				conn.Send (protocolRet);
				return;
			}
				
			ProtocolBytes protocolLogout = new ProtocolBytes ();
			protocolLogout.AddString ("Logout");
			if (!Player.KickOff (id, protocolLogout)) {
				protocolRet.AddInt32 (-1);
				conn.Send (protocolRet);
				return;
			}

			PlayerData playerData = DataMgr.instance.GetPlayerData (id);
			if (playerData == null) {
				protocolRet.AddInt32 (-1);
				conn.Send (protocolRet);
				return;
			}

			conn.player = new Player (id, conn);
			conn.player.data = playerData;
			ServNet.instance.handlePlayerEvent.OnLogin (conn.player);

			protocolRet.AddInt32 (0);
			conn.Send (protocolRet);
		}

		// Protocol: Logout
		// Rec Args: int[logout reason]
		// Return: int[0-logout normally]
		public void MsgLogout(Conn conn, ProtocolBytes protoBase)
		{
			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("Logout");
			protocolRet.AddInt32 (0);
			Console.WriteLine ("[HandleConnMsg.MsgLogout] Receive Logout protocol.");

			conn.Send (protocolRet);

			if (conn.player != null) {
				conn.player.Logout ();
			} else {
				conn.Close ();
			}
		}
			
	}
}

