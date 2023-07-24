/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Globalization;

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.StartStopActions;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Melee;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable;
    using UnityEngine;
    using GenericItemEffects = Items.Actions.Modules.Shootable.GenericItemEffects;
    using ItemAmmo = Items.Actions.Modules.Shootable.ItemAmmo;
    using SpawnProjectile = Items.Actions.Modules.Throwable.SpawnProjectile;

    /// <summary>
    /// Builds a new item.
    /// </summary>
    public class ItemBuilder
    {
        /// <summary>
        /// The type of action to create. 
        /// </summary>
        public enum ActionType {
            Shootable,      // The item uses a shootable action.
            Melee,          // The item uses a melee action.
            Shield,         // The item uses a shield.
            Magic,          // The item uses a magic action.
            Throwable,      // The item uses a throwable action.
            Usable,         // The item is a generic action (such as flashlight).
        }

        /// <summary>
        /// Container struct.
        /// </summary>
        public struct ActionInfo
        {
            public ActionType Type;
            public string Name;
        }

        /// <summary>
        /// Builds the item with the specified parameters.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="itemDefinition">The ItemDefinition that the item uses (optional).</param>
        /// <param name="animatorItemID">The ID of the item within the animator.</param>
        /// <param name="character">The character that the item should be attached to (optional).</param>
        /// <param name="slotID">The ID of the slot that the item is parented to.</param>
        /// <param name="addToDefaultLoadout">Should the item be added to the character's default loadout?</param>
        /// <param name="addFirstPersonPerspective">Should the first person perspective be added?</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonObjectAnimatorController">A reference to the animator controller added to the first person object. Can be null.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item. Can be null.</param>
        /// <param name="firstPersonCharacterItemSlot">A reference to the ItemSlot to add the visible item to.</param>
        /// <param name="firstPersonVisibleItemAnimatorController">A reference to the animator controller added to the first person visible item. Can be null.</param>
        /// <param name="addThirdPersonPerspective">Should the third person perspective be added?</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        /// <param name="thirdPersonCharacterItemSlot">A reference to the ItemSlot to add the third person item to.</param>
        /// <param name="thirdPersonObjectAnimatorController">A reference to the animator controller added to the third person object. Can be null.</param>
        /// <param name="invisibleShadowCasterMaterial">A reference to the invisible shadow caster material. This is only used for first person characters.</param>
        /// <param name="actionTypes">The type of items to create.</param>
        public static GameObject BuildItem(string name, ItemDefinitionBase itemDefinition, int animatorItemID, GameObject character, int slotID, bool addToDefaultLoadout, bool addFirstPersonPerspective,
            GameObject firstPersonObject, RuntimeAnimatorController firstPersonObjectAnimatorController, GameObject firstPersonVisibleItem, CharacterItemSlot firstPersonCharacterItemSlot, 
            RuntimeAnimatorController firstPersonVisibleItemAnimatorController, bool addThirdPersonPerspective, GameObject thirdPersonObject, CharacterItemSlot thirdPersonCharacterItemSlot, 
            RuntimeAnimatorController thirdPersonObjectAnimatorController, Material invisibleShadowCasterMaterial, ActionInfo[] actionTypes)
        {
            var itemGameObject = new GameObject(name);
            var itemSlotID = (character == null || (firstPersonCharacterItemSlot == null && thirdPersonCharacterItemSlot == null)) ? slotID :
                                                    (firstPersonCharacterItemSlot != null ? firstPersonCharacterItemSlot.ID : thirdPersonCharacterItemSlot.ID);

            // If character is null then a prefab will be created.
            if (character != null) {
                // The attach to object must have an ItemPlacement component.
                var itemPlacement = character.GetComponentInChildren<ItemPlacement>();
                if (itemPlacement == null) {
                    Debug.LogError("Error: Unable to find the ItemPlacement component within " + character.name + ".");
                    return null;
                }

                // Organize the main item GameObject under the ItemPlacement GameObject.
                itemGameObject.transform.SetParentOrigin(itemPlacement.transform);

                // The item can automatically be added to the inventory's default loadout.
                if (itemDefinition != null && addToDefaultLoadout) {
                    var inventory = character.GetComponent<Inventory>();
                    var defaultLoadout = inventory.DefaultLoadout;
                    if (defaultLoadout == null) {
                        defaultLoadout = new ItemIdentifierAmount[0];
                    }
                    var hasItemDefinition = false;
                    for (int i = 0; i < defaultLoadout.Length; ++i) {
                        // If the ItemIdentifier has already been added then a new ItemIdentifier doesn't need to be added.
                        if (defaultLoadout[i].ItemDefinition == itemDefinition) {
                            defaultLoadout[i].Amount++;
                            hasItemDefinition = true;
                            break;
                        }
                    }
                    if (!hasItemDefinition) {
                        System.Array.Resize(ref defaultLoadout, defaultLoadout.Length + 1);
                        defaultLoadout[defaultLoadout.Length - 1] = new ItemIdentifierAmount(itemDefinition, 1);
                    }
                    inventory.DefaultLoadout = defaultLoadout;
                }
            }
            var item = itemGameObject.AddComponent<CharacterItem>();
            item.ItemDefinition = itemDefinition;
            item.SlotID = itemSlotID;
            item.AnimatorItemID = animatorItemID;

#if FIRST_PERSON_CONTROLLER
            // Add the first person object.
            if (addFirstPersonPerspective) {
                AddFirstPersonObject(character, name, itemGameObject, ref firstPersonObject, firstPersonObjectAnimatorController, ref firstPersonVisibleItem, firstPersonCharacterItemSlot,
                                        firstPersonVisibleItemAnimatorController);
                // If the character doesn't have an animator then the item should be equipped by a timer.
                if (character != null && character.GetComponent<Animator>() == null) {
                    item.EquipEvent.WaitForAnimationEvent = false;
                }
            }
#endif

            // Add the third person object. The character will always have a third person object if the character has an animator.
            if (addThirdPersonPerspective) {
                AddThirdPersonObject(character, name, itemGameObject, ref thirdPersonObject, thirdPersonCharacterItemSlot, thirdPersonObjectAnimatorController, invisibleShadowCasterMaterial,
                                    !addFirstPersonPerspective || firstPersonObject != null || firstPersonVisibleItem != null);
            }

            // Add the specified action type.
            if (actionTypes != null) {
                for (int i = 0; i < actionTypes.Length; ++i) {
                    AddAction(itemGameObject, firstPersonObject, firstPersonVisibleItem, thirdPersonObject, actionTypes[i].Type, actionTypes[i].Name);
                }
            }

            return itemGameObject;
        }

#if FIRST_PERSON_CONTROLLER
        /// <summary>
        /// Adds the first person object to the specified item.
        /// </summary>
        /// <param name="character">The character that the first person object is being added to.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="itemGameObject">A reference to the item's GameObject.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonObjectAnimatorController">A reference to the animator controller added to the first person object. Can be null.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item. Can be null.</param>
        /// <param name="firstPersonCharacterItemSlot">A reference to the ItemSlot to add the visible item to.</param>
        /// <param name="firstPersonVisibleItemAnimatorController">A reference to the animator controller added to the first person visible item. Can be null.</param>
        public static void AddFirstPersonObject(GameObject character, string name, GameObject itemGameObject, 
            ref GameObject firstPersonObject, RuntimeAnimatorController firstPersonObjectAnimatorController, ref GameObject firstPersonVisibleItem, CharacterItemSlot firstPersonCharacterItemSlot,
            RuntimeAnimatorController firstPersonVisibleItemAnimatorController)
        {
            var parentFirstPersonObject = false;
            if (firstPersonObject != null && (character == null || !firstPersonObject.transform.IsChildOf(character.transform))) {
                parentFirstPersonObject = true;
                var origFirstPersonPerspectiveItem = firstPersonVisibleItem;
                var visibleItemName = string.Empty;
                var visibleItemSearchName = string.Empty;
                // The visible item is a child of the object. When the object is instantiated the new visible item should be found again.
                // This is done by giving the visible item a unique name.
                if (firstPersonVisibleItem != null) {
                    visibleItemName = firstPersonVisibleItem.name;
                    firstPersonVisibleItem.name += Random.value.ToString(CultureInfo.InvariantCulture);

                    // Remember the path so the newly created visible item can be found again.
                    var parent = firstPersonVisibleItem.transform.parent;
                    visibleItemSearchName = firstPersonVisibleItem.name;
                    while (parent != firstPersonObject.transform && parent != null) {
                        visibleItemSearchName = parent.name + "/" + visibleItemSearchName;
                        parent = parent.parent;
                    }
                }

#if UNITY_EDITOR
                if (!Application.isPlaying && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(firstPersonVisibleItem))) {
                    firstPersonObject = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(firstPersonObject);
                } else {
#endif
                    firstPersonObject = Object.Instantiate(firstPersonObject);
#if UNITY_EDITOR
                }
#endif

                if (character == null) {
                    firstPersonObject.name = "FirstPerson" + name;
                } else {
                    firstPersonObject.name = firstPersonObject.name.Substring(0, firstPersonObject.name.Length - 7); // Remove "(Clone)".
                }

                AddFirstPersonArms(character, firstPersonObject, firstPersonObjectAnimatorController);

                // An ItemSlot must also be added to the base object if no visible item exists.
                if (firstPersonVisibleItem == null) {
                    firstPersonObject.AddComponent<CharacterItemSlot>();
                }

                // A new visible item would have been created.
                if (firstPersonVisibleItem != null) {
                    var foundVisibleItem = firstPersonObject.transform.Find(visibleItemSearchName);
                    if (foundVisibleItem != null) {
                        // The newly created visible item is now the main visible item.
                        firstPersonVisibleItem = foundVisibleItem.gameObject;
                    } else {
                        // The visible item may not have been a child of the first person object GameObject.
#if UNITY_EDITOR
                        if (!Application.isPlaying && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(firstPersonVisibleItem))) {
                            firstPersonVisibleItem = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(firstPersonVisibleItem);
                        } else {
#endif
                            firstPersonVisibleItem = Object.Instantiate(firstPersonVisibleItem);
#if UNITY_EDITOR
                        }
#endif

                        // The ItemSlot reference also needs to be updated.
                        var itemSlots = firstPersonObject.GetComponentsInChildren<CharacterItemSlot>();
                        for (int i = 0; i < itemSlots.Length; ++i) {
                            if (itemSlots[i].ID == firstPersonCharacterItemSlot.ID) {
                                firstPersonCharacterItemSlot = itemSlots[i];
                                break;
                            }
                        }
                        firstPersonVisibleItem.transform.SetParentOrigin(firstPersonCharacterItemSlot.transform);
                    }
                    origFirstPersonPerspectiveItem.name = firstPersonVisibleItem.name = visibleItemName;
                }
            } else if (firstPersonVisibleItem != null) {
#if UNITY_EDITOR
                if (!Application.isPlaying && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(firstPersonVisibleItem))) {
                    firstPersonVisibleItem = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(firstPersonVisibleItem);
                } else {
#endif
                    firstPersonVisibleItem = Object.Instantiate(firstPersonVisibleItem);
#if UNITY_EDITOR
                }
