using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.Controls;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileSceneWindowInit : RuntimeWindowExtension
    {
        [SerializeField]
        private RectTransform m_mobileLayerPrefab = null;

        [SerializeField]
        private bool m_hideSceneSettings = true;

        public override string WindowTypeName
        {
            get { return BuiltInWindowNames.Scene; }
        }

        protected override void Extend(RuntimeWindow window)
        {
            RectTransform mobileLayer = Instantiate(m_mobileLayerPrefab, window.ViewRoot, false);
            mobileLayer.name = "Mobile Layer";

            if (m_hideSceneSettings)
            {
                ISceneSettingsComponent sceneSettingsComponent = window.IOCContainer.Resolve<ISceneSettingsComponent>();
                sceneSettingsComponent.IsUserDefined = false;
            }

            IRTE editor = IOC.Resolve<IRTE>();
            IRuntimeSceneComponent sceneComponent = window.IOCContainer.Resolve<IRuntimeSceneComponent>();

            if (editor.TouchInput.IsTouchSupported)
            {
                MobileContextPanelPositionUpdater positionUpdater = mobileLayer.GetComponentInChildren<MobileContextPanelPositionUpdater>(true);
                positionUpdater.MarginBottom = 100;

                sceneComponent.GameObject.AddComponent<MobileSceneInput>();
                sceneComponent.IsBoxSelectionEnabled = false;
            }

            PositionHandleModel positionHandleModel = sceneComponent.PositionHandle.Model as PositionHandleModel;
            positionHandleModel.QuadLength = 0.33f;

            editor.Tools.LockAxes = new LockObject { RotationFree = true };
            
        }
    }
}

