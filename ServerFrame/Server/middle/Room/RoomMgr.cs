using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
	public class RoomMgr
	{
		public static RoomMgr instance;
		public RoomMgr()
		{
			instance = this;
		}

		public List<Room> roomList = new List<Room>();

		#region Operation
		/// <summary>
		/// Create a room, owner is player.
		/// </summary>
		/// <param name="player">Player.</param>
		public void CreateRoom(Player player)
		{
			Room room = new Room ();
			lock (roomList) {
				roomList.Add (room);
				room.AddPlayer (player);
			}
		}
		/// <summary>
		/// player leave the room.
		/// </summary>
		/// <param name="player">Player.</param>
		public void LeaveRoom(Player player)
		{
			PlayerTempData tempData = player.tempData;
			if (tempData.status != PlayerTempData.Status.Room)
				return;

			Room room = tempData.room;
			lock (roomList) {
				room.DelPlayer (player.id);
				if (room.playerList.Count == 0) {
					roomList.Remove (room);
				}
			}
		}
		/// <summary>
		/// Get the room list information.
		/// </summary>
		/// <returns>The room list.</returns>
		public ProtocolBytes GetRoomList()
		{
			ProtocolBytes protocol = new ProtocolBytes ();
			protocol.AddString ("GetRoomList");
			protocol.AddInt32 (roomList.Count);
			foreach (Room room in roomList) {
				protocol.AddInt32 (room.playerList.Count);
				protocol.AddInt32 ((int)room.status);
			}

			return protocol;
		}
		#endregion
	}
}

