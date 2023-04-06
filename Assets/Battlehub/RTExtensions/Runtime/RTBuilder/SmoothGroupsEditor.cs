using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class SmoothGroupsEditor : MonoBehaviour
    {
        [SerializeField]
        private PropertyEditor m_normalsScaleEditor = null;

        [SerializeField]
        private PropertyEditor m_previewOpacityEditor = null;

        [SerializeField]
        private Transform m_contentPanel = null;

        [SerializeField]
        private SmoothGroupEditor m_groupPrefab = null;

        private IRTE m_editor;
        private IProBuilderTool m_proBuilderTool = null;

        private Dictionary<PBMesh, PBSmoothGroupData> m_smoothGroups = new Dictionary<PBMesh, PBSmoothGroupData>();
        private MeshSelection m_selection;

        private float NormalsScale
        {
            get { return PBSmoothGroupData.NormalsScale; }
            set
            {
                PBSmoothGroupData.NormalsScale = value;
                PlayerPrefs.SetFloat("SmoothGroupsEditor.NormalsScale", value);
            }
        }

        private float PreviewOpacity
        {
            get { return PBSmoothGroupData.PreviewOpacity; }
            set
            {
                PBSmoothGroupData.PreviewOpacity = value;
                PlayerPrefs.SetFloat("SmoothGroupsEditor.PreviewOpacity", value);
            }
        }

        private void Awake()
        {
            m_proBuilderTool = IOC.Resolve<IProBuilderTool>();

            NormalsScale = PlayerPrefs.GetFloat("SmoothGroupsEditor.NormalsScale", 0.2f);
            PreviewOpacity = PlayerPrefs.GetFloat("SmoothGroupsEditor.PreviewOpacity", 0.5f);

            ILocalization lc = IOC.Resolve<ILocalization>();
            if (m_normalsScaleEditor != null)
            {
                m_normalsScaleEditor.Init(this, Strong.PropertyInfo((SmoothGroupsEditor x) => x.NormalsScale), lc.GetString("ID_RTBuilder_SmoothGroupsEditor_NormalsScale"), false);
            }
            if(m_previewOpacityEditor != null)
            {
                m_previewOpacityEditor.Init(this, Strong.PropertyInfo((SmoothGroupsEditor x) => x.PreviewOpacity), lc.GetString("ID_RTBuilder_SmoothGroupsEditor_PreviewOpacity"), false);
            }
        }

        private void OnEnable()
        {
            OnSelectionChanged();
            m_proBuilderTool.ModeChanged += OnModeChanged;
            m_proBuilderTool.MeshesChanged += OnMeshesChanged;
            m_proBuilderTool.SelectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            if (m_proBuilderTool != null)
            {
                m_proBuilderTool.ModeChanged -= OnModeChanged;
                m_proBuilderTool.MeshesChanged -= OnMeshesChanged;
                m_proBuilderTool.SelectionChanged -= OnSelectionChanged;
            }
            ClearSmoothGroups();
        }

        private void OnSelectionChanged()
        {
            m_selection = null;
            IMeshEditor meshEditor = m_proBuilderTool.GetEditor();
            if (meshEditor == null)
            {
                return;
            }

            m_selection = meshEditor.GetSelection();

            ClearSmoothGroups();
            CreateSmoothGroups();
            UpdateEditorsPanel();
        }

        private void OnModeChanged(ProBuilderToolMode oldMode)
        {
            ProBuilderToolMode mode = m_proBuilderTool.Mode;
            if (mode != ProBuilderToolMode.Edge && mode != ProBuilderToolMode.Vertex && mode != ProBuilderToolMode.Face)
            {
                ClearSmoothGroups();
                UpdateEditorsPanel();
            }
        }

        private void OnMeshesChanged()
        {
            RebuildSmoothGroups();
        }

        private void ClearSmoothGroups()
        {
            foreach (KeyValuePair<PBMesh, PBSmoothGroupData> kvp in m_smoothGroups)
            {
                PBSmoothGroupData data = kvp.Value;
                data.Clear();
            }
            m_smoothGroups.Clear();
        }

        private void CreateSmoothGroups()
        {   
            if(m_selection == null)
            {
                return;
            }

            foreach (PBMesh pbMesh in m_selection.GetSelectedMeshes())
            {
                if (!m_smoothGroups.ContainsKey(pbMesh))
                {
                    m_smoothGroups.Add(pbMesh, new PBSmoothGroupData(pbMesh));
                }
            }
        }

        private void RebuildSmoothGroups()
        {
            if (m_selection == null)
            {
                return;
            }

            foreach (PBMesh pbMesh in m_selection.GetSelectedMeshes())
            {
                m_smoothGroups[pbMesh].Rebuild(pbMesh);
            }
        }

        private void UpdateEditorsPanel()
        {
            foreach (Transform child in m_contentPanel)
            {
                SmoothGroupEditor editor = child.GetComponent<SmoothGroupEditor>();
                Unsubscribe(editor);

                Destroy(child.gameObject);
            }

            IMeshEditor meshEditor = m_proBuilderTool.GetEditor();
            if (meshEditor == null)
            {
                return;
            }

            MeshSelection selection = meshEditor.GetSelection();
            if (selection == null)
            {
                return;
            }

            selection = selection.ToFaces(false);
            if (!selection.HasFaces)
            {
                return;
            }

            const int maxVisibleGroups = 8;
            int index = 0;

            foreach(PBMesh pbMesh in selection.GetSelectedMeshes())
            {
                index++;
                if(index == maxVisibleGroups)
                {
                    return;
                }

                SmoothGroupEditor editor = Instantiate(m_groupPrefab, m_contentPanel);
                editor.Data = m_smoothGroups[pbMesh];
                Subscribe(editor);
            }
        }

        private void Subscribe(SmoothGroupEditor editor)
        {
            editor.Clear += OnClear;
            editor.ExpandSelection += OnExpandSelection;
            editor.Smooth += OnSmooth;
        }

        private void Unsubscribe(SmoothGroupEditor editor)
        {
            if (editor == null)
            {
                return;
            }
            editor.Clear -= OnClear;
            editor.ExpandSelection -= OnExpandSelection;
            editor.Smooth -= OnSmooth;
        }

        private void OnSmooth(object sender, SmoothGroupArgs e)
        {
            SmoothGroupEditor editor = (SmoothGroupEditor)sender;
            PBMesh pbMesh = editor.Data.PBMesh;

            IMeshEditor meshEditor = m_proBuilderTool.GetEditor();
            MeshEditorState oldState = meshEditor.GetState(false);
            PBSmoothing.SetGroup(pbMesh, meshEditor.GetSelection(), e.Index);
            
            m_smoothGroups[pbMesh].Rebuild(pbMesh);
            m_proBuilderTool.TryUpdatePivotTransform();

            MeshEditorState newState = meshEditor.GetState(false);
            m_proBuilderTool.RecordState(oldState, newState, true);
        }

        private void OnExpandSelection(object sender, EventArgs e)
        {
            IMeshEditor meshEditor = m_proBuilderTool.GetEditor();
            MeshSelection selection = meshEditor.GetSelection();
            MeshEditorState oldState = meshEditor.GetState(false);
            selection = PBSmoothing.ExpandSelection(selection);
            if(m_proBuilderTool.Mode == ProBuilderToolMode.Vertex)
            {
                selection = selection.ToVertices(false);
            }
            else if(m_proBuilderTool.Mode == ProBuilderToolMode.Edge)
            {
                selection = selection.ToEdges(false);
            }

            meshEditor.SetSelection(selection);
            m_proBuilderTool.TryUpdatePivotTransform();

            MeshEditorState newState = meshEditor.GetState(false);
            m_proBuilderTool.RecordState(oldState, newState, true);
        }

        private void OnClear(object sender, EventArgs e)
        {
            SmoothGroupEditor editor = (SmoothGroupEditor)sender;
            PBMesh pbMesh = editor.Data.PBMesh;

            IMeshEditor meshEditor = m_proBuilderTool.GetEditor();
            MeshEditorState oldState = meshEditor.GetState(false);
            PBSmoothing.ClearGroup(pbMesh, meshEditor.GetSelection());

            m_smoothGroups[pbMesh].Rebuild(pbMesh);
            m_proBuilderTool.TryUpdatePivotTransform();

            MeshEditorState newState = meshEditor.GetState(false);
            m_proBuilderTool.RecordState(oldState, newState, true);
        }

       
    }
}

