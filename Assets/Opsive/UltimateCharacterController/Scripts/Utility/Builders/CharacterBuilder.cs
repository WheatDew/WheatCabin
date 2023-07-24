/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Allows for the Ultimate Character Controller components to be added/removed at runtime.
    /// </summary>
    public static class CharacterBuilder
    {
        private const string c_MovingStateGUID = "527d884c54f1a4b4a82fed73411305a8";
        private const string c_MoveTowardsStateGUID = "69afbe578b168cd4f99b2873bfca1a8f";
        private const string c_CharacterPhysicMaterialGUID = "b202ee37f3bfe31498a60fa2c5a67885";

        /// <summary>
        /// Adds the essnetial components to the specified character and sets the MovementType.
        /// </summary>
        /// <param name="character">The GameObject of the character.</param>
        /// <param name="characterModels">The models associated with the character. Can be null.</param>
        /// <param name="addAnimator">Should the animator components be added?</param>
        /// <param name="animatorControllers">A reference to the animator controllers. Multiple animator controllers can be specified for multiple character models.</param>
        /// <param name="firstPersonMovementType">The first person MovementType that should be added.</param>
        /// <param name="thirdPersonMovementType">The third person MovementType that should be added.</param>
        /// <param name="startFirstPersonPerspective">Should the character start in a first person perspective?</param>
        /// <param name="thirdPersonObjects">The objects that should be hidden in first person view.</param>
        /// <param name="invisibleShadowCasterMaterial">The shadow caster material applied to the invisible first person objects.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        public static void BuildCharacter(GameObject character, GameObject[] characterModels, bool addAnimator, RuntimeAnimatorController[] animatorControllers, string firstPersonMovementType, string thirdPersonMovementType, bool startFirstPersonPerspective,
            GameObject[][] thirdPersonObjects, Material invisibleShadowCasterMaterial, bool aiAgent)
        {
            // Determine if the ThirdPersonObject component should be added or the invisible object renderer should be directly set to the invisible shadow caster.
            if (thirdPersonObjects != null) {
                for (int i = 0; i < thirdPersonObjects.Length; ++i) {
                    if (thirdPersonObjects[i] == null) {
                        continue;
                    }

                    for (int j = 0; j < thirdPersonObjects[i].Length; ++j) {
                        if (thirdPersonObjects[i][j] == null) {
                            continue;
                        }

                        if (string.IsNullOrEmpty(thirdPersonMovementType)) {
                            var renderers = thirdPersonObjects[i][j].GetComponents<Renderer>();
                            for (int k = 0; k < renderers.Length; ++k) {
                                var materials = renderers[k].sharedMaterials;
                                for (int m = 0; m < materials.Length; ++m) {
                                    materials[m] = invisibleShadowCasterMaterial;
                                }
                                renderers[k].sharedMaterials = materials;
                            }
                        }
                        thirdPersonObjects[i][j].AddComponent<ThirdPersonObject>();
                    }
                }
            }

            AddEssentials(character, characterModels, addAnimator, animatorControllers, !string.IsNullOrEmpty(firstPersonMovementType) && !string.IsNullOrEmpty(thirdPersonMovementType), invisibleShadowCasterMaterial, aiAgent);

            // The last added MovementType is starting movement type.
            if (startFirstPersonPerspective) {
                if (!string.IsNullOrEmpty(thirdPersonMovementType)) {
                    AddMovementType(character, thirdPersonMovementType);
                }
                if (!string.IsNullOrEmpty(firstPersonMovementType)) {
                    AddMovementType(character, firstPersonMovementType);
                }
            } else {
                if (!string.IsNullOrEmpty(firstPersonMovementType)) {
                    AddMovementType(character, firstPersonMovementType);
                }
                if (!string.IsNullOrEmpty(thirdPersonMovementType)) {
                    AddMovementType(character, thirdPersonMovementType);
                }
            }
        }

        /// <summary>
        /// Adds the Ultimate Character Controller essential components to the specified character.
        /// </summary>
        /// <param name="character">The character to add the components to.</param>
        /// <param name="characterModels">The models associated with the character. Can be null.</param>
        /// <param name="addAnimator">Should the animator components be added?</param>
        /// <param name="animatorControllers">A reference to the animator controllers. Multiple animator controllers can be specified for multiple character models.</param>
        /// <param name="addPerspectiveMonitor">Should the perspective monitor be added?</param>
        /// <param name="invisibleShadowCasterMaterial">The shadow caster material applied to the invisible first person objects.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        public static void AddEssentials(GameObject character, GameObject[] characterModels, bool addAnimator, RuntimeAnimatorController[] animatorControllers, bool addPerspectiveMonitor, Material invisibleShadowCasterMaterial, bool aiAgent)
        {
            if (!aiAgent) {
                character.tag = "Player";
            }
            character.layer = LayerManager.Character;
            if (character.GetComponent<CharacterLayerManager>() == null) {
                character.AddComponent<CharacterLayerManager>();
            }

            var rigidbody = character.GetComponent<Rigidbody>();
            if (rigidbody == null) {
                rigidbody = character.AddComponent<Rigidbody>();
            }
            rigidbody.drag = rigidbody.angularDrag = 0;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.mass = 80;

            var autoInstantiation = CharacterInitializer.AutoInitialization;
            if (Application.isPlaying) {
                CharacterInitializer.AutoInitialization = false;
            }

            if (addAnimator) {
                AddAnimator(characterModels, animatorControllers, aiAgent);
            }

            if (character.GetComponent<UltimateCharacterLocomotion>() == null) {
                var characterLocomotion = character.AddComponent<UltimateCharacterLocomotion>();
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    // The Moving and Move Towards states should automatically be added.
                    var movingPresetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(c_MovingStateGUID);
                    var moveTowardsPresetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(c_MoveTowardsStateGUID);
                    if (!string.IsNullOrEmpty(movingPresetPath) || !string.IsNullOrEmpty(moveTowardsPresetPath)) {
                        var movingPreset = UnityEditor.AssetDatabase.LoadAssetAtPath(movingPresetPath, typeof(PersistablePreset)) as PersistablePreset;
                        var moveTowardsPreset = UnityEditor.AssetDatabase.LoadAssetAtPath(moveTowardsPresetPath, typeof(PersistablePreset)) as PersistablePreset;
                        if (movingPreset != null || moveTowardsPreset != null) {
                            var states = characterLocomotion.States;
                            System.Array.Resize(ref states, states.Length + (movingPreset != null ? 1 : 0) + (moveTowardsPreset != null ? 1 : 0));
                            // Default must always be at the end.
                            states[states.Length - 1] = states[0];
                            if (movingPreset != null) {
                                states[states.Length - 2] = new State("Moving", movingPreset, null);
                            }
                            if (moveTowardsPreset != null) {
                                states[states.Length - 2 - (movingPreset != null ? 1 : 0)] = new State("MoveTowards", moveTowardsPreset, null);
                            }
                            characterLocomotion.States = states;
                        }
                    }
                }
#endif
                if (aiAgent) {
                    characterLocomotion.MotorRotationSpeed *= 10;
                } 
            }

            if (characterModels != null && characterModels.Length > 1) {
                AddCharacterModels(character, characterModels);
            }

            // Setup the collider parent on each character model.
            for (int i = 0; i < (characterModels != null ? characterModels.Length : 1); ++i) {
                GameObject colliderParent;
                if (characterModels == null) {
                    colliderParent = character;
                } else {
                    colliderParent = characterModels[i];
                }
                AddCollider(colliderParent);
            }

            if (aiAgent) {
                AddAIAgent(character);
            } else {
                AddUnityInput(character);

                if (character.GetComponent<UltimateCharacterLocomotionHandler>() == null) {
                    character.AddComponent<UltimateCharacterLocomotionHandler>();
                }
            }

