/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Draws the Character Builder settings within the window.
    /// </summary>
    [Serializable]
    [OrderedEditorItem("Character", 2)]
    public class CharacterManager : Manager
    {
        /// <summary>
        /// Specifies the perspective that the character can change into.
        /// </summary>
        private enum Perspective
        {
            First,  // The character can only be in first person perspective.
            Third,  // The character can only be in third person perspective.
            Both,   // The character can be in first or third person perspective.
        }
        /// <summary>
        /// Specifies if the rig is a humanoid or generic. Humanoids allow for animation retargetting whereas generic characters do not.
        /// </summary>
        private enum ModelType { Humanoid, Generic }

        [SerializeField] private GameObject m_Character;
        [SerializeField] private Perspective m_Perspective;
        [SerializeField] private int m_FirstPersonMovementTypeIndex;
        [SerializeField] private int m_ThirdPersonMovementTypeIndex;
        [SerializeField] private bool m_StartFirstPersonPerspective = true;
        [SerializeField] private bool m_Animator = true;
        [SerializeField] private GameObject[] m_CharacterModels;
        [SerializeField] private ModelType[] m_ModelTypes;
        [SerializeField] private RuntimeAnimatorController[] m_AnimatorControllers;
        [SerializeField] private GameObject[][] m_FirstPersonArms;
        [SerializeField] private RuntimeAnimatorController[][] m_FirstPersonArmsAnimatorController;
        [SerializeField] private GameObject[][] m_ThirdPersonObjects;
        [SerializeField] private GameObject[][][] m_ItemSlotParents;
        [SerializeField] private int[][][] m_ItemSlotParentIDs;

        [SerializeField] private GameObject m_TemplateCharacter;
        [SerializeField] private bool m_CopyComponents = true;
        [SerializeField] private bool m_CopyAbilities = true;
        [SerializeField] private bool m_CopyItemAbilities = true;
        [SerializeField] private bool m_CopyEffects = true;
        [SerializeField] private bool m_CopyItems = true;

        [SerializeField] private bool m_StandardAbilities = true;
        [SerializeField] private bool m_AIAgent = false;
        [SerializeField] private bool m_NavMeshAgent = false;
        [SerializeField] private bool m_Items = true;
        [SerializeField] private Inventory.ItemCollection m_ItemCollection;
        [SerializeField] private Inventory.ItemSetRuleBase m_ItemSetRule;
        [SerializeField] private bool m_Health = true;
        [SerializeField] private bool m_UnityIK = true;
        [SerializeField] private bool m_FootEffects = true;
        [SerializeField] private bool m_Ragdoll = true;

        [SerializeField] private GameObject[] m_OriginalCharacterModels;
        [SerializeField] private GameObject[][] m_OriginalFirstPersonArms;
        [SerializeField] private GameObject[][] m_OriginalThirdPersonObjects;
        [SerializeField] private GameObject[][][] m_OriginalItemSlotParents;

        private List<Type> m_FirstPersonMovementTypes = new List<Type>();
        private List<string> m_FirstPersonMovementTypeStrings = new List<string>();
        private List<Type> m_ThirdPersonMovementTypes = new List<Type>();
        private List<string> m_ThirdPersonMovementTypeStrings = new List<string>();
        private List<string> m_PerspectiveNames = new List<string>() { "First", "Third", "Both" };
        private Material m_InvisibleShadowCaster;

        private VisualElement m_CharacterContainer;
        private CharacterItemSlotWindow m_CharacterItemSlotWindow;
        private ReferenceResolverWindow m_ReferenceResolverWindow;

        /// <summary>
        /// Initializes the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            // Set the default perspective based on what asset is installed.
#if FIRST_PERSON_CONTROLLER
            m_Perspective = Perspective.First;
#elif THIRD_PERSON_CONTROLLER
            m_Perspective = Perspective.Third;
#endif

            // Get a list of the available movement types.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from MovementType.
                    if (!typeof(Character.MovementTypes.MovementType).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    var fullName = assemblyTypes[j].FullName;
                    if (fullName != null && fullName.Contains("FirstPersonController")) {
                        m_FirstPersonMovementTypes.Add(assemblyTypes[j]);
                    } else if (assemblyTypes[j].FullName.Contains("ThirdPersonController")) {
                        m_ThirdPersonMovementTypes.Add(assemblyTypes[j]);
                    }
                }
            }

            // Create an array of display names for the popup.
            for (int i = 0; i < m_FirstPersonMovementTypes.Count; ++i) {
                m_FirstPersonMovementTypeStrings.Add(InspectorUtility.DisplayTypeName(m_FirstPersonMovementTypes[i], true));
            }
            for (int i = 0; i < m_ThirdPersonMovementTypes.Count; ++i) {
                m_ThirdPersonMovementTypeStrings.Add(InspectorUtility.DisplayTypeName(m_ThirdPersonMovementTypes[i], true));
            }

            m_CharacterItemSlotWindow = EditorWindow.FindObjectOfType<CharacterItemSlotWindow>();
            if (m_CharacterItemSlotWindow != null) {
                m_CharacterItemSlotWindow.Close();
            }
            m_ReferenceResolverWindow = EditorWindow.FindObjectOfType<ReferenceResolverWindow>();
            if (m_ReferenceResolverWindow != null) {
                m_ReferenceResolverWindow.Close();
            }

            CharacterGameObjectChange();
        }

        /// <summary>
        /// Refreshes the content for the current manager.
        /// </summary>
        public override void Refresh()
        {
            m_ItemCollection = ManagerUtility.FindItemCollection(m_MainManagerWindow);
            m_ItemSetRule = ManagerUtility.FindItemSetRule(m_MainManagerWindow);
            m_InvisibleShadowCaster = ManagerUtility.FindInvisibleShadowCaster(m_MainManagerWindow);

            if (m_CharacterContainer != null) {
                ShowCharacter(m_CharacterContainer);
            }
        }

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            m_CharacterContainer = new VisualElement();
            m_ManagerContentContainer.Add(m_CharacterContainer);
            ShowCharacter(m_CharacterContainer);
        }

        /// <summary>
        /// Shows the new character options.
        /// </summary>
        private void ShowCharacter(VisualElement container)
        {
            container.Clear();

            var scrollView = new ScrollView();
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            container.Add(scrollView);

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.marginTop = 5;
            horizontalLayout.style.marginBottom = 2;
            scrollView.Add(horizontalLayout);

            // UIToolkit does not support links as of Unity 2021.3.
            var startConfigLabel = new Label("See ");
            horizontalLayout.Add(startConfigLabel);
            var linkConfigLabel = new Label(string.Format("<color={0}>this page</color>", EditorGUIUtility.isProSkin ? "#00aeff" : "#0000ee"));
            linkConfigLabel.RegisterCallback<ClickEvent>(c =>
            {
                Application.OpenURL("https://opsive.com/support/documentation/ultimate-character-controller/character/character-creation/common-setups/");
            });
            linkConfigLabel.enableRichText = true;
            linkConfigLabel.AddToClassList("hyperlink");
            horizontalLayout.Add(linkConfigLabel);
            var endConfigLabel = new Label("for example configurations.");
            horizontalLayout.Add(endConfigLabel);

            var canBuild = true;
            ShowCharacterField(container, scrollView, canBuild);
            if (canBuild) {
                canBuild = CheckValidCharacter(scrollView, m_Character) && canBuild;
            }

            canBuild = ShowPerspective(container, scrollView) && canBuild;
            if (canBuild) {
                var firstPersonMovementTypeField = new PopupField<string>("First Person Movement", m_FirstPersonMovementTypeStrings, m_FirstPersonMovementTypeIndex);
                firstPersonMovementTypeField.style.display = m_Perspective == Perspective.First || m_Perspective == Perspective.Both ? DisplayStyle.Flex : DisplayStyle.None;
                firstPersonMovementTypeField.RegisterValueChangedCallback(c =>
                {
                    m_FirstPersonMovementTypeIndex = firstPersonMovementTypeField.index;
                });
                scrollView.Add(firstPersonMovementTypeField);

                var thirdPersonMovementTypeField = new PopupField<string>("Third Person Movement", m_ThirdPersonMovementTypeStrings, m_ThirdPersonMovementTypeIndex);
                thirdPersonMovementTypeField.style.display = m_Perspective == Perspective.Third || m_Perspective == Perspective.Both ? DisplayStyle.Flex : DisplayStyle.None;
                thirdPersonMovementTypeField.RegisterValueChangedCallback(c =>
                {
                    m_ThirdPersonMovementTypeIndex = thirdPersonMovementTypeField.index;
                });
                scrollView.Add(thirdPersonMovementTypeField);
            }

            canBuild = ShowAnimator(container, scrollView, canBuild && m_Perspective == Perspective.First && m_Character == null, canBuild) && canBuild;

            if (canBuild) {
                var modelContainer = new VisualElement();
                canBuild = ShowModelOptions(container, modelContainer, canBuild);
                scrollView.Add(modelContainer);
            }

            // If the character is null then ShowModelOptions will not have a chance to draw the first person options.
            if (m_Character == null || m_CharacterModels == null) {
                canBuild = ShowFirstPerson(container, scrollView, canBuild, 0);
                canBuild = ShowItemSlots(container, scrollView, canBuild, 0) && canBuild;
            }

            // The manager can use an already created character.
            var templateCharacterField = new ObjectField("Template Character");
            templateCharacterField.value = m_TemplateCharacter;
            templateCharacterField.objectType = typeof(GameObject);
            templateCharacterField.RegisterValueChangedCallback(c =>
            {
                m_TemplateCharacter = (GameObject)c.newValue;
                ShowCharacter(container);
            });
            templateCharacterField.style.marginTop = 8;
            templateCharacterField.SetEnabled(canBuild);
            scrollView.Add(templateCharacterField);

            // Separate out the functionality and template options.
            var optionsContainer = new VisualElement();
            optionsContainer.SetEnabled(canBuild);
            optionsContainer.style.marginTop = 4;

            if (m_TemplateCharacter != null) {
                if (m_TemplateCharacter.GetComponent<UltimateCharacterLocomotion>() == null) {
                    if (canBuild) {
                        scrollView.Add(new HelpBox("The template character must be an existing character.", HelpBoxMessageType.Error));
                    }
                    canBuild = false;
                } else if (m_TemplateCharacter == m_Character) {
                    if (canBuild) {
                        scrollView.Add(new HelpBox("The template character cannot be the same as the character.", HelpBoxMessageType.Error));
                    }
                    canBuild = false;
                } else if (m_TemplateCharacter.GetComponent<LegacyCharacterLocomotion>() != null) {
                    if (canBuild) {
                        scrollView.Add(new HelpBox("The template character cannot a legacy character.", HelpBoxMessageType.Error));
                    }
                    canBuild = false;
                }

                optionsContainer.SetEnabled(canBuild);
                scrollView.Add(optionsContainer);
                ShowTemplateOptions(optionsContainer);
            } else {
                scrollView.Add(optionsContainer);
                canBuild = ShowFunctionalityOptions(container, optionsContainer, canBuild) && canBuild;
            }

            // All of the options have been shown. Allow the character to be created or updated.
            var actionButton = new Button();
            var newCharacter = m_Character == null || m_Character.GetComponent<UltimateCharacterLocomotion>() == null;
            actionButton.text = newCharacter ? "Build Character" : "Update Character";
            actionButton.SetEnabled(canBuild);
            scrollView.Add(actionButton);
            actionButton.clicked += () =>
            {
                if (m_CharacterItemSlotWindow != null) {
                    m_CharacterItemSlotWindow.Close();
                    m_CharacterItemSlotWindow = null;
                }
                if (m_ReferenceResolverWindow != null) {
                    m_ReferenceResolverWindow.Close();
                    m_ReferenceResolverWindow = null;
                }
                var newCharacter = m_Character == null || m_Character.GetComponent<UltimateCharacterLocomotion>() == null;
                if (newCharacter) {
                    BuildCharacter(container);
                } else {
                    UpdateCharacter(container);
                }
                if (m_TemplateCharacter != null) {
                    CopyTemplateCharacter();
                }
            };
        }

        /// <summary>
        /// Shows the perspective options.
        /// </summary>
        private bool ShowPerspective(VisualElement baseContainer, VisualElement container)
        {
            var perspectiveField = new PopupField<string>("Perspective", m_PerspectiveNames, (int)m_Perspective);
            perspectiveField.RegisterValueChangedCallback(c =>
            {
                m_Perspective = (Perspective)perspectiveField.index;
                if (m_Perspective != Perspective.First) {
                    m_Animator = true;
                }
                if (m_CharacterItemSlotWindow != null) {
                    m_CharacterItemSlotWindow.Close();
                    m_CharacterItemSlotWindow = null;
                }
                if (m_ReferenceResolverWindow != null) {
                    m_ReferenceResolverWindow.Close();
                    m_ReferenceResolverWindow = null;
                }
                ShowCharacter(baseContainer);
            });
            container.Add(perspectiveField);

            // Determine if the selected perspective is supported.
            var perspective = m_Perspective;
#if !FIRST_PERSON_CONTROLLER
            if (perspective == Perspective.First || perspective == Perspective.Both) {
                var helpBox = new HelpBox("Unable to select the first person perspective. If you'd like to create a first person character ensure the First Person Controller is imported.", HelpBoxMessageType.Error);
                container.Add(helpBox);
                return false;
            }
#endif
#if !THIRD_PERSON_CONTROLLER
            if (perspective == Perspective.Third || perspective == Perspective.Both) {
                var helpBox = new HelpBox("Unable to select the third person perspective. If you'd like to create a third person character ensure the Third Person Controller is imported.", HelpBoxMessageType.Error);
                container.Add(helpBox);
                return false;
            }
#endif
            return true;
        }

        /// <summary>
        /// Shows the character options.
        /// </summary>
        private void ShowCharacterField(VisualElement baseContainer, VisualElement container, bool canBuild)
        {
            var characterField = new ObjectField("Character");
            characterField.objectType = typeof(GameObject);
            characterField.allowSceneObjects = true;
            characterField.value = m_Character;
            characterField.RegisterValueChangedCallback(c =>
            {
                m_Character = (GameObject)c.newValue;

                CharacterGameObjectChange();
                ShowCharacter(baseContainer);
            });

            characterField.SetEnabled(canBuild);
            container.Add(characterField);
        }

        /// <summary>
        /// The character field has changed.
        /// </summary>
        private void CharacterGameObjectChange()
        {
            var newCharacter = m_Character == null || m_Character.GetComponent<UltimateCharacterLocomotion>() == null;
            if (newCharacter) {
                var animator = m_Character?.GetComponent<Animator>();
                var animatorController = animator != null ? animator.runtimeAnimatorController : null;
                if (IsValidHumanoid(m_Character)) {
                    m_ModelTypes = new ModelType[] { ModelType.Humanoid };
                    m_AnimatorControllers = new RuntimeAnimatorController[] { animatorController != null ? animatorController : ManagerUtility.FindAnimatorController(m_MainManagerWindow) };
                    m_UnityIK = m_Ragdoll = true;
                } else {
                    m_ModelTypes = new ModelType[] { ModelType.Generic };
                    m_AnimatorControllers = new RuntimeAnimatorController[] { animatorController };
                    m_UnityIK = m_Ragdoll = false;
                }

                m_CharacterModels = m_Character != null ? new GameObject[] { m_Character } : null;
                m_OriginalCharacterModels = null;
                m_OriginalFirstPersonArms = m_OriginalThirdPersonObjects = m_FirstPersonArms = m_ThirdPersonObjects = null;
                m_FirstPersonArmsAnimatorController = null;
                m_OriginalItemSlotParents = null;
                m_ItemSlotParents = null;
                m_ItemSlotParentIDs = null;
            } else {
                m_AIAgent = IsAIAgent(m_Character);
                m_NavMeshAgent = HasNavMeshAgent(m_Character);
                m_Perspective = CurrentPerspective();
                m_Animator = HasAnimator(m_Character);
                var animatorMonitors = m_Character.GetComponentsInChildren<AnimatorMonitor>();
                var modelCount = animatorMonitors.Length > 0 ? animatorMonitors.Length : 1; // First person may not have a full body.
                m_OriginalCharacterModels = new GameObject[modelCount];
                if (m_Animator) {
                    m_AnimatorControllers = new RuntimeAnimatorController[animatorMonitors.Length];
                    m_ModelTypes = new ModelType[animatorMonitors.Length];
                    m_CharacterModels = new GameObject[animatorMonitors.Length];
                    var modelManager = m_Character.GetComponent<ModelManager>();
                    for (int i = 0; i < animatorMonitors.Length; ++i) {
                        var animator = animatorMonitors[i].GetComponent<Animator>();
                        m_AnimatorControllers[i] = animator.runtimeAnimatorController;
                        m_ModelTypes[i] = IsValidHumanoid(animator) ? ModelType.Humanoid : ModelType.Generic;
                        m_OriginalCharacterModels[i] = m_CharacterModels[i] = modelManager != null ? modelManager.AvailableModels[i] : animatorMonitors[i].gameObject;
                    }
                } else {
                    m_CharacterModels = null;
                }

#if FIRST_PERSON_CONTROLLER
                m_OriginalFirstPersonArms = new GameObject[modelCount][];
                m_FirstPersonArms = new GameObject[modelCount][];
                m_FirstPersonArmsAnimatorController = new RuntimeAnimatorController[modelCount][];
#endif
                m_OriginalThirdPersonObjects = new GameObject[modelCount][];
                m_ThirdPersonObjects = new GameObject[modelCount][];
                m_OriginalItemSlotParents = new GameObject[modelCount][][];
                m_ItemSlotParents = new GameObject[modelCount][][];
                m_ItemSlotParentIDs = new int[modelCount][][];

                for (int i = 0; i < modelCount; ++i) {
                    var characterModel = m_CharacterModels != null && m_CharacterModels[i] != null ? m_CharacterModels[i] : m_Character;
                    var firstPersonBaseCount = 0;
#if FIRST_PERSON_CONTROLLER
                    // Find any existing first person arms.
                    var firstPersonBaseObjects = characterModel.GetComponentsInChildren<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
                    firstPersonBaseCount = firstPersonBaseObjects.Length;
                    var existingFirstPersonArms = new List<GameObject>();
                    m_FirstPersonArmsAnimatorController[i] = new RuntimeAnimatorController[firstPersonBaseObjects.Length];
                    for (int j = 0; j < firstPersonBaseObjects.Length; ++j) {
                        existingFirstPersonArms.Add(firstPersonBaseObjects[j].gameObject);
                        var animator = firstPersonBaseObjects[j].GetComponent<Animator>();
                        if (animator != null) {
                            m_FirstPersonArmsAnimatorController[i][j] = animator.runtimeAnimatorController;
                        }
                    }
                    m_FirstPersonArms[i] = existingFirstPersonArms.ToArray();
                    m_OriginalFirstPersonArms[i] = existingFirstPersonArms.ToArray();
#endif

                    // Search for any existing first person hidden objects.
                    var renderers = characterModel.GetComponentsInChildren<Renderer>();
                    var existingthirdPersonObjects = new List<GameObject>();
                    for (int j = 0; j < renderers.Length; ++j) {
                        if (renderers[j].GetComponent<Character.Identifiers.ThirdPersonObject>() != null) {
                            // Items are handled separately.
                            if (renderers[j].GetComponentInParent<Items.CharacterItemSlot>() != null) {
                                continue;
                            }

                            existingthirdPersonObjects.Add(renderers[j].gameObject);
                            continue;
                        }

                        var addGameObject = false;
                        var materials = renderers[j].sharedMaterials;
                        for (int k = 0; k < materials.Length; ++k) {
                            if (materials[k] == m_InvisibleShadowCaster) {
                                addGameObject = true;
                                break;
                            }
                        }
                        if (addGameObject) {
                            existingthirdPersonObjects.Add(renderers[j].gameObject);
                        }
                    }
                    m_ThirdPersonObjects[i] = existingthirdPersonObjects.ToArray();
                    m_OriginalThirdPersonObjects[i] = existingthirdPersonObjects.ToArray();

                    // Find any existing item slots on the model.
                    m_OriginalItemSlotParents[i] = new GameObject[firstPersonBaseCount + 1][];
                    m_ItemSlotParents[i] = new GameObject[firstPersonBaseCount + 1][];
                    m_ItemSlotParentIDs[i] = new int[firstPersonBaseCount + 1][];

                    var itemSlots = characterModel.GetComponentsInChildren<Items.CharacterItemSlot>();
                    m_OriginalItemSlotParents[i][0] = new GameObject[itemSlots.Length];
                    m_ItemSlotParents[i][0] = new GameObject[itemSlots.Length];
                    m_ItemSlotParentIDs[i][0] = new int[itemSlots.Length];
                    var validItemSlots = 0;
                    for (int j = 0; j < itemSlots.Length; ++j) {
#if FIRST_PERSON_CONTROLLER
                        // Do not add any item slots belonging to the first person arms.
                        if (itemSlots[j].GetComponentInParent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                            continue;
                        }
#endif
                        // Use the parent instead of the Item GameObject directly.
                        m_OriginalItemSlotParents[i][0][validItemSlots] = itemSlots[j].transform.parent.gameObject;
                        m_ItemSlotParents[i][0][validItemSlots] = itemSlots[j].transform.parent.gameObject;
                        m_ItemSlotParentIDs[i][0][validItemSlots] = itemSlots[j].ID;
                        validItemSlots++;
                    }

                    // The array may have empty elements at the end due to the first person arms.
                    if (m_OriginalItemSlotParents[i][0].Length != validItemSlots) {
                        Array.Resize(ref m_OriginalItemSlotParents[i][0], validItemSlots);
                        Array.Resize(ref m_ItemSlotParents[i][0], validItemSlots);
                        Array.Resize(ref m_ItemSlotParentIDs[i][0], validItemSlots);
                    }

#if FIRST_PERSON_CONTROLLER
                    // Find any existing slots on the first person arms.
                    if (m_FirstPersonArms != null && m_FirstPersonArms[i] != null) {
                        for (int j = 0; j < m_FirstPersonArms[i].Length; ++j) {
                            if (m_FirstPersonArms[i][j] == null) {
                                continue;
                            }
                            itemSlots = m_FirstPersonArms[i][j].GetComponentsInChildren<Items.CharacterItemSlot>();
                            m_OriginalItemSlotParents[i][j + 1] = new GameObject[itemSlots.Length];
                            m_ItemSlotParents[i][j + 1] = new GameObject[itemSlots.Length];
                            m_ItemSlotParentIDs[i][j + 1] = new int[itemSlots.Length];
                            for (int k = 0; k < itemSlots.Length; ++k) {
                                // Use the parent instead of the Item GameObject directly.
                                m_OriginalItemSlotParents[i][j + 1][k] = itemSlots[k].transform.parent.gameObject;
                                m_ItemSlotParents[i][j + 1][k] = itemSlots[k].transform.parent.gameObject;
                                m_ItemSlotParentIDs[i][j + 1][k] = itemSlots[k].ID;
                            }
                        }
                    }
#endif
                }

                m_Items = HasItems(m_Character, m_AIAgent, m_Perspective != Perspective.Third);
                if (m_Items) {
                    var itemSetManager = m_Character.GetComponent<Inventory.ItemSetManager>();
                    if (itemSetManager != null) {
                        m_ItemCollection = itemSetManager.ItemCollection;

                        if (itemSetManager.ItemSetGroups != null && itemSetManager.ItemSetGroups.Length > 0 &&
                            itemSetManager.ItemSetGroups[0].StartingItemSetRules != null &&
                            itemSetManager.ItemSetGroups[0].StartingItemSetRules.Length > 0) {
                            m_ItemSetRule = itemSetManager.ItemSetGroups[0].StartingItemSetRules[0];
                        }
                    }
                }
                var characterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
                if (characterLocomotion.GetAbility<Character.Abilities.Jump>() != null &&
                    characterLocomotion.GetAbility<Character.Abilities.Fall>() != null &&
                    characterLocomotion.GetAbility<Character.Abilities.MoveTowards>() != null &&
                    characterLocomotion.GetAbility<Character.Abilities.HeightChange>() != null &&
                    characterLocomotion.GetAbility<Character.Abilities.SpeedChange>() != null) {
                    m_StandardAbilities = true;
                } else {
                    m_StandardAbilities = false;
                }
                m_Health = HasHealth(m_Character);
                m_UnityIK = HasUnityIK(m_Character);
                m_FootEffects = HasFootEffects(m_Character);
                m_Ragdoll = HasRagdoll(m_Character);
            }

            if (m_CharacterItemSlotWindow != null) {
                m_CharacterItemSlotWindow.Close();
                m_CharacterItemSlotWindow = null;
            }
            if (m_ReferenceResolverWindow != null) {
                m_ReferenceResolverWindow.Close();
                m_ReferenceResolverWindow = null;
            }
        }

        /// <summary>
        /// Is the character a valid humanoid?
        /// </summary>
        /// <param name="character">The character GameObject to check against.</param>
        /// <returns>True if the character is a valid humanoid.</returns>
        private bool IsValidHumanoid(GameObject character)
        {
            if (character == null) {
                return false;
            }
            var spawnedCharacter = false;
            // The character has to be spawned in order to be able to detect if it is a Humanoid.
            if (AssetDatabase.GetAssetPath(character).Length > 0) {
                character = UnityEngine.Object.Instantiate(character);
                spawnedCharacter = true;
            }
            var animator = character.GetComponent<Animator>();
            var hasAnimator = animator != null;
            if (!hasAnimator) {
                animator = character.AddComponent<Animator>();
            }
            var isHumanoid = IsValidHumanoid(animator);
            // Clean up.
            if (!hasAnimator) {
                UnityEngine.Object.DestroyImmediate(animator, true);
            }
            if (spawnedCharacter) {
                UnityEngine.Object.DestroyImmediate(character, true);
            }
            return isHumanoid;
        }

        /// <summary>
        /// Is the Animator a humanoid Animator?
        /// </summary>
        private bool IsValidHumanoid(Animator animator)
        {
            if (animator == null) {
                return false;
            }

            // A human will have a head.
            var isHumanoid = animator.GetBoneTransform(HumanBodyBones.Head) != null;
            // GetBoneTransform sometimes returns a false negative.
            if (!isHumanoid) {
                isHumanoid = animator.isHuman;
            }
            return isHumanoid;
        }

        /// <summary>
        /// Is the character a valid character?
        /// </summary>
        private bool CheckValidCharacter(VisualElement container, GameObject character)
        {
            if (character == null) {
                if (m_Perspective != Perspective.First) {
                    container.Add(new HelpBox("Select the GameObject which will be used as the character. This object will have the majority of the components added to it.", HelpBoxMessageType.Error));
                    return false;
                }
                return true;
            }

            if (EditorUtility.IsPersistent(character)) {
                container.Add(new HelpBox("Please drag your character into the scene. The Character Manager cannot add components to prefabs.", HelpBoxMessageType.Error));
                return false;
            }

            var characterLocomotion = character.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion != null && characterLocomotion.gameObject != character) {
                container.Add(new HelpBox("A parent character already exists.", HelpBoxMessageType.Error));
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                container.Add(new HelpBox($"The character cannot be {(characterLocomotion == null ? "created" : "updated")} in play mode.", HelpBoxMessageType.Error));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows the animator controls.
        /// </summary>
        private bool ShowAnimator(VisualElement baseContainer, VisualElement container, bool enableUI, bool canShowError)
        {
            var animatorToggle = new Toggle("Animator");
            animatorToggle.value = m_Animator;
            animatorToggle.SetEnabled(enableUI);
            animatorToggle.RegisterValueChangedCallback(c =>
            {
                m_Animator = c.newValue;
                if (!m_Animator) {
                    m_UnityIK = m_Ragdoll = false;
                }
                ShowCharacter(baseContainer);
            });
            container.Add(animatorToggle);
            if (m_Animator && m_Character == null) {
                if (canShowError) {
                    var helpbox = new HelpBox("A rigged character must be specified in order for an Animator to be added.", HelpBoxMessageType.Error);
                    container.Add(helpbox);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows the additional character model options.
        /// </summary>
        private bool ShowModelOptions(VisualElement baseContainer, VisualElement container, bool canBuild)
        {
            container.Clear();

            // No model options if the character is null or has no full body model..
            if (m_Character == null || m_CharacterModels == null) {
                return true;
            }

            for (int i = 0; i < m_CharacterModels.Length + 1; ++i) {
                var horizontalContainer = new VisualElement();
                horizontalContainer.style.marginTop = 8;
                horizontalContainer.AddToClassList("horizontal-layout");
                var index = i;
                var label = new Label("Character Model " + (i + 1));
                label.style.paddingLeft = 3;
                label.AddToClassList("unity-base-field__label");
                horizontalContainer.Add(label);
                var modelField = new ObjectField();
                modelField.AddToClassList("flex-grow");
                modelField.AddToClassList("flex-shrink");
                modelField.objectType = typeof(GameObject);
                modelField.value = (i < m_CharacterModels.Length ? m_CharacterModels[i] : null);
                modelField.SetEnabled(modelField.value == null);
                modelField.RegisterValueChangedCallback(c =>
                {
                    if (m_CharacterModels.Length == 1 && c.newValue == null) {
                        modelField.SetValueWithoutNotify(c.previousValue);
                        return;
                    }
                    if (index == m_CharacterModels.Length) { // New model.
                        // All of the arrays need to increase by one.
                        Array.Resize(ref m_CharacterModels, m_CharacterModels.Length + 1);
                        Array.Resize(ref m_ModelTypes, m_ModelTypes.Length + 1);
                        Array.Resize(ref m_AnimatorControllers, m_AnimatorControllers.Length + 1);
#if FIRST_PERSON_CONTROLLER
                        if (m_OriginalFirstPersonArms != null) {
                            Array.Resize(ref m_OriginalFirstPersonArms, m_OriginalFirstPersonArms.Length + 1);
                            Array.Resize(ref m_OriginalThirdPersonObjects, m_AnimatorControllers.Length + 1);
                            Array.Resize(ref m_OriginalItemSlotParents, m_OriginalItemSlotParents.Length + 1);
                        }
                        if (m_FirstPersonArms != null) {
                            Array.Resize(ref m_FirstPersonArms, m_FirstPersonArms.Length + 1);
                            Array.Resize(ref m_FirstPersonArmsAnimatorController, m_FirstPersonArmsAnimatorController.Length + 1);
                        }
#endif
                        Array.Resize(ref m_ThirdPersonObjects, m_ThirdPersonObjects.Length + 1);
                        Array.Resize(ref m_ItemSlotParents, m_ItemSlotParents.Length + 1);
                        Array.Resize(ref m_ItemSlotParentIDs, m_ItemSlotParentIDs.Length + 1);
                    }
                    m_CharacterModels[index] = (GameObject)c.newValue;

                    // Try to populate the values.
                    if (m_CharacterModels[index] != null) {
                        var animator = m_CharacterModels[index].GetComponent<Animator>();
                        m_ModelTypes[index] = IsValidHumanoid(animator) ? ModelType.Humanoid : ModelType.Generic;
                        if (m_ModelTypes[index] == ModelType.Humanoid) {
                            m_AnimatorControllers[index] = (animator != null && animator.runtimeAnimatorController != null) ? animator.runtimeAnimatorController :
                                                                                                            ManagerUtility.FindAnimatorController(m_MainManagerWindow);
                        } else {
                            m_AnimatorControllers[index] = animator != null ? animator.runtimeAnimatorController : null;
                        }
                    }

                    ShowCharacter(baseContainer);
                });
                horizontalContainer.Add(modelField);

                // At least one model has to exist.
                if (m_CharacterModels.Length > 1 && index > 0 && index < m_CharacterModels.Length) {
                    var removeButton = new Button();
                    removeButton.text = "-";
                    removeButton.clicked += () =>
                    {
                        ArrayUtility.RemoveAt(ref m_CharacterModels, index);
                        ArrayUtility.RemoveAt(ref m_ModelTypes, index);
                        ArrayUtility.RemoveAt(ref m_AnimatorControllers, index);
#if FIRST_PERSON_CONTROLLER
                        if (m_OriginalFirstPersonArms != null) {
                            ArrayUtility.RemoveAt(ref m_OriginalFirstPersonArms, index);
                            ArrayUtility.RemoveAt(ref m_OriginalThirdPersonObjects, index);
                            ArrayUtility.RemoveAt(ref m_OriginalItemSlotParents, index);
                        }
                        ArrayUtility.RemoveAt(ref m_FirstPersonArms, index);
                        ArrayUtility.RemoveAt(ref m_FirstPersonArmsAnimatorController, index);
#endif
                        ArrayUtility.RemoveAt(ref m_ThirdPersonObjects, index);
                        ArrayUtility.RemoveAt(ref m_ItemSlotParents, index);
                        ArrayUtility.RemoveAt(ref m_ItemSlotParentIDs, index);

                        ShowCharacter(baseContainer);
                    };
                    horizontalContainer.Add(removeButton);
                }
                container.Add(horizontalContainer);

                if (canBuild && i > 0 && i < m_CharacterModels.Length && m_CharacterModels[i] != null &&
                                    m_CharacterModels[i] != m_Character && m_CharacterModels[i].GetComponent<UltimateCharacterLocomotion>() != null) {
                    container.Add(new HelpBox("The character model cannot be an already created character.", HelpBoxMessageType.Error));
                    canBuild = false;
                }

                if (i < m_CharacterModels.Length && canBuild) {
                    canBuild = ShowAnimatorOptions(baseContainer, container, canBuild, i);
                    canBuild = ShowFirstPerson(baseContainer, container, canBuild, i) && canBuild;
                    canBuild = ShowItemSlots(baseContainer, container, canBuild, i) && canBuild;
                }
            }

            return canBuild;
        }

        /// <summary>
        /// Shows the options for the animated character.
        /// </summary>
        private bool ShowAnimatorOptions(VisualElement baseContainer, VisualElement container, bool canBuild, int modelIndex)
        {
            if (!m_Animator) {
                return true;
            }

            // Model Type.
            var modelTypePopup = new EnumField("Model Type", m_ModelTypes[modelIndex]);
            modelTypePopup.RegisterValueChangedCallback(c =>
            {
                m_ModelTypes[modelIndex] = (ModelType)c.newValue;
                if (m_ModelTypes[modelIndex] == ModelType.Humanoid) {
                    // Humanoids support retargetting so can use the demo controller.
                    m_AnimatorControllers[modelIndex] = ManagerUtility.FindAnimatorController(m_MainManagerWindow);
                } else {
                    // Generic characters require a custom animator controller.
                    m_AnimatorControllers[modelIndex] = null;
                    m_UnityIK = m_Ragdoll = false;
                }
                ShowCharacter(baseContainer);
            });
            modelTypePopup.style.display = m_Animator ? DisplayStyle.Flex : DisplayStyle.None;
            modelTypePopup.Q<Label>().AddToClassList("indent");
            container.Add(modelTypePopup);

            if (m_Animator && m_ModelTypes[modelIndex] == ModelType.Humanoid && !IsValidHumanoid(m_CharacterModels[modelIndex])) {
                if (canBuild) {
                    var helpbox = new HelpBox("The specified character is not a humanoid.\nHumanoid characters should have the avatar is set to humanoid and have a head bone assigned.", HelpBoxMessageType.Error);
                    container.Add(helpbox);
                }
                return false;
            }

            // Animator Controller.
            var animatorControllerField = new ObjectField("Animator Controller");
            animatorControllerField.objectType = typeof(RuntimeAnimatorController);
            animatorControllerField.value = m_AnimatorControllers[modelIndex];
            animatorControllerField.RegisterValueChangedCallback(c =>
            {
                m_AnimatorControllers[modelIndex] = (RuntimeAnimatorController)c.newValue;
            });
            animatorControllerField.style.display = m_Animator ? DisplayStyle.Flex : DisplayStyle.None;
            animatorControllerField.Q<Label>().AddToClassList("indent");
            container.Add(animatorControllerField);

            return true;
        }

        /// <summary>
        /// Shows the first person options.
        /// </summary>
        private bool ShowFirstPerson(VisualElement baseContainer, VisualElement container, bool canBuild, int modelIndex)
        {
            if (m_Perspective == Perspective.First || m_Perspective == Perspective.Both) {
                // The first person arms are the generic objects only seen while in first person.
                var firstPersonArmsContainer = new VisualElement();
                firstPersonArmsContainer.SetEnabled(canBuild);
                canBuild = ShowFirstPersonArms(baseContainer, firstPersonArmsContainer, modelIndex) && canBuild;
                container.Add(firstPersonArmsContainer);

                // The third person objects are the objects that are hidden while in first person view.
                if (m_Character != null && m_CharacterModels != null) {
                    var thirdPersonObjectsContainer = new VisualElement();
                    thirdPersonObjectsContainer.SetEnabled(canBuild);
                    ShowThirdPersonObjects(thirdPersonObjectsContainer, modelIndex);
                    container.Add(thirdPersonObjectsContainer);
                }
            }
            return canBuild;
        }

        /// <summary>
        /// Shows the first person arms options.
        /// </summary>
        private bool ShowFirstPersonArms(VisualElement baseContainer, VisualElement container, int modelIndex)
        {
            container.Clear();

            if (m_FirstPersonArms == null || m_FirstPersonArms.Length == 0) {
                m_FirstPersonArms = new GameObject[1][];
                m_FirstPersonArmsAnimatorController = new RuntimeAnimatorController[1][];
                m_ItemSlotParents = new GameObject[m_CharacterModels != null ? m_CharacterModels.Length : 0][][];
                m_ItemSlotParentIDs = new int[m_CharacterModels != null ? m_CharacterModels.Length : 0][][];
            }

            if (m_FirstPersonArms[modelIndex] == null) {
                m_FirstPersonArms[modelIndex] = new GameObject[0];
                m_FirstPersonArmsAnimatorController[modelIndex] = new RuntimeAnimatorController[0];
            }

            // The character can have any number of first person arm objects. Use a custom control
            // type to draw the selection.
            for (int i = 0; i < m_FirstPersonArms[modelIndex].Length + 1; ++i) {
                var horizontalContainer = new VisualElement();
                horizontalContainer.AddToClassList("horizontal-layout");
                var index = i;
                var firstPersonArmsField = new ObjectField(i == 0 ? "First Person Arms" : " ");
                firstPersonArmsField.AddToClassList("flex-grow");
                firstPersonArmsField.AddToClassList("flex-shrink");
                if (m_Character != null && m_CharacterModels != null) {
                    firstPersonArmsField.Q<Label>().AddToClassList("indent");
                }
                firstPersonArmsField.objectType = typeof(GameObject);
                firstPersonArmsField.value = i < m_FirstPersonArms[modelIndex].Length ? m_FirstPersonArms[modelIndex][i] : null;
                firstPersonArmsField.RegisterValueChangedCallback(c =>
                {
                    if (index < m_FirstPersonArms[modelIndex].Length) {
                        m_FirstPersonArms[modelIndex][index] = (GameObject)c.newValue;
                        if (m_FirstPersonArms[modelIndex][index] != null && index == m_FirstPersonArms[modelIndex].Length) {
                            Array.Resize(ref m_FirstPersonArms[modelIndex], m_FirstPersonArms[modelIndex].Length + 1);
                            Array.Resize(ref m_FirstPersonArmsAnimatorController[modelIndex], m_FirstPersonArmsAnimatorController[modelIndex].Length + 1);
                            Array.Resize(ref m_ItemSlotParents[modelIndex], m_ItemSlotParents[modelIndex].Length + 1);
                            Array.Resize(ref m_ItemSlotParentIDs[modelIndex], m_ItemSlotParentIDs[modelIndex].Length + 1);
                        }
                    } else { // Existing character. The object doesn't exist yet.
                        Array.Resize(ref m_FirstPersonArms[modelIndex], m_FirstPersonArms[modelIndex].Length + 1);
                        Array.Resize(ref m_FirstPersonArmsAnimatorController[modelIndex], m_FirstPersonArmsAnimatorController[modelIndex].Length + 1);
                        if (m_ItemSlotParents.Length == 0) {
                            Array.Resize(ref m_ItemSlotParents, 1);
                            Array.Resize(ref m_ItemSlotParentIDs, 1);
                        } else {
                            Array.Resize(ref m_ItemSlotParents[modelIndex], m_ItemSlotParents[modelIndex].Length + 1);
                            Array.Resize(ref m_ItemSlotParentIDs[modelIndex], m_ItemSlotParentIDs[modelIndex].Length + 1);
                        }
                        m_FirstPersonArms[modelIndex][index] = (GameObject)c.newValue;
                    }
                    ShowCharacter(baseContainer);

                    if (m_CharacterItemSlotWindow != null) {
                        m_CharacterItemSlotWindow.Close();
                        m_CharacterItemSlotWindow = null;
                    }
                });
                horizontalContainer.Add(firstPersonArmsField);

                var firstPersonArmsAnimatorControllerField = new ObjectField();
                firstPersonArmsAnimatorControllerField.objectType = typeof(RuntimeAnimatorController);
                firstPersonArmsAnimatorControllerField.value = i < m_FirstPersonArmsAnimatorController[modelIndex].Length ? m_FirstPersonArmsAnimatorController[modelIndex][i] : null;
                firstPersonArmsAnimatorControllerField.RegisterValueChangedCallback(c =>
                {
                    m_FirstPersonArmsAnimatorController[modelIndex][index] = (RuntimeAnimatorController)c.newValue;
                });
                firstPersonArmsAnimatorControllerField.style.display = ((i < m_FirstPersonArms[modelIndex].Length && m_FirstPersonArms[modelIndex][index] == null) || i == m_FirstPersonArms[modelIndex].Length) ? DisplayStyle.None : DisplayStyle.Flex;
                firstPersonArmsAnimatorControllerField.style.width = 170;
                horizontalContainer.Add(firstPersonArmsAnimatorControllerField);

                // Any element before the last element can be removed.
                if (index < m_FirstPersonArms[modelIndex].Length) {
                    var removeButton = new Button();
                    removeButton.text = "-";
                    removeButton.clicked += () =>
                    {
                        ArrayUtility.RemoveAt(ref m_FirstPersonArms[modelIndex], index);
                        ArrayUtility.RemoveAt(ref m_FirstPersonArmsAnimatorController[modelIndex], index);
                        ArrayUtility.RemoveAt(ref m_ItemSlotParents[modelIndex], index + (m_CharacterModels != null ? 1 : 0)); // The base character model is index 0.
                        ArrayUtility.RemoveAt(ref m_ItemSlotParentIDs[modelIndex], index + (m_CharacterModels != null ? 1 : 0));
                        ShowCharacter(baseContainer);

                        if (m_CharacterItemSlotWindow != null) {
                            m_CharacterItemSlotWindow.Close();
                            m_CharacterItemSlotWindow = null;
                        }
                    };
                    horizontalContainer.Add(removeButton);
                }

                container.Add(horizontalContainer);
            }

            return true;
        }

        /// <summary>
        /// Shows the third person object options.
        /// </summary>
        private void ShowThirdPersonObjects(VisualElement container, int modelIndex)
        {
            container.Clear();

            if (m_ThirdPersonObjects == null) {
                m_ThirdPersonObjects = new GameObject[1][];
            }
            if (m_ThirdPersonObjects[modelIndex] == null) {
                m_ThirdPersonObjects[modelIndex] = new GameObject[0];
            }
            // The character can have any number of third person objects. Use a custom control
            // type to draw the selection.
            for (int i = 0; i < m_ThirdPersonObjects[modelIndex].Length + 1; ++i) {
                var horizontalContainer = new VisualElement();
                horizontalContainer.AddToClassList("horizontal-layout");
                var index = i;
                var thirdPersonObjectsField = new ObjectField(i == 0 ? "Third Person Objects" : " ");
                thirdPersonObjectsField.AddToClassList("flex-grow");
                thirdPersonObjectsField.AddToClassList("flex-shrink");
                if (m_CharacterModels != null) {
                    thirdPersonObjectsField.Q<Label>().AddToClassList("indent");
                }
                thirdPersonObjectsField.objectType = typeof(GameObject);
                thirdPersonObjectsField.value = (i < m_ThirdPersonObjects[modelIndex].Length ? m_ThirdPersonObjects[modelIndex][i] : null);
                thirdPersonObjectsField.RegisterValueChangedCallback(c =>
                {
                    if (index < m_ThirdPersonObjects[modelIndex].Length) {
                        m_ThirdPersonObjects[modelIndex][index] = (GameObject)c.newValue;
                        if (m_ThirdPersonObjects[modelIndex][index] != null && index == m_ThirdPersonObjects[modelIndex].Length) {
                            Array.Resize(ref m_ThirdPersonObjects[modelIndex], m_ThirdPersonObjects[modelIndex].Length + 1);
                        }
                    } else { // Existing character. The object doesn't exist yet.
                        Array.Resize(ref m_ThirdPersonObjects[modelIndex], m_ThirdPersonObjects[modelIndex].Length + 1);
                        m_ThirdPersonObjects[modelIndex][index] = (GameObject)c.newValue;
                    }
                    ShowThirdPersonObjects(container, modelIndex);
                });
                horizontalContainer.Add(thirdPersonObjectsField);

                if (index < m_ThirdPersonObjects[modelIndex].Length) {
                    var removeButton = new Button();
                    removeButton.text = "-";
                    removeButton.clicked += () =>
                    {
                        ArrayUtility.RemoveAt(ref m_ThirdPersonObjects[modelIndex], index);
                        ShowThirdPersonObjects(container, modelIndex);
                    };
                    horizontalContainer.Add(removeButton);
                }

                container.Add(horizontalContainer);
            }
        }

        /// <summary>
        /// Shows an uneditable list of item slots on the character.
        /// </summary>
        private bool ShowItemSlots(VisualElement baseContainer, VisualElement container, bool canBuild, int modelIndex)
        {
            if (!m_Items) {
                return true;
            }

            if (m_ItemSlotParents == null) {
                m_ItemSlotParents = new GameObject[(m_CharacterModels != null ? m_CharacterModels.Length : 0)][][];
                m_ItemSlotParentIDs = new int[m_ItemSlotParents.Length][][];
            }

            if (m_ItemSlotParents.Length > 0 && m_ItemSlotParents[modelIndex] == null) {
                m_ItemSlotParents[modelIndex] = new GameObject[0][];
                m_ItemSlotParentIDs[modelIndex] = new int[0][];

                var characterModel = m_CharacterModels != null && m_CharacterModels[modelIndex] != null ? m_CharacterModels[modelIndex] : m_Character;
                if (characterModel != null) {
                    var animator = characterModel.GetComponent<Animator>();
                    var hasAnimator = animator != null;
                    if (!hasAnimator) {
                        animator = characterModel.AddComponent<Animator>();
                    }
                    // If the model is humanoid then the left and right hands can automatically be selected.
                    if (IsValidHumanoid(animator)) {
                        var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                        var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        if (leftHand != null && rightHand != null) {
                            m_ItemSlotParents[modelIndex] = new GameObject[1][];
                            m_ItemSlotParentIDs[modelIndex] = new int[1][];
                            m_ItemSlotParents[modelIndex][0] = new GameObject[] { leftHand.gameObject, rightHand.gameObject };
                            m_ItemSlotParentIDs[modelIndex][0] = new int[] { 1, 0 };
                        }
                    }
                    // Clean up.
                    if (!hasAnimator) {
                        UnityEngine.Object.DestroyImmediate(animator, true);
                    }
                }
            }

            var firstSlot = true;
            for (int i = 0; i < (m_ItemSlotParents.Length > 0 && m_ItemSlotParents[modelIndex] != null ? m_ItemSlotParents[modelIndex].Length : 0) + 1; ++i) {
                var validReference = m_ItemSlotParents.Length > modelIndex && i < m_ItemSlotParents[modelIndex].Length && m_ItemSlotParents[modelIndex][i] != null;
                for (int j = 0; j < (validReference ? m_ItemSlotParents[modelIndex][i].Length : 1); ++j) {
                    var value = validReference ? m_ItemSlotParents[modelIndex][i][j] : null;
                    if (value == null && !firstSlot) {
                        continue;
                    }

                    var horizontalContainer = new VisualElement();
                    horizontalContainer.AddToClassList("horizontal-layout");
                    horizontalContainer.SetEnabled(canBuild);
                    container.Add(horizontalContainer);

                    var objectField = new ObjectField(firstSlot ? "Item Slots" : " ");
                    if (m_Character != null && m_CharacterModels != null) {
                        objectField.Q<Label>().AddToClassList("indent");
                    }
                    objectField.objectType = typeof(GameObject);
                    objectField.value = value;
                    // The label should be enabled but the field should be disabled.
                    foreach (var child in objectField.Children()) {
                        if (child.GetType() == typeof(VisualElement)) {
                            child.SetEnabled(false);
                            break;
                        }
                    }
                    objectField.style.flexGrow = 1;
                    horizontalContainer.Add(objectField);

                    if (firstSlot) {
                        var itemSlotsButton = new Button();
                        itemSlotsButton.text = "Adjust Slots";
                        itemSlotsButton.SetEnabled(m_CharacterModels != null || (m_FirstPersonArms != null && m_FirstPersonArms[modelIndex] != null && m_FirstPersonArms[modelIndex].Length > 0));
                        horizontalContainer.Add(itemSlotsButton);
                        itemSlotsButton.clicked += () =>
                        {
                            m_CharacterItemSlotWindow = EditorWindow.GetWindow<CharacterItemSlotWindow>(true, "Character Item Slots");
                            m_CharacterItemSlotWindow.minSize = m_CharacterItemSlotWindow.maxSize = new Vector2(500, 400);
                            var characterModel = m_CharacterModels != null && m_CharacterModels[modelIndex] != null ? m_CharacterModels[modelIndex] : m_Character;
                            m_CharacterItemSlotWindow.Initialize(characterModel, m_FirstPersonArms != null ? m_FirstPersonArms[modelIndex] : null,
                                m_ItemSlotParents[modelIndex],
                                (GameObject[][] slotParents) =>
                                {
                                    m_ItemSlotParents[modelIndex] = slotParents;
                                    ShowCharacter(baseContainer);
                                }, m_ItemSlotParentIDs[modelIndex], (int[][] ids) =>
                                {
                                    m_ItemSlotParentIDs[modelIndex] = ids;
                                    ShowCharacter(baseContainer);
                                });
                        };
                        firstSlot = false;
                    }
                }
            }

            if (m_CharacterItemSlotWindow != null && !m_CharacterItemSlotWindow.CanBuild) {
                var helpBox = new HelpBox("Please fix any errors within the Item Slot Window before continuing.", HelpBoxMessageType.Error);
                container.Add(helpBox);
                canBuild = false;
            }
            return canBuild;
        }

        /// <summary>
        /// Shows the functionality options.
        /// </summary>
        private bool ShowFunctionalityOptions(VisualElement baseContainer, VisualElement container, bool canShowError)
        {
            container.Clear();

            var canBuild = true;

            // Standard Abilities.
            var standardAbilitiesToggle = new Toggle("Standard Abilities");
            standardAbilitiesToggle.value = m_StandardAbilities;
            standardAbilitiesToggle.RegisterValueChangedCallback(c =>
            {
                m_StandardAbilities = c.newValue;
            });
            container.Add(standardAbilitiesToggle);

            // AI Agent.
            var aiAgentToggle = new Toggle("AI Agent");
            aiAgentToggle.value = m_AIAgent;
            aiAgentToggle.RegisterValueChangedCallback(c =>
            {
                m_AIAgent = c.newValue;
            });
            container.Add(aiAgentToggle);

            // NavMeshAgent.
            var navMeshAgentToggle = new Toggle("NavMeshAgent");
            navMeshAgentToggle.value = m_NavMeshAgent;
            navMeshAgentToggle.RegisterValueChangedCallback(c =>
            {
                m_NavMeshAgent = c.newValue;
            });
            container.Add(navMeshAgentToggle);

            // Items.
            var itemsToggle = new Toggle("Items");
            var itemCollectionField = new ObjectField("Item Collection");
            var itemSetRuleField = new ObjectField("Item Set Rule");
            itemsToggle.value = m_Items;
            itemsToggle.RegisterValueChangedCallback(c =>
            {
                m_Items = c.newValue;
                ShowCharacter(baseContainer);
            });
            container.Add(itemsToggle);

            // ItemCollection.
            itemCollectionField.objectType = typeof(Inventory.ItemCollection);
            itemCollectionField.value = m_ItemCollection;
            itemCollectionField.RegisterValueChangedCallback(c =>
            {
                m_ItemCollection = (Inventory.ItemCollection)c.newValue;
                if (m_ItemCollection != null) {
                    EditorPrefs.SetString(ManagerUtility.LastItemCollectionGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_ItemCollection)));
                }
                itemCollectionField.style.display = m_Items ? DisplayStyle.Flex : DisplayStyle.None;
            });
            itemCollectionField.style.display = m_Items ? DisplayStyle.Flex : DisplayStyle.None;
            itemCollectionField.Q<Label>().AddToClassList("indent");
            container.Add(itemCollectionField);
            if (m_Items && m_ItemCollection == null) {
                if (canShowError) {
                    var helpBox = new HelpBox("An ItemCollection needs to be specified for the character to be created.", HelpBoxMessageType.Error);
                    container.Add(helpBox);
                    canShowError = false;
                }
                canBuild = false;
            }

            // ItemSetRule.
            itemSetRuleField.objectType = typeof(Inventory.ItemSetRuleBase);
            itemSetRuleField.value = m_ItemSetRule;
            itemSetRuleField.RegisterValueChangedCallback(c =>
            {
                m_ItemSetRule = (Inventory.ItemSetRuleBase)c.newValue;
                if (m_ItemSetRule != null) {
                    EditorPrefs.SetString(ManagerUtility.LastItemSetRuleGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_ItemSetRule)));
                }
                itemSetRuleField.style.display = m_Items ? DisplayStyle.Flex : DisplayStyle.None;
            });
            itemSetRuleField.style.display = m_Items ? DisplayStyle.Flex : DisplayStyle.None;
            itemSetRuleField.Q<Label>().AddToClassList("indent");
            container.Add(itemSetRuleField);
            if (m_Items && m_ItemSetRule == null) {
                if (canShowError) {
                    var helpBox = new HelpBox("An ItemSetRule needs to be specified for the character to be created or items won't equip as expected.", HelpBoxMessageType.Error);
                    container.Add(helpBox);
                    canShowError = false;
                }
                canBuild = false;
            }

            // Health.
            var healthToggle = new Toggle("Health");
            healthToggle.value = m_Health;
            healthToggle.RegisterValueChangedCallback(c =>
            {
                m_Health = c.newValue;
            });
            container.Add(healthToggle);

            // UnityIK.
            var ikToggle = new Toggle("Unity IK");
            ikToggle.value = m_UnityIK;
            ikToggle.RegisterValueChangedCallback(c =>
            {
                m_UnityIK = c.newValue;
                ShowCharacter(baseContainer);
            });
            container.Add(ikToggle);
            if (m_UnityIK) {
                var count = (m_CharacterModels != null ? m_CharacterModels.Length : 1);
                var validCount = 0;
                for (int i = 0; i < count; ++i) {
                    GameObject model = null;
                    if (m_CharacterModels == null) {
                        model = m_Character;
                    } else {
                        model = m_CharacterModels[i];
                    }

                    if (IsValidHumanoid(model)) {
                        validCount++;
                    }
                }

                if (validCount != count) {
                    if (canShowError) {
                        var message = string.Empty;
                        if (validCount > 0) {
                            message = "The Unity IK component will be used for all humanoid models. Generic models cannot use Unity's IK system.";
                        } else {
                            message = "Unity's IK system requires a humanoid character with an Animator.";
                        }
                        var helpBox = new HelpBox(message, HelpBoxMessageType.Error);
                        container.Add(helpBox);
                        canShowError = false;
                    }
                    canBuild = false;
                }
            }

            // Foot Effects
            var footEffectsToggle = new Toggle("Foot Effects");
            footEffectsToggle.value = m_FootEffects;
            footEffectsToggle.RegisterValueChangedCallback(c =>
            {
                m_FootEffects = c.newValue;
            });
            container.Add(footEffectsToggle);

            // Ragdoll.
            var ragdollToggle = new Toggle("Ragdoll");
            ragdollToggle.value = m_Ragdoll;
            ragdollToggle.RegisterValueChangedCallback(c =>
            {
                m_Ragdoll = c.newValue;
                ShowCharacter(baseContainer);
            });
            container.Add(ragdollToggle);
            if (m_Ragdoll) {
                var count = (m_CharacterModels != null ? m_CharacterModels.Length : 1);
                var validCount = 0;
                for (int i = 0; i < count; ++i) {
                    GameObject model = null;
                    if (m_CharacterModels == null) {
                        model = m_Character;
                    } else {
                        model = m_CharacterModels[i];
                    }

                    if (IsValidHumanoid(model)) {
                        validCount++;
                    }
                }

                if (validCount != count) {
                    if (canShowError) {
                        var message = string.Empty;
                        if (validCount > 0) {
                            message = "Unity's ragdoll system will be added to all humanoid models. Generic models cannot use Unity's ragdoll system.";
                        } else {
                            message = "Unity's ragdoll system requires a humanoid character with an Animator.";
                        }
                        var helpBox = new HelpBox(message, HelpBoxMessageType.Error);
                        container.Add(helpBox);
                        canShowError = false;
                    }
                    canBuild = false;
                }
            }

            return canBuild;
        }

        /// <summary>
        /// Shows the options for copying from a template character.
        /// </summary>
        private void ShowTemplateOptions(VisualElement container)
        {
            container.Clear();

            // Components.
            var componentsToggle = new Toggle("Copy Components");
            componentsToggle.value = m_CopyComponents;
            componentsToggle.RegisterValueChangedCallback(c =>
            {
                m_CopyComponents = c.newValue;
            });
            container.Add(componentsToggle);

            // Abilities.
            var abilitiesToggle = new Toggle("Copy Abilities");
            abilitiesToggle.value = m_CopyAbilities;
            abilitiesToggle.RegisterValueChangedCallback(c =>
            {
                m_CopyAbilities = c.newValue;
            });
            container.Add(abilitiesToggle);

            // Item Abilities.
            var itemAbilitiesToggle = new Toggle("Copy Item Abilities");
            itemAbilitiesToggle.value = m_CopyItemAbilities;
            itemAbilitiesToggle.RegisterValueChangedCallback(c =>
            {
                m_CopyItemAbilities = c.newValue;
            });
            container.Add(itemAbilitiesToggle);

            // Effects.
            var effectsToggle = new Toggle("Copy Effects");
            effectsToggle.value = m_CopyEffects;
            effectsToggle.RegisterValueChangedCallback(c =>
            {
                m_CopyEffects = c.newValue;
            });
            container.Add(effectsToggle);

            // Items.
            var itemsToggle = new Toggle("Copy Items");
            itemsToggle.value = m_CopyAbilities;
            itemsToggle.RegisterValueChangedCallback(c =>
            {
                m_CopyAbilities = c.newValue;
            });
            container.Add(itemsToggle);
        }

        /// <summary>
        /// Builds the new character.
        /// </summary>
        private void BuildCharacter(VisualElement container)
        {
            // The first person perspective allows for null characters.
            if (m_Character == null) {
                m_Character = new GameObject("FirstPersonCharacter");
            }
            if (EditorUtility.IsPersistent(m_Character)) {
                var name = m_Character.name;
                m_Character = (GameObject)PrefabUtility.InstantiatePrefab(m_Character);
                m_Character.name = name;
            }
            if (!m_Character.activeSelf) {
                m_Character.SetActive(true);
            }

            // Arrange the models before adding the character components.
            if (m_CharacterModels != null && m_CharacterModels.Length > 0) {
                // Create a new GameObject to act as the base character.
                m_Character = new GameObject(m_Character.name);
                m_Character.transform.SetPositionAndRotation(m_CharacterModels[0].transform.position, m_CharacterModels[0].transform.rotation);

                for (int i = 0; i < m_CharacterModels.Length; ++i) {
                    if (EditorUtility.IsPersistent(m_CharacterModels[i])) {
                        var name = m_CharacterModels[i].name;
                        m_CharacterModels[i] = (GameObject)PrefabUtility.InstantiatePrefab(m_CharacterModels[i]);
                        m_CharacterModels[i].name = name;
                    }
                    m_CharacterModels[i].transform.SetParentOrigin(m_Character.transform);
                }
            }

            CharacterBuilder.BuildCharacter(m_Character, m_CharacterModels, m_Animator, m_AnimatorControllers,
                                                m_Perspective != Perspective.Third ? m_FirstPersonMovementTypes[m_FirstPersonMovementTypeIndex].FullName : string.Empty,
                                                m_Perspective != Perspective.First ? m_ThirdPersonMovementTypes[m_ThirdPersonMovementTypeIndex].FullName : string.Empty,
                                                m_StartFirstPersonPerspective, m_ThirdPersonObjects, m_InvisibleShadowCaster, m_AIAgent);
            if (m_TemplateCharacter == null) {
                CharacterBuilder.BuildCharacterComponents(m_Character, m_AIAgent, m_Items, m_ItemCollection, m_ItemSetRule, (m_Perspective == Perspective.First || m_Perspective == Perspective.Both) && (m_FirstPersonArms != null && m_FirstPersonArms.Length > 1),
                    m_Health, m_UnityIK, m_FootEffects, m_StandardAbilities, m_NavMeshAgent);
            }

            // Add the new item slots.
            if (m_ItemSlotParents != null && m_Items) {
                for (int i = 0; i < m_ItemSlotParents.Length; ++i) { // Model loop.
                    if (m_ItemSlotParents[i] == null) {
                        continue;
                    }

                    for (int j = 0; j < m_ItemSlotParents[i].Length; ++j) { // Arm loop.
                        if (m_ItemSlotParents[i][j] == null) {
                            continue;
                        }

                        for (int k = 0; k < m_ItemSlotParents[i][j].Length; ++k) { // Slot loop.
                            if (m_ItemSlotParents[i][j][k] == null) {
                                continue;
                            }
                            ItemBuilder.AddItemSlot(m_ItemSlotParents[i][j][k], m_ItemSlotParentIDs[i][j][k]);
                        }
                    }
                }
            }

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonArms != null && m_Perspective != Perspective.Third) {
                for (int i = 0; i < (m_CharacterModels != null ? m_CharacterModels.Length : 1); ++i) {
                    GameObject firstPersonObjectsParent;
                    if (m_CharacterModels == null) {
                        firstPersonObjectsParent = m_Character;
                    } else {
                        firstPersonObjectsParent = m_CharacterModels[i];
                    }
                    CharacterBuilder.AddFirstPersonObjects(firstPersonObjectsParent);
                }

                for (int i = 0; i < m_FirstPersonArms.Length; ++i) {
                    if (m_FirstPersonArms[i] == null) {
                        continue;
                    }

                    var firstPersonObjects = (m_CharacterModels != null ? m_CharacterModels[i] : m_Character).GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
                    for (int j = 0; j < m_FirstPersonArms[i].Length; ++j) {
                        if (m_FirstPersonArms[i][j] == null) {
                            continue;
                        }

                        if (EditorUtility.IsPersistent(m_FirstPersonArms[i][j])) {
                            var name = m_FirstPersonArms[i][j].name;
                            m_FirstPersonArms[i][j] = (GameObject)PrefabUtility.InstantiatePrefab(m_FirstPersonArms[i][j]);
                            m_FirstPersonArms[i][j].name = name;
                        }

                        ItemBuilder.AddFirstPersonArms(m_Character, m_FirstPersonArms[i][j], m_FirstPersonArmsAnimatorController[i][j]);
                        m_FirstPersonArms[i][j].transform.SetParentOrigin(firstPersonObjects.transform);
                    }
                }
            }
