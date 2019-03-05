using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : PanelBase {

	private GameObject roomPrefab;
	private Transform contentTrans;
	private Text infoText;
	private Button createBtn;
	private Button reflashBtn;
	private Button logoutBtn;

	#region Operation-Cycle
	public override void Init (params object[] args)
	{
		base.Init (args);
		skinPath = "LobbyPanel";
		layer = PanelMgr.PanelLayer.Panel;
	}

	public override void OnShowing ()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		Transform infoTrans = skinTrans.Find ("infoImage");
		Transform listTrans = skinTrans.Find ("listImage");
		roomPrefab = Resources.Load<GameObject> ("roomPrefab");

		logoutBtn = skinTrans.Find ("closeBtn").GetComponent<Button> ();
		infoText = infoTrans.Find ("Text").GetComponent<Text> ();
		contentTrans = listTrans.Find ("Scroll View").Find ("Viewport").Find ("Content");
		createBtn = listTrans.Find ("createBtn").GetComponent<Button> ();
		reflashBtn = listTrans.Find ("reflashBtn").GetComponent<Button> ();

		createBtn.onClick.AddListener (OnCreateRoomButtonClick);
		reflashBtn.onClick.AddListener (OnReflashRoomButtonClick);
		logoutBtn.onClick.AddListener (OnLogoutButtonClick);

		NetMgr.servConn.msgDist.AddListener ("GetAchieve", RecvGetAchieve);
		NetMgr.servConn.msgDist.AddListener ("GetRoomList", RecvGetRoomList);

		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("GetRoomList");
		NetMgr.servConn.Send (protocol);

		protocol = new ProtocolBytes ();
		protocol.AddString ("GetAchieve");
		NetMgr.servConn.Send (protocol);
	}

	public override void OnClosing ()
	{
		NetMgr.servConn.msgDist.DelListener ("GetAchieve", RecvGetAchieve);
		NetMgr.servConn.msgDist.DelListener ("GetRoomList", RecvGetRoomList);
	}
	#endregion

	#region OnButtonClickEvent
	void OnJoinRoomButtonClick(string name)
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("EnterRoom");
		protocol.AddInt32 (int.Parse (name));
		NetMgr.servConn.Send (protocol, OnJoinRoomBack);
	}

	void OnCreateRoomButtonClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("CreateRoom");
		NetMgr.servConn.Send (protocol, OnCreateRoomBack);
	}

	void OnReflashRoomButtonClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("GetRoomList");
		NetMgr.servConn.Send (protocol);
	}

	void OnLogoutButtonClick()
	{
		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Logout");
		NetMgr.servConn.Send (protocol, OnLogoutBack);
	}
	#endregion

	#region OnBackEvent
	/// <summary>
	/// After Join Room Buccton Click, call this function to handle the info which send by server.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void OnJoinRoomBack(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int result = protocol.GetInt (start, ref start);
		if (result == 0) {
			PanelMgr.instance.OpenPanel<RoomPanel> ("");
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Join room success.");
			Close ();
		} else {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Join room fail.");
		}
	}
	/// <summary>
	/// After Create Room Buccton Click, call this function to handle the info which send by server.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void OnCreateRoomBack(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int result = protocol.GetInt (start, ref start);
		if (result == 0) {
			PanelMgr.instance.OpenPanel<RoomPanel> ("");
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Create room success.");
			Close ();
		} else {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Create room fail.");
		}
	}
	/// <summary>
	/// After Logout Buccton Click, call this function to handle the info which send by server.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void OnLogoutBack(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int result = protocol.GetInt (start, ref start);
		if (result == 0) {
			PanelMgr.instance.OpenPanel<LoginPanel> ("");
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Logout success.");
			NetMgr.servConn.Disconnect ();
			Close ();
		} else {
			PanelMgr.instance.OpenPanel<TipPanel> ("", "Logout fail.");
		}
	}
	#endregion

	#region Listen Recv Function
	/// <summary>
	/// When receive GetAchieve message from server, call this function.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void RecvGetAchieve(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int win = protocol.GetInt (start, ref start);
		int fail = protocol.GetInt (start, ref start);
		infoText.text = GameMgr.instance.id;
		infoText.text += "\n\nwin: " + win.ToString ();
		infoText.text += "\nfail: " + fail.ToString ();
	}
	/// <summary>
	/// When receive GetRoomList message from server, call this function.
	/// </summary>
	/// <param name="protoBase">Proto base.</param>
	void RecvGetRoomList(ProtocolBase protoBase)
	{
		ClearRoomUnit ();

		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int count = protocol.GetInt (start, ref start);
		for (int i = 0; i < count; i++) {
			int num = protocol.GetInt (start, ref start);
			int status = protocol.GetInt (start, ref start);
			GenerateRoomUnit (i, num, status);
		}
	}
	#endregion

	#region Help Function
	/// <summary>
	/// Clear all the room in room list.
	/// </summary>
	void ClearRoomUnit()
	{
		for (int i = 0; i < contentTrans.childCount; i++) {
			Destroy (contentTrans.GetChild (i).gameObject);
		}
	}
	/// <summary>
	/// Create a room in room list.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="num">Number.</param>
	/// <param name="status">Status.</param>
	void GenerateRoomUnit(int index, int num, int status)
	{
		Transform roomTrans = Instantiate (roomPrefab, contentTrans).transform;
		Text roomText = roomTrans.Find ("Text").GetComponent<Text> ();
		Button joinBtn = roomTrans.Find ("joinBtn").GetComponent<Button> ();

		roomText.text = "No: " + index.ToString ();
		roomText.text += "\nNum: " + num.ToString () + "//4";
		if (status == 1) {
			roomText.text += "\n\nStatus: Preparing...";
		} else {
			roomText.text += "\n\nStatus: Fighting...";
		}

		joinBtn.name = index.ToString ();
		joinBtn.onClick.AddListener (delegate() {
			OnJoinRoomButtonClick (joinBtn.name);
		}
		);
	}
	#endregion
}

