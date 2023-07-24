/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

#if !ULTIMATE_CHARACTER_CONTROLLER_DEBUG

namespace Opsive.UltimateCharacterController.Editor.References
{
#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using System.Collections.Generic;
#endif
    using Opsive.UltimateCharacterController.Demo.References;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [InitializeOnLoad]
    public class ObjectRemover
    {
        private static Scene s_ActiveScene;

        /// <summary>
        /// Registers for the scene change callback.
        /// </summary>
        static ObjectRemover()
        {
            EditorApplication.update += Update;
        }

        /// <summary>
        /// The scene has been changed.
        /// </summary>
        private static void Update()
        {
            ProcessScene();
        }

        /// <summary>
        /// Removes the necessary scene objects.
        /// </summary>
        private static void ProcessScene()
        {
            var scene = SceneManager.GetActiveScene();

            if (scene == s_ActiveScene || Application.isPlaying) {
                return;
            }
            s_ActiveScene = scene;

            // Only the add-on and integration scenes are be affected.
            var scenePath = s_ActiveScene.path.Replace("\\", "/");
            if (!scenePath.Contains("UltimateCharacterController/Add-Ons") && !scenePath.Contains("UltimateCharacterController/Integrations") && 
                !scenePath.Contains("Integrations/UltimateCharacterController")) {
                return;
            }

            // Find the object which contains the objects that should be removed.
            var objectReferences = GameObject.FindObjectsOfType<ObjectReferences>();
            for (int i = 0; i < objectReferences.Length; ++i) {
                if (objectReferences[i] == null) {
                    continue;
                }
                ProcessObjectReferences(objectReferences[i], true);
            }
        }

