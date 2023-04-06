using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.MenuControl;
using System;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene5
{
    /// <summary>
    /// This is a demonstration of how to read and save a layout to a file, and how to load and recreate a layout from a file.
    /// </summary>
    [MenuDefinition]
    public class LayoutSaveLoadMenuDefinitionExample : MonoBehaviour
    {
        /// <summary>
        /// Gets and saves layout to file
        /// </summary>
        private static void SaveLayout(string layoutName, IWindowManager wm)
        {
            ILayoutStorageModel layoutStorage = IOC.Resolve<ILayoutStorageModel>();
            layoutStorage.SaveLayout(layoutName, wm.GetLayout());
        }


        /// <summary>
        /// Reads and recreates layout from file
        /// </summary>
        private static void LoadLayout(string layoutName, IWindowManager wm)
        {
            ILayoutStorageModel layoutStorage = IOC.Resolve<ILayoutStorageModel>();
            if(layoutStorage.LayoutExists(layoutName))
            {
                wm.SetLayout(() => layoutStorage.LoadLayout(layoutName));
            }
            else
            {
                Debug.Log($"File {layoutName} not found");
            }
        }


        #region Main Menu Definition 
        [MenuCommand("Layout/Save")]
        public void SaveLayout()
        {
            Prompt("Save Layout", "DefaultLayout", "Save", "Cancel", SaveLayout);
        }

        [MenuCommand("Layout/Load")]
        public void LoadLayout()
        {
            Prompt("Load Layout", "DefaultLayout", "Load", "Cancel", LoadLayout);
        }

        [MenuCommand("Layout/Default")]
        public void ResetToDefaultLayout()
        {
            ILayoutStorageModel layoutStorage = IOC.Resolve<ILayoutStorageModel>();
            bool layoutExist = layoutStorage.LayoutExists(layoutStorage.DefaultLayoutName);
            if (layoutExist)
            {
                layoutStorage.DeleteLayout(layoutStorage.DefaultLayoutName);
            }

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.SetDefaultLayout();
        }

        [MenuCommand("Window/Scene")]
        public void OpenScene()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow(BuiltInWindowNames.Scene);
        }

        [MenuCommand("Window/Inspector")]
        public void OpenInspector()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow(BuiltInWindowNames.Inspector);
        }

        [MenuCommand("Window/Hierarchy")]
        public void OpenHierarchy()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow(BuiltInWindowNames.Hierarchy);
        }
        #endregion

        private static void Prompt(string header, string defaultText, string okText, string cancelText, Action<string, IWindowManager> okAction)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.Prompt(header, defaultText, (sender, okArgs) =>
            {
                if (string.IsNullOrEmpty(okArgs.Text))
                {
                    okArgs.Cancel = true;
                    return;
                }
                okAction(okArgs.Text, wm);
            },
            (sender, cancelArgs) => { },
            okText,
            cancelText);
        }
    }
}