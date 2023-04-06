using System;
using System.Linq;
using UnityEngine;

using Battlehub.RTCommon;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class InspectorView : RuntimeWindow
    {
        [SerializeField]
        private Transform m_panel = null;

        private GameObject m_editor;
        private IEditorsMap m_editorsMap;
        private ISettingsComponent m_settingsComponent;

        private IRuntimeSelection m_selectionOverride;

        private GameObject SelectedGameObject
        {
            get { return SelectedObject as GameObject; }
        }

        private UnityObject SelectedObject
        {
            get
            {
                if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
                {
                    return Editor.Selection.activeObject;
                }

                return m_selectionOverride.activeObject;
            }
        }

        private UnityObject[] SelectedObjects
        {
            get
            {
                if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
                {
                    return Editor.Selection.objects;
                }

                return m_selectionOverride.objects;
            }
        }

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Inspector;
            base.AwakeOverride();
        }

        private void Start()
        {
            m_editorsMap = IOC.Resolve<IEditorsMap>();

            m_selectionOverride = IOCContainer.Resolve<IRuntimeSelection>();
            if (m_selectionOverride != null)
            {
                m_selectionOverride.SelectionChanged += OnSelectionChanged;
            }

            Editor.Selection.SelectionChanged += OnEditorSelectionChanged;
            CreateEditor();

            m_settingsComponent = IOC.Resolve<ISettingsComponent>();
            if(m_settingsComponent != null)
            {
                m_settingsComponent.SelectedThemeChanged += OnSelectedThemeChanged;
            }
        }


        protected virtual void Update()
        {
            UpdateOverride();
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();
            UnityObject obj = SelectedObject;
            if (obj == null)
            {
                DestroyEditor();
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            if (m_selectionOverride != null)
            {
                m_selectionOverride.SelectionChanged -= OnSelectionChanged;
                m_selectionOverride = null;
            }

            if (m_settingsComponent != null)
            {
                m_settingsComponent.SelectedThemeChanged -= OnSelectedThemeChanged;
                m_settingsComponent = null;
            }
        }
    

        private void OnEditorSelectionChanged(UnityObject[] unselectedObjects)
        {
            if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
            {
                SaveAndCreateEditor(unselectedObjects);
            }
        }

        private void OnSelectionChanged(UnityObject[] unselectedObjects)
        {
            SaveAndCreateEditor(unselectedObjects);
        }

        private void SaveAndCreateEditor(UnityObject[] unselectedObjects)
        {
            if (m_editor != null && unselectedObjects != null && unselectedObjects.Length > 0)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                if (editor.IsDirty)
                {
                    editor.IsDirty = false;
                    editor.SaveAssets(unselectedObjects, result =>
                    {
                        CreateEditor();
                    });
                }
                else
                {
                    CreateEditor();
                }
            }
            else
            {
                CreateEditor();
            }
        }


        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsDirty && SelectedObjects != null && SelectedObjects.Length > 0)
            {
                editor.IsDirty = false;
                editor.SaveAssets(SelectedObjects, result =>
                {
                });
            }
        }

        private void DestroyEditor()
        {
            if (m_editor != null)
            {
                Destroy(m_editor);
            }
        }

        private bool OfSameType(UnityObject[] objects, out Type type)
        {
            type = objects[0].GetType();
            for (int i = 1; i < objects.Length; ++i)
            {
                if (type != objects[i].GetType())
                {
                    return false;
                }
            }
            return true;
        }

        private void OnSelectedThemeChanged(object sender, ThemeAsset oldValue, ThemeAsset newValue)
        {
            CreateEditor();
        }

        private void CreateEditor()
        {
            DestroyEditor();

            if (SelectedObject == null)
            {
                return;
            }

            if (SelectedObjects[0] == null)
            {
                return;
            }


            UnityObject[] selectedObjects = SelectedObjects.Where(o => o != null).ToArray();
            Type objType;
            if (!OfSameType(selectedObjects, out objType))
            {
                return;
            }

            ExposeToEditor exposeToEditor = null;
            if (objType == typeof(GameObject))
            {
                exposeToEditor = SelectedGameObject.GetComponent<ExposeToEditor>();
                if (exposeToEditor != null && !exposeToEditor.CanInspect)
                {
                    return;
                }
            }

            GameObject editorPrefab;
            if (objType == typeof(Material))
            {
                Material mat = selectedObjects[0] as Material;
                if (mat.shader == null)
                {
                    return;
                }

                Shader shader = mat.shader;
                for (int i = 0; i < selectedObjects.Length; ++i)
                {
                    Material material = (Material)selectedObjects[i];
                    if (material.shader != shader)
                    {
                        return;
                    }
                }

                editorPrefab = m_editorsMap.GetMaterialEditor(mat.shader);
            }
            else
            {
                if (!m_editorsMap.IsObjectEditorEnabled(objType))
                {
                    return;
                }
                editorPrefab = m_editorsMap.GetObjectEditor(objType);
            }

            if (editorPrefab != null)
            {
                bool wasActive = editorPrefab.activeSelf;
                editorPrefab.SetActive(false);
                m_editor = Instantiate(editorPrefab);
                m_editor.transform.SetParent(m_panel, false);
                m_editor.transform.SetAsFirstSibling();
                m_editor.SetActive(true);
                editorPrefab.SetActive(wasActive);
            }
        }
    }
}
