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
		android.Call("RequestFreshAd", banner);
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
	private void		_DestroyBanner(int banner_id)
	{
		android.Call("DestroyBanner", banner_id);
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
	private void		_RequestFullscreenAd(string uid) {}
	private void		_ShowFullscreenAd() {}
	private bool		_isFullscreenAdReady() { return false; }
	private void		_DestroyBanner(int banner_id) {}
#endif
	
	public enum AdmobEvent
	{
		OnDismissScreen = 1,
		OnFailedToReceiveAd = 2,
		OnLeaveApplication = 3,
		OnPresentScreen = 4,
		OnReceiveAd = 5
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
	private int		internal_id = 1;						
	
	public delegate void			HandleAdmobEvent(AdmobEvent e, int width, int height, string error);
	public delegate void			HandleFullscreenAdEvent(AdmobEvent e, string error);
	public event HandleFullscreenAdEvent		OnFullscreenAdEvent;
	
	public int		CreateBanner(AdmobBannerSize size, string uid, HandleAdmobEvent handle)
	{
		int iid = generateNextID();
		handles.Add(iid, handle);
		banners.Add(iid, new AdmobBanner(size, uid));
		_CreateBanner(iid, (int)size, uid);
		return iid;
	}
	
	public void		SetBannerPosition(int banner, int left, int top) 
	{
		_SetBannerPosition(banner, left, top); 
	}
	public void		SetBannerVisibility(int banner, bool state)
	{ _SetBannerVisibility(banner, state); banners[banner].Visible = state; }
	public void		RefreshBanner(int banner)
	{ _RefreshBanner(banner); banners[banner].AdLoaded = false; }
	public void		SetGender(AdmobGender gender) { _SetGender(gender.ToString()); }	
	public void		SetBirthdate(string year, string month, string day) { _SetBirthdate(year, month, day); }
	public void		SetLocation(string location) { _SetLocation(location); }
	public void		RequestFullscreenAd(string uid) { _RequestFullscreenAd(uid);  }
	public void		ShowFullscreenAd() { _ShowFullscreenAd(); }
	public bool		isFullscreenAdReady() { return _isFullscreenAdReady(); }
	public void		SetCustomRequestStatus(bool status) { _SetCustomRequestStatus(status); }
	public AdmobBanner		GetBanner(int banner_id) { return banners[banner_id]; }
	public bool				isBannerVisible(int banner_id) { return banners[banner_id].Visible; }
	public bool				isBannerLoadedWithAd(int banner_id) { return banners[banner_id].AdLoaded; }
	public void				DestroyBanner(ref int banner_id)
	{
		_DestroyBanner(banner_id);
		handles.Remove(banner_id);
		banners.Remove(banner_id);
		banner_id = 0;
	}
	public void				SetBannerEventListener(int banner_id, HandleAdmobEvent handle)
	{
		handles[banner_id] = handle;
	}
	
	private int		generateNextID() { return ++internal_id; }
	
	#region IAgentEventListener implementation
	public void 	AdmobAgentReceiveAgentEvent (string json)
	{
		Hashtable data = MiniJSON.JsonDecode(json) as Hashtable;
		AdmobEvent eid = (AdmobEvent)int.Parse(data["eid"].ToString());
		int iid = int.Parse(data["iid"].ToString());
		string error = "";
		if (iid == -1000)
		{
			if (eid == AdmobEvent.OnFailedToReceiveAd) error = data["error"].ToString();
			if (OnFullscreenAdEvent != null) OnFullscreenAdEvent(eid, error);
		} else
		{
			if (handles.ContainsKey(iid))
			{
				HandleAdmobEvent ehandle = handles[iid];
				int width = 0, height = 0;
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
	public override bool		isEnable { get { return true; } }
#else
	public override bool		isEnable { get { return false; } }	
#endif
	#endregion
}
