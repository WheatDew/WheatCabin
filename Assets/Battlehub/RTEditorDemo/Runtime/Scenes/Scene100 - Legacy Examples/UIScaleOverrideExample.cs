using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class UIScaleOverrideExample : EditorExtension
    {
        [SerializeField]
        private float Scale = 2;

        protected override void OnEditorExist()
        {
            ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();
            settings.UIScale = Scale;
        }
    }
}
