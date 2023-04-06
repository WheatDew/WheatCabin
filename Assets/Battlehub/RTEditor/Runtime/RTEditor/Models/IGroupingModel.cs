using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    
    public interface IGroupingModel
    {
        bool CanGroup(GameObject[] gameObjects);

        void GroupAndRecord(GameObject[] gameObjects, string groupName, bool local = false);

        bool CanUngroup(GameObject[] gameObjects);

        void UngroupAndRecord(GameObject[] groups);

        bool CanMerge(GameObject[] gameObjects);

        void MergeAndRecord(GameObject[] gameObjects);

        void MergeOptimizedAndRecord(GameObject[] gameObjects);

        bool CanSeparate(GameObject[] gameObjects);

        void SeparateAndRecord(GameObject[] gameObjects);
    }
}