using System;

namespace Server
{
	public class Player
	{
		public string id;
		public Conn conn;
		public PlayerData data;
		public PlayerTempData tempData;

		public Player(string id, Conn conn)
		{
			this.id = id;
			this.conn = conn;
			tempData = new PlayerTempData ();
		}

		#region Player Operation
		public void Send(ProtocolBase protoBase)
		{
			if (conn == null)
				return;

			ServNet.instance.Send (conn, protoBase);
		}

		public static bool KickOff(string id, ProtocolBase protoBase)
		{
			Console.WriteLine ("Bug again start.");
			Conn[] conns = ServNet.instance.conns;
			for (int i = 0; i < conns.Length; i++) {
				Conn conn = conns [i];
				if (conn == null || conn.isUse == false)
					continue;
				if (conn.player == null)
					continue;
				
				if (conn.player.id == id) {
					lock (conn.player) {
						if (protoBase != null) {
							ProtocolBytes protocol = (ProtocolBytes)protoBase;
							protocol.AddInt32 (-1);
							conn.player.Send ((ProtocolBase)protocol);
						}
						
						return conn.player.Logout ();
					}
				}
			}
			return true;
		}

		public bool Logout()
		{
			ServNet.instance.handlePlayerEvent.OnLogout (this);

			if (!DataMgr.instance.SavePlayer (this)) {
				return false;
			}

			conn.player = null;
			conn.Close ();
			return true;
		}
		#endregion

	}
}

