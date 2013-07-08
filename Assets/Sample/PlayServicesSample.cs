using UnityEngine;
using System.Collections;

public class PlayServicesSample : AgentUISample 
{
	public override string Title { get { return "Play Services"; } }
	public override void DrawAgentGUI (Samples s)
	{
		GUILayout.Label("is Play Services Available: " + PlayServicesAgent.instance.isPlayServicesAvailable(false));
		if (!PlayServicesAgent.instance.isPlayServicesAvailable(false))
		{
			if (GUILayout.Button("Show Error", s.ButtonStyle1)) PlayServicesAgent.instance.isPlayServicesAvailable(true);
		}
		if (PlayServicesAgent.instance.isPlayServicesAvailable(false))
		{
			GUILayout.Label("is Connected: " + PlayServicesAgent.instance.isConnected());
			if (!PlayServicesAgent.instance.isConnected())
			{
				GUILayout.Label("is Connecting: " + PlayServicesAgent.instance.isConnecting());
				if (GUILayout.Button("Connect", s.ButtonStyle1)) PlayServicesAgent.instance.Connect();
			} else
			{
				if (GUILayout.Button("Show Leaderboards", s.ButtonStyle1))
					PlayServicesAgent.instance.ShowLeaderboards();
				if (GUILayout.Button("Show Achievements", s.ButtonStyle1))
					PlayServicesAgent.instance.ShowAchievements();
				if (GUILayout.Button("Submit Score", s.ButtonStyle1))
					PlayServicesAgent.instance.SubmitScore("CgkI-eHf1dcFEAIQBg", Random.Range(0, Random.Range(100, 1000)));
				if (GUILayout.Button("Unlock Achievement", s.ButtonStyle1))
					PlayServicesAgent.instance.UnlockAchievement("CgkI-eHf1dcFEAIQAQ");
				if (GUILayout.Button("Increament Achievement", s.ButtonStyle1))
					PlayServicesAgent.instance.IncrementAchievement("CgkI-eHf1dcFEAIQBQ", 1);
			}
		}
	}
}
