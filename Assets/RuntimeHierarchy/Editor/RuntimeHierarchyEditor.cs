using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RHierarchy;

[CustomEditor(typeof(RuntimeHierarchy))]
public class RuntimeHierarchyEditor : Editor
{
	public class DrawData
	{
		/// <param name="type">Data type.</param>
		/// <param name="isStatic">Is static data?</param>
		public DrawData(Type type)
		{
			this.type = type;
			Type currentType = type;
			MemberInfo[] infos = null;
			List<MemberInfo> memberList = new List<MemberInfo>();
			List<string> memberNameList = new List<string>();
			do
			{
				infos = currentType.GetMembers(BindingFlags.Instance
									| BindingFlags.Public
									| BindingFlags.NonPublic
									| BindingFlags.GetProperty
									| BindingFlags.SetProperty
									| BindingFlags.GetField
									| BindingFlags.SetField);

				foreach (var info in infos)
				{
					if (info.MemberType == MemberTypes.Property &&
						(!info.IsDefined(typeof(ObsoleteAttribute), true)
						|| info.Name == "enabled"
						|| info.Name == "isActiveAndEnabled"
						|| info.Name == "name"
						|| info.Name == "tag"))
					{
						memberList.Add(info);
						memberNameList.Add(info.Name);
					}
				}

				if (currentType.BaseType != typeof(System.Object) &&
					currentType.BaseType != typeof(UnityEngine.Object) &&
					currentType.BaseType != typeof(MonoBehaviour) &&
					currentType.BaseType != typeof(Behaviour) &&
					currentType.BaseType != typeof(Component))
					currentType = currentType.BaseType;
				else
					currentType = null;

			}
			while (currentType != null);

			memberInfos = memberList.ToArray();
			memberNames = memberNameList.ToArray();
			indexs = new int[memberNames.Length];
			for (int i = indexs.Length - 1; i >= 0; --i)
			{
				indexs[i] = i;
			}
		}
		/// <summary>
		/// Which type is this object.
		/// </summary>
		public Type type;
		/// <summary>
		/// All property member in this object.
		/// </summary>
		public MemberInfo[] memberInfos = null;

		/// <summary>
		/// All property name in this object.
		/// </summary>
		public string[] memberNames = null;

		/// <summary>
		/// index for gui.
		/// </summary>
		public int[] indexs = null;

		/// <summary>
		/// Returns index of this property name.
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		public int IndexOf(string Name)
		{
			for (int i = memberNames.Length - 1; i >= 0; --i)
			{
				if (memberNames[i] == Name)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Last selected property idnex.
		/// </summary>
		public int lastSelected = 0;
	}

	/// <summary>
	/// current selected target object.
	/// </summary>
	static GameObject obj2Watch;
	/// <summary>
	/// Temp list for get components.
	/// </summary>
	List<Component> components = new List<Component>();
	public override void OnInspectorGUI()
	{
		var hierarchy = (RuntimeHierarchy)target;

		GUILayout.Space(15f);

		EditorGUILayout.BeginHorizontal();
		hierarchy.updateRootRate = EditorGUILayout.FloatField("updateRootRate :", hierarchy.updateRootRate);
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(8f);

		EditorGUILayout.BeginVertical(GUI.skin.box);

		EditorGUILayout.LabelField("======== Property Watch List ========");

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Add Object : ");
		obj2Watch = (GameObject)EditorGUILayout.ObjectField(obj2Watch, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal();
		if (obj2Watch != null)
		{
			Undo.RecordObject(hierarchy, "Add object");
			obj2Watch.GetComponents(components);
			var watch = hierarchy.Get(obj2Watch);
			foreach (var com in components)
			{
				if (com.GetType() == typeof(Transform)) continue;
				watch.Get(com);
			}
			obj2Watch = null;
		}

		for (int i = 0, max = hierarchy.watchDatas.Length; i < max; ++i)
		{
			DrawLine();

			var list = hierarchy.watchDatas[i].components;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(hierarchy.watchDatas[i].targetObject.name);
			if (GUILayout.Button("Remove"))
			{
				Undo.RecordObject(hierarchy, "Remove object");
				hierarchy.RemoveAtWatchData(i);
				break;
			}
			EditorGUILayout.EndHorizontal();
			for (int j = 0, jmax = list.Count; j < jmax; ++j)
			{
				DrawComponent(hierarchy, hierarchy.watchDatas[i], list[j]);
			}
		}

		EditorGUILayout.EndVertical();
	}

	/// <summary>
	/// Dictionary for collect draw data.
	/// </summary>
	static Dictionary<Type, DrawData> map = new Dictionary<Type, DrawData>();

	void DrawComponent(RuntimeHierarchy hierarchy, RuntimeHierarchy.WatchData watch, RuntimeHierarchy.ComponentData com)
	{
		Type type = com.component.GetType();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		EditorGUILayout.LabelField(string.Format("===== {0} =====", type.Name));
		EditorGUILayout.EndHorizontal();

		if (!map.ContainsKey(type))
			map.Add(type, new DrawData(type));

		var drawData = map[type];
		for (int i = 0, max = com.propertyNames.Count; i < max; ++i)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40f);
			EditorGUILayout.LabelField(com.propertyNames[i]);
			if (GUILayout.Button("Remove"))
			{
				com.propertyNames.RemoveAt(i);
				Undo.RecordObject(hierarchy, "Remove property");
				break;
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		EditorGUILayout.LabelField("Property : ", GUILayout.Width(120f));
		drawData.lastSelected = EditorGUILayout.IntPopup(drawData.lastSelected, drawData.memberNames, drawData.indexs);
		if (GUILayout.Button("Add"))
		{
			string propertyName = ((drawData.lastSelected >= 0 && drawData.lastSelected < drawData.memberNames.Length) ? drawData.memberNames[drawData.lastSelected] : string.Empty);
			if (!string.IsNullOrEmpty(propertyName))
			{
				if (!com.propertyNames.Contains(propertyName))
				{
					Undo.RecordObject(hierarchy, "Add property");
					com.propertyNames.Add(propertyName);
				}
				drawData.lastSelected = 0;
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	#region ====== Draw Line ======
	[NonSerialized] static GUIStyle line = null;
	static GUIStyle Line
	{
		get
		{
			if (line == null)
			{
				line = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
				var tex = new Texture2D(2, 2);
				var colours = tex.GetPixels32();
				for (int c = colours.Length - 1; c >= 0; --c)
					colours[c] = new Color32(255, 255, 255, 255);
				tex.SetPixels32(colours);
				line.normal.background = tex;
			}
			if (line.normal.background == null)
			{
				var tex = new Texture2D(2, 2);
				var colours = tex.GetPixels32();
				for (int c = colours.Length - 1; c >= 0; --c)
					colours[c] = new Color32(255, 255, 255, 255);
				tex.SetPixels32(colours);
				line.normal.background = tex;
			}
			return line;
		}
	}

	static Color LineColor
	{
		get { return EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f); }
	}

	/// <summary>
	/// Draw line with height.
	/// </summary>
	public static void DrawLine(float height = 1f)
	{
		Rect position = GUILayoutUtility.GetRect(GUIContent.none, Line, GUILayout.Height(height));

		if (Event.current.type == EventType.Repaint)
		{
			Color oldColor = GUI.color;
			GUI.color = LineColor;
			Line.Draw(position, false, false, false, false);
			GUI.color = oldColor;
		}
	}
	#endregion
}