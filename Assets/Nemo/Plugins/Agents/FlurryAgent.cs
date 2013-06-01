#undef ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FlurryAgent : NemoAgent 
{
	public static FlurryAgent		instance { get { return _instance; } }
	private static FlurryAgent		_instance;
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	public static string	ObjectClassPath = "com.nemogames.FlurryAgent";
	AndroidJavaObject 		android;
	AndroidJavaObject		logMapObject;
	IntPtr					logMethodID, logClearID;
	private void	_StartSession()
	{
		android.Call("StartSession");
	}
	private void	_EndSession()
	{
		android.Call("EndSession");
	}
	private void	_SetReportLocation(bool status)
	{
		android.Call("SetReportLocation", status);
	}
	private void	_SetCaptureUncouthExceptions(bool status)
	{
		android.Call("SetCaptureUncouthExceptions");
	}
	private void	_SetContinueSessionMilis(long milis)
	{
		android.Call("SetContinueSessionMilis", milis);
	}
	private void	_LogEvent(string id)
	{
		android.Call("LogEvent", id);
	}
	private void	_LogEvent(string id, Dictionary<string, string> data)
	{
		__fillMapFromDictionary(data);
		android.Call("LogEvent", id, logMapObject);
	}
	private void	_LogEvent(string id, Dictionary<string, string> data, bool timed)
	{
		__fillMapFromDictionary(data);
		android.Call("LogEvent", id, logMapObject);
	}
	private void	_LogEvent(string id, bool timed)
	{
		android.Call("LogEvent", id, timed);
	}
	private void	_LogPageView()
	{
		android.Call("LogPageView");
	}
	private void	_SetLogEnabled(bool status)
	{
		android.Call("SetLogEnabled", status);
	}
	private void	_SetUseHttps(bool status)
	{
		android.Call("SetUseHttps", status);
	}
	private void	_SetAge(int age)
	{
		android.Call("SetAge", age);
	}
	private void	_SetGender(int gender)
	{
		android.Call("SetGender", gender);
	}
	private void		_SetUserID(string id)
	{
		android.Call("SetUserID", id);
	}
	private void		__fillMapFromDictionary(Dictionary<string, string> dic)
	{
		AndroidJNI.CallVoidMethod(logMapObject.GetRawObject(), logClearID, AndroidJNIHelper.CreateJNIArgArray(new object[0]));
        object[] args = new object[2];
        foreach(KeyValuePair<string, string> kvp in dic)
        {
            using(AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key))
            {
                using(AndroidJavaObject v = new AndroidJavaObject("java.lang.String", kvp.Value))
                {
                    args[0] = k;
                    args[1] = v;
                    AndroidJNI.CallObjectMethod(logMapObject.GetRawObject(), 
                        logMethodID, AndroidJNIHelper.CreateJNIArgArray(args));
                }
            }
        }
	}
#else
	private void	_StartSession() {}
	private void	_EndSession() {}
	private void	_SetReportLocation(bool status) {}
	private void	_SetCaptureUncouthExceptions(bool status) {}
	private void	_SetContinueSessionMilis(long milis) {}
	private void	_LogEvent(string id) {}
	private void	_LogEvent(string id, Dictionary<string, string> data) {}
	private void	_LogEvent(string id, Dictionary<string, string> data, bool timed) {}
	private void	_LogEvent(string id, bool timed) {}
	private void	_LogPageView() {}
	private void	_SetLogEnabled(bool status) {}
	private void	_SetUseHttps(bool status) {}
	private void	_SetAge(int age) {}
	private void	_SetGender(int gender) {}
	private void	_SetUserID(string id) {}
#endif
	
	void Awake()
	{
		_instance = this;
#if UNITY_ANDROID && ENABLE_PLUGIN
		android = new AndroidJavaObject(ObjectClassPath);
		android.Call("init");
		
		logMapObject = new AndroidJavaObject("java.util.HashMap");
		logMethodID = AndroidJNIHelper.GetMethodID(logMapObject.GetRawClass(), "put", 
            "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
		logClearID =  AndroidJNIHelper.GetMethodID(logMapObject.GetRawClass(), "clear", "()V");
#endif
		SetReportLocation(ReportLocation);
		SetLogEnabled(LogEnabled);
		SetCaptureUncouthExceptions(CaptureUncouthException);
		SetUseHttps(UseHttps);
		if (StartSessionOnAwake) StartSession();
	}
	
	public bool		StartSessionOnAwake = true,
					EndSessionOnSuspend = true;
	public bool		ReportLocation = true,
					CaptureUncouthException = true,
					LogEnabled = true,
					UseHttps = false;

	public void		StartSession() { _StartSession(); }	
	public void		EndSession() { _EndSession(); }
	public void		SetReportLocation(bool status) { _SetReportLocation(status); }
	public void		SetCaptureUncouthExceptions(bool status) { _SetCaptureUncouthExceptions(true); }
	public void		SetContinueSessionMilis(long milis) { _SetContinueSessionMilis(milis); }
	public void		LogEvent(string id) { _LogEvent(id); }
	public void		LogEvent(string id, Dictionary<string, string> data) { _LogEvent(id, data); }
	public void		LogEvent(string id, Dictionary<string, string> data, bool timed) { _LogEvent(id, data, timed); }
	public void		LogEvent(string id, bool timed) { _LogEvent(id, timed); }
	public void		LogPageView() { _LogPageView(); }
	public void		SetLogEnabled(bool status) { _SetLogEnabled(status); }
	public void		SetUseHttps(bool status) { _SetUseHttps(status); }
	public void		SetAge(int age) { _SetAge(age); }
	public void		SetGender(int gender) { _SetGender(gender); }
	public void		SetUserID(string id) { _SetUserID(id); }
	
	#region NemoAgent implementation
#if ENABLE_PLUGIN
	public override bool		isEnable { get { return true; } }
#else
	public override bool		isEnable { get { return false; } }	
#endif
	#endregion
	
}
