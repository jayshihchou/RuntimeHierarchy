using System;
using UnityEngine;

namespace RHierarchy
{
	public class ColorViewer : MonoBehaviour
	{
		public int windowID;

		public Color DrawColorField(Color color, string colorName)
		{
			GUILayout.Label(color.ToString(), Utility.YelloLabelStyle, GUILayout.Width(250f));

			Rect position = GUILayoutUtility.GetRect(GUIContent.none, Utility.ColorStyle);
			Rect alphaPos = position;
			alphaPos.y += position.height * 0.8f;
			alphaPos.height *= 0.2f;

			if (Event.current.type == EventType.Repaint)
			{
				Color oldColor = GUI.color;
				Color guiColor = color;
				guiColor.a = 1.0f;
				GUI.color = guiColor;
				GUI.DrawTexture(position, Texture2D.whiteTexture);
				guiColor = Color.white;
				guiColor *= color.a;
				guiColor.a = 1.0f;
				GUI.color = guiColor;
				GUI.DrawTexture(alphaPos, Texture2D.whiteTexture);
				GUI.color = oldColor;

				Rect text = position;
				text.y -= text.height * 0.2f;
				text.height = position.height * 2f;
				if (color.g > 0f)
					GUI.Label(text, "(Click To Change)", Utility.BlackLabelStyle);
				else
					GUI.Label(text, "(Click To Change)", Utility.LabelStyle);
			}

			if (GUI.Button(position, "", Utility.YelloLabelStyle))
			{
				if (string.IsNullOrEmpty(currentName) || currentName != colorName)
				{
					if (currentName != colorName)
					{
						currentName = colorName;
						enabled = true;
						SetResultColor(color);
					}
					else
						enabled = false;
				}
			}

			if (currentName == colorName)
				return Utility.Color32ToColor(resultColor);

			return color;
		}

		private void OnDisable()
		{
			currentName = string.Empty;
		}

		private void OnGUI()
		{
			if (!string.IsNullOrEmpty(currentName))
			{
				var next = GUI.Window(windowID, colorWindowPos, ColorFieldWindow, currentName, Utility.GrayWindowStyle);
				if (next.x != colorWindowPos.x || next.y != colorWindowPos.y)
				{
					colorWindowPos = next;
				}
			}
		}
		string currentName;

		private void OnApplicationPause(bool pause)
		{
			colorFieldMouseType = MouseOrTouchType.None;
			colorFieldType = SelectingFieldType.None;
		}

		/// <summary>
		/// color window position.
		/// </summary>
		Rect colorWindowPos = new Rect(15, 15, 300, 500);

		enum SelectingFieldType
		{
			None, ColorField, LineField, Slider1, Slider2, Slider3, Slider4, Dragging
		}

		enum MouseOrTouchType
		{
			Start, Pressing, None
		}

		float colorFieldSliderValue1 = 255.0f;
		string colorFieldSlider1Str = "255";

		float colorFieldSliderValue2 = 0f;
		string colorFieldSlider2Str = "0";

		float colorFieldSliderValue3 = 0f;
		string colorFieldSlider3Str = "0";

		float colorFieldSliderValue4 = 255f;
		string colorFieldSlider4Str = "255";

		float colorFieldInputX = 1.0f, colorFieldInputY = 1.0f;
		float colorFieldLineY = 1.0f;

		SelectingFieldType colorFieldType = SelectingFieldType.None;
		MouseOrTouchType colorFieldMouseType = MouseOrTouchType.None;

		static Color32 resultColor = new Color32(255, 0, 0, 255);
		[NonSerialized] static Color32 currentColorMax = new Color32(255, 0, 0, 255);

		static Texture2D colorFieldTexture = null;
		static Texture2D ColorFieldTexture
		{
			get
			{
				if (colorFieldTexture == null)
				{
					colorFieldTexture = new Texture2D(2, 2) { wrapMode = TextureWrapMode.Clamp };
					var colours = colorFieldTexture.GetPixels32();
					colours[2] = new Color32(255, 255, 255, 255);
					colours[3] = currentColorMax;
					colours[0] = new Color32(0, 0, 0, 255);
					colours[1] = new Color32(0, 0, 0, 255);
					colorFieldTexture.SetPixels32(colours);
					colorFieldTexture.Apply(true);
				}
				return colorFieldTexture;
			}
		}

