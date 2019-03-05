using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgDistribution {

	public int maxCount = 15;
	public List<ProtocolBase> msgList = new List<ProtocolBase>();

	public delegate void Delegate(ProtocolBase protoBase);
	private Dictionary<string, Delegate> eventDict = new Dictionary<string, Delegate>();
	private Dictionary<string, Delegate> onceDict = new Dictionary<string, Delegate>();

	#region Lift-Cycle
	/// <summary>
	/// Dispatch protocol each frame.
	/// </summary>
	public void Update()
	{
		for (int i = 0; i < maxCount; i++) {
			if (msgList.Count > 0) {
				DispatchMsgEvent (msgList [0]);
				lock (msgList) {
					msgList.RemoveAt (0);
				}
			} else {
				break;
			}
		}
	}

	void DispatchMsgEvent(ProtocolBase protoBase)
	{
		string name = protoBase.GetName ();
		Console.WriteLine ("[MsgDistribution.DispatchMsgEvent] Dispatch protocol: " + name);
		if (eventDict.ContainsKey (name)) {
			eventDict [name] (protoBase);
		}
		if (onceDict.ContainsKey (name)) {
			onceDict [name] (protoBase);
			onceDict [name] = null;
			onceDict.Remove (name);
		}
	}
	#endregion

	#region Operation
	public void AddListener(string name, Delegate cb)
	{
		if (eventDict.ContainsKey (name)) {
			eventDict [name] += cb;
		} else {
			eventDict [name] = cb;
		}
	}

	public void AddOnceListener(string name, Delegate cb)
	{
		if (onceDict.ContainsKey (name)) {
			onceDict [name] += cb;
		} else {
			onceDict [name] = cb;
		}
	}

	public void DelListener(string name, Delegate cb)
	{
		if (eventDict.ContainsKey (name)) {
			eventDict [name] -= cb;
			if (eventDict [name] == null)
				eventDict.Remove (name);
		}
	}

	public void DelOnceListener(string name, Delegate cb)
	{
		if (onceDict.ContainsKey (name)) {
			onceDict [name] -= cb;
			if (onceDict [name] == null)
				onceDict.Remove (name);
		}
	}
	#endregion
}