        /// <summary>
        /// Removes the objects specified by the object references object.
        /// </summary>
        private static void ProcessObjectReferences(ObjectReferences objectReferences, bool fromScene)
        {
            if (objectReferences == null) {
                return;
            }

            RemoveObjects(objectReferences.RemoveObjects);
            objectReferences.RemoveObjects = null;
#if !FIRST_PERSON_CONTROLLER
            RemoveObjects(objectReferences.FirstPersonObjects);
            objectReferences.FirstPersonObjects = null;
#endif
#if !THIRD_PERSON_CONTROLLER
            RemoveObjects(objectReferences.ThirdPersonObjects);
            objectReferences.ThirdPersonObjects = null;
#endif

#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
            // Remove any view types and states that are no longer valid.
            Opsive.UltimateCharacterController.Camera.CameraController[] cameraControllers;
            if (fromScene) {
                cameraControllers = GameObject.FindObjectsOfType<CameraController>();
            } else {
                cameraControllers = objectReferences.GetComponents<CameraController>();
            }
            if (cameraControllers != null) {
                for (int i = 0; i < cameraControllers.Length; ++i) {
                    var viewTypes = new List<ViewType>(cameraControllers[i].ViewTypes);
                    for (int j = viewTypes.Count - 1; j > -1; --j) {
                        if (viewTypes[j] == null) {
                            viewTypes.RemoveAt(j);
                            continue;
                        }
                        viewTypes[j].States = RemoveUnusedStates(viewTypes[j].States);
                    }
                    cameraControllers[i].ViewTypes = viewTypes.ToArray();
                    Shared.Editor.Utility.EditorUtility.SetDirty(cameraControllers[i]);
                }
            }

            // Remove any movement types, abilities, or states that are no longer valid.
            Character.UltimateCharacterLocomotion[] characterLocomotions;
            if (fromScene) {
                characterLocomotions = GameObject.FindObjectsOfType<Character.UltimateCharacterLocomotion>();
            } else {
                characterLocomotions = objectReferences.GetComponents<Character.UltimateCharacterLocomotion>();
            }
            if (characterLocomotions != null) {
                for (int i = 0; i < characterLocomotions.Length; ++i) {
                    var movementTypes = new List<MovementType>(characterLocomotions[i].MovementTypes);
                    for (int j = movementTypes.Count - 1; j > -1; --j) {
                        if (movementTypes[j] == null) {
                            movementTypes.RemoveAt(j);
                            continue;
                        }
                        movementTypes[j].States = RemoveUnusedStates(movementTypes[j].States);
                    }
                    characterLocomotions[i].MovementTypes = movementTypes.ToArray();
#if FIRST_PERSON_CONTROLLER
                    characterLocomotions[i].SetMovementType(TypeUtility.GetType(characterLocomotions[i].FirstPersonMovementTypeFullName));
#else
                    characterLocomotions[i].SetMovementType(TypeUtility.GetType(characterLocomotions[i].ThirdPersonMovementTypeFullName));
#endif

                    // Check for unused ability states.
                    var abilities = new List<Ability>(characterLocomotions[i].Abilities);
                    for (int j = abilities.Count - 1; j > -1; --j) {
                        if (abilities[j] == null) {
                            abilities.RemoveAt(j);
                            continue;
                        }
                        abilities[j].States = RemoveUnusedStates(abilities[j].States);
                    }
                    characterLocomotions[i].Abilities = abilities.ToArray();

                    // Check for unused item ability states.
                    var itemAbilities = new List<ItemAbility>(characterLocomotions[i].ItemAbilities);
                    for (int j = itemAbilities.Count - 1; j > -1; --j) {
                        if (itemAbilities[j] == null) {
                            itemAbilities.RemoveAt(j);
                            continue;
                        }
                        itemAbilities[j].States = RemoveUnusedStates(itemAbilities[j].States);
                    }
                    characterLocomotions[i].ItemAbilities = itemAbilities.ToArray();

                    Shared.Editor.Utility.EditorUtility.SetDirty(characterLocomotions[i]);
                }
            }

#if !THIRD_PERSON_CONTROLLER
            // Set the shadow caster for the first person only objects.
            var shadowCaster = Managers.ManagerUtility.FindInvisibleShadowCaster(null);
            if (shadowCaster != null) {
                for (int i = 0; i < objectReferences.ShadowCasterObjects.Length; ++i) {
                    if (objectReferences.ShadowCasterObjects[i] == null) {
                        continue;
                    }

                    var renderers = objectReferences.ShadowCasterObjects[i].GetComponentsInChildren<Renderer>();
                    for (int j = 0; j < renderers.Length; ++j) {
                        var materials = renderers[j].sharedMaterials;
                        for (int k = 0; k < materials.Length; ++k) {
                            materials[k] = shadowCaster;
                        }
                        renderers[j].sharedMaterials = materials;
                        Shared.Editor.Utility.EditorUtility.SetDirty(renderers[j]);
                    }
                }
            }
#endif

            var items = objectReferences.GetComponentsInChildren<Items.CharacterItem>();
            for (int i = 0; i < items.Length; ++i) {
                CheckItem(items[i].gameObject);
            }

            // Ensure all of the states point to a preset
            Shared.StateSystem.StateBehavior[] stateBehaviors;
            if (fromScene) {
                stateBehaviors = GameObject.FindObjectsOfType<Shared.StateSystem.StateBehavior>();
            } else {
                stateBehaviors = objectReferences.GetComponentsInChildren<Shared.StateSystem.StateBehavior>(true);
            }
            if (stateBehaviors != null) {
                for (int i = 0; i < stateBehaviors.Length; ++i) {
                    stateBehaviors[i].States = RemoveUnusedStates(stateBehaviors[i].States);
                    Shared.Editor.Utility.EditorUtility.SetDirty(stateBehaviors[i]);
                }
            }
#endif

            for (int i = 0; i < objectReferences.NestedReferences.Length; ++i) {
                var nestedObject = objectReferences.NestedReferences[i];
                if (nestedObject == null) {
                    continue;
                }
                GameObject nestedRoot = null;
                if (PrefabUtility.IsPartOfPrefabAsset(nestedObject)) {
                    nestedRoot = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(objectReferences.NestedReferences[i]));
                    nestedObject = nestedRoot.GetComponent<ObjectReferences>();
                }
                ProcessObjectReferences(nestedObject, false);
                if (nestedRoot != null) {
                    PrefabUtility.SaveAsPrefabAsset(nestedRoot, AssetDatabase.GetAssetPath(objectReferences.NestedReferences[i]));
                    PrefabUtility.UnloadPrefabContents(nestedRoot);
                }
            }

