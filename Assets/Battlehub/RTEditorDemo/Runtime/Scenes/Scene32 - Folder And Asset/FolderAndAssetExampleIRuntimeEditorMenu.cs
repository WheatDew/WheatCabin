using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.MenuControl;
using Battlehub.Utils;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene33
{
    /// <summary>
    /// When using the runtime editor to correctly create prefabs and delete assets
    /// use the IRuntimeEditor interface instead of IProjectAsync
    ///</summary>
    [MenuDefinition]
    public class FolderAndAssetExampleIRuntimeEditorMenu : MonoBehaviour
    {
        [MenuCommand("Example/IRuntimeEditor/Create Prefab", validate: true)]
        public bool CanCreatePrefab()
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            GameObject go = editor.Selection.activeGameObject;
            return go != null && !go.IsPrefab();
        }

        [MenuCommand("Example/IRuntimeEditor/Create Prefab")]
        public async void CreatePrefab()
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            GameObject go = editor.Selection.activeGameObject;

            editor.IsBusy = true;

            ProjectItem folder = GetFolder();

            //Use the IRuntimeEditor interface instead of IProjectAsync
            //to correctly create prefabs with previews
            await editor.CreatePrefabAsync(folder, go.GetComponent<ExposeToEditor>(), true);
            editor.IsBusy = false;
        }

        [MenuCommand("Example/IRuntimeEditor/Update Material", validate: true)]
        public bool CanUpdateMaterial()
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            return editor.Selection.activeObject is Material;
        }

        [MenuCommand("Example/IRuntimeEditor/Update Material")]
        public async void UpdateMaterial()
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.IsBusy = true;

            Material material = (Material)editor.Selection.activeObject;
            material.Color(Random.ColorHSV());

            await editor.SaveAssetsAsync(new[] { material });

            editor.IsBusy = false;
        }

        [MenuCommand("Example/IRuntimeEditor/Delete Selected Assets", validate: true)]
        public bool CanDeleteSelectedAssets()
        {
            IProjectFolderViewModel projectFolder = IOC.Resolve<IProjectFolderViewModel>();
            return projectFolder != null && projectFolder.HasSelectedItems;
        }

        [MenuCommand("Example/IRuntimeEditor/Delete Selected Assets")]
        public async void DeleteSelectedAssets()
        {
            IProjectFolderViewModel projectFolder = IOC.Resolve<IProjectFolderViewModel>();

            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.IsBusy = true;

            //Use the IRuntimeEditor interface instead of IProjectAsync
            //to correctly delete assets and clear the undo stack
            await editor.DeleteAssetsAsync(projectFolder.SelectedItems.ToArray());

            editor.IsBusy = false;
        }

        private static ProjectItem GetFolder()
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            ProjectItem folder = project.State.RootFolder;

            IProjectTreeModel projectTree = IOC.Resolve<IProjectTreeModel>();
            if (projectTree != null && projectTree.SelectedItem != null)
            {
                folder = projectTree.SelectedItem;
            }
            return folder;
        }
    }
}
