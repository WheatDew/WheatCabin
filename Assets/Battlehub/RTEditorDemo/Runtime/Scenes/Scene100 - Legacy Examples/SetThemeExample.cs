using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class SetThemeExample : EditorOverride
    {
        [SerializeField]
        private ThemeAsset Theme = null;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();

            //clear existing themes
            //settings.Themes = null;

            settings.SelectedTheme = Theme;
        }
    }
}


