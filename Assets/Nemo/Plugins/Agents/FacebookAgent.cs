#define	ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FacebookAgent : MonoBehaviour 
{
	private static FacebookAgent _instance;
	public static FacebookAgent		instance { get { return _instance; } }
	
	void Awake()
	{
#if UNITY_ANDROID && ENABLE_PLUGIN
		android = new AndroidJavaObject(ObjectClassPath);	
		android.Call("init", gameObject.name, "FacebookAgentReceiveAgentEvent");

		logMapObject = new AndroidJavaObject("android.os.Bundle");
		logMethodID = AndroidJNIHelper.GetMethodID(logMapObject.GetRawClass(), "putString", 
            "(Ljava/lang/String;Ljava/lang/String;)V");
		logClearID =  AndroidJNIHelper.GetMethodID(logMapObject.GetRawClass(), "clear", "()V");
		logMethodByteArray = AndroidJNIHelper.GetMethodID(logMapObject.GetRawClass(), "putByteArray",
			"(Ljava/lang/String;[B)V");
#endif
		_instance = this;
	}
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	public static string	ObjectClassPath = "com.nemogames.FAgentActivity";
	AndroidJavaObject 		android;
	AndroidJavaObject		logMapObject;
	IntPtr					logMethodID, logClearID, logMethodByteArray;
	private bool		_hasPublishPermission()
    {
		return android.Call<bool>("hasPublishPermission");
    }
    private bool		_hasPermission(string permission)
    {
		return android.Call<bool>("hasPermission", permission);
	}
    private bool		_isLoggedIn()
    {
		return android.Call<bool>("isLoggedIn");
    }
    private void		_Login()
    {
		android.Call("Login");
    }
    private void		_RequestNewPublishPermission(string[] perms)
    {
		ArrayList json = new ArrayList(perms);
		android.Call("RequestNewPublishPermission", MiniJSON.JsonEncode(json));
    }
    private void		_RequestNewReadPermission(string[] perms)
    {
		ArrayList json = new ArrayList(perms);
		android.Call("RequestNewReadPermission", MiniJSON.JsonEncode(json));
    }
    private void		_FetchGraphUser()
    {
		android.Call("FetchGraphUser");
    }
    private void		_FetchUserFriends()
    {
		android.Call("FetchUserFriends");
    }
    private void		_InviteFreindsDialog(string message)
    {
		android.Call("InviteFreindsDialog", message);
    }
    private void		_PostFeed()
    {
    }
	private string		_getAccessToken()
	{
		return android.Call<string>("getAccessToken");
	}
	private string		_getSessionState() 
	{
		return android.Call<string>("getSessionState");
	}
	private void			_ShowFeedDialog(Dictionary<string, string> props)
	{
		__fillMapFromDictionary(props);
		android.Call("ShowFeedDialog", logMapObject);
	}
	private void			_ShowFeedDialog(Dictionary<string, string> props, string filename)
	{
		__fillMapFromDictionary(props);
		android.Call("ShowFeedDialog", logMapObject);
	}
	private void			_ShowFeedDialog(Dictionary<string, string> props, byte[] data)
	{
		__fillMapFromDictionary(props);
		___addMapPictureSource(data);
		android.Call("ShowFeedDialog", logMapObject);
	}
	private void			_ShowFriendPicker()
	{
		android.Call("ShowFriendPicker");
	}
	private void			_ShowWebDialog(string action, Dictionary<string, string> props)
	{
		__fillMapFromDictionary(props);
		android.Call("ShowWebDialog", action, logMapObject);
	}
	private void			_UploadPhotoRequest(Texture2D image)
	{
		android.Call("UploadPhotoRequest", new AndroidJavaObject(AndroidJNI.ToByteArray(image.EncodeToPNG())));
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
            		AndroidJNI.CallVoidMethod(logMapObject.GetRawObject(), logMethodID,
						AndroidJNIHelper.CreateJNIArgArray(args));
				}
			}
        }
	}
	private void		___addMapPictureSource(byte[] data)
	{
		object[] args = new object[2];
		using(AndroidJavaObject k = new AndroidJavaObject("java.lang.String", "source"))
        {
			using (AndroidJavaObject v = new AndroidJavaObject(AndroidJNI.ToByteArray(data)))
			{
				args[0] = k;
				args[1] = v;
    			AndroidJNI.CallVoidMethod(logMapObject.GetRawObject(), logMethodByteArray,
					AndroidJNIHelper.CreateJNIArgArray(args));
			}
		}
	}
#endif
	
    public bool			hasPublishPermission() { return _hasPublishPermission(); }
    public bool			hasPermission(string permission) { return _hasPermission(permission); }
    public bool			isLoggedIn() { return _isLoggedIn(); }    
    public void			Login() { _Login(); }   
	public void			Logout() { }
    public void			RequestNewPublishPermission(string[] perms) { _RequestNewPublishPermission(perms); }
    public void			RequestNewReadPermission(string[] perms) { _RequestNewReadPermission(perms); }
    public void			FetchGraphUser() { _FetchGraphUser(); }
    public void			FetchUserFriends() { _FetchUserFriends(); }
    public void			InviteFreindsDialog(string message) { _InviteFreindsDialog(message); }
    public void			PostFeed() { _PostFeed(); }
	public string		getAccessToken() { return _getAccessToken(); }
	public string		getSessionState() { return _getSessionState(); }
	public void			ShowFeedDialog(Dictionary<string, string> props) { _ShowFeedDialog(props); }
	public void			ShowFeedDialog(Dictionary<string, string> props, Texture2D picture) { _ShowFeedDialog(props, picture.EncodeToPNG()); }
	public void			ShowFeedDialog(Dictionary<string, string> props, string picturefilename) { _ShowFeedDialog(props, picturefilename); }
	public void			ShowFriendPicker() { _ShowFriendPicker(); }
	public void			ShowWebDialog(string action, Dictionary<string, string> props) { _ShowWebDialog(action, props); }
	public void			UploadPhotoRequest(Texture2D image) { _UploadPhotoRequest(image); }
	
	public static void	FillBundle(ref Dictionary<string, string>	output,
		string name, string caption, string link, string pictureurl)
	{
		output.Clear();
		if (name != "") output.Add("name", name);
		if (caption != "") output.Add("caption", caption);
		if (link != "") output.Add("link", link);
		if (pictureurl != "") output.Add("picture", pictureurl);
	}
	
	public void				FacebookAgentReceiveAgentEvent(string json)	
	{
		Debug.Log(json);
	}
}