#if THIRD_PERSON_CONTROLLER
            if (addPerspectiveMonitor && character.GetComponent<ThirdPersonController.Character.PerspectiveMonitor>() == null) {
                var perspectiveMonitor = character.AddComponent<ThirdPersonController.Character.PerspectiveMonitor>();
                if (perspectiveMonitor.InvisibleMaterial == null) {
                    perspectiveMonitor.InvisibleMaterial = invisibleShadowCasterMaterial;
                }
            }
#endif

            // All of the child GameObjects should be set to the SubCharacter layer to prevent any added-colliders from interferring with the locomotion.
            SetRecursiveLayer(character, LayerManager.SubCharacter, LayerManager.Character);

            if (Application.isPlaying) {
                CharacterInitializer.AutoInitialization = autoInstantiation;
                if (autoInstantiation) {
                    CharacterInitializer.Instance.OnAwake();
                    CharacterInitializer.Instance.OnEnable();

                    GameObject.DestroyImmediate(CharacterInitializer.Instance.gameObject, true);
                }
            }
        }

        /// <summary>
        /// Adds the animator with the specified controller to the character.
        /// </summary>
        /// <param name="characterModels">The models associated with the character.</param>
        /// <param name="animatorControllers">A reference to the animator controllers. Multiple animator controllers can be specified for multiple character models.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        public static void AddAnimator(GameObject[] characterModels, RuntimeAnimatorController[] animatorControllers, bool aiAgent)
        {
            // Setup the collider parent on each character model.
            for (int i = 0; i < characterModels.Length; ++i) {
                AddAnimator(characterModels[i], animatorControllers[i], aiAgent);
            }
        }

        /// <summary>
        /// Adds the animator with the specified controller to the character.
        /// </summary>
        /// <param name="character">The character that should have the Animator added.</param>
        /// <param name="animatorController">The Animator Controller that should be set..</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        public static void AddAnimator(GameObject character, RuntimeAnimatorController animatorController, bool aiAgent)
        {
            Animator animator;
            if ((animator = character.GetComponent<Animator>()) == null) {
                animator = character.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = animatorController;
            animator.updateMode = AnimatorUpdateMode.Normal;
            if (!aiAgent) {
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            if (character.GetComponent<AnimatorMonitor>() == null) {
                character.AddComponent<AnimatorMonitor>();
            }
        }

        /// <summary>
        /// Removes the animator from the character.
        /// </summary>
        /// <param name="character">The character to remove the animator from.</param>
        public static void RemoveAnimator(GameObject character)
        {
            var animatorMonitors = character.GetComponentsInChildren<AnimatorMonitor>();
            for (int i = animatorMonitors.Length - 1; i >= 0; --i) {
                var animator = animatorMonitors[i].GetComponent<Animator>();
                if (animator != null) {
                    Object.DestroyImmediate(animator, true);
                }
                Object.DestroyImmediate(animatorMonitors[i], true);
            }
        }

        /// <summary>
        /// Adds the collider structure to the character.
        /// </summary>
        /// <param name="character">The character that should have a collider added.</param>
        public static void AddCollider(GameObject character)
        {
            GameObject collider = null;
            var colliderIdentifier = character.GetComponent<CharacterColliderBaseIdentifier>();
            if (colliderIdentifier == null) {
                var colliders = new GameObject("Colliders");
                colliders.AddComponent<CharacterColliderBaseIdentifier>();
                colliders.layer = LayerManager.Character;
                colliders.transform.SetParentOrigin(character.transform);
                collider = new GameObject("CapsuleCollider");
                collider.layer = LayerManager.Character;
                collider.transform.SetParentOrigin(colliders.transform);
                var capsuleCollider = collider.AddComponent<CapsuleCollider>();
                capsuleCollider.center = new Vector3(0, 1, 0);
                capsuleCollider.height = 2;
                capsuleCollider.radius = 0.4f;
#if UNITY_EDITOR
                var physicMaterialPath = UnityEditor.AssetDatabase.GUIDToAssetPath(c_CharacterPhysicMaterialGUID);
                if (!string.IsNullOrEmpty(physicMaterialPath)) {
                    var colliderPhysicMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath(physicMaterialPath, typeof(PhysicMaterial)) as PhysicMaterial;
                    capsuleCollider.material = colliderPhysicMaterial;
                }
#endif
            }

            if (collider != null) {
                var positioner = collider.AddComponent<CapsuleColliderPositioner>();
                positioner.FirstEndCapTarget = character.transform;

                var animator = character.GetComponent<Animator>();
                if (animator != null) {
                    // The CapsuleColliderPositioner should follow the character's movements.
                    var head = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (head != null) {
                        positioner.SecondEndCapTarget = head;
                        positioner.RotationBone = positioner.PositionBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the GameObject to the specified layer. Will recursively set the children unless the child contains a component that shouldn't be set.
        /// </summary>
        /// <param name="gameObject">The GameObject to set.</param>
        /// <param name="layer">The layer to set the GameObject to.</param>
        /// <param name="characterLayer">The layer of the character. GameObjects with this layer will not be set to the specified layer.</param>
        private static void SetRecursiveLayer(GameObject gameObject, int layer, int characterLayer)
        {
            var children = gameObject.transform.childCount;
            for (int i = 0; i < children; ++i) {
                var child = gameObject.transform.GetChild(i);
                // Do not set the layer if the child is already set to the Character layer or contains the item identifier components.
                if (child.gameObject.layer == characterLayer || child.GetComponent<Items.ItemPlacement>() != null) {
                    continue;
                }

#if FIRST_PERSON_CONTROLLER
                // First person objects do not need to be set.
                if (child.GetComponent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                    continue;
                }
#endif

                // Set the layer.
                var childGameObject = child.gameObject;
                childGameObject.layer = layer;
                SetRecursiveLayer(childGameObject, layer, characterLayer);
            }
        }

        /// <summary>
        /// Removes the Ultimate Character Controller essential components from the specified character.
        /// </summary>
        /// <param name="character">The character to remove the components from.</param>
        public static void RemoveEssentials(GameObject character)
        {
            var rigidbody = character.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                Object.DestroyImmediate(rigidbody, true);
            }

            var colliders = character.GetComponentsInChildren<CharacterColliderBaseIdentifier>();
            if (colliders != null) {
                for (int i = 0; i < colliders.Length; ++i) {
                    Object.DestroyImmediate(colliders[i], true);
                }
            }

            var ultimateCharacterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            if (ultimateCharacterLocomotion != null) {
                Object.DestroyImmediate(ultimateCharacterLocomotion, true);
            }

            var ultimateCharacterLocomotionHandler = character.GetComponent<UltimateCharacterLocomotionHandler>();
            if (ultimateCharacterLocomotionHandler != null) {
                Object.DestroyImmediate(ultimateCharacterLocomotionHandler, true);
            }

            var localLookSource = character.GetComponent<LocalLookSource>();
            if (localLookSource != null) {
                Object.DestroyImmediate(localLookSource, true);
            }

            var layerManager = character.GetComponent<CharacterLayerManager>();
            if (layerManager != null) {
                Object.DestroyImmediate(layerManager, true);
            }

#if THIRD_PERSON_CONTROLLER
            var perspectiveMonitor = character.GetComponent<ThirdPersonController.Character.PerspectiveMonitor>();
            if (perspectiveMonitor != null) {
                Object.DestroyImmediate(perspectiveMonitor, true);
            }
#endif
        }

        /// <summary>
        /// Adds the specified MovementType to the character.
        /// </summary>
        /// <param name="character">The character to add the MovementType to.</param>
        /// <param name="movementType">The MovementType to add.</param>
        public static void AddMovementType(GameObject character, string movementType)
        {
            var CharacterLocomotion = character.GetComponent<UltimateCharacterController.Character.UltimateCharacterLocomotion>();
            if (CharacterLocomotion != null) {
                // Don't allow duplicate MovementTypes.
                var type = System.Type.GetType(movementType);
                var movementTypes = CharacterLocomotion.MovementTypes;
                var add = true;
                if (movementTypes != null) {
                    for (int i = 0; i < movementTypes.Length; ++i) {
                        if (movementTypes[i].GetType() == type) {
                            add = false;
                        }
                    }
                }
                if (add) {
                    var movementTypesList = new List<MovementType>();
                    if (movementTypes != null) {
                        movementTypesList.AddRange(movementTypes);
                    }
                    var movementTypeObj = System.Activator.CreateInstance(type) as MovementType;
                    movementTypesList.Add(movementTypeObj);
                    CharacterLocomotion.MovementTypes = movementTypesList.ToArray();

                    // If the character has already been initialized then the movement type should be initialized.
                    if (Application.isPlaying) {
                        movementTypeObj.Initialize(CharacterLocomotion);
                        movementTypeObj.Awake();
                    }
                }

                // Set the added movement type as the default.
                CharacterLocomotion.SetMovementType(type);
            }
        }

        /// <summary>
        /// Adds the non-essential Ultimate Character Controller components to the character.
        /// </summary>
        /// <param name="character">The character to add the components to.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        /// <param name="addItems">Should the item components be added?</param>
        /// <param name="itemCollection">A reference to the ItemCollection component.</param>
        /// <param name="itemSetRule">A reference to the ItemSetRule component.</param>
        /// <param name="firstPersonItems">Does the character support first person items?</param>
        /// <param name="addHealth">Should the health components be added?</param>
        /// <param name="addUnityIK">Should the CharacterIK component be added?</param>
        /// <param name="addFootEffects">Should the CharacterFootEffects component be added?</param>
        /// <param name="addStandardAbilities">Should the standard abilities be added?</param>
        /// <param name="addNavMeshAgent">Should the NavMeshAgent component be added?</param>
        public static void BuildCharacterComponents(GameObject character, bool aiAgent, bool addItems,
            ItemCollection itemCollection, ItemSetRuleBase itemSetRule, bool firstPersonItems, bool addHealth, bool addUnityIK, bool addFootEffects, bool addStandardAbilities, bool addNavMeshAgent)
        {
            var autoInstantiation = CharacterInitializer.AutoInitialization;
            if (Application.isPlaying) {
                CharacterInitializer.AutoInitialization = false;
            }

            if (addItems) {
                AddItemSupport(character, itemCollection, itemSetRule, aiAgent, firstPersonItems);
            }
            if (addHealth) {
                AddHealth(character);
            }
            if (addUnityIK) {
                AddUnityIK(character);
            }
            if (addFootEffects) {
                AddFootEffects(character);
            }
            if (addStandardAbilities) {
                // Add the Jump, Fall, Speed Change, and Height Change abilities.
                var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
                var jump = AbilityBuilder.AddAbility(characterLocomotion, typeof(Jump));
                if (characterLocomotion.GetComponentInChildren<AnimatorMonitor>() == null) {
                    (jump as Jump).JumpEvent = new AnimationEventTrigger(false, 0);
                }
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Fall));
                AbilityBuilder.AddAbility(characterLocomotion, typeof(MoveTowards));
                AbilityBuilder.AddAbility(characterLocomotion, typeof(SpeedChange));
                AbilityBuilder.AddAbility(characterLocomotion, typeof(HeightChange));
                // The abilities should not use an input related start type.
                if (aiAgent) {
                    var abilities = characterLocomotion.GetAbilities<Ability>();
                    for (int i = 0; i < abilities.Length; ++i) {
                        if (abilities[i].StartType != Ability.AbilityStartType.Automatic &&
                            abilities[i].StartType != Ability.AbilityStartType.Manual) {
                            abilities[i].StartType = Ability.AbilityStartType.Manual;
                        }
                        if (abilities[i].StopType != Ability.AbilityStopType.Automatic &&
                            abilities[i].StopType != Ability.AbilityStopType.Manual) {
                            abilities[i].StopType = Ability.AbilityStopType.Manual;
                        }
                        if (abilities[i] is Character.Abilities.Items.Use) {
                            abilities[i].StopType = Ability.AbilityStopType.Manual;
                        }
                    }
                }
            }
            if (addNavMeshAgent) {
                var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
                var abilities = characterLocomotion.Abilities;
                var index = abilities != null ? abilities.Length : 0;
                if (abilities != null) {
                    for (int i = 0; i < abilities.Length; ++i) {
                        if (abilities[i] is SpeedChange || abilities[i] is MoveTowards) {
                            index = i;
                            break;
                        }
                    }
                }
                // The ability should be positioned before the SpeedChange ability.
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.AI.NavMeshAgentMovement), index);
                var navMeshAgent = character.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navMeshAgent != null) {
                    navMeshAgent.stoppingDistance = 0.1f;
                }
            }

            if (Application.isPlaying) {
                CharacterInitializer.AutoInitialization = autoInstantiation;
                if (autoInstantiation) {
                    CharacterInitializer.Instance.OnAwake();

                    GameObject.DestroyImmediate(CharacterInitializer.Instance.gameObject, true);
                }
            }
        }

        /// <summary>
        /// Adds the ai agent components to the character.
        /// </summary>
        /// <param name="character">The character to add the ai agent components to.</param>
        public static void AddAIAgent(GameObject character)
        {
            if (character.GetComponent<LocalLookSource>() == null) {
                character.AddComponent<LocalLookSource>();
            }

            var locomotionHandler = character.GetComponent<UltimateCharacterLocomotionHandler>();
            if (locomotionHandler != null) {
                Object.DestroyImmediate(locomotionHandler, true);
            }

            var itemHandler = character.GetComponent<ItemHandler>();
            if (itemHandler != null) {
                Object.DestroyImmediate(itemHandler, true);
            }

            RemoveUnityInput(character);
        }

        /// <summary>
        /// Removes the ai agent components from the character.
        /// </summary>
        /// <param name="character">The character to remove the ai agent components to.</param>
        public static void RemoveAIAgent(GameObject character)
        {
            var localLookSource = character.GetComponent<LocalLookSource>();
            if (localLookSource != null) {
                Object.DestroyImmediate(localLookSource, true);
            }

            if (character.GetComponent<UltimateCharacterLocomotionHandler>() == null) {
                character.AddComponent<UltimateCharacterLocomotionHandler>();
            }

            if (character.GetComponent<InventoryBase>() != null && character.GetComponent<ItemHandler>() == null) {
                character.AddComponent<ItemHandler>();
            }

            AddUnityInput(character);
        }

        /// <summary>
        /// Adds the UnityInput component to the character.
        /// </summary>
        /// <param name="character">The character to add the UnityInput component to.</param>
        public static void AddUnityInput(GameObject character)
        {
            if (character.GetComponentInChildren<Shared.Input.UnityInput>() == null) {
                var inputGameObject = new GameObject(character.name + "Input");
                inputGameObject.transform.parent = character.transform;
                var unityInput = inputGameObject.AddComponent<Shared.Input.UnityInput>();
                var inputProxy = character.AddComponent<Shared.Input.PlayerInputProxy>();
                inputProxy.PlayerInput = unityInput;
            }
        }

        /// <summary>
        /// Removes the UnityInput component from the character.
        /// </summary>
        /// <param name="character">The character to remove the UnityInput component from.</param>
        public static void RemoveUnityInput(GameObject character)
        {
            var inputProxy = character.GetComponent<Shared.Input.PlayerInputProxy>();
            if (inputProxy != null) {
                Object.DestroyImmediate(inputProxy, true);
            }

            var unityInput = character.GetComponentInChildren<Shared.Input.UnityInput>();
            if (unityInput != null) {
                Object.DestroyImmediate(unityInput.gameObject, true);
            }
        }

