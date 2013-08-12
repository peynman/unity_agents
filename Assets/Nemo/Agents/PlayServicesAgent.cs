#undef ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class PlayServicesAgent : NemoAgent 
{
	static PlayServicesAgent _instance;
	public static PlayServicesAgent		instance { get { return _instance; } }
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	private static string	ObjectClassPath = "com.nemogames.PlayServicesAgent";
	AndroidJavaObject 		android;
	IntPtr					handleSaveStatePtr, handleSaveStateImmedatePtr;
	
	private bool		_isPlayServicesAvailable(bool show_error_dialog)
	{
		return android.Call<bool>("isPlayServicesAvailable", show_error_dialog);
	}
	private bool		_isConnected()
	{
		return android.Call<bool>("isConnected");
	}
	private bool		_isConnecting()
	{
		return android.Call<bool>("isConnecting");
	}
	private void		_Connect()
	{
		android.Call("Connect");
	}
	private void		_Dissconnect()
	{
		android.Call("Dissconnect");
	}
	private void		_UnlockAchievement(string id)
	{
		android.Call("UnlockAchievement", id);
	}
	private void		_IncrementAchievement(string id, int step)
	{
		android.Call("IncrementAchievement", id, step);
	}
	private void		_ShowAchievements()
	{
		android.Call("ShowAchievements");
	}
	private void		_ShowLeaderboards()
	{
		android.Call("ShowLeaderboards");
	}
	private void		_ShowLeaderboard(string id)
	{
		android.Call("ShowLeaderboard", id);
	}
	private void		_SubmitScore(string id, long value)
	{
		android.Call("SubmitScore", id, value);
	}
	private void		_UpdateStateImmediate(int iid, int key, byte[] data)
	{
		object[] args = new object[3] { iid, key, AndroidJNIHelper.ConvertToJNIArray(data) };
		AndroidJNI.CallVoidMethod(android.GetRawObject(), handleSaveStateImmedatePtr,
			AndroidJNIHelper.CreateJNIArgArray(args));
	}
	private void		_UpdateState(int key, byte[] data)
	{
		object[] args = new object[2] { key, AndroidJNIHelper.ConvertToJNIArray(data) };
		AndroidJNI.CallVoidMethod(android.GetRawObject(), handleSaveStatePtr,
			AndroidJNIHelper.CreateJNIArgArray(args));		
	}
	private void		_LoadState(int iid, int key)
	{
		android.Call("LoadState", iid, key);
	}
#else
	private bool		_isPlayServicesAvailable(bool show_error_dialog) {  NemoAgent.LogDisabledAgentCall(); return false; }
	private bool		_isConnected() {  NemoAgent.LogDisabledAgentCall(); return false; }
	private bool		_isConnecting() {  NemoAgent.LogDisabledAgentCall(); return false; }
	private void		_Connect() { NemoAgent.LogDisabledAgentCall(); }
	private void		_Dissconnect() { NemoAgent.LogDisabledAgentCall(); }
	private void		_UnlockAchievement(string id) { NemoAgent.LogDisabledAgentCall(); }
	private void		_IncrementAchievement(string id, int step) { NemoAgent.LogDisabledAgentCall(); }
	private void		_ShowAchievements() { NemoAgent.LogDisabledAgentCall(); }
	private void		_ShowLeaderboards() { NemoAgent.LogDisabledAgentCall(); }
	private void		_ShowLeaderboard(string id) { NemoAgent.LogDisabledAgentCall(); }
	private void		_SubmitScore(string id, long value) { NemoAgent.LogDisabledAgentCall(); }
	private void		_UpdateStateImmediate(int iid, int key, byte[] data) { NemoAgent.LogDisabledAgentCall(); }
	private void		_UpdateState(int key, byte[] data) { NemoAgent.LogDisabledAgentCall(); }
	private void		_LoadState(int iid, int key) { NemoAgent.LogDisabledAgentCall(); }
#endif
	
	void Awake()
	{
		_instance = this;
#if UNITY_ANDROID && ENABLE_PLUGIN
		android = new AndroidJavaObject(ObjectClassPath);
		int clients = 1;
		if (EnableCloudSave) clients |= 2;
		android.Call("init", gameObject.name, "PlayServicesReceiveAgentEvent", clients);
		handleSaveStatePtr = AndroidJNIHelper.GetMethodID(android.GetRawClass(), "UpdateState", "(I[B)V");
		handleSaveStateImmedatePtr = AndroidJNIHelper.GetMethodID(android.GetRawClass(), "UpdateStateImmediate", "(II[B)V");
#endif
	}
	
	public bool		EnableCloudSave = false;
	
	public enum PlayServicesEvent
	{
		OnConnectionFailed = 1,
		OnConnectionSuccess = 2,
		OnStateLoadConflict = 3,
		OnStateLoaded = 4,
		OnStateNetworkErrorWithData = 5,
		OnStateLoadFailed = 6
	}
	public delegate void	HandlePlayServicesEvent(PlayServicesEvent e);
	public delegate void	HandleStateConflict(int state, string version, byte[] local, byte[] server);
	public delegate void	HandleStateLoaded(int status, int key, byte[] data);
	public event HandlePlayServicesEvent	OnPlayServicesEvent;
	public event HandleStateLoaded			OnStateLoaded;
	public event HandleStateConflict		OnStateConflict;
	
	public bool		isPlayServicesAvailable(bool show_error_dialog) { return _isPlayServicesAvailable(show_error_dialog); }
	public bool		isConnected() { return _isConnected(); }
	public bool		isConnecting() { return _isConnecting(); }
	public void		Connect() { _Connect(); }
	public void		Dissconnect() { _Dissconnect(); }
	public void		UnlockAchievement(string id) { _UnlockAchievement(id); }
	public void		IncrementAchievement(string id, int step) { _IncrementAchievement(id, step); }
	public void		ShowAchievements() { _ShowAchievements(); }
	public void		ShowLeaderboards() { _ShowLeaderboards(); }
	public void		ShowLeaderboard(string id) { _ShowLeaderboard(id); }
	public void		SubmitScore(string id, long value) { _SubmitScore(id, value); }
	public void		UpdateStateImmediate(int key, byte[] data) { _UpdateStateImmediate(0, key, data); }
	public void		UpdateState(int key, byte[] data) { _UpdateState(key, data); }
	public void		LoadState(int key) { _LoadState(0, key); }
	public void		ResolveConflict(int key, string version, byte[] data) {}
	
#region IAgentEventListener implementation
	public void		PlayServicesReceiveAgentEvent(string json)
	{
		Hashtable data = MiniJSON.JsonDecode(json) as Hashtable;
		PlayServicesEvent e = (PlayServicesEvent)int.Parse(data["eid"].ToString());
		switch (e)
		{
		case PlayServicesEvent.OnConnectionFailed:
		case PlayServicesEvent.OnConnectionSuccess:
			if (OnPlayServicesEvent != null) OnPlayServicesEvent(e);
			break;
		case PlayServicesEvent.OnStateLoadConflict:
			if (OnStateConflict != null) OnStateConflict(0, "", null, null);
			break;
		case PlayServicesEvent.OnStateLoaded:
			if (OnStateLoaded != null) OnStateLoaded(0, 0, null);
			break;
		}
	}
#endregion
	
	#region NemoAgent implementation
#if ENABLE_PLUGIN
	public override bool		isEnable { get { return true; } }
#else
	public override bool		isEnable { get { return false; } }	
#endif
	#endregion
}
