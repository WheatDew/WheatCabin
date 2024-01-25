/// <summary>
/// Project : Easy Build System
/// Class : LayoutUtility.cs
/// Namespace : EasyBuildSystem.Features.Editor.Extensions.ReorderableList
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Editor.Extensions.ReorderableList
{
	public static class LayoutUtility
	{
		const float k_FoldoutAdjustingWidth = 12f;

		static bool HasFoldout(SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Generic:
					return true;

				case SerializedPropertyType.Vector4:
					return true;

				case SerializedPropertyType.Quaternion:
					return true;

				default:
					return false;
			}
		}

		static Rect FoldoutSildedRect(Rect rect)
		{
			rect.x += k_FoldoutAdjustingWidth;
			rect.width -= k_FoldoutAdjustingWidth;

			return rect;
		}

		const float k_SingleElementHeightMargin = 2f;
	
		public static Rect AdjustedRect(Rect rect, SerializedProperty property)
		{
			if (HasFoldout(property))
            {
                rect = FoldoutSildedRect(rect);
            }

            rect.y += k_SingleElementHeightMargin;

			return rect;
		}

		public static float ElementHeight(SerializedProperty property) => EditorGUI.GetPropertyHeight(property) + k_SingleElementHeightMargin * 2f;
	}
}
