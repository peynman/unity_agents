#undef ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FacebookAgent : NemoAgent 
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
	private static string	ObjectClassPath = "com.nemogames.FacebookAgent";
	AndroidJavaObject 		android;
	AndroidJavaObject		logMapObject;
	IntPtr					logMethodID, logClearID, logMethodByteArray;
	string					Method_intStringArraySigniture = "(I[Ljava/lang/String;)V";
	
    private bool			_hasPublishPermission()
	{
		return android.Call<bool>("hasPublishPermission");
	}
    private bool			_hasPermission(string permission)
	{
		return android.Call<bool>("hasPermission", permission);
	}
    private bool			_isSessionOpened()
	{
		return android.Call<bool>("isSessionOpened");
	}
	private string			_getAccessToken()
	{
		return android.Call<string>("getAccessToken");
	}
	private string			_getSessionState()
	{
		return android.Call<string>("getSessionState");
	}
	private bool			_hasCurrentUserBasicInfo()
	{
		return android.Call<bool>("hasCurrentUserBasicInfo");
	}
	private bool			_hasCurrentUserFriends()
	{
		return android.Call<bool>("hasCurrentUserFriends");
	}
	private void			_OpenForRead(string[] permissions, int handle)
	{
		object[] args = new object[2] { handle,
		new AndroidJavaObject(AndroidJNIHelper.ConvertToJNIArray(permissions)) };
		IntPtr method_ptr = AndroidJNI.GetMethodID(android.GetRawClass(),
			"OpenForRead", Method_intStringArraySigniture);
				AndroidJNI.CallVoidMethod(android.GetRawObject(), method_ptr, AndroidJNIHelper.CreateJNIArgArray(args));
		AndroidJNI.DeleteLocalRef(method_ptr);
	}
	private void			_OpenForPublish(string[] permissions, int handle)
	{
		object[] args = new object[2] { handle,
		new AndroidJavaObject(AndroidJNIHelper.ConvertToJNIArray(permissions)) };
		IntPtr method_ptr = AndroidJNI.GetMethodID(android.GetRawClass(),
			"OpenForPublish", Method_intStringArraySigniture);
				AndroidJNI.CallVoidMethod(android.GetRawObject(), method_ptr, AndroidJNIHelper.CreateJNIArgArray(args));
		AndroidJNI.DeleteLocalRef(method_ptr);
	}
	private void			_Close()
	{
		android.Call("Close");
	}
	private void			_CloseAndClear()
	{
		android.Call("CloseAndClear");
	}
    private void			_RequestNewPublishPermission(string[] permissions, int handle)
	{
		object[] args = new object[2] { handle,
		new AndroidJavaObject(AndroidJNIHelper.ConvertToJNIArray(permissions)) };
		IntPtr method_ptr = AndroidJNI.GetMethodID(android.GetRawClass(),
			"RequestNewPublishPermission", Method_intStringArraySigniture);
				AndroidJNI.CallVoidMethod(android.GetRawObject(), method_ptr, AndroidJNIHelper.CreateJNIArgArray(args));
		AndroidJNI.DeleteLocalRef(method_ptr);
	}
    private void			_RequestNewReadPermission(string[] permissions, int handle)
	{
		object[] args = new object[2] { handle,
		new AndroidJavaObject(AndroidJNIHelper.ConvertToJNIArray(permissions)) };
		IntPtr method_ptr = AndroidJNI.GetMethodID(android.GetRawClass(),
			"RequestNewReadPermission", Method_intStringArraySigniture);
				AndroidJNI.CallVoidMethod(android.GetRawObject(), method_ptr, AndroidJNIHelper.CreateJNIArgArray(args));
		AndroidJNI.DeleteLocalRef(method_ptr);
	}
    private void			_ExecuteMeRequest(int handle)
	{
		android.Call("ExecuteMeRequest", handle);
	}
    private void			_ExecuteMyFriendsRequest(int handle)
	{
		android.Call("ExecuteMyFriendsRequest", handle);
	}
	private void			_ExecuteUploadPhotoRequest(string message, Texture2D image, int handle)
	{
		using (AndroidJavaObject jstring = new AndroidJavaObject("java.lang.String", message))
		{
			object[] args = new object[3] { handle, jstring,
			new AndroidJavaObject(AndroidJNIHelper.ConvertToJNIArray(image.EncodeToPNG())) };
			IntPtr method_ptr = AndroidJNI.GetMethodID(android.GetRawClass(),
				"ExecuteUploadPhotoRequest", "(ILjava/lang/String;[B)V");
			AndroidJNI.CallVoidMethod(android.GetRawObject(), method_ptr, AndroidJNIHelper.CreateJNIArgArray(args));
			AndroidJNI.DeleteLocalRef(method_ptr);
		}
	}
	private void			_ExecuteUploadPhotoRequest(string message, string filepath, int handle)
	{
		android.Call("ExecuteUploadPhotoRequest", handle, message, filepath);
	}
	private void			_ExecuteGraphPathRequest(string path, int handle)
	{
		android.Call("ExecuteGraphPathRequest", handle, path);
	}
	private void			_ExecuteStatusUpdateRequest(Dictionary<string, string> bundle, int handle)
	{
		__fillMapFromDictionary(bundle);
		android.Call("ExecuteStatusUpdateRequest", handle, logMapObject);
	}
	private void			_ShowFeedDialog(Dictionary<string, string> bundle, int handle)
	{
		__fillMapFromDictionary(bundle);
		android.Call("ShowFeedDialog", handle, logMapObject);
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
#else
	private bool		_hasPublishPermission() { return false; }
    private bool		_hasPermission(string permission) { return false; }
    private bool		_isSessionOpened() { return false; }
	private string		_getAccessToken() { return ""; }
	private string		_getSessionState() { return ""; }
	private bool		_hasCurrentUserBasicInfo() { return false; }
	private bool		_hasCurrentUserFriends() { return false; }
    private void		_OpenForRead(string[] permissions, int handle) {}
	private void		_OpenForPublish(string[] permissions, int handle) {}
	private void		_Close() {}
	private void		_CloseAndClear() {}
    private void		_RequestNewPublishPermission(string[] permissions, int handle) {}
    private void		_RequestNewReadPermission(string[] permissions, int handle) {}
    private void		_ExecuteMeRequest(int handle) {}
    private void		_ExecuteMyFriendsRequest(int handle) {}
	private void		_ExecuteUploadPhotoRequest(string message, Texture2D image, int handle) {}
	private void		_ExecuteUploadPhotoRequest(string message, string filepath, int handle) {}
	private void		_ExecuteGraphPathRequest(string path, int handle) {}
	private void		_ExecuteStatusUpdateRequest(Dictionary<string, string> bundle, int handle) {}
	private void		_ShowFeedDialog(Dictionary<string, string> bundle, int handle) {}
#endif
	
	public enum FacebookDialogEvent 
	{
		Success,
		Failed,
		Canceled
	}
	public enum FacebookRequestEvent
	{
		Success,
		Failed,
	}
	
	public static string		Bundle_Name = "name";
	public static string		Bundle_Caption = "caption";
	public static string		Bundle_Link = "link";
	public static string		Bundle_PictureURL = "picture";
	public static string		Bundle_Message = "message";
	
	public delegate void		HandleRequestEvent(FacebookRequestEvent e, string error);
	public delegate void		HandleDialogEvent(FacebookDialogEvent e, string error);
	
	Dictionary<int, HandleDialogEvent>	dialog_handles = new Dictionary<int, HandleDialogEvent>();
	Dictionary<int, HandleRequestEvent>	req_handles = new Dictionary<int, HandleRequestEvent>();
	
    public bool			hasPublishPermission() { return _hasPublishPermission(); }
    public bool			hasPermission(string permission) { return _hasPermission(permission); }
    public bool			isSessionOpened() { return _isSessionOpened(); }   
	public string		getAccessToken() { return _getAccessToken(); }
	public string		getSessionState() { return _getSessionState(); }
	public bool			hasCurrentUserBasicInfo() { return _hasCurrentUserBasicInfo(); }
	public bool			hasCurrentUserFriends() { return _hasCurrentUserFriends(); }

    public void			OpenForRead(string[] permissions, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_OpenForRead(permissions, iid);
	}
	public void			OpenForPublish(string[] permissions, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_OpenForPublish(permissions, iid);
	}
	public void			Close() { _Close(); }
	public void			CloseAndClear() { _CloseAndClear(); }
    public void			RequestNewPublishPermission(string[] permissions, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_RequestNewPublishPermission(permissions, iid);
	}
    public void			RequestNewReadPermission(string[] permissions, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_RequestNewReadPermission(permissions, iid);
	}
    public void			ExecuteMeRequest(HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_ExecuteMeRequest(iid);
	}
    public void			ExecuteMyFriendsRequest(HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_ExecuteMyFriendsRequest(iid);
	}
	public void			ExecuteUploadPhotoRequest(string message, Texture2D image, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_ExecuteUploadPhotoRequest(message, image, iid);
	}
	public void			ExecuteUploadPhotoRequest(string message, string filepath, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_ExecuteUploadPhotoRequest(message, filepath, iid);
	}
	public void			ExecuteGraphPathRequest(string path, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_ExecuteGraphPathRequest(path, iid);
	}
	public void			ExecuteStatusUpdateRequest(Dictionary<string, string> bundle, HandleRequestEvent handle)
	{
		int iid = generateNextID();
		req_handles.Add(iid, handle);
		_ExecuteStatusUpdateRequest(bundle, iid);
	}
	public void			ShowFeedDialog(Dictionary<string, string> bundle, HandleDialogEvent handle)
	{
		int iid = generateNextID();
		dialog_handles.Add(iid, handle);
		_ShowFeedDialog(bundle, iid);
	}
	
	private int		internal_id = 1;
	private int		generateNextID() { return internal_id++; }
#region Function to call from Java
	public void				FacebookAgentReceiveAgentEvent(string json)	
	{
		Hashtable data = (Hashtable)MiniJSON.JsonDecode(json);
		int iid = int.Parse(data["iid"].ToString());
		int eid = int.Parse(data["eid"].ToString());
		switch (eid)
		{
		case 1:			// req success
		case 6:
			if (req_handles.ContainsKey(iid))
			{
				HandleRequestEvent handle = req_handles[iid];
				if (handle != null) handle(FacebookRequestEvent.Success, "");
				req_handles.Remove(iid);
			} else Debug.Log("Could not found handle for iid: " + iid);
			break;
		case 2:			// req failed
		case 7:
			if (req_handles.ContainsKey(iid))
			{
				HandleRequestEvent handle = req_handles[iid];
				if (handle != null) handle(FacebookRequestEvent.Failed, data["error"].ToString());
				req_handles.Remove(iid);
			} else Debug.Log("Could not found handle (request) for iid: " + iid);
			break;
		case 3:			// dialog success
			if (dialog_handles.ContainsKey(iid))
			{
				HandleDialogEvent handle = dialog_handles[iid];
				if (handle != null) handle(FacebookDialogEvent.Success, "");
			} else Debug.Log("Could not found handle (dialog) for iid: " + iid);
			break;
		case 4:			// dialog cancel
			if (dialog_handles.ContainsKey(iid))
			{
				HandleDialogEvent handle = dialog_handles[iid];
				if (handle != null) handle(FacebookDialogEvent.Canceled, "");
			} else Debug.Log("Could not found handle (dialog) for iid: " + iid);
			break;
		case 5:
			if (dialog_handles.ContainsKey(iid))
			{
				HandleDialogEvent handle = dialog_handles[iid];
				if (handle != null) handle(FacebookDialogEvent.Failed, data["error"].ToString());
			} else Debug.Log("Could not found handle (dialog) for iid: " + iid);
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

public static class FacebookBundle
{
	public static Dictionary<string, string>	
	CreateBundle(string name, string caption, string message, string link, string picurl)
	{
		Dictionary<string, string> bundle = new Dictionary<string, string>();
		if (name != "") bundle.Add("name", name);
		if (caption != "") bundle.Add("caption", caption);
		if (message != "") bundle.Add("message", message);
		if (link != "") bundle.Add("link", link);
		if (picurl != "") bundle.Add("picture", picurl);
		return bundle;
	}
	
	
}