		static Texture2D lineColorFieldTexture = null;
		static Texture2D LineColorFieldTexture
		{
			get
			{
				if (lineColorFieldTexture == null)
				{
					lineColorFieldTexture = new Texture2D(1, 7);
					var colours = lineColorFieldTexture.GetPixels32();
					colours[0] = new Color32(255, 0, 0, 255);
					colours[1] = new Color32(255, 255, 0, 255);
					colours[2] = new Color32(0, 255, 0, 255);
					colours[3] = new Color32(0, 255, 255, 255);
					colours[4] = new Color32(0, 0, 255, 255);
					colours[5] = new Color32(255, 0, 255, 255);
					colours[6] = new Color32(255, 0, 0, 255);
					lineColorFieldTexture.SetPixels32(colours);
					lineColorFieldTexture.Apply(true);
				}
				return lineColorFieldTexture;
			}
		}

		static Texture2D redColorFieldSliderTexture = null;
		static Texture2D RedColorFieldSliderTexture
		{
			get
			{
				if (redColorFieldSliderTexture == null)
				{
					redColorFieldSliderTexture = new Texture2D(2, 1) { wrapMode = TextureWrapMode.Clamp };
					var colours = redColorFieldSliderTexture.GetPixels32();
					colours[1] = new Color32(255, resultColor.g, resultColor.b, 255);
					colours[0] = new Color32(0, resultColor.g, resultColor.b, 255);
					redColorFieldSliderTexture.SetPixels32(colours);
					redColorFieldSliderTexture.Apply(true);
				}
				return redColorFieldSliderTexture;
			}
		}

		static Texture2D greenColorFieldSliderTexture = null;
		static Texture2D GreenColorFieldSliderTexture
		{
			get
			{
				if (greenColorFieldSliderTexture == null)
				{
					greenColorFieldSliderTexture = new Texture2D(2, 1) { wrapMode = TextureWrapMode.Clamp };
					var colours = greenColorFieldSliderTexture.GetPixels32();
					colours[1] = new Color32(resultColor.r, 255, resultColor.b, 255);
					colours[0] = new Color32(resultColor.r, 0, resultColor.b, 255);
					greenColorFieldSliderTexture.SetPixels32(colours);
					greenColorFieldSliderTexture.Apply(true);
				}
				return greenColorFieldSliderTexture;
			}
		}

		static Texture2D blueColorFieldSliderTexture = null;
		static Texture2D BlueColorFieldSliderTexture
		{
			get
			{
				if (blueColorFieldSliderTexture == null)
				{
					blueColorFieldSliderTexture = new Texture2D(2, 1) { wrapMode = TextureWrapMode.Clamp };
					var colours = blueColorFieldSliderTexture.GetPixels32();
					colours[1] = new Color32(resultColor.r, resultColor.g, 255, 255);
					colours[0] = new Color32(resultColor.r, resultColor.g, 0, 255);
					blueColorFieldSliderTexture.SetPixels32(colours);
					blueColorFieldSliderTexture.Apply(true);
				}
				return blueColorFieldSliderTexture;
			}
		}

		static Texture2D alphaColorFieldSliderTexture = null;
		static Texture2D AlphaColorFieldSliderTexture
		{
			get
			{
				if (alphaColorFieldSliderTexture == null)
				{
					alphaColorFieldSliderTexture = new Texture2D(2, 1) { wrapMode = TextureWrapMode.Clamp };
					var colours = alphaColorFieldSliderTexture.GetPixels32();
					colours[1] = new Color32(255, 255, 255, 255);
					colours[0] = new Color32(0, 0, 0, 255);
					alphaColorFieldSliderTexture.SetPixels32(colours);
					alphaColorFieldSliderTexture.Apply(true);
				}
				return alphaColorFieldSliderTexture;
			}
		}

