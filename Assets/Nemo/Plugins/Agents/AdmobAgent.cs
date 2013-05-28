#define	ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class AdmobAgent : NemoAgent
{
	static AdmobAgent	_instance;
	public static AdmobAgent	instance { get { return _instance; } }
	void Awake()
	{
#if UNITY_ANDROID && ENABLE_PLUGIN
		android = new AndroidJavaObject(ObjectClassPath);
		android.Call("init", gameObject.name, "AdmobAgentReceiveAgentEvent");
#endif
		_instance = this;
		handles = new Dictionary<int, HandleAdmobEvent>();
		banners = new Dictionary<int, AdmobBanner>();
	}
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	public static string	ObjectClassPath = "com.nemogames.AdmobAgent";
	AndroidJavaObject 		android;
	private void		_CreateBanner(int iid, int size, string uid)
	{
		android.Call("CreateBanner", iid, size, uid);
	}
	private void		_SetBannerPosition(int banner, int left, int top)
	{
		android.Call("SetBannerPosition", banner, left, top);
	}
	private void		_SetBannerVisibility(int banner, bool state)
	{
		android.Call("SetBannerVisiblity", banner, state);
	}
	private void		_RefreshBanner(int banner)
	{
		android.Call("RefreshAdLoad", banner);
	}
	private void		_SetGender(string gender)
	{
		android.Call("SetGender", gender);
	}
	private void		_SetBirthdate(string year, string month, string day)
	{
		android.Call("SetBirthdate", year, month, day);
	}
	private void		_SetLocation(string location)
	{
		android.Call("SetLocation", location);
	}
	private void		_SetCustomRequestStatus(bool status)
	{
		android.Call("SetCustomRequestStatus", status);
	}
	private void		_RequestFullscreenAd(string uid)
	{
		android.Call("RequestFullscreenAd", uid);
	}
	private void		_ShowFullscreenAd()
	{
		android.Call("ShowFullscreenAd");
	}
	private bool		_isFullscreenAdReady()
	{
		return android.Call<bool>("isFullscreenAdReady");
	}
#else
	private void		_CreateBanner(int iid, int size, string uid) {}
	private void		_SetBannerPosition(int banner, int left, int top) {}
	private void		_SetBannerVisibility(int banner, bool state) {}
	private void		_RefreshBanner(int banner) {}
	private void		_SetGender(string gender) {}
	private void		_SetBirthdate(string year, string month, string day) {}
	private void		_SetLocation(string location) {}
	private void		_SetCustomRequestStatus(bool status) {}
#endif
	
	public enum AdmobEvent
	{
		OnDismissScreen = 1,
		OnFailedToReceiveAd = 2,
		OnLeaveApplication = 3,
		OnPresentScreen = 4,
		OnReceiveAd = 5,
		OnFullscreenAdReady = 6
	}
	public enum AdmobBannerSize
	{
		SmartBanner = 1,
		MMAStandardBanner = 2,
		IABStandardBanner = 3,
		IABMRect = 4,
		WideSkyScraper = 5,
		IABLeaderboard = 6
	}
	public enum AdmobGender
	{
		Male = 1,
		Female = 2
	}
	
	public class AdmobBanner
	{
		public AdmobBannerSize 	Size;
		public bool				Visible;
		public bool				AdLoaded;
		public string			UID;
		
		public AdmobBanner(AdmobBannerSize size, string id)
		{
			UID = id;
			Size = size;
			Visible = false;
			AdLoaded = false;
		}
	}
	
	Dictionary<int, HandleAdmobEvent>	handles;
	Dictionary<int, AdmobBanner>		banners;
	HandleFullscreenAdReady				fullscreenbanner_handle;
	private int		internal_id = 1;						
	
	public delegate void			HandleAdmobEvent(AdmobEvent e, int width, int height, string error);
	public delegate void			HandleFullscreenAdReady();
	
	public int		CreateBanner(AdmobBannerSize size, string uid, HandleAdmobEvent handle)
	{
		int iid = generateNextID();
		handles.Add(iid, handle);
		banners.Add(iid, new AdmobBanner(size, uid));
		_CreateBanner(iid, (int)size, uid);
		return iid;
	}
	
	public void		SetBannerPosition(int banner, int left, int top) { _SetBannerPosition(banner, left, top); }
	public void		SetBannerVisibility(int banner, bool state)
	{ _SetBannerVisibility(banner, state); banners[banner].Visible = state; }
	public void		RefreshBanner(int banner)
	{ _RefreshBanner(banner); banners[banner].AdLoaded = false; }
	public void		SetGender(AdmobGender gender) { _SetGender(gender.ToString()); }	
	public void		SetBirthdate(string year, string month, string day) { _SetBirthdate(year, month, day); }
	public void		SetLocation(string location) { _SetLocation(location); }
	public void		RequestFullscreenAd(string uid, HandleFullscreenAdReady handle)
	{
		fullscreenbanner_handle = handle;
		_RequestFullscreenAd(uid); 
	}
	public void		ShowFullscreenAd() { _ShowFullscreenAd(); }
	public bool		isFullscreenAdReady() { return _isFullscreenAdReady(); }
	public void		SetCustomRequestStatus(bool status) { _SetCustomRequestStatus(status); }
	public AdmobBanner		GetBanner(int banner_id) { return banners[banner_id]; }
	public bool				isBannerVisible(int banner_id) { return banners[banner_id].Visible; }
	public bool				isBannerLoadedWithAd(int banner_id) { return banners[banner_id].AdLoaded; }
	
	private int		generateNextID() { return ++internal_id; }
	
	#region IAgentEventListener implementation
	public void 	AdmobAgentReceiveAgentEvent (string json)
	{
		Hashtable data = MiniJSON.JsonDecode(json) as Hashtable;
		AdmobEvent eid = (AdmobEvent)int.Parse(data["eid"].ToString());
		if (eid == AdmobEvent.OnFullscreenAdReady)
		{
			if (fullscreenbanner_handle != null) fullscreenbanner_handle();
		} else
		{
			int iid = int.Parse(data["iid"].ToString());
			if (handles.ContainsKey(iid))
			{
				HandleAdmobEvent ehandle = handles[iid];
				int width = 0, height = 0;
				string error = "";
				if (eid == AdmobEvent.OnReceiveAd)
				{
					width = int.Parse(data["width"].ToString());
					height = int.Parse(data["height"].ToString());
					banners[iid].AdLoaded = true;
					banners[iid].Visible = true;
				}
				if (eid == AdmobEvent.OnFailedToReceiveAd)
				{
					error = data["error"].ToString();
					banners[iid].AdLoaded = false;
					banners[iid].Visible = false;
				}
				if (ehandle != null) ehandle(eid, width, height, error);
			} else
			{
				Debug.LogWarning("Received event without a hanlde (AdmobAgent). IID:" + iid);
			}
		}
	}
	#endregion

	#region NemoAgent implementation
#if ENABLE_PLUGIN
	public override bool			isEnable { get { return true; } }
#else
	public override bool		isEnable { get { return false; } }	
#endif
	public override string		Title { get { return "Admob"; } }
	public override string		Description { get { return "Admob Advertising"; } }
	public override string 		VersionName { get { return "1.0"; } }
#if UNITY_ANDROID
	public override string		ElementName { get { return "admob-agent"; } }
#endif
	#endregion
}
