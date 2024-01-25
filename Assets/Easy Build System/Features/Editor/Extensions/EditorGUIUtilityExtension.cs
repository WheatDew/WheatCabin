/// <summary>
/// Project : Easy Build System
/// Class : EditorGUIUtilityExtension.cs
/// Namespace : EasyBuildSystem.Features.Editor.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using System.Collections.Generic;

namespace EasyBuildSystem.Features.Editor.Extensions
{
    public class EditorGUIUtilityExtension
    {
        public static void DrawHeader(string title, string description)
        {
            GUILayout.Space(5f);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (title != string.Empty)
            {
                if (EditorGUIUtility.isProSkin)
                {
                    EditorGUILayout.LabelField(title, EditorStyles.whiteLargeLabel, GUILayout.Width(350), GUILayout.Height(20));
                }
                else
                { 
                    EditorGUILayout.LabelField(title, EditorStyles.largeLabel, GUILayout.Width(300), GUILayout.Height(20)); 
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (description != string.Empty)
            {
                GUI.enabled = false;
                GUILayout.Label(description.Replace("\n", "\n\n"), EditorStyles.wordWrappedMiniLabel);
                GUI.enabled = true;
                GUILayout.Space(5f);
            }
            else
            {
                GUILayout.Space(5f);
            }

            Rect lineRect = EditorGUILayout.GetControlRect(GUILayout.Height(1));
            lineRect.x += 3;
            lineRect.width -= 3;
            lineRect.height = 1;

            EditorGUI.DrawRect(lineRect, Color.white / 2);
            GUILayout.Space(5f);
            GUILayout.EndVertical();
        }

        public static void LinkLabel(string caption, string url)
        {
            GUIStyle style = GUI.skin.label;
            style.richText = true;
            caption = string.Format("<color=#3386FF>{0}</color>", caption);

            bool bClicked = GUILayout.Button(caption, style);

            Rect rect = GUILayoutUtility.GetLastRect();
            rect.width = style.CalcSize(new GUIContent(caption)).x;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (bClicked)
            {
                Application.OpenURL(url);
            }
        }

        public static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }

            layerMask.value = mask;

            return layerMask;
        }

        public static bool BeginFoldout(GUIContent content, bool state, bool indent = true, float indentSpacing = 16f)
        {
            BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.Width(250));
            GUILayout.Space(15);
            state = EditorGUILayout.Foldout(state, content, true);

            GUILayout.EndHorizontal();

            if (indent)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(indentSpacing);
            }

            GUILayout.BeginVertical();

            return state;
        }

        public static void EndFoldout(bool indent = true)
        {
            GUILayout.EndVertical();

            if (indent)
            {
                GUILayout.EndHorizontal();
            }
            
            EndVertical();
        }

        public static void BeginVertical(params GUILayoutOption[] layout)
        {
            int borderSize = 2;
            GUIStyle style = new GUIStyle
            {
                border = new RectOffset(borderSize, borderSize, borderSize, borderSize)
            };
            style.normal.background = Resources.Load<Texture2D>("UI/Editor/border");

            GUI.color = Color.black / 3f;
            GUILayout.BeginVertical(style, layout);
            GUI.color = Color.white;
#if UNITY_2021_1_OR_NEWER
            GUILayout.Space(5f);
#else
            GUILayout.Space(3f);
#endif
            GUILayout.BeginHorizontal();
            GUILayout.Space(5f);

            GUILayout.BeginVertical();
        }

        public static void EndVertical()
        {
            GUILayout.EndVertical();
            GUILayout.Space(5f);
            GUILayout.EndHorizontal();

#if UNITY_2021_1_OR_NEWER
            GUILayout.Space(5f);
#else
            GUILayout.Space(3f);
#endif
            GUILayout.EndVertical();

            GUILayout.Space(3f);
        }
    }
}