using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour {

	#region Properties
	public string skinPath;				// skin's actual path = 'Assets/Resources/' + skinPath
	public GameObject skin;				// an instance based on skinPath.
	public PanelMgr.PanelLayer layer;	// current panel's layer(equal to which group the panel belong to.)
	public object[] args;				// panel's arguments.
	#endregion

	#region Operation Cycle
	/*
	 *	Panel's basic life-time:
	 *		Init -> OnShowing -> OnShowed -> Update -> OnClosing -> OnClosed
	 */
	/// <summary>
	/// Init the panel with args.
	/// </summary>
	/// <param name="args">panel initia arguments.</param>
	public virtual void Init(params object[] args)
	{
		this.args = args;
	}
	/// <summary>
	/// Do something before show the panel.
	/// </summary>
	public virtual void OnShowing() { }

	/// <summary>
	/// Do something after show the panel.
	/// </summary>
	public virtual void OnShowed() { }

	/// <summary>
	/// Update info each frame during the panel is opening.
	/// </summary>
	public virtual void Update() { }

	/// <summary>
	/// Do something before close the panel.
	/// </summary>
	public virtual void OnClosing() { }

	/// <summary>
	/// Do something after close the panel.
	/// </summary>
	public virtual void OnClosed() { }
	#endregion

	#region Operation
	/// <summary>
	/// Close the panel by calling PanelMgr.ClosePanel function.
	/// </summary>
	protected virtual void Close()
	{
		string name = this.GetType ().ToString ();
		PanelMgr.instance.ClosePanel (name);
	}
	#endregion
}
