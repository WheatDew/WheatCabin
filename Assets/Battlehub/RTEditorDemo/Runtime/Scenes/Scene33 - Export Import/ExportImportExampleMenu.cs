using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.MenuControl;
//#if UNITY_STANDALONE
//using SFB;
//#endif
using System.IO;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene34
{
    /// <summary>
    ///This is a demonstration of the ExportProjectAsync and ImportProjectAsync methods.
    ///The former exports all the assets and scenes to a zip archive, the latter extracts scenes and assets from the archive.
    /// </summary>
    [MenuDefinition]
    public class ExportImportExampleMenu : MonoBehaviour
    {
#if UNITY_STANDALONE
        //[MenuCommand("Example/Export Project")]
        //public async void Export()
        //{
        //    IRTE editor = IOC.Resolve<IRTE>();
        //    editor.IsBusy = true;

        //    IProjectAsync project = IOC.Resolve<IProjectAsync>();
        //    string currentProject = project.State.ProjectInfo.Name;
        //    string targetPath = StandaloneFileBrowser.SaveFilePanel("Export Project", "", currentProject, "rtpackage");
        //    if (targetPath != null)
        //    {
        //        await project.ExportProjectAsync(currentProject, targetPath);
        //    }

        //    editor.IsBusy = false;
        //}

        //[MenuCommand("Example/Import Project")]
        //public async void Import()
        //{
        //    IRTE editor = IOC.Resolve<IRTE>();
        //    editor.IsBusy = true;

        //    IProjectAsync project = IOC.Resolve<IProjectAsync>();
        //    string[] path = StandaloneFileBrowser.OpenFilePanel("Import Project", "", "rtpackage", false);
        //    if (path != null && path.Length > 0)
        //    {
        //        string sourcePath = path[0];
        //        string targetProjectName = Path.GetFileNameWithoutExtension(sourcePath);

        //        if (targetProjectName == project.State.ProjectInfo.Name)
        //        {
        //            await project.CloseProjectAsync();
        //            await project.ImportProjectAsync(targetProjectName, sourcePath, overwrite: true);
        //            await project.OpenProjectAsync(targetProjectName);

        //            IRuntimeSceneManager sceneManager = IOC.Resolve<IRuntimeSceneManager>();
        //            sceneManager.CreateNewScene();
        //        }
        //        else
        //        {
        //            await project.ImportProjectAsync(targetProjectName, sourcePath, overwrite: true);
        //        }
        //    }

        //    editor.IsBusy = false;
        //}
#endif
    }
}
