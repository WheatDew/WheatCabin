using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.Spline3;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.MeshDeformer3
{
    [DefaultExecutionOrder(-1)]
    public class MeshDeformerView : RuntimeWindow
    {
        [SerializeField]
        private Toggle m_toggleObject = null;
        [SerializeField]
        private Toggle m_toggleControlPoints = null;
        [SerializeField]
        private Toggle m_toggleSettings = null;

        [SerializeField]
        private VirtualizingTreeView m_commandsList = null;
        [SerializeField]
        private GameObject m_settingsPanel = null;

        [SerializeField]
        private GameObject m_deformerSettingsSection = null;
        [SerializeField]
        private BoolEditor m_showTerminalPointsEditor = null;
        [SerializeField]
        private BoolEditor m_showOriginalMeshEditor = null;
        [SerializeField]
        private RangeIntEditor m_pointsPerSegmentEditor = null;

        private ToolCmd[] m_commands;
        private bool m_isMeshDeformerSelected;
        private bool m_isDragging;
        private IMeshDeformerTool m_tool;
        private IRuntimeEditor m_runtimeEditor;
        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();

            m_localization = IOC.Resolve<ILocalization>();

            m_tool = IOC.Resolve<IMeshDeformerTool>();
            m_tool.ModeChanged += OnModeChanged;
            m_tool.SelectionChanged += OnSelectionChanged;

            m_runtimeEditor = IOC.Resolve<IRuntimeEditor>();
            m_runtimeEditor.SceneLoading += OnSceneLoading;
            m_runtimeEditor.SceneLoaded += OnSceneLoaded;
            m_runtimeEditor.Selection.SelectionChanged += OnEditorSelectionChanged;
            m_runtimeEditor.Undo.UndoCompleted += OnEditorUndo;
            m_runtimeEditor.Undo.RedoCompleted += OnEditorRedo;
            m_runtimeEditor.Undo.StateChanged += OnEditorUndoStateChanged;

            m_commandsList.ItemClick += OnItemClick;
            m_commandsList.ItemDataBinding += OnItemDataBinding;
            m_commandsList.ItemExpanding += OnItemExpanding;
            m_commandsList.ItemBeginDrag += OnItemBeginDrag;
            m_commandsList.ItemDrop += OnItemDrop;
            m_commandsList.ItemDragEnter += OnItemDragEnter;
            m_commandsList.ItemDragExit += OnItemDragExit;
            m_commandsList.ItemEndDrag += OnItemEndDrag;

            m_commandsList.CanEdit = false;
            m_commandsList.CanReorder = false;
            m_commandsList.CanReparent = false;
            m_commandsList.CanSelectAll = false;
            m_commandsList.CanUnselectAll = true;
            m_commandsList.CanRemove = false;

            UnityEventHelper.AddListener(m_toggleObject, o => o.onValueChanged, OnObjectMode);
            UnityEventHelper.AddListener(m_toggleControlPoints, o => o.onValueChanged, OnControlPointMode);
            UnityEventHelper.AddListener(m_toggleSettings, o => o.onValueChanged, OnSettings);

            m_showTerminalPointsEditor.Init(m_tool, Strong.PropertyInfo((IMeshDeformerTool x) => x.ShowTerminalPoints), m_localization.GetString("ID_RTDeformer_View_ShowTerminalPoints"));

            m_showOriginalMeshEditor.Init(m_tool, Strong.PropertyInfo((IMeshDeformerTool x) => x.ShowOriginal), m_localization.GetString("ID_RTDeformer_View_ShowOriginal"));

            m_pointsPerSegmentEditor.Min = 0;
            m_pointsPerSegmentEditor.Max = 10;
            m_pointsPerSegmentEditor.Init(m_tool, Strong.PropertyInfo((IMeshDeformerTool x) => x.PointsPerSegment), m_localization.GetString("ID_RTDeformer_View_PointsPerSegment"));
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_tool != null)
            {
                m_tool.ModeChanged -= OnModeChanged;
                m_tool.SelectionChanged -= OnSelectionChanged;
                m_tool.Mode = MeshDeformerToolMode.Object;
            }

            if (m_runtimeEditor != null)
            {
                if (m_runtimeEditor.Selection != null)
                {
                    m_runtimeEditor.Selection.SelectionChanged -= OnEditorSelectionChanged;
                }
                m_runtimeEditor.SceneLoading -= OnSceneLoading;
                m_runtimeEditor.SceneLoaded -= OnSceneLoaded;
                if(m_runtimeEditor.Undo != null)
                {
                    m_runtimeEditor.Undo.UndoCompleted -= OnEditorUndo;
                    m_runtimeEditor.Undo.RedoCompleted -= OnEditorRedo;
                    m_runtimeEditor.Undo.StateChanged -= OnEditorUndoStateChanged;
                }
            }

            if (m_commandsList != null)
            {
                m_commandsList.ItemClick -= OnItemClick;
                m_commandsList.ItemDataBinding -= OnItemDataBinding;
                m_commandsList.ItemExpanding -= OnItemExpanding;
                m_commandsList.ItemBeginDrag -= OnItemBeginDrag;
                m_commandsList.ItemDrop -= OnItemDrop;
                m_commandsList.ItemDragEnter -= OnItemDragEnter;
                m_commandsList.ItemDragExit -= OnItemDragExit;
                m_commandsList.ItemEndDrag -= OnItemEndDrag;
            }

            UnityEventHelper.RemoveListener(m_toggleObject, o => o.onValueChanged, OnObjectMode);
            UnityEventHelper.RemoveListener(m_toggleControlPoints, o => o.onValueChanged, OnControlPointMode);
            UnityEventHelper.RemoveListener(m_toggleSettings, o => o.onValueChanged, OnSettings);
        }

        protected virtual void Start()
        {
            UpdateFlagsAndDataBind();
        }

        protected virtual void LateUpdate()
        {
            RuntimeWindow window = Editor.ActiveWindow;
            if (window == null || window.WindowType != RuntimeWindowType.Scene || !window.IsPointerOver)
            {
                return;
            }
            if (m_tool.Mode == MeshDeformerToolMode.Object)
            {
                return;
            }

            IInput input = Editor.Input;
            bool remove = input.GetKeyDown(KeyCode.Delete);
            if (m_isMeshDeformerSelected && remove && m_tool.CanRemove())
            {
                Remove();
            }
 
            if (Editor.Tools.ActiveTool == null && input.GetPointerDown(0))
            {
                if (Editor.ActiveWindow != null )
                {
                    m_tool.SelectControlPoint(window.Camera, input.GetPointerXY(0));
                }
            }
            else
            {
                KeyCode key = Application.isEditor ? KeyCode.LeftControl : KeyCode.LeftShift;

                bool extend = input.GetKey(key) && !m_isDragging || input.GetKeyDown(key);
                m_isDragging = m_tool.DragControlPoint(extend);
            }
        }

        private void OnObjectMode(bool value)
        {
            if(value)
            {
                m_tool.Mode = MeshDeformerToolMode.Object;
            }
        }

        private void OnControlPointMode(bool value)
        {
            if(value)
            {
                m_tool.Mode = MeshDeformerToolMode.ControlPoint;
            }
        }

        private void OnSettings(bool value)
        {
            if(value)
            {
                m_tool.Mode = MeshDeformerToolMode.Settings;
            }
        }

        private void OnSelectionChanged()
        {
            UpdateFlagsAndDataBind();
        }

        private void OnModeChanged()
        {
            if (m_tool.Mode == MeshDeformerToolMode.Object)
            {
                m_toggleObject.isOn = true;
            }
            else if (m_tool.Mode == MeshDeformerToolMode.ControlPoint)
            {
                m_toggleControlPoints.isOn = true;
            }
            else if(m_tool.Mode == MeshDeformerToolMode.Settings)
            {
                m_toggleSettings.isOn = true;
            }

            m_settingsPanel.SetActive(m_tool.Mode == MeshDeformerToolMode.Settings);
            m_commandsList.gameObject.SetActive(m_tool.Mode != MeshDeformerToolMode.Settings);

            UpdateFlagsAndDataBind();
        }

        private List<ToolCmd> GetCommands()
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            List<ToolCmd> commands = new List<ToolCmd>();
            if(m_tool.Mode == MeshDeformerToolMode.Object)
            {
                ToolCmd deformCmd = new ToolCmd(lc.GetString("ID_RTDeformer_View_Deform", "Deform"), () => DeformAxis(Axis.Z), CanDeform);
                deformCmd.Children = new List<ToolCmd>
                {
                    new ToolCmd(lc.GetString("ID_RTDeformer_View_DeformX", "Deform X"), () => DeformAxis(Axis.X), CanDeform) { Parent = deformCmd },
                    new ToolCmd(lc.GetString("ID_RTDeformer_View_DeformY", "Deform Y"), () => DeformAxis(Axis.Y), CanDeform) { Parent = deformCmd },
                };
                commands.Add(deformCmd);

                ToolCmd subdivideMeshCmd = new ToolCmd(lc.GetString("ID_RTDeformer_View_SubdivideMesh"), Subdivide, CanSubdivide);
                commands.Add(subdivideMeshCmd);

                ToolCmd removeDeformerCmd = new ToolCmd(lc.GetString("ID_RTDeformer_View_RemoveDeformer"), RemoveDeformer, CanDestroyDeformer);
                commands.Add(removeDeformerCmd);
            }
            else if(m_tool.Mode == MeshDeformerToolMode.ControlPoint)
            {
                commands.Add(new ToolCmd(lc.GetString("ID_RTDeformer_View_Append", "Append"), Append, CanAppend));
                commands.Add(new ToolCmd(lc.GetString("ID_RTDeformer_View_Remove", "Remove"), Remove, CanRemove));
            }

            return commands;
        }

        private bool CanDeform()
        {
            return m_tool.CanDeform();
        }

        private void DeformAxis(Axis axis)
        {
            m_tool.DeformAxis(axis);
            m_tool.Mode = MeshDeformerToolMode.ControlPoint;
        }

        private bool CanDestroyDeformer()
        {
            return m_tool.CanDestroy();
        }

        private void RemoveDeformer()
        {
            m_tool.Destroy();
        }

        private bool CanSubdivide()
        {
            return m_tool.CanSubdivide();
        }

        private void Subdivide()
        {
            m_tool.Subdivide();
        }

        private bool CanAppend()
        {
            if(!m_isMeshDeformerSelected)
            {
                return false;
            }
            return m_tool.CanAppend();
        }

        private void Append()
        {
            m_tool.Append();
        }

        private bool CanRemove()
        {
            if(!m_isMeshDeformerSelected)
            {
                return false;
            }
            return m_tool.CanRemove();
        }

        private void Remove()
        {
            m_tool.Remove();
        }

        private void UpdateFlags()
        {
            GameObject[] selected = Editor.Selection.gameObjects;
            if (selected != null && selected.Length > 0)
            {
                m_isMeshDeformerSelected = selected.Where(go =>
                {
                    if (go.GetComponent<Deformer>())
                    {
                        return true;
                    }

                    ControlPointPicker picker = go.GetComponentInParent<ControlPointPicker>();
                    return picker != null && picker.Selection != null && picker.Selection.GetSpline() is Deformer && picker.Selection.Index >= 0;
                }).Any();
            }
            else
            {
                m_isMeshDeformerSelected = false;
            }

            if (m_deformerSettingsSection != null)
            {
                m_deformerSettingsSection.SetActive(m_isMeshDeformerSelected);
            }
        }

        private void UpdateFlagsAndDataBind()
        {
            UpdateFlags();
            m_commands = GetCommands().ToArray();
            m_commandsList.Items = m_commands;
            if(m_commands.Length > 0)
            {
                m_commandsList.Expand(m_commands[0]);
            }
        }

        private void OnEditorSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            UpdateFlags();
            m_commandsList.DataBindVisible();
        }

        private void OnSceneLoading()
        {
            if (m_tool != null)
            {
                m_tool.Mode = MeshDeformerToolMode.Object;
            }
        }

        private void OnSceneLoaded()
        {
            UpdateFlagsAndDataBind();
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>();
            ToolCmd cmd = (ToolCmd)e.Item;
            text.text = cmd.Text;

            bool isValid = cmd.Validate();
            Color color = text.color;
            color.a = isValid ? 1 : 0.5f;
            text.color = color;
          
            e.CanDrag = cmd.CanDrag;
            e.HasChildren = cmd.HasChildren;
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            ToolCmd cmd = (ToolCmd)e.Item;
            e.Children = cmd.Children;
        }

        private void OnItemClick(object sender, ItemArgs e)
        {
            ToolCmd cmd = (ToolCmd)e.Items[0];
            if(cmd.Validate())
            {
                cmd.Run();
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseBeginDrag(this, e.Items, e.PointerEventData);
        }

        private void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            e.Cancel = true;
        }

        private void OnItemDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        private void OnItemDragExit(object sender, EventArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void OnEditorUndoStateChanged()
        {
            UpdateFlagsAndDataBind();
        }

        private void OnEditorRedo()
        {
            UpdateFlagsAndDataBind();
        }

        private void OnEditorUndo()
        {
            UpdateFlagsAndDataBind();
        }

    }
}


