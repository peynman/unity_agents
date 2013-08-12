#undef ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ScreenshotAgent : NemoAgent
{
	static ScreenshotAgent	_instance;
	public static ScreenshotAgent	instance { get { return _instance; } }
	
	public enum SaveScreenshotEvent 
	{
		Success = 1,
		NotEnoughFreeSpace = 2,
		NoWriteAccess = 3
	}
	public delegate void		HandleSaveScreenshotEvent(SaveScreenshotEvent e);
	Dictionary<int, HandleSaveScreenshotEvent>	handles = new Dictionary<int, HandleSaveScreenshotEvent>();
	int internal_id = 1;
	int		registerHandle(HandleSaveScreenshotEvent h)
	{
		int id = internal_id++;
		handles.Add(id, h);
		return id;
	}
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	public static string	ObjectClassPath = "com.nemogames.ScreenshotAgent";
	AndroidJavaObject 		android;
	private void		_SaveScreenshotAsync(string filepath, int handle)
	{
		android.Call("SaveScreenshotAsync", handle, filepath);
	}
	private void		_SaveScreenshotToPublicGalleryAsync(string filename, string album, int handle)
	{
		android.Call("SaveScreenshotToPublicGalleryAsync", handle, filename, album);
	}
	private void		_SaveScreenshotToPrivateFolderAsync(string filename, string album, int handle)
	{
		android.Call("SaveScreenshotToPrivateFolderAsync", handle, filename, album);
	}
	private string		_GetPublicGalleryPath(string album)
	{
		return android.Call<string>("GetPublicGalleryPath", album);
	}
	private string		_GetPrivateFolderPath(string album)
	{
		return android.Call<string>("GetPrivateFolderPath", album);
	}
	private void		_RefreshGallery()
	{
		android.Call("RefreshAndroidGallery");
	}
#else
	private void		_SaveScreenshotAsync(string filepath, int handle) { NemoAgent.LogDisabledAgentCall(); }
	private void		_SaveScreenshotToPublicGalleryAsync(string filename, string album, int handle) { NemoAgent.LogDisabledAgentCall(); }
	private void		_SaveScreenshotToPrivateFolderAsync(string filename, string album, int handle) { NemoAgent.LogDisabledAgentCall(); }
	private string		_GetPublicGalleryPath(string album) { NemoAgent.LogDisabledAgentCall(); return ""; }
	private string		_GetPrivateFolderPath(string album) { NemoAgent.LogDisabledAgentCall(); return ""; }
	private void		_RefreshGallery() { NemoAgent.LogDisabledAgentCall(); }
#endif

	void Awake()
	{
		_instance = this;
#if UNITY_ANDROID && ENABLE_PLUGIN
		android = new AndroidJavaObject(ObjectClassPath);
		android.Call("init", gameObject.name, "ScreenshotReceiveAgentEvent");
#endif
	}
	
	public static string	unique_agent_temp_filename = "screenshot_agent_temp.jpg";
	
	public void		SaveScreenshotAsync(string filepath, HandleSaveScreenshotEvent handle)
	{
		if (File.Exists(Application.persistentDataPath + "/" + unique_agent_temp_filename)) File.Delete(Application.persistentDataPath + "/" + unique_agent_temp_filename);
		Application.CaptureScreenshot(unique_agent_temp_filename);
		StartCoroutine(waitAndCheckForLatestScreenshot(unique_agent_temp_filename, filepath, handle));
	}
	public void		SaveScreenshotToPublicGalleryAsync(string filename, string album, HandleSaveScreenshotEvent handle)
	{
		if (File.Exists(Application.persistentDataPath + "/" + unique_agent_temp_filename)) File.Delete(Application.persistentDataPath + "/" + unique_agent_temp_filename);
		Application.CaptureScreenshot(unique_agent_temp_filename);
		
		if (!Directory.Exists(GetPublicGalleryPath(album) + "/")) Directory.CreateDirectory(GetPublicGalleryPath(album) + "/");
		StartCoroutine(waitAndCheckForLatestScreenshot(unique_agent_temp_filename, GetPublicGalleryPath(album) + "/" + filename, handle));
		RefreshGallery();
	}
	public void		SaveScreenshotToPrivateFolderAsync(string filename, string album, HandleSaveScreenshotEvent handle)
	{
		if (File.Exists(Application.persistentDataPath + "/" + unique_agent_temp_filename)) File.Delete(Application.persistentDataPath + "/" + unique_agent_temp_filename);
		Application.CaptureScreenshot(unique_agent_temp_filename);
		
		if (!Directory.Exists(GetPrivateFolderPath(album) + "/")) Directory.CreateDirectory(GetPrivateFolderPath(album) + "/");
		StartCoroutine(waitAndCheckForLatestScreenshot(unique_agent_temp_filename,
			GetPrivateFolderPath(album) + "/" + filename
			, handle));
	}
	public string		GetPublicGalleryPath(string album)
	{
		return _GetPublicGalleryPath(album);
	}
	public string		GetPrivateFolderPath(string album)
	{
		return _GetPrivateFolderPath(album);
	}
	public void			RefreshGallery() { _RefreshGallery(); }
	
	IEnumerator waitAndCheckForLatestScreenshot(string source, string destpath, HandleSaveScreenshotEvent handle)
	{
		yield return new WaitForSeconds(1);
		if (System.IO.File.Exists(Application.persistentDataPath + "/" + source))
		{
			if (destpath != "")
			{
				if (File.Exists(destpath)) File.Delete(destpath);
				System.IO.File.Move(Application.persistentDataPath + "/" + source, destpath);
			}
			if (handle != null) handle(SaveScreenshotEvent.Success);
		} else
			StartCoroutine(waitAndCheckForLatestScreenshot(source, destpath, handle));
	}
	
#region IAgentEventListener implementation
	public void		ScreenshotReceiveAgentEvent(string data)
	{
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