		static GUIStyle colorFieldSliderStyle = null;
		static GUIStyle ColorFieldSlider1Style
		{
			get
			{
				if (colorFieldSliderStyle == null)
				{
					colorFieldSliderStyle = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
				}
				colorFieldSliderStyle.normal.background = RedColorFieldSliderTexture;
				return colorFieldSliderStyle;
			}
		}

		static GUIStyle ColorFieldSlider2Style
		{
			get
			{
				if (colorFieldSliderStyle == null)
				{
					colorFieldSliderStyle = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
				}
				colorFieldSliderStyle.normal.background = GreenColorFieldSliderTexture;
				return colorFieldSliderStyle;
			}
		}

		static GUIStyle ColorFieldSlider3Style
		{
			get
			{
				if (colorFieldSliderStyle == null)
				{
					colorFieldSliderStyle = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
				}
				colorFieldSliderStyle.normal.background = BlueColorFieldSliderTexture;
				return colorFieldSliderStyle;
			}
		}

		static GUIStyle ColorFieldSlider4Style
		{
			get
			{
				if (colorFieldSliderStyle == null)
				{
					colorFieldSliderStyle = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
				}
				colorFieldSliderStyle.normal.background = AlphaColorFieldSliderTexture;
				return colorFieldSliderStyle;
			}
		}

		static GUIStyle colorFieldThumbStyle = null;
		static GUIStyle ColorFieldThumbStyle
		{
			get
			{
				if (colorFieldThumbStyle == null)
				{
					colorFieldThumbStyle = new GUIStyle();
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(0, 0, 0, 0);
					tex.SetPixels32(colours);
					colorFieldThumbStyle.normal.background = colorFieldThumbStyle.active.background = colorFieldThumbStyle.hover.background = colorFieldThumbStyle.focused.background = tex;
				}
				return colorFieldThumbStyle;
			}
		}

		static GUIStyle colorFieldIStyle = null;
		static GUIStyle ColorFieldIStyle
		{
			get
			{
				if (colorFieldIStyle == null)
				{
					colorFieldIStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 26 };
					colorFieldIStyle.normal.textColor = Color.black;
				}

				return colorFieldIStyle;
			}
		}

		static bool NumberOnly(char c)
		{
			return c >= '0' && c <= '9';
		}

		static bool IsFloat(string str)
		{
			int dotCount = 0;
			for (int i = str.Length - 1; i >= 0; --i)
			{
				if (!NumberOnly(str[i]) && str[i] != '.')
				{
					return false;
				}
				if (str[i] == '.')
				{
					if (++dotCount > 1)
						return false;
				}
			}
			return true;
		}

		void SetResultColor(Color color)
		{
			resultColor = color;
			SetResultColor();
		}

		void ResetColors()
		{
			if (colorFieldTexture != null) DestroyImmediate(colorFieldTexture);
			colorFieldTexture = null;

			resultColor = ColorFieldColor(colorFieldInputX, colorFieldInputY);

			ResetSubColors();
		}

		void SetResultColor()
		{
			if (colorFieldTexture != null) DestroyImmediate(colorFieldTexture);
			colorFieldTexture = null;

			colorFieldLineY = GetLineColorToY();

			ColorFieldColorToCoord(out colorFieldInputX, out colorFieldInputY);

			ResetSubColors();
		}

		void ResetSubColors()
		{
			colorFieldSliderValue1 = resultColor.r;
			colorFieldSlider1Str = ((int)colorFieldSliderValue1).ToString();
			colorFieldSliderValue2 = resultColor.g;
			colorFieldSlider2Str = ((int)colorFieldSliderValue2).ToString();
			colorFieldSliderValue3 = resultColor.b;
			colorFieldSlider3Str = ((int)colorFieldSliderValue3).ToString();
			colorFieldSliderValue4 = resultColor.a;
			colorFieldSlider4Str = ((int)colorFieldSliderValue4).ToString();

			if (redColorFieldSliderTexture != null) DestroyImmediate(redColorFieldSliderTexture);
			redColorFieldSliderTexture = null;
			if (greenColorFieldSliderTexture != null) DestroyImmediate(greenColorFieldSliderTexture);
			greenColorFieldSliderTexture = null;
			if (blueColorFieldSliderTexture != null) DestroyImmediate(blueColorFieldSliderTexture);
			blueColorFieldSliderTexture = null;
		}

