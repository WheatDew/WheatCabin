using UnityEngine;
using Battlehub.RTEditor;
using Battlehub.Utils;
using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class UVAutoEditorPanel : MonoBehaviour
    {
        [SerializeField]
        private EnumEditor m_fillModeEditor = null;

        [SerializeField]
        private EnumEditor m_anchorEditor = null;

        [SerializeField]
        private Vector2Editor m_offsetEditor = null;

        [SerializeField]
        private RangeEditor m_rotationEditor = null;

        [SerializeField]
        private Vector2Editor m_tilingEditor = null;

        [SerializeField]
        private BoolEditor m_worldSpaceEditor = null;

        [SerializeField]
        private BoolEditor m_flipUEditor = null;

        [SerializeField]
        private BoolEditor m_flipVEditor = null;

        [SerializeField]
        private BoolEditor m_swapUVEditor = null;

        [SerializeField]
        private Button m_btnGroupFaces = null;

        [SerializeField]
        private Button m_btnUngroupFaces = null;

        [SerializeField]
        private Button m_selectFaceGroup = null;

        [SerializeField]
        private Button m_resetUVs = null;

        private IRTE m_editor;
        private ILocalization m_localization;

        private IProBuilderTool m_tool;
        public IProBuilderTool Tool
        {
            get { return m_tool; }
            set
            {
                m_tool = value;
            }
        }

        private void Awake()
        {
            Tool = IOC.Resolve<IProBuilderTool>();
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_localization = IOC.Resolve<ILocalization>();
        
            if(m_fillModeEditor != null)
            {
                m_fillModeEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.fill), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_Fill", "Fill"));
            }

            if (m_anchorEditor != null)
            {
                m_anchorEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.anchor), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_Anchor", "Anchor"));
            }

            if (m_offsetEditor != null)
            {
                m_offsetEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.offset), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_Offset", "Offset"));
            }

            if(m_rotationEditor != null)
            {
                m_rotationEditor.Min = 0;
                m_rotationEditor.Max = 360;
                m_rotationEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.rotation), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_Rotation", "Rotation"));
            }

            if(m_tilingEditor != null)
            {
                m_tilingEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.scale), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_Tiling", "Tiling"));
            }

            if(m_worldSpaceEditor != null)
            {
                m_worldSpaceEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.useWorldSpace), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_WorldSpace", "World Space"));
            }

            if(m_flipUEditor != null)
            {
                m_flipUEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.flipU), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_FlipU", "Flip U"));
            }

            if(m_flipVEditor != null)
            {
                m_flipVEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.flipV), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_FlipV", "Flip V"));
            }

            if(m_swapUVEditor != null)
            {
                m_swapUVEditor.Init(m_tool.UV, m_tool.UV, Strong.PropertyInfo((PBAutoUnwrapSettings x) => x.swapUV), null, m_localization.GetString("ID_RTBuilder_UVEditorAuto_SwapUV", "Swap UV"));
            }

            if(m_btnGroupFaces != null)
            {
                m_btnGroupFaces.onClick.AddListener(OnGroupFaces);
            }

            if(m_btnUngroupFaces != null)
            {
                m_btnUngroupFaces.onClick.AddListener(OnUngroupFaces);
            }

            if(m_selectFaceGroup != null)
            {
                m_selectFaceGroup.onClick.AddListener(OnSelectFaceGroup);
            }

            if(m_resetUVs != null)
            {
                m_resetUVs.onClick.AddListener(OnResetUVs);
            }
        }

        private void OnDestroy()
        {
            Tool = null;

            if (m_btnGroupFaces != null)
            {
                m_btnGroupFaces.onClick.RemoveListener(OnGroupFaces);
            }

            if (m_btnUngroupFaces != null)
            {
                m_btnUngroupFaces.onClick.RemoveListener(OnUngroupFaces);
            }

            if (m_selectFaceGroup != null)
            {
                m_selectFaceGroup.onClick.RemoveListener(OnSelectFaceGroup);
            }

            if (m_resetUVs != null)
            {
                m_resetUVs.onClick.RemoveListener(OnResetUVs);
            }
        }

        private void OnGroupFaces()
        {
            m_tool.GroupFaces();
        }

        private void OnUngroupFaces()
        {
            m_tool.UngroupFaces();
        }

        private void OnSelectFaceGroup()
        {
            m_tool.SelectFaceGroup();
        }

        private void OnResetUVs()
        {
            m_tool.ResetUVs();
        }
    }
}

