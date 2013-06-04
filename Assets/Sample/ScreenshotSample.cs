using UnityEngine;
using System.Collections;

public class ScreenshotSample : AgentUISample
{
	public override string Title { get { return "Screenshot Agent"; } }

	public override void DrawAgentGUI (Samples s)
	{
		GUILayout.Label("Path to public images: " + ScreenshotAgent.instance.GetPublicGalleryPath("Agents"));
		GUILayout.Label("Path to private images: " + ScreenshotAgent.instance.GetPrivateFolderPath("Agents"));
		
		if (GUILayout.Button("Capture to Gallery", s.ButtonStyle1))
		{
			ScreenshotAgent.instance.SaveScreenshotToPublicGalleryAsync(
				System.DateTime.Now.ToString("MMM-d-yyyy-H-mm-ss") + ".jpg", "Agents", 
			delegate(ScreenshotAgent.SaveScreenshotEvent e) 
			{
				
			});
		}
		
		if (GUILayout.Button("Capture to Private folder", s.ButtonStyle1))
		{
			ScreenshotAgent.instance.SaveScreenshotToPrivateFolderAsync(
				System.DateTime.Now.ToString("MMM-d-yyyy-H-mm-ss") + ".jpg", "Agents",
			delegate(ScreenshotAgent.SaveScreenshotEvent e) 
			{
				
			});
		}
	}
}
