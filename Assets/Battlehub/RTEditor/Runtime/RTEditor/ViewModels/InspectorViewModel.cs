using Battlehub.RTCommon;
using System;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class InspectorViewModel : ViewModel
    {
        private Transform m_objectEditorParent;
        [Binding]
        public Transform ObjectEditorParent
        {
            get { return m_objectEditorParent; }
            set { m_objectEditorParent = value; }
        }

        private UnityObject[] m_selectedObjects;
        [Binding]
        public UnityObject[] SelectedObjects
        {
            get { return m_selectedObjects; }
            set 
            {
                if(m_selectedObjects != value)
                {
                    UnityObject[] unselectedObjects = m_selectedObjects;
                    m_selectedObjects = value;
                    OnSelectedObjectsChanged(unselectedObjects, m_selectedObjects);
                }   
            }
        }

        protected GameObject SelectedGameObject
        {
            get { return SelectedObject as GameObject; }
        }

        protected UnityObject SelectedObject
        {
            get { return m_selectedObjects != null && m_selectedObjects.Length > 0 ? m_selectedObjects[0] : null; }
        }

        private ISettingsComponent m_settingsComponent;

        private IEditorsMap m_editorsMap;
        protected IEditorsMap EditorsMap
        {
            get { return m_editorsMap; }
            set { m_editorsMap = value; }
        }
        
        private GameObject m_objectEditor;
        protected GameObject ObjectEditor
        {
            get { return m_objectEditor; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_editorsMap = IOC.Resolve<IEditorsMap>();
            m_settingsComponent = IOC.Resolve<ISettingsComponent>();
            if(m_settingsComponent != null)
            {
                m_settingsComponent.SelectedThemeChanged += OnSelectedThemeChanged;
            }

            CreateObjectEditor();
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            DestroyObjectEditor();
            m_editorsMap = null;
            m_objectEditor = null;
            if(m_settingsComponent != null)
            {
                m_settingsComponent.SelectedThemeChanged -= OnSelectedThemeChanged;
                m_settingsComponent = null;
            }

      
        }

        protected virtual void Update()
        {
            UnityObject obj = SelectedObject;
            if(obj == null)
            {
                DestroyObjectEditor();
            }
        }

        #region Bound UnityEvents

        public override void OnDeactivated()
        {
            base.OnDeactivated();
            OnSave();
        }

        public virtual void OnSave()
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsDirty && SelectedObjects != null && SelectedObjects.Length > 0)
            {
                editor.IsDirty = false;
                editor.IsBusy = true;
                editor.SaveAssets(SelectedObjects, result =>
                {
                    editor.IsBusy = false;
                });
            }
        }

        #endregion

        #region Methods

        private void OnSelectedThemeChanged(object sender, ThemeAsset oldValue, ThemeAsset newValue)
        {
            CreateObjectEditor();
        }

        protected virtual void OnSelectedObjectsChanged(UnityObject[] unselectedObjects, UnityObject[] selectedObjects)
        {
            if (m_objectEditor != null && unselectedObjects != null && unselectedObjects.Length > 0)
            {
                if (Editor.IsDirty)
                {
                    Editor.IsDirty = false;
                    Editor.SaveAssets(unselectedObjects, result =>
                    {
                        CreateObjectEditor();
                    });
                }
                else
                {
                    CreateObjectEditor();
                }
            }
            else
            {
                CreateObjectEditor();
            }
        }

        protected bool OfSameType(UnityObject[] objects, out Type type)
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

        protected virtual void CreateObjectEditor()
        {
            DestroyObjectEditor();
            
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (SelectedObject == null)
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
                string editorName = editorPrefab.name;

                editorPrefab.SetActive(false);
                m_objectEditor = Instantiate(editorPrefab);
                m_objectEditor.transform.SetParent(m_objectEditorParent, false);
                m_objectEditor.transform.SetAsFirstSibling();
                m_objectEditor.name = editorName;
                m_objectEditor.SetActive(true);
                editorPrefab.SetActive(wasActive);
            }
        }

        protected virtual void DestroyObjectEditor()
        {
            if (m_objectEditor != null)
            {
                Destroy(m_objectEditor);
                m_objectEditor = null;
            }
        }

        #endregion
    }
}