#endif

                firstPersonVisibleItem.name = (character == null ? "FirstPerson" : "") + name;
            }
            var perspectiveItem = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            perspectiveItem.Object = firstPersonObject;
            perspectiveItem.VisibleItem = firstPersonVisibleItem;

            if (firstPersonVisibleItem != null) {
                if (firstPersonVisibleItem.GetComponent<AudioSource>() == null) {
                    var audioSource = firstPersonVisibleItem.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1;
                    audioSource.maxDistance = 20;
                }
            }

            // The visible item can use an animator.
            if (firstPersonVisibleItemAnimatorController != null && firstPersonVisibleItem != null) {
                Animator animator;
                if ((animator = firstPersonVisibleItem.GetComponent<Animator>()) == null) {
                    animator = firstPersonVisibleItem.AddComponent<Animator>();
                }
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.runtimeAnimatorController = firstPersonVisibleItemAnimatorController;
                if (firstPersonVisibleItem.GetComponent<ChildAnimatorMonitor>() == null) {
                    firstPersonVisibleItem.AddComponent<ChildAnimatorMonitor>();
                }
            }

            Transform parentTransform = null;
            if (character != null) {
                // The object should be a child of the First Person Objects GameObject.
                if (firstPersonObject != null && parentFirstPersonObject) {
                    var firstPersonObjects = character.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
                    if (firstPersonObjects == null) {
                        Debug.LogError($"Error: Unable to find the FirstPersonObjects component within {character.name}.");
                        return;
                    } else {
                        parentTransform = firstPersonObjects.transform;
                    }
                } else if (firstPersonVisibleItem != null) {
                    parentTransform = firstPersonCharacterItemSlot.transform;
                }
            } else {
                // The object should be a child of the item GameObject.
                parentTransform = itemGameObject.transform;
            }

            // Assign the transform. The object will contain the visible item if it exists.
            var obj = firstPersonObject && parentFirstPersonObject ? firstPersonObject : firstPersonVisibleItem;
            if (obj != null) {
                obj.transform.SetParentOrigin(parentTransform);

                // The item's object should be on the first person overlay layer so it'll render over all other objects.
                obj.transform.SetLayerRecursively(LayerManager.Overlay);
            } else if (firstPersonVisibleItem != null) {
                firstPersonVisibleItem.transform.SetLayerRecursively(LayerManager.Overlay);
            }

            // Add any properties for actions which have already been added.
            AddPropertiesToActions(itemGameObject);
        }

        /// <summary>
        /// Adds the FirstPersonBaseObject to the arms.
        /// </summary>
        /// <param name="character">The character that contains the FirstPersonObject.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonObjectAnimatorController">A reference to the animator controller added to the first person object. Can be null.</param>
        public static void AddFirstPersonArms(GameObject character, GameObject firstPersonObject, RuntimeAnimatorController firstPersonObjectAnimatorController)
        {
            var maxID = -1;
            if (character != null && firstPersonObject.GetComponent<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>() == null) {
                // The base object ID must be unique.
                var baseObjects = character.GetComponentsInChildren<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
                for (int i = 0; i < baseObjects.Length; ++i) {
                    if (baseObjects[i].ID > maxID) {
                        maxID = (int)baseObjects[i].ID;
                    }
                }
            }

            if (firstPersonObject.GetComponent<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>() == null) {
                var baseObject = firstPersonObject.AddComponent<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
                baseObject.ID = (uint)(maxID + 1);
                firstPersonObject.transform.SetLayerRecursively(LayerManager.Overlay);
            }

            if (firstPersonObjectAnimatorController != null) {
                Animator animator;
                if ((animator = firstPersonObject.GetComponent<Animator>()) == null) {
                    animator = firstPersonObject.AddComponent<Animator>();
                }
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.runtimeAnimatorController = firstPersonObjectAnimatorController;
                if (firstPersonObject.GetComponent<ChildAnimatorMonitor>() == null) {
                    firstPersonObject.AddComponent<ChildAnimatorMonitor>();
                }
            }
        }

        /// <summary>
        /// Updates the animator on the FirstPersonObject.
        /// </summary>
        /// <param name="firstPersonObject">The object that should be updated.</param>
        /// <param name="firstPersonObjectAnimatorController">The Animator Controller that should be assigned to the First Person Object.</param>
        public static void UpdateFirstPersonAnimator(GameObject firstPersonObject, RuntimeAnimatorController firstPersonObjectAnimatorController)
        {
            if (firstPersonObject == null) {
                return;
            }

            if (firstPersonObjectAnimatorController != null) {
                Animator animator;
                if ((animator = firstPersonObject.GetComponent<Animator>()) == null) {
                    animator = firstPersonObject.AddComponent<Animator>();
                }
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.runtimeAnimatorController = firstPersonObjectAnimatorController;
                if (firstPersonObject.GetComponent<ChildAnimatorMonitor>() == null) {
                    firstPersonObject.AddComponent<ChildAnimatorMonitor>();
                }
            } else {
                var animatorMonitor = firstPersonObject.GetComponent<ChildAnimatorMonitor>();
                if (animatorMonitor != null) {
                    Object.DestroyImmediate(animatorMonitor, true);
                }
                var animator = firstPersonObject.GetComponent<Animator>();
                if (animator != null) {
                    Object.DestroyImmediate(animator, true);
                }
            }
        }
