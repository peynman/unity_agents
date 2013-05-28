using UnityEngine;
using System.Collections;

public class NativeUISample : AgentUISample
{
	public override string Title { get { return "Native UI Agent"; } }

	public override void DrawAgentGUI (Samples s)
	{
		if (GUILayout.Button("Sample Message Box", s.ButtonStyle1))
		{
			NativeUIAgent.instance.ShowMessageBox("Sample Title", "Sample Message Goes Here", "Positive", "Natural", "Negetive",
			delegate (int button)
			{
				NativeUIAgent.instance.ShowMessageBox("Sample Result", "You selected button with index:" + button, "OK", "", "", null);
			});
		}
		if (GUILayout.Button("Message Box With Input", s.ButtonStyle1))
		{
			NativeUIAgent.instance.ShowMessageBoxWithInput("Box With Input", "Enter Place holder text here", "OK", "",
			delegate (int button, string text)
			{
				NativeUIAgent.instance.ShowMessageBox("Input Result", "You entered: " + text + " and selected button with index: " + button, "OK", "", "", null);
			});
		}
		if (GUILayout.Button("Popup Menu", s.ButtonStyle1))
		{
			NativeUIAgent.instance.ShowPopupMenu(new string[] { "Button 1", "Button 2", "Button 3", "Button 4"},
			delegate(string button)
			{
				NativeUIAgent.instance.ShowMessageBox("Popup Result", "You selected button with text: " + button, "OK", "", "", null);
			});
		}
		if (GUILayout.Button("Show Webpage", s.ButtonStyle1))
		{
			NativeUIAgent.instance.ShowWebView("Web Title", "https://www.nemo-games.com", 600, 400);
		}
		if (GUILayout.Button("Compose Mail", s.ButtonStyle1))
		{
			NativeUIAgent.instance.ShowComposeMail("contact@nemo-games.com", "Email Subject", "Email Body");
		}
		if (GUILayout.Button("Show Message", s.ButtonStyle1))
		{
			NativeUIAgent.instance.ShowMessage("A Message for a short amount of time :)", 1);
		}
	}
} 
