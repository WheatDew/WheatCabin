using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTTerrain
{
    public class TerrainGridEditor : MonoBehaviour
    {
        [SerializeField]
        private VirtualizingTreeView m_commandsList = null;

        [SerializeField]
        private BoolEditor m_zTestEditor = null;

        [SerializeField]
        private RangeEditor m_xSpacingEditor = null;

        [SerializeField]
        private RangeEditor m_zSpacingEditor = null;

        [SerializeField]
        private GameObject m_cutHolesEditor = null;

        [SerializeField]
        private Button m_cutHolesApplyButton = null;

        [SerializeField]
        private Button m_cutHolesCancelButton = null;


        private ToolCmd[] m_commands;
        private ITerrainGridTool m_terrainTool;
        private ICustomSelectionComponent m_customSelection;
        private IRuntimeEditor m_editor;
        private ILocalization m_localization;

        private bool m_isTerrainHandleSelected = false;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
                        
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

            m_localization = IOC.Resolve<ILocalization>();
            m_terrainTool = IOC.Resolve<ITerrainGridTool>();
            m_customSelection = IOC.Resolve<ICustomSelectionComponent>();
            m_customSelection.Selection.SelectionChanged += OnTerrainToolSelectionChanged;
                        
            if (m_xSpacingEditor != null)
            {
                m_xSpacingEditor.Min = 5;
                m_xSpacingEditor.Max = 40;
                m_xSpacingEditor.Init(m_terrainTool, m_terrainTool, Strong.PropertyInfo((ITerrainGridTool x) => x.XSpacing), null, m_localization.GetString("ID_RTTerrain_SelectionHandles_XSpacing", "X Space"), null, null, () => m_terrainTool.Refresh(), false);
            }

            if (m_zSpacingEditor != null)
            {
                m_zSpacingEditor.Min = 5;
                m_zSpacingEditor.Max = 40;
                m_zSpacingEditor.Init(m_terrainTool, m_terrainTool, Strong.PropertyInfo((ITerrainGridTool x) => x.ZSpacing), null, m_localization.GetString("ID_RTTerrain_SelectionHandles_ZSpacing", "Z Space"), null, null, () => m_terrainTool.Refresh(), false);
            }

            if (m_zTestEditor != null)
            {
                m_zTestEditor.Init(m_terrainTool, m_terrainTool, Strong.PropertyInfo((ITerrainGridTool x) => x.EnableZTest), null, m_localization.GetString("ID_RTTerrain_SelectionHandles_ZTest", "Z Test"));
            }
        }

        private void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            if(m_customSelection != null)
            {
                m_customSelection.Selection.SelectionChanged -= OnTerrainToolSelectionChanged;
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
        }

        private void Start()
        {
            UpdateFlags();
            m_commands = GetCommands().ToArray();
            m_commandsList.Items = m_commands;
        }

        private void OnEnable()
        {
            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged += OnSelectionChanged;
            }

            if(m_terrainTool != null)
            {
                m_terrainTool.IsActive = true;
            }
            
            UpdateFlags();
            m_commandsList.DataBindVisible();
        }

        private void OnDisable()
        {
            if(m_terrainTool != null)
            {
                m_terrainTool.IsActive = false;
            }

            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            EndCutHoles();
        }

        private List<ToolCmd> GetCommands()
        {
            return new List<ToolCmd>()
            {
                new ToolCmd(m_localization.GetString("ID_RTTerrain_SelectionHandles_ResetPosition", "Reset Position"), () => m_terrainTool.ResetPosition(), () => m_isTerrainHandleSelected),
                new ToolCmd(m_localization.GetString("ID_RTTerrain_SelectionHandles_CutHoles", "Cut Holes"), () => BeginCutHoles()),
                new ToolCmd(m_localization.GetString("ID_RTTerrain_SelectionHandles_ClearHoles", "Clear Holes"), () => m_terrainTool.ClearHoles()),
            };
        }

        private void BeginCutHoles()
        {
            if(m_cutHolesEditor != null)
            {
                m_cutHolesEditor.SetActive(true);
                m_commandsList.gameObject.SetActive(false);
                m_terrainTool.SelectObjectsMode = true;

                UnityEventHelper.AddListener(m_cutHolesApplyButton, button => button.onClick, CutHoles);
                UnityEventHelper.AddListener(m_cutHolesCancelButton, button => button.onClick, EndCutHoles);
            }
        }

        private void CutHoles()
        {
            m_terrainTool.CutHoles();
            EndCutHoles();
        }

        private void EndCutHoles()
        {
            if (m_cutHolesEditor != null)
            {
                m_cutHolesEditor.SetActive(false);
                m_terrainTool.SelectObjectsMode = false;
                m_commandsList.gameObject.SetActive(true);

                UnityEventHelper.RemoveListener(m_cutHolesApplyButton, button => button.onClick, CutHoles);
                UnityEventHelper.RemoveListener(m_cutHolesCancelButton, button => button.onClick, EndCutHoles);
            }
        }

        private void Update()
        {
            if(m_cutHolesEditor != null && m_cutHolesEditor.activeSelf)
            {
                if(m_editor.Input.GetKeyDown(KeyCode.Escape))
                {
                    EndCutHoles();
                }
            }
        }

        private void UpdateFlags()
        {
            GameObject[] selected = m_customSelection.Selection.gameObjects;
            if (selected != null && selected.Length > 0)
            {
                m_isTerrainHandleSelected = selected.Where(go => go.GetComponent<TerrainGridHandle>() != null).Any();
            }
            else
            {
                m_isTerrainHandleSelected = false;
            }
        }

        private void OnSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            UpdateFlags();
            m_commandsList.DataBindVisible();
        }

        private void OnTerrainToolSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            UpdateFlags();
            m_commandsList.DataBindVisible();
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
            if (cmd.Validate())
            {
                cmd.Run();
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            m_editor.DragDrop.RaiseBeginDrag(this, e.Items, e.PointerEventData);
        }

        private void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            m_editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            e.Cancel = true;
        }

        private void OnItemDrag(object sender, ItemArgs e)
        {
            m_editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        private void OnItemDragExit(object sender, EventArgs e)
        {
            m_editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            m_editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
            m_editor.DragDrop.RaiseDrop(e.PointerEventData);
        }
    }
}


