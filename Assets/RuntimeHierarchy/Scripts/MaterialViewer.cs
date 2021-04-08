using UnityEngine;

namespace RHierarchy
{
	public class MaterialViewer : MonoBehaviour
	{
		Rect windowPos = new Rect(15f, 15f, 500f, 700f);
		Material current;
		string shaderName;
		MaterialData.PropertyData[] propertyDatas;

		Utility.Popup shaderListPopup;
		Utility.Popup ShaderListPopup
		{
			get
			{
				if (shaderListPopup == null)
				{
					int[] values = new int[MaterialData.Instance.ShaderNames.Length];
					for (int i = values.Length - 1; i >= 0; --i)
						values[i] = i;
					shaderListPopup = Utility.Popup.NewPopup(MaterialData.Instance.ShaderNames, values, 0);
				}
				return shaderListPopup;
			}
		}
		int shaderIndex;

		Vector2 scrollViewPos;

		public int windowID;
		public ColorViewer colorViewer;
		public TextureViewer textureViewer;
		public RuntimeHierarchy runtimeHierarchy;

		public Material DrawMaterialField(Material mat)
		{
			if (mat == null)
			{
				GUILayout.Label("null material", Utility.YelloLabelStyle);
			}
			else if (GUILayout.Button(string.Format("{0} (Click to view)", mat.name), Utility.ButtonStyle))
			{
				if (current != mat)
				{
					enabled = true;
					current = mat;
					if (current != null && current.shader != null)
					{
						shaderName = current.shader.name;
						var list = MaterialData.Instance.ShaderNames;
						for (int i = list.Length - 1; i >= 0; --i)
						{
							if (list[i] == shaderName)
							{
								shaderIndex = ShaderListPopup.SelectedIndex = i;
								break;
							}
						}
					}
				}
				else
				{
					enabled = false;
				}
			}

			return mat;
		}

		private void Start()
		{
			if (runtimeHierarchy == null)
			{
				windowID = 50220;
			}

			colorViewer = gameObject.GetComponent<ColorViewer>();
			if (colorViewer == null) colorViewer = gameObject.AddComponent<ColorViewer>();
			colorViewer.windowID = windowID;
			colorViewer.enabled = false;
			textureViewer = gameObject.GetComponent<TextureViewer>();
			if (textureViewer == null) textureViewer = gameObject.AddComponent<TextureViewer>();
			textureViewer.windowID = windowID;
			textureViewer.enabled = false;

			windowPos.width = Mathf.Clamp((float)Screen.width * 2f / 3f, 300f, Screen.width - 30f);
			windowPos.height = Mathf.Clamp(windowPos.height, 300f, Screen.height - 30f);
		}

		private void OnDisable()
		{
			current = null;
			propertyDatas = null;
		}

		private void OnGUI()
		{
			if (current != null)
			{
				windowPos = GUI.Window(windowID, windowPos, WindowMaterial, current.name, Utility.DarkWindowStyle);

				if (runtimeHierarchy == null)
				{
					int id = windowID;

					int lastID = id;
					int focusedID = id;

					if (colorViewer.enabled)
						colorViewer.windowID = focusedID = ++id;
					if (textureViewer.enabled)
						textureViewer.windowID = focusedID = ++id;
					if (lastID != focusedID)
						GUI.BringWindowToFront(focusedID);
				}
			}
		}

		void WindowMaterial(int id)
		{
			Rect closeButtonRect = new Rect(15f, windowPos.height - 15f - 25f, windowPos.width - 30f, 25f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Shader : ", Utility.YelloLabelStyle, GUILayout.Width(150f));
			GUILayout.Label(string.IsNullOrEmpty(shaderName) ? "No shader selected!" : shaderName, Utility.YelloLabelStyle);
			var nextShaderIndex = ShaderListPopup.DrawPopup(shaderIndex);
			if (nextShaderIndex != shaderIndex)
			{
				shaderIndex = nextShaderIndex;
				current.shader = Shader.Find(shaderName = MaterialData.Instance.ShaderNames[shaderIndex]);
				propertyDatas = null;
			}

			GUILayout.EndHorizontal();

			if (propertyDatas == null || propertyDatas.Length == 0)
			{
				if (MaterialData.Instance != null)
				{
					propertyDatas = MaterialData.Instance.GetPropertyData(current);
				}
			}

			if (propertyDatas != null)
			{
				float height = windowPos.height - Utility.buttonHeight - Utility.buttonHeight - 45f;
				scrollViewPos = Utility.BeginScrollView(scrollViewPos, GUILayout.Height(height));

				for (int i = 0, max = propertyDatas.Length; i < max; ++i)
				{
					var prop = propertyDatas[i];
					GUILayout.BeginHorizontal();
					GUILayout.Label(prop.propertyName, Utility.WhiteLabelStyle, GUILayout.Width(200f));

					switch (prop.propertyType)
					{
						case MaterialData.PropertyType.Color:
							var color = current.GetColor(prop.propertyId);
							var c_next = colorViewer.DrawColorField(color, prop.propertyName);
							if (c_next != color)
							{
								current.SetColor(prop.propertyId, c_next);
							}
							break;
						case MaterialData.PropertyType.Float:
							var value = current.GetFloat(prop.propertyId);
							var f_next = Utility.FloatField(value);
							if (f_next != value)
							{
								current.SetFloat(prop.propertyId, f_next);
							}
							break;
						case MaterialData.PropertyType.Texture:
							var tex = current.GetTexture(prop.propertyId);
							var t_next = textureViewer.DrawTextureField(tex);
							if (t_next != tex)
							{
								current.SetTexture(prop.propertyId, t_next);
							}
							break;
						case MaterialData.PropertyType.Vector:
							var vector = current.GetVector(prop.propertyId);
							var v_next = Utility.DrawVector4(vector);
							if (vector != v_next)
							{
								current.SetVector(prop.propertyId, v_next);
							}
							break;
					}
					GUILayout.EndHorizontal();
				}

				GUILayout.EndScrollView();
			}

			if (GUI.Button(closeButtonRect, "Close", Utility.ButtonStyle))
			{
				current = null;
				propertyDatas = null;
			}
			GUI.DragWindow();
		}
	}
}