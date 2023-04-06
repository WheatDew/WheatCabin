using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    public static class IResourcePreviewUtilityExtensions 
    {
        public static void ResetPreview(this IRuntimeSelection selection, ProjectItem[] projectItems)
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            if (selection.activeObject != null)
            {
                object id = project.Utils.ToPersistentID(selection.activeObject);
                ProjectItem selectedProjectItem = projectItems.Where(projectItem => !projectItem.IsFolder && !(projectItem is ImportAssetItem) && project.Utils.ToPersistentID(projectItem).Equals(id)).FirstOrDefault();
                if (selectedProjectItem != null)
                {
                    selectedProjectItem.SetPreview(null);
                }
            }
        }
        
        public static IEnumerator CoCreatePreviews(this IResourcePreviewUtility resourcePreview, ProjectItem[] projectItems, Action done = null, bool resetSelected = true, int batchSize = 10)
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            if (resourcePreview == null)
            {
                done?.Invoke();
                yield break;
            }

            if(resetSelected)
            {
                IRTE rte = IOC.Resolve<IRTE>();
                rte.Selection.ResetPreview(projectItems);
            }

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                ImportAssetItem importItem = projectItems[i] as ImportAssetItem;
                if (importItem != null)
                {
                    if (importItem.Object != null)
                    {
                        importItem.SetPreview(resourcePreview.CreatePreviewData(importItem.Object));
                    }
                }
                else
                {
                    if (!projectItem.IsFolder)
                    {
                        UnityObject obj = null;
                        if (projectItem.GetPreview() == null)
                        {
                            obj = project.Utils.FromProjectItem<UnityObject>(projectItem);
                        }

                        if (obj != null)
                        {
                            projectItem.SetPreview(resourcePreview.CreatePreviewData(obj));
                        }
                    }
                }

                if (i % batchSize == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            done?.Invoke();
        }
    }

}
