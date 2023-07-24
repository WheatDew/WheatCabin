/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    /// Static class defining information about the asset.
    /// </summary>
    public static class AssetInfo
    {
        public static string Version { get { return "3.0.11"; } }

        public static string Name
        {
            get
            {
#pragma warning disable 0162
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
                return "Ultimate Character Controller";
#endif
#if FIRST_PERSON_CONTROLLER
                return "Ultimate First Person Shooter";
#endif
#if THIRD_PERSON_CONTROLLER
                return "Third Person Controller";
#endif
                return string.Empty;
#pragma warning restore 0162
            }
        }
    }
}