#endif

        /// <summary>
        /// Adds the CharacterItemSlot to the parent GameObject.
        /// </summary>
        /// <param name="parent">The GameObject that should have a child slot added.</param>
        /// <param name="id">The ID of the slot.</param>
        public static void AddItemSlot(GameObject parent, int id)
        {
            CharacterItemSlot itemSlot;
            if ((itemSlot = parent.GetComponentInChildren<Items.CharacterItemSlot>()) != null) {
                itemSlot.ID = id;
                return;
            }

            var items = new GameObject("Items");
            items.transform.SetParentOrigin(parent.transform);
            itemSlot = items.AddComponent<Items.CharacterItemSlot>();
            itemSlot.ID = id;
        }

        /// <summary>
        /// Removes the CharacterItemSlot from the parent GameObject.
        /// </summary>
        /// <param name="parent">The parent of the ChildItemSlot that should be removed.</param>
        public static void RemoveItemSlot(GameObject parent)
        {
            var itemSlot = parent.GetComponent<Items.CharacterItemSlot>();
            if (itemSlot == null) {
                return;
            }

            Object.DestroyImmediate(itemSlot.gameObject, true);
        }

        /// <summary>
        /// Adds the third person object to the specified item.
        /// </summary>
        /// <param name="character">The character that the third person object is being added to.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="itemGameObject">A reference to the item's GameObject.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        /// <param name="thirdPersonCharacterItemSlot">A reference to the ItemSlot to add the third person item to.</param>
        /// <param name="thirdPersonObjectAnimatorController">A reference to the animator controller added to the third person object. Can be null.</param>
        /// <param name="invisibleShadowCasterMaterial">A reference to the invisible shadow caster material. This is only used for first person characters.</param>
        /// <param name="defaultAddThirdPersonObject">Should the ThirdPersonObject component be added to the object?</param>
        public static void AddThirdPersonObject(GameObject character, string name, GameObject itemGameObject, ref GameObject thirdPersonObject, CharacterItemSlot thirdPersonCharacterItemSlot,
                                                RuntimeAnimatorController thirdPersonObjectAnimatorController, Material invisibleShadowCasterMaterial, bool defaultAddThirdPersonObject)
        {
            var visibleItem = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonObject != null) {
#if UNITY_EDITOR
                if (!Application.isPlaying && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(thirdPersonObject))) {
                    thirdPersonObject = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(thirdPersonObject);
                } else {
#endif
                    thirdPersonObject = Object.Instantiate(thirdPersonObject);
#if UNITY_EDITOR
                }
