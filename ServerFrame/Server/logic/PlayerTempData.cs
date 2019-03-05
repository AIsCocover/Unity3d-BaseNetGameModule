using System;

namespace Server
{
	public class PlayerTempData
	{
		public enum Status 
		{
			None,
			Room,
			Fight,
		}
		public Status status = Status.None;

		public Room room;
		public int team = 1;
		public bool isOwner = false;
	}
}

