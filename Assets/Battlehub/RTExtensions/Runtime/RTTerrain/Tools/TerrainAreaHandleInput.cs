using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandleInput : BaseHandleInput
    {
        [SerializeField]
        private KeyCode m_focusKey = KeyCode.F;

        private TerrainAreaHandle TerrainAreaHandle
        {
            get { return (TerrainAreaHandle)m_handle; }
        }

        private IPlacementModel m_placementModel;

        protected override void Start()
        {
            base.Start();
            m_placementModel = IOC.Resolve<IPlacementModel>();
        }


        protected override void Update()
        {
            base.Update();

            if (m_editor.Tools.IsViewing)
            {
                return;
            }

            if (!m_handle.IsWindowActive || !m_handle.Window.IsPointerOver)
            {
                return;
            }

            if (ChangePositionAction() && !m_handle.IsDragging && m_handle.SelectedAxis == RuntimeHandleAxis.None)
            {
                TerrainAreaHandle.ChangePosition();
            }

            if (FocusAction() && m_handle != null && m_handle.IsWindowActive)
            {
                IScenePivot pivot = m_placementModel.GetSelectionComponent();
                Vector3[] areaResizerPositions = TerrainAreaHandle.AreaResizerPositions;
                pivot.Focus(m_handle.Position, Mathf.Max(TerrainAreaHandle.Appearance.HandleScale, (areaResizerPositions[1] - areaResizerPositions[0]).magnitude));
            }
        }

        protected virtual bool ChangePositionAction()
        {
            return m_editor.Input.GetPointerUp(0);
        }

        protected virtual bool FocusAction()
        {
            return m_editor.Input.GetKeyDown(m_focusKey);
        }
    }
}

