using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

public class AndroidPluginManager : EditorWindow 
{
	Manifest			manifest;
	AgentDependency		plugins;
	ManifestResource	strings;
	
	GUIStyle	PositiveButton = new GUIStyle("Button");
	GUIStyle	NegetiveButton = new GUIStyle("Button");
	GUIStyle	BoldLabel = new GUIStyle("Label"),
				SubBoldLabel = new GUIStyle("Label"),
				GreenLabel = new GUIStyle("Label"),
				RedLabel = new GUIStyle("Label"),
				YellowLabel = new GUIStyle("Label");
	GUIStyle	NormalLabel = new GUIStyle("Label");
	Vector2		scroll = Vector2.zero;
	AgentManifest	agent = null;
	Vector2	window_scroll = Vector2.zero;
	
	List<AgentSetVersion>	Versions;
	UnityGUI_Versions 		guiVersions;
	UnityGUI_AgentVersion	guiProperties;
	
	
	void initialize()
	{
		manifest = new Manifest();
		plugins = new AgentDependency();
		if (System.IO.File.Exists(AgentDependency.AgentsFile))
		{
			plugins = AndroidAgentEditor.LoadAgentsFromFile(AgentDependency.AgentsFile);
			if (System.IO.File.Exists(Manifest.ManifestFile))
				manifest = AndroidManifestEditor.LoadManifestFromFile(Manifest.ManifestFile);
			else
				manifest = new Manifest();
			/*
			if (System.IO.File.Exists(ManifestResource.StringsFilename))
				strings = StringEditor.LoadResourcesFromFile(ManifestResource.StringsFilename);
			else */
				strings = new ManifestResource();
		} else
			Debug.LogError("AgentDependencies.xml not found.");

		PositiveButton.fixedWidth = 30;
		PositiveButton.stretchWidth = false;
		NegetiveButton.fixedWidth = 30;
		NegetiveButton.stretchWidth = false;
		BoldLabel.normal.textColor = Color.white;
		BoldLabel.fontStyle = FontStyle.Bold;
		SubBoldLabel.fontStyle = FontStyle.Bold;
		RedLabel.normal.textColor = new Color(0.8f, 0.1f, 0.1f, 1.0f);
		GreenLabel.normal.textColor = new Color(0.1f, 0.8f, 0.1f, 1.0f);
		YellowLabel.normal.textColor = new Color(0.8f, 0.8f, 0.1f, 1.0f);
		
		if (System.IO.Directory.Exists(AgentVersion.VersionsPath))
			Versions = GetVersionsFromPath(AgentVersion.VersionsPath, plugins);
		else
			Versions = new List<AgentSetVersion>();
		
		guiVersions = new UnityGUI_Versions(Versions, plugins, manifest);
		guiProperties = new UnityGUI_AgentVersion(manifest);
		guiVersions.SetStyles(PositiveButton, NegetiveButton, BoldLabel, GreenLabel, RedLabel, YellowLabel);
		guiProperties.SetStyles(PositiveButton, NegetiveButton, SubBoldLabel, GreenLabel, RedLabel, YellowLabel);
	}
	void OnGUI()
	{
		window_scroll = EditorGUILayout.BeginScrollView(window_scroll);
		guiVersions.OnGUI();
		if (guiVersions.EditingVersion != null)
		{
			OnGUI_Version(guiVersions.EditingVersion);
			if (agent != null && guiVersions.EditingVersion.hasPlugin(agent.filename))
			{
				EditorGUILayout.LabelField("Plugin Properties: " + agent.filename, BoldLabel);
				guiProperties.OnGUI(guiVersions.EditingVersion.getVersionOfPlugin(agent.filename));
			}
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space();
		if (GUILayout.Button("Export Version"))
		{
			if (guiVersions.EditingVersion != null)
				SaveVersionToFile(GetVersionFilename(guiVersions.EditingVersion), guiVersions.EditingVersion);
		}
		if (GUILayout.Button("Apply Current Version"))
		{
			if (guiVersions.EditingVersion != null)
			{
				SaveVersionToFile(GetVersionFilename(guiVersions.EditingVersion), guiVersions.EditingVersion);
				ApplyVersion(guiVersions.EditingVersion);
			}
		}
		if (GUILayout.Button("Close"))
		{
			this.Close();
		}
		EditorGUILayout.Space();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		
	}
	
	void OnGUI_Version(AgentSetVersion version)
	{
		GUIStyle version_style;
		EditorGUILayout.LabelField("All Plugins", BoldLabel);
		scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(180));
		for (int i = 0; i < plugins.Agents.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			if (version.hasPlugin(plugins.Agents[i].filename))
			{
				AgentVersion v = version.getVersionOfPlugin(plugins.Agents[i].filename);
				v.status = EditorGUILayout.Toggle(v.status, GUILayout.MaxWidth(50));
				if (v.status == false)
					version_style = NormalLabel;
				else
				{
					if (v.isVersionReady())
					{
						if (v.isManifestReady(manifest) && v.isStringsReady(strings)) version_style = GreenLabel;
						else version_style = YellowLabel;
					} else version_style = RedLabel;
				}
			} else
			{
				bool status = EditorGUILayout.Toggle(false, GUILayout.MaxWidth(50));
				if (status == true)
				{
					version.addPlugin().ImportFromManifest(plugins.Agents[i], true);
					version_style = RedLabel;
				} else
					version_style = NormalLabel;
			}
			EditorGUILayout.LabelField(plugins.Agents[i].filename, version_style);
			if (GUILayout.Button("...", PositiveButton))
			{
				agent = plugins.Agents[i];
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
	}
	
#region Apply
	private void		ApplyVersion(AgentSetVersion v)
	{
		PlayerSettings.bundleVersion = v.versionName;
		PlayerSettings.Android.bundleVersionCode = int.Parse(v.versionCode);
		PlayerSettings.bundleIdentifier = v.bundleid;
		manifest.package = v.bundleid;
		manifest.versionCode = v.versionCode;
		manifest.versionName = v.versionName;
		
		foreach (AgentManifest mans in plugins.Agents)
		{
			if (!v.hasPlugin(mans.filename) || v.getVersionOfPlugin(mans.filename).status==false)
			{
				RemoveDependencies(mans);
			} else
			{
				AddPluginDependencies(v.getVersionOfPlugin(mans.filename));
			}
		}
		AndroidManifestEditor.SaveManifestToFile(Manifest.ManifestFile, manifest);
		//StringEditor.SaveResourcesToFile(ManifestResource.StringsFilename, strings);
		Debug.Log("Done :)");
	}
	private void		AddPluginDependencies(AgentVersion plug)
	{
		SetPreDefinitionForFilename(plug.filename);
		foreach (ManifestMetaData source in plug.MetaData)
		{
			if (!manifest.application.hasMetaData(source.name))
				manifest.application.addMetaData().name = source.name;
			manifest.application.getMetaData(source.name).value = source.value;
			if (source.resource != "NOT SET")
				manifest.application.getMetaData(source.name).resource = source.resource;
		}
		foreach (ManifestActivity act in plug.ManifestSource.Activity)
		{
			if (!manifest.application.hasActivity(act.name))
				manifest.application.addActivity().name = act.name;
			CopyActivityData(act, manifest.application.getActivity(act.name));
		}
		foreach (ManifestPermission perm in plug.ManifestSource.Permission)
		{
			if (!manifest.hasPermission(perm.name)) manifest.addPermission().name = perm.name;
		}
		for (int i = 0; i < plug.ManifestSource.Strings.Count; i++)
		{
			if (strings.hasName(plug.ManifestSource.Strings.names[i]))
				strings.setValue(plug.ManifestSource.Strings.names[i], plug.ManifestSource.Strings.values[i]);
			else
				strings.addValue(plug.ManifestSource.Strings.names[i], plug.ManifestSource.Strings.values[i]);
		}
		if (!System.IO.Directory.Exists(Manifest.LibsFolder)) System.IO.Directory.CreateDirectory(Manifest.LibsFolder);
		CopyLibraryFiles(System.IO.Directory.GetFiles(plug.ManifestSource.LibraryFolder));
		if (plug.ManifestSource.hasResources())
		{
			ResFolder resfolder = new ResFolder(plug.ManifestSource.ResFolder);
			resfolder.CopyHierarchyTo(Manifest.ResFolder);
		}
	}
	private void		RemoveDependencies(AgentManifest mans)
	{
		// todo: clear manifest
		
		if (mans.hasResources())
		{
			ResFolder resfolder = new ResFolder(mans.ResFolder);
			resfolder.ClearHierarchyFrom(Manifest.ResFolder);
		}
		ResFolder libfolder = new ResFolder(mans.LibraryFolder);
		libfolder.ClearHierarchyFrom(Manifest.LibsFolder);
	}
	private void		CopyLibraryFiles(string[] libs)
	{
		foreach (string libname in libs)
		{
			if (libname.EndsWith(".jar"))
			{
				string destname = Manifest.LibsFolder + libname.Substring(libname.LastIndexOf("/")+1);
				if (System.IO.File.Exists(destname)) System.IO.File.SetAttributes(destname, System.IO.FileAttributes.Normal);
				System.IO.File.Copy(libname, destname, true);
			}
		}
	}
	private void		CopyResFiles(string folder)
	{
		string[] files = System.IO.Directory.GetFiles(folder);
		string[] folders = System.IO.Directory.GetDirectories(folder);
		if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
		foreach (string resfile in files)
		{
			string filename = resfile.Substring(resfile.LastIndexOf("/"));
			if (filename != "strings.xml")
				System.IO.File.Copy(resfile, Manifest.ResFolder, true);
			else
				ReadResourcesFileAndAddToStrings(resfile);
		}
		foreach (string resfolder in folders)
			CopyResFiles(resfolder);
	}
	private void		ReadResourcesFileAndAddToStrings(string filename)
	{
	}
	private void		CopyActivityData(ManifestActivity source, ManifestActivity dest)
	{
		if (source.name != dest.name) { Debug.LogError("Could not copy data!"); return; }
		dest.configChanges = source.configChanges;
		dest.label = source.label;
		dest.Startup = source.Startup;
		dest.meta_data.Clear();
		foreach (ManifestMetaData mmd in source.meta_data)
		{
			ManifestMetaData nmd = new ManifestMetaData();
			nmd.Set(mmd.name, mmd.value, mmd.resource);
			dest.meta_data.Add(nmd);
		}
	}
	private void		SetPreDefinitionForFilename(string filename)
	{
		string path = SearchPathForFilename(Application.dataPath, filename);
		if (System.IO.File.Exists(path))
		{
			System.IO.StreamReader reader = new System.IO.StreamReader(path);
			string line_1 = reader.ReadLine();
			if (line_1.StartsWith("#define") || line_1.StartsWith("#undef"))
			{
				
			}
		}
	}
	private string		SearchPathForFilename(string path, string filename)
	{
		string[] target = System.IO.Directory.GetFiles(path, filename, System.IO.SearchOption.AllDirectories);
		if (target.Length == 1) return target[0];
		else
		{
			Debug.Log("Could not found filename: " + filename + " or it was more than one!");
			return "";
		}
	}
#endregion
	
#region Static XML/IO helpers
	public static string					GetVersionFilename(AgentSetVersion v)
	{
		return AgentVersion.VersionsPath + v.name;
	}
	public static List<AgentSetVersion>		GetVersionsFromPath(string path, AgentDependency deps)
	{
		List<AgentSetVersion> versions = new List<AgentSetVersion>();
		string[] allnames = System.IO.Directory.GetFiles(path);
		for (int i = 0; i < allnames.Length; i++)
		{
			if (allnames[i].EndsWith(".xml"))
			{
				bool isvalid = true;
				AgentSetVersion newv = LoadVersionFromFile(allnames[i]);
				for(int j = 0; j < newv.Plugins.Count; j++)
				{
					AgentVersion plug = newv.Plugins[j];
					if (deps.getAgentWithFilename(plug.filename) != null)
						plug.ManifestSource = deps.getAgentWithFilename(plug.filename);
					else
					{
						newv.Plugins.RemoveAt(j);
						j--; continue;
					}
				}
				if (isvalid)
					versions.Add(newv);
				else
					Debug.Log("Could not add version from file: " + allnames[i] + ". Version missmatch or something like that!");
			}
		}
		return versions;
	}
	public static AgentSetVersion	LoadVersionFromFile(string path)
	{
		AgentSetVersion av = new AgentSetVersion();
		System.IO.StreamReader reader = new System.IO.StreamReader(path);
		string xmldata = reader.ReadToEnd();
		reader.Close();
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(xmldata);
		XmlNode agent = doc.GetElementsByTagName(AgentDependency.Element.AgentSet)[0];
		av.Read(agent);
		doc.Clone();
		return av;
	}
	public static void			SaveVersionToFile(string path, AgentSetVersion version)
	{
		XmlTextWriter	writer = new XmlTextWriter(path, System.Text.Encoding.UTF8);
		writer.Indentation = 4;
		writer.Formatting = Formatting.Indented;
		writer.Settings.NewLineHandling = NewLineHandling.Entitize;
		writer.Settings.NewLineOnAttributes = true;
		writer.WriteStartDocument();
		writer.WriteComment("This file is generated by Android Plugin Manager (created by Peyman Abdi peyman[at]nemo-games[dot]com).");
		version.Write(writer);
		writer.Close();
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}
#endregion
	
#if UNITY_ANDROID
	[MenuItem("Nemo/Android/Plugin Manager")]
	public static AndroidPluginManager		ShowPluginManagerWindow()
	{
		AndroidPluginManager editor = EditorWindow.GetWindow(typeof(AndroidPluginManager),
			true, "Android Manifest Editor") as AndroidPluginManager;
		editor.initialize();
		editor.Show();
		return editor;
	}
#endif
}
