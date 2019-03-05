using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMgr {

	public static Connection servConn = new Connection();
	//public static Connection platformConn = new Connection();

	public static void Update()
	{
		servConn.Update ();
		//platformConn.Update ();
	}

	public static ProtocolBase GetHeartBeatProtocol()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("HeartBeat");
		return protocol;
	}

}
