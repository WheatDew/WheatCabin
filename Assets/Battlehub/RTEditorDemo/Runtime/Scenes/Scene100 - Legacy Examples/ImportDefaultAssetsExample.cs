using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;

using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class ImportDefaultAssetsExample : EditorExtension
    {
        public bool FirstRun
        {
            get { return PlayerPrefs.GetInt("FirstRun", 1) == 1; }
            set { PlayerPrefs.SetInt("FirstRun", value ? 1 : 0); }
        }

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            if (!FirstRun)
            {
                return;
            }
            FirstRun = false;

            IProject project = IOC.Resolve<IProject>();
            IRTE editor = IOC.Resolve<IRTE>();

            project.LoadImportItems("DemoAssetLibrary", true, (error, root) =>
            {
                if (error.HasError)
                {
                    editor.IsBusy = false;
                    Debug.LogError(error.ToString());
                }
                else
                {
                    IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
                    ImportAssetItem[] importItems = root.Flatten(true).OfType<ImportAssetItem>().ToArray();
                    for (int i = 0; i < importItems.Length; ++i)
                    {
                        ImportAssetItem importItem = importItems[i];
                        importItem.SetPreview(resourcePreview.CreatePreviewData(importItems[i].Object));
                    }
                    project.UnloadImportItems(root);

                    project.Import(importItems, (importError, assetItems) =>
                    {
                        if (importError.HasError)
                        {
                            Debug.LogError(importError.ErrorText);
                        }

                        editor.IsBusy = false;
                    });
                }
            });
        }
    }
}

