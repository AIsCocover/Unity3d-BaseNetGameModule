using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : PanelBase {
	/*
	 * Add a logout listener.
	 */
	private InputField idInput;
	private InputField pwInput;
	private Button loginBtn;
	private Button regBtn;
	private Button exitBtn;

	#region Operation-Cycle
	public override void Init (params object[] args)
	{
		base.Init (args);
		skinPath = "LoginPanel";
		layer = PanelMgr.PanelLayer.Panel;
	}

	public override void OnShowing ()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		idInput = skinTrans.Find ("idInput").GetComponent<InputField> ();
		pwInput = skinTrans.Find ("pwInput").GetComponent<InputField> ();
		loginBtn = skinTrans.Find ("loginBtn").GetComponent<Button> ();
		regBtn = skinTrans.Find ("regBtn").GetComponent<Button> ();
		exitBtn = skinTrans.Find ("exitBtn").GetComponent<Button> ();

		loginBtn.onClick.AddListener (OnLoginButtonClick);
		regBtn.onClick.AddListener (OnRegisterButtonClick);
		exitBtn.onClick.AddListener (OnExitButtonClick);
	}
	#endregion

	#region OnButtonClickEvent
	void OnLoginButtonClick()
	{
		if (idInput.text == "" || pwInput.text == "") {
			//Debug.Log ("[LoginPanel.OnLoginButtonClick] Id and pw can't stay empty.");
			PanelMgr.instance.OpenPanel<TipPanel>("","Id and pw can't stay empty.");
			return;
		}

		if (NetMgr.servConn.status != Connection.Status.Connected) {
			NetMgr.servConn.proto = new ProtocolBytes ();
			if (!NetMgr.servConn.Connect ("127.0.0.1", 1234)) {
				PanelMgr.instance.OpenPanel<TipPanel> ("", "Connect server fail.");
			}
		}

		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Login");
		protocol.AddString (idInput.text);
		protocol.AddString (pwInput.text);
		Debug.Log ("[LoginPanel.OnLoginButtonClick] Send protocol: Login " + idInput.text + "[id] " + pwInput.text + "[pw]");
		NetMgr.servConn.Send (protocol, OnLoginBack);
	}
	void OnRegisterButtonClick()
	{
		PanelMgr.instance.OpenPanel<RegisterPanel> ("");
		Close ();
	}

	void OnExitButtonClick()
	{
		Application.Quit ();
	}
	#endregion

	#region OnBackEvent
	void OnLoginBack(ProtocolBase protoBase)
	{
		ProtocolBytes protocolRec = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocolRec.GetString (start, ref start);
		int result = protocolRec.GetInt (start, ref start);
		if (result == 0) {
			//Debug.Log ("[LoginPanel.OnLoginBack] Login success. ");
			PanelMgr.instance.OpenPanel<TipPanel>("","Login success. ");
			PanelMgr.instance.OpenPanel<LobbyPanel> ("");
			Close ();
		}else{
			//Debug.Log ("[LoginPanel.OnLoginBack] Login fail. ");
			PanelMgr.instance.OpenPanel<TipPanel>("","Login fail. Please check your username and password.");
		}
	}
	#endregion
}
