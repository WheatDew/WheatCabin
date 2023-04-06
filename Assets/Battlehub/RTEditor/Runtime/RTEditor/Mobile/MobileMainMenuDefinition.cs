using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    [MenuDefinition]
    public class MobileMainMenuDefinition : MonoBehaviour
    {
        private IRuntimeEditor Editor
        {
            get { return IOC.Resolve<IRuntimeEditor>(); }
        }

        [MenuCommand("/New Scene", "RTE_NewScene")]
        public void NewScene()
        {
            Editor.NewScene();
        }

        [MenuCommand("/Save Scene", "RTE_Save")]
        public void SaveScene()
        {
            Editor.SaveScene();
        }

        [MenuCommand("/Save Scene As...", "RTE_Dialog_SaveAs")]
        public void SaveSceneAs()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.SaveScene.ToString());
        }

        [MenuCommand("/Import Assets", "RTE_Dialog_Import")]
        public void ImportAssets()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.SelectAssetLibrary.ToString());
        }

        [MenuCommand("/Import From Url...", "RTE_Dialog_ImportFile")]
        public void ImportFromUrl()
        {
            Editor.CreateOrActivateWindow(MobileWindowNames.Importer);
        }

        [MenuCommand("/Manage Projects", "RTE_Dialog_OpenProject")]
        public void ManageProjects()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.OpenProject.ToString());
        }

        [MenuCommand("/Settings", "RTE_Settings")]
        public void ShowSettings()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateDialogWindow(RuntimeWindowType.Settings.ToString(), "ID_RTEditor_WM_Header_Settings",
                (sender, args) => { }, (sender, args) => { }, 250, 125, -1, -1, true);
        }

        [MenuCommand("/Quit", "")]
        public void Quit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif

        }

        [MenuCommand("MenuFile", hide: true)]
        public void HideMenuFile() { }

        [MenuCommand("MenuEdit", hide: true)]
        public void HideMenuEdit() { }

        [MenuCommand("MenuGameObject", hide: true)]
        public void HideMenuGameObject() { }

        [MenuCommand("MenuWindow", hide: true)]
        public void HideMenuWindow() { }

        [MenuCommand("MenuHelp", hide: true)]
        public void HideMenuHelp() { }

    }
}