		void ColorFieldColorToCoord(out float x, out float y)
		{
			x = y = 0f;

			if (resultColor.r == resultColor.g && resultColor.r == resultColor.b)
			{
				x = 0f;
			}
			else if (resultColor.r <= resultColor.g && resultColor.r <= resultColor.b)
			{
				x = 1f - ((float)resultColor.r / 255f);
			}
			else if (resultColor.g <= resultColor.r && resultColor.g <= resultColor.b)
			{
				x = 1f - ((float)resultColor.g / 255f);
			}
			else if (resultColor.b <= resultColor.r && resultColor.b <= resultColor.g)
			{
				x = 1f - ((float)resultColor.b / 255f);
			}

			if (currentColorMax.r != 255)
			{
				if (currentColorMax.b == 255)
					y = (float)resultColor.b / 255f;
				else if (currentColorMax.g == 255)
					y = (float)resultColor.g / 255f;
				else
					y = resultColor.r;
			}
			else if (currentColorMax.g != 255)
			{
				if (currentColorMax.b == 255)
					y = (float)resultColor.b / 255f;
				else if (currentColorMax.r == 255)
					y = (float)resultColor.r / 255f;
				else
					y = resultColor.g;
			}
			else if (currentColorMax.b != 255)
			{
				if (currentColorMax.g == 255)
					y = (float)resultColor.g / 255f;
				else if (currentColorMax.r == 255)
					y = (float)resultColor.r / 255f;
				else
					y = resultColor.b;
			}
		}

		Color32 ColorFieldColor(float x, float y)
		{
			Color currMin = new Color((float)currentColorMax.r / 255f, (float)currentColorMax.g / 255f, (float)currentColorMax.b / 255f, (float)currentColorMax.a / 255f);

			currMin = Color.Lerp(currMin, Color.white, 1f - x);

			currMin = Color.Lerp(Color.black, currMin, y);

			return new Color32((byte)(currMin.r * 255f), (byte)(currMin.g * 255f), (byte)(currMin.b * 255f), resultColor.a);
		}

		Color LineColorAt(int index)
		{
			switch (index)
			{
				case 1: return new Color(1.0f, 1.0f, 0.0f, 1.0f);
				case 2: return Color.green;
				case 3: return Color.cyan;
				case 4: return Color.blue;
				case 5: return Color.magenta;
			}
			return Color.red;
		}

		int ColorToIndex(Color32 color)
		{
			if (color.r == 255 && color.g == 0)
				return 0;
			else if (color.g == 0 && color.b == 255)
				return 1;
			else if (color.r == 0 && color.b == 255)
				return 2;
			else if (color.r == 0 && color.g == 255)
				return 3;
			else if (color.g == 255 && color.b == 0)
				return 4;
			else if (color.r == 255 && color.b == 0)
				return 5;

			return 6;
		}

		Color32 SetCurrentMax(Color32 color)
		{
			if (color.r == color.g && color.r == color.b && color.b != 0)
			{
				return new Color32(255, 0, 0, 255);
			}
			if (color.r == 255)
			{
				if (color.g > color.b)
				{
					return new Color32(255, color.g, 0, 255);
				}
				return new Color32(255, 0, color.b, 255);
			}
			if (color.g == 255)
			{
				if (color.r > color.b)
				{
					return new Color32(color.r, 255, 0, 255);
				}
				return new Color32(0, 255, color.b, 255);
			}
			if (color.r > color.g)
			{
				return new Color32(color.r, 0, color.b, 255);
			}
			return new Color32(0, color.g, 255, 255);
		}

		Color32 GetColorLine(float value)
		{
			value = (value) * 6f;
			int ind = (int)value;
			float frac = value - ind;
			Color from = LineColorAt(ind);
			Color to = LineColorAt(ind + 1);

			Color result = Color.Lerp(from, to, frac);

			return SetCurrentMax(new Color32((byte)(result.r * 255f), (byte)(result.g * 255f), (byte)(result.b * 255f), (byte)(result.a * 255f)));
		}

