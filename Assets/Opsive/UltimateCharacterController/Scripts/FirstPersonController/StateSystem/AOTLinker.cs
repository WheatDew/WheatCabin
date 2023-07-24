/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.StateSystem
{
    using UnityEngine;
#if ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP || ULTIMATE_CHARACTER_CONTROLLER_HDRP
    using Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes;
    using Opsive.Shared.StateSystem;
    using System;
#endif

    // See Opsive.UltimateCharacterController.StateSystem.AOTLinker for an explanation of this class.
    public class AOTLinker : MonoBehaviour
    {
        /// <summary>
        /// Initialize te linker.
        /// </summary>
        public void Linker()
        {
#pragma warning disable 0219
#if ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP || ULTIMATE_CHARACTER_CONTROLLER_HDRP
            var objectOverlayRenderTypeFirstPersonCameraViewType = new Preset.GenericDelegate<FirstPerson.ObjectOverlayRenderType>();
            var objectOverlayRenderTypeFirstPersonCameraFuncDelegate = new Func<FirstPerson.ObjectOverlayRenderType>(() => { return 0; });
            var objectOverlayRenderTypeFirstPersonCameraActionDelegate = new Action<FirstPerson.ObjectOverlayRenderType>((FirstPerson.ObjectOverlayRenderType value) => { });
#endif
#pragma warning restore 0219
        }
    }
}