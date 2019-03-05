using System;
using System.Collections;

namespace Server
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			DataMgr dataMgr = new DataMgr ();
			RoomMgr roomMgr = new RoomMgr ();
			ServNet serv = new ServNet ();
			serv.proto = new ProtocolBytes ();
			serv.Start ("127.0.0.1", 1234);

			while (true) {
				string str = Console.ReadLine ();
				switch (str) {
					case "quit":
						serv.Close ();
					return;
					case "print":
						serv.Print ();
						break;
				}
			}
		}
	}
}
