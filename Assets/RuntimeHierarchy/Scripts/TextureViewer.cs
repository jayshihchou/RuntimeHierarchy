using UnityEngine;

namespace RHierarchy
{
	public class TextureViewer : MonoBehaviour
	{
		Rect textureWindowPos = new Rect(15f, 15f, 300f, 450f);
		Texture inspectorTexture = null;
		Sprite inspectorSprite = null;
		Rect spriteRect = new Rect();
		bool viewSpriteFull = false;
		bool drawLines = false;
		static Texture2D background = null;
		static Texture2D Background
		{
			get
			{
				if (background == null)
				{
					background = new Texture2D(16, 16) { filterMode = FilterMode.Point };
					var colors = background.GetPixels32();
					for (int y = 0; y < 16; ++y)
					{
						for (int x = 0; x < 16; ++x)
						{
							Color32 color = new Color32(128, 128, 128, 255);
							if ((y % 2 == 0 && x % 2 == 0) || (y % 2 == 1 && x % 2 == 1))
							{
								color = new Color32(192, 192, 192, 255);
							}
							colors[y * 16 + x] = color;
						}
					}
					background.SetPixels32(colors);
					background.Apply();
				}
				return background;
			}
		}

		[System.NonSerialized] static GUIStyle horizontalDottedLine = null;
		/// <summary>
		/// draw line
		/// </summary>
		static GUIStyle HorizontalDottedLine
		{
			get
			{
				if (horizontalDottedLine == null)
				{
					horizontalDottedLine = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(0, 255, 0, 255);
					tex.SetPixels32(colours);
					tex.Apply();
					horizontalDottedLine.normal.background = tex;
				}
				if (horizontalDottedLine.normal.background == null)
				{
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(0, 255, 0, 255);
					tex.SetPixels32(colours);
					tex.Apply();
					horizontalDottedLine.normal.background = tex;
				}
				return horizontalDottedLine;
			}
		}

		[System.NonSerialized] static GUIStyle verticalDottedLine = null;
		/// <summary>
		/// draw line
		/// </summary>
		static GUIStyle VerticalDottedLine
		{
			get
			{
				if (verticalDottedLine == null)
				{
					verticalDottedLine = new GUIStyle() { stretchWidth = true, margin = new RectOffset(0, 0, 7, 7) };
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(0, 255, 0, 255);
					tex.SetPixels32(colours);
					tex.Apply();
					verticalDottedLine.normal.background = tex;
				}
				if (verticalDottedLine.normal.background == null)
				{
					var tex = new Texture2D(2, 2);
					var colours = tex.GetPixels32();
					for (int c = colours.Length - 1; c >= 0; --c)
						colours[c] = new Color32(0, 255, 0, 255);
					tex.SetPixels32(colours);
					tex.Apply();
					verticalDottedLine.normal.background = tex;
				}
				return verticalDottedLine;
			}
		}

		/// <summary>
		/// Draw line with width x height.
		/// </summary>
		static void DrawHorizontalLine(Rect position)
		{
			if (Event.current.type == EventType.Repaint)
			{
				HorizontalDottedLine.Draw(position, false, false, false, false);
			}
		}

		/// <summary>
		/// Draw line with width x height.
		/// </summary>
		static void DrawVerticalLine(Rect position)
		{
			if (Event.current.type == EventType.Repaint)
			{
				VerticalDottedLine.Draw(position, false, false, false, false);
			}
		}

		public int windowID;

		string WindowName
		{
			get
			{
				if (inspectorTexture != null)
					return inspectorTexture.name;
				else if (inspectorSprite != null)
					return inspectorSprite.name;
				return string.Empty;
			}
		}

		public Texture DrawTextureField(Texture texture)
		{
			if (texture == null)
			{
				GUILayout.Label("Texture is null", Utility.YelloLabelStyle);
			}
			else if (GUILayout.Button(string.Format("{0} ({1})  (Click to view)", texture.name, texture.GetType().Name), Utility.ButtonStyle))
			{
				if (inspectorTexture != texture)
				{
					enabled = true;
					inspectorTexture = texture;
				}
				else enabled = false;
			}

			return texture;
		}

		public Sprite DrawSpriteField(Sprite sprite)
		{
			if (sprite == null || sprite.texture == null)
			{
				GUILayout.Label("Sprite is null", Utility.YelloLabelStyle);
			}
			else if (GUILayout.Button(string.Format("{0} ({1})  (Click to view)", sprite.name, sprite.GetType().Name), Utility.ButtonStyle))
			{
				if (inspectorSprite != sprite)
				{
					enabled = true;
					inspectorSprite = sprite;
					spriteRect = inspectorSprite.rect;
					float wid = (float)sprite.texture.width;
					float hei = (float)sprite.texture.height;
					spriteRect.x /= wid;
					spriteRect.y /= hei;
					spriteRect.width /= wid;
					spriteRect.height /= hei;
					viewSpriteFull = false;
				}
				else enabled = false;
			}

			return sprite;
		}

		private void OnDisable()
		{
			inspectorTexture = null;
			inspectorSprite = null;
		}

		private void OnGUI()
		{
			if (inspectorTexture != null || inspectorSprite != null)
			{
				textureWindowPos = GUI.Window(windowID, textureWindowPos, TextureWindow, WindowName, Utility.GrayWindowStyle);
			}
		}

		void TextureWindow(int id)
		{
			Rect texRect = new Rect(15f, 30f, 300f - 30f, 300f - 30f);
			GUI.DrawTexture(texRect, Background);
			if (inspectorTexture != null)
			{
				GUI.DrawTexture(texRect, inspectorTexture);
			}
			else if (inspectorSprite != null)
			{
				if (viewSpriteFull)
				{
					GUI.DrawTexture(texRect, inspectorSprite.texture);
					if (GUI.Button(new Rect(15f, 450f - 45f - 45f, 300f - 30f, 25f), "Draw Rect", Utility.ButtonStyle))
					{
						drawLines = !drawLines;
					}
					if (drawLines)
					{
						Rect rect = spriteRect;
						Rect left, top, right, bottom;
						left = top = right = bottom = texRect;
						left.x = texRect.x + (rect.x * texRect.width);
						right.x = texRect.x + (rect.x + rect.width) * texRect.width;
						left.width = right.width = 1;
						top.height = bottom.height = 1;
						top.y = texRect.y + texRect.height - (rect.y * texRect.height);
						bottom.y = texRect.y + texRect.height - (rect.y + rect.height) * texRect.height;
						DrawHorizontalLine(left);
						DrawHorizontalLine(right);
						DrawVerticalLine(top);
						DrawVerticalLine(bottom);
					}
				}
				else
				{
					GUI.DrawTextureWithTexCoords(texRect, inspectorSprite.texture, spriteRect);
				}
				if (GUI.Button(new Rect(15f, 450f - 45f - 45f - 45f, 300f - 30f, 25f), "View Full Texture", Utility.ButtonStyle))
					viewSpriteFull = !viewSpriteFull;
			}

			if (GUI.Button(new Rect(15f, 450f - 45f, 300f - 30f, 25f), "Close", Utility.ButtonStyle))
			{
				enabled = false;
				inspectorTexture = null;
				inspectorSprite = null;
				viewSpriteFull = false;
			}

			GUI.DragWindow();
		}
	}
}