#if FIRST_PERSON_CONTROLLER
        /// <summary>
        /// Adds the first person objects to the character.
        /// </summary>
        /// <param name="character">The character to add the first person objects to.</param>
        public static void AddFirstPersonObjects(GameObject character)
        {
            var firstPersonObjects = character.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
            if (firstPersonObjects == null) {
                var firstPersonObjectsGameObject = new GameObject("FirstPersonObjects");
                firstPersonObjectsGameObject.transform.SetParentOrigin(character.transform);
                firstPersonObjectsGameObject.AddComponent<FirstPersonController.Character.FirstPersonObjects>();
            }
        }

        /// <summary>
        /// Removes the first person obejcts from the character.
        /// </summary>
        /// <param name="character">The character to add the first person objects from.</param>
        public static void RemoveFirstPersonObjects(GameObject character)
        {
            var firstPersonObjects = character.GetComponentsInChildren<FirstPersonController.Character.FirstPersonObjects>();
            if (firstPersonObjects != null) {
                for (int i = firstPersonObjects.Length - 1; i >= 0; --i) {
                    Object.DestroyImmediate(firstPersonObjects[i].gameObject, true);
                }
            }
        }
#endif

        /// <summary>
        /// Adds support for items to the character.
        /// </summary>
        /// <param name="character">The character to add support for items to.</param>
        /// <param name="itemCollection">A reference to the inventory's ItemCollection.</param>
        /// <param name="itemSetRule">A reference to the inventory's ItemSetRule.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        /// <param name="firstPersonItems">Does the character support first person items?</param>
        public static void AddItemSupport(GameObject character, ItemCollection itemCollection, ItemSetRuleBase itemSetRule, bool aiAgent, bool firstPersonItems)
        {
            // Even if the character doesn't have an animator the items may make use of one.
            if (character.GetComponentInChildren<AnimatorMonitor>(true) == null) {
                character.AddComponent<AnimatorMonitor>();
            }

            if (character.GetComponentInChildren<Items.ItemPlacement>(true) == null) {
                var items = new GameObject("Items");
                items.transform.SetParentOrigin(character.transform);
                items.AddComponent<Items.ItemPlacement>();
            }

            var animator = character.GetComponent<Animator>();
            if (animator != null) {
                var head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null) {
                    var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    if (leftHand != null && rightHand != null) {
                        ItemBuilder.AddItemSlot(leftHand.gameObject, 1);
                        ItemBuilder.AddItemSlot(rightHand.gameObject, 0);
                    }
                }
            }

            // Items use the inventory for equip/unequip.
            if (character.GetComponent<Inventory>() == null) {
                character.AddComponent<Inventory>();
            }

            ItemSetManager itemSetManager;
            if ((itemSetManager = character.GetComponent<ItemSetManager>()) == null) {
                itemSetManager = character.AddComponent<ItemSetManager>();
            }
            itemSetManager.SetItemCollection(itemCollection, true, !Application.isPlaying);

            ItemSetGroup itemSetGroup;
            // At least one ItemSetGroup is required for equip/unequip abilities to work.
            if (itemSetManager.ItemSetGroups == null || itemSetManager.ItemSetGroups.Length == 0) {
                itemSetManager.ItemSetGroups = new ItemSetGroup[1];
                itemSetGroup = new ItemSetGroup();
                itemSetManager.ItemSetGroups[0] = itemSetGroup;
            } else {
                itemSetGroup = itemSetManager.ItemSetGroups[0];
            }

            if (itemSetGroup.SerializedItemCategory == null) {
                // Assign the default category if none are assigned
                itemSetGroup.ItemCategory = itemCollection?.Categories?.Length > 0 ? itemCollection.Categories[0] : null;
            }

            if (itemSetGroup.StartingItemSetRules == null || itemSetGroup.StartingItemSetRules.Length == 0) {
                // Assign the default ItemSetRule if none are assigned
                itemSetGroup.StartingItemSetRules = new ItemSetRuleBase[] { itemSetRule };
            }

            if (!aiAgent && character.GetComponent<ItemHandler>() == null) {
                character.AddComponent<ItemHandler>();
            }

            // Add the Equip, Aim, Use, and Reload item abilities.
            var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.Reload));
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.Use));
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipUnequip));
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.ToggleEquip));
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipNext));
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipPrevious));
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipScroll));
            AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.Aim));
            // The buttons should not use an input related start type.
            if (aiAgent) {
                var itemAbilities = characterLocomotion.GetAbilities<Character.Abilities.Items.ItemAbility>();
                for (int i = 0; i < itemAbilities.Length; ++i) {
                    if (itemAbilities[i].StartType != Ability.AbilityStartType.Automatic &&
                        itemAbilities[i].StartType != Ability.AbilityStartType.Manual) {
                        itemAbilities[i].StartType = Ability.AbilityStartType.Manual;
                    }
                    if (itemAbilities[i].StopType != Ability.AbilityStopType.Automatic &&
                        itemAbilities[i].StopType != Ability.AbilityStopType.Manual) {
                        itemAbilities[i].StopType = Ability.AbilityStopType.Manual;
                    }
                }
            }

            // The ItemEquipVerifier needs to be added after the item abilities.
            AbilityBuilder.AddAbility(characterLocomotion, typeof(ItemEquipVerifier));

            // Reinitialize the ItemCollection after the abilities have been added.
            if (Application.isPlaying) {
                itemSetManager.Initialize(true);
            }

