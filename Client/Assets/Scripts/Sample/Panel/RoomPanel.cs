using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : PanelBase {

	private List<Transform> playerList = new List<Transform> ();
	private Button startBtn;
	private Button closeBtn;

	#region Operation-Cycle
	public override void Init (params object[] args)
	{
		base.Init (args);
		base.OnShowing ();
		skinPath = "RoomPanel";
		layer = PanelMgr.PanelLayer.Panel;
	}

	public override void OnShowing ()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		for (int i = 0; i < 6; i++) {
			string name = "playerPrefab" + i.ToString ();
			Transform playerTrans = skinTrans.Find (name);
			playerList.Add (playerTrans);
		}
		startBtn = skinTrans.Find ("startBtn").GetComponent<Button> ();
		closeBtn = skinTrans.Find ("closeBtn").GetComponent<Button> ();

		startBtn.onClick.AddListener (OnStartButtonClick);
		closeBtn.onClick.AddListener (OnCloseButtonClick);

		NetMgr.servConn.msgDist.AddListener ("GetRoomInfo", RecvGetRoomInfo);
		NetMgr.servConn.msgDist.AddListener ("Fight", RecvFight);

		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("GetRoomInfo");
		NetMgr.servConn.Send (protocol);
	}

	public override void OnClosing ()
	{
		NetMgr.servConn.msgDist.DelListener ("GetRoomInfo", RecvGetRoomInfo);
		NetMgr.servConn.msgDist.DelListener ("Fight", RecvFight);
	}
	#endregion

	#region OnButtonClickEvent
	void OnStartButtonClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Fight");
		NetMgr.servConn.Send (protocol, OnStartBack);
	}

	void OnCloseButtonClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("LeaveRoom");
		NetMgr.servConn.Send (protocol, OnCloseBack);
	}
	#endregion

	#region OnBackEvent
	/// <summary>
	/// After Start Buccton Click, call this function to handle the info which send by server.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void OnStartBack(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int result = protocol.GetInt (start, ref start);
		if (result != 0) {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Start game fail, each team must contain at least two member.");
		}
	}
	/// <summary>
	/// After CLose Button Click, call this function to handle the info which send by server.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void OnCloseBack(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int result = protocol.GetInt (start, ref start);
		if (result == 0) {
			PanelMgr.instance.OpenPanel<LobbyPanel> ("");
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Leave room success.");
			Close ();
		} else {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Leave room fail.");
		}
	}
	#endregion

	#region Listen Recv Function
	/// <summary>
	/// When receive GetRoomInfo message from server, call this function.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void RecvGetRoomInfo(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int count = protocol.GetInt (start, ref start);
		int i = 0;
		for (; i < count; i++) {
			Transform playerTrans = playerList [i];
			Text playerText = playerTrans.Find ("Text").GetComponent<Text> ();

			string id = protocol.GetString (start, ref start);
			int team = protocol.GetInt (start, ref start);
			int win = protocol.GetInt (start, ref start);
			int fail = protocol.GetInt (start, ref start);
			int isOwner = protocol.GetInt (start, ref start);

			string str = "Name: " + id;
			str += "\nTeam: " + (team == 1 ? "red" : "blue");
			str += "\nWin: " + win.ToString ();
			str += "\nFail: " + fail.ToString ();
			if (id == GameMgr.instance.id) {
				str += "\n\n[Myself]";
			}
			if (isOwner == 1) {
				str += "\n\n[Owner]";
			}

			playerText.text = str;
			if (team == 1)
				playerTrans.GetComponent<Image> ().color = Color.red;
			else
				playerTrans.GetComponent<Image> ().color = Color.blue;
		}

		for (; i < 6; i++) {
			Transform playerTrans = playerList [i];
			Text playerText = playerTrans.Find ("Text").GetComponent<Text> ();

			playerText.text = "[Waiting]";
			playerTrans.GetComponent<Image> ().color = Color.gray;
		}
	}

	/// <summary>
	/// When receive Fight message from server, call this function.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void RecvFight(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		// Start a game and send the protocol to it.
		Close();
	}
	#endregion

}
