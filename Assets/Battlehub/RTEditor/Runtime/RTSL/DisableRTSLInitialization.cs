using Battlehub.RTSL;
using UnityEngine;

namespace Battlehub.RTSL
{
    /// <summary>
    /// Disables RTSL initialization (see Init method in Assets\Battlehub\RTEditor\Runtime\RTSL\RTSLDeps.cs)
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DisableRTSLInitialization : MonoBehaviour
    {
        private void OnEnable()
        {
            RTSLSettings.RuntimeInitializeOnLoad = false;
        }
    }

}
