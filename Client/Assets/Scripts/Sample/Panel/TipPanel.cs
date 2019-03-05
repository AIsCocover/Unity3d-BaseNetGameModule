using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : PanelBase {

	private Text msgText;
	private Button closeBtn;

	private string str = "";

	#region Operation-Cycle
	public override void Init (params object[] args)
	{
		base.Init (args);
		skinPath = "TipPanel";
		layer = PanelMgr.PanelLayer.Tips;

		if (args.Length == 1) {
			str += (string)args [0];
		}
	}

	public override void OnShowing ()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		msgText = skinTrans.Find ("msgText").GetComponent<Text> ();
		closeBtn = skinTrans.Find ("closeBtn").GetComponent<Button> ();

		msgText.text = str;
		closeBtn.onClick.AddListener (OnCloseButtonClick);
	}
	#endregion

	#region OnButtonClickEvent
	void OnCloseButtonClick()
	{
		Close ();
	}
	#endregion

}
