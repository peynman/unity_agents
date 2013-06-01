using UnityEngine;
using System.Collections;

public class FlurrySample : NativeUISample 
{
	public override string Title { get { return "Flurry Agent"; } }
	public override void DrawAgentGUI (Samples s)
	{
		GUILayout.Label("Flurry is integrated already. Nothing needs to be done.");
	}
}
