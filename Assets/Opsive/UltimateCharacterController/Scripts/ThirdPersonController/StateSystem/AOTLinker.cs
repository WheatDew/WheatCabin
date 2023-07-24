/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.StateSystem
{
    using Opsive.Shared.StateSystem;
    using System;
    using UnityEngine;

    // See Opsive.UltimateCharacterController.StateSystem.AOTLinker for an explanation of this class.
    public class AOTLinker : MonoBehaviour
    {
        /// <summary>
        /// Initialize the Linker
        /// </summary>
        public void Linker()
        {
#pragma warning disable 0219
#if THIRD_PERSON_CONTROLLER
            var objectDeathVisiblityGenericDelegate = new Preset.GenericDelegate<Character.PerspectiveMonitor.ObjectDeathVisiblity>();
            var objectDeathVisiblityFuncDelegate = new Func<Character.PerspectiveMonitor.ObjectDeathVisiblity>(() => { return 0; });
            var objectDeathVisiblityActionDelegate = new Action<Character.PerspectiveMonitor.ObjectDeathVisiblity>((Character.PerspectiveMonitor.ObjectDeathVisiblity value) => { });
#endif
#pragma warning restore 0219
        }
    }
}
