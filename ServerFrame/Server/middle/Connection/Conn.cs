using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
	public class Conn
	{
		private const int BUFFER_SIZE = 1024;

		public Socket socket;
		public bool isUse = false;
		public byte[] readBuff = new byte[BUFFER_SIZE];

		// Handle Data Package
		public int buffCount = 0;
		public Int32 msgLength = 0;
		public byte[] lenBuff = new byte[sizeof(Int32)];

		public Player player;

		public long lastTickTime = 0;

		public Conn()
		{
			readBuff = new byte[BUFFER_SIZE];
		}

		/// <summary>
		/// Initialize a Conn with socket.
		/// </summary>
		/// <param name="socket">Socket.</param>
		public void Init(Socket socket)
		{
			this.socket = socket;
			isUse = true;
			buffCount = 0;
			lastTickTime = Sys.GetTimeStamp ();
		}
		/// <summary>
		/// Get client's address.
		/// </summary>
		/// <returns>The address.</returns>
		public string GetAddress()
		{
			if (!isUse)
				return "[Conn.GetAddress] Conn not running.";
			return socket.RemoteEndPoint.ToString ();
		}
		/// <summary>
		/// Return the rest length of readBuff.
		/// </summary>
		/// <returns>The remain.</returns>
		public int BufferRemain()
		{
			return BUFFER_SIZE - buffCount;
		}
		/// <summary>
		/// Close the conn.
		/// </summary>
		public void Close()
		{
			if (isUse == false)
				return;
			if(player != null)
			{
				player.Logout();
				return;
			}

			Console.WriteLine ("[Conn.Close] Disconnect with " + GetAddress ());
			socket.Shutdown (SocketShutdown.Both);
			socket.Close ();
			isUse = false;
		}

		public void Send(ProtocolBase protoBase)
		{
			ServNet.instance.Send (this, protoBase);
		}
	}
}

