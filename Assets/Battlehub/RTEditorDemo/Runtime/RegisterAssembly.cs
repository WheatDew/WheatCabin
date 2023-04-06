using UnityEngine;

namespace Battlehub
{
    internal class RegisterAssembly : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Initialize()
        {
            KnownAssemblies.Add("Battlehub.RTEditor.Demo");
        }
    }
}
