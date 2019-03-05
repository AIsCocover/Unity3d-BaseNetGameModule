using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMgr : MonoBehaviour {

	#region Properties
	public enum PanelLayer									// panel layer list.
	{
		Panel,
		Tips,
	}

	public static PanelMgr instance;						// PanelMgr's single instance.

	public Dictionary<string, PanelBase> dict;				// key:paneltype value:panel instance.

	private GameObject canvas;								// current panel's canvas.
	private Dictionary<PanelLayer, Transform> layerDict;	// each layer's actual transfrom in scenes.
	#endregion

	#region Operation Cycle
	void Awake()
	{
		instance = this;
		InitLayer ();
		dict = new Dictionary<string, PanelBase> ();
	}
	#endregion

	#region Help Function
	/// <summary>
	/// Initialize layerDict.
	/// </summary>
	void InitLayer()
	{
		canvas = GameObject.Find ("Canvas");
		if (canvas == null) {
			Debug.LogError ("[PanelMgr.InitLayer] GameObject canvas is not exist.");
		}

		layerDict = new Dictionary<PanelLayer, Transform> ();
		foreach (PanelLayer pl in Enum.GetValues(typeof(PanelLayer))) {
			string name = pl.ToString ();
			Transform transform = canvas.transform.Find (name);
			layerDict.Add (pl, transform);
		}
	}
	#endregion

	#region Operation Function
	/// <summary>
	/// Open a panel which type is skinPath. At the same time, current scene's canvas will add a same type script.
	/// </summary>
	/// <param name="skinPath">Skin path.</param>
	/// <param name="args">Arguments.</param>
	/// <typeparam name="T">Type T must inherit PanelBase</typeparam>
	public void OpenPanel<T> (string skinPath, params object[] args)
		where T : PanelBase
	{
		string name = typeof(T).ToString ();
		if (dict.ContainsKey (name))
			return;

		PanelBase panel = canvas.AddComponent<T> ();
		panel.Init (args);
		dict.Add (name, panel);

		skinPath = (skinPath != "" ? skinPath : panel.skinPath);
		GameObject skin = Resources.Load<GameObject> (skinPath);
		if (skin == null) {
			Debug.LogError ("[PanelMgr.OpenPanel] Can't find " + skinPath + " under Assets/Resources/");
		}
		PanelLayer layer = panel.layer;
		Transform parent = layerDict [layer];
		panel.skin = (GameObject)Instantiate (skin, parent);

		panel.OnShowing ();

		panel.OnShowed ();
	}

	/// <summary>
	/// Close a panel which type is name.
	/// </summary>
	/// <param name="name">Panel Instance's name</param>
	public void ClosePanel(string name)
	{
		PanelBase panel = (PanelBase)dict [name];
		if (panel == null)
			return;

		panel.OnClosing ();
		dict.Remove (name);
		panel.OnClosed ();
		GameObject.Destroy (panel.skin);
		Component.Destroy (panel);
	}
	#endregion
}
