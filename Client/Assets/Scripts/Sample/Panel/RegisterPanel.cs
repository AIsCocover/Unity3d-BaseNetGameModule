using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : PanelBase {

	private InputField idInput;
	private InputField pwInput;
	private InputField rpwInput;
	private Button regBtn;
	private Button closeBtn;

	#region Operation-Cycle
	public override void Init (params object[] args)
	{
		base.Init (args);
		skinPath = "RegisterPanel";
		layer = PanelMgr.PanelLayer.Panel;
	}

	public override void OnShowing ()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		idInput = skinTrans.Find ("idInput").GetComponent<InputField> ();
		pwInput = skinTrans.Find ("pwInput").GetComponent<InputField> ();
		rpwInput = skinTrans.Find ("rpwInput").GetComponent<InputField> ();
		regBtn = skinTrans.Find ("regBtn").GetComponent<Button> ();
		closeBtn = skinTrans.Find ("closeBtn").GetComponent<Button> ();

		regBtn.onClick.AddListener (OnRegisterButtonClick);
		closeBtn.onClick.AddListener (OnCloseButtonClick);
	}
	#endregion

	#region OnButtonClickEvent
	void OnRegisterButtonClick()
	{
		if (idInput.text == "" || pwInput.text == "" || rpwInput.text == "") {
			//Debug.Log ("[RegisterPanel.OnRegisterButtonClick] Id and pw and check pw can't stay empty.");
			PanelMgr.instance.OpenPanel<TipPanel>("","Id and pw and check pw can't stay empty.");
			return;
		}

		if (pwInput.text != rpwInput.text) {
			//Debug.Log ("[RegisterPanel.OnRegisterButtonClick] Two password are different.");
			PanelMgr.instance.OpenPanel<TipPanel>("","Two password are different.");
			return;
		}

		if (NetMgr.servConn.status != Connection.Status.Connected) {
			NetMgr.servConn.proto = new ProtocolBytes ();
			if (!NetMgr.servConn.Connect ("127.0.0.1", 1234)) {
				PanelMgr.instance.OpenPanel<TipPanel> ("", "Connect server fail.");
			}
		}

		ProtocolBytes protocol = new ProtocolBytes ();
		protocol.AddString ("Register");
		protocol.AddString (idInput.text);
		protocol.AddString (pwInput.text);
		Debug.Log ("[LoginPanel.OnRegisterButtonClick] Send protocol: Register " + idInput.text + "[id] " + pwInput.text + "[pw]");
		NetMgr.servConn.Send (protocol, OnRegisterBack);
	}

	void OnCloseButtonClick()
	{
		PanelMgr.instance.OpenPanel<LoginPanel> ("");
		Close ();
	}
	#endregion

	#region OnBackEvent
	void OnRegisterBack(ProtocolBase protoBase)
	{
		ProtocolBytes protocol = (ProtocolBytes)protoBase;
		int start = 0;
		string protoName = protocol.GetString (start, ref start);
		int result = protocol.GetInt (start, ref start);
		if (result == 0) {
			//Debug.Log ("[RegisterPanel.OnRegisterBack] Register success.");
			PanelMgr.instance.OpenPanel<TipPanel>("","Register success.");
			PanelMgr.instance.OpenPanel<LoginPanel> ("");
			Close ();
		} else {
			//Debug.Log ("[RegisterPanel.OnRegisterBack] Register fail.");
			PanelMgr.instance.OpenPanel<TipPanel>("","Register fail. Please use another id.");
		}
	}
	#endregion
}