#endif
            if (m_Ragdoll && m_TemplateCharacter == null) {
                // Add the ragdoll ability and open Unity's ragdoll builder.
                var characterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.Ragdoll));
                for (int i = 0; i < m_CharacterModels.Length; ++i) {
                    Controls.Types.AbilityDrawers.RagdollInspectorDrawer.AddRagdollColliders(m_CharacterModels[i], true);
                }
            }
            if (m_Animator) {
                // Ensure the Animator Controller has the required parameters.
                for (int i = 0; i < m_AnimatorControllers.Length; ++i) {
                    AnimatorBuilder.AddParameters((UnityEditor.Animations.AnimatorController)m_AnimatorControllers[i]);
                }
            }
            Selection.activeObject = m_Character;

            CharacterGameObjectChange();
            ShowCharacter(container);
        }

        /// <summary>
        /// Updates the existing character.
        /// </summary>
        private void UpdateCharacter(VisualElement container)
        {
            var characterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
            var movementTypes = new List<Character.MovementTypes.MovementType>(characterLocomotion.MovementTypes);
            if (m_Perspective != CurrentPerspective()) {
                if (m_Perspective != Perspective.Both) {
                    for (int i = movementTypes.Count - 1; i >= 0; --i) {
                        if (m_Perspective == Perspective.First && movementTypes[i].GetType().FullName.Contains("ThirdPerson")) {
                            movementTypes.RemoveAt(i);
                        } else if (m_Perspective == Perspective.Third && movementTypes[i].GetType().FullName.Contains("FirstPerson")) {
                            movementTypes.RemoveAt(i);
                        }
                    }
                    characterLocomotion.MovementTypes = movementTypes.ToArray();
                    if (m_Perspective == Perspective.First) {
                        characterLocomotion.FirstPersonMovementTypeFullName = String.Empty;
                    } else if (m_Perspective == Perspective.Third) {
                        characterLocomotion.ThirdPersonMovementTypeFullName = String.Empty;
                    }
                }

#if THIRD_PERSON_CONTROLLER
                // If the perspective was switched from/to the both perspective then the perspective monitor needs to be added or removed.
                var perspectiveMonitor = m_Character.GetComponent<ThirdPersonController.Character.PerspectiveMonitor>();
                if (m_Perspective == Perspective.Both) {
                    if (perspectiveMonitor == null) {
                        m_Character.AddComponent<ThirdPersonController.Character.PerspectiveMonitor>();
                    }
                } else if (perspectiveMonitor != null) {
                    UnityEngine.Object.DestroyImmediate(perspectiveMonitor, true);
                }
#endif
#if FIRST_PERSON_CONTROLLER
                // The First Person Objects component should also be added/removed if the character supports items.
                if (HasItems(m_Character, m_AIAgent, false)) {
                    var firstPersonObjects = m_Character.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
                    if (m_Perspective != Perspective.Third && firstPersonObjects == null) {
                        var firstPersonObjectsGameObject = new GameObject("FirstPersonObjects");
                        firstPersonObjectsGameObject.transform.SetParentOrigin(m_Character.transform);
                        firstPersonObjectsGameObject.AddComponent<FirstPersonController.Character.FirstPersonObjects>();
                    } else if (m_Perspective == Perspective.Third && firstPersonObjects != null) {
                        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Character);
                        if (!string.IsNullOrEmpty(prefabPath)) {
                            PrefabUtility.UnpackPrefabInstance(m_Character, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                        }
                        UnityEngine.Object.DestroyImmediate(firstPersonObjects.gameObject, true);
                        if (!string.IsNullOrEmpty(prefabPath)) {
                            PrefabUtility.SaveAsPrefabAssetAndConnect(m_Character, prefabPath, InteractionMode.AutomatedAction);
                        }
                    }

                    if (m_Perspective == Perspective.Third) {
                        var thirdPersonObjects = m_Character.GetComponentsInChildren<Character.Identifiers.ThirdPersonObject>();
                        if (thirdPersonObjects != null) {
                            var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Character);
                            if (!string.IsNullOrEmpty(prefabPath)) {
                                PrefabUtility.UnpackPrefabInstance(m_Character, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                            }
                            for (int i = 0; i < thirdPersonObjects.Length; ++i) {
                                UnityEngine.Object.DestroyImmediate(thirdPersonObjects[i], true);
                            }
                            if (!string.IsNullOrEmpty(prefabPath)) {
                                PrefabUtility.SaveAsPrefabAssetAndConnect(m_Character, prefabPath, InteractionMode.AutomatedAction);
                            }
                        }
                    }
                }
#endif
            }
            // Ensure the character has the movement types specified.
            var hasFirstPersonMovementType = false;
            var hasThirdPersonMovementType = false;
            for (int i = 0; i < movementTypes.Count; ++i) {
#if FIRST_PERSON_CONTROLLER
                if (!hasFirstPersonMovementType && (m_Perspective == Perspective.First || m_Perspective == Perspective.Both)) {
                    if (movementTypes[i].GetType().FullName == m_FirstPersonMovementTypes[m_FirstPersonMovementTypeIndex].FullName) {
                        hasFirstPersonMovementType = true;
                        continue;
                    }
                }
#endif
                if (!hasThirdPersonMovementType && (m_Perspective == Perspective.Third || m_Perspective == Perspective.Both)) {
                    if (movementTypes[i].GetType().FullName == m_ThirdPersonMovementTypes[m_ThirdPersonMovementTypeIndex].FullName) {
                        hasThirdPersonMovementType = true;
                        continue;
                    }
                }
            }
            if (!hasFirstPersonMovementType && (m_Perspective == Perspective.First || m_Perspective == Perspective.Both)) {
                CharacterBuilder.AddMovementType(m_Character, m_FirstPersonMovementTypes[m_FirstPersonMovementTypeIndex].FullName);
            }
            if (!hasThirdPersonMovementType && (m_Perspective == Perspective.Third || m_Perspective == Perspective.Both)) {
                CharacterBuilder.AddMovementType(m_Character, m_ThirdPersonMovementTypes[m_ThirdPersonMovementTypeIndex].FullName);
            }

            if (m_Animator != HasAnimator(m_Character)) {
                if (m_Animator) {
                    CharacterBuilder.AddAnimator(m_CharacterModels, m_AnimatorControllers, m_AIAgent);
                } else {
                    CharacterBuilder.RemoveAnimator(m_Character);
                }
            } else if (m_Animator) {
                // The animator controller may have changed.
                var animatorMonitors = m_Character.GetComponentsInChildren<AnimatorMonitor>(true);
                for (int i = 0; i < animatorMonitors.Length; ++i) {
                    var animator = animatorMonitors[i].GetComponent<Animator>();
                    if (animator == null) {
                        continue;
                    }
                    animator.runtimeAnimatorController = m_AnimatorControllers[i];
                }
            }

            // The character models may have changed.
            var modelChange = (m_OriginalCharacterModels != null && m_CharacterModels != null && m_OriginalCharacterModels.Length != m_CharacterModels.Length) ||
                                            ((m_OriginalCharacterModels == null || m_OriginalCharacterModels.Length == 0) && m_CharacterModels.Length > 0);
            if (!modelChange && m_OriginalCharacterModels != null && m_CharacterModels != null) {
                for (int i = 0; i < m_OriginalCharacterModels.Length; ++i) {
                    if (m_OriginalCharacterModels[i] != m_CharacterModels[i]) {
                        modelChange = true;
                        break;
                    }
                }
            }
            if (modelChange) {
                // Remove models that are no longer used.
                for (int i = m_OriginalCharacterModels.Length - 1; i >= 0; --i) {
                    var validModel = false;
                    for (int j = 0; j < m_CharacterModels.Length; ++j) {
                        if (m_OriginalCharacterModels[i] == m_CharacterModels[j]) {
                            validModel = true;
                            break;
                        }
                    }

                    if (validModel) {
                        continue;
                    }

                    // The model is no longer valid and can be removed.
                    CharacterBuilder.RemoveCharacterModel(m_Character, m_OriginalCharacterModels[i]);
                    var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Character);
                    if (!string.IsNullOrEmpty(prefabPath)) {
                        PrefabUtility.UnpackPrefabInstance(m_Character, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    }
                    UnityEngine.Object.DestroyImmediate(m_OriginalCharacterModels[i], true);
                    if (!string.IsNullOrEmpty(prefabPath)) {
                        PrefabUtility.SaveAsPrefabAssetAndConnect(m_Character, prefabPath, InteractionMode.AutomatedAction);
                    }
                }

                // Add new models.
                for (int i = 0; i < m_CharacterModels.Length; ++i) {
                    var newModel = true;
                    for (int j = 0; j < m_OriginalCharacterModels.Length; ++j) {
                        if (m_CharacterModels[i] == m_OriginalCharacterModels[j]) {
                            newModel = false;
                            break;
                        }
                    }

                    if (!newModel) {
                        continue;
                    }

                    // Setup the new model.
                    if (EditorUtility.IsPersistent(m_CharacterModels[i])) {
                        var name = m_CharacterModels[i].name;
                        m_CharacterModels[i] = (GameObject)PrefabUtility.InstantiatePrefab(m_CharacterModels[i]);
                        m_CharacterModels[i].name = name;
                    }
                    m_CharacterModels[i].transform.SetParentOrigin(m_Character.transform);
                    CharacterBuilder.AddAnimator(m_CharacterModels[i], m_AnimatorControllers[i], m_AIAgent);
                    CharacterBuilder.AddCollider(m_CharacterModels[i]);
                    CharacterBuilder.AddCharacterModel(m_Character, m_CharacterModels[i]);
#if FIRST_PERSON_CONTROLLER
                    if (m_Perspective != Perspective.Third) {
                        CharacterBuilder.AddFirstPersonObjects(m_CharacterModels[i]);
                    }
#endif
                }

                Shared.Editor.Utility.EditorUtility.SetDirty(m_Character);
                Array.Resize(ref m_OriginalCharacterModels, m_CharacterModels.Length);
                m_CharacterModels.CopyTo(m_OriginalCharacterModels, 0);
            }

#if FIRST_PERSON_CONTROLLER
            if (m_FirstPersonArms != null) {
                for (int i = 0; i < m_FirstPersonArms.Length; ++i) {
                    if (m_FirstPersonArms[i] == null) {
                        continue;
                    }

                    // The arms may have changed.
                    var firstPersonArmsChange = ((m_OriginalFirstPersonArms == null || m_OriginalFirstPersonArms[i] == null || m_OriginalFirstPersonArms[i].Length == 0) && m_FirstPersonArms[i].Length > 0) ||
                                                (m_OriginalFirstPersonArms[i] != null && m_OriginalFirstPersonArms[i].Length != m_FirstPersonArms[i].Length);
                    if (!firstPersonArmsChange && m_OriginalFirstPersonArms != null && m_OriginalFirstPersonArms[i] != null) {
                        for (int j = 0; j < m_OriginalFirstPersonArms[i].Length; ++j) {
                            if (m_OriginalFirstPersonArms[i][j] != m_FirstPersonArms[i][j]) {
                                firstPersonArmsChange = true;
                                break;
                            }
                        }
                    }

                    if (firstPersonArmsChange) {
                        // Remove the original arms who are no longer used.
                        if (m_OriginalFirstPersonArms != null && m_OriginalFirstPersonArms[i] != null) {
                            for (int j = 0; j < m_OriginalFirstPersonArms[i].Length; ++j) {
                                if (m_OriginalFirstPersonArms[i][j] == null) {
                                    continue;
                                }
                                var remove = true;
                                for (int k = 0; k < m_FirstPersonArms[i].Length; ++k) {
                                    if (m_OriginalFirstPersonArms[i][j] == m_FirstPersonArms[i][k]) {
                                        remove = false;
                                        break;
                                    }
                                }
                                if (remove) {
                                    var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Character);
                                    if (!string.IsNullOrEmpty(prefabPath)) {
                                        PrefabUtility.UnpackPrefabInstance(m_Character, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                                    }
                                    UnityEngine.Object.DestroyImmediate(m_OriginalFirstPersonArms[i][j], true);
                                    if (!string.IsNullOrEmpty(prefabPath)) {
                                        PrefabUtility.SaveAsPrefabAssetAndConnect(m_Character, prefabPath, InteractionMode.AutomatedAction);
                                    }
                                }
                            }
                        }

                        // Add the new objects.
                        for (int j = 0; j < m_FirstPersonArms[i].Length; ++j) {
                            if (m_FirstPersonArms[i][j] == null) {
                                continue;
                            }
                            var add = true;
                            if (m_OriginalFirstPersonArms != null && m_OriginalFirstPersonArms[i] != null) {
                                for (int k = 0; k < m_OriginalFirstPersonArms[i].Length; ++k) {
                                    if (m_FirstPersonArms[i][j] == m_OriginalFirstPersonArms[i][k]) {
                                        add = false;
                                        break;
                                    }
                                }
                            }

                            if (add) {
                                if (EditorUtility.IsPersistent(m_FirstPersonArms[i][j])) {
                                    var name = m_FirstPersonArms[i][j].name;
                                    m_FirstPersonArms[i][j] = (GameObject)PrefabUtility.InstantiatePrefab(m_FirstPersonArms[i][j]);
                                    m_FirstPersonArms[i][j].name = name;
                                }

                                var parent = (m_CharacterModels != null && m_CharacterModels[i] != null) ? m_CharacterModels[i] : m_Character;
                                ItemBuilder.AddFirstPersonArms(parent, m_FirstPersonArms[i][j], m_FirstPersonArmsAnimatorController[i][j]);
                                m_FirstPersonArms[i][j].transform.SetParentOrigin(parent.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>().transform);
                            }
                        }
                        Shared.Editor.Utility.EditorUtility.SetDirty(m_Character);
                        if (m_OriginalFirstPersonArms == null) {
                            m_OriginalFirstPersonArms = new GameObject[1][];
                        }
                        Array.Resize(ref m_OriginalFirstPersonArms[i], m_FirstPersonArms[i].Length);
                        m_FirstPersonArms[i].CopyTo(m_OriginalFirstPersonArms[i], 0);
                    } else {
                        // Ensure the animator is up to date.
                        if (m_FirstPersonArms[i] != null) {
                            for (int j = 0; j < m_FirstPersonArms[i].Length; ++j) {
                                ItemBuilder.UpdateFirstPersonAnimator(m_FirstPersonArms[i][j], m_FirstPersonArmsAnimatorController[i][j]);
                            }
                        }
                    }
                }
            }
#endif
            if (m_ThirdPersonObjects != null) {
                for (int i = 0; i < m_ThirdPersonObjects.Length; ++i) {
                    if (m_ThirdPersonObjects[i] == null) {
                        continue;
                    }

                    // Determine if the hidden objects have changed.
                    var thirdPersonObjectChange = (m_OriginalThirdPersonObjects[i] != null && m_OriginalThirdPersonObjects[i].Length != m_ThirdPersonObjects[i].Length) ||
                                                ((m_OriginalThirdPersonObjects[i] == null || m_OriginalThirdPersonObjects[i].Length == 0) && m_ThirdPersonObjects[i].Length > 0);
                    if (m_OriginalThirdPersonObjects[i] != null && !thirdPersonObjectChange) {
                        for (int j = 0; j < m_OriginalThirdPersonObjects[i].Length; ++j) {
                            if (m_OriginalThirdPersonObjects[i][j] != m_ThirdPersonObjects[i][j]) {
                                thirdPersonObjectChange = true;
                                break;
                            }
                        }
                    }

                    if (thirdPersonObjectChange) {
                        // Remove all of the original third person objects before adding them back again.
                        if (m_OriginalThirdPersonObjects[i] != null) {
                            for (int j = 0; j < m_OriginalThirdPersonObjects[i].Length; ++j) {
                                if (m_OriginalThirdPersonObjects[i][j] == null) {
                                    continue;
                                }

                                Character.Identifiers.ThirdPersonObject thirdPersonObject;
                                if ((thirdPersonObject = m_OriginalThirdPersonObjects[i][j].GetComponent<Character.Identifiers.ThirdPersonObject>()) != null) {
                                    UnityEngine.Object.DestroyImmediate(thirdPersonObject, true);
                                    continue;
                                }

                                var renderers = m_OriginalThirdPersonObjects[i][j].GetComponents<Renderer>();
                                for (int k = 0; k < renderers.Length; ++k) {
                                    var materials = renderers[k].sharedMaterials;
                                    for (int m = 0; m < materials.Length; ++m) {
                                        if (materials[m] == m_InvisibleShadowCaster) {
                                            materials[m] = null;
                                        }
                                    }
                                    renderers[k].sharedMaterials = materials;
                                }
                            }
                        }

                        var addThirdPersonObject = false;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                        var networkInfo = m_Character.GetComponent<Shared.Networking.INetworkInfo>();
                        if (networkInfo != null) {
                            addThirdPersonObject = true;
                        }
#endif
                        if (!addThirdPersonObject) {
                            for (int j = 0; j < movementTypes.Count; ++j) {
                                if (movementTypes[j].GetType().FullName.Contains("ThirdPerson")) {
                                    addThirdPersonObject = true;
                                    break;
                                }
                            }
                        }

                        // All of the original hidden objects have been removed. Add the new objects.
                        for (int j = 0; j < m_ThirdPersonObjects[i].Length; ++j) {
                            if (m_ThirdPersonObjects[i][j] == null) {
                                continue;
                            }

                            if (!addThirdPersonObject) {
                                var renderers = m_ThirdPersonObjects[i][j].GetComponents<Renderer>();
                                for (int k = 0; k < renderers.Length; ++k) {
                                    var materials = renderers[k].sharedMaterials;
                                    for (int m = 0; m < materials.Length; ++m) {
                                        materials[m] = m_InvisibleShadowCaster;
                                    }
                                    renderers[k].sharedMaterials = materials;
                                }
                            } else {
                                // The PerspectiveMonitor component is responsible for switching out the material.
                                m_ThirdPersonObjects[i][j].AddComponent<Character.Identifiers.ThirdPersonObject>();
                            }
                        }
                        Shared.Editor.Utility.EditorUtility.SetDirty(m_Character);
                        Array.Resize(ref m_OriginalThirdPersonObjects[i], m_ThirdPersonObjects[i].Length);
                        m_ThirdPersonObjects[i].CopyTo(m_OriginalThirdPersonObjects[i], 0);
                    }
                }
            }

            if (m_ItemSlotParents != null) {
                for (int i = 0; i < m_ItemSlotParents.Length; ++i) { // Model loop.
                    if (m_ItemSlotParents[i] == null) {
                        continue;
                    }

                    for (int j = 0; j < m_ItemSlotParents[i].Length; ++j) { // Arm loop.
                        if (m_ItemSlotParents[i][j] == null) {
                            continue;
                        }

                        // The slots may have changed.
                        var slotChange = ((m_OriginalItemSlotParents == null || m_OriginalItemSlotParents[i] == null || m_OriginalItemSlotParents[i].Length <= j ||
                                            m_OriginalItemSlotParents[i][j] == null || m_OriginalItemSlotParents[i][j].Length == 0) &&
                                            m_ItemSlotParents[i][j].Length > 0) || (m_OriginalItemSlotParents[i][j].Length != m_ItemSlotParents[i][j].Length);
                        if (!slotChange && m_OriginalItemSlotParents != null && m_OriginalItemSlotParents[i] != null && m_OriginalItemSlotParents[i][j] != null) {
                            for (int k = 0; k < m_OriginalItemSlotParents[i][j].Length; ++k) {
                                if (m_OriginalItemSlotParents[i][j][k] != m_ItemSlotParents[i][j][k]) {
                                    slotChange = true;
                                    break;
                                }
                            }
                        }

                        if (slotChange) {
                            // Remove the original slots that are no longer used.
                            if (m_OriginalItemSlotParents != null && i < m_OriginalItemSlotParents.Length && m_OriginalItemSlotParents[i] != null &&
                                    j < m_OriginalItemSlotParents[i].Length && m_OriginalItemSlotParents[i][j] != null) {
                                for (int k = 0; k < m_OriginalItemSlotParents[i][j].Length; ++k) {
                                    if (m_OriginalItemSlotParents[i][j][k] == null) {
                                        continue;
                                    }
                                    var remove = true;
                                    for (int m = 0; m < m_ItemSlotParents[i][j].Length; ++m) {
                                        if (m_OriginalItemSlotParents[i][j][k] == m_ItemSlotParents[i][j][m]) {
                                            remove = false;
                                            break;
                                        }
                                    }
                                    if (remove) {
                                        var itemSlot = m_OriginalItemSlotParents[i][j][k].GetComponentInChildren<Items.CharacterItemSlot>();
                                        if (itemSlot != null) {
                                            // Only remove the component if objects are parented to the GameObject. This will prevent undesired objects from being removed.
                                            var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Character);
                                            if (!string.IsNullOrEmpty(prefabPath)) {
                                                PrefabUtility.UnpackPrefabInstance(m_Character, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                                            }
                                            UnityEngine.Object.DestroyImmediate(itemSlot.transform.childCount == 0 ? itemSlot.gameObject : itemSlot, true);
                                            if (!string.IsNullOrEmpty(prefabPath)) {
                                                PrefabUtility.SaveAsPrefabAssetAndConnect(m_Character, prefabPath, InteractionMode.AutomatedAction);
                                            }
                                        }
                                    }
                                }
                            }

                            // Add the new objects.
                            for (int k = 0; k < m_ItemSlotParents[i][j].Length; ++k) {
                                if (m_ItemSlotParents[i][j][k] == null) {
                                    continue;
                                }
                                ItemBuilder.AddItemSlot(m_ItemSlotParents[i][j][k], m_ItemSlotParentIDs[i][j][k]);
                            }
                            Shared.Editor.Utility.EditorUtility.SetDirty(m_Character);
                            Array.Resize(ref m_OriginalItemSlotParents[i], m_ItemSlotParents[i].Length);
                            Array.Resize(ref m_OriginalItemSlotParents[i][j], m_ItemSlotParents[i][j].Length);
                            m_ItemSlotParents[i][j].CopyTo(m_OriginalItemSlotParents[i][j], 0);
                        }
                    }
                }
            }

            if (m_TemplateCharacter == null) {
                if (m_AIAgent != IsAIAgent(m_Character)) {
                    if (m_AIAgent) {
                        CharacterBuilder.AddAIAgent(m_Character);
                    } else {
                        CharacterBuilder.RemoveAIAgent(m_Character);
                    }
                }
                if (m_NavMeshAgent != HasNavMeshAgent(m_Character)) {
                    if (m_NavMeshAgent) {
                        var abilities = characterLocomotion.Abilities;
                        var index = abilities != null ? abilities.Length : 0;
                        if (abilities != null) {
                            for (int i = 0; i < abilities.Length; ++i) {
                                if (abilities[i] is Character.Abilities.SpeedChange) {
                                    index = i;
                                    break;
                                }
                            }
                        }
                        // The ability should be positioned before the SpeedChange ability.
                        AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.AI.NavMeshAgentMovement), index);
                        var navMeshAgent = m_Character.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (navMeshAgent != null) {
                            navMeshAgent.stoppingDistance = 0.1f;
                        }
                    } else {
                        AbilityBuilder.RemoveAbility<Character.Abilities.AI.NavMeshAgentMovement>(m_Character.GetComponent<UltimateCharacterLocomotion>());
                        var navMeshAgent = m_Character.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (navMeshAgent != null) {
                            UnityEngine.Object.DestroyImmediate(navMeshAgent, true);
                        }
                    }
                }
                if (m_Items != HasItems(m_Character, m_AIAgent, m_Perspective != Perspective.Third)) {
                    if (m_Items) {
                        CharacterBuilder.AddItemSupport(m_Character, m_ItemCollection, m_ItemSetRule, m_AIAgent, m_Perspective != Perspective.Third);
                    } else {
                        CharacterBuilder.RemoveItemSupport(m_Character);
                    }
                } else {
                    var itemSetManager = m_Character.GetComponent<Inventory.ItemSetManager>();
                    if (itemSetManager != null && m_ItemCollection != itemSetManager.ItemCollection) {
                        itemSetManager.ItemSetGroups = null;
                        itemSetManager.ItemCollection = m_ItemCollection;
                    }
                }
                if (m_Health != HasHealth(m_Character)) {
                    if (m_Health) {
                        CharacterBuilder.AddHealth(m_Character);
                    } else {
                        CharacterBuilder.RemoveHealth(m_Character);
                    }
                }
                if (m_UnityIK != HasUnityIK(m_Character)) {
                    if (m_UnityIK) {
                        CharacterBuilder.AddUnityIK(m_Character);
                    } else {
                        CharacterBuilder.RemoveUnityIK(m_Character);
                    }
                }
                if (m_FootEffects != HasFootEffects(m_Character)) {
                    if (m_FootEffects) {
                        CharacterBuilder.AddFootEffects(m_Character);
                    } else {
                        CharacterBuilder.RemoveFootEffects(m_Character);
                    }
                }
                if (m_Ragdoll != HasRagdoll(m_Character)) {
                    if (m_Ragdoll) {
                        AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.Ragdoll));
                        for (int i = 0; i < m_CharacterModels.Length; ++i) {
                            Controls.Types.AbilityDrawers.RagdollInspectorDrawer.AddRagdollColliders(m_CharacterModels[i], true);
                        }
                    } else {
                        AbilityBuilder.RemoveAbility<Character.Abilities.Ragdoll>(characterLocomotion);
                        for (int i = 0; i < m_CharacterModels.Length; ++i) {
                            Controls.Types.AbilityDrawers.RagdollInspectorDrawer.RemoveRagdollColliders(m_CharacterModels[i]);
                        }
                    }
                }
            }
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Character);
            ShowCharacter(container);
        }

        /// <summary>
        /// Retrieves the current perspective of the character.
        /// </summary>
        /// <returns>The character's perspective.</returns>
        private Perspective CurrentPerspective()
        {
            var hasBothComponents = false;
#if THIRD_PERSON_CONTROLLER
            hasBothComponents = m_Character.GetComponentInChildren<ThirdPersonController.Character.PerspectiveMonitor>() != null;
#endif
            if (hasBothComponents) {
                // If the character has the perspective monitor then it can switch perspectives.
                return Perspective.Both;
            } else {
                if (!m_Animator) {
                    // If the character doesn't have an animator then it has to be in first person.
                    return Perspective.First;
                } else {
                    // Use the movement types to determine the perspective.
                    var perspective = Perspective.Third;
                    var characterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
                    if (characterLocomotion != null) {
                        var movementTypes = characterLocomotion.MovementTypes;
                        for (int i = 0; i < movementTypes.Length; ++i) {
                            var movementNamespace = movementTypes[i].GetType().Namespace;
                            if (movementNamespace != null && movementNamespace.Contains("FirstPersonController")) {
                                perspective = Perspective.First;
                                break;
                            }
                        }
                    }
                    return perspective;
                }
            }
        }

        /// <summary>
        /// Does the character have an animator?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has an animator.</returns>
        private bool HasAnimator(GameObject character)
        {
            var animatorMonitor = character.GetComponentInChildren<AnimatorMonitor>();
            if (animatorMonitor == null) {
                return false;
            }

            var animator = animatorMonitor.GetComponent<Animator>();
            return animator != null;
        }

        /// <summary>
        /// Is the character an AI agent?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character is an AI agent.</returns>
        private bool IsAIAgent(GameObject character)
        {
            return character.GetComponent<LocalLookSource>() && !character.GetComponent<UltimateCharacterLocomotionHandler>() && !character.GetComponent<ItemHandler>();
        }

        /// <summary>
        /// Does the character have the NavMeshAgent components?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the NavMeshAgent components.</returns>
        private bool HasNavMeshAgent(GameObject character)
        {
            return character.GetComponent<UnityEngine.AI.NavMeshAgent>() != null && character.GetComponent<UltimateCharacterLocomotion>().GetAbility<Character.Abilities.AI.NavMeshAgentMovement>() != null;
        }

        /// <summary>
        /// Does the character have the components required for items?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        /// <param name="checkFirstPersonObjects">Should the FirstPersonObjects component be checked?</param>
        /// <returns>True if the character has the components required for items.</returns>
        private bool HasItems(GameObject character, bool aiAgent, bool checkFirstPersonObjects)
        {
            if ((!aiAgent && character.GetComponent<ItemHandler>() == null) || character.GetComponentInChildren<Items.ItemPlacement>() == null || character.GetComponentInChildren<AnimatorMonitor>() == null) {
                return false;
            }

#if FIRST_PERSON_CONTROLLER
            if (checkFirstPersonObjects && character.GetComponentInChildren<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>() == null) {
                return false;
            }
#endif
            return character.GetComponent<Inventory.Inventory>() && character.GetComponent<Inventory.ItemSetManager>();
        }

        /// <summary>
        /// Does the character have the health components?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the health components.</returns>
        private bool HasHealth(GameObject character)
        {
            return character.GetComponent<Traits.CharacterHealth>() && character.GetComponent<Traits.AttributeManager>() && character.GetComponent<Traits.CharacterRespawner>();
        }

        /// <summary>
        /// Does the character have the CharacterIK component?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the CharacterIK component.</returns>
        private bool HasUnityIK(GameObject character)
        {
            var animatorMonitors = character.GetComponentsInChildren<AnimatorMonitor>();
            var count = 0;
            for (int i = 0; i < animatorMonitors.Length; ++i) {
                var animator = animatorMonitors[i].GetComponent<Animator>();
                if (animator == null || !IsValidHumanoid(animator)) {
                    continue;
                }
                count++;
            }
            return count > 0 && character.GetComponentsInChildren<CharacterIK>().Length == count;
        }

        /// <summary>
        /// Does the character have the CharacterFootEffects component?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the CharacterFootEffects component.</returns>
        private bool HasFootEffects(GameObject character)
        {
            var animatorMonitors = character.GetComponentsInChildren<AnimatorMonitor>();
            var count = 0;
            for (int i = 0; i < animatorMonitors.Length; ++i) {
                var animator = animatorMonitors[i].GetComponent<Animator>();
                if (animator == null || !IsValidHumanoid(animator)) {
                    continue;
                }
                count++;
            }
            return count > 0 && character.GetComponentsInChildren<CharacterFootEffects>().Length == count;
        }

        /// <summary>
        /// Does the character have the ragdoll ability?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the ragdoll ability.</returns>
        private bool HasRagdoll(GameObject character)
        {
            var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return false;
            }

            var ragdoll = characterLocomotion.GetAbility<Character.Abilities.Ragdoll>();
            return ragdoll != null;
        }

        /// <summary>
        /// Copies the template character values to the specified character.
        /// </summary>
        private void CopyTemplateCharacter()
        {
            var conflictingObjects = new List<ReferenceResolverWindow.ConflictingObjects>();

            var characterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
            var originalAbilities = characterLocomotion.Abilities;
            var originalItemAbilities = characterLocomotion.ItemAbilities;
            var originalEffects = characterLocomotion.Effects;

            var toResolveTemplateObjects = new List<object>();
            var toResolveTargetObjects = new List<object>();

            if (m_CopyComponents) {
                var components = m_TemplateCharacter.GetComponents<Component>();
                for (int i = 0; i < components.Length; ++i) {
                    // Only copy the Opsive components.
                    if (components[i] == null || !components[i].GetType().Namespace.Contains("Opsive")) {
                        continue;
                    }

                    // Ignore the IgnoreTemplateCopy classes.
                    if (components[i].GetType().GetCustomAttributes(typeof(IgnoreTemplateCopy), true).Length > 0) {
                        continue;
                    }

                    Component targetComponent;
                    if ((targetComponent = m_Character.GetComponent(components[i].GetType())) == null) {
                        targetComponent = m_Character.AddComponent(components[i].GetType());
                    }

                    toResolveTemplateObjects.Add(components[i]);
                    toResolveTargetObjects.Add(targetComponent);
                }

                // The abilities, item abilities, and effects should only be copied if specified.
                characterLocomotion.Abilities = originalAbilities;
                characterLocomotion.ItemAbilities = originalItemAbilities;
                characterLocomotion.Effects = originalEffects;
            }

            var templateCharacterLocomotion = m_TemplateCharacter.GetComponent<UltimateCharacterLocomotion>();
            if (m_CopyAbilities && templateCharacterLocomotion.Abilities != null) {
                for (int i = 0; i < templateCharacterLocomotion.Abilities.Length; ++i) {
                    var addedAbility = AbilityBuilder.AddAbility(characterLocomotion, templateCharacterLocomotion.Abilities[i].GetType());
                    // Allow the ability to perform any initialization.
                    var abilityDrawer = Controls.Types.AbilityDrawers.AbilityDrawerUtility.FindAbilityDrawer(addedAbility.GetType(), true);
                    if (abilityDrawer != null) {
                        abilityDrawer.AbilityAdded(addedAbility, characterLocomotion);
                    }

                    toResolveTemplateObjects.Add(templateCharacterLocomotion.Abilities[i]);
                    toResolveTargetObjects.Add(addedAbility);
                }
            }

            if (m_CopyItemAbilities && templateCharacterLocomotion.ItemAbilities != null) {
                for (int i = 0; i < templateCharacterLocomotion.ItemAbilities.Length; ++i) {
                    var addedItemAbility = AbilityBuilder.AddItemAbility(characterLocomotion, templateCharacterLocomotion.ItemAbilities[i].GetType());
                    // Allow the ability to perform any initialization.
                    var abilityDrawer = Controls.Types.AbilityDrawers.AbilityDrawerUtility.FindAbilityDrawer(addedItemAbility.GetType(), true);
                    if (abilityDrawer != null) {
                        abilityDrawer.AbilityAdded(addedItemAbility, characterLocomotion);
                    }

                    toResolveTemplateObjects.Add(templateCharacterLocomotion.ItemAbilities[i]);
                    toResolveTargetObjects.Add(addedItemAbility);
                }
            }

            if (m_CopyEffects && templateCharacterLocomotion.Effects != null) {
                for (int i = 0; i < templateCharacterLocomotion.Effects.Length; ++i) {
                    var addedEffect = EffectBuilder.AddEffect(characterLocomotion, templateCharacterLocomotion.Effects[i].GetType());
                    toResolveTemplateObjects.Add(templateCharacterLocomotion.Effects[i]);
                    toResolveTargetObjects.Add(addedEffect);
                }
            }

            if (m_CopyItems) {
                var itemParent = characterLocomotion.GetComponentInChildren<Items.ItemPlacement>(true);
                if (characterLocomotion.GetComponentInChildren<Items.ItemPlacement>() == null) {
                    var items = new GameObject("Items");
                    items.transform.parent = characterLocomotion.transform;
                    itemParent = items.AddComponent<Items.ItemPlacement>();
                }

                var templateItems = templateCharacterLocomotion.GetComponentsInChildren<Items.CharacterItem>();
                for (int i = 0; i < templateItems.Length; ++i) {
                    GameObject.Instantiate(templateItems[i], itemParent.transform);
                }
            }

            // Resolve all of the fields after all of the components have been added. This will ensure any potential target references are valid.
            for (int i = 0; i < toResolveTemplateObjects.Count; ++i) {
                ReferenceResolverWindow.ResolveFields(m_TemplateCharacter, m_Character, typeof(AnimatorMonitor), toResolveTemplateObjects[i], toResolveTargetObjects[i], conflictingObjects);
            }

            if (conflictingObjects.Count > 0) {
                m_ReferenceResolverWindow = EditorWindow.GetWindow<ReferenceResolverWindow>(true, "Reference Resolver");
                m_ReferenceResolverWindow.minSize = m_ReferenceResolverWindow.maxSize = new Vector2(600, 500);
                m_ReferenceResolverWindow.Initialize(conflictingObjects, m_TemplateCharacter, "character");
            }
        }
    }
}