using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FacebookSample : AgentUISample 
{
	public override string Title { get { return "Facebook Agent"; } }
	
	public Texture2D		UploadPhoto;
	
	public override void DrawAgentGUI (Samples s)
	{
		bool isloggedin = FacebookAgent.instance.isLoggedIn();
		GUILayout.Label("Is Logged in? " + isloggedin);
		GUILayout.Label("Access Token: " + FacebookAgent.instance.getAccessToken());
		GUILayout.Label("Session State: " + FacebookAgent.instance.getSessionState());
		GUILayout.Label("has Publish Permission: " + FacebookAgent.instance.hasPublishPermission());
		if (!isloggedin)
		{
			if (GUILayout.Button("Login", s.ButtonStyle1)) FacebookAgent.instance.Login();
		} else
		{
			if (GUILayout.Button("Logout", s.ButtonStyle1)) FacebookAgent.instance.Logout();
			if (GUILayout.Button("Fetch Graph User", s.ButtonStyle1)) FacebookAgent.instance.FetchGraphUser();
			if (GUILayout.Button("Fetch User Friends", s.ButtonStyle1)) FacebookAgent.instance.FetchUserFriends();
			if (GUILayout.Button("Request Publish Permissions", s.ButtonStyle1))
				FacebookAgent.instance.RequestNewPublishPermission(new string[] { "publish_actions" });
			if (GUILayout.Button("Request Email Permission", s.ButtonStyle1))
				FacebookAgent.instance.RequestNewReadPermission(new string[] { "email" });
			if (GUILayout.Button("Feed Dialog", s.ButtonStyle1))
			{
				Dictionary<string, string>	bundle = new Dictionary<string, string>();
				FacebookAgent.FillBundle(ref bundle, "Name Goes Here", "Caption Goes Here", "http://wwww.nemo-games.com",
					"http://wwww.nemo-games.com/icon.png");
				FacebookAgent.instance.ShowFeedDialog(bundle);
			}
			if (GUILayout.Button("Feed With Picture", s.ButtonStyle1))
			{
				
				FacebookAgent.instance.UploadPhotoRequest(UploadPhoto);
			}
		}
	}
}
