using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene2
{
    /// <summary>
    /// Sets the scale of the user interface so that the ui controls are easy to use on a small screen.
    /// </summary>
    public class SetUIScaleExample : EditorExtension
    {
        [SerializeField]
        [Range(0.5f, 3)]
        private float m_uiScale = 3;

        protected override void OnInit()
        {
            base.OnInit();

            ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();

            //The Settings component provides various editor settings that are persistent. We need to specify a key prefix to isolate the settings set in this scene.
            settingsComponent.SettingsKeyPrefix = "Battlehub.RTEditor.Demo.Scene2.";

            //Set the scale of the user interface so that the ui controls are easy to use on a small screen. (This setting also increases the size of Transform Handles)
            settingsComponent.UIScale = m_uiScale;
        }
    }
}
