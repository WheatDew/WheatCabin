using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(ProjectItem), typeof(Sprite), typeof(ProjectItemAdapterOptions))]
    public class ProjectItemToSpriteAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            ProjectItemAdapterOptions adapterOptions = (ProjectItemAdapterOptions)options;
            ProjectItem projectItem = (ProjectItem)valueIn;

            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            if (project.Utils.IsScene(projectItem))
            {
                return adapterOptions.SceneIncon;
            }
            return adapterOptions.FolderIcon;
        }
    }

}
