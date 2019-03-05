using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Server
{
	public class DataMgr
	{
		MySqlConnection sqlConn;

		public static DataMgr instance;
		public DataMgr()
		{
			instance = this;
			Connect ();
		}

		#region Help Function
		/// <summary>
		/// Connect to mysql server/
		/// </summary>
		void Connect()
		{
			string connStr = "Database=game;DataSource='127.0.0.1';";
			connStr += "User Id=root;Password=123456;port=3306;";
			sqlConn = new MySqlConnection (connStr);
			try {
				sqlConn.Open();
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.Connect] Can't open the connection. " + ex.Message);
			}
		}
		/// <summary>
		/// Check the string , if it has illegal characters.
		/// </summary>
		/// <returns><c>true</c> if this instance is safe string the specified str; otherwise, <c>false</c>.</returns>
		/// <param name="str">String.</param>
		bool IsSafeStr(string str)
		{
			return !Regex.IsMatch (str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
		}
		/// <summary>
		/// Check if the id has benn store in database.
		/// </summary>
		/// <returns><c>true</c> if this instance can register the specified id; otherwise, <c>false</c>.</returns>
		/// <param name="id">Identifier.</param>
		bool CanRegister(string id)
		{
			if (!IsSafeStr (id)) {
				Console.WriteLine ("[DataMgr.CanRegister] illegal characters.");
				return false;
			}

			string formatStr = "select * from user where id='{0}';";
			string cmdStr = string.Format (formatStr, id);
			MySqlCommand cmd = new MySqlCommand (cmdStr, sqlConn);

			try {
				MySqlDataReader dataReader = cmd.ExecuteReader();
				bool hasRow = dataReader.HasRows;
				dataReader.Close();
				return !hasRow;
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.CanRegister] Read data fail. " + ex.Message);
				return false;
			}
		}
			
		#endregion
		/// <summary>
		/// Register a new account by id and pw.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="pw">Pw.</param>
		public bool Register(string id, string pw)
		{
			if (!IsSafeStr (id) || !IsSafeStr (pw)) {
				Console.WriteLine ("[DataMgr.Register] Illegal characters.");
				return false; 
			}

			if (!CanRegister (id)) {
				Console.WriteLine ("[DataMgr.Register] Can't register.");
				return false;
			}

			string formatStr = "insert into user set id='{0}', pw='{1}';";
			string cmdStr = string.Format (formatStr, id, pw);
			MySqlCommand cmd = new MySqlCommand (cmdStr, sqlConn);

			try {
				cmd.ExecuteNonQuery();
				return true;
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.Register] Execute command fail. " + ex.Message);
				return false;
			}
		}
		/// <summary>
		/// Check the password.
		/// </summary>
		/// <returns><c>true</c>, if password was checked, <c>false</c> otherwise.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="pw">Pw.</param>
		public bool CheckPassword(string id, string pw)
		{
			if (!IsSafeStr (id) || !IsSafeStr (pw)) {
				Console.WriteLine ("[DataMgr.CheckPassword] Illegal characters.");
				return false;
			}

			string formatStr = "select * from user where id='{0}' and pw='{1}';";
			string cmdStr = string.Format (formatStr, id, pw);
			MySqlCommand cmd = new MySqlCommand (cmdStr, sqlConn);

			try {
				MySqlDataReader dataReader = cmd.ExecuteReader();
				bool hasRow = dataReader.HasRows;
				dataReader.Close();
				return hasRow;
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.CheckPassword] Read data fail. " + ex.Message);
				return false;
			}
		}
		/// <summary>
		/// Create a playerData for player whose id is args' id.
		/// </summary>
		/// <returns><c>true</c>, if player was created, <c>false</c> otherwise.</returns>
		/// <param name="id">Identifier.</param>
		public bool CreatePlayer(string id)
		{
			if (!IsSafeStr (id)) {
				Console.WriteLine ("[DataMgr.CreatePlayer] Illegal characters.");
				return false;
			}

			PlayerData playerData = new PlayerData ();
			MemoryStream stream = new MemoryStream ();
			BinaryFormatter formatter = new BinaryFormatter ();

			try {
				formatter.Serialize(stream, playerData);
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.CreatePlayer] Serialize data fail. " + ex.Message);
				return false;
			}

			byte[] dataBytes = stream.ToArray ();

			string formatStr = "insert into player set id='{0}', data=@data;";
			string cmdStr = string.Format (formatStr, id);
			MySqlCommand cmd = new MySqlCommand (cmdStr, sqlConn);
			cmd.Parameters.Add ("@data", MySqlDbType.Blob);
			cmd.Parameters [0].Value = dataBytes;

			try {
				cmd.ExecuteNonQuery();
				return true;
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.CreatePlayer] Execute command fail. " + ex.Message);
				return false;
			}
		}
		/// <summary>
		/// Get playerdata according by id.
		/// </summary>
		/// <returns>The player data.</returns>
		/// <param name="id">Identifier.</param>
		public PlayerData GetPlayerData(string id)
		{
			PlayerData playerData = null;
			if (!IsSafeStr (id)) {
				Console.WriteLine ("[DataMgr.GetPlayerData] Illegal characters.");
				return playerData;
			}

			byte[] dataBytes = new byte[1];
			string formatStr = "select * from player where id='{0}';";
			string cmdStr = string.Format (formatStr, id);
			MySqlCommand cmd = new MySqlCommand (cmdStr, sqlConn);

			try {
				MySqlDataReader dataReader = cmd.ExecuteReader();
				bool hasRow = dataReader.HasRows;
				if(!hasRow)
				{
					Console.WriteLine("[DataMgr.GetPlayerData] data not exist.");
					return playerData;
				}
				dataReader.Read();
					
				long len = dataReader.GetBytes(1, 0, null, 0, 0);
				dataBytes = new byte[len];
				dataReader.GetBytes(1, 0, dataBytes, 0, (int)len);
				dataReader.Close();
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.GetPlayerData] Read data fail. " + ex.Message);
				return playerData;
			}

			MemoryStream stream = new MemoryStream (dataBytes);
			BinaryFormatter formatter = new BinaryFormatter ();

			try {
				playerData = (PlayerData)formatter.Deserialize(stream);
				return playerData;
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.GetPlayerData] Deserialize stream fail. " + ex.Message);
				return playerData;
			}
		}
		/// <summary>
		/// Save all the player's data into database.
		/// </summary>
		/// <returns><c>true</c>, if player was saved, <c>false</c> otherwise.</returns>
		/// <param name="player">Player.</param>
		public bool SavePlayer(Player player)
		{
			string id = player.id;	
			PlayerData playerData = player.data;
			MemoryStream stream = new MemoryStream ();
			BinaryFormatter formatter = new BinaryFormatter ();

			try {
				formatter.Serialize(stream, playerData);
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.SavePlayer] Serialize data fail. " + ex.Message);
				return false;
			}

			byte[] dataBytes = stream.ToArray ();

			string formatStr = "update player set data=@data where id='{0}';";
			string cmdStr = string.Format (formatStr, id);
			MySqlCommand cmd = new MySqlCommand (cmdStr, sqlConn);
			cmd.Parameters.Add ("@data", MySqlDbType.Blob);
			cmd.Parameters [0].Value = dataBytes;

			try {
				cmd.ExecuteNonQuery();
				return true;
			} catch (Exception ex) {
				Console.WriteLine ("[DataMgr.SavePlayer] Execute command fail. " + ex.Message);
				return false;
			}
		}

	}
}