#if FIRST_PERSON_CONTROLLER
            if (firstPersonItems) {
                var animatorMonitors = character.GetComponentsInChildren<AnimatorMonitor>();
                for (int i = 0; i < animatorMonitors.Length; ++i) {
                    AddFirstPersonObjects(animatorMonitors[i].gameObject);
                }
            }
#endif
        }

        /// <summary>
        /// Removes support for items from the character.
        /// </summary>
        /// <param name="character">The character to remove support for the items from.</param>
        public static void RemoveItemSupport(GameObject character)
        {
            var animatorMonitor = character.GetComponent<ItemHandler>();
            if (animatorMonitor != null && character.GetComponent<Animator>() == null) {
                character.AddComponent<Animator>();
            }
            var itemHandler = character.GetComponent<ItemHandler>();
            if (itemHandler != null) {
                Object.DestroyImmediate(itemHandler, true);
            }
            var itemPlacement = character.GetComponentInChildren<Items.ItemPlacement>();
            if (itemPlacement != null) {
                Object.DestroyImmediate(itemPlacement.gameObject, true);
            }
            var itemSlots = character.GetComponentsInChildren<Items.CharacterItemSlot>();
            if (itemSlots != null && itemSlots.Length > 0) {
                for (int i = itemSlots.Length - 1; i >= 0; --i) {
                    Object.DestroyImmediate(itemSlots[i].gameObject, true);
                }
            }
            var inventory = character.GetComponent<Inventory>();
            if (inventory != null) {
                Object.DestroyImmediate(inventory, true);
            }
            var itemSetManager = character.GetComponent<ItemSetManager>();
            if (itemSetManager != null) {
                Object.DestroyImmediate(itemSetManager, true);
            }
            // All of the item abilities should be removed.
            var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            characterLocomotion.ItemAbilities = new Character.Abilities.Items.ItemAbility[0];
            AbilityBuilder.RemoveAbility<ItemEquipVerifier>(characterLocomotion);
#if FIRST_PERSON_CONTROLLER
            // There is no use for the first person objects component.
            RemoveFirstPersonObjects(character);
#endif
        }

        /// <summary>
        /// Adds the health components to the character.
        /// </summary>
        /// <param name="character">The character to add the health components to.</param>
        public static void AddHealth(GameObject character)
        {
            if (character.GetComponent<Traits.CharacterAttributeManager>() == null) {
                character.AddComponent<Traits.CharacterAttributeManager>();
            }

            if (character.GetComponent<Traits.CharacterHealth>() == null) {
                character.AddComponent<Traits.CharacterHealth>();
            }

            if (character.GetComponent<Traits.CharacterRespawner>() == null) {
                character.AddComponent<Traits.CharacterRespawner>();
            }
        }

        /// <summary>
        /// Removes the health components from the character.
        /// </summary>
        /// <param name="character">The character to remove the health components from.</param>
        public static void RemoveHealth(GameObject character)
        {
            var health = character.GetComponent<Traits.CharacterHealth>();
            if (health != null) {
                Object.DestroyImmediate(health, true);
            }

            var attributeManager = character.GetComponent<Traits.CharacterAttributeManager>();
            if (attributeManager != null) {
                Object.DestroyImmediate(attributeManager, true);
            }

            var respawner = character.GetComponent<Traits.CharacterRespawner>();
            if (respawner != null) {
                Object.DestroyImmediate(respawner, true);
            }
        }

        /// <summary>
        /// Adds the CharacterIK component to the character.
        /// </summary>
        /// <param name="character">The character to add the CharacterIK component to.</param>
        public static void AddUnityIK(GameObject character)
        {
            var animatorMonitors = character.GetComponentsInChildren<AnimatorMonitor>();
            for (int i = 0; i < animatorMonitors.Length; ++i) {
                var animator = animatorMonitors[i].GetComponent<Animator>();
                if (animator == null || !animator.isHuman || animator.GetBoneTransform(HumanBodyBones.Head) == null) {
                    continue;
                }

                if (animatorMonitors[i].GetComponent<CharacterIK>() == null) {
                    animatorMonitors[i].gameObject.AddComponent<CharacterIK>();
                }
            }
        }

        /// <summary>
        /// Removes the CharacterIK component from the character.
        /// </summary>
        /// <param name="character">The character to remove the CharacterIK component from.</param>
        public static void RemoveUnityIK(GameObject character)
        {
            var characterIKs = character.GetComponentsInChildren<CharacterIK>();
            for (int i = characterIKs.Length - 1; i >= 0; --i) {
                Object.DestroyImmediate(characterIKs[i], true);
            }
        }

        /// <summary>
        /// Adds the CharacterFootEffects component to the character.
        /// </summary>
        /// <param name="character">The character to add the CharacterFootEffects component to.</param>
        public static void AddFootEffects(GameObject character)
        {
            var animatorMonitors = character.GetComponentsInChildren<AnimatorMonitor>();
            for (int i = 0; i < animatorMonitors.Length; ++i) {
                var animator = animatorMonitors[i].GetComponent<Animator>();
                if (animator == null) {
                    continue;
                }

                if (animatorMonitors[i].GetComponent<CharacterFootEffects>() == null) {
                    var footEffects = animatorMonitors[i].gameObject.AddComponent<CharacterFootEffects>();
                    footEffects.InitializeHumanoidFeet(true);

                    // If the character doesn't have any feet then the foot effects should use the AudioSource on the main character GameObject.
                    if (footEffects.Feet == null || footEffects.Feet.Length == 0) {
                        var audioSource = animatorMonitors[i].gameObject.AddComponent<AudioSource>();
                        audioSource.spatialBlend = 1;
                        audioSource.playOnAwake = false;
                    }
                }
            }
        }

        /// <summary>
        /// Removes the CharacterFootEffects component from the character.
        /// </summary>
        /// <param name="character">The character to remove the CharacterFootEffects component from.</param>
        public static void RemoveFootEffects(GameObject character)
        {
            var footEffects = character.GetComponentsInChildren<CharacterFootEffects>();
            for (int i = footEffects.Length - 1; i >= 0; --i) {
                Object.DestroyImmediate(footEffects[i], true);
            }
        }

        /// <summary>
        /// Adds the specified model to the Model Manager.
        /// </summary>
        /// <param name="character">The character that has the Model Manager.</param>
        /// <param name="model">The model that should be added.</param>
        public static void AddCharacterModel(GameObject character, GameObject model)
        {
            var modelManager = character.GetComponent<ModelManager>();
            if (modelManager == null) {
                modelManager = character.AddComponent<ModelManager>();

                // Populate the initial models. Add the new model as well for an early exit.
                var animatorMonitors = character.GetComponentsInChildren<AnimatorMonitor>();
                modelManager.AvailableModels = new GameObject[animatorMonitors.Length];
                for (int i = 0; i < animatorMonitors.Length; ++i) {
                    modelManager.AvailableModels[i] = animatorMonitors[i].gameObject;
                }

                // The new model should not be first in the list.
                if (modelManager.AvailableModels[0] == model && modelManager.AvailableModels.Length > 1) {
                    modelManager.AvailableModels[0] = modelManager.AvailableModels[1];
                    modelManager.AvailableModels[1] = model;
                }

                return;
            }

            var availableModels = modelManager.AvailableModels;
            var addModel = true;
            if (availableModels != null) {
                for (int i = 0; i < availableModels.Length; ++i) {
                    if (availableModels[i] == model) {
                        addModel = false;
                        break;
                    }
                }
            }

            if (!addModel) {
                return;
            }

            if (availableModels == null) {
                availableModels = new GameObject[1];
            } else {
                System.Array.Resize(ref availableModels, availableModels.Length + 1);
            }
            availableModels[availableModels.Length - 1] = model;
            modelManager.AvailableModels = availableModels;
        }

        /// <summary>
        /// Adds the models to the Model Manager.
        /// </summary>
        /// <param name="character">The character that has the Model Manager.</param>
        /// <param name="models">The models that should be added.</param>
        public static void AddCharacterModels(GameObject character, GameObject[] models)
        {
            var modelManager = character.GetComponent<ModelManager>();
            if (modelManager == null) {
                modelManager = character.AddComponent<ModelManager>();
            }

            modelManager.AvailableModels = models;
        }

        /// <summary>
        /// Removes the specified model from the Model Manager.
        /// </summary>
        /// <param name="character">The character that has the Model Manager.</param>
        /// <param name="model">The model that should be removed.</param>
        public static void RemoveCharacterModel(GameObject character, GameObject model)
        {
            var modelManager = character.GetComponent<ModelManager>();
            if (modelManager == null) {
                return;
            }

            // Remove the model manager when there are no models to switch between.
            if (modelManager.AvailableModels.Length <= 2) {
                Object.DestroyImmediate(modelManager, true);
                return;
            }

            var availableModels = modelManager.AvailableModels;
            var index = -1;
            for (int i = 0; i < availableModels.Length; ++i) {
                if (availableModels[i] == model) {
                    index = i;
                    break;
                }
            }

            if (index == -1) {
                return;
            }

            for (int i = index + 1; i < availableModels.Length; ++i) {
                availableModels[i - 1] = availableModels[i];
            }
            System.Array.Resize(ref availableModels, availableModels.Length - 1);
            modelManager.AvailableModels = availableModels;
        }
    }
}