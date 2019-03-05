using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
	public class Room
	{
		public enum Status
		{
			Preparing = 1,
			Fight = 2,
		}
		public Status status = Status.Preparing;

		public int maxPlayerCount = 4;
		public Dictionary<string,Player> playerList = new Dictionary<string, Player>();

		#region Operation
		/// <summary>
		/// Add a player into room.
		/// </summary>
		/// <returns><c>true</c>, if player was added, <c>	false</c> otherwise.</returns>
		/// <param name="player">Player.</param>
		public bool AddPlayer(Player player)
		{
			lock (playerList) {
				if (playerList.Count >= maxPlayerCount)
					return false;

				PlayerTempData tempData = player.tempData;
				tempData.room = this;
				tempData.status = PlayerTempData.Status.Room;
				tempData.team = SwichTeam ();

				if (playerList.Count == 0)
					tempData.isOwner = true;

				string id = player.id;
				playerList.Add (id, player);
				return true;
			}
		}
		/// <summary>
		/// Delete a player from the room.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public void DelPlayer(string id)
		{
			lock (playerList) {
				if (!playerList.ContainsKey (id))
					return;

				PlayerTempData tempData = playerList [id].tempData;
				tempData.status = PlayerTempData.Status.None;
				bool isOwner = tempData.isOwner;
				playerList.Remove (id);

				if (isOwner)
					UpdateOwner ();
			}
		}
		/// <summary>
		/// Broadcast message to all player who in the room.
		/// </summary>
		/// <param name="protoBase">Proto base.</param>
		public void Broadcast(ProtocolBase protoBase)
		{
			foreach (Player player in playerList.Values) {
				player.Send (protoBase);
			}
		}
		/// <summary>
		/// Get the room information.
		/// </summary>
		/// <returns>The room info.</returns>
		public ProtocolBytes GetRoomInfo()
		{
			ProtocolBytes protocol = new ProtocolBytes ();
			protocol.AddString ("GetRoomInfo");
			protocol.AddInt32 (playerList.Count);
			foreach (Player player in playerList.Values) {
				protocol.AddString (player.id);
				protocol.AddInt32 (player.tempData.team);
				protocol.AddInt32 (player.data.win);
				protocol.AddInt32 (player.data.fail);
				int isOwner = player.tempData.isOwner ? 1 : 0;
				protocol.AddInt32 (isOwner);
			}

			return protocol;
		}
		#endregion

		#region Help Function
		/// <summary>
		/// Dispatch a team automatically.
		/// </summary>
		/// <returns>The team.</returns>
		int SwichTeam()
		{
			int count1 = 0;
			int count2 = 0;

			foreach (Player player in playerList.Values) {
				if (player.tempData.team == 1)
					count1++;
				if (player.tempData.team == 2)
					count2++;
			}

			return count1 >= count2 ? 2 : 1;
		}
		/// <summary>
		/// After the captain leave room, select a new room owner.
		/// </summary>
		void UpdateOwner()
		{
			if (playerList.Count <= 0)
				return;

			foreach (Player player in playerList.Values) {
				player.tempData.isOwner = false;
			}

			Player owner = playerList.Values.First ();
			owner.tempData.isOwner = true;
		}
		#endregion
	}
}

