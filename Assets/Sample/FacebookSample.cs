using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FacebookSample : AgentUISample
{
	public override string Title { get { return "Facebook Agent"; } }
	
	public Texture2D		UploadPhoto;
	
	public override void DrawAgentGUI (Samples s)
	{
		GUILayout.Label("is Session Open: " + FacebookAgent.instance.isSessionOpened());
		GUILayout.Label("Session State: " + FacebookAgent.instance.getSessionState());
		if (!FacebookAgent.instance.isSessionOpened())
		{
			if (GUILayout.Button("Open For Read", s.ButtonStyle1))
			{
				FacebookAgent.instance.OpenForRead(new string[] { "email" }, null);
			}
			if (GUILayout.Button("Open For Publish", s.ButtonStyle1))
			{
				FacebookAgent.instance.OpenForPublish(new string[] { "publish_actions" }, null);
			}
		} else
		{
			GUILayout.Label("Session Control");
			if (GUILayout.Button("Close", s.ButtonStyle1))
			{
				FacebookAgent.instance.Close();
			}
			if (GUILayout.Button("Close And Clear", s.ButtonStyle1))
			{
				FacebookAgent.instance.CloseAndClear();
			}
			GUILayout.Label("Permissions");
			GUILayout.Label("Publish permission: " + FacebookAgent.instance.hasPublishPermission());
			GUILayout.Label("Email permission: " + FacebookAgent.instance.hasPermission("email"));
			if (!FacebookAgent.instance.hasPermission("email"))
			{
				if (GUILayout.Button("Request New Read Permission", s.ButtonStyle1))
				{
					FacebookAgent.instance.RequestNewReadPermission(new string[] { "email" },
					delegate(FacebookAgent.FacebookRequestEvent e, string error) 
					{
						Debug.Log("Request Result: " + e + " > error? " + error);
					});
				}
			} else
			{
				GUILayout.Label("Read");
				if (GUILayout.Button("Request Me", s.ButtonStyle1))
				{
					FacebookAgent.instance.ExecuteMeRequest(
					delegate(FacebookAgent.FacebookRequestEvent e, string error) 
					{
						Debug.Log("Request Result: " + e + " > error? " + error);
					});
				}
				if (GUILayout.Button("Request My Friends", s.ButtonStyle1))
				{
					FacebookAgent.instance.ExecuteMyFriendsRequest(
					delegate(FacebookAgent.FacebookRequestEvent e, string error) 
					{
						Debug.Log("Request Result: " + e + " > error? " + error);
					});
				}
			}
			if (!FacebookAgent.instance.hasPublishPermission())
			{
				if (GUILayout.Button("Request New Publish Permission", s.ButtonStyle1))
				{
					FacebookAgent.instance.RequestNewPublishPermission(new string[] { "publish_actions" },
					delegate(FacebookAgent.FacebookRequestEvent e, string error) 
					{
						Debug.Log("Request Result: " + e + " > error? " + error);
					});
				}
			} else
			{
				GUILayout.Label("Publish");
				if (GUILayout.Button("Request Photo upload", s.ButtonStyle1))
				{
					FacebookAgent.instance.ExecuteUploadPhotoRequest("This is a test", UploadPhoto,
					delegate(FacebookAgent.FacebookRequestEvent e, string error) 
					{
						Debug.Log("Request Result: " + e + " > error? " + error);
					});
				}
				if (GUILayout.Button("Request Status update", s.ButtonStyle1))
				{
					FacebookAgent.instance.ExecuteStatusUpdateRequest(
					FacebookBundle.CreateBundle("Name", "Caption goes here", "Message goes here", "http://www.nemo-games.com", ""),
					delegate(FacebookAgent.FacebookRequestEvent e, string error) 
					{
						Debug.Log("Request Result: " + e + " > error? " + error);
					});

				}
				if (GUILayout.Button("Request Graph path", s.ButtonStyle1))
				{
					
				}
				if (GUILayout.Button("Show Feed dialog", s.ButtonStyle1))
				{
					FacebookAgent.instance.ShowFeedDialog(
					FacebookBundle.CreateBundle("Name", "Caption goes here", "Message goes here", "http://www.nemo-games.com", ""),
					delegate(FacebookAgent.FacebookDialogEvent e, string error) 
					{
						Debug.Log("Dialog Result: " + e + " > error? " + error);
					});
				}
			}
		}
	}
}
