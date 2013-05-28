using UnityEngine;
using System.Collections;

public abstract class AgentUISample : MonoBehaviour
{
	public abstract void		DrawAgentGUI(Samples s);
	public abstract	string		Title { get; }
}

public class Samples : MonoBehaviour 
{
	public string[]			AgentsName;
	public AgentUISample[]	SampleGUIs;
	public GUIStyle			ButtonStyle1;
	
	Rect		mainwindow;
	int			selected_index = -1;
	void Start()
	{
		AgentUISample[] agents = GameObject.FindObjectsOfType(typeof(AgentUISample)) as AgentUISample[];
		AgentsName = new string[agents.Length];
		SampleGUIs = new AgentUISample[agents.Length];
		for (int i = 0; i < agents.Length; i++)
		{
			AgentsName[i] = agents[i].Title;
			SampleGUIs[i] = agents[i];
		}
		
		mainwindow = GetCenterRect(Screen.width*0.7f, 80, 0 , -100);
	}
	
	void OnGUI()
	{
		ButtonStyle1 = new GUIStyle("Button");
		ButtonStyle1.fixedHeight = 30;
		
		mainwindow = GUILayout.Window(1, mainwindow,
		delegate(int id)
		{
			selected_index = GUILayout.SelectionGrid(selected_index, AgentsName, 3);
			if (selected_index >= 0 && selected_index < SampleGUIs.Length)
			{
				GUILayout.Label(AgentsName[selected_index]);
				SampleGUIs[selected_index].DrawAgentGUI(this);
			}
		}, "Agents");
	}
	
	public Rect		GetCenterRect(float width, float height, float offx, float offy)
	{
		return  new Rect((Screen.width-width)/2.0f+offx, (Screen.height-height)/2.0f+offy, width, height);
	}
	
}
