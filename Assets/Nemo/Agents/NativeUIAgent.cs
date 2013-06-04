#define	ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NativeUIAgent : NemoAgent
{
	public enum NativeUIEvent 
	{
		MessageBoxButton = 1,
		InputBoxButton = 2,
		PopupMenuButton = 3
	}
	
	public static NativeUIAgent	instance { get { return _instance; } }
	private static NativeUIAgent	_instance;
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	public static string	ObjectClassPath = "com.nemogames.NativeUIAgent";
	AndroidJavaObject 		android;
	private void		_ShowMessageBox(string title, string message, string button1, string button2, string button3)
	{
		android.Call("ShowMessageBox", title, message, button1, button2, button3);
	}
	private void		_ShowMessage(string message, float time)
	{
		android.Call("ShowMessage", message, ((time > 1)? false:true));
	}
	private void		_ShowBusy(string title, string message, bool indicator)
	{
		android.Call("ShowProgressDialog", title, message, indicator);
	}
	private void		_CloseBusy()
	{
		android.Call("CloseBusy");
	}
	private void		_TakeScreenshot(string path)
	{
		android.Call("TakeScreenshot", path);
	}
	private void		_ShowWebView(string title, string url, int width, int height)
	{
		android.Call("ShowWebView", title, url, width, height);
	}
	private void		_ShowMessageBoxWithInput(string title, string message, string button1, string button2, string button3)
	{
		android.Call("ShowMessageBoxWithInput", title, message, button1, button2, button3);
	}
	private void		_ShowPopupMenu(string[] buttons)
	{
		using (AndroidJavaObject array = new AndroidJavaObject(AndroidJNIHelper.ConvertToJNIArray(buttons)))
		{
			android.Call("ShowPopupMenu", array);
		}
	}
	private void		_ShowComposeMail(string address, string subject, string body, string attachment)
	{
		android.Call("ShowComposeMail", address, subject, body, attachment);
	}
#else
	private void		_ShowMessageBox(string title, string message, string button1, string button2, string button3) {}
	private void		_ShowMessage(string message, float time) {}
	private void		_ShowBusy(string title, string message, bool indicator) {}
	private void		_CloseBusy() {}
	private void		_TakeScreenshot(string path) {}
	private void		_ShowWebView(string title, string url, int width, int height) {}
	private void		_ShowMessageBoxWithInput(string title, string message, string button1, string button2, string button3) {}
	private void		_ShowPopupMenu(string[] buttons) {}
	private void		_ShowComposeMail(string address, string subject, string body, string attachment) {}
#endif
	
	void Awake()
	{
		_instance = this;
#if UNITY_ANDROID && ENABLE_PLUGIN
		android = new AndroidJavaObject(ObjectClassPath);
		android.Call("init", gameObject.name, "NativeUIReceiveAgentEvent");
#endif
	}
	
	public delegate void		HandleMessageBoxEvent(int button_index);
	public delegate void		HandleMessageBoxWithInputEvent(int button_index, string input);
	public delegate void		HandlePopupEvent(int button_index);
	
	HandleMessageBoxEvent 			msgbox_handle;
	HandleMessageBoxWithInputEvent	input_handle;
	HandlePopupEvent				popup_handle;
	
	public void		ShowMessageBox(string title, string message, string button_0, string button_1, string button_2, HandleMessageBoxEvent handle)
	{
		msgbox_handle = handle;
		_ShowMessageBox(title, message, button_0, button_1, button_2);
	}
	
	public void		ShowMessage(string message, float time) { _ShowMessage(message, time); }
	public void		ShowBusy(string title, string message, bool indicator) { _ShowBusy(title, message, indicator); }
	public void		CloseBusy() { _CloseBusy(); }
	public void		TakeScreenshot(string path) { _TakeScreenshot(path); }	
	public void		ShowWebView(string title, string url, int width, int height) { _ShowWebView(title, url, width, height); }
	public void		ShowMessageBoxWithInput(string title, string message, string button_0, string button_1,
		string button_2, HandleMessageBoxWithInputEvent handle)
	{
		input_handle = handle;
		_ShowMessageBoxWithInput(title, message, button_0, button_1, button_2);
	}
	public void		ShowPopupMenu(string[] buttons, HandlePopupEvent handle)
	{
		popup_handle = handle;
		_ShowPopupMenu(buttons);
	}
	
	public void		ShowComposeMail(string address, string subject, string body, string attachment)
	{ _ShowComposeMail(address, subject, body, attachment); }
	
	#region IAgentEventListener implementation
	public void 	NativeUIReceiveAgentEvent (string json)
	{
		Hashtable data = MiniJSON.JsonDecode(json) as Hashtable;
		NativeUIEvent e = (NativeUIEvent)int.Parse(data["eid"].ToString());
		switch (e)
		{
		case NativeUIEvent.InputBoxButton:
			if (input_handle != null) 
			{		
				int buttond_index = int.Parse(data["button_index"].ToString());
				string inputstring = data["input"].ToString();
				input_handle(buttond_index, inputstring); 
			}
			break;
		case NativeUIEvent.MessageBoxButton:
			if (msgbox_handle != null) msgbox_handle(int.Parse(data["button_index"].ToString()));
			break;
		case NativeUIEvent.PopupMenuButton:
			if (popup_handle != null) popup_handle(int.Parse(data["button_index"].ToString()));
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
