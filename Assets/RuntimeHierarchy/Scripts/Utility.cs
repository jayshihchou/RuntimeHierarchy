using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RHierarchy
{
	public class Utility
	{
		static bool debugFlag = false;
		public static bool DebugFlag { get { return debugFlag; } }
		public static void SetDebugFlag(bool flag)
		{
			debugFlag = flag;
		}

		public static float buttonWidth = 300f;
		public static float buttonHeight = 50f;
		public static float smallFieldWidth = 200f;
		static readonly string[] xyzw = new string[] { "X", "Y", "Z", "W" };
		static Dictionary<Type, DrawData> drawDatas = new Dictionary<Type, DrawData>();
		static Dictionary<Type, DrawData> staticDrawDatas = new Dictionary<Type, DrawData>();

		/// <summary>
		/// Creates a 1x1 texture in colour.
		/// </summary>
		static Texture2D CreateTextureInColour(Color32 colour)
		{
#if UNITY_2018 || UNITY_2018_1_OR_NEWER
			var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
#else
			var tex = new Texture2D(1, 1, TextureFormat.ARGB4444, false);
#endif
			tex.SetPixels32(new Color32[] { colour });
			tex.Apply(true);
			return tex;
		}

		static GUIStyle labelStyle = null;
		/// <summary>
		/// Style for label.
		/// </summary>
		public static GUIStyle LabelStyle
		{
			get
			{
				if (labelStyle == null)
				{
					labelStyle = new GUIStyle(GUI.skin.label);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					labelStyle.fontSize = 24;
#endif
				}
				return labelStyle;
			}
		}

		static GUIStyle yellowLabelStyle = null;
		/// <summary>
		/// Style for yellow label.
		/// </summary>
		public static GUIStyle YelloLabelStyle
		{
			get
			{
				if (yellowLabelStyle == null)
				{
					yellowLabelStyle = new GUIStyle(GUI.skin.label);
					yellowLabelStyle.normal.textColor = new Color(1f, 0.6f, 0f, 1f);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					yellowLabelStyle.fontSize = 24;
#endif
				}
				return yellowLabelStyle;
			}
		}

		static GUIStyle backgrounedWhiteLabelStyle = null;
		/// <summary>
		/// Style for white label.
		/// </summary>
		public static GUIStyle BackgroundedWhiteLabelStyle
		{
			get
			{
				if (backgrounedWhiteLabelStyle == null)
				{
					backgrounedWhiteLabelStyle = new GUIStyle(GUI.skin.label);
					backgrounedWhiteLabelStyle.normal.background = backgrounedWhiteLabelStyle.onNormal.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					backgrounedWhiteLabelStyle.hover.background = backgrounedWhiteLabelStyle.onHover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					backgrounedWhiteLabelStyle.normal.textColor = Color.white;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					backgrounedWhiteLabelStyle.fontSize = 24;
#endif
				}
				if (backgrounedWhiteLabelStyle.normal.background == null)
				{
					backgrounedWhiteLabelStyle.normal.background = backgrounedWhiteLabelStyle.onNormal.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					backgrounedWhiteLabelStyle.hover.background = backgrounedWhiteLabelStyle.onHover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
				}
				return backgrounedWhiteLabelStyle;
			}
		}

		static GUIStyle whiteLabelStyle = null;
		/// <summary>
		/// Style for white label.
		/// </summary>
		public static GUIStyle WhiteLabelStyle
		{
			get
			{
				if (whiteLabelStyle == null)
				{
					whiteLabelStyle = new GUIStyle(GUI.skin.label);
					whiteLabelStyle.normal.textColor = Color.white;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					whiteLabelStyle.fontSize = 24;
#endif
				}
				return whiteLabelStyle;
			}
		}

		static GUIStyle blackLabelStyle = null;
		/// <summary>
		/// Style for black label.
		/// </summary>
		public static GUIStyle BlackLabelStyle
		{
			get
			{
				if (blackLabelStyle == null)
				{
					blackLabelStyle = new GUIStyle(GUI.skin.label);
					blackLabelStyle.normal.background = CreateTextureInColour(new Color32(0, 0, 0, 0));
					blackLabelStyle.normal.textColor = new Color(0f, 0f, 0f, 1f);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					blackLabelStyle.fontSize = 24;
#endif
				}
				if (blackLabelStyle.normal.background == null)
				{
					blackLabelStyle.normal.background = CreateTextureInColour(new Color32(0, 0, 0, 0));
				}
				return blackLabelStyle;
			}
		}

		static GUIStyle textFieldStyle = null;
		/// <summary>
		/// Style for text field.
		/// </summary>
		public static GUIStyle TextFieldStyle
		{
			get
			{
				if (textFieldStyle == null)
				{
					textFieldStyle = new GUIStyle(GUI.skin.textField);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					textFieldStyle.fontSize = 24;
#endif
				}
				return textFieldStyle;
			}
		}

		static GUIStyle textAreaStyle = null;
		/// <summary>
		/// Style for text field.
		/// </summary>
		public static GUIStyle TextAreaStyle
		{
			get
			{
				if (textAreaStyle == null)
				{
					textAreaStyle = new GUIStyle(GUI.skin.textField);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					textAreaStyle.fontSize = 24;
#endif
				}
				return textAreaStyle;
			}
		}

		static GUIStyle toggleStyle = null;
		/// <summary>
		/// Style for text field.
		/// </summary>
		public static GUIStyle ToggleStyle
		{
			get
			{
				if (toggleStyle == null)
				{
					toggleStyle = new GUIStyle(GUI.skin.toggle);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					toggleStyle.fontSize = 24;
#endif
				}
				return toggleStyle;
			}
		}

		static GUIStyle windowStyle = null;
		/// <summary>
		/// Style for window.
		/// </summary>
		public static GUIStyle WindowStyle
		{
			get
			{
				if (windowStyle == null)
				{
					windowStyle = new GUIStyle(GUI.skin.window);
					windowStyle.normal.textColor = windowStyle.active.textColor = windowStyle.hover.textColor = windowStyle.focused.textColor =
						windowStyle.onNormal.textColor = windowStyle.onActive.textColor = windowStyle.onHover.textColor = windowStyle.onFocused.textColor =
						Color.black;
					windowStyle.normal.background =
					windowStyle.onNormal.background =
					windowStyle.hover.background =
					windowStyle.onHover.background =
					windowStyle.active.background =
					windowStyle.onActive.background =
					windowStyle.focused.background =
					windowStyle.onFocused.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					windowStyle.fontSize = 24;
#endif
				}
				if (windowStyle.normal.background == null)
				{
					windowStyle.normal.background =
					windowStyle.onNormal.background =
					windowStyle.hover.background =
					windowStyle.onHover.background =
					windowStyle.active.background =
					windowStyle.onActive.background =
					windowStyle.focused.background =
					windowStyle.onFocused.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
				}
					return windowStyle;
			}
		}

		static GUIStyle darkWindowStyle = null;
		/// <summary>
		/// Style for window.
		/// </summary>
		public static GUIStyle DarkWindowStyle
		{
			get
			{
				if (darkWindowStyle == null)
				{
					darkWindowStyle = new GUIStyle(GUI.skin.window);
					darkWindowStyle.normal.textColor = darkWindowStyle.active.textColor = darkWindowStyle.hover.textColor = darkWindowStyle.focused.textColor =
						darkWindowStyle.onNormal.textColor = darkWindowStyle.onActive.textColor = darkWindowStyle.onHover.textColor = darkWindowStyle.onFocused.textColor =
						new Color(1f, 0.6f, 0f, 1f);
					darkWindowStyle.normal.background =
					darkWindowStyle.onNormal.background =
					darkWindowStyle.hover.background =
					darkWindowStyle.onHover.background =
					darkWindowStyle.active.background =
					darkWindowStyle.onActive.background =
					darkWindowStyle.focused.background =
					darkWindowStyle.onFocused.background = CreateTextureInColour(new Color32(52, 52, 52, 255));
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					darkWindowStyle.fontSize = 24;
#endif
				}
				if (darkWindowStyle.normal.background == null)
				{
					darkWindowStyle.normal.background =
					darkWindowStyle.onNormal.background =
					darkWindowStyle.hover.background =
					darkWindowStyle.onHover.background =
					darkWindowStyle.active.background =
					darkWindowStyle.onActive.background =
					darkWindowStyle.focused.background =
					darkWindowStyle.onFocused.background = CreateTextureInColour(new Color32(52, 52, 52, 255));
				}
				return darkWindowStyle;
			}
		}

		static GUIStyle grayWindowStyle = null;
		/// <summary>
		/// Style for window.
		/// </summary>
		public static GUIStyle GrayWindowStyle
		{
			get
			{
				if (grayWindowStyle == null)
				{
					grayWindowStyle = new GUIStyle(GUI.skin.window);
					grayWindowStyle.normal.textColor = grayWindowStyle.active.textColor = grayWindowStyle.hover.textColor = grayWindowStyle.focused.textColor =
						grayWindowStyle.onNormal.textColor = grayWindowStyle.onActive.textColor = grayWindowStyle.onHover.textColor = grayWindowStyle.onFocused.textColor =
						new Color(1f, 1f, 1f, 1f);
					grayWindowStyle.normal.background =
					grayWindowStyle.onNormal.background =
					grayWindowStyle.hover.background =
					grayWindowStyle.onHover.background =
					grayWindowStyle.active.background =
					grayWindowStyle.onActive.background =
					grayWindowStyle.focused.background =
					grayWindowStyle.onFocused.background = CreateTextureInColour(new Color32(128, 128, 128, 255));
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					grayWindowStyle.fontSize = 24;
#endif
				}
				if (grayWindowStyle.normal.background == null)
				{
					grayWindowStyle.normal.background =
					grayWindowStyle.onNormal.background =
					grayWindowStyle.hover.background =
					grayWindowStyle.onHover.background =
					grayWindowStyle.active.background =
					grayWindowStyle.onActive.background =
					grayWindowStyle.focused.background =
					grayWindowStyle.onFocused.background = CreateTextureInColour(new Color32(128, 128, 128, 255));
				}
					return grayWindowStyle;
			}
		}

		static GUIStyle whiteWindowStyle = null;
		/// <summary>
		/// Style for window.
		/// </summary>
		public static GUIStyle WhiteWindowStyle
		{
			get
			{
				if (whiteWindowStyle == null)
				{
					whiteWindowStyle = new GUIStyle(GUI.skin.window);
					whiteWindowStyle.normal.textColor = whiteWindowStyle.active.textColor = whiteWindowStyle.hover.textColor = whiteWindowStyle.focused.textColor =
						whiteWindowStyle.onNormal.textColor = whiteWindowStyle.onActive.textColor = whiteWindowStyle.onHover.textColor = whiteWindowStyle.onFocused.textColor =
						new Color(1f, 0.6f, 0f, 1f);
					whiteWindowStyle.normal.background =
					whiteWindowStyle.onNormal.background =
					whiteWindowStyle.hover.background =
					whiteWindowStyle.onHover.background =
					whiteWindowStyle.active.background =
					whiteWindowStyle.onActive.background =
					whiteWindowStyle.focused.background =
					whiteWindowStyle.onFocused.background = CreateTextureInColour(new Color32(255, 255, 255, 255));
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					whiteWindowStyle.fontSize = 24;
#endif
				}
				if (whiteWindowStyle.normal.background == null)
				{
					whiteWindowStyle.normal.background =
					whiteWindowStyle.onNormal.background =
					whiteWindowStyle.hover.background =
					whiteWindowStyle.onHover.background =
					whiteWindowStyle.active.background =
					whiteWindowStyle.onActive.background =
					whiteWindowStyle.focused.background =
					whiteWindowStyle.onFocused.background = CreateTextureInColour(new Color32(255, 255, 255, 255));
				}
				return whiteWindowStyle;
			}
		}

		static GUIStyle resizeLine = null;

		/// <summary>
		/// Style for resize line.
		/// </summary>
		public static GUIStyle ResizeLine
		{
			get
			{
				if (resizeLine == null)
				{
					resizeLine = new GUIStyle(GUI.skin.box);
					resizeLine.normal.background =
					resizeLine.onNormal.background = CreateTextureInColour(new Color32(52, 52, 52, 200));
					resizeLine.hover.background =
					resizeLine.onHover.background =
					resizeLine.active.background =
					resizeLine.onActive.background =
					resizeLine.focused.background =
					resizeLine.onFocused.background = CreateTextureInColour(new Color32(200, 200, 200, 200));
				}
				if (resizeLine.normal.background == null)
				{
					resizeLine.normal.background =
					resizeLine.onNormal.background = CreateTextureInColour(new Color32(52, 52, 52, 200));
					resizeLine.hover.background =
					resizeLine.onHover.background =
					resizeLine.active.background =
					resizeLine.onActive.background =
					resizeLine.focused.background =
					resizeLine.onFocused.background = CreateTextureInColour(new Color32(200, 200, 200, 200));
				}

				return resizeLine;
			}
		}

		static GUIStyle boxStyle = null;
		/// <summary>
		/// Style for label.
		/// </summary>
		public static GUIStyle BoxStyle
		{
			get
			{
				if (boxStyle == null)
				{
					boxStyle = new GUIStyle(GUI.skin.box);
					boxStyle.normal.background =
					boxStyle.onNormal.background =
					boxStyle.hover.background =
					boxStyle.onHover.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					boxStyle.active.background =
					boxStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					boxStyle.focused.background =
					boxStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					boxStyle.fontSize = 24;
#endif
				}
				if (boxStyle.normal.background == null)
				{
					boxStyle.normal.background =
					boxStyle.onNormal.background =
					boxStyle.hover.background =
					boxStyle.onHover.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					boxStyle.active.background =
					boxStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					boxStyle.focused.background =
					boxStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return boxStyle;
			}
		}

		static GUIStyle buttonStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle ButtonStyle
		{
			get
			{
				if (buttonStyle == null)
				{
					buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
					buttonStyle.onNormal.background = buttonStyle.normal.background = CreateTextureInColour(new Color32(28, 28, 28, 128));
					buttonStyle.onHover.background = buttonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					buttonStyle.onActive.background = buttonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					buttonStyle.onFocused.background = buttonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					buttonStyle.fontSize = 24;
#endif
				}
				if (buttonStyle.onNormal.background == null)
				{
					buttonStyle.onNormal.background = buttonStyle.normal.background = CreateTextureInColour(new Color32(28, 28, 28, 128));
					buttonStyle.onHover.background = buttonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					buttonStyle.onActive.background = buttonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					buttonStyle.onFocused.background = buttonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return buttonStyle;
			}
		}

		static GUIStyle inactivatedButtonStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle InactivatedButtonStyle
		{
			get
			{
				if (inactivatedButtonStyle == null)
				{
					inactivatedButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
					inactivatedButtonStyle.normal.textColor = inactivatedButtonStyle.active.textColor = inactivatedButtonStyle.hover.textColor = inactivatedButtonStyle.focused.textColor =
						inactivatedButtonStyle.onNormal.textColor = inactivatedButtonStyle.onActive.textColor = inactivatedButtonStyle.onHover.textColor = inactivatedButtonStyle.onFocused.textColor =
						Color.gray;
					inactivatedButtonStyle.onNormal.background = inactivatedButtonStyle.normal.background = CreateTextureInColour(new Color32(28, 28, 28, 128));
					inactivatedButtonStyle.onHover.background = inactivatedButtonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					inactivatedButtonStyle.onActive.background = inactivatedButtonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					inactivatedButtonStyle.onFocused.background = inactivatedButtonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					inactivatedButtonStyle.fontSize = 24;
#endif
				}
				if (inactivatedButtonStyle.onNormal.background == null)
				{
					inactivatedButtonStyle.onNormal.background = inactivatedButtonStyle.normal.background = CreateTextureInColour(new Color32(28, 28, 28, 128));
					inactivatedButtonStyle.onHover.background = inactivatedButtonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					inactivatedButtonStyle.onActive.background = inactivatedButtonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					inactivatedButtonStyle.onFocused.background = inactivatedButtonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return inactivatedButtonStyle;
			}
		}

		static GUIStyle inactivatedSelectedButtonStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle InactivatedSelectedButtonStyle
		{
			get
			{
				if (inactivatedSelectedButtonStyle == null)
				{
					inactivatedSelectedButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
					inactivatedSelectedButtonStyle.normal.textColor = inactivatedSelectedButtonStyle.active.textColor = inactivatedSelectedButtonStyle.hover.textColor = inactivatedSelectedButtonStyle.focused.textColor =
						inactivatedSelectedButtonStyle.onNormal.textColor = inactivatedSelectedButtonStyle.onActive.textColor = inactivatedSelectedButtonStyle.onHover.textColor = inactivatedSelectedButtonStyle.onFocused.textColor =
						Color.gray;
					inactivatedSelectedButtonStyle.onNormal.background = inactivatedSelectedButtonStyle.normal.background =
					inactivatedSelectedButtonStyle.onHover.background = inactivatedSelectedButtonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					inactivatedSelectedButtonStyle.onActive.background = inactivatedSelectedButtonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					inactivatedSelectedButtonStyle.onFocused.background = inactivatedSelectedButtonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					inactivatedSelectedButtonStyle.fontSize = 24;
#endif
				}
				if (inactivatedSelectedButtonStyle.onNormal.background == null)
				{
					inactivatedSelectedButtonStyle.onNormal.background = inactivatedSelectedButtonStyle.normal.background =
					inactivatedSelectedButtonStyle.onHover.background = inactivatedSelectedButtonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					inactivatedSelectedButtonStyle.onActive.background = inactivatedSelectedButtonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					inactivatedSelectedButtonStyle.onFocused.background = inactivatedSelectedButtonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return inactivatedSelectedButtonStyle;
			}
		}

		static GUIStyle selectedButtonStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle SelectedButtonStyle
		{
			get
			{
				if (selectedButtonStyle == null)
				{
					selectedButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
					selectedButtonStyle.onNormal.background = selectedButtonStyle.normal.background =
					selectedButtonStyle.onHover.background = selectedButtonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					selectedButtonStyle.onActive.background = selectedButtonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					selectedButtonStyle.onFocused.background = selectedButtonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					selectedButtonStyle.fontSize = 24;
#endif
				}
				if (selectedButtonStyle.onNormal.background == null)
				{
					selectedButtonStyle.onNormal.background = selectedButtonStyle.normal.background =
					selectedButtonStyle.onHover.background = selectedButtonStyle.hover.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					selectedButtonStyle.onActive.background = selectedButtonStyle.active.background = CreateTextureInColour(new Color32(255, 255, 255, 128));
					selectedButtonStyle.onFocused.background = selectedButtonStyle.focused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return selectedButtonStyle;
			}
		}

		static GUIStyle verticalScrollbarStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle VerticalScrollbarStyle
		{
			get
			{
				if (verticalScrollbarStyle == null)
				{
					verticalScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);
					verticalScrollbarStyle.normal.background =
					verticalScrollbarStyle.onNormal.background =
					verticalScrollbarStyle.hover.background =
					verticalScrollbarStyle.onHover.background =
					verticalScrollbarStyle.active.background =
					verticalScrollbarStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					verticalScrollbarStyle.focused.background =
					verticalScrollbarStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				if (verticalScrollbarStyle.normal.background == null)
				{
					verticalScrollbarStyle.normal.background =
					verticalScrollbarStyle.onNormal.background =
					verticalScrollbarStyle.hover.background =
					verticalScrollbarStyle.onHover.background =
					verticalScrollbarStyle.active.background =
					verticalScrollbarStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					verticalScrollbarStyle.focused.background =
					verticalScrollbarStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return verticalScrollbarStyle;
			}
		}

		static GUIStyle verticalScrollbarThumbStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle VerticalScrollbarThumbStyle
		{
			get
			{
				if (verticalScrollbarThumbStyle == null)
				{
					verticalScrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb);
					verticalScrollbarThumbStyle.normal.background =
					verticalScrollbarThumbStyle.onNormal.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					verticalScrollbarThumbStyle.hover.background =
					verticalScrollbarThumbStyle.onHover.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					verticalScrollbarThumbStyle.active.background =
					verticalScrollbarThumbStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					verticalScrollbarThumbStyle.focused.background =
					verticalScrollbarThumbStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				if (verticalScrollbarThumbStyle.normal.background == null)
				{
					verticalScrollbarThumbStyle.normal.background =
					verticalScrollbarThumbStyle.onNormal.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					verticalScrollbarThumbStyle.hover.background =
					verticalScrollbarThumbStyle.onHover.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					verticalScrollbarThumbStyle.active.background =
					verticalScrollbarThumbStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					verticalScrollbarThumbStyle.focused.background =
					verticalScrollbarThumbStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return verticalScrollbarThumbStyle;
			}
		}

		static GUIStyle horizontalScrollbarStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle HorizontalScrollbarStyle
		{
			get
			{
				if (horizontalScrollbarStyle == null)
				{
					horizontalScrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
					horizontalScrollbarStyle.normal.background =
					horizontalScrollbarStyle.onNormal.background =
					horizontalScrollbarStyle.hover.background =
					horizontalScrollbarStyle.onHover.background =
					horizontalScrollbarStyle.active.background =
					horizontalScrollbarStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					horizontalScrollbarStyle.focused.background =
					horizontalScrollbarStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				if (horizontalScrollbarStyle.normal.background == null)
				{
					horizontalScrollbarStyle.normal.background =
					horizontalScrollbarStyle.onNormal.background =
					horizontalScrollbarStyle.hover.background =
					horizontalScrollbarStyle.onHover.background =
					horizontalScrollbarStyle.active.background =
					horizontalScrollbarStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					horizontalScrollbarStyle.focused.background =
					horizontalScrollbarStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return horizontalScrollbarStyle;
			}
		}

		static GUIStyle horizontalScrollbarThumbStyle;
		/// <summary>
		/// Button style.
		/// </summary>
		public static GUIStyle HorizontalScrollbarThumbStyle
		{
			get
			{
				if (horizontalScrollbarThumbStyle == null)
				{
					horizontalScrollbarThumbStyle = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
					horizontalScrollbarThumbStyle.normal.background =
					horizontalScrollbarThumbStyle.onNormal.background =
					horizontalScrollbarThumbStyle.hover.background =
					horizontalScrollbarThumbStyle.onHover.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					horizontalScrollbarThumbStyle.active.background =
					horizontalScrollbarThumbStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					horizontalScrollbarThumbStyle.focused.background =
					horizontalScrollbarThumbStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				if (horizontalScrollbarThumbStyle.normal.background == null)
				{
					horizontalScrollbarThumbStyle.normal.background =
					horizontalScrollbarThumbStyle.onNormal.background =
					horizontalScrollbarThumbStyle.hover.background =
					horizontalScrollbarThumbStyle.onHover.background = CreateTextureInColour(new Color32(52, 52, 52, 128));
					horizontalScrollbarThumbStyle.active.background =
					horizontalScrollbarThumbStyle.onActive.background = CreateTextureInColour(new Color32(128, 128, 128, 128));
					horizontalScrollbarThumbStyle.focused.background =
					horizontalScrollbarThumbStyle.onFocused.background = CreateTextureInColour(new Color32(255, 0, 0, 128));
				}
				return horizontalScrollbarThumbStyle;
			}
		}

		[NonSerialized] static GUIStyle colorStyle = null;
		/// <summary>
		/// draw line
		/// </summary>
		public static GUIStyle ColorStyle
		{
			get
			{
				if (colorStyle == null)
				{
					colorStyle = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(255, 255, 255, 255);
					tex.SetPixels32(colours);
					tex.Apply();
					colorStyle.normal.background = tex;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
					colorStyle.fontSize = 24;
#endif
				}

				if (colorStyle.normal.background == null)
				{
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(255, 255, 255, 255);
					tex.SetPixels32(colours);
					tex.Apply();
					colorStyle.normal.background = tex;
				}
				return colorStyle;
			}
		}

		[NonSerialized] static GUIStyle line = null;
		/// <summary>
		/// draw line
		/// </summary>
		public static GUIStyle Line
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
					tex.Apply();
					line.normal.background = tex;
				}
				if (line.normal.background == null)
				{
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(255, 255, 255, 255);
					tex.SetPixels32(colours);
					tex.Apply();
					line.normal.background = tex;
				}
				return line;
			}
		}

		/// <summary>
		/// color of line
		/// </summary>
		static Color LineColor
		{
			get { return new Color(0.157f, 0.157f, 0.157f); }
		}

		/// <summary>
		/// Draw line with width x height.
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

		/// <summary>
		/// Method info data for drawing.
		/// </summary>
		public class MethodData
		{
			/// <param name="info">Data from.</param>
			/// <param name="infos">All info from parent object.</param>
			public MethodData(MethodInfo info, MemberInfo[] infos)
			{
				method = info;
				parameters = method.GetParameters();
				if (parameters != null && parameters.Length > 0)
				{
					cachedParamterDatas = new object[parameters.Length];
					hasParameter = true;
				}
				else hasParameter = false;

				canDraw = !(method.IsGenericMethod
					|| method.IsGenericMethodDefinition
					|| method.ContainsGenericParameters
					|| method.IsAbstract
					|| IsProperty(method, infos)
					|| !IsParameterDrawable);
				isBaseType = IsBaseType(info);
			}

			/// <summary>
			/// Is method comes from unity engine or System.Object
			/// </summary>
			static bool IsBaseType(MethodInfo info)
			{
				var type = info.DeclaringType;
				if (type == typeof(object))
					return true;
				if (!string.IsNullOrEmpty(type.Namespace) &&
					(type.Namespace.Contains("UnityEngine")
					|| type.Namespace.Contains("UnityEditor")
					|| type.Namespace.Contains("UnityEngineInternal")
					|| type.Namespace.Contains("UnityEditorInternal")
					|| type.Namespace.Contains("UnityScript")))
					return true;
				return false;
			}

			/// <summary>
			/// Return true if methodInfo is a property method. (eg. get or set)
			/// </summary>
			static bool IsProperty(MethodInfo methodInfo, MemberInfo[] infos)
			{
				for (int c = infos.Length - 1; c >= 0; --c)
				{
					if (infos[c].MemberType == MemberTypes.Property)
					{
						var pro = infos[c] as PropertyInfo;
						if (pro.CanRead && pro.GetGetMethod(true) == methodInfo)
							return true;
						if (pro.CanWrite && pro.GetSetMethod(true) == methodInfo)
							return true;
					}
				}

				return false;
			}

			/// <summary>
			/// Make sure no generic / interface or object parameter. Also not a abstract method.
			/// </summary>
			bool IsParameterDrawable
			{
				get
				{
					if (hasParameter)
					{
						for (int c = parameters.Length - 1; c >= 0; --c)
						{
							var type = parameters[c].ParameterType;
							if (type == typeof(string)) return true;
							if (type.IsAbstract || type.IsInterface || type.IsGenericType || type == typeof(object)) return false;
							if (type.IsClass)
							{
								var constructor = type.GetConstructor(Type.EmptyTypes);
								if (constructor == null) return false;
								if (!IsTypeDrawable(type)) return false;
							}
						}
					}
					return true;
				}
			}

			/// <summary>
			/// Check if type can be draw.
			/// </summary>
			bool IsTypeDrawable(Type type)
			{
				var infos = type.GetMembers(BindingFlags.Instance
									| BindingFlags.Public
									| BindingFlags.NonPublic
									| BindingFlags.GetProperty
									| BindingFlags.SetProperty
									| BindingFlags.GetField
									| BindingFlags.SetField);
				for (int c = infos.Length - 1; c >= 0; --c)
				{
					if (infos[c].MemberType == MemberTypes.Property)
					{
						var pro = infos[c] as PropertyInfo;
						return !pro.CanWrite;
					}
				}
				return true;
			}

			/// <summary>
			/// Cached info.
			/// </summary>
			public MethodInfo method = null;
			/// <summary>
			/// Cached parameter infos.
			/// </summary>
			public ParameterInfo[] parameters = null;
			/// <summary>
			/// Cached parameter datas.
			/// </summary>
			public object[] cachedParamterDatas = null;
			/// <summary>
			/// Is Data can draw in inspector.
			/// </summary>
			public bool canDraw;
			/// <summary>
			/// Is method has any parameter.
			/// </summary>
			public bool hasParameter;
			/// <summary>
			/// Is a method create by unity.
			/// </summary>
			public bool isBaseType;
		}

		/// <summary>
		/// Data of member for drawing.
		/// </summary>
		public class MemberData
		{
			/// <param name="info">PropertyInfo or FieldInfo.</param>
			public MemberData(MemberInfo info)
			{
				member = info;
				property = member.MemberType == MemberTypes.Property;
				if (property)
				{
					dontDrawProperty = DontDraw(info as PropertyInfo);
					drawSet = (info as PropertyInfo).CanWrite && (info as PropertyInfo).IsDefined(typeof(RHierarchy.DrawAttribute), true);
				}
			}
			/// <summary>
			/// A PropertyInfo or FieldInfo
			/// </summary>
			public MemberInfo member;
			/// <summary>
			/// Is property?
			/// </summary>
			public bool property;

			/// <summary>
			/// is good type of 
			/// </summary>
			public bool dontDrawProperty;

			/// <summary>
			/// Does property has DrawSetAttribute
			/// </summary>
			public bool drawSet;
		}

		/// <summary>
		/// All member info data for drawing.
		/// </summary>
		public class DrawData
		{
			/// <param name="type">Data type.</param>
			/// <param name="isStatic">Is static data?</param>
			public DrawData(Type type, bool isStatic)
			{
				this.type = type;
				Type currentType = type;
				MemberInfo[] infos = null;
				List<MemberData> memberList = new List<MemberData>();
				List<MethodData> methodList = new List<MethodData>();
				do
				{
					if (isStatic)
					{
						infos = currentType.GetMembers(BindingFlags.Static
										| BindingFlags.Public
										| BindingFlags.NonPublic
										| BindingFlags.GetProperty
										| BindingFlags.SetProperty
										| BindingFlags.GetField
										| BindingFlags.SetField
										| BindingFlags.InvokeMethod);
					}
					else
					{
						infos = currentType.GetMembers(BindingFlags.Instance
										| BindingFlags.Public
										| BindingFlags.NonPublic
										| BindingFlags.GetProperty
										| BindingFlags.SetProperty
										| BindingFlags.GetField
										| BindingFlags.SetField
										| BindingFlags.InvokeMethod);
					}

					foreach (var info in infos)
					//for (int c = infos.Length - 1; c >= 0; --c)
					{
						//var info = infos[c];
						switch (info.MemberType)
						{
							case MemberTypes.Field:
							case MemberTypes.Property:
								memberList.Add(new MemberData(info));
								break;
							case MemberTypes.Method:
								methodList.Add(new MethodData(info as MethodInfo, infos));
								break;
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
				methodInfos = methodList.ToArray();
			}
			/// <summary>
			/// Which type is this object.
			/// </summary>
			public Type type;
			/// <summary>
			/// All property/field member in this object.
			/// </summary>
			public MemberData[] memberInfos = null;
			/// <summary>
			/// All method data in this object.
			/// </summary>
			public MethodData[] methodInfos = null;
		}

		/// <summary>
		/// List data for drawing.
		/// </summary>
		public class ListData
		{
			/// <param name="parent">From which show hide data</param>
			public ListData()
			{
				currentIndex = -1;
			}
			int currentIndex = -1;
			/// <summary>
			/// Is index foldered.
			/// </summary>
			public bool IsFoldered(int index)
			{
				return currentIndex != index;
			}
			/// <summary>
			/// Set currentIndex to index.
			/// </summary>
			public void ToggleFolder(int index)
			{
				currentIndex = currentIndex == index ? -1 : index;
			}
		}

		/// <summary>
		/// Diectionary data for drawing.
		/// </summary>
		public class DictionaryData
		{
			/// <param name="parentType">Type of dictionary</param>
			public DictionaryData(Type parentType)
			{
				if (parentType.IsGenericType)
				{
					var types = parentType.GetGenericArguments();
					if (types.Length == 2)
					{
						keyType = types[0];
						valueType = types[1];
					}
				}
				show = false;
			}
			bool show;
			/// <summary>
			/// Is currently show add box.
			/// </summary>
			public bool Show
			{
				get { return show && keyType != null && valueType != null; }
				set { show = value; }
			}
			/// <summary>
			/// Key type.
			/// </summary>
			public Type keyType;
			/// <summary>
			/// Key data.
			/// </summary>
			public object key;
			/// <summary>
			/// Value Type.
			/// </summary>
			public Type valueType;
			/// <summary>
			/// Value data.
			/// </summary>
			public object value;
		}

		/// <summary>
		/// Helper class for popup.
		/// </summary>
		public class Popup
		{
			static Dictionary<Type, int[]> enumValueMap = new Dictionary<Type, int[]>();
			/// <summary>
			/// Helper method to get enum values.
			/// </summary>
			public static int[] GetEnumValues(Type type)
			{
				if (type != null && typeof(Enum).IsAssignableFrom(type))
				{
					if (!enumValueMap.ContainsKey(type) || enumValueMap[type] == null)
					{
						enumValueMap.Remove(type);
						enumValueMap.Add(type, Enum.GetValues(type) as int[]);
					}
					return enumValueMap[type];
				}

				Debug.LogError("Type is null or Type is not a Enum.");
				return null;
			}

			protected static Dictionary<Type, Popup> popups = new Dictionary<Type, Popup>();
			/// <summary>
			/// Return cached popup.
			/// </summary>
			public static Popup GetPopup(Type enumType, int defaultIndex)
			{
				if (enumType != null && typeof(Enum).IsAssignableFrom(enumType))
				{
					if (!popups.ContainsKey(enumType))
					{
						popups[enumType] = NewEnumPopup(enumType, defaultIndex);
					}
					return popups[enumType];
				}

				Debug.LogError("Type is null or Type is not a Enum.");
				return null;
			}

			static Popup layerPopup = null;
			public static Popup GetLayerPopup(int defaultLayer)
			{
				if (layerPopup == null)
				{
					List<string> layerNames = new List<string>();
					List<int> layerValues = new List<int>();
					for (int i = 0; i < 32; ++i)
					{
						var lname = LayerMask.LayerToName(i);
						if (lname.Length > 0)
						{
							layerNames.Add(lname);
							int layer = LayerMask.NameToLayer(lname);
							layerValues.Add(layer);
						}
					}
					layerPopup = NewPopup(layerNames.ToArray(), layerValues.ToArray(), defaultLayer);
				}
				return layerPopup;
			}

			/// <summary>
			/// A helper method for create new popup
			/// </summary>
			public static Popup NewEnumPopup(Type enumType, int defaultIndex = 0, GUIStyle _buttonStyle = null, GUIStyle _boxStyle = null, GUIStyle _listStyle = null, bool endIsCount = false, bool AddTitle = false)
			{
				string[] names = Enum.GetNames(enumType);
				int[] values = GetEnumValues(enumType);
				int count = names.Length;
				if (endIsCount) count--;
				// there is Count in the end
				GUIContent[] lastContents = new GUIContent[count];
				for (int c = 0; c < count; ++c)
				{
					string content;
					if (AddTitle)
						content = string.Format("{0}. {1}", c + 1, names[c]);
					else
						content = names[c];
					lastContents[c] = new GUIContent(content);
				}
				return new Popup(lastContents, values, names, defaultIndex, _buttonStyle, _boxStyle, _listStyle);
			}

			/// <summary>
			/// A helper method for create new popup
			/// </summary>
			public static Popup NewPopup(string[] names, int[] values, int defaultIndex = 0, GUIStyle _buttonStyle = null, GUIStyle _boxStyle = null, GUIStyle _listStyle = null, bool endIsCount = false, bool AddTitle = false)
			{
				int count = names.Length;
				if (endIsCount) count--;
				// there is Count in the end
				GUIContent[] lastContents = new GUIContent[count];
				for (int c = 0; c < count; ++c)
				{
					string content;
					if (AddTitle)
						content = string.Format("{0}. {1}", c + 1, names[c]);
					else
						content = names[c];
					lastContents[c] = new GUIContent(content);
				}
				return new Popup(lastContents, values, names, defaultIndex, _buttonStyle, _boxStyle, _listStyle);
			}

			protected GUIContent currentContent;
			protected GUIContent[] allContent;
			protected string[] allNames;
			protected GUIStyle mainStyle;
			protected GUIStyle backgroundStyle;
			protected GUIStyle listStyle;

			protected int[] enumValues;

			protected bool isExpand = false;
			protected int selectedIndex = 0;

			/// <summary>
			/// return selected index.
			/// </summary>
			public int SelectedIndex
			{
				get { return selectedIndex; }
				set
				{
					selectedIndex = value;
					currentContent = allContent[selectedIndex];
				}
			}

			/// <summary>
			/// return is popup currently clicked.
			/// </summary>
			public bool IsExpand { get { return isExpand; } }

			public Popup(GUIContent[] _allContent, int[] values, string[] names, int defaultSelectedIndex, GUIStyle _mainStyle = null, GUIStyle _backgroundStyle = null, GUIStyle _listStyle = null)
			{
				allContent = _allContent;
				allNames = names;
				if (_mainStyle == null) _mainStyle = ButtonStyle;
				mainStyle = _mainStyle;
				if (_backgroundStyle == null) _backgroundStyle = BoxStyle;
				backgroundStyle = _backgroundStyle;
				if (_listStyle == null) _listStyle = BackgroundedWhiteLabelStyle;
				listStyle = _listStyle;
				enumValues = values;
				for (int i = enumValues.Length - 1; i >= 0; --i)
				{
					if (enumValues[i] == defaultSelectedIndex)
					{
						selectedIndex = i;
						break;
					}
				}
				currentContent = new GUIContent(allContent[selectedIndex].text);
			}

			Vector2 scrollViewPos;

			protected void DrawGUI(GUILayoutOption[] options)
			{
				GUILayout.BeginVertical(backgroundStyle, options);

				if (GUILayout.Button(currentContent, mainStyle))
				{
					isExpand = !isExpand;
				}

				if (isExpand)
				{
					if (allContent.Length > 5)
						scrollViewPos = BeginScrollView(scrollViewPos, GUILayout.MinHeight(25*Mathf.Clamp(allContent.Length, 5, 15)));
					int newSelectedItemIndex = GUILayout.SelectionGrid(selectedIndex, allContent, 1, listStyle);
					if (newSelectedItemIndex != selectedIndex)
					{
						selectedIndex = newSelectedItemIndex;
						isExpand = false;
					}
					if (allContent.Length > 5)
						GUILayout.EndScrollView();
				}

				GUILayout.EndVertical();
			}

			int IndexOf(int selection)
			{
				for (int i = enumValues.Length - 1; i >= 0; --i)
				{
					if (enumValues[i] == selection)
						return i;
				}
				return 0;
			}

			/// <summary>
			/// Draw popup, return selected enum value in integer.
			/// </summary>
			public virtual int DrawPopup(int selection, params GUILayoutOption[] options)
			{
				int ind = IndexOf(selection);
				if (selectedIndex != ind)
				{
					SelectedIndex = ind;
				}

				DrawGUI(options);

				selectedIndex = Mathf.Clamp(selectedIndex, 0, enumValues.Length);

				currentContent = allContent[selectedIndex];

				return enumValues[selectedIndex];
			}
		}

		/// <summary>
		/// Mask popup.
		/// </summary>
		public class MaskPopup : Popup
		{
			/// <summary>
			/// A helper method for create new mask popup
			/// </summary>
			public static MaskPopup NewMaskPopup(Type enumType, int defaultIndex = 0, GUIStyle _buttonStyle = null, GUIStyle _boxStyle = null, GUIStyle _listStyle = null, bool endIsCount = false, bool AddTitle = false)
			{
				string[] names = Enum.GetNames(enumType);
				int[] values = GetEnumValues(enumType);
				int count = names.Length;
				if (endIsCount) count--;
				// there is Count in the end
				GUIContent[] listContents = new GUIContent[count + 2];
				int[] masks = new int[count + 2];
				string[] maskNames = new string[count + 2];
				for (int c = 0; c < count; ++c)
				{
					int ind = c + 2;
					if (AddTitle)
						maskNames[ind] = string.Format("{0}. {1}", ind + 1, names[c]);
					else
						maskNames[ind] = names[c];
					listContents[ind] = new GUIContent(maskNames[ind]);
					masks[ind] = values[c];
				}
				masks[0] = 0;
				maskNames[0] = AddTitle ? "1. Nothing" : "Nothing";
				listContents[0] = new GUIContent(maskNames[0]);
				masks[1] = -1;
				maskNames[1] = AddTitle ? "2. Everything" : "Everything";
				listContents[1] = new GUIContent(maskNames[1]);

				return new MaskPopup(listContents, values, maskNames, defaultIndex, _buttonStyle, _boxStyle, _listStyle);
			}

			/// <summary>
			/// A helper method for create new mask popup.
			/// </summary>
			public static MaskPopup NewMaskPopup(string[] names, int[] values, int defaultIndex = 0, GUIStyle _buttonStyle = null, GUIStyle _boxStyle = null, GUIStyle _listStyle = null, bool endIsCount = false, bool AddTitle = false)
			{
				int count = names.Length;
				if (endIsCount) count--;
				// there is Count in the end
				GUIContent[] listContents = new GUIContent[count + 2];
				int[] masks = new int[count + 2];
				string[] maskNames = new string[count + 2];
				for (int c = 0; c < count; ++c)
				{
					int ind = c + 2;
					if (AddTitle)
						maskNames[ind] = string.Format("{0}. {1}", ind + 1, names[c]);
					else
						maskNames[ind] = names[c];
					listContents[ind] = new GUIContent(maskNames[ind]);
					masks[ind] = values[c];
				}
				masks[0] = 0;
				maskNames[0] = AddTitle ? "1. Nothing" : "Nothing";
				listContents[0] = new GUIContent(maskNames[0]);
				masks[1] = -1;
				maskNames[1] = AddTitle ? "2. Everything" : "Everything";
				listContents[1] = new GUIContent(maskNames[1]);

				return new MaskPopup(listContents, values, maskNames, defaultIndex, _buttonStyle, _boxStyle, _listStyle);
			}

			/// <summary>
			/// Return cached popup.
			/// </summary>
			public static MaskPopup GetMaskPopup(Type enumType, int defaultIndex)
			{
				if (enumType != null && typeof(Enum).IsAssignableFrom(enumType))
				{
					if (!popups.ContainsKey(enumType))
					{
						popups[enumType] = NewMaskPopup(enumType, defaultIndex);
					}
					return (MaskPopup)popups[enumType];
				}

				Debug.LogError("Type is null or Type is not a Enum.");
				return null;
			}

			static MaskPopup layerMaskPopup = null;
			public static MaskPopup GetLayerMaskPopup(int defaultLayer)
			{
				if (layerMaskPopup == null)
				{
					List<string> layerNames = new List<string>();
					List<int> layerValues = new List<int>();
					for (int i = 0; i < 32; ++i)
					{
						var lname = LayerMask.LayerToName(i);
						if (lname.Length > 0)
						{
							layerNames.Add(lname);
							int layer = LayerMask.NameToLayer(lname);
							layerValues.Add(1 << layer);
						}
					}
					layerMaskPopup = NewMaskPopup(layerNames.ToArray(), layerValues.ToArray(), defaultLayer);
				}
				return layerMaskPopup;
			}

			readonly int allSelected = 0;
			int flags = 0;
			public MaskPopup(GUIContent[] _allContent, int[] values, string[] names, int defaultSelectedIndex, GUIStyle _mainStyle = null, GUIStyle _backgroundStyle = null, GUIStyle _listStyle = null)
				: base(_allContent, values, names, defaultSelectedIndex, _mainStyle, _backgroundStyle, _listStyle)
			{
				for (int i = enumValues.Length - 1; i >= 0; --i)
				{
					allSelected = AddFlag(allSelected, enumValues[i]);
				}
				flags = defaultSelectedIndex;
				UpdateCurrentContect();
				selectedIndex = -1;
			}

			void UpdateCurrentContect()
			{
				if (flags == -1)
				{
					currentContent.text = "Everything";
					for (int i = allContent.Length - 1; i >= 0; --i)
					{
						if (i != 0)
							allContent[i].text = "\u2713 " + allNames[i];
						else
							allContent[i].text = allNames[i];
					}
				}
				else if (flags == 0)
				{
					currentContent.text = "Nothing";
					for (int i = allContent.Length - 1; i >= 0; --i)
					{
						if (i == 0)
							allContent[i].text = "\u2713 " + allNames[i];
						else
							allContent[i].text = allNames[i];
					}
				}
				else
				{
					currentContent.text = "Mixed ...";
					allContent[0].text = allNames[0];
					allContent[1].text = allNames[1];
					for (int i = allContent.Length - 1; i > 1; --i)
					{
						if (ContainsFlag(flags, enumValues[i - 2]))
							allContent[i].text = "\u2713 " + allNames[i];
						else
							allContent[i].text = allNames[i];
					}
				}
			}

			public override int DrawPopup(int selection, params GUILayoutOption[] options)
			{
				if (selection != flags)
				{
					flags = selection;
				}

				DrawGUI(options);

				if (selectedIndex != -1)
				{
					if (selectedIndex == 0)
					{
						flags = 0;
					}
					else if (selectedIndex == 1)
					{
						flags = -1;
					}
					else
					{
						if (flags != -1 && !ContainsFlag(flags, enumValues[selectedIndex - 2]))
						{
							flags = AddFlag(flags, enumValues[selectedIndex - 2]);
						}
						else
						{
							if (flags == -1) flags = allSelected;
							flags = RemoveFlag(flags, enumValues[selectedIndex - 2]);
						}
					}
					if (flags == allSelected)
						flags = -1;
					selectedIndex = -1;
					UpdateCurrentContect();
				}

				return flags;
			}
		}

		/// <summary>
		/// Helper method for contains flags.
		/// </summary>
		public static bool ContainsFlag(int flags, int flag)
		{
			return (flags & flag) != 0;
		}

		/// <summary>
		/// Helper method for add flags.
		/// </summary>
		public static int AddFlag(int flags, int flag)
		{
			return (flags | flag);
		}

		/// <summary>
		/// Helper method for remove flags.
		/// </summary>
		public static int RemoveFlag(int flags, int flag)
		{
			return (flags & (~flag));
		}

		/// <summary>
		/// Get draw data.
		/// </summary>
		public static DrawData GetDrawDatas(Type type, bool isStatic)
		{
			if (type == null) return null;
			if (isStatic)
			{
				if (!staticDrawDatas.ContainsKey(type))
				{
					staticDrawDatas[type] = new DrawData(type, true);
				}

				return staticDrawDatas[type];
			}
			if (!drawDatas.ContainsKey(type))
			{
				drawDatas[type] = new DrawData(type, false);
			}
			return drawDatas[type];
		}

		/// <summary>
		/// Define property should not be drawed.
		/// </summary>
		public static bool DontDraw(PropertyInfo info)
		{
			if (info.IsDefined(typeof(ObsoleteAttribute), true)
				|| info.Name == "enabled"
				|| info.Name == "isActiveAndEnabled"
				|| info.Name == "name"
				|| info.Name == "tag"
				|| !(info.IsDefined(typeof(RHierarchy.DrawAttribute), true)))
				return true;
			return false;
		}

		/// <summary>
		/// Draw int field.
		/// </summary>
		public static int IntField(int value, params GUILayoutOption[] options)
		{
			var str = GUILayout.TextField(value.ToString(), TextFieldStyle, options);
			int next;
			if (int.TryParse(str, out next))
			{
				return next;
			}
			return value;
		}

		/// <summary>
		/// Draw long field.
		/// </summary>
		public static long LongField(long value, params GUILayoutOption[] options)
		{
			var str = GUILayout.TextField(value.ToString(), TextFieldStyle, options);
			long next;
			if (long.TryParse(str, out next))
			{
				return next;
			}
			return value;
		}

		static string floatFieldStr;
		/// <summary>
		/// Draw float field.
		/// </summary>
		public static float FloatField(float value, params GUILayoutOption[] options)
		{
			var val = value.ToString();
			if (string.IsNullOrEmpty(floatFieldStr) || floatFieldStr[floatFieldStr.Length - 1] != '.')
			{
				floatFieldStr = val;
			}
			else
			{
				if (floatFieldStr.Contains(val))
					val = floatFieldStr;
			}
			var str = GUILayout.TextField(val, TextFieldStyle, options);
			if (str != val)
			{
				floatFieldStr = str;
				float next;
				if (float.TryParse(str, out next))
				{
					return next;
				}
			}
			return value;
		}

		static string doubleFieldStr;
		/// <summary>
		/// draw double field.
		/// </summary>
		public static double DoubleField(double value, params GUILayoutOption[] options)
		{
			var val = value.ToString();
			if (string.IsNullOrEmpty(doubleFieldStr) || doubleFieldStr[doubleFieldStr.Length - 1] != '.')
			{
				doubleFieldStr = val;
			}
			else
			{
				if (doubleFieldStr.Contains(val))
					val = doubleFieldStr;
			}
			var str = GUILayout.TextField(val, TextFieldStyle, options);
			if (str != val)
			{
				doubleFieldStr = str;
				double next;
				if (double.TryParse(str, out next))
				{
					return next;
				}
			}
			return value;
		}

		/// <summary>
		/// Draw text field/area.
		/// </summary>
		public static string DrawText(string str, params GUILayoutOption[] options)
		{
			if (str != null)
			{
				if (str.Length < 60)
					return GUILayout.TextField(str, TextFieldStyle, options);

				if (str.Length > 15000)
				{
					GUILayout.Label("String is too long for TextMesh Generator.", LabelStyle);
					return str;
				}
			}
			return GUILayout.TextArea(str, TextAreaStyle, options);
		}

		/// <summary>
		/// Draw label (for string use DrawText).
		/// </summary>
		public static object DrawObjectField(object obj, params GUILayoutOption[] options)
		{
			if (obj == null)
			{
				GUILayout.Label("null", LabelStyle, options);
			}
			else
			{
				GUILayout.Label(obj.ToString(), LabelStyle, options);
			}
			return obj;
		}

		/// <summary>
		/// Draw area (for string use DrawText).
		/// </summary>
		public static object DrawArea(object obj, params GUILayoutOption[] options)
		{
			if (obj == null)
			{
				GUILayout.TextArea("null", TextAreaStyle, options);
			}
			else
			{
				GUILayout.TextArea(obj.ToString(), TextAreaStyle, options);
			}
			return obj;
		}

#if UNITY_2017_2_OR_NEWER
		/// <summary>
		/// Draw Vector2Int.
		/// </summary>
		public static Vector2Int DrawVector2Int(Vector2Int vec)
		{
			for (int c = 0; c < 2; ++c)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(xyzw[c], LabelStyle, GUILayout.Width(15f));
				vec[c] = IntField(vec[c], GUILayout.MinWidth(smallFieldWidth));
				GUILayout.EndHorizontal();
			}
			return vec;
		}

		/// <summary>
		/// Draw Vector3Int.
		/// </summary>
		public static Vector3Int DrawVector3Int(Vector3Int vec)
		{
			for (int c = 0; c < 3; ++c)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(xyzw[c], LabelStyle, GUILayout.Width(15f));
				vec[c] = IntField(vec[c], GUILayout.MinWidth(smallFieldWidth));
				GUILayout.EndHorizontal();
			}
			return vec;
		}
#endif

		/// <summary>
		/// Draw Vector2.
		/// </summary>
		public static Vector2 DrawVector2(Vector2 vec)
		{
			for (int c = 0; c < 2; ++c)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(xyzw[c], LabelStyle, GUILayout.Width(15f));
				vec[c] = FloatField(vec[c], GUILayout.MinWidth(smallFieldWidth));
				GUILayout.EndHorizontal();
			}
			return vec;
		}

		/// <summary>
		/// Draw Vector3.
		/// </summary>
		public static Vector3 DrawVector3(Vector3 vec)
		{
			for (int c = 0; c < 3; ++c)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(xyzw[c], LabelStyle, GUILayout.Width(15f));
				vec[c] = FloatField(vec[c], GUILayout.MinWidth(smallFieldWidth));
				GUILayout.EndHorizontal();
			}
			return vec;
		}

		/// <summary>
		/// Draw Vector4.
		/// </summary>
		public static Vector4 DrawVector4(Vector4 vec)
		{
			for (int c = 0; c < 4; ++c)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(xyzw[c], LabelStyle, GUILayout.Width(15f));
				vec[c] = FloatField(vec[c], GUILayout.MinWidth(smallFieldWidth));
				GUILayout.EndHorizontal();
			}
			return vec;
		}

		/// <summary>
		/// Draw Quaternion.
		/// </summary>
		public static Quaternion DrawQuaternion(Quaternion quat)
		{
			for (int c = 0; c < 4; ++c)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(xyzw[c], LabelStyle, GUILayout.Width(15f));
				quat[c] = FloatField(quat[c], GUILayout.MinWidth(smallFieldWidth));
				GUILayout.EndHorizontal();
			}
			return quat;
		}

		public static Color Color32ToColor(Color32 color32)
		{
			return new Color((float)color32.r / 255f, (float)color32.g / 255f, (float)color32.b / 255f, (float)color32.a / 255f);
		}

		public static Color32 ColorToColor32(Color color)
		{
			return new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
		}

		public static int DrawPopup(int lastSelected, Type type, params GUILayoutOption[] options)
		{
			return Popup.GetPopup(type, lastSelected).DrawPopup(lastSelected, options);
		}

		public static int DrawLayerPopup(int lastSelected, params GUILayoutOption[] options)
		{
			return Popup.GetLayerPopup(lastSelected).DrawPopup(lastSelected, options);
		}

		public static int DrawMaskPopup(int defaultSelected, Type type, params GUILayoutOption[] options)
		{
			return MaskPopup.GetMaskPopup(type, defaultSelected).DrawPopup(defaultSelected, options);
		}

		public static int DrawLayerMaskPopup(int defaultLayer, params GUILayoutOption[] options)
		{
			return MaskPopup.GetLayerMaskPopup(defaultLayer).DrawPopup(defaultLayer, options);
		}

		public static Vector2 BeginScrollView(Vector2 scrollViewPos, params GUILayoutOption[] options)
		{
			var hts = GUI.skin.horizontalScrollbarThumb;
			GUI.skin.horizontalScrollbarThumb = HorizontalScrollbarThumbStyle;
			var vts = GUI.skin.verticalScrollbarThumb;
			GUI.skin.verticalScrollbarThumb = VerticalScrollbarThumbStyle;
			scrollViewPos = GUILayout.BeginScrollView(scrollViewPos, HorizontalScrollbarStyle, VerticalScrollbarStyle, options);
			GUI.skin.horizontalScrollbarThumb = hts;
			GUI.skin.verticalScrollbarThumb = vts;
			return scrollViewPos;
		}
	}
}