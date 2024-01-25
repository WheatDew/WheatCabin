/// <summary>
/// Project : Master Survival Kit
/// Class : CustomHeaderDrawer.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Bases.Drawers.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.IO;

using UnityEngine;
using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Bases.Drawers.Editor
{
    [CustomPropertyDrawer(typeof(CustomHeaderAttribute))]
    public class CustomHeaderDrawer : DecoratorDrawer
    {
        float m_BaseHeight;

        CustomHeaderAttribute HeaderAttribute { get { return (CustomHeaderAttribute)attribute; } }

        public override void OnGUI(Rect position)
        {
            m_BaseHeight = 0;

            if (HeaderAttribute.Description == string.Empty)
            {
                position.yMin += EditorGUIUtility.singleLineHeight * .2f;
                position = EditorGUI.IndentedRect(position);
                position.y -= 5;
                GUI.Label(position, HeaderAttribute.Text, EditorStyles.boldLabel);
                position.y += 8;

                m_BaseHeight += 40;

                Rect lineRect = position;

                lineRect.yMin += 25f;
                lineRect.x += 3;
                lineRect.width -= 3;
                lineRect.height = 1;

                EditorGUI.DrawRect(lineRect, Color.white / 2);

                return;
            }

            position.yMin += EditorGUIUtility.singleLineHeight * .2f;
            position = EditorGUI.IndentedRect(position);
            position.y -= 15;
            GUI.Label(position, HeaderAttribute.Text, EditorStyles.boldLabel);
            position.y += 15;
            m_BaseHeight += 25;

            int currentLineCount = 0;
            int lineCount = HeaderAttribute.Description.Split('\n').Length;

            using (StringReader reader = new StringReader(HeaderAttribute.Description))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    position.yMin += EditorGUIUtility.singleLineHeight * 1.2f;
                    position = EditorGUI.IndentedRect(position);

                    GUI.enabled = false;
                    GUI.Label(position, line, EditorStyles.wordWrappedMiniLabel);
                    GUI.enabled = true;

                    currentLineCount++;

                    m_BaseHeight += 30;

                    if (currentLineCount >= lineCount)
                    {
                        Rect lineRect = position;

                        lineRect.yMin += 22f;
                        lineRect.x += 3;
                        lineRect.width -= 3;
                        lineRect.height = 1;

                        EditorGUI.DrawRect(lineRect, Color.white / 2);
                    }
                }
            }
        }

        public override float GetHeight()
        {
            return m_BaseHeight;
        }
    }
}