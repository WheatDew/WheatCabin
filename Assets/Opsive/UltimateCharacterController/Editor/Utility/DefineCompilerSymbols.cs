/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------


namespace Opsive.UltimateCharacterController.Editor.Utility
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Networking;
    using UnityEditor;

    /// <summary>
    /// Editor script which will define or remove the Ultimate Character Controller compiler symbols so the components are aware of the asset import status.
    /// </summary>
    [InitializeOnLoad]
    public class DefineCompilerSymbols
    {
        private static string s_FirstPersonControllerSymbol = "FIRST_PERSON_CONTROLLER";
        private static string s_ThirdPersonControllerSymbol = "THIRD_PERSON_CONTROLLER";
        private static string s_MultiplayerSymbol = "ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER";
        private static string s_AgilitySymbol = "ULTIMATE_CHARACTER_CONTROLLER_AGILITY";
        private static string s_ClimbingSymbol = "ULTIMATE_CHARACTER_CONTROLLER_CLIMBING";
        private static string s_SwimmingSymbol = "ULTIMATE_CHARACTER_CONTROLLER_SWIMMING";
        private static string s_UniversalRPSymbol = "ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP";
        private static string s_HDRPSymbol = "ULTIMATE_CHARACTER_CONTROLLER_HDRP";
        private static string s_TextMeshProSymbol = "TEXTMESH_PRO_PRESENT";

        /// <summary>
        /// If the specified classes exist then the compiler symbol should be defined, otherwise the symbol should be removed.
        /// </summary>
        static DefineCompilerSymbols()
        {
            // The First Person Controller Combat MovementType will exist when the First Person Controller asset is imported.
            var firstPersonControllerExists = TypeUtility.GetType("Opsive.UltimateCharacterController.FirstPersonController.Character.MovementTypes.Combat") != null;
#if FIRST_PERSON_CONTROLLER
            if (!firstPersonControllerExists) {
                RemoveSymbol(s_FirstPersonControllerSymbol);
            }
#else
            if (firstPersonControllerExists) {
                AddSymbol(s_FirstPersonControllerSymbol);
            }
#endif

            // The Third Person Controller Combat MovementType will exist when the Third Person Controller asset is imported.
            var thirdPersonControllerExists = TypeUtility.GetType("Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes.Combat") != null;
#if THIRD_PERSON_CONTROLLER
            if (!thirdPersonControllerExists) {
                RemoveSymbol(s_ThirdPersonControllerSymbol);
            }
#else
            if (thirdPersonControllerExists) {
                AddSymbol(s_ThirdPersonControllerSymbol);
            }
#endif

            // The MultiplayerStatus ScriptableObject will determine if a multiplayer add-on is imported.
            MultiplayerStatus multiplayerStatus = null;
            var multiplayerStatusAssets = AssetDatabase.FindAssets("t:MultiplayerStatus");
            if (multiplayerStatusAssets != null && multiplayerStatusAssets.Length > 0) {
                for (int i = 0; i < multiplayerStatusAssets.Length; ++i) {
                    var path = AssetDatabase.GUIDToAssetPath(multiplayerStatusAssets[i]);
                    if (string.IsNullOrEmpty(path)) {
                        path = multiplayerStatusAssets[i];
                    }
                    multiplayerStatus = AssetDatabase.LoadAssetAtPath(path, typeof(MultiplayerStatus)) as MultiplayerStatus;
                    if (multiplayerStatus != null) {
                        break;
                    }
                }
            }
            var multiplayerExists = multiplayerStatus != null && multiplayerStatus.SupportsMultiplayer;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (!multiplayerExists) {
                RemoveSymbol(s_MultiplayerSymbol);
            }
#else
            if (multiplayerExists) {
                AddSymbol(s_MultiplayerSymbol);
            }
#endif

            // Agility Add-On Inspector will exist if the Agility Pack is imported.
            var agilityExists = TypeUtility.GetType("Opsive.UltimateCharacterController.AddOns.Agility.Editor.AgilityAddOnInspector") != null;
#if ULTIMATE_CHARACTER_CONTROLLER_AGILITY
            if (!agilityExists) {
                RemoveSymbol(s_AgilitySymbol);
            }
#else
            if (agilityExists) {
                AddSymbol(s_AgilitySymbol);
            }
#endif

            // Climbing Add-On Inspector will exist if the Climbing Pack is imported.
            var climbingExists = TypeUtility.GetType("Opsive.UltimateCharacterController.AddOns.Climbing.Editor.ClimbingAddOnInspector") != null;
#if ULTIMATE_CHARACTER_CONTROLLER_CLIMBING
            if (!climbingExists) {
                RemoveSymbol(s_ClimbingSymbol);
            }
#else
            if (climbingExists) {
                AddSymbol(s_ClimbingSymbol);
            }
#endif

            // Swimming Add-On Inspector will exist if the Swimming Pack is imported.
            var swimmingExists = TypeUtility.GetType("Opsive.UltimateCharacterController.AddOns.Swimming.Editor.SwimmingAddOnInspector") != null;
#if ULTIMATE_CHARACTER_CONTROLLER_SWIMMING
            if (!swimmingExists) {
                RemoveSymbol(s_SwimmingSymbol);
            }
#else
            if (swimmingExists) {
                AddSymbol(s_SwimmingSymbol);
            }
#endif

            // The URP data will exists when the URP is imported. This assembly definition must be added to the Opsive.UltimateCaracterController.Editor assembly definition.
            var universalrpExists = TypeUtility.GetType("UnityEngine.Rendering.Universal.ForwardRendererData") != null;
#if ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
            if (!universalrpExists) {
                RemoveSymbol(s_UniversalRPSymbol);
            }
#else
            if (universalrpExists) {
                AddSymbol(s_UniversalRPSymbol);
            }
#endif
            var hdrpExists = TypeUtility.GetType("UnityEngine.Rendering.HighDefinition.CustomPassVolume") != null;
#if ULTIMATE_CHARACTER_CONTROLLER_HDRP
            if (!hdrpExists) {
                RemoveSymbol(s_HDRPSymbol);
            }
#else
            if (hdrpExists) {
                AddSymbol(s_HDRPSymbol);
            }
#endif
            // The TMP_Text component will exist when the TextMesh Pro asset is imported.
            var textMeshProExists = TypeUtility.GetType("TMPro.TMP_Text") != null;
#if TEXTMESH_PRO_PRESENT
            if (!textMeshProExists) {
                RemoveSymbol(s_TextMeshProSymbol);
            }
#else
            if (textMeshProExists) {
                AddSymbol(s_TextMeshProSymbol);
            }
#endif
        }

        /// <summary>
        /// Adds the specified symbol to the compiler definitions.
        /// </summary>
        /// <param name="symbol">The symbol to add.</param>
        private static void AddSymbol(string symbol)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (symbols.Contains(symbol)) {
                return;
            }
            symbols += (";" + symbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }

        /// <summary>
        /// Remove the specified symbol from the compiler definitions.
        /// </summary>
        /// <param name="symbol">The symbol to remove.</param>
        private static void RemoveSymbol(string symbol)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!symbols.Contains(symbol)) {
                return;
            }
            if (symbols.Contains(";" + symbol)) {
                symbols = symbols.Replace(";" + symbol, "");
            } else {
                symbols = symbols.Replace(symbol, "");
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }
    }
}