using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RHierarchy
{
	/// <summary>
	/// Draw attribute can make your property be draw by RuntimeHierarchy.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class DrawAttribute : Attribute
	{ }

	public class RuntimeHierarchy : MonoBehaviour
	{
		/// <summary>
		/// update rate for each time hierarchy refind root objects.
		/// </summary>
		public float updateRootRate = 3f;

		/// <summary>
		/// Add new property into watch list. Return true if added successfully.
		/// </summary>
		public bool AddProperty(GameObject target, Component component, string propertyName)
		{
			WatchData watchData = Get(target);

			ComponentData comData = watchData.Get(component);

			try
			{
				var property = component.GetType().GetProperty(propertyName);
				if (property != null && !comData.propertyNames.Contains(propertyName))
				{
					if (!property.IsDefined(typeof(ObsoleteAttribute), true))
					{
						comData.propertyNames.Add(propertyName);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
			return false;
		}

		/// <summary>
		/// First WindowID for GUI.Window
		/// </summary>
		public int GUIWindowID = 50120;

		/// <summary>
		/// Maximun depth for sub structure.
		/// </summary>
		public int maximumDepth = 5;
		#region ====== Hierarcy ======
		bool fullScreen = true;
		Rect resizeWindowThumbPos;
		Rect windowPos;
		Rect resizePos;
		Rect hideWindowPos;

		ColorViewer colorViewer;
		TextureViewer textureViewer;
		MaterialViewer materialViewer;

		Vector2 scrollViewPos;
		UnityEngine.Object selected = null;
		GameObject[][] roots = null;
		float timer;
		HashSet<int> sceneFolder = new HashSet<int>();
		HashSet<int> folder = new HashSet<int>();

		int destroyInstanceID = 0;

		bool hide = false;

		private void Start()
		{
			Search();
			if (Screen.dpi < 120)
			{
				Utility.smallFieldWidth = 60f;
				Utility.buttonWidth = 150f;
				Utility.buttonHeight = 25f;
			}
			SetWindowPosition();
			TrimWatchData();

			colorViewer = gameObject.GetComponent<ColorViewer>();
			if (colorViewer == null) colorViewer = gameObject.AddComponent<ColorViewer>();
			colorViewer.windowID = GUIWindowID;
			colorViewer.enabled = false;
			textureViewer = gameObject.GetComponent<TextureViewer>();
			if (textureViewer == null) textureViewer = gameObject.AddComponent<TextureViewer>();
			textureViewer.windowID = GUIWindowID;
			textureViewer.enabled = false;
			materialViewer = gameObject.GetComponent<MaterialViewer>();
			if (materialViewer == null) materialViewer = gameObject.AddComponent<MaterialViewer>();
			materialViewer.windowID = GUIWindowID;
			materialViewer.runtimeHierarchy = this;
		}

		void SetWindowPosition()
		{
			float wid = Screen.width / 3f;
			windowPos = new Rect(0f, 0f, wid, Screen.height);
			resizePos = new Rect(wid, 0f, Utility.buttonHeight - 10f, Screen.height);
			inspectorWindowPos = new Rect(windowPos.x + windowPos.width + 15f, 0f,
				(float)Screen.width - wid - 15f, Screen.height);
			hideWindowPos = new Rect(0f, Screen.height - Utility.buttonHeight + 30f, Utility.buttonWidth + 30f, Utility.buttonHeight + 30f);
			resizeWindowThumbPos = new Rect(Screen.width - Utility.buttonHeight, Screen.height - Utility.buttonHeight, Utility.buttonHeight, Utility.buttonHeight);
		}

		void Search()
		{
			if (timer < Time.realtimeSinceStartup)
			{
				timer = Time.realtimeSinceStartup + updateRootRate;
				roots = new GameObject[UnityEngine.SceneManagement.SceneManager.sceneCount][];
				for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
				{
					roots[i] = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects();
				}
			}
		}

		void SetWindowAnchor()
		{
			resizeWindowThumbPos.x = Mathf.Clamp(resizeWindowThumbPos.x, windowPos.x + Utility.buttonWidth * 2, Screen.width - Utility.buttonHeight);
			resizeWindowThumbPos.y = Mathf.Clamp(resizeWindowThumbPos.y, windowPos.y + Utility.buttonWidth * 2, Screen.height - Utility.buttonHeight);
			var winSize = new Rect(0f, 0f, resizeWindowThumbPos.x + resizeWindowThumbPos.width, resizeWindowThumbPos.y + resizeWindowThumbPos.height);
			if (fullScreen)
			{
				winSize = new Rect(0f, 0f, Screen.width, Screen.height);
			}

			if (!fullScreen)
			{
				resizePos.x = Mathf.Clamp(resizePos.x, windowPos.x + Utility.buttonWidth, (resizeWindowThumbPos.x) - Utility.buttonWidth);
				resizePos.height = (resizeWindowThumbPos.y + resizeWindowThumbPos.height) - resizePos.y;
			}
			else
			{
				resizePos.y = 0f;
				resizePos.x = Mathf.Clamp(resizePos.x, Utility.buttonWidth, (resizeWindowThumbPos.x) - Utility.buttonWidth);
			}

			if (!fullScreen)
			{
				windowPos.height = resizePos.height;
				windowPos.width = resizePos.x - windowPos.x;
			}
			else
			{
				windowPos.x = windowPos.y = 0f;
				windowPos.width = resizePos.x;
			}

			inspectorWindowPos.x = resizePos.x + resizePos.width;
			inspectorWindowPos.width = winSize.width - inspectorWindowPos.x;
			if (!fullScreen) inspectorWindowPos.height = resizePos.height;
		}

		private void OnGUI()
		{
			int id = GUIWindowID;
			if (hide)
			{
				hideWindowPos = GUI.Window(id, hideWindowPos, HideWindow, "", Utility.WindowStyle);
				hideWindowPos.x = Mathf.Clamp(hideWindowPos.x, 0, Screen.width - hideWindowPos.width);
				hideWindowPos.y = Mathf.Clamp(hideWindowPos.y, 0, Screen.height - hideWindowPos.height);
			}
			else
			{
				var winPos2 = GUI.Window(id++, windowPos, HierarchyWindow, "Hierarchy", Utility.WindowStyle);
				var resizePos2 = GUI.Window(id++, resizePos, ResizeWindow, "", Utility.ResizeLine);
				resizePos.x = resizePos2.x;
				var inspWinPos2 = GUI.Window(id++, inspectorWindowPos, DrawInspectorWindow, (drawWatch ? "Watch List" : "Inspector"), Utility.WindowStyle);
				if (!fullScreen)
				{
					resizeWindowThumbPos = GUI.Window(id++, resizeWindowThumbPos, ResizeWindow, "", Utility.ResizeLine);
					GUI.BringWindowToFront(id - 1);

					float dx = 0f, dy = 0f;
					if (winPos2 != windowPos)
					{
						dx = winPos2.x - windowPos.x;
						dy = winPos2.y - windowPos.y;
					}
					else if (inspWinPos2 != inspectorWindowPos)
					{
						dx = inspWinPos2.x - inspectorWindowPos.x;
						dy = inspWinPos2.y - inspectorWindowPos.y;
					}
					if (dx != 0f || dy != 0f)
					{
						var dxbound = resizeWindowThumbPos.x + resizeWindowThumbPos.width + dx;
						if (dxbound >= Screen.width)
						{
							dx = Mathf.Clamp(dx - (dxbound - Screen.width), 0f, dx);
						}
						dxbound = windowPos.x + dx;
						if (dxbound <= 0f)
						{
							dx = Mathf.Clamp(dx - dxbound, dx, -dx);
						}
						var dybound = resizeWindowThumbPos.y + resizeWindowThumbPos.height + dy;
						if (dybound >= Screen.height)
						{
							dy = Mathf.Clamp(dy - (dybound - Screen.height), 0f, dy);
						}
						dybound = windowPos.y + dy;
						if (dybound <= 0f)
						{
							dy = Mathf.Clamp(dy - dybound, dy, -dy);
						}
						windowPos.x += dx;
						windowPos.y += dy;
						resizePos.x += dx;
						resizePos.y += dy;
						inspectorWindowPos.y += dy;
						resizeWindowThumbPos.x += dx;
						resizeWindowThumbPos.y += dy;
					}
				}
				SetWindowAnchor();
			}
			int lastID = id;
			int focusedID = id;

			if (materialViewer.enabled)
				materialViewer.windowID = focusedID = ++id;
			if (colorViewer.enabled)
				colorViewer.windowID = focusedID = ++id;
			if (textureViewer.enabled)
				textureViewer.windowID = focusedID = ++id;
			if (addComponentTarget != null)
				addComponentWindowPos = GUI.Window(focusedID = ++id, addComponentWindowPos, AddComponentWindow, "Add Component", Utility.DarkWindowStyle);
			if (lastID != focusedID)
				GUI.BringWindowToFront(focusedID);
		}

		void ResizeWindow(int id)
		{
			GUI.DragWindow();
		}

		void HideWindow(int id)
		{
			if (GUI.Button(new Rect(15f, 20f, Utility.buttonWidth, Utility.buttonHeight), "Show", Utility.ButtonStyle))
			{
				hide = !hide;
			}
			GUI.DragWindow();
		}

		void HierarchyWindow(int id)
		{
			Search();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(fullScreen ? "Window Mode" : "Full Screen", Utility.ButtonStyle, GUILayout.Width(100f)))
			{
				fullScreen = !fullScreen;
				SetWindowPosition();
			}
			if (GUILayout.Button("Reset Window Position", Utility.ButtonStyle))
			{
				SetWindowPosition();
			}
			GUILayout.EndHorizontal();

			float height = windowPos.height - Utility.buttonHeight * 2 - 45f;
			if (watchDatas != null && watchDatas.Length > 0)
				height -= (Utility.buttonHeight + 2.0f);

			scrollViewPos = Utility.BeginScrollView(scrollViewPos, GUILayout.Height(height));

			for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				int buildInd = scene.buildIndex;
				bool before = sceneFolder.Contains(buildInd);
				string mark = before ? "\u25BC " : "\u25BA ";
				string sceneName = scene.name;
				if (string.IsNullOrEmpty(sceneName)) sceneName = mark + " Scene : Untitled";
				else sceneName = string.Format("{0} Scene : {1}", mark, sceneName);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(sceneName, Utility.ButtonStyle, GUILayout.MinWidth(150f)))
				{
					if (!before) sceneFolder.Add(buildInd);
					else sceneFolder.Remove(buildInd);
				}

				if (GUILayout.Button("New Game Object", Utility.ButtonStyle, GUILayout.MinWidth(120f)))
				{
					GameObject go = new GameObject();
					UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
					timer = 0f;
				}
				GUILayout.EndHorizontal();
				if (sceneFolder.Contains(buildInd))
				{
					foreach (var obj in roots[i])
					{
						if (obj != null && obj.transform != null)
							DrawTransform(obj.transform, 1, obj.activeSelf);
					}
				}
			}

			GUILayout.EndScrollView();

			if (watchDatas != null && watchDatas.Length > 0)
				drawWatch = GUI.Toggle(new Rect(15f, windowPos.height - 15f - Utility.buttonHeight - Utility.buttonHeight - 2f, windowPos.width - 30f, Utility.buttonHeight), drawWatch, "Watch List", Utility.ButtonStyle);
			if (GUI.Button(new Rect(15f, windowPos.height - 15f - Utility.buttonHeight, windowPos.width - 30f, Utility.buttonHeight), "Hide", Utility.ButtonStyle))
			{
				hide = !hide;
			}

			if (!fullScreen) GUI.DragWindow();
		}

		void DrawTransform(Transform trans, int depth, bool rootInactivated)
		{
			if (trans == null) return;

			GUILayout.BeginHorizontal();
			GUILayout.Space(depth * 15f);

			int instanceID = trans.GetInstanceID();
			bool before = folder.Contains(instanceID);
			string mark = before ? "\u25BC " : "\u25BA ";
			bool hasChild = trans.childCount > 0;
			if (!hasChild) mark = "";

			bool active = rootInactivated && trans.gameObject.activeSelf;
			bool isSelected = selected == trans.gameObject;
			GUIStyle buttonStyle = isSelected ? (active ? Utility.SelectedButtonStyle : Utility.InactivatedSelectedButtonStyle) : (active ? Utility.ButtonStyle : Utility.InactivatedButtonStyle);
			if (destroyInstanceID == trans.gameObject.GetInstanceID())
			{
				GUILayout.Label("Are you sure?", Utility.YelloLabelStyle, GUILayout.MinWidth(150f));
				if (GUILayout.Button("Yes", Utility.ButtonStyle))
				{
					destroyInstanceID = 0;
					Destroy(trans.gameObject);
				}
				else if (GUILayout.Button("No", Utility.ButtonStyle))
				{
					destroyInstanceID = 0;
				}
			}
			else
			{
				if (GUILayout.Button(mark + trans.name, buttonStyle, GUILayout.MinWidth(150f)))
				{
					if (hasChild)
					{
						if (!before) folder.Add(instanceID);
						else folder.Remove(instanceID);
					}
					if (!lockTarget)
					{
						if (selected != trans.gameObject)
						{
							selected = trans.gameObject;
							drawWatch = false;
							listDatas.Clear();
							dictDatas.Clear();
							tempRemoveList.Clear();
							addComponentTarget = null;
							materialViewer.enabled = false;
							colorViewer.enabled = false;
							textureViewer.enabled = false;
						}
					}
				}

				if (GUILayout.Button("Destroy", Utility.ButtonStyle, GUILayout.MinWidth(60f)))
				{
					destroyInstanceID = trans.gameObject.GetInstanceID();
				}
			}

			GUILayout.EndHorizontal();
			if (folder.Contains(instanceID))
			{
				for (int i = 0; i < trans.childCount; ++i)
				{
					DrawTransform(trans.GetChild(i), depth + 1, rootInactivated);
				}
			}
		}
		#endregion
		#region ====== Watch ======
		bool drawWatch = false;

		/// <summary>
		/// Helper class for record watch data.
		/// </summary>
		[Serializable]
		public class ComponentData
		{
			public Component component;
			public List<string> propertyNames = new List<string>();
		}

		/// <summary>
		/// Helper class for record watch data.
		/// </summary>
		[Serializable]
		public class WatchData
		{
			public GameObject targetObject = null;
			public List<ComponentData> components = new List<ComponentData>();

			public bool HasComponent(Component com)
			{
				foreach (var type in components)
				{
					if (type.component == com)
					{
						return true;
					}
				}
				return false;
			}

			public ComponentData Get(Component com)
			{
				foreach (var type in components)
				{
					if (type.component == com)
					{
						return type;
					}
				}

				var data = new ComponentData() { component = com };
				components.Add(data);

				return data;
			}

			public void Trim()
			{
				for (int i = components.Count - 1; i >= 0; --i)
				{
					if (components[i].propertyNames.Count == 0)
						components.RemoveAt(i);
					else
					{
						for (int j = components[i].propertyNames.Count - 1; j >= 0; --j)
						{
							if (string.IsNullOrEmpty(components[i].propertyNames[j]))
								components[i].propertyNames.RemoveAt(j);
						}
					}
				}
			}
		}

		/// <summary>
		/// Recorded watch data.
		/// </summary>
		public WatchData[] watchDatas = new WatchData[0];

		/// <summary>
		/// Get watch data from GameObject.
		/// </summary>
		public WatchData Get(GameObject target)
		{
			WatchData watchData = null;

			if (watchDatas == null) watchDatas = new WatchData[0];

			foreach (var data in watchDatas)
			{
				if (data.targetObject == target)
				{
					watchData = data;
				}
			}

			if (watchData == null)
			{
				watchData = new WatchData() { targetObject = target };
				watchDatas = (WatchData[])Insert(watchDatas, watchData, watchDatas.Length);
			}

			return watchData;
		}

		/// <summary>
		/// Remove watch data at index.
		/// </summary>
		public void RemoveAtWatchData(int index)
		{
			watchDatas = (WatchData[])RemoveAt(watchDatas, index);
		}

		/// <summary>
		/// Trim watch data array.
		/// </summary>
		void TrimWatchData()
		{
			HashSet<int> list = new HashSet<int>();
			for (int i = watchDatas.Length - 1; i >= 0; --i)
			{
				watchDatas[i].Trim();

				if (watchDatas[i].targetObject == null || watchDatas[i].components.Count == 0)
					list.Add(i);
			}
			watchDatas = (WatchData[])RemoveFrom(watchDatas, list);
		}
		#endregion
		#region ====== Inspector ======
		Vector2 insScrollPos;
		Vector2 watchScrollPos;
		Vector2 staticScrollPos;
		bool lockTarget;
		HashSet<Type> showHideComponents = new HashSet<Type>();
		HashSet<Type> showHideStaticComponents = new HashSet<Type>();
		HashSet<int> showHideComponentMethods = new HashSet<int>();
		readonly List<Component> components = new List<Component>();
		Rect inspectorWindowPos;
		Type staticType = null;
		string staticTypeStr = string.Empty;
		string addStaticStr = string.Empty;
		List<string> inspect_static_property = new List<string>();

		void DrawInspectorWindow(int id)
		{
			DrawInspector(selected);
			GUI.DragWindow();
		}

		void DrawInspector(UnityEngine.Object target)
		{
			insScrollPos = Utility.BeginScrollView(insScrollPos);

			if (drawWatch)
			{
				watchScrollPos = Utility.BeginScrollView(watchScrollPos, GUILayout.Height(Screen.height * 0.5f));
				for (int i = 0, max = watchDatas.Length; i < max; ++i)
				{
					var goName = watchDatas[i].targetObject.name;
					var coms = watchDatas[i].components;
					for (int j = 0, jmax = coms.Count; j < jmax; ++j)
					{
						var comp = coms[j];

						Type comType = comp.component.GetType();

						for (int k = 0, kmax = comp.propertyNames.Count; k < kmax; ++k)
						{
							try
							{
								PropertyInfo property = comType.GetProperty(comp.propertyNames[k]);
								var value = property.GetValue(comp.component, null);
								string propertyName = string.Format("{0}.{1}.{2}", goName, comType.Name, comp.propertyNames[k]);
								var result = DrawPropertyOrField(propertyName, property.PropertyType, value, comType, 0, -1, 200f);
								if (property.CanWrite && result != null)
								{
									if (result is string)
										property.SetValue(comp.component, System.ComponentModel.TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(result.ToString()), null);
									else
										property.SetValue(comp.component, result, null);
								}
							}
							catch (Exception ex)
							{
								if (Utility.DebugFlag) Debug.LogError(string.Format("Type:{0} property:{1} throw errores:{2}", comType.Name, comp.propertyNames[k], ex));
							}
						}
					}
				}
				GUILayout.EndScrollView();
				Utility.DrawLine();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Static Inspector", Utility.YelloLabelStyle);
				GUILayout.EndHorizontal();
				staticScrollPos = Utility.BeginScrollView(staticScrollPos);
				if (staticType == null)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Static Class Name : ", Utility.LabelStyle);
					staticTypeStr = GUILayout.TextField(staticTypeStr, Utility.TextFieldStyle);
					GUILayout.EndHorizontal();
					if (GUILayout.Button("Inspect", Utility.ButtonStyle))
					{
						if (!string.IsNullOrEmpty(staticTypeStr))
						{
							staticType = GetTypeFrom(staticTypeStr, Assembly.GetExecutingAssembly().GetTypes(), false);
							try
							{
								if (staticType == null)
								{
									var assembly = Assembly.GetAssembly(typeof(UnityEngine.Object));
									staticType = GetTypeFrom(staticTypeStr, assembly.GetTypes(), false);
								}
#if UNITY_EDITOR
								if (staticType == null)
								{
									var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
									staticType = GetTypeFrom(staticTypeStr, assembly.GetTypes(), false);
								}
#endif
							}
							catch (Exception e)
							{
								Debug.LogWarning(e);
							}
							if (staticType == null)
								Debug.LogWarning("Cannot find this type.");
						}
					}
				}
				else
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(staticType.FullName, Utility.YelloLabelStyle);
					if (GUILayout.Button("Inspect other", Utility.ButtonStyle))
					{
						staticType = null;
						inspect_static_property.Clear();
						return;
					}
					GUILayout.EndHorizontal();

					Utility.DrawData data;
					DrawObject(staticType, null, staticType, out data, 0);

					GUILayout.BeginHorizontal();
					GUILayout.Label("Add property name : ", Utility.LabelStyle);
					addStaticStr = GUILayout.TextField(addStaticStr, Utility.TextFieldStyle);
					if (GUILayout.Button("Add", Utility.ButtonStyle, GUILayout.Width(120f)))
					{
						if (!inspect_static_property.Contains(addStaticStr))
						{
							var info = staticType.GetProperty(addStaticStr);
							if (info != null && info.CanRead)
								inspect_static_property.Add(addStaticStr);
							else Debug.LogWarning("Cannot find : " + addStaticStr);
						}
						addStaticStr = string.Empty;
					}
					GUILayout.EndHorizontal();
					int removeIndex = -1;
					for (int i = 0, max = inspect_static_property.Count; i < max; ++i)
					{
						var info = staticType.GetProperty(inspect_static_property[i]);
						var val = info.GetValue(null, null);
						GUILayout.BeginHorizontal();
						val = DrawPropertyOrField(inspect_static_property[i], info.PropertyType, val, staticType, 0);
						if (info.CanWrite)
							info.SetValue(null, val, null);

						if (GUILayout.Button("Remove", Utility.ButtonStyle, GUILayout.Width(120f)))
						{
							removeIndex = i;
							break;
						}
						GUILayout.EndHorizontal();
					}
					if (removeIndex != -1)
					{
						inspect_static_property.RemoveAt(removeIndex);
					}

					DrawMethods(null, data, 0);
				}
				GUILayout.EndScrollView();
			}
			else if (target == null)
			{
				GUILayout.Label("no object selected", Utility.LabelStyle);
			}
			else
			{
				GUILayout.BeginHorizontal();

				GameObject go = target as GameObject;
				if (go != null)
				{
					GUILayout.Space(8f);
					if (go.activeSelf != GUILayout.Toggle(go.activeSelf, "", Utility.ToggleStyle, GUILayout.MaxWidth(20f)))
						go.SetActive(!go.activeSelf);
					target.name = GUILayout.TextField(target.name, Utility.TextFieldStyle);
					lockTarget = GUILayout.Toggle(lockTarget, "Lock Inspector", Utility.ToggleStyle, GUILayout.MaxWidth(105f));

					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();

					GUILayout.Label("Tag : " + go.tag, Utility.LabelStyle);
					GUILayout.Label("Layer : ", Utility.LabelStyle);
					int layer = Utility.DrawLayerPopup(go.layer);
					if (layer != go.layer)
						go.layer = layer;
				}
				else
					target.name = GUILayout.TextField(target.name, Utility.TextFieldStyle);

				GUILayout.EndHorizontal();

				Utility.DrawLine();

				if (go != null)
				{
					go.GetComponents(components);
					foreach (var component in components)
					{
						if (DrawComponent(component))
						{
							Destroy(component);
							break;
						}
						Utility.DrawLine();
					}
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Add Component", Utility.ButtonStyle, GUILayout.MinWidth(200f)))
					{
						addComponentTarget = go;
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndScrollView();
		}

		bool DrawComponent(UnityEngine.Object component)
		{
			Type componentType = component.GetType();
			bool remove = false;
			bool before = !showHideComponents.Contains(componentType);
			string mark = before ? "\u25BC" : "\u25BA";

			GUILayout.BeginHorizontal();

			GUILayout.Label(mark, Utility.LabelStyle, GUILayout.Width(20f));

			var enabledProperty = component.GetType().GetProperty("enabled");
			if (enabledProperty != null)
				enabledProperty.SetValue(component, GUILayout.Toggle((bool)enabledProperty.GetValue(component, null), "", Utility.ToggleStyle, GUILayout.Width(20f)), null);

			bool result = GUILayout.Toggle(before, component.GetType().Name, Utility.YelloLabelStyle);
			if (before != result)
			{
				if (result) showHideComponents.Remove(componentType);
				else showHideComponents.Add(componentType);
			}

			GUILayout.EndHorizontal();

			string comType = componentType.FullName;

			if (!showHideComponents.Contains(componentType))
			{
				bool removable = true;
				if (typeof(Transform) == componentType)
				{
					DrawTransformComponent(component as Transform);
					removable = false;
				}
				else if (typeof(RectTransform) == componentType)
				{
					DrawRectTransformComponent(component as RectTransform);
					removable = false;
				}
				else if (typeof(Collider).IsAssignableFrom(componentType))
				{
					DrawColliderComponent(component as Collider);
				}
				else if (typeof(Collider2D).IsAssignableFrom(componentType))
				{
					DrawCollider2DComponent(component as Collider2D);
				}
				else if (typeof(Renderer).IsAssignableFrom(componentType))
				{
					DrawRendererComponent(component as Renderer);
				}
				else if (typeof(Rigidbody).IsAssignableFrom(componentType))
				{
					DrawRigidbodyComponent(component as Rigidbody);
				}
				else if (typeof(Rigidbody2D).IsAssignableFrom(componentType))
				{
					DrawRigidbody2DComponent(component as Rigidbody2D);
				}
				else if (typeof(Camera) == componentType)
				{
					DrawCameraComponent(component as Camera);
				}
				else if (typeof(Light) == componentType)
				{
					DrawLightComponent(component as Light);
				}
				else if (typeof(UnityEngine.Video.VideoPlayer) == componentType)
				{
					DrawVideoPlayerComponent(component as UnityEngine.Video.VideoPlayer);
				}
				else if (typeof(AudioSource) == componentType)
				{
					DrawAudioSource(component as AudioSource);
				}
				//else if (typeof(ParticleSystem) == componentType)
				//{
				//	GUILayout.Label("ParticleSystem is not supported.", Utility.LabelStyle);
				//}
				else
				{
					Utility.DrawData data;
					DrawObject(componentType, component, componentType, out data, 0);
					DrawMethods(component, data, 0);

					// Static part
					remove = false;
					before = showHideStaticComponents.Contains(componentType);
					mark = before ? "\u25BC" : "\u25BA";

					GUILayout.BeginVertical(Utility.BoxStyle);
					GUILayout.BeginHorizontal();

					GUILayout.Label(mark, Utility.LabelStyle, GUILayout.Width(20f));

					result = GUILayout.Toggle(before, "Static datas", Utility.YelloLabelStyle);
					if (before != result)
					{
						if (result) showHideStaticComponents.Add(componentType);
						else showHideStaticComponents.Remove(componentType);
					}

					GUILayout.EndHorizontal();

					if (showHideStaticComponents.Contains(componentType))
					{
						Utility.DrawData data2;
						DrawObject(componentType, null, componentType, out data2, 0);
					}
					GUILayout.EndVertical();
				}

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (removable && GUILayout.Button("Remove Component", Utility.ButtonStyle, GUILayout.MinWidth(200f)))
				{
					remove = true;
				}
				GUILayout.EndHorizontal();
			}
			return remove;
		}
		#endregion
		#region ====== Add Component Window ======
		Rect addComponentWindowPos = new Rect(15f, 15f, 500f, 200f);
		GameObject addComponentTarget = null;
		string addComponentStr = string.Empty;

		Type GetTypeFrom(string typeName, Type[] types, bool checkComponent = true)
		{
			foreach (var allCustomType in types)
			{
				if ((!checkComponent || typeof(Component).IsAssignableFrom(allCustomType)) &&
					!allCustomType.IsAbstract &&
					allCustomType.Name == typeName)
				{
					return allCustomType;
				}
			}
			return null;
		}

		void AddComponentWindow(int id)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Component Name : ", Utility.LabelStyle);
			addComponentStr = GUILayout.TextField(addComponentStr, Utility.TextFieldStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Add Component", Utility.ButtonStyle) &&
				!string.IsNullOrEmpty(addComponentStr))
			{
				Type type = GetTypeFrom(addComponentStr, Assembly.GetExecutingAssembly().GetTypes());
				try
				{
					if (type == null)
					{
						var assembly = Assembly.GetAssembly(typeof(UnityEngine.Object));
						type = GetTypeFrom(addComponentStr, assembly.GetTypes());
					}
#if UNITY_EDITOR
					if (type == null)
					{
						var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
						type = GetTypeFrom(addComponentStr, assembly.GetTypes());
					}
#endif
				}
				catch (Exception e)
				{
					Debug.LogWarning(e);
				}
				if (type == null || !typeof(Component).IsAssignableFrom(type))
					Debug.LogWarning("Cannot find this component.");
				else
				{
					try
					{
						addComponentTarget.AddComponent(type);
					}
					catch (System.Exception e)
					{
						Debug.LogWarning(e);
					}
				}
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Close", Utility.ButtonStyle))
			{
				addComponentTarget = null;
			}

			GUI.DragWindow();
		}
		#endregion
		#region ====== Helper Methods ======
		const float fieldMinWidth = 300f;
		[NonSerialized] Dictionary<int, Utility.ListData> listDatas = new Dictionary<int, Utility.ListData>();
		[NonSerialized] Dictionary<int, Utility.DictionaryData> dictDatas = new Dictionary<int, Utility.DictionaryData>();
		static HashSet<object> tempRemoveList = new HashSet<object>();

		#region ===== Draw Unity standard component =====
		/// <summary>
		/// Draw transform inspector.
		/// </summary>
		void DrawTransformComponent(Transform com)
		{
			if (com != null)
			{
				// position
				GUILayout.BeginHorizontal();
				bool reset = GUILayout.Button("Position", Utility.ButtonStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.localPosition = Utility.DrawVector3(com.localPosition);
				if (reset) com.localPosition = Vector3.zero;
				GUILayout.EndHorizontal();
				// rotation
				GUILayout.BeginHorizontal();
				reset = GUILayout.Button("Rotation", Utility.ButtonStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.localEulerAngles = Utility.DrawVector3(com.localEulerAngles);
				if (reset) com.localRotation = Quaternion.identity;
				GUILayout.EndHorizontal();
				// scale
				GUILayout.BeginHorizontal();
				reset = GUILayout.Button("Scale", Utility.ButtonStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.localScale = Utility.DrawVector3(com.localScale);
				if (reset) com.localScale = Vector3.one;
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw rect transform inspector.
		/// </summary>
		void DrawRectTransformComponent(RectTransform com)
		{
			if (com != null)
			{
				// position
				GUILayout.BeginHorizontal();
				bool reset = GUILayout.Button("Position", Utility.ButtonStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.localPosition = Utility.DrawVector3(com.localPosition);
				if (reset) com.localPosition = Vector3.zero;
				GUILayout.EndHorizontal();
				// rect size
				GUILayout.BeginHorizontal();
				var size = com.sizeDelta;
				GUILayout.Label("Width", Utility.LabelStyle, GUILayout.Width(Utility.smallFieldWidth));
				size.x = Utility.FloatField(size.x);
				GUILayout.Label("Height", Utility.LabelStyle, GUILayout.Width(Utility.smallFieldWidth));
				size.y = Utility.FloatField(size.y);
				com.sizeDelta = size;
				GUILayout.EndHorizontal();
				// anchors
				GUILayout.BeginHorizontal();
				GUILayout.Label("Anchors", Utility.LabelStyle, GUILayout.Width(Utility.smallFieldWidth));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Min", Utility.LabelStyle);
				var min = com.anchorMin;
				GUILayout.Label("X", Utility.LabelStyle);
				min.x = Utility.FloatField(min.x);
				GUILayout.Label("Y", Utility.LabelStyle);
				min.y = Utility.FloatField(min.y);
				com.anchorMin = min;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Max", Utility.LabelStyle);
				var max = com.anchorMax;
				GUILayout.Label("X", Utility.LabelStyle);
				max.x = Utility.FloatField(max.x);
				GUILayout.Label("Y", Utility.LabelStyle);
				max.y = Utility.FloatField(max.y);
				com.anchorMax = max;
				GUILayout.EndHorizontal();
				// pivot
				GUILayout.BeginHorizontal();
				GUILayout.Label("Pivot", Utility.LabelStyle, GUILayout.Width(Utility.smallFieldWidth));
				var pivot = com.pivot;
				GUILayout.Label("X", Utility.LabelStyle);
				pivot.x = Utility.FloatField(pivot.x);
				GUILayout.Label("Y", Utility.LabelStyle);
				pivot.y = Utility.FloatField(pivot.y);
				com.pivot = pivot;
				GUILayout.EndHorizontal();
				// rotation
				GUILayout.BeginHorizontal();
				reset = GUILayout.Button("Rotation", Utility.ButtonStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.localRotation = Utility.DrawQuaternion(com.localRotation);
				if (reset) com.localRotation = Quaternion.identity;
				GUILayout.EndHorizontal();
				// scale
				GUILayout.BeginHorizontal();
				reset = GUILayout.Button("Scale", Utility.ButtonStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.localScale = Utility.DrawVector3(com.localScale);
				if (reset) com.localScale = Vector3.one;
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw Collider inspector.
		/// </summary>
		void DrawColliderComponent(Collider com)
		{
			if (com != null)
			{
				CharacterController character = com as CharacterController;
				if (character)
				{
					// Slope limit
					GUILayout.BeginHorizontal();
					GUILayout.Label("Slope Limit", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					character.slopeLimit = Utility.FloatField(character.slopeLimit);
					GUILayout.EndHorizontal();
					// Step offset
					GUILayout.BeginHorizontal();
					GUILayout.Label("Step Offset", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					character.stepOffset = Utility.FloatField(character.stepOffset);
					GUILayout.EndHorizontal();
					// Skin width
					GUILayout.BeginHorizontal();
					GUILayout.Label("Skin Width", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					character.skinWidth = Utility.FloatField(character.skinWidth);
					GUILayout.EndHorizontal();
					// Min move distance
					GUILayout.BeginHorizontal();
					GUILayout.Label("Min Move Distance", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					character.minMoveDistance = Utility.FloatField(character.minMoveDistance);
					GUILayout.EndHorizontal();
					// Center
					GUILayout.BeginHorizontal();
					GUILayout.Label("Center", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					character.center = Utility.DrawVector3(character.center);
					GUILayout.EndHorizontal();
					// Radius
					GUILayout.BeginHorizontal();
					GUILayout.Label("Radius", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					character.radius = Utility.FloatField(character.radius);
					GUILayout.EndHorizontal();
					// Height
					GUILayout.BeginHorizontal();
					GUILayout.Label("Height", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					character.height = Utility.FloatField(character.height);
					GUILayout.EndHorizontal();
				}
				else
				{
					// Trigger
					GUILayout.BeginHorizontal();
					var trigger = GUILayout.Toggle(com.isTrigger, "Is Trigger", Utility.ToggleStyle);
					if (trigger != com.isTrigger)
					{
						com.isTrigger = trigger;
					}
					GUILayout.EndHorizontal();
					// Physic Material
					GUILayout.BeginHorizontal();
					GUILayout.Label("Physic Material", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					Utility.DrawObjectField(com.sharedMaterial);
					GUILayout.EndHorizontal();

					BoxCollider box = com as BoxCollider;
					if (box)
					{
						// Center
						GUILayout.BeginHorizontal();
						GUILayout.Label("Center", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						box.center = Utility.DrawVector3(box.center);
						GUILayout.EndHorizontal();
						// Size
						GUILayout.BeginHorizontal();
						GUILayout.Label("Size", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						box.size = Utility.DrawVector3(box.size);
						GUILayout.EndHorizontal();
					}
					else
					{
						SphereCollider sphere = com as SphereCollider;
						if (sphere)
						{
							// Center
							GUILayout.BeginHorizontal();
							GUILayout.Label("Center", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
							sphere.center = Utility.DrawVector3(sphere.center);
							GUILayout.EndHorizontal();
							// Radius
							GUILayout.BeginHorizontal();
							GUILayout.Label("Radius", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
							sphere.radius = Utility.FloatField(sphere.radius);
							GUILayout.EndHorizontal();
						}
						else
						{
							CapsuleCollider capsule = com as CapsuleCollider;
							if (capsule)
							{
								// Center
								GUILayout.BeginHorizontal();
								GUILayout.Label("Center", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								capsule.center = Utility.DrawVector3(capsule.center);
								GUILayout.EndHorizontal();
								// Radius
								GUILayout.BeginHorizontal();
								GUILayout.Label("Radius", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								capsule.radius = Utility.FloatField(capsule.radius);
								GUILayout.EndHorizontal();
								// Height
								GUILayout.BeginHorizontal();
								GUILayout.Label("Height", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								capsule.height = Utility.FloatField(capsule.height);
								GUILayout.EndHorizontal();
								// Direction
								GUILayout.BeginHorizontal();
								GUILayout.Label("Direction", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								capsule.direction = Utility.IntField(capsule.direction);
								GUILayout.EndHorizontal();
							}
							else
							{
								MeshCollider mesh = com as MeshCollider;
								if (mesh)
								{
									// Mesh
									GUILayout.BeginHorizontal();
									GUILayout.Label("Mesh", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
									Utility.DrawObjectField(mesh.sharedMesh);
									GUILayout.EndHorizontal();
								}
								// Bounds
								GUILayout.BeginHorizontal();
								GUILayout.Label("Bounds", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								Utility.DrawObjectField(com.bounds);
								GUILayout.EndHorizontal();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Draw Collider2D inspector.
		/// </summary>
		void DrawCollider2DComponent(Collider2D com)
		{
			if (com != null)
			{
				// Physic Material
				GUILayout.BeginHorizontal();
				GUILayout.Label("Physic Material", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				Utility.DrawObjectField(com.sharedMaterial);
				GUILayout.EndHorizontal();
				// trigger
				GUILayout.BeginHorizontal();
				var trigger = GUILayout.Toggle(com.isTrigger, "Is Trigger", Utility.ToggleStyle);
				if (trigger != com.isTrigger)
				{
					com.isTrigger = trigger;
				}
				GUILayout.EndHorizontal();
				// Use by effector
				GUILayout.BeginHorizontal();
				var useByEffector = GUILayout.Toggle(com.usedByEffector, "Use By Effector", Utility.ToggleStyle);
				if (useByEffector != com.usedByEffector)
				{
					com.usedByEffector = useByEffector;
				}
				GUILayout.EndHorizontal();

				BoxCollider2D box = com as BoxCollider2D;
				if (box)
				{
					// Use by composite
					GUILayout.BeginHorizontal();
					var useByComposite = GUILayout.Toggle(box.usedByComposite, "Use By Composite", Utility.ToggleStyle);
					if (useByComposite != box.usedByComposite)
					{
						box.usedByComposite = useByComposite;
					}
					GUILayout.EndHorizontal();
					// Auto tiling
					GUILayout.BeginHorizontal();
					var autoTiling = GUILayout.Toggle(box.autoTiling, "Auto Tiling", Utility.ToggleStyle);
					if (autoTiling != box.autoTiling)
					{
						box.autoTiling = autoTiling;
					}
					GUILayout.EndHorizontal();
					// Offset
					GUILayout.BeginHorizontal();
					GUILayout.Label("Offset", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					box.offset = Utility.DrawVector2(box.offset);
					GUILayout.EndHorizontal();
					// Size
					GUILayout.BeginHorizontal();
					GUILayout.Label("Size", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					box.size = Utility.DrawVector2(box.size);
					GUILayout.EndHorizontal();
					// Edge radius
					GUILayout.BeginHorizontal();
					GUILayout.Label("Edge Radius", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					box.edgeRadius = Utility.FloatField(box.edgeRadius);
					GUILayout.EndHorizontal();
				}
				else
				{
					CircleCollider2D circle = com as CircleCollider2D;
					if (circle)
					{
						// Offset
						GUILayout.BeginHorizontal();
						GUILayout.Label("Offset", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						circle.offset = Utility.DrawVector2(circle.offset);
						GUILayout.EndHorizontal();
						// Radius
						GUILayout.BeginHorizontal();
						GUILayout.Label("Radius", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						circle.radius = Utility.FloatField(circle.radius);
						GUILayout.EndHorizontal();
					}
					else
					{
						CapsuleCollider2D capsule = com as CapsuleCollider2D;
						if (capsule)
						{
							// Offset
							GUILayout.BeginHorizontal();
							GUILayout.Label("Offset", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
							capsule.offset = Utility.DrawVector2(capsule.offset);
							GUILayout.EndHorizontal();
							// Size
							GUILayout.BeginHorizontal();
							GUILayout.Label("Size", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
							capsule.size = Utility.DrawVector2(capsule.size);
							GUILayout.EndHorizontal();
							// Direction
							GUILayout.BeginHorizontal();
							GUILayout.Label("Direction", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
							capsule.direction = (CapsuleDirection2D)Utility.DrawPopup((int)capsule.direction, typeof(CapsuleDirection2D));
							GUILayout.EndHorizontal();
						}
						else
						{
							CompositeCollider2D composite = com as CompositeCollider2D;
							if (composite)
							{
								// Offset
								GUILayout.BeginHorizontal();
								GUILayout.Label("Offset", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								composite.offset = Utility.DrawVector2(composite.offset);
								GUILayout.EndHorizontal();
								// Geometry type
								GUILayout.BeginHorizontal();
								GUILayout.Label("Geometry Type", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								composite.geometryType = (CompositeCollider2D.GeometryType)Utility.DrawPopup((int)composite.geometryType, typeof(CompositeCollider2D.GeometryType));
								GUILayout.EndHorizontal();
								// Generation type
								GUILayout.BeginHorizontal();
								GUILayout.Label("Generation Type", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								composite.generationType = (CompositeCollider2D.GenerationType)Utility.DrawPopup((int)composite.generationType, typeof(CompositeCollider2D.GenerationType));
								GUILayout.EndHorizontal();
								// Vertex distance
								GUILayout.BeginHorizontal();
								GUILayout.Label("Vertex Distance", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								composite.vertexDistance = Utility.FloatField(composite.vertexDistance);
								GUILayout.EndHorizontal();
								// Edge radius
								GUILayout.BeginHorizontal();
								GUILayout.Label("Edge Radius", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
								composite.edgeRadius = Utility.FloatField(composite.edgeRadius);
								GUILayout.EndHorizontal();
							}
							else
							{
								EdgeCollider2D edge = com as EdgeCollider2D;
								if (edge)
								{
									// Offset
									GUILayout.BeginHorizontal();
									GUILayout.Label("Offset", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
									edge.offset = Utility.DrawVector2(edge.offset);
									GUILayout.EndHorizontal();
									// Edge radius
									GUILayout.BeginHorizontal();
									GUILayout.Label("Edge Radius", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
									edge.edgeRadius = Utility.FloatField(edge.edgeRadius);
									GUILayout.EndHorizontal();
								}
								else
								{
									PolygonCollider2D polygon = com as PolygonCollider2D;
									if (polygon)
									{
										// Use by composite
										GUILayout.BeginHorizontal();
										var useByComposite = GUILayout.Toggle(polygon.usedByComposite, "Use By Composite", Utility.ToggleStyle);
										if (useByComposite != polygon.usedByComposite)
										{
											polygon.usedByComposite = useByComposite;
										}
										GUILayout.EndHorizontal();
										// Auto tiling
										GUILayout.BeginHorizontal();
										var autoTiling = GUILayout.Toggle(polygon.autoTiling, "Auto Tiling", Utility.ToggleStyle);
										if (autoTiling != polygon.autoTiling)
										{
											polygon.autoTiling = autoTiling;
										}
										GUILayout.EndHorizontal();
										// Offset
										GUILayout.BeginHorizontal();
										GUILayout.Label("Offset", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
										polygon.offset = Utility.DrawVector2(polygon.offset);
										GUILayout.EndHorizontal();
									}
									else
									{
										// Bounds
										GUILayout.BeginHorizontal();
										GUILayout.Label("Bounds", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
										Utility.DrawObjectField(com.bounds);
										GUILayout.EndHorizontal();
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Draw renderer inspector.
		/// </summary>
		void DrawRendererComponent(Renderer com)
		{
			if (com != null)
			{
				// Light probes
				GUILayout.BeginHorizontal();
				GUILayout.Label("Light probes", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.lightProbeUsage = (UnityEngine.Rendering.LightProbeUsage)Utility.DrawPopup((int)com.lightProbeUsage, typeof(UnityEngine.Rendering.LightProbeUsage));
				GUILayout.EndHorizontal();
				// Reflection Probes
				GUILayout.BeginHorizontal();
				GUILayout.Label("Reflection Probes", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.reflectionProbeUsage = (UnityEngine.Rendering.ReflectionProbeUsage)Utility.DrawPopup((int)com.reflectionProbeUsage, typeof(UnityEngine.Rendering.ReflectionProbeUsage));
				GUILayout.EndHorizontal();
				// anchor override
				GUILayout.BeginHorizontal();
				GUILayout.Label("Anchor Override : " + (com.probeAnchor == null ? "null" : com.probeAnchor.name), Utility.LabelStyle);
				GUILayout.EndHorizontal();
				// Cast shadows
				GUILayout.BeginHorizontal();
				GUILayout.Label("Cast Shadows", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.shadowCastingMode = (UnityEngine.Rendering.ShadowCastingMode)Utility.DrawPopup((int)com.shadowCastingMode, typeof(UnityEngine.Rendering.ShadowCastingMode));
				GUILayout.EndHorizontal();
				// Receive shadows
				GUILayout.BeginHorizontal();
				GUILayout.Label("Receive Shadows", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				var receiveShadow = GUILayout.Toggle(com.receiveShadows, "", Utility.ToggleStyle);
				if (receiveShadow != com.receiveShadows)
				{
					com.receiveShadows = receiveShadow;
				}
				GUILayout.EndHorizontal();
				// Motion vectors
				GUILayout.BeginHorizontal();
				GUILayout.Label("Motion Vectors", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.motionVectorGenerationMode = (MotionVectorGenerationMode)Utility.DrawPopup((int)com.motionVectorGenerationMode, typeof(MotionVectorGenerationMode));
				GUILayout.EndHorizontal();

				// Shared materials
				GUILayout.BeginHorizontal();
				GUILayout.Label("Shared Materials", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				var mats = com.sharedMaterials;
				for (int i = 0, max = mats.Length; i < max; ++i)
				{
					materialViewer.DrawMaterialField(mats[i]);
				}
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw rigidbody inspector.
		/// </summary>
		void DrawRigidbodyComponent(Rigidbody com)
		{
			if (com != null)
			{
				// Mass
				GUILayout.BeginHorizontal();
				GUILayout.Label("Mass", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.mass = Utility.FloatField(com.mass);
				GUILayout.EndHorizontal();
				// Drag
				GUILayout.BeginHorizontal();
				GUILayout.Label("Drag", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.drag = Utility.FloatField(com.drag);
				GUILayout.EndHorizontal();
				// Angular drag
				GUILayout.BeginHorizontal();
				GUILayout.Label("Angular Drag", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.angularDrag = Utility.FloatField(com.angularDrag);
				GUILayout.EndHorizontal();
				// Use gravity
				GUILayout.BeginHorizontal();
				com.useGravity = GUILayout.Toggle(com.useGravity, "Use Gravity", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Is kinematic
				GUILayout.BeginHorizontal();
				com.isKinematic = GUILayout.Toggle(com.isKinematic, "Is Kinematic", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Interpolate
				GUILayout.BeginHorizontal();
				GUILayout.Label("Interpolate", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.interpolation = (RigidbodyInterpolation)Utility.DrawPopup((int)com.interpolation, typeof(RigidbodyInterpolation));
				GUILayout.EndHorizontal();
				// Collisiont detection
				GUILayout.BeginHorizontal();
				GUILayout.Label("Collisiont Detection", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.collisionDetectionMode = (CollisionDetectionMode)Utility.DrawPopup((int)com.collisionDetectionMode, typeof(CollisionDetectionMode));
				GUILayout.EndHorizontal();
				// Constraints
				GUILayout.BeginHorizontal();
				GUILayout.Label("Constraints", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				GUILayout.Label("Freeze Position", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				bool freezePosX = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionX);
				bool freezePosY = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionY);
				bool freezePosZ = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionZ);
				var fx = GUILayout.Toggle(freezePosX, "X", Utility.ToggleStyle);
				if (fx != freezePosX)
				{
					if (fx) com.constraints = (RigidbodyConstraints)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionX);
					else com.constraints = (RigidbodyConstraints)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionX);
				}
				var fy = GUILayout.Toggle(freezePosY, "Y", Utility.ToggleStyle);
				if (fy != freezePosY)
				{
					if (fy) com.constraints = (RigidbodyConstraints)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionY);
					else com.constraints = (RigidbodyConstraints)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionY);
				}
				var fz = GUILayout.Toggle(freezePosZ, "Z", Utility.ToggleStyle);
				if (fz != freezePosZ)
				{
					if (fz) com.constraints = (RigidbodyConstraints)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionZ);
					else com.constraints = (RigidbodyConstraints)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints.FreezePositionZ);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				GUILayout.Label("Freeze Rotation", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				bool freezeRotX = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationX);
				bool freezeRotY = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationY);
				bool freezeRotZ = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationZ);
				fx = GUILayout.Toggle(freezeRotX, "X", Utility.ToggleStyle);
				if (fx != freezeRotX)
				{
					if (fx) com.constraints = (RigidbodyConstraints)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationX);
					else com.constraints = (RigidbodyConstraints)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationX);
				}
				fy = GUILayout.Toggle(freezeRotY, "Y", Utility.ToggleStyle);
				if (fy != freezeRotY)
				{
					if (fy) com.constraints = (RigidbodyConstraints)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationY);
					else com.constraints = (RigidbodyConstraints)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationY);
				}
				fz = GUILayout.Toggle(freezeRotZ, "Z", Utility.ToggleStyle);
				if (fz != freezeRotZ)
				{
					if (fz) com.constraints = (RigidbodyConstraints)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationZ);
					else com.constraints = (RigidbodyConstraints)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints.FreezeRotationZ);
				}
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw rigidbody 2D inspector.
		/// </summary>
		void DrawRigidbody2DComponent(Rigidbody2D com)
		{
			if (com != null)
			{
				// Body type
				GUILayout.BeginHorizontal();
				GUILayout.Label("Body Type", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.bodyType = (RigidbodyType2D)Utility.DrawPopup((int)com.bodyType, typeof(RigidbodyType2D));
				GUILayout.EndHorizontal();
				// Simulated
				GUILayout.BeginHorizontal();
				com.simulated = GUILayout.Toggle(com.simulated, "Simulated", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Use auto mass
				GUILayout.BeginHorizontal();
				com.useAutoMass = GUILayout.Toggle(com.useAutoMass, "Use Auto Mass", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Mass
				GUILayout.BeginHorizontal();
				GUILayout.Label("Mass", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				var mass = Utility.FloatField(com.mass);
				if (!com.useAutoMass && com.mass != mass)
				{
					com.mass = mass;
				}
				GUILayout.EndHorizontal();
				// Linear drag
				GUILayout.BeginHorizontal();
				GUILayout.Label("Linear Drag", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.drag = Utility.FloatField(com.drag);
				GUILayout.EndHorizontal();
				// Angular drag
				GUILayout.BeginHorizontal();
				GUILayout.Label("Angular Drag", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.angularDrag = Utility.FloatField(com.angularDrag);
				GUILayout.EndHorizontal();
				// Gravity scale
				GUILayout.BeginHorizontal();
				GUILayout.Label("Gravity Scale", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.gravityScale = Utility.FloatField(com.gravityScale);
				GUILayout.EndHorizontal();
				// Collisiont detection
				GUILayout.BeginHorizontal();
				GUILayout.Label("Collisiont Detection", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.collisionDetectionMode = (CollisionDetectionMode2D)Utility.DrawPopup((int)com.collisionDetectionMode, typeof(CollisionDetectionMode2D));
				GUILayout.EndHorizontal();
				// Sleeping mode
				GUILayout.BeginHorizontal();
				GUILayout.Label("Sleeping Mode", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.sleepMode = (RigidbodySleepMode2D)Utility.DrawPopup((int)com.sleepMode, typeof(RigidbodySleepMode2D));
				GUILayout.EndHorizontal();
				// Interpolate
				GUILayout.BeginHorizontal();
				GUILayout.Label("Interpolate", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.interpolation = (RigidbodyInterpolation2D)Utility.DrawPopup((int)com.interpolation, typeof(RigidbodyInterpolation2D));
				GUILayout.EndHorizontal();
				// Constraints
				GUILayout.BeginHorizontal();
				GUILayout.Label("Constraints", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				GUILayout.Label("Freeze Position", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				bool freezePosX = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezePositionX);
				bool freezePosY = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezePositionY);
				var fx = GUILayout.Toggle(freezePosX, "X", Utility.ToggleStyle);
				if (fx != freezePosX)
				{
					if (fx) com.constraints = (RigidbodyConstraints2D)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezePositionX);
					else com.constraints = (RigidbodyConstraints2D)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezePositionX);
				}
				var fy = GUILayout.Toggle(freezePosY, "Y", Utility.ToggleStyle);
				if (fy != freezePosY)
				{
					if (fy) com.constraints = (RigidbodyConstraints2D)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezePositionY);
					else com.constraints = (RigidbodyConstraints2D)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezePositionY);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				GUILayout.Label("Freeze Rotation", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				bool freezeRotZ = Utility.ContainsFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezeRotation);
				var fz = GUILayout.Toggle(freezeRotZ, "Z", Utility.ToggleStyle);
				if (fz != freezeRotZ)
				{
					if (fz) com.constraints = (RigidbodyConstraints2D)Utility.AddFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezeRotation);
					else com.constraints = (RigidbodyConstraints2D)Utility.RemoveFlag((int)com.constraints, (int)RigidbodyConstraints2D.FreezeRotation);
				}
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw camera inspector.
		/// </summary>
		void DrawCameraComponent(Camera com)
		{
			if (com != null)
			{
				// Clear flags
				GUILayout.BeginHorizontal();
				GUILayout.Label("Clear Flags", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.clearFlags = (CameraClearFlags)Utility.DrawPopup((int)com.clearFlags, typeof(CameraClearFlags));
				GUILayout.EndHorizontal();
				if (com.clearFlags <= CameraClearFlags.Color)
				{
					// Background
					GUILayout.BeginHorizontal();
					GUILayout.Label("Background", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					com.backgroundColor = colorViewer.DrawColorField(com.backgroundColor, "UnityEngine.Camera.Background");
					GUILayout.EndHorizontal();
				}
				// Culling mask
				GUILayout.BeginHorizontal();
				GUILayout.Label("Culling Mask", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.cullingMask = Utility.DrawLayerMaskPopup((int)com.cullingMask);
				GUILayout.EndHorizontal();
				// Projection
				GUILayout.BeginHorizontal();
				GUILayout.Label("Orthographic", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.orthographic = GUILayout.Toggle(com.orthographic, "", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Field of view
				GUILayout.BeginHorizontal();
				GUILayout.Label("Field Of View", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.fieldOfView = Utility.FloatField(com.fieldOfView);
				GUILayout.EndHorizontal();
				// Clipping planes
				GUILayout.BeginHorizontal();
				GUILayout.Label("Clipping Planes", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				GUILayout.Label("Near", Utility.LabelStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.nearClipPlane = Utility.FloatField(com.nearClipPlane);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				GUILayout.Label("Far", Utility.LabelStyle, GUILayout.Width(Utility.smallFieldWidth));
				com.farClipPlane = Utility.FloatField(com.farClipPlane);
				GUILayout.EndHorizontal();
				// Viewport rect
				var rect = com.rect;
				GUILayout.BeginHorizontal();
				GUILayout.Label("Viewport rect", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				GUILayout.Label("X", Utility.LabelStyle, GUILayout.Width(15f));
				rect.x = Utility.FloatField(rect.x);
				GUILayout.Label("Y", Utility.LabelStyle, GUILayout.Width(15f));
				rect.y = Utility.FloatField(rect.y);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				GUILayout.Label("W", Utility.LabelStyle, GUILayout.Width(15f));
				rect.width = Utility.FloatField(rect.width);
				GUILayout.Label("H", Utility.LabelStyle, GUILayout.Width(15f));
				rect.height = Utility.FloatField(rect.height);
				GUILayout.EndHorizontal();
				com.rect = rect;
				// Depth
				GUILayout.BeginHorizontal();
				GUILayout.Label("Depth", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.depth = Utility.FloatField(com.depth);
				GUILayout.EndHorizontal();
				// Rendering path
				GUILayout.BeginHorizontal();
				GUILayout.Label("Rendering Path", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.renderingPath = (RenderingPath)Utility.DrawPopup((int)com.renderingPath, typeof(RenderingPath));
				GUILayout.EndHorizontal();
				// Target texture
				GUILayout.BeginHorizontal();
				GUILayout.Label("Target Texture", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.targetTexture = (RenderTexture)textureViewer.DrawTextureField(com.targetTexture);
				GUILayout.EndHorizontal();
				// Occlusion culling
				GUILayout.BeginHorizontal();
				GUILayout.Label("Occlusion Culling", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.useOcclusionCulling = GUILayout.Toggle(com.useOcclusionCulling, "", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Allow HDR
				GUILayout.BeginHorizontal();
				GUILayout.Label("Allow HDR", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.allowHDR = GUILayout.Toggle(com.allowHDR, "", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Allow MSAA
				GUILayout.BeginHorizontal();
				GUILayout.Label("Allow MSAA", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.allowMSAA = GUILayout.Toggle(com.allowMSAA, "", Utility.ToggleStyle);
				GUILayout.EndHorizontal();
				// Target display
				GUILayout.BeginHorizontal();
				GUILayout.Label("Target Display", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.targetDisplay = Utility.IntField(com.targetDisplay);
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw light inspector.
		/// </summary>
		void DrawLightComponent(Light com)
		{
			if (com != null)
			{
				// Type
				GUILayout.BeginHorizontal();
				GUILayout.Label("Type", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.type = (LightType)Utility.DrawPopup((int)com.type, typeof(LightType));
				GUILayout.EndHorizontal();

				switch (com.type)
				{
					case LightType.Area:
						// Range
						GUILayout.BeginHorizontal();
						GUILayout.Label("Range : ", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						GUILayout.Label(com.range.ToString(), Utility.BackgroundedWhiteLabelStyle);
						GUILayout.EndHorizontal();

						// areaSize only run in editor
						//Vector2 areaSize = com.areaSize;
						//// Width
						//GUILayout.BeginHorizontal();
						//GUILayout.Label("Width", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						//areaSize.x = Utility.FloatField(areaSize.x);
						//GUILayout.EndHorizontal();
						//// Height
						//GUILayout.BeginHorizontal();
						//GUILayout.Label("Height", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						//areaSize.y = Utility.FloatField(areaSize.y);
						//GUILayout.EndHorizontal();

						//if (areaSize != com.areaSize)
						//	com.areaSize = areaSize;
						break;
					case LightType.Spot:
						// Range
						GUILayout.BeginHorizontal();
						GUILayout.Label("Range", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						com.range = Utility.FloatField(com.range);
						GUILayout.EndHorizontal();

						// Spot angle
						GUILayout.BeginHorizontal();
						GUILayout.Label("Spot Angle", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						com.spotAngle = Utility.FloatField(com.spotAngle);
						GUILayout.EndHorizontal();
						break;
					case LightType.Point:
						// Range
						GUILayout.BeginHorizontal();
						GUILayout.Label("Range", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
						com.range = Utility.FloatField(com.range);
						GUILayout.EndHorizontal();
						break;
				}

				// Color
				GUILayout.BeginHorizontal();
				GUILayout.Label("Color", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.color = colorViewer.DrawColorField(com.color, "UnityEngine.Light.Color");
				GUILayout.EndHorizontal();

				// Lightmap bake type only run in editor
				//// Mode
				//GUILayout.BeginHorizontal();
				//GUILayout.Label("Mode", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				//com.lightmapBakeType = (LightmapBakeType)Utility.DrawPopup((int)com.lightmapBakeType, typeof(LightmapBakeType));
				//GUILayout.EndHorizontal();

				// Intensity
				GUILayout.BeginHorizontal();
				GUILayout.Label("Intensity", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.intensity = Utility.FloatField(com.intensity);
				GUILayout.EndHorizontal();

				// Indirect multiplier
				GUILayout.BeginHorizontal();
				GUILayout.Label("Indirect Multiplier", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.bounceIntensity = Utility.FloatField(com.bounceIntensity);
				GUILayout.EndHorizontal();

				// Shadow type
				GUILayout.BeginHorizontal();
				GUILayout.Label("Shadow Type", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.shadows = (LightShadows)Utility.DrawPopup((int)com.shadows, typeof(LightShadows));
				GUILayout.EndHorizontal();

				// Cookie
				GUILayout.BeginHorizontal();
				GUILayout.Label("Cookie", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				var cookie = textureViewer.DrawTextureField(com.cookie);
				if (cookie != com.cookie)
					com.cookie = cookie;
				GUILayout.EndHorizontal();

				// Cookie size
				GUILayout.BeginHorizontal();
				GUILayout.Label("Cookie Size", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.cookieSize = Utility.FloatField(com.cookieSize);
				GUILayout.EndHorizontal();

				// Draw halo
				GUILayout.BeginHorizontal();
				GUILayout.Label("Draw Halo", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				Behaviour halo = com.gameObject.GetComponent("Halo") as Behaviour;
				if (halo)
				{
					var enableHalo = GUILayout.Toggle(halo.enabled, "", Utility.ToggleStyle);
					if (enableHalo != halo.enabled)
					{
						halo.enabled = enableHalo;
					}
				}
				else
				{
					GUILayout.Label("No Halo Attach On This GameObject", Utility.YelloLabelStyle);
				}
				GUILayout.EndHorizontal();

				// Flare
				GUILayout.BeginHorizontal();
				GUILayout.Label("Flare", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				Utility.DrawObjectField(com.flare);
				GUILayout.EndHorizontal();

				// Render mode
				GUILayout.BeginHorizontal();
				GUILayout.Label("Render Mode", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.renderMode = (LightRenderMode)Utility.DrawPopup((int)com.renderMode, typeof(LightRenderMode));
				GUILayout.EndHorizontal();

				// Culling mask
				GUILayout.BeginHorizontal();
				GUILayout.Label("Culling Mask", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.cullingMask = Utility.DrawLayerMaskPopup((int)com.cullingMask);
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw AudioSource inspector.
		/// </summary>
		void DrawAudioSource(AudioSource com)
		{
			if (com != null)
			{
				// Audio clip
				GUILayout.BeginHorizontal();
				GUILayout.Label("Audio Clip", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				Utility.DrawObjectField(com.clip);
				GUILayout.EndHorizontal();
				// Output
				GUILayout.BeginHorizontal();
				GUILayout.Label("Output", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				Utility.DrawObjectField(com.outputAudioMixerGroup);
				GUILayout.EndHorizontal();
				// Mute
				GUILayout.BeginHorizontal();
				var mute = GUILayout.Toggle(com.mute, "Mute", Utility.ToggleStyle);
				if (com.mute != mute)
				{
					com.mute = mute;
				}
				GUILayout.EndHorizontal();
				// Bypass effects
				GUILayout.BeginHorizontal();
				var bypassEffects = GUILayout.Toggle(com.bypassEffects, "Bypass Effects", Utility.ToggleStyle);
				if (com.bypassEffects != bypassEffects)
				{
					com.bypassEffects = bypassEffects;
				}
				GUILayout.EndHorizontal();
				// Bypass listener effects
				GUILayout.BeginHorizontal();
				var bypassListenerEffects = GUILayout.Toggle(com.bypassListenerEffects, "Bypass Listener Effects", Utility.ToggleStyle);
				if (com.bypassListenerEffects != bypassListenerEffects)
				{
					com.bypassListenerEffects = bypassListenerEffects;
				}
				GUILayout.EndHorizontal();
				// Bypass reverb zones
				GUILayout.BeginHorizontal();
				var bypassReverbZones = GUILayout.Toggle(com.bypassReverbZones, "Bypass Reverb Zones", Utility.ToggleStyle);
				if (com.bypassReverbZones != bypassReverbZones)
				{
					com.bypassReverbZones = bypassReverbZones;
				}
				GUILayout.EndHorizontal();
				// Play on awake
				GUILayout.BeginHorizontal();
				var playOnAwake = GUILayout.Toggle(com.playOnAwake, "Play On Awake", Utility.ToggleStyle);
				if (com.playOnAwake != playOnAwake)
				{
					com.playOnAwake = playOnAwake;
				}
				GUILayout.EndHorizontal();
				// Loop
				GUILayout.BeginHorizontal();
				var loop = GUILayout.Toggle(com.loop, "Loop", Utility.ToggleStyle);
				if (com.loop != loop)
				{
					com.loop = loop;
				}
				GUILayout.EndHorizontal();
				// Priority
				GUILayout.BeginHorizontal();
				GUILayout.Label("Priority", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.priority = Utility.IntField(com.priority);
				GUILayout.EndHorizontal();
				// Volume
				GUILayout.BeginHorizontal();
				GUILayout.Label("Volume", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.volume = Utility.FloatField(com.volume);
				GUILayout.EndHorizontal();
				// Pitch
				GUILayout.BeginHorizontal();
				GUILayout.Label("Pitch", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.pitch = Utility.FloatField(com.pitch);
				GUILayout.EndHorizontal();
				// Stereo pan
				GUILayout.BeginHorizontal();
				GUILayout.Label("Stereo Pan", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.panStereo = Utility.FloatField(com.panStereo);
				GUILayout.EndHorizontal();
				// Spatial blend
				GUILayout.BeginHorizontal();
				GUILayout.Label("Spatial Blend", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.spatialBlend = Utility.FloatField(com.spatialBlend);
				GUILayout.EndHorizontal();
				// Reverb zone mix
				GUILayout.BeginHorizontal();
				GUILayout.Label("Reverb Zone Mix", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.reverbZoneMix = Utility.FloatField(com.reverbZoneMix);
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Draw VideoPlayer inspector.
		/// </summary>
		void DrawVideoPlayerComponent(UnityEngine.Video.VideoPlayer com)
		{
			if (com != null)
			{
				// Source
				GUILayout.BeginHorizontal();
				GUILayout.Label("Source", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.source = (UnityEngine.Video.VideoSource)Utility.DrawPopup((int)com.source, typeof(UnityEngine.Video.VideoSource));
				GUILayout.EndHorizontal();

				if (com.source == UnityEngine.Video.VideoSource.VideoClip)
				{
					// Video clip
					GUILayout.BeginHorizontal();
					GUILayout.Label("Video Clip", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					Utility.DrawObjectField(com.clip);
					GUILayout.EndHorizontal();
				}
				else if (com.source == UnityEngine.Video.VideoSource.Url)
				{
					// URL
					GUILayout.BeginHorizontal();
					GUILayout.Label("URL", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
					var url = Utility.DrawText(com.url);
					if (url != com.url)
					{
						com.url = url;
					}
					GUILayout.EndHorizontal();
				}
				// Play on awake
				GUILayout.BeginHorizontal();
				GUILayout.Label("Play On Awake", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.playOnAwake = GUILayout.Toggle(com.playOnAwake, "", Utility.ToggleStyle);
				GUILayout.EndHorizontal();

				// Wait for first frame
				GUILayout.BeginHorizontal();
				GUILayout.Label("Wait For First Frame", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.waitForFirstFrame = GUILayout.Toggle(com.waitForFirstFrame, "", Utility.ToggleStyle);
				GUILayout.EndHorizontal();

				// Loop
				GUILayout.BeginHorizontal();
				GUILayout.Label("Loop", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.isLooping = GUILayout.Toggle(com.isLooping, "", Utility.ToggleStyle);
				GUILayout.EndHorizontal();

				// Playback speed
				GUILayout.BeginHorizontal();
				GUILayout.Label("Playback Speed", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.playbackSpeed = Utility.FloatField(com.playbackSpeed);
				GUILayout.EndHorizontal();

				// Render mode
				GUILayout.BeginHorizontal();
				GUILayout.Label("Render Mode", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.renderMode = (UnityEngine.Video.VideoRenderMode)Utility.DrawPopup((int)com.renderMode, typeof(UnityEngine.Video.VideoRenderMode));
				GUILayout.EndHorizontal();

				// Target texture
				GUILayout.BeginHorizontal();
				GUILayout.Label("Target Texture", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				var tex = textureViewer.DrawTextureField(com.targetTexture);
				if (com.targetTexture != tex)
				{
					com.targetTexture = (RenderTexture)tex;
				}
				GUILayout.EndHorizontal();

				// Aspect ratio
				GUILayout.BeginHorizontal();
				GUILayout.Label("Aspect Ratio", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.aspectRatio = (UnityEngine.Video.VideoAspectRatio)Utility.DrawPopup((int)com.aspectRatio, typeof(UnityEngine.Video.VideoAspectRatio));
				GUILayout.EndHorizontal();

				// Audio output mode
				GUILayout.BeginHorizontal();
				GUILayout.Label("Audio Output Mode", Utility.LabelStyle, GUILayout.Width(fieldMinWidth));
				com.audioOutputMode = (UnityEngine.Video.VideoAudioOutputMode)Utility.DrawPopup((int)com.audioOutputMode, typeof(UnityEngine.Video.VideoAudioOutputMode));
				GUILayout.EndHorizontal();
			}
		}
		#endregion

		/// <summary>
		/// Draw object.
		/// </summary>
		object DrawObject(Type type, object obj, Type parentType, out Utility.DrawData drawData, int depth = 0)
		{
			drawData = Utility.GetDrawDatas(type, obj == null);

			if (drawData != null && depth < maximumDepth)
			{
				for (int c = 0, max = drawData.memberInfos.Length; c < max; ++c)
				{
					var member = drawData.memberInfos[c];
					if (member.property)
					{
						if (!member.dontDrawProperty)
							obj = DrawProperty(member.member as PropertyInfo, obj, type, depth, c, member.drawSet);
					}
					else
					{
						obj = DrawField(member.member as FieldInfo, obj, type, depth, c);
					}
				}
			}

			return obj;
		}

		/// <summary>
		/// Draw property info and set.
		/// </summary>
		object DrawProperty(PropertyInfo property, object obj, Type parentType, int depth, int index, bool drawSet)
		{
			try
			{
				var value = property.GetValue(obj, null);
				var result = DrawPropertyOrField(property.Name, property.PropertyType, value, parentType, depth, index);
				if (drawSet)
				{
					if (result != null)
					{
						if (result is string)
							property.SetValue(obj, System.ComponentModel.TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(result.ToString()), null);
						else
							property.SetValue(obj, result, null);
					}
				}
			}
			catch (Exception ex)
			{
				if (Utility.DebugFlag) Debug.LogError(string.Format("Object:{0} property:{1} throw errores:{2}", parentType.Name, property.Name, ex));
			}
			return obj;
		}

		/// <summary>
		/// Draw field and set.
		/// </summary>
		object DrawField(FieldInfo field, object obj, Type parentType, int depth, int index)
		{
			object value = null;
			try
			{
				value = field.GetValue(obj);
			}
			catch (Exception ex)
			{
				if (Utility.DebugFlag) Debug.LogError(string.Format("Component:{0} field:{1} throw errores:{2}", parentType.Name, field.Name, ex));
				return obj;
			}
			bool editable = !field.IsLiteral && !field.IsInitOnly;

			var result = DrawPropertyOrField(field.Name, field.FieldType, value, parentType, depth, index);

			if (editable && result != null)
			{
				try
				{
					if (result.GetType() == typeof(string))
						field.SetValue(obj, System.ComponentModel.TypeDescriptor.GetConverter(field.FieldType).ConvertFromString(result.ToString()));
					else
						field.SetValue(obj, result);
				}
				catch (Exception ex)
				{
					if (Utility.DebugFlag) Debug.LogError(string.Format("Component:{0} field:{1} throw errores:{2}", parentType.Name, field.Name, ex));
				}
			}

			return obj;
		}

		/// <summary>
		/// main draw of property / field.
		/// </summary>
		object DrawPropertyOrField(string propertyName, Type propertyType, object property, Type parentType, int depth, int index = -1, float namewidth = 150f)
		{
			if (depth > maximumDepth) return property;
			string typeName = propertyType == null ? null : propertyType.FullName;
			object result = null;

			GUILayout.BeginHorizontal();

			GUILayout.Space((depth + 1) * 20f);

			GUILayout.Label(propertyName, Utility.LabelStyle, GUILayout.Width(namewidth));

			switch (typeName)
			{
				case "System.IntPtr":
				case "System.UIntPtr":
				case "System.Type":
					result = Utility.DrawObjectField(property, GUILayout.MinWidth(fieldMinWidth));
					break;
				case "UnityEngine.Vector2":
					result = Utility.DrawVector2((Vector2)property);
					break;
				case "UnityEngine.Vector3":
					result = Utility.DrawVector3((Vector3)property);
					break;
				case "UnityEngine.Vector4":
					result = Utility.DrawVector4((Vector4)property);
					break;
				case "UnityEngine.Quaternion":
					result = Utility.DrawQuaternion((Quaternion)property);
					break;
				case "System.Int64":
				case "System.UInt64":
					result = Utility.LongField(Convert.ToInt64(property), GUILayout.MinWidth(fieldMinWidth));
					break;
				case "System.Int32":
				case "System.Int16":
				case "System.UInt16":
				case "System.UInt32":
					result = Utility.IntField(Convert.ToInt32(property), GUILayout.MinWidth(fieldMinWidth));
					break;
				case "System.Decimal":
				case "System.Double":
					result = Utility.DoubleField(Convert.ToDouble(property), GUILayout.MinWidth(fieldMinWidth));
					break;
				case "System.Single":
					result = Utility.FloatField(Convert.ToSingle(property), GUILayout.MinWidth(fieldMinWidth));
					break;
				case "System.String":
					result = Utility.DrawText(Convert.ToString(property), GUILayout.MinWidth(fieldMinWidth));
					break;
				case "System.Text.StringBuilder":
					Utility.DrawText(Convert.ToString(property), GUILayout.MinWidth(fieldMinWidth));
					result = property;
					break;
				case "System.Boolean":
					result = GUILayout.Toggle(Convert.ToBoolean(property), "", Utility.ToggleStyle, GUILayout.MinWidth(fieldMinWidth));
					break;
#if UNITY_2017_2_OR_NEWER
				case "UnityEngine.Vector2Int":
					result = Utility.DrawVector2Int((Vector2Int)property);
					break;
				case "UnityEngine.Vector3Int":
					result = Utility.DrawVector3Int((Vector3Int)property);
					break;
				case "UnityEngine.RectInt":
				case "UnityEngine.BoundsInt":
#endif
				case "UnityEngine.Rect":
				case "UnityEngine.Bounds":
				case "UnityEngine.AnimationCurve":
					result = Utility.DrawObjectField(property, GUILayout.MinWidth(fieldMinWidth));
					break;
				case "UnityEngine.Material":
					result = materialViewer.DrawMaterialField((Material)property);
					break;
				case "UnityEngine.Color":
					result = colorViewer.DrawColorField((Color)property, propertyName);
					break;
				case "UnityEngine.Color32":
					result = colorViewer.DrawColorField((Color32)property, propertyName);
					break;
				case "UnityEngine.Sprite":
					result = textureViewer.DrawSpriteField((Sprite)property);
					break;
				case "UnityEngine.Texture":
				case "UnityEngine.Texture2D":
				case "UnityEngine.RenderTexture":
					result = textureViewer.DrawTextureField((Texture)property);
					break;
				case "UnityEngine.Matrix4x4":
					result = Utility.DrawArea(property, GUILayout.MinWidth(fieldMinWidth));
					break;
				default:
					if (property == null)
					{
						GUILayout.Label("null", Utility.LabelStyle, GUILayout.MinWidth(fieldMinWidth));
					}
					else if (typeof(Enum).IsAssignableFrom(propertyType))
					{
						result = Utility.DrawPopup((int)property, propertyType);
					}
					else if (property is UnityEngine.Object)
					{
						result = Utility.DrawObjectField(property, GUILayout.MinWidth(fieldMinWidth));
					}
					else
					{
						try
						{
							if (property is IDictionary)
							{
								result = DrawIDictionary(depth + 1, index, (IDictionary)property, propertyType, parentType);
							}
							else if (property is IEnumerable)
							{
								result = DrawIEnumerable(depth + 1, index, (IEnumerable)property, parentType);
							}
							else
							{
								GUILayout.BeginVertical(Utility.BoxStyle);
								Utility.DrawData data;
								result = DrawObject(propertyType, property, parentType, out data, depth + 1);
								GUILayout.EndVertical();
							}
						}
						catch (Exception e)
						{
							Debug.LogError(e);
						}
					}
					break;
			}

			GUILayout.EndHorizontal();

			return result;
		}

		/// <summary>
		/// Get "Count" property.
		/// </summary>
		static int GetCount(object list)
		{
			if (list == null) return 0;

			if (list is ICollection)
			{
				return (list as ICollection).Count;
			}
			else if (list is ICollection<int>)
			{
				return (list as ICollection<int>).Count;
			}
			else if (list is ICollection<string>)
			{
				return (list as ICollection<string>).Count;
			}
			else if (list is ICollection<uint>)
			{
				return (list as ICollection<uint>).Count;
			}
			else if (list is ICollection<long>)
			{
				return (list as ICollection<long>).Count;
			}
			else if (list is ICollection<ulong>)
			{
				return (list as ICollection<ulong>).Count;
			}
			else if (list is ICollection<short>)
			{
				return (list as ICollection<short>).Count;
			}
			else if (list is ICollection<ushort>)
			{
				return (list as ICollection<ushort>).Count;
			}
			else if (list is ICollection<float>)
			{
				return (list as ICollection<float>).Count;
			}
			else if (list is ICollection<double>)
			{
				return (list as ICollection<double>).Count;
			}
			else
			{
				try
				{
					var objlist = (ICollection<object>)list;
					if (objlist != null)
						return objlist.Count;
				}
				catch { }
			}
			return 0;
		}

		/// <summary>
		/// Get array/dictionary element type.
		/// </summary>
		static Type GetElementType(object property, int index = 0)
		{
			if (property == null) return null;

			Type enumerableType = property.GetType();
			Type propertyType = null;
			if (enumerableType.HasElementType)
				propertyType = enumerableType.GetElementType();
			else
				propertyType = enumerableType.GetGenericArguments()[index];

			return propertyType;
		}

		/// <summary>
		/// Can this type be create by Activator.CreateInstance?
		/// </summary>
		static bool CanCreateInstance(Type type, bool log)
		{
			if (type == typeof(string) || type.IsEnum) return false;
			if (type.IsAbstract || type.IsInterface || type.IsGenericType || type == typeof(object)) return false;
			if (type.IsClass)
			{
				if (typeof(UnityEngine.Object).IsAssignableFrom(type) || type.IsSubclassOf(typeof(Delegate)))
					return false;
				var constructor = type.GetConstructor(Type.EmptyTypes);
				if (constructor == null)
				{
					if (log) Debug.LogError("CreateInstance Error : Type (" + type + ") does not have a default constructor.");
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Create instance using Activator.CreateInstance.
		/// </summary>
		static object CreateInstance(Type type, bool log = false)
		{
			object result = null;
			if (CanCreateInstance(type, log))
				result = Activator.CreateInstance(type);
			else if (type == typeof(string))
				result = string.Empty;
			else if (type.IsEnum)
				result = type.GetFields(BindingFlags.Static | BindingFlags.Public).First().GetValue(null);
			return result;
		}

		/// <summary>
		/// Insert value to array.
		/// </summary>
		static Array Insert(Array list, object obj, int index)
		{
			Array arr = Array.CreateInstance(list.GetType().GetElementType(), list.Length + 1);

			for (int i = 0, j = 0, max = arr.Length; i < max; ++i)
			{
				if (i != index)
					arr.SetValue(list.GetValue(j++), i);
				else arr.SetValue(obj, i);
			}

			return arr;
		}

		/// <summary>
		/// Remove array at index.
		/// </summary>
		static Array RemoveAt(Array list, int index)
		{
			if (list.Length == 0) return list;
			Array arr = Array.CreateInstance(list.GetType().GetElementType(), list.Length - 1);
			for (int i = 0, j = 0, max = arr.Length; i < max; ++i)
			{
				if (i != index)
					arr.SetValue(list.GetValue(i), j++);
			}
			return arr;
		}

		/// <summary>
		/// Remove list of index.
		/// </summary>
		static Array RemoveFrom(Array list, ICollection<int> indexs)
		{
			if (list.Length == 0) return list;
			Array arr = Array.CreateInstance(list.GetType().GetElementType(), Mathf.Clamp(list.Length - indexs.Count, 0, list.Length));
			for (int i = 0, j = 0, max = arr.Length; i < max; ++i)
			{
				if (!indexs.Contains(i))
					arr.SetValue(list.GetValue(i), j++);
			}
			return arr;
		}

		/// <summary>
		/// Try Add data to collection.
		/// </summary>
		static object TryAdd(object list, object obj)
		{
			if (list == null) return list;

			if (list is Array)
			{
				return Insert((Array)list, obj, 0);
			}

			if (list is IList)
			{
				(list as IList).Add(obj);
			}

			else if (list is ICollection<int>)
			{
				(list as ICollection<int>).Add((int)obj);
			}
			else if (list is ICollection<string>)
			{
				(list as ICollection<string>).Add((string)obj);
			}
			else if (list is ICollection<uint>)
			{
				(list as ICollection<uint>).Add((uint)obj);
			}
			else if (list is ICollection<long>)
			{
				(list as ICollection<long>).Add((long)obj);
			}
			else if (list is ICollection<ulong>)
			{
				(list as ICollection<ulong>).Add((ulong)obj);
			}
			else if (list is ICollection<short>)
			{
				(list as ICollection<short>).Add((short)obj);
			}
			else if (list is ICollection<ushort>)
			{
				(list as ICollection<ushort>).Add((ushort)obj);
			}
			else if (list is ICollection<float>)
			{
				(list as ICollection<float>).Add((float)obj);
			}
			else if (list is ICollection<double>)
			{
				(list as ICollection<double>).Add((double)obj);
			}
			else
			{
				try
				{
					var objlist = (ICollection<object>)list;
					if (objlist != null)
					{
						objlist.Add(obj);
					}
				}
				catch { }
			}
			return list;
		}

		/// <summary>
		/// Helper method for drawing folder.
		/// </summary>
		static bool ToggleFolder(string title, bool showing, int depth, float titleLabelWidth = 0f)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Space(depth * 20f);

			string mark = showing ? "\u25BC  " : "\u25BA  ";
			bool clicked = false;

			title = mark + title;

			if (titleLabelWidth != 0f)
				GUILayout.Label(title, Utility.LabelStyle, GUILayout.Width(titleLabelWidth));
			else
				GUILayout.Label(title, Utility.LabelStyle);

			GUILayout.EndHorizontal();

			var rect = GUILayoutUtility.GetLastRect();
			if (GUI.Button(rect, "", Utility.YelloLabelStyle))
				clicked = true;

			return clicked;
		}

		/// <summary>
		/// Draw long list and folder.
		/// </summary>
		static void DrawIEnumerableFolder(int size, int depth, Utility.ListData longListData, Action<int, int> onDraw, int eachSize = 50)
		{
			int totalLine = (size / Mathf.Max(eachSize, 1)) + 1;
			for (int c = 0; c < totalLine; ++c)
			{
				int st = c * eachSize;
				int ed = Mathf.Min((c + 1) * eachSize, size);
				if (st >= ed) continue;
				string title = (st != (ed - 1)) ? string.Format("{0}~{1}", st, ed - 1) : st.ToString();
				if (ToggleFolder(title, !longListData.IsFoldered(c), depth))
				{
					longListData.ToggleFolder(c);
				}
				if (!longListData.IsFoldered(c))
				{
					onDraw(st, ed);
				}
			}
		}

		/// <summary>
		/// Draw IEnumerable.
		/// </summary>
		IEnumerable DrawIEnumerable(int depth, int index, IEnumerable property, Type parentType)
		{
			int size = GetCount(property);

			GUILayout.Label("Size : " + size, Utility.LabelStyle);

			if (GUILayout.Button("Add", Utility.ButtonStyle, GUILayout.Width(80f)))
			{
				try
				{
					Type propertyType = GetElementType(property, 0);
					if (propertyType != null)
					{
						var obj = CreateInstance(propertyType, true);
						if (obj != null)
						{
							property = (IEnumerable)TryAdd(property, obj);
						}
					}
				}
				catch (Exception ex)
				{
					if (Utility.DebugFlag) Debug.LogError(ex);
				}
			}

			if (!listDatas.ContainsKey(depth * 100000 + index))
				listDatas.Add(depth * 100000 + index, new Utility.ListData());
			var longListData = listDatas[depth * 100000 + index];

			if (property is IList)
			{
				var list = property as IList;

				if (GUILayout.Button("Clear", Utility.ButtonStyle, GUILayout.Width(80f)))
				{
					list.Clear();
				}

				GUILayout.EndHorizontal();

				DrawIEnumerableFolder(size, depth + 1, longListData, (st, ed) =>
				{
					Type type = GetElementType(property, 0);
					tempRemoveList.Clear();
					for (int c = st; c < ed; ++c)
					{
						var ele = list[c];
						GUILayout.BeginHorizontal();
						var result = DrawPropertyOrField("Element " + c, type, ele, parentType, depth + 1, c);
						if (result != ele)
						{
							list[c] = result;
						}
						if (GUILayout.Button("Remove", Utility.ButtonStyle, GUILayout.Width(100f)))
						{
							tempRemoveList.Add(result);
						}
						GUILayout.EndHorizontal();
					}
					if (tempRemoveList.Count > 0)
					{
						foreach (var key in tempRemoveList)
						{
							list.Remove(key);
						}
					}
				});
			}
			else
			{
				GUILayout.EndHorizontal();

				int ind = 0;

				DrawIEnumerableFolder(size, depth + 1, longListData, (st, ed) =>
				{
					Type type = GetElementType(property, 0);
					foreach (var ele in property)
					{
						if (ind >= st && ind < ed)
						{
							DrawPropertyOrField("Element " + ind, type, ele, parentType, depth + 1, ind);
						}
						ind++;
					}
				});
			}

			GUILayout.BeginHorizontal();
			return property;
		}

		/// <summary>
		/// Draw IDictionary.
		/// </summary>
		IDictionary DrawIDictionary(int depth, int index, IDictionary property, Type propertyType, Type parentType)
		{
			if (!dictDatas.ContainsKey(depth * 100000 + index))
				dictDatas.Add(depth * 100000 + index, new Utility.DictionaryData(propertyType));
			var dictionaryData = dictDatas[depth * 100000 + index];

			GUILayout.Label("Size : " + property.Count, Utility.LabelStyle);

			if (GUILayout.Button("Add", Utility.ButtonStyle, GUILayout.Width(80f)))
			{
				dictionaryData.Show = !dictionaryData.Show;
			}
			if (GUILayout.Button("Clear", Utility.ButtonStyle, GUILayout.Width(80f)))
			{
				property.Clear();
			}

			GUILayout.EndHorizontal();

			if (dictionaryData.Show)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				GUILayout.BeginVertical(Utility.BoxStyle);
				{
					GUILayout.BeginHorizontal();
					dictionaryData.key = DrawPropertyOrField(string.Format("Key ({0}):", dictionaryData.keyType), dictionaryData.keyType, dictionaryData.key, parentType, 0, 0);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					dictionaryData.value = DrawPropertyOrField(string.Format("Value ({0}):", dictionaryData.valueType), dictionaryData.valueType, dictionaryData.value, parentType, 0, 0);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Space(depth * 20f);
					if (GUILayout.Button("Add New Object", Utility.ButtonStyle))
					{
						try
						{
							if (!property.Contains(dictionaryData.key))
							{
								property.Add(dictionaryData.key, dictionaryData.value);
								dictionaryData.key = dictionaryData.value = null;
								dictionaryData.Show = false;
							}
							else
							{
								Debug.LogError(string.Format("Key:{0}, already existed.", dictionaryData.key));
							}
						}
						catch (Exception ex)
						{
							if (Utility.DebugFlag) Debug.LogError(ex);
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}

			if (property.Count > 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space((depth + 1) * 20f);
				GUILayout.BeginVertical(Utility.BoxStyle);
				{
					if (!listDatas.ContainsKey(depth * 100000 + index))
						listDatas.Add(depth * 100000 + index, new Utility.ListData());
					var longListData = listDatas[depth * 100000 + index];

					DrawIEnumerableFolder(property.Count, depth, longListData, (st, ed) =>
					{
						Type keyType = GetElementType(property, 0);
						Type valueType = GetElementType(property, 1);
						tempRemoveList.Clear();
						int ind = 0;
						foreach (DictionaryEntry ele in property)
						{
							if (ind >= st && ind < ed)
							{
								GUILayout.BeginHorizontal();
								DrawPropertyOrField("key", keyType, ele.Key, parentType, depth + 1, ind);
								GUILayout.EndHorizontal();
								GUILayout.BeginHorizontal();
								DrawPropertyOrField("value", valueType, ele.Value, parentType, depth + 1, ind);
								if (GUILayout.Button("Remove", Utility.ButtonStyle, GUILayout.Width(100f)))
								{
									tempRemoveList.Add(ele.Key);
								}
								GUILayout.EndHorizontal();
							}
							ind++;
						}
						if (tempRemoveList.Count > 0)
						{
							foreach (var key in tempRemoveList)
							{
								property.Remove(key);
							}
						}
					});
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			return property;
		}

		/// <summary>
		/// Draw all methods.
		/// </summary>
		void DrawMethods(UnityEngine.Object component, Utility.DrawData drawData, int depth)
		{
			if (drawData == null || component == null) return;
			int instanceID = component.GetInstanceID();
			GUILayout.BeginVertical(Utility.BoxStyle);
			{
				GUILayout.BeginHorizontal();
				bool before = showHideComponentMethods.Contains(instanceID);
				string mark = before ? "\u25BC" : "\u25BA";

				GUILayout.Label(mark, Utility.LabelStyle, GUILayout.Width(20f));

				bool result = GUILayout.Toggle(before, component.GetType().Name + " Methods:", Utility.YelloLabelStyle);
				if (before != result)
				{
					if (result) showHideComponentMethods.Add(instanceID);
					else showHideComponentMethods.Remove(instanceID);
				}

				GUILayout.EndHorizontal();

				if (showHideComponentMethods.Contains(instanceID))
				{
					foreach (var method in drawData.methodInfos)
					{
						if (method == null || method.isBaseType || !method.canDraw || method.hasParameter) continue;
						DrawMethod(method, component, depth);
					}
				}
			}
			GUILayout.EndVertical();
		}

		/// <summary>
		/// Draw method.
		/// </summary>
		void DrawMethod(Utility.MethodData method, object owner, int depth)
		{
			GUILayout.BeginVertical(Utility.BoxStyle);
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space((depth + 1) * 20f);
				GUILayout.Label(method.method.Name, Utility.LabelStyle, GUILayout.Width(300f));

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Call", Utility.ButtonStyle, GUILayout.Width(120f)))
				{
					try
					{
						object methodResult = null;

						methodResult = method.method.Invoke(owner, null);

						if (method.method.ReturnType != null && method.method.ReturnType != typeof(void))
						{
							if (methodResult is string)
							{
								string st = methodResult as string;
								if (string.IsNullOrEmpty(st))
									Debug.Log("Method (" + method.method.Name + ") Returning Null");
								else
									Debug.Log("Method (" + method.method.Name + ") Result: " + st);
							}
							else if (methodResult == null)
								Debug.Log("Method (" + method.method.Name + ") Returning Null");
							else
								Debug.Log("Method (" + method.method.Name + ") Result: " + methodResult);
						}
						else
							Debug.Log("Method (" + method.method.Name + ") Call Succeed.");
					}
					catch (Exception ex)
					{
						if (Utility.DebugFlag) Debug.LogError(ex);
					}
				}

				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}
		#endregion
	}
}