		Color32 GetCurrentColorMax(Color32 color)
		{
			if (color.r >= color.g && color.r >= color.b)
			{
				return SetCurrentMax(new Color32(255, color.g, color.b, 255));
			}
			else if (color.g >= color.r && color.g >= color.b)
			{
				return SetCurrentMax(new Color32(color.r, 255, color.b, 255));
			}
			else if (color.b >= color.r && color.b >= color.g)
			{
				return SetCurrentMax(new Color32(color.r, color.g, 255, 255));
			}
			return new Color32(255, 0, 0, 255);
		}

		float GetLineColorToY()
		{
			currentColorMax = GetCurrentColorMax(resultColor);

			int index = ColorToIndex(currentColorMax);
			float each = 255f / 6f;

			float result = 0f;

			switch (index)
			{
				case 1:
					result = 1f - ((float)currentColorMax.r / 255f);
					break;
				case 2:
					result = (float)currentColorMax.g / 255f;
					break;
				case 3:
					result = 1f - ((float)currentColorMax.b / 255f);
					break;
				case 4:
					result = (float)currentColorMax.r / 255f;
					break;
				case 5:
					result = 1f - ((float)currentColorMax.g / 255f);
					break;
				default:
					result = (float)currentColorMax.b / 255f;
					break;
			}

			return 1f - (((result * each) + (each * index)) / 255f);
		}

		Color DrawColorRect(Rect rect, Color color)
		{
			if (Event.current.type == EventType.Repaint)
			{
				Color oldColor = GUI.color;
				GUI.color = color;
				GUI.DrawTexture(rect, Texture2D.whiteTexture);
				GUI.color = oldColor;
			}
			return color;
		}

		/// <summary>
		/// Draw slider.
		/// </summary>
		void Slider(Rect rect, ref float val, ref string valStr, GUIStyle style)
		{
			var value1 = GUI.HorizontalSlider(rect, val, 0f, 255f, style, ColorFieldThumbStyle);
			if (value1 != val)
			{
				val = value1;
				valStr = ((int)val).ToString();
			}

			Rect slider1label = new Rect(15f + colorWindowPos.width - 60f + 7f, rect.y, 30f, 25f);
			valStr = GUI.TextField(slider1label, valStr, Utility.TextFieldStyle);
			if (!IsFloat(valStr))
			{
				valStr = ((int)val).ToString();
			}
			else
			{
				float val1;
				if (float.TryParse(valStr, out val1))
				{
					val = val1;
				}
			}

			val = Mathf.Clamp(val, 0f, 255f);

			DrawColorRect(new Rect(15f + Mathf.Lerp(0f, colorWindowPos.width - 70f, val / 255f),
				rect.y, 10f, 27f), Color.black);
		}

		/// <summary>
		/// Convert screen space back to gui space.
		/// </summary>
		Vector2 ToRectSpace(Rect rect, Vector2 pos)
		{
			return new Vector2(pos.x - rect.x, pos.y - rect.y);
		}

		/// <summary>
		/// Convert rect from gui space to screen space for capturing.
		/// </summary>
		Rect ConvertToScreenSpace(Rect rect)
		{
			return new Rect(rect.x, Mathf.CeilToInt(Screen.height - rect.y - rect.height), rect.width, rect.height);
		}

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
		static int currentTouchID = -1;
#endif

