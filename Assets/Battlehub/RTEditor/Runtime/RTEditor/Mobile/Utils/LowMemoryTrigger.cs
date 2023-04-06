using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class LowMemoryTrigger : MonoBehaviour
    {
        private void Start()
        {
            Application.lowMemory += OnLowMemory;
        }

        private void OnDestroy()
        {
            Application.lowMemory -= OnLowMemory;
        }

        private void OnLowMemory()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}
