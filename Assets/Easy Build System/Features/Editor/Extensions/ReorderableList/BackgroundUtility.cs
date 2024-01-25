/// <summary>
/// Project : Easy Build System
/// Class : BackgroundUtility.cs
/// Namespace : EasyBuildSystem.Features.Editor.Extensions.ReorderableList
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Editor.Extensions.ReorderableList
{
	public static class BackgroundUtility
	{
		static readonly Color ActiveColorLightSkin = new Color(0.3f, 0.6f, 0.95f, 0.95f);
		static readonly Color ActivateColorDarkSkin = new Color(0.2f, 0.4f, 0.7f, 0.95f);

		static Color ActiveColor
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
					return ActivateColorDarkSkin;
				else
					return ActiveColorLightSkin;
			}
		}

		static Color BackgroundColorLightSkin = new Color(0.85f, 0.85f, 0.85f, 1f);

		static Color BackgroundColorDarkSkin = new Color(0.25f, 0.25f, 0.25f, 1f);

		static Color DifferentBackgroundColor
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
					return BackgroundColorDarkSkin;
				else
					return BackgroundColorLightSkin;
			}
		}

		static float ElementLeftMargin
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
					return 1f;
				else
					return 2f;
			}
		}

		static float ElementRightExtrusion => 2f;

		static float ActiveLeftMargin => 1f;

		static float ActiveRightExtrusion
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
                {
                    return 2f;
                }
                else
                {
                    return 1f;
                }
            }
		}

		public static void DrawElementBackgroundColorDifferent(Rect rect)
			=> DrawElementBackgroundColor(
				rect,
				DifferentBackgroundColor,
				ElementLeftMargin,
				ElementRightExtrusion
				);

		public static void DrawElementBackgroundColorActive(Rect rect)
			=> DrawElementBackgroundColor(
				rect,
				ActiveColor,
				ActiveLeftMargin,
				ActiveRightExtrusion
				);

		static void DrawElementBackgroundColor(
			Rect rect, 
			Color color, 
			float leftMargin,
			float rightExtrusion)
		{
			GUI.DrawTexture(
				HorizontalAdjusted(rect, leftMargin, rightExtrusion),
				CreateColorTexture(color),
				ScaleMode.StretchToFill
			);
		}

		static Rect HorizontalAdjusted(
			Rect rect,
			float leftMargin,
			float rightExtrusion)
		{
			rect.x += leftMargin;

			rect.width -= (leftMargin + rightExtrusion);

			return rect;
		}

		static Texture2D CreateColorTexture(Color color)
		{
            Texture2D texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, color);
			texture.Apply();
			return texture;
		}
	}
}