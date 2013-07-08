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
	ManifestResource	exist_strings;
	
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
	
	public static string		ManagerVersionName = "0.2";
	
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
			
			if (System.IO.File.Exists(ManifestResource.StringsFilename))
				exist_strings = StringEditor.LoadResourcesFromFile(ManifestResource.StringsFilename);
			else
				exist_strings = new ManifestResource();
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
		if (plugins.VersionName != ManagerVersionName)
		{
			EditorGUILayout.LabelField("Dependencies version missmatch", RedLabel);
			EditorGUILayout.LabelField("Manager version: " + ManagerVersionName);
			EditorGUILayout.LabelField("Dependencies version: " + plugins.VersionName);
			return;
		}
		
		window_scroll = EditorGUILayout.BeginScrollView(window_scroll);
		guiVersions.OnGUI();
		if (guiVersions.EditingVersion != null)
		{
			OnGUI_Version(guiVersions.EditingVersion);
			if (agent != null && guiVersions.EditingVersion.hasPlugin(agent.filename))
			{
				EditorGUILayout.LabelField("Plugin Properties: " + agent.filename, BoldLabel);
				guiProperties.OnGUI(guiVersions.EditingVersion.getVersionOfPlugin(agent.filename)
					, plugins, guiVersions.EditingVersion);
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
		if (GUILayout.Button("Export & Apply"))
		{
			if (guiVersions.EditingVersion != null)
			{
				if (guiVersions.EditingVersion.isVersionReady())
				{
					SaveVersionToFile(GetVersionFilename(guiVersions.EditingVersion), guiVersions.EditingVersion);
					ApplyVersion(guiVersions.EditingVersion);
					this.Close();
				} else
				{
					EditorUtility.DisplayDialog("Error", "Aditional information is needed for this version, please fill all required data", "OK");
				}
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
					{
						if (v.isVersionReady(version))
						{
							if (v.isManifestReady(manifest) && v.isStringsReady(exist_strings)) version_style = GreenLabel;
							else version_style = YellowLabel;
						} else version_style = RedLabel;
					}
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
	string[]		PreDefinitions;
	private void		ApplyVersion(AgentSetVersion v)
	{
		PlayerSettings.bundleVersion = v.versionName;
		PlayerSettings.Android.bundleVersionCode = int.Parse(v.versionCode);
		PlayerSettings.bundleIdentifier = v.bundleid;
		manifest.package = v.bundleid;
		manifest.versionCode = v.versionCode;
		manifest.versionName = v.versionName;
		
		PreDefinitions = v.PreDefine.ToArray();
		SetPreDefinitions();
		
		System.IO.Directory.CreateDirectory(Application.dataPath + "/Plugins/Android/");
		System.IO.Directory.CreateDirectory(Application.dataPath + "/Plugins/Android/res");
		System.IO.Directory.CreateDirectory(Application.dataPath + "/Plugins/Android/libs");
		
		foreach (AgentManifest mans in plugins.Agents)
		{
			if (!v.hasPlugin(mans.filename))
			{
				SetPluginDefinition(SearchPathForFilename(Application.dataPath, mans.filename), false);
				RemoveDependencies(mans);
			} else if (v.getVersionOfPlugin(mans.filename).status == false)
			{
				SetPluginDefinition(SearchPathForFilename(Application.dataPath, mans.filename), false);
				RemoveDependencies(v.getVersionOfPlugin(mans.filename));
			}
		}
		foreach (AgentManifest mans in plugins.Agents)
		{
			if (v.hasPlugin(mans.filename))
			{
				AgentVersion plug = v.getVersionOfPlugin(mans.filename);
				if (plug.status)
				{
					SetPluginDefinition(SearchPathForFilename(Application.dataPath, mans.filename), true);
					AddPluginDependencies(plug);
				}
			}
		}
		AndroidManifestEditor.SaveManifestToFile(Manifest.ManifestFile, manifest);
		ManifestResource current_strings;
		if (System.IO.File.Exists(ManifestResource.StringsFilename))
			current_strings = StringEditor.LoadResourcesFromFile(ManifestResource.StringsFilename);
		else
			current_strings = new ManifestResource();
		for (int i = 0; i < strings.Count; i++)
		{
			if (current_strings.hasName(strings.strings[i].name))
				current_strings.setValue(strings.strings[i].name, strings.strings[i]);
			else
				current_strings.addString(strings.strings[i].name, strings.strings[i]);
		}
		StringEditor.SaveResourcesToFile(ManifestResource.StringsFilename, current_strings);
		Debug.Log("Done :)");
	}
	private void		AddPluginDependencies(AgentVersion plug)
	{
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
		foreach (ManifestMetaData hardmmd in plug.ManifestSource.MetaData)
		{
			if (ManifestResource.isResourceString(hardmmd.value))
			{
				if (!manifest.application.hasMetaData(hardmmd.name))
					manifest.application.addMetaData().name = hardmmd.name;
				manifest.application.getMetaData(hardmmd.name).value = hardmmd.value;
				if (hardmmd.resource != "NOT SET")
					manifest.application.getMetaData(hardmmd.name).resource = hardmmd.resource;
			}
		}
		foreach (ManifestPermission perm in plug.ManifestSource.Permission)
		{
			if (!manifest.hasPermission(perm.name)) manifest.addPermission().name = perm.name;
		}
		for (int i = 0; i < plug.ManifestSource.Strings.Count; i++)
		{
			if (strings.hasName(plug.Strings.strings[i].name))
				strings.setValue(plug.Strings.strings[i].name, plug.Strings.strings[i]);
			else
				strings.addString(plug.Strings.strings[i].name, plug.Strings.strings[i]);
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
		if (mans.hasResources())
		{
			ResFolder resfolder = new ResFolder(mans.ResFolder);
			resfolder.ClearHierarchyFrom(Manifest.ResFolder);
		}
		ResFolder libfolder = new ResFolder(mans.LibraryFolder);
		libfolder.ClearHierarchyFrom(Manifest.LibsFolder);
		foreach (ManifestMetaData mmd in mans.MetaData)
		{
			if (manifest.application.hasMetaData(mmd.name))
				manifest.application.meta_data.Remove(manifest.application.getMetaData(mmd.name));
		}
	}
	private void		RemoveDependencies(AgentVersion plug)
	{
		RemoveDependencies(plug.ManifestSource);
		foreach (ManifestActivity act in plug.ManifestSource.Activity)
		{
			if (manifest.application.hasActivity(act.name))
				manifest.application.activity.Remove(manifest.application.getActivity(act.name));
		}
		foreach (ManifestMetaData mmd in plug.MetaData)
		{
			if (manifest.application.hasMetaData(mmd.name))
				manifest.application.meta_data.Remove(manifest.application.getMetaData(mmd.name));
		}
		for (int i = 0; i < plug.Strings.Count; i++)
		{
			if (strings.hasName(plug.Strings.strings[i].name))
			{
				strings.removeEntry(plug.Strings.strings[i].name);
			}
		}
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
	private void		SetPreDefinitions()
	{
		string[] csharp_files = System.IO.Directory.GetFiles(Application.dataPath, "*.cs", System.IO.SearchOption.AllDirectories);
		foreach (string path in csharp_files) SetPreDefinitionForFilename(path);
	}
	private void		SetPreDefinitionForFilename(string filename)
	{
		List<string> lines = new List<string>(System.IO.File.ReadAllLines(filename));
		if (lines[0].Contains("begin pre definitions"))
		{
			int end_prev_definitions = -1;
			for (int i = 1; i < lines.Count; i++)
			{
				if (lines[i].Contains("end pre definitions")) { end_prev_definitions = i; break; }
			}
			if (end_prev_definitions > 1)
				for (int i = 0; i < end_prev_definitions-1; i++) lines.RemoveAt(1);
			if (end_prev_definitions != -1)
			{
				for (int i = 0; i < PreDefinitions.Length; i++) lines.Insert(1, "#define " + PreDefinitions[i]);
				System.IO.File.WriteAllLines(filename, lines.ToArray());
			}
		}
	}
	private void		SetPluginDefinition(string filename, bool status)
	{
		string[] lines = System.IO.File.ReadAllLines(filename);
		if (lines[0].StartsWith("#define") || lines[0].StartsWith("#undef"))
		{
			if (status)
				lines[0] = "#define	ENABLE_PLUGIN";
			else
				lines[0] = "#undef ENABLE_PLUGIN";
		}
		System.IO.File.SetAttributes(filename, System.IO.FileAttributes.Normal);
		System.IO.File.WriteAllLines(filename, lines);
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
			true, "Android Plugin Manager - version " + ManagerVersionName) as AndroidPluginManager;
		editor.initialize();
		editor.Show();
		return editor;
	}
#endif
}
