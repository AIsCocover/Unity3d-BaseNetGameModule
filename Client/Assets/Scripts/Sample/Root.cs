using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour {

	void Start()
	{
		PanelMgr.instance.OpenPanel<LoginPanel> ("");
	}

	void Update()
	{
		NetMgr.Update ();
	}
}
