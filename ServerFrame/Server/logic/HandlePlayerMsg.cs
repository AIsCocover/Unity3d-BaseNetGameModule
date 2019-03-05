using System;

namespace Server
{
	public partial class HandlePlayerMsg
	{
		// Protocol: GetScore
		// Rec Args: None
		// Return: int[score]
		public void MsgGetScore(Player player, ProtocolBase protoBase)
		{
			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("GetScore");
			protocolRet.AddInt32 (player.data.score);
			player.Send (protocolRet);
			Console.WriteLine ("[HandlePlayerMsg.MsgGetScore] Receive GetScore protocol: score[" + player.data.score + "]");
		}
			
		// Protocol: GetAchieve
		// Rec Args: None
		// Return: int[win] int[fail]
		public void MsgGetAchieve(Player player, ProtocolBase protoBase)
		{
			ProtocolBytes protocolRet = new ProtocolBytes ();
			protocolRet.AddString ("GetAchieve");
			protocolRet.AddInt32 (player.data.win);
			protocolRet.AddInt32 (player.data.fail);

			player.Send (protocolRet);
			Console.WriteLine ("[HandlePlayerMsg.MsgGetAchieve] Receive GetAchieve protocol from " + player.id);
		}
			
	}
}

