/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateCharacterController.Character;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// </summary>
    [OrderedEditorItem("Migration", 6)]
    public class MigrationManager : Manager
    {
        [SerializeField] protected GameObject m_Object;

        private Button m_BuildButton;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.marginTop = 5;
            horizontalLayout.style.flexGrow = 0;
            m_ManagerContentContainer.Add(horizontalLayout);

            // UIToolkit does not support links as of Unity 2021.3.
            var startLabel = new Label("<b>IMPORTANT</b>: See ");
            startLabel.enableRichText = true;
            horizontalLayout.Add(startLabel);
            var linkLabel = new Label(string.Format("<color={0}>this page</color>", EditorGUIUtility.isProSkin ? "#00aeff" : "#0000ee"));
            linkLabel.RegisterCallback<ClickEvent>(c =>
            {
                Application.OpenURL("https://opsive.com/support/documentation/ultimate-character-controller/getting-started/version-3-migration-guide/");
            });
            linkLabel.enableRichText = true;
            linkLabel.AddToClassList("hyperlink");
            horizontalLayout.Add(linkLabel);
            var endLabel = new Label("for migration steps.");
            horizontalLayout.Add(endLabel);

            m_BuildButton = ManagerUtility.ShowControlBox("Object Migration", "Migrates your version 2 character, camera, or item to version 3.", ShowMigrationOptions, "Migrate Object", MigrateObject, m_ManagerContentContainer, true);
            UpdateBuildButtonState();
        }

        /// <summary>
        /// Draws the migration objects field.
        /// </summary>
        private void ShowMigrationOptions(VisualElement container)
        {
            container.Clear();

            var objectField = new ObjectField("Object");
            objectField.objectType = typeof(GameObject);
            objectField.allowSceneObjects = true;
            objectField.value = m_Object;
            objectField.RegisterValueChangedCallback(c =>
            {
                m_Object = (GameObject)c.newValue;
                ShowMigrationOptions(container);
            });
            container.Add(objectField);

            // The object must be a character or item.
            if (m_Object != null) {
                if (EditorUtility.IsPersistent(m_Object)) {
                    var helpbox = new HelpBox("The object must exist within the scene.", HelpBoxMessageType.Error);
                    container.Add(helpbox);
                } else if (m_Object.GetComponent<Character.LegacyCharacterLocomotion>() == null && 
                    m_Object.GetComponent<UltimateCharacterController.Camera.LegacyCameraController>() == null && 
                    m_Object.GetComponent<Items.CharacterItem>() == null) {
                    var helpbox = new HelpBox("The object must contain a reference to a version 2 character, camera, or item.", HelpBoxMessageType.Error);
                    container.Add(helpbox);
                }
            }

            UpdateBuildButtonState();
        }

        /// <summary>
        /// Updates the build button enabled state.
        /// </summary>
        private void UpdateBuildButtonState()
        {
            // The build button will be null during the first pass of ShowMigrationOptions.
            if (m_BuildButton == null) {
                return;
            }

            var canBuild = true;
            if (m_Object == null) {
                canBuild = false;
            } else if (EditorUtility.IsPersistent(m_Object)) {
                canBuild = false;
            } else if (m_Object.GetComponent<Character.LegacyCharacterLocomotion>() == null &&
                     m_Object.GetComponent<UltimateCharacterController.Camera.LegacyCameraController>() == null &&
                     m_Object.GetComponent<Items.CharacterItem>() == null) {
                canBuild = false;
            }

            m_BuildButton.SetEnabled(canBuild);
        }

        /// <summary>
        /// Migrates the object.
        /// </summary>
        private void MigrateObject()
        {
            MigrateObject(m_Object);

            Debug.Log(m_Object.name + " has been migrated to version 3!");
            UpdateBuildButtonState();
        }

        /// <summary>
        /// Migrates the specified object.
        /// </summary>
        /// <param name="obj">The object that should be migrated.</param>
        public static void MigrateObject(GameObject obj)
        {
            if (obj.GetComponent<Character.LegacyCharacterLocomotion>()) {
                MigrateCharacter(obj.GetComponent<Character.LegacyCharacterLocomotion>());
            } else if (obj.GetComponent<UltimateCharacterController.Camera.LegacyCameraController>()) {
                MigrateCamera(obj.GetComponent<UltimateCharacterController.Camera.LegacyCameraController>());
            } else if (obj.GetComponent<Items.CharacterItem>()){ // Item.
                MigrateItem(obj.GetComponent<Items.CharacterItem>());
            }
        }

        /// <summary>
        /// Migrates the version 2 character to version 3.
        /// </summary>
        /// <param name="legacyCharacterLocomotion">The version 2 character.</param>
        private static void MigrateCharacter(Character.LegacyCharacterLocomotion legacyCharacterLocomotion)
        {
            var character = legacyCharacterLocomotion.gameObject;

            // Migrate the legacy movement types, abilities, and effects.
            if (legacyCharacterLocomotion.HasMovementTypeData) {
                legacyCharacterLocomotion.MovementTypes = legacyCharacterLocomotion.GetDeserializedMovementTypes();
            }
            if (legacyCharacterLocomotion.HasAbilityData) {
                legacyCharacterLocomotion.Abilities = legacyCharacterLocomotion.GetDeserializedAbilities();
            }
            if (legacyCharacterLocomotion.HasItemAbilityData) {
                legacyCharacterLocomotion.ItemAbilities = legacyCharacterLocomotion.GetDeserializedItemAbilities();
            }
            if (legacyCharacterLocomotion.HasEffectData) {
                legacyCharacterLocomotion.Effects = legacyCharacterLocomotion.GetDeserializedEffects();
            }

            // Replace Legacy Character Locomotion with Ultimate Character Locomotion.
            var legacySerializedObject = new SerializedObject(legacyCharacterLocomotion);
            var legacyScriptProperty = legacySerializedObject.FindProperty("m_Script");
            legacySerializedObject.Update();

            var tempGameObject = new GameObject();
            var characterLocomotion = tempGameObject.AddComponent<Character.UltimateCharacterLocomotion>();
            legacyScriptProperty.objectReferenceValue = MonoScript.FromMonoBehaviour(characterLocomotion);
            legacySerializedObject.ApplyModifiedProperties();
            Object.DestroyImmediate(tempGameObject, true);

            var characterRigidbody = character.GetComponent<Rigidbody>();
            characterRigidbody.constraints = RigidbodyConstraints.None;

            var legacyPlayerInput = character.GetComponent<Shared.Input.PlayerInput>();
            if (legacyPlayerInput != null) {
                var inputGameObject = new GameObject(character.name + "Input");
                inputGameObject.transform.parent = character.transform;
                var playerInput = inputGameObject.AddComponent(legacyPlayerInput.GetType()) as Shared.Input.PlayerInput;
                EditorUtility.CopySerialized(legacyPlayerInput, playerInput);
                Object.DestroyImmediate(legacyPlayerInput, true);

                var proxy = character.AddComponent<Shared.Input.PlayerInputProxy>();
                proxy.PlayerInput = playerInput;
            }

            EditorUtility.SetDirty(character);

            // The model and animator controller move to being a child of the main character.
            var legacyAnimator = character.GetComponent<Animator>();
            if (legacyAnimator != null) {
                var modelGameObject = new GameObject(character.name);
                modelGameObject.transform.parent = character.transform;
                modelGameObject.transform.localPosition = Vector3.zero;
                modelGameObject.transform.localRotation = Quaternion.identity;
                var animator = modelGameObject.AddComponent<Animator>();
                EditorUtility.CopySerialized(legacyAnimator, animator);
                Object.DestroyImmediate(legacyAnimator, true);

                var legacyAnimatorMonitor = character.GetComponent<AnimatorMonitor>();
                var animatorMonitor = modelGameObject.AddComponent<AnimatorMonitor>();
                EditorUtility.CopySerialized(legacyAnimatorMonitor, animatorMonitor);
                Object.DestroyImmediate(legacyAnimatorMonitor, true);
                var characterAnimator = animatorMonitor.GetComponent<Animator>();
                if (characterAnimator != null) {
                    characterAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
                    EditorUtility.SetDirty(characterAnimator);
                }

                var legacyCharacterIK = character.GetComponent<CharacterIK>();
                if (legacyCharacterIK != null) {
                    var characterIK = modelGameObject.AddComponent<CharacterIK>();
                    EditorUtility.CopySerialized(legacyCharacterIK, characterIK);
                    Object.DestroyImmediate(legacyCharacterIK, true);
                }

                if (PrefabUtility.IsPartOfAnyPrefab(character)) {
                    PrefabUtility.UnpackPrefabInstance(character, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                // Loop through the direct children to determine if they should be a child of the character.
                for (int i = character.transform.childCount - 1; i >= 0; --i) {
                    var child = character.transform.GetChild(i);
                    if (child == modelGameObject.transform) {
                        continue;
                    }

                    // Ensure the items are not moved.
                    if (child.GetComponent<Items.ItemPlacement>()) {
                        continue;
                    }

                    var moveGameObject = false;
                    if (child.gameObject.GetComponentInChildren<CapsuleColliderPositioner>(true) || 
                        child.gameObject.GetComponent<Character.Identifiers.CharacterColliderBaseIdentifier>()) {
                        moveGameObject = true;
                    }
#if FIRST_PERSON_CONTROLLER
                    else if (child.GetComponent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                        moveGameObject = true;
                    }
#endif
                    else if (child.GetComponentInChildren<Character.Identifiers.ThirdPersonObject>(true) || 
                                child.GetComponentInChildren<Items.CharacterItemSlot>(true) || child.GetComponentInChildren<Renderer>(true)) {
                        moveGameObject = true;
                    }

                    if (moveGameObject) {
                        child.parent = modelGameObject.transform;
                    }
                }
            }
        }

        /// <summary>
        /// Migrates the version 2 camera to version 3.
        /// </summary>
        /// <param name="legacyCameraController">The version 2 camera.</param>
        private static void MigrateCamera(UltimateCharacterController.Camera.LegacyCameraController legacyCameraController)
        {
            var camera = legacyCameraController.gameObject;

            // Migrate the legacy view types.
            if (legacyCameraController.HasViewTypeData) {
                legacyCameraController.ViewTypes = legacyCameraController.GetDeserializedViewTypes();
            }

            // Replace Legacy Camera Controller with the Camera Controller component.
            var legacySerializedObject = new SerializedObject(legacyCameraController);
            var legacyScriptProperty = legacySerializedObject.FindProperty("m_Script");
            legacySerializedObject.Update();

            var tempGameObject = new GameObject();
            var cameraController = tempGameObject.AddComponent<UltimateCharacterController.Camera.CameraController>();
            legacyScriptProperty.objectReferenceValue = MonoScript.FromMonoBehaviour(cameraController);
            legacySerializedObject.ApplyModifiedProperties();
            Object.DestroyImmediate(tempGameObject, true);

            EditorUtility.SetDirty(camera);
        }

        /// <summary>
        /// Migrates the version 2 item to version 3.
        /// </summary>
        /// <param name="item">The version 2 item.</param>
        private static void MigrateItem(Items.CharacterItem item)
        {
            // The perspective components have been removed.
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(item.gameObject);

            // Setup default values for those that exist.
            UltimateCharacterController.Utility.Builders.ItemBuilder.AddPropertiesToActions(item.gameObject);
        }
    }
}