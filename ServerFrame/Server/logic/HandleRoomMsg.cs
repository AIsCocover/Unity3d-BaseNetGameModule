using System;

namespace Server
{
	public partial class HandlePlayerMsg
	{
		// Protocol: GetRoomList
		// Rec Args: None
		// Return: int[roomCount] int[room1playercount] int[room1status] int[room2playercount] int[oom2status]...
		public void MsgGetRoomList(Player player, ProtocolBase protoBase)
		{
			Console.WriteLine ("[HandleRoomMsg.MsgGetRoomList] Receive GetRoomList protocol from " + player.id);

			player.Send (RoomMgr.instance.GetRoomList ());
		}

		// Protocol: CreateRoom
		// Rec Args: None
		// Return: int[0-success -1-fail]
		public void MsgCreateRoom(Player player, ProtocolBase protoBase)
		{
			Console.WriteLine ("[HandleRoomMsg.MsgCreateRoom] Receive CreateRoom protocol from " + player.id);

			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("CreateRoom");

			if (player.tempData.status != PlayerTempData.Status.None) {
				Console.WriteLine ("[HandleRoomMsg.MsgCreateRoom] Create room fail, " + player.id + " is not in None Status");
				protocolRet.AddInt32 (-1);
				player.Send (protocolRet);
				return;
			}

			RoomMgr.instance.CreateRoom (player);
			protocolRet.AddInt32 (0);
			player.Send (protocolRet);
		}

		// Protocol: GetRoomInfo
		// Rec Args: None
		// Return: int[playercount] int[player1id] int[player1team] int[player1win] int[player1fail] int[player1owner]...
		public void MsgGetRoomInfo(Player player, ProtocolBase protoBase)
		{
			Console.WriteLine ("[HandleRoomMsg.MsgGetRoomInfo] Receive GetRoomInfo protocol from " + player.id);

			if (player.tempData.status != PlayerTempData.Status.Room) {
				Console.WriteLine ("[HandleRoomMsg.MsgGetRoomInfo] Get room info fail, " + player.id + " is not in Room Status");
				return;
			}
			player.Send (player.tempData.room.GetRoomInfo ());
		}

		// Protocol: EnterRoom
		// Rec Args: int[roomindex]
		// Return: int[0-success -1-fail]
		public void MsgEnterRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolBytes protocol = (ProtocolBytes)protoBase;
			int start = 0;
			string protoName = protocol.GetString (start, ref start);
			int index = protocol.GetInt (start, ref start);
			Console.WriteLine ("[HandleRoomMsg.MsgEnterRoom] Receive EnterRoom " + index.ToString () + " protocol from " + player.id);

			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("EnterRoom");

			if (index < 0 || index >= RoomMgr.instance.roomList.Count) {
				Console.WriteLine ("[HandleRoomMsg.MsgEnterRoom] " + player.id + " enter room fail, index is illegal.");
				protocolRet.AddInt32 (-1);
				player.Send (protocolRet);
				return;
			}

			Room room = RoomMgr.instance.roomList [index];
			if (room.status != Room.Status.Preparing) {
				Console.WriteLine ("[HandleRoomMsg.MsgEnterRoom] " + player.id + " enter room fail, room is not in Preparing Status.");
				protocolRet.AddInt32 (-1);
				player.Send (protocolRet);
				return;
			}
			if (room.AddPlayer (player)) {
				room.Broadcast (room.GetRoomInfo ());
				protocolRet.AddInt32 (0);
				player.Send (protocolRet);
			} else {
				Console.WriteLine ("[HandleRoomMsg.MsgEnterRoom] " + player.id + " enter room fail, room is full.");
				protocolRet.AddInt32 (-1);
				player.Send (protocolRet);
			}
		}

		// Protocol: LeaveRoom
		// Rec Args: None
		// Return: int[0-success -1-fail]
		public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
		{
			Console.WriteLine ("[HandleRoomMsg.MsgLeaveRoom] Receive LeaveRoom protocol from " + player.id);

			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("LeaveRoom");

			if (player.tempData.status != PlayerTempData.Status.Room) {
				Console.WriteLine ("[HandleRoomMsg.MsgLeaveRoom] Leave room fail, " + player.id + " is not in Room Status");
				protocolRet.AddInt32 (-1);
				player.Send (protocolRet);
				return;
			}

			protocolRet.AddInt32 (0);
			player.Send (protocolRet);

			Room room = player.tempData.room;
			RoomMgr.instance.LeaveRoom (player);

			if (room != null)
				room.Broadcast (room.GetRoomInfo ());
		}
	}
}

