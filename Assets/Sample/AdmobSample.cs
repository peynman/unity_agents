using UnityEngine;
using System.Collections;

public class AdmobSample : AgentUISample 
{
	public override string Title { get { return "Admob Agent"; } }
	
	public string		BannerUID = "";
	
	int 	SampleBannerID = 0;
	bool	isFullscreenBannerLoaded = false;
	
	void Start()
	{
		AdmobAgent.instance.OnFullscreenAdEvent += delegate(AdmobAgent.AdmobEvent e, string error) 
		{
			switch (e)
			{
			case AdmobAgent.AdmobEvent.OnReceiveAd:
				isFullscreenBannerLoaded = true;
				break;
			case AdmobAgent.AdmobEvent.OnFailedToReceiveAd:
				isFullscreenBannerLoaded = false;
				break;
			case AdmobAgent.AdmobEvent.OnPresentScreen:
				// pause game or silent music or something like this :)
				break;
			case AdmobAgent.AdmobEvent.OnDismissScreen:
				// resume whatever you where doing
				break;
			}
			Debug.Log("Fullscreen banner event: " + e);
		};
	}
	
	public override void DrawAgentGUI (Samples s)
	{
		if (SampleBannerID == 0)
		{
			if (GUILayout.Button("Create New Banner", s.ButtonStyle1))
			{
				SampleBannerID = AdmobAgent.instance.CreateBanner(AdmobAgent.AdmobBannerSize.SmartBanner, BannerUID,
				delegate(AdmobAgent.AdmobEvent e, int width, int height, string error)
				{
					Debug.Log("Admob Banner Event: " + e);
				});
				
			}
		} else
		{
			GUILayout.Label("is Banner Received Ad: " + AdmobAgent.instance.GetBanner(SampleBannerID).AdLoaded);
			if (AdmobAgent.instance.GetBanner(SampleBannerID).AdLoaded)
			{
				if (GUILayout.Button("Show/Hide Banner", s.ButtonStyle1))
				{
					AdmobAgent.instance.SetBannerVisibility(SampleBannerID, !AdmobAgent.instance.isBannerVisible(SampleBannerID));
				}
				if (GUILayout.Button("Move Banner", s.ButtonStyle1))
				{
					AdmobAgent.instance.SetBannerPosition(SampleBannerID, 0, Random.Range(0, 100));
				}
				if (GUILayout.Button("Refresh Ad", s.ButtonStyle1))
				{
					AdmobAgent.instance.RefreshBanner(SampleBannerID);
				}
				if (GUILayout.Button("Destroy Banner", s.ButtonStyle1))
				{
					AdmobAgent.instance.DestroyBanner(ref SampleBannerID);
				}
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Request Fullscreen Banner", s.ButtonStyle1))
			{
				isFullscreenBannerLoaded = false;
				AdmobAgent.instance.RequestFullscreenAd(BannerUID);
			}
			if (isFullscreenBannerLoaded)
			{
				if (GUILayout.Button("Show Fullscreen Banner", s.ButtonStyle1))
				{
					isFullscreenBannerLoaded = false;
					AdmobAgent.instance.ShowFullscreenAd();
				}
			}
		}
	}
}
