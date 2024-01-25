using System;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Editor.Extensions.ReadMe
{
    public class ReadMe : ScriptableObject
    {
        public Section[] Sections;

        [Serializable]
        public class Section
        {
            [Serializable]
            public class ExtraLink
            {
                public string Link;
                public string Url;
            }

            public string Title, Description;
            public ExtraLink[] Links;
        }
    }

    [InitializeOnLoad, CustomEditor(typeof(ReadMe))]
    public class ReadMeInspector : UnityEditor.Editor
    {
        private static readonly float Spacing = 16f;

        private bool Initialized;

        private GUIStyle LinkStyle;

        private GUIStyle HeadingStyle;

        private GUIStyle BodyStyle;

        static ReadMeInspector() { }

        private void Init()
        {
            if (Initialized)
            {
                return;
            }

            BodyStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 13,
                wordWrap = true,
                richText = true
            };

            HeadingStyle = new GUIStyle(BodyStyle)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };
            
            LinkStyle = new GUIStyle(BodyStyle)
            {
                wordWrap = false
            };

            LinkStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0f, 2f, 2f) : new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            LinkStyle.stretchWidth = false;
            LinkStyle.wordWrap = false;

            Initialized = true;
        }

        private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            Rect LabelPosition = GUILayoutUtility.GetRect(label, LinkStyle, options);

            Handles.BeginGUI();
            Handles.color = LinkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(LabelPosition.xMin, LabelPosition.yMax), new Vector3(LabelPosition.xMax, LabelPosition.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(LabelPosition, MouseCursor.Link);

            return GUI.Button(LabelPosition, label, LinkStyle);
        }

        protected override void OnHeaderGUI()
        {
            ReadMe Target = (ReadMe)target;

            Init();
        }

        public override void OnInspectorGUI()
        {
            ReadMe Target = (ReadMe)target;

            Init();

            foreach (ReadMe.Section Section in Target.Sections)
            {
                float NewSpacing = Spacing;

                if (string.IsNullOrEmpty(Section.Title))
                {
                    NewSpacing = 6;
                }

                GUILayout.Space(NewSpacing);

                if (!string.IsNullOrEmpty(Section.Title))
                {
                    GUILayout.Label(Section.Title.Replace("\\n", "\n"), HeadingStyle);
                    GUILayout.Space(8f);
                }

                if (!string.IsNullOrEmpty(Section.Description))
                {
                    GUILayout.Label(Section.Description.Replace("\\n", "\n").Replace("\n", "\n\n"), BodyStyle);
                } 

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();

                if (Section.Links != null)
                {
                    for (int i = 0; i < Section.Links.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(Section.Links[i].Link))
                        {
                             if (LinkLabel(new GUIContent(Section.Links[i].Link)))
                            {
                                Application.OpenURL(Section.Links[i].Url);
                            }
                            GUILayout.Space(5);
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
