using System;

namespace Server
{
	public class Sys
	{
		/// <summary>
		/// Get the total seconds from 1970.1.1 0:0:0:0 to now.
		/// </summary>
		/// <returns>The time stamp.</returns>
		public static long GetTimeStamp()
		{
			TimeSpan ts = DateTime.UtcNow - new DateTime (1970, 1, 1, 0, 0, 0, 0);
			return Convert.ToInt64 (ts.TotalSeconds);
		}
	}
}