		static Vector3 MouseOrTouch
		{
			get
			{
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
				if (currentTouchID != -1)
				{
					var touch = Input.GetTouch(currentTouchID);
					return new Vector3(touch.position.x, (float)Screen.height - touch.position.y);
				}
				return new Vector3();
#else
				return new Vector3(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
#endif
			}
		}

		static MouseOrTouchType CurrentInputType
		{
			get
			{
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
				for (int i = 0; i < Input.touchCount; ++i)
				{
					var touch = Input.GetTouch(i);
					switch (touch.phase)
					{
						case TouchPhase.Began:
							currentTouchID = i;
							return MouseOrTouchType.Start;
						case TouchPhase.Moved:
						case TouchPhase.Stationary:
							currentTouchID = i;
							return MouseOrTouchType.Pressing;
					}
				}
				currentTouchID = -1;
#else

				if (Input.GetMouseButtonDown(0))
				{
					return MouseOrTouchType.Start;
				}
				else if (Input.GetMouseButtonUp(0))
				{
					return MouseOrTouchType.None;
				}
				else if (Input.GetMouseButton(0))
				{
					return MouseOrTouchType.Pressing;
				}
#endif
				return MouseOrTouchType.None;
			}
		}

		/// <summary>
		/// Process rect input event.
		/// </summary>
		MouseOrTouchType ProcessInput(Rect parent, Rect rect, out float x, out float y, MouseOrTouchType type)
		{
			rect.x += parent.x;
			rect.y += parent.y - 4;
			x = 0f;
			y = 0f;

			var currType = CurrentInputType;

			if (currType != MouseOrTouchType.None)
			{
				var mPos = MouseOrTouch;
				if (type == MouseOrTouchType.Pressing || rect.Contains(MouseOrTouch))
				{
					type = currType;
					var pos = ToRectSpace(rect, mPos);
					x = Mathf.Clamp01(pos.x / (rect.width - 4f));
					y = 1.0f - Mathf.Clamp01(pos.y / (rect.height - 4f));
				}
				else type = MouseOrTouchType.None;
			}
			else type = MouseOrTouchType.None;

			return type;
		}

		/// <summary>
		/// Main GUI.
		/// </summary>
		void ColorFieldWindow(int id)
		{
			Rect mainRect = new Rect(10f, 20f, colorWindowPos.height * 0.5f, colorWindowPos.height * 0.5f);
			GUI.DrawTexture(mainRect, ColorFieldTexture);

			float x = 0f, y = 0f;
			if (colorFieldType == SelectingFieldType.None || colorFieldType == SelectingFieldType.ColorField)
			{
				if ((colorFieldMouseType = ProcessInput(colorWindowPos, mainRect, out x, out y, colorFieldMouseType)) != MouseOrTouchType.None)
				{
					resultColor = ColorFieldColor(colorFieldInputX = x, colorFieldInputY = y);
					ResetSubColors();
					if (colorFieldMouseType == MouseOrTouchType.Start)
					{
						colorFieldType = SelectingFieldType.ColorField;
					}
				}
				else if (colorFieldType == SelectingFieldType.ColorField) colorFieldType = SelectingFieldType.None;
			}

			Rect circle = new Rect(mainRect.x + (colorFieldInputX * (mainRect.height - 11f)), -4f + mainRect.y + ((1f - colorFieldInputY) * (mainRect.height - 15f)), 15f, 20f);
			GUI.Label(circle, "O");

			Rect lineRect = new Rect(colorWindowPos.height * 0.5f + 15f, 20f, 20f, colorWindowPos.height * 0.5f);
			GUI.DrawTexture(lineRect, LineColorFieldTexture);

			if (colorFieldType == SelectingFieldType.None || colorFieldType == SelectingFieldType.LineField)
			{
				if ((colorFieldMouseType = ProcessInput(colorWindowPos, lineRect, out x, out y, colorFieldMouseType)) != MouseOrTouchType.None)
				{
					currentColorMax = GetColorLine(colorFieldLineY = y);
					ResetColors();
					if (colorFieldMouseType == MouseOrTouchType.Start)
					{
						colorFieldType = SelectingFieldType.LineField;
					}
				}
				else if (colorFieldType == SelectingFieldType.LineField) colorFieldType = SelectingFieldType.None;
			}

			DrawColorRect(new Rect(colorWindowPos.height * 0.5f + 15f, 20f + Mathf.Lerp(0f, colorWindowPos.height * 0.5f - 10f, 1f - colorFieldLineY), 27f, 10f), Color.black);

			Rect slider1 = new Rect(15f, colorWindowPos.height * 0.5f + 35f, colorWindowPos.width - 60f, 25f);
			Slider(slider1, ref colorFieldSliderValue1, ref colorFieldSlider1Str, ColorFieldSlider1Style);

			if (colorFieldType == SelectingFieldType.None || colorFieldType == SelectingFieldType.Slider1)
			{
				if ((colorFieldMouseType = ProcessInput(colorWindowPos, slider1, out x, out y, colorFieldMouseType)) != MouseOrTouchType.None)
				{
					resultColor.r = (byte)(x * 255f);
					SetResultColor();
					if (colorFieldMouseType == MouseOrTouchType.Start)
					{
						colorFieldType = SelectingFieldType.Slider1;
					}
				}
				else if (colorFieldType == SelectingFieldType.Slider1) colorFieldType = SelectingFieldType.None;
			}

			Rect slider2 = new Rect(15f, colorWindowPos.height * 0.5f + 35f + 35f, colorWindowPos.width - 60f, 25f);
			Slider(slider2, ref colorFieldSliderValue2, ref colorFieldSlider2Str, ColorFieldSlider2Style);

			if (colorFieldType == SelectingFieldType.None || colorFieldType == SelectingFieldType.Slider2)
			{
				if ((colorFieldMouseType = ProcessInput(colorWindowPos, slider2, out x, out y, colorFieldMouseType)) != MouseOrTouchType.None)
				{
					resultColor.g = (byte)(x * 255f);
					SetResultColor();
					if (colorFieldMouseType == MouseOrTouchType.Start)
					{
						colorFieldType = SelectingFieldType.Slider2;
					}
				}
				else if (colorFieldType == SelectingFieldType.Slider2) colorFieldType = SelectingFieldType.None;
			}

			Rect slider3 = new Rect(15f, colorWindowPos.height * 0.5f + 35f + 35f + 35f, colorWindowPos.width - 60f, 25f);
			Slider(slider3, ref colorFieldSliderValue3, ref colorFieldSlider3Str, ColorFieldSlider3Style);

			if (colorFieldType == SelectingFieldType.None || colorFieldType == SelectingFieldType.Slider3)
			{
				if ((colorFieldMouseType = ProcessInput(colorWindowPos, slider3, out x, out y, colorFieldMouseType)) != MouseOrTouchType.None)
				{
					resultColor.b = (byte)(x * 255f);
					SetResultColor();
					if (colorFieldMouseType == MouseOrTouchType.Start)
					{
						colorFieldType = SelectingFieldType.Slider3;
					}
				}
				else if (colorFieldType == SelectingFieldType.Slider3) colorFieldType = SelectingFieldType.None;
			}

			Rect slider4 = new Rect(15f, colorWindowPos.height * 0.5f + 35f + 35f + 35f + 35f, colorWindowPos.width - 60f, 25f);
			Slider(slider4, ref colorFieldSliderValue4, ref colorFieldSlider4Str, ColorFieldSlider4Style);

			if (colorFieldType == SelectingFieldType.None || colorFieldType == SelectingFieldType.Slider4)
			{
				if ((colorFieldMouseType = ProcessInput(colorWindowPos, slider4, out x, out y, colorFieldMouseType)) != MouseOrTouchType.None)
				{
					resultColor.a = (byte)(x * 255f);
					ResetSubColors();
					if (colorFieldMouseType == MouseOrTouchType.Start)
					{
						colorFieldType = SelectingFieldType.Slider4;
					}
				}
				else if (colorFieldType == SelectingFieldType.Slider4) colorFieldType = SelectingFieldType.None;
			}

			Rect closeButtonRect = new Rect(15f, colorWindowPos.height - 15f - 25f, colorWindowPos.width - 30f, 25f);

			if (GUI.Button(closeButtonRect, "Close", Utility.ButtonStyle))
			{
				currentName = string.Empty;
				enabled = false;
			}

			if (colorFieldType == SelectingFieldType.None || colorFieldType == SelectingFieldType.Dragging)
			{
				if ((colorFieldMouseType = ProcessInput(new Rect(), colorWindowPos, out x, out y, colorFieldMouseType)) != MouseOrTouchType.None)
				{
					if (colorFieldMouseType == MouseOrTouchType.Start)
					{
						colorFieldType = SelectingFieldType.Dragging;
					}
				}
				else if (colorFieldType == SelectingFieldType.Dragging) colorFieldType = SelectingFieldType.None;

				GUI.DragWindow();
			}
		}
	}
}