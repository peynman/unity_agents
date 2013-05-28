#define	ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NemoActivity : MonoBehaviour
{
	static NemoActivity			_instance;
	public static NemoActivity	instance { get { return _instance; } }
	void Awake()
	{
#if UNITY_ANDROID && ENABLE_PLUGIN
		AndroidJavaClass stinstance = new AndroidJavaClass(ObjectClassPath);
		android = stinstance.CallStatic<AndroidJavaObject>("instance");
		android.Call("init", gameObject.name, "NemoActivityReceiveAgentEvent");
#endif
		_instance = this;
	}
	
	public enum NemoActivityEvent
	{
		OnBackPressed = 1
	}
	
	public delegate void	HandleActivityEvent(NemoActivityEvent e);
	public event HandleActivityEvent	OnActivityEvent;
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	public static string	ObjectClassPath = "com.nemogames.NemoActivity";
	AndroidJavaObject 		android;
#endif

#region IAgentEventListener implementation
	public void 	NemoActivityReceiveAgentEvent(string json)
	{
		if (OnActivityEvent != null)
		{
			Hashtable data = (Hashtable)MiniJSON.JsonDecode(json);
			NemoActivityEvent e = (NemoActivityEvent)int.Parse(data["eid"].ToString());
			OnActivityEvent(e);
		}
	}
#endregion
	
}
