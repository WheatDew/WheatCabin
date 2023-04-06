using Battlehub.RTSL.Interface;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(ProjectItem), typeof(ImportStatus))]
    public class ProjectItemToImportStatusAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            ProjectItem projectItem = (ProjectItem)valueIn;
            if (projectItem is ImportAssetItem)
            {
                ImportAssetItem importItem = (ImportAssetItem)projectItem;
                return importItem.Status;
            }

            return ImportStatus.None;
        }
    }

}