#endif
                thirdPersonObject.name = (character == null ? "ThirdPerson" : "") + name;
                visibleItem.Object = thirdPersonObject;

                var addThirdPersonObject = defaultAddThirdPersonObject;
#if THIRD_PERSON_CONTROLLER
                if (character != null && !addThirdPersonObject) {
                    var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
                    var movementTypes = characterLocomotion.MovementTypes;
                    if (movementTypes != null) {
                        for (int i = 0; i < movementTypes.Length; ++i) {
                            if (characterLocomotion.MovementTypes[i] == null) {
                                continue;
                            }
                            if (characterLocomotion.MovementTypes[i].GetType().FullName.Contains("ThirdPerson")) {
                                addThirdPersonObject = true;
                                break;
                            }
                        }
                    }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    var networkInfo = character.GetComponent<Shared.Networking.INetworkInfo>();
                    if (networkInfo != null) {
                        addThirdPersonObject = true;
                    }
#endif
                }
#else
                addThirdPersonObject = false;
#endif

                if (addThirdPersonObject) {
                    // The ThirdPersonObject component is added so the PerspectiveMonitor knows what objects should use the invisible shadow caster material.
                    thirdPersonObject.AddComponent<Character.Identifiers.ThirdPersonObject>();
                } else {
                    // If the ThirdPersonObject isn't added then the renderer should be directly attached.
                    var renderers = thirdPersonObject.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i) {
                        var materials = renderers[i].sharedMaterials;
                        for (int j = 0; j < materials.Length; ++j) {
                            materials[j] = invisibleShadowCasterMaterial;
                        }
                        renderers[i].sharedMaterials = materials;
                    }
                }

                if (thirdPersonObject.GetComponent<AudioSource>() == null) {
                    var audioSource = thirdPersonObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1;
                    audioSource.maxDistance = 20;
                }
                // Optionally add the animator.
                if (thirdPersonObjectAnimatorController != null) {
                    Animator animator;
                    if ((animator = thirdPersonObject.GetComponent<Animator>()) == null) {
                        animator = thirdPersonObject.AddComponent<Animator>();
                    }
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.runtimeAnimatorController = thirdPersonObjectAnimatorController;
                    if (thirdPersonObject.GetComponent<ChildAnimatorMonitor>() == null) {
                        thirdPersonObject.AddComponent<ChildAnimatorMonitor>();
                    }
                }
                Transform parentTransform = null;
                if (character != null) {
                    parentTransform = thirdPersonCharacterItemSlot.transform;
                } else {
                    // The object should be a child of the item GameObject.
                    parentTransform = itemGameObject.transform;
                }

                // Assign the transform position and layer.
                thirdPersonObject.transform.SetParentOrigin(parentTransform);
                thirdPersonObject.transform.SetLayerRecursively(LayerManager.SubCharacter);
            }

            // Add any properties for actions which have already been added.
            AddPropertiesToActions(itemGameObject);
        }

        /// <summary>
        /// Adds the properties to any actions already created.
        /// </summary>
        /// <param name="itemGameObject">A reference to the item's GameObject.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        public static void AddPropertiesToActions(GameObject itemGameObject)
        {
            var actions = itemGameObject.GetComponents<CharacterItemAction>();
            for (int i = 0; i < actions.Length; ++i) {
                if (actions[i] is ShootableAction) {
                    AddShootableActionProperties(actions[i] as ShootableAction);
                    continue;
                }
                if (actions[i] is MeleeAction) {
                    AddMeleeActionProperties(actions[i] as MeleeAction);
                    continue;
                }
                if (actions[i] is ThrowableAction) {
                    AddThrowableActionProperties(actions[i] as ThrowableAction);
                    continue;
                }
            }
        }

        /// <summary>
        /// Adds the specified ActionType to the item.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the action to.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        /// <param name="actionType">The type of action to add.</param>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>The added CharacterItemAction.</returns>
        public static CharacterItemAction AddAction(GameObject itemGameObject, GameObject firstPersonObject, GameObject firstPersonVisibleItem, 
                                                        GameObject thirdPersonObject, ActionType actionType, string actionName)
        {
            // The action ID must be unique.
            var maxID = -1;
            var actions = itemGameObject.GetComponents<CharacterItemAction>();
            for (int i = 0; i < actions.Length; ++i) {
                if (actions[i].ID > maxID) {
                    maxID = actions[i].ID;
                }
            }

            CharacterItemAction itemAction = null;
            switch (actionType) {
                case ActionType.Shootable:
                    var shootableAction = itemGameObject.AddComponent<ShootableAction>();
                    itemAction = shootableAction;
                    shootableAction.ID = maxID + 1;
                    shootableAction.ActionName = actionName;
                    AddShootableActionProperties(shootableAction);
                    break;
                case ActionType.Melee:
                    var meleeAction = itemGameObject.AddComponent<MeleeAction>();
                    itemAction = meleeAction;
                    meleeAction.ID = maxID + 1;
                    meleeAction.FaceTarget = false;
                    meleeAction.ActionName = actionName;
                    AddMeleeActionProperties(meleeAction);
                    break;
                case ActionType.Shield:
                    var shieldAction = itemGameObject.AddComponent<ShieldAction>();
                    itemAction = shieldAction;
                    shieldAction.ID = maxID + 1;
                    shieldAction.ActionName = actionName;
                    var shieldAttributeManager = itemGameObject.AddComponent<Traits.AttributeManager>();
                    shieldAttributeManager.Attributes[0].Name = "Durability"; // Rename the Health attribute to Durability.
                    AddShieldActionProperties(shieldAction, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    // The Block ability should be added if it isn't already.
                    var characterLocomotion = itemGameObject.GetComponentInParent<UltimateCharacterLocomotion>();
                    if (characterLocomotion != null) {
                        var blockAbility = characterLocomotion.GetAbility<Character.Abilities.Items.Block>();
                        if (blockAbility == null) {
                            AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.Items.Block));
                        }
                    }
                    break;
                case ActionType.Magic:
                    var magicAction = itemGameObject.AddComponent<MagicAction>();
                    itemAction = magicAction;
                    magicAction.ActionName = actionName;
                    var item = itemGameObject.GetComponent<CharacterItem>();
                    item.EquipEvent = new AnimationSlotEventTrigger(false, 0);
                    item.UnequipEvent = new AnimationSlotEventTrigger(false, 0);
                    break;
                case ActionType.Throwable:
                    var throwableAction = itemGameObject.AddComponent<ThrowableAction>();
                    itemAction = throwableAction;
                    throwableAction.ID = maxID + 1;
                    throwableAction.ActionName = actionName;
                    AddThrowableActionProperties(throwableAction);
                    break;
                case ActionType.Usable:
                    var flashLight = itemGameObject.AddComponent<UsableAction>();
                    itemAction = flashLight;
                    flashLight.ID = maxID + 1;
                    flashLight.ActionName = actionName;
                    flashLight.UseEvent = new AnimationSlotEventTrigger(false, 0);
                    flashLight.UseCompleteEvent = new AnimationSlotEventTrigger(false, 0);
                    var flashlightAttributeManager = itemGameObject.AddComponent<Traits.AttributeManager>();
                    flashlightAttributeManager.Attributes[0].Name = "Battery"; // Rename the Health attribute to Battery.
                    AddFlashlightProperties(flashLight, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    break;
            }
            return itemAction;
        }

        /// <summary>
        /// Adds the ShootableAction properties to the specified GameObject.
        /// </summary>
        /// <param name="shootableAction">The action that is being modified.</param>
        private static void AddShootableActionProperties(ShootableAction shootableAction)
        {
            shootableAction.TriggerActionModuleGroup = new ActionModuleGroup<TriggerModule>();
            shootableAction.TriggerActionModuleGroup.AddModule(new Repeat(), shootableAction.gameObject);

            shootableAction.ShooterModuleGroup = new ActionModuleGroup<ShootableShooterModule>();
            shootableAction.ShooterModuleGroup.AddModule(new HitscanShooter(), shootableAction.gameObject);

            shootableAction.AmmoModuleGroup = new ActionModuleGroup<ShootableAmmoModule>();
            shootableAction.AmmoModuleGroup.AddModule(new ItemAmmo(), shootableAction.gameObject);

            shootableAction.ClipModuleGroup = new ActionModuleGroup<ShootableClipModule>();
            shootableAction.ClipModuleGroup.AddModule(new SimpleClip(), shootableAction.gameObject);

            shootableAction.ProjectileModuleGroup = new ActionModuleGroup<ShootableProjectileModule>();
            shootableAction.ProjectileModuleGroup.AddModule(new BasicProjectile(), shootableAction.gameObject);

            shootableAction.FireEffectsModuleGroup = new ActionModuleGroup<ShootableFireEffectModule>();
            shootableAction.FireEffectsModuleGroup.AddModule(new MuzzleEffect(), shootableAction.gameObject);
            shootableAction.FireEffectsModuleGroup.AddModule(new ShellEffect(), shootableAction.gameObject);
            shootableAction.FireEffectsModuleGroup.AddModule(new RecoilEffect(), shootableAction.gameObject);
            shootableAction.FireEffectsModuleGroup.AddModule(new CrosshairsSpread(), shootableAction.gameObject);

            shootableAction.DryFireEffectsModuleGroup = new ActionModuleGroup<ShootableFireEffectModule>();
            shootableAction.DryFireEffectsModuleGroup.AddModule(new GenericItemEffects(), shootableAction.gameObject);

            shootableAction.DryFireEffectsModuleGroup = new ActionModuleGroup<ShootableFireEffectModule>();
            shootableAction.DryFireEffectsModuleGroup.AddModule(new GenericItemEffects(), shootableAction.gameObject);

            shootableAction.ImpactModuleGroup = new ActionModuleGroup<ShootableImpactModule>();
            shootableAction.ImpactModuleGroup.AddModule(new GenericShootableImpactModule(), shootableAction.gameObject);

            shootableAction.ReloaderModuleGroup = new ActionModuleGroup<ShootableReloaderModule>();
            shootableAction.ReloaderModuleGroup.AddModule(new GenericReloader(), shootableAction.gameObject);

            shootableAction.ExtraModuleGroup = new ActionModuleGroup<ShootableExtraModule>();
            shootableAction.ExtraModuleGroup.AddModule(new DryFireSubstate(), shootableAction.gameObject);
            shootableAction.ExtraModuleGroup.AddModule(new Items.Actions.Modules.Shootable.SlotItemMonitorModule(), shootableAction.gameObject);
        }

        /// <summary>
        /// Adds the MeleeAction properties to the specified GameObject.
        /// </summary>
        /// <param name="meleeAction">The melee action that is being modified.</param>
        private static void AddMeleeActionProperties(MeleeAction meleeAction)
        {
            meleeAction.TriggerActionModuleGroup = new ActionModuleGroup<TriggerModule>();
            meleeAction.TriggerActionModuleGroup.AddModule(new RepeatCombo(), meleeAction.gameObject);

            meleeAction.AttackModuleGroup = new ActionModuleGroup<MeleeAttackModule>();
            meleeAction.AttackModuleGroup.AddModule(new SimpleAttack(), meleeAction.gameObject);

            meleeAction.CollisionModuleGroup = new ActionModuleGroup<MeleeCollisionModule>();
            meleeAction.CollisionModuleGroup.AddModule(new HitboxCollision(), meleeAction.gameObject);

            meleeAction.ImpactModuleGroup = new ActionModuleGroup<MeleeImpactModule>();
            meleeAction.ImpactModuleGroup.AddModule(new GenericMeleeImpactModule(), meleeAction.gameObject);

            meleeAction.RecoilModuleGroup = new ActionModuleGroup<MeleeRecoilModule>();
            meleeAction.RecoilModuleGroup.AddModule(new SimpleRecoil(), meleeAction.gameObject);
        }

        /// <summary>
        /// Adds the ShieldAction properties to the specified GameObject.
        /// </summary>
        /// <param name="shieldAction">A reference to the parent Shield component.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddShieldActionProperties(ShieldAction shieldAction, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var shieldCollider = firstPersonVisibleItem.AddComponent<Objects.ItemAssist.ShieldCollider>();
                shieldCollider.ShieldAction = shieldAction;

                if (firstPersonVisibleItem.GetComponent<BoxCollider>() == null) {
                    firstPersonVisibleItem.AddComponent<BoxCollider>();
                }
            }
