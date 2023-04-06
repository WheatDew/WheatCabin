using Battlehub.RTEditor;
using UnityEngine.AI;

namespace Battlehub.RTNavigation
{
    [BuiltInDescriptor]
    public class NavMeshAgentComponentDescriptor : ComponentDescriptorBase<NavMeshAgent>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            return new PropertyDescriptor[0];
        }
    }
}