            UnpackPrefab(objectReferences);
            Object.DestroyImmediate(objectReferences, true);
        }

        /// <summary>
        /// Removes the specified objects.
        /// </summary>
        private static void RemoveObjects(Object[] objects)
        {
            if (objects == null) {
                return;
            }

            for (int i = objects.Length - 1; i > -1; --i) {
                if (objects[i] == null || PrefabUtility.GetPrefabAssetType(objects[i]) == PrefabAssetType.MissingAsset) {
                    continue;
                }

                if (objects[i] is GameObject && (objects[i] as GameObject).transform.parent == null && AssetDatabase.GetAssetPath(objects[i]).Length > 0 &&
                    PrefabUtility.GetPrefabAssetType(objects[i]) == PrefabAssetType.Regular) {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(objects[i]));
                } else {
                    UnpackPrefab(objects[i]);
                    Object.DestroyImmediate(objects[i], true);
                }
            }
        }

#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
        /// <summary>
        /// Ensure the item only has the valid states.
        /// </summary>
        private static void CheckItem(GameObject gameObject)
        {
            if (gameObject == null || gameObject.GetComponent<Items.CharacterItem>() == null) {
                return;
            }

            var itemActions = gameObject.GetComponents<Items.Actions.CharacterItemAction>();
            var moduleGroups = new List<Opsive.UltimateCharacterController.Items.Actions.Modules.ActionModuleGroupBase>();
            for (int i = 0; i < itemActions.Length; ++i) {
                moduleGroups.Clear();
                itemActions[i].GetAllModuleGroups(moduleGroups);
                for (int j = 0; j < moduleGroups.Count; ++j) {
                    var baseModules = moduleGroups[j].BaseModules;
                    if (baseModules != null) {
                        for (int k = 0; k < baseModules.Count; ++k) {
                            baseModules[k].States = RemoveUnusedStates(baseModules[k].States);
                        }
                    }
                }
                Shared.Editor.Utility.EditorUtility.SetDirty(itemActions[i]);
            }
        }
#endif

        /// <summary>
        /// Unpacks the prefab root.
        /// </summary>
        /// <param name="obj">The object that should be unpacked.</param>
        private static void UnpackPrefab(Object obj)
        {
            if (obj != null && PrefabUtility.IsPartOfAnyPrefab(obj)) {
                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                if (root != null && PrefabUtility.GetPrefabInstanceStatus(root) == PrefabInstanceStatus.Connected) {
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }
        }

#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
        /// <summary>
        /// Removes any states whose preset will be exlcluded.
        /// </summary>
        private static Shared.StateSystem.State[] RemoveUnusedStates(Opsive.Shared.StateSystem.State[] stateArray)
        {
            var states = new List<Shared.StateSystem.State>(stateArray);
            var stateRemovals = new HashSet<string>();
            for (int i = states.Count - 2; i > -1; --i) {
                var preset = states[i].Preset;
                if (preset == null) {
                    stateRemovals.Add(states[i].Name);
                    states.RemoveAt(i);
                }
            }
            for (int i = 0; i < states.Count; ++i) {
                if (states[i].BlockList == null) {
                    continue;
                }
                var blockList = new List<string>(states[i].BlockList);
                for (int j = blockList.Count - 1; j > -1; --j) {
                    if (stateRemovals.Contains(blockList[j])) {
                        blockList.RemoveAt(j);
                    }
                }
                states[i].BlockList = blockList.ToArray();
            }
            return states.ToArray();
        }
#endif
    }
}
#endif