#endif
            if (thirdPersonObject != null) {
                var shieldCollider = thirdPersonObject.AddComponent<Objects.ItemAssist.ShieldCollider>();
                shieldCollider.ShieldAction = shieldAction;

                if (thirdPersonObject.GetComponent<BoxCollider>() == null) {
                    thirdPersonObject.AddComponent<BoxCollider>();
                }
            }
        }

        /// <summary>
        /// Adds the ThrowableAction properties to the specified GameObject.
        /// </summary>
        /// <param name="throwableAction">The throwable action that is being modified.</param>
        private static void AddThrowableActionProperties(ThrowableAction throwableAction)
        {
            throwableAction.TriggerActionModuleGroup = new ActionModuleGroup<TriggerModule>();
            throwableAction.TriggerActionModuleGroup.AddModule(new Simple(), throwableAction.gameObject);

            throwableAction.ThrowerModuleGroup = new ActionModuleGroup<ThrowableThrowerModule>();
            throwableAction.ThrowerModuleGroup.AddModule(new ProjectileThrower(), throwableAction.gameObject);

            throwableAction.AmmoModuleGroup = new ActionModuleGroup<ThrowableAmmoModule>();
            throwableAction.AmmoModuleGroup.AddModule(new Items.Actions.Modules.Throwable.ItemAmmo(), throwableAction.gameObject);

            throwableAction.ProjectileModuleGroup = new ActionModuleGroup<ThrowableProjectileModule>();
            throwableAction.ProjectileModuleGroup.AddModule(new SpawnProjectile(), throwableAction.gameObject);

            throwableAction.ImpactModuleGroup = new ActionModuleGroup<ThrowableImpactModule>();
            throwableAction.ImpactModuleGroup.AddModule(new GenericThrowableImpactModule(), throwableAction.gameObject);

            throwableAction.ReequiperModuleGroup = new ActionModuleGroup<ThrowableReequipperModule>();
            throwableAction.ReequiperModuleGroup.AddModule(new SimpleReequipper() { CanEquipEmptyItem = true }, throwableAction.gameObject);

            throwableAction.ExtraModuleGroup = new ActionModuleGroup<ThrowableExtraModule>();
            throwableAction.ExtraModuleGroup.AddModule(new ThrowableVisualizeTrajectory(), throwableAction.gameObject);
            throwableAction.ExtraModuleGroup.AddModule(new Items.Actions.Modules.Throwable.SlotItemMonitorModule(), throwableAction.gameObject);

            // Throwable items should be completely dropped.
            var item = throwableAction.gameObject.GetComponent<CharacterItem>();
            item.FullInventoryDrop = true;

            throwableAction.gameObject.AddComponent<Objects.TrajectoryObject>();
        }

        /// <summary>
        /// Adds the flashlight properties to the specified GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the properties to.</param>
        /// <param name="usableAction">The usable action that is being modified.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddFlashlightProperties(UsableAction usableAction, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
            usableAction.TriggerActionModuleGroup = new ActionModuleGroup<TriggerModule>();
            usableAction.TriggerActionModuleGroup.AddModule(new Simple(), usableAction.gameObject);

            usableAction.UsableActionModuleGroup = new ActionModuleGroup<UsableActionModule>();
            var useModifier = new UseAttributeModifierToggle();
            useModifier.UseModifier.AttributeName = "Battery";
            useModifier.GameObjectToggle = new PerspectiveGameObjectToggle[] { new PerspectiveGameObjectToggle() };
            useModifier.GameObjectToggle[0].GameObject = new ItemPerspectiveIDObjectProperty<GameObject>();
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var lightGameObject = new GameObject("Light", typeof(Light));
                lightGameObject.transform.SetParentOrigin((firstPersonVisibleItem != null ? firstPersonVisibleItem : firstPersonObject).transform);
                useModifier.GameObjectToggle[0].GameObject.SetFirstPersonValue(lightGameObject);
            }
            if (thirdPersonObject != null) {
                var lightGameObject = new GameObject("Light", typeof(Light));
                lightGameObject.transform.SetParentOrigin(thirdPersonObject.transform);
                useModifier.GameObjectToggle[0].GameObject.SetThirdPersonValue(lightGameObject);
            }
            usableAction.UsableActionModuleGroup.AddModule(useModifier, usableAction.gameObject);
        }

        /// <summary>
        /// Adds the specified ActionType to the item.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the action to.</param>
        /// <param name="actionType">The type of action to add.</param>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>The added CharacterItemAction.</returns>
        public static CharacterItemAction AddAction(GameObject itemGameObject, ActionType actionType, string actionName)
        {
            GameObject firstPersonObject = null, firstPersonVisibleItemGameObject = null, thirdPersonObject = null;
            PopulatePerspectiveObjects(itemGameObject, ref firstPersonObject, ref firstPersonVisibleItemGameObject, ref thirdPersonObject);
            return AddAction(itemGameObject, firstPersonObject, firstPersonVisibleItemGameObject, thirdPersonObject, actionType, actionName);
        }

        /// <summary>
        /// Populates the first and third person objects for the specified item GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to get the first and third person references of.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItemGameObject">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void PopulatePerspectiveObjects(GameObject itemGameObject, ref GameObject firstPersonObject, ref GameObject firstPersonVisibleItemGameObject, ref GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            var firstPersonVisibleItem = itemGameObject.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonVisibleItem != null) {
                firstPersonObject = firstPersonVisibleItem.Object;
                firstPersonVisibleItemGameObject = firstPersonVisibleItem.VisibleItem;
            }
#endif
            var thirdPersonVisibleItem = itemGameObject.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonVisibleItem != null) {
                thirdPersonObject = thirdPersonVisibleItem.Object;
            }
        }
    }
}