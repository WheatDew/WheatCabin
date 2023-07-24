/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
/// 
namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Inventory;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using System.Collections.Generic;

    /// <summary>
    /// The ItemManager will draw any item properties
    /// </summary>
    [OrderedEditorItem("Item", 4)]
    public class ItemManager : Manager
    {
        private const int c_SectionSeparationHeight = 16;
        private enum ThirdPersonHumanoidParentHand { Left, Right }

        /// <summary>
        /// The Visual Element for each ActionInfo row in the ReorderableList.
        /// </summary>
        private class ActionInfoRowElement : VisualElement
        {
            private ItemManager m_ItemManager;
            private int m_Index;

            private EnumField m_ActionTypeField;
            private TextField m_ActionNameField;

            /// <summary>
            /// Creates the ActionInfoRowElement.
            /// </summary>
            /// <param name="itemManager">A reference to the parent ItemManager.</param>
            /// <param name="list">The ReorderableList.</param>
            public ActionInfoRowElement(ItemManager itemManager, ReorderableList list)
            {
                m_ItemManager = itemManager;

                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                Add(horizontalLayout);

                m_ActionTypeField = new EnumField(ItemBuilder.ActionType.Shootable);
                m_ActionTypeField.tooltip = "A drop down field which allows you to specify which Item Action should be added. More Item Actions can be added to the Item through the Existing Item tab.";
                m_ActionTypeField.style.marginTop = c_SectionSeparationHeight;
                m_ActionTypeField.RegisterValueChangedCallback(c =>
                {
                    itemManager.ActionTypes[m_Index].Type = (ItemBuilder.ActionType)c.newValue;
                    list.Refresh();
                });
                m_ActionTypeField.style.marginTop = 1;
                m_ActionTypeField.style.width = 200;
                horizontalLayout.Add(m_ActionTypeField);

                m_ActionNameField = new TextField();
                m_ActionNameField.RegisterValueChangedCallback(c =>
                {
                    itemManager.ActionTypes[m_Index].Name = c.newValue;
                });
                m_ActionNameField.style.marginLeft = 6;
                m_ActionNameField.style.flexGrow = 1;
                horizontalLayout.Add(m_ActionNameField);
            }

            /// <summary>
            /// Binds the element to the specified ActionTypeInfo index.
            /// </summary>
            /// <param name="index">The index of the element.</param>
            public void BindItem(int index)
            {
                m_Index = index;
                m_ActionTypeField.value = m_ItemManager.ActionTypes[index].Type;
                m_ActionNameField.value = m_ItemManager.ActionTypes[index].Name;
            }
        }

        [SerializeField] private GameObject m_Item;
        [SerializeField] private string m_Name;
        [SerializeField] private GameObject m_Character;
        [SerializeField] private ItemDefinitionBase m_ItemDefinition;
        [SerializeField] private bool m_AddToDefaultLoadout = true;
        [SerializeField] private bool m_AddToItemTypePrefab = true;
        [SerializeField] private int m_SlotID;
        [SerializeField] private int m_AnimatorItemID;
        [SerializeField] private bool m_AddFirstPersonPerspective = true;
        [SerializeField] private GameObject m_FirstPersonBase;
        [SerializeField] private RuntimeAnimatorController m_FirstPersonBaseAnimatorController = null;
        [SerializeField] private GameObject m_FirstPersonVisibleItem;
        [SerializeField] private RuntimeAnimatorController m_FirstPersonVisibleItemAnimatorController = null;
        [SerializeField] private GameObject m_FirstPersonParent;
        [SerializeField] private bool m_AddThirdPersonPerspective = true;
        [SerializeField] private GameObject m_ThirdPersonVisibleItem;
        [SerializeField] private RuntimeAnimatorController m_ThirdPersonObjectAnimatorController;
        [SerializeField] private ThirdPersonHumanoidParentHand m_ThirdHumanoidParentHand = ThirdPersonHumanoidParentHand.Right;
        [SerializeField] private GameObject m_ThirdPersonParent;

        [SerializeField] private CharacterItem m_ActionTemplate;
        [SerializeField] private ItemBuilder.ActionInfo[] m_ActionTypes;
        [SerializeField] private List<ItemBuilder.ActionInfo> m_RemoveActions;
        [SerializeField] private List<ItemBuilder.ActionInfo> m_AddActions;

        private CharacterItemSlot m_FirstPersonCharacterItemSlot = null;
        private CharacterItemSlot m_ThirdPersonCharacterItemSlot = null;

        private Material m_InvisibleShadowCaster;

        private HelpBox m_NextHelpBox;
        private ReferenceResolverWindow m_ReferenceResolverWindow;

        private ItemBuilder.ActionInfo[] ActionTypes => m_ActionTypes;

        /// <summary>
        /// Initializes the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            m_InvisibleShadowCaster = ManagerUtility.FindInvisibleShadowCaster(m_MainManagerWindow);

            m_ReferenceResolverWindow = EditorWindow.FindObjectOfType<ReferenceResolverWindow>();
            if (m_ReferenceResolverWindow != null) {
                m_ReferenceResolverWindow.Close();
            }
        }

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var container = new VisualElement();
            m_ManagerContentContainer.Add(container);
            ShowItem(container);
        }

        /// <summary>
        /// Shows the item fields.
        /// </summary>
        private void ShowItem(VisualElement container)
        {
            container.Clear();
            m_NextHelpBox = null;

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
                Application.OpenURL("https://opsive.com/support/documentation/ultimate-character-controller/items-inventory/item-creation/common-setups");
            });
            linkConfigLabel.enableRichText = true;
            linkConfigLabel.AddToClassList("hyperlink");
            horizontalLayout.Add(linkConfigLabel);
            var endConfigLabel = new Label("for example configurations.");
            horizontalLayout.Add(endConfigLabel);

            var canBuild = true;
            var buildButton = new Button();

            horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            scrollView.Add(horizontalLayout);

            var itemField = new ObjectField("Item");
            itemField.objectType = typeof(GameObject);
            itemField.value = m_Item;
            itemField.tooltip = "Specifies the visual representation of the item.";
            itemField.RegisterValueChangedCallback(c =>
            {
                m_Item = (GameObject)c.newValue;

                OnItemChange();
                ShowItem(container);
            });
            itemField.style.flexGrow = 1;
            horizontalLayout.Add(itemField);
            var itemError = false;
            if (m_Item != null && m_Item.GetComponent<CharacterItem>() == null) {
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m_Item))) {
                    ShowHelpBox(scrollView, "The Item GameObject should be dragged into the scene.", canBuild);
                    canBuild = false;
                    itemError = true;
                }
            } else if (m_Item != null && m_Item.GetComponent<CharacterItem>() != null && m_Character != null) {
                var removeButton = new Button();
                removeButton.text = "Remove";
                removeButton.clicked += () =>
                {
                    var itemPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_Item);
                    if (!string.IsNullOrEmpty(itemPrefabPath)) {
                        PrefabUtility.UnpackPrefabInstance(m_Item, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    }

                    var perspectiveItems = m_Item.GetComponents<PerspectiveItem>();
                    for (int i = 0; i < perspectiveItems.Length; ++i) {
                        var visibleObject = perspectiveItems[i].GetVisibleObject();
                        if (visibleObject != null) {
                            Object.DestroyImmediate(visibleObject, true);
                        }
                    }
                    Object.DestroyImmediate(m_Item, true);
                    m_Item = null;
                    OnItemChange();
                    ShowItem(container);
                };
                horizontalLayout.Add(removeButton);
            }
            var buildItem = m_Item == null || m_Item.GetComponent<CharacterItem>() == null;

            var createItemDefinitionButton = new Button();
            var itemNameField = new TextField("Name");
            var itemNameHelpBox = new HelpBox("The item must have a name.", HelpBoxMessageType.Error);
            itemNameField.tooltip = "Specifies the name of the item. It is recommended that this be a unique name though it is not required.";
            itemNameField.value = m_Name;
            itemNameField.RegisterValueChangedCallback(c =>
            {
                m_Name = c.newValue;
                var validName = !string.IsNullOrEmpty(m_Name);
                itemNameHelpBox.style.display = validName ? DisplayStyle.None : DisplayStyle.Flex;
                createItemDefinitionButton.SetEnabled(!string.IsNullOrEmpty(m_Name));

                // When the name textfield is updated ShowItem isn't called to prevent the textfield from losing focus.
                // If the name switches valid states the next error message should be displayed.
                if (m_NextHelpBox != null) {
                    m_NextHelpBox.style.display = validName ? DisplayStyle.Flex : DisplayStyle.None;
                }
                buildButton.SetEnabled(validName && m_NextHelpBox == null);
            });
            scrollView.Add(itemNameField);
            canBuild = !string.IsNullOrEmpty(m_Name) && canBuild;
            itemNameHelpBox.style.display = canBuild || itemError ? DisplayStyle.None : DisplayStyle.Flex;
            scrollView.Add(itemNameHelpBox);

            if (buildItem) {
                var characterField = new ObjectField("Character");
                characterField.tooltip = "Specifies the character that the Item should be added to. This field should be empty if the item will be added at runtime.";
                characterField.objectType = typeof(GameObject);
                characterField.value = m_Character;
                characterField.RegisterValueChangedCallback(c =>
                {
                    m_Character = (GameObject)c.newValue;
                    if (m_Character != null && m_Character.GetComponent<Inventory>()) {
                        m_AddToDefaultLoadout = true;
                    }
                    ShowItem(container);
                });
                scrollView.Add(characterField);
                if (m_Character != null) {
                    if (EditorUtility.IsPersistent(m_Character)) {
                        ShowHelpBox(scrollView, "The character must be located within the scene.", canBuild);
                        canBuild = false;
                    } else if (m_Character.GetComponent<Character.UltimateCharacterLocomotion>() == null) {
                        ShowHelpBox(scrollView, "The character must be an already created character.", canBuild);
                        canBuild = false;
                    }

                    if (canBuild) {
                        var modelManager = m_Character.GetComponent<Character.ModelManager>();
                        if (modelManager != null && modelManager.AvailableModels.Length > 1) {
                            var warningBox = new HelpBox("Items should be built as a prefab if the character can switch models.", HelpBoxMessageType.Warning);
                            scrollView.Add(warningBox);
                        }
                    }
                } else if (m_Character == null) {
                    var slotIDField = new IntegerField("Slot ID");
                    slotIDField.tooltip = "The ID of the slot that the Item should occupy. " +
                                                        "The Item will be parented to the Item Slot component for the corresponding perspective. " +
                                                        "The Slot ID must match for both first and third person perspective.";
                    slotIDField.value = m_SlotID;
                    slotIDField.RegisterValueChangedCallback(c =>
                    {
                        m_SlotID = c.newValue;
                    });
                    scrollView.Add(slotIDField);
                }
            }

            horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            scrollView.Add(horizontalLayout);

            var itemDefinitionField = new ObjectField("Item Definition");
            itemDefinitionField.tooltip = "The Item Definition that the Item should use. The Item Definition works with the Inventory to determine the properties for that item.";
            itemDefinitionField.objectType = typeof(ItemDefinitionBase);
            itemDefinitionField.allowSceneObjects = false;
            itemDefinitionField.value = m_ItemDefinition;
            itemDefinitionField.style.flexGrow = 1;
            itemDefinitionField.RegisterValueChangedCallback(c =>
            {
                m_ItemDefinition = (ItemDefinitionBase)c.newValue;
                ShowItem(container);
            });
            horizontalLayout.Add(itemDefinitionField);
            if (m_Character != null) {
                if (m_ItemDefinition == null) {
                    var itemCollection = m_Character.GetComponent<ItemSetManager>()?.ItemCollection;
                    if (itemCollection != null) {
                        createItemDefinitionButton.text = "Create";
                        createItemDefinitionButton.style.width = 75;
                        createItemDefinitionButton.SetEnabled(!string.IsNullOrEmpty(m_Name));
                        createItemDefinitionButton.clicked += () =>
                        {
                            m_ItemDefinition = ItemTypeManager.AddItemType(itemCollection, m_Name);
                            ShowItem(container);
                        };
                        horizontalLayout.Add(createItemDefinitionButton);
                    }
                } else {
                    if (m_ItemDefinition != null & m_Character.GetComponent<Inventory>() != null && buildItem) {
                        // The item can automatically be added to the default loadout if the inventory component exists.
                        var addToDefaultLoadoutToggle = new Toggle("Add to Default Loadout");
                        addToDefaultLoadoutToggle.tooltip = "If a character is specified the Item Definition can automatically be added to the Inventory's Default Loadout.";
                        addToDefaultLoadoutToggle.Q<Label>().AddToClassList("indent");
                        addToDefaultLoadoutToggle.value = m_AddToDefaultLoadout;
                        addToDefaultLoadoutToggle.RegisterValueChangedCallback(c =>
                        {
                            m_AddToDefaultLoadout = c.newValue;
                        });
                        scrollView.Add(addToDefaultLoadoutToggle);
                    } else {
                        m_AddToDefaultLoadout = false;
                    }
                }
            } else if (m_ItemDefinition != null && m_ItemDefinition is ItemType && buildItem) {
                // The prefab can automatically be added to the item type.
                var addToItemDefinitionToggle = new Toggle("Add Item Prefab to Item Definition");
                addToItemDefinitionToggle.tooltip = "If a prefab is created the prefab can automatically be added to the Item Type's prefab field.";
                addToItemDefinitionToggle.Q<Label>().AddToClassList("indent");
                addToItemDefinitionToggle.value = m_AddToItemTypePrefab;
                addToItemDefinitionToggle.RegisterValueChangedCallback(c =>
                {
                    m_AddToItemTypePrefab = c.newValue;
                });
                scrollView.Add(addToItemDefinitionToggle);
            }
            if (m_ItemDefinition == null) {
                ShowHelpBox(scrollView, "The item must have an Item Definition.", canBuild);
                canBuild = false;
            }

            var animatorIDField = new IntegerField("Animator Item ID");
            animatorIDField.tooltip = "The ID of the Item within the Animator Controller. " +
                                      "This ID is used by the SlotXItemID parameter within the Animator Controller and it must be unique for each item.";
            animatorIDField.value = m_AnimatorItemID;
            animatorIDField.RegisterValueChangedCallback(c =>
            {
                m_AnimatorItemID = c.newValue;
            });
            scrollView.Add(animatorIDField);

            if (buildItem) {
#if FIRST_PERSON_CONTROLLER
                var firstPersonLabel = new Label("First Person");
                firstPersonLabel.style.marginTop = c_SectionSeparationHeight;
                firstPersonLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                firstPersonLabel.style.fontSize = 12;
                scrollView.Add(firstPersonLabel);

                var firstPersonToggle = new Toggle("Add First Person Item");
                var firstPersonContainer = new VisualElement();
                firstPersonToggle.tooltip = "Should the first person item perspective be added?";
                firstPersonToggle.value = m_AddFirstPersonPerspective;
                firstPersonToggle.RegisterValueChangedCallback(c =>
                {
                    m_AddFirstPersonPerspective = c.newValue;
                    ShowItem(container);
                });
                firstPersonContainer.SetEnabled(m_AddFirstPersonPerspective);
                scrollView.Add(firstPersonToggle);
                scrollView.Add(firstPersonContainer);

                canBuild = ShowFirstPersonObjects(container, firstPersonContainer, canBuild);
#endif

                var thirdPersonLabel = new Label("Third Person (including AI and Multiplayer)");
                thirdPersonLabel.style.marginTop = c_SectionSeparationHeight;
                thirdPersonLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                thirdPersonLabel.style.fontSize = 12;
                scrollView.Add(thirdPersonLabel);

                var thirdPersonToggle = new Toggle("Add Third Person Item");
                var thirdPersonContainer = new VisualElement();
                thirdPersonToggle.tooltip = "Should the third person item perspective be added?";
                thirdPersonToggle.value = m_AddThirdPersonPerspective;
                thirdPersonToggle.RegisterValueChangedCallback(c =>
                {
                    m_AddThirdPersonPerspective = c.newValue;
                    ShowItem(container);
                });
                thirdPersonContainer.SetEnabled(m_AddThirdPersonPerspective);
                scrollView.Add(thirdPersonToggle);
                scrollView.Add(thirdPersonContainer);

                canBuild = ShowThirdPersonObjects(container, thirdPersonContainer, canBuild);
            }

            var actionLabel = new Label("Actions");
            actionLabel.style.marginTop = c_SectionSeparationHeight;
            actionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionLabel.style.fontSize = 12;
            scrollView.Add(actionLabel);

            HelpBox actionHelpBox = null;
            if (m_Item == null || m_Item.GetComponent<CharacterItem>() == null) {
                var actionTemplateField = new ObjectField("Action Template");
                actionTemplateField.objectType = typeof(CharacterItem);
                actionTemplateField.value = m_ActionTemplate;
                actionTemplateField.RegisterValueChangedCallback(c =>
                {
                    m_ActionTemplate = (CharacterItem)c.newValue;
                    if (m_ActionTemplate != null) {
                        // Item Actions. Sort by ID.
                        var itemActions = m_ActionTemplate.GetComponents<CharacterItemAction>();
                        System.Array.Sort(itemActions, CompareActions);
                        m_ActionTypes = new ItemBuilder.ActionInfo[itemActions.Length];
                        for (int i = 0; i < m_ActionTypes.Length; ++i) {
                            var actionType = GetActionType(itemActions[i]);
                            m_ActionTypes[i] = new ItemBuilder.ActionInfo() { Type = actionType, Name = itemActions[i].ActionName };
                        }
                    } else {
                        m_ActionTypes = null;
                    }
                    ShowItem(container);
                });
                scrollView.Add(actionTemplateField);
                if (m_ActionTemplate != null && m_Item == m_ActionTemplate.gameObject) {
                    ShowHelpBox(scrollView, "The template item cannot be the same as the editing item.", canBuild);
                    canBuild = false;
                }

                if (canBuild && (m_ActionTypes == null || m_ActionTypes.Length == 0)) {
                    actionHelpBox = new HelpBox("It is recommended that at least one action is added.", HelpBoxMessageType.Info);
                    scrollView.Add(actionHelpBox);
                }
            }

            ReorderableList reorderableList = null;
            reorderableList = new ReorderableList(m_ActionTypes,
            (VisualElement element, int index) => // Make Item.
            {
                element.Add(new ActionInfoRowElement(this, reorderableList));
            }, (VisualElement element, int index) => // Bind Item.
            {
                var actionInfoRowElement = element.Q<ActionInfoRowElement>();
                actionInfoRowElement.BindItem(index);
            }, null, null, () => // On Add.
            {
                var addMenu = new GenericMenu();
                var actionTypes = System.Enum.GetNames(typeof(ItemBuilder.ActionType));
                for (int i = 0; i < actionTypes.Length; ++i) {
                    var index = i;
                    addMenu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(actionTypes[i])), false, () =>
                    {
                        if (m_ActionTypes == null) {
                            m_ActionTypes = new ItemBuilder.ActionInfo[1];
                        } else {
                            System.Array.Resize(ref m_ActionTypes, m_ActionTypes.Length + 1);
                        }
                        m_ActionTypes[m_ActionTypes.Length - 1].Type = (ItemBuilder.ActionType)index;

                        var count = 1;
                        for (int i = 0; i < m_ActionTypes.Length - 1; ++i) {
                            if (m_ActionTypes[i].Type == (ItemBuilder.ActionType)index) {
                                count++;
                            }
                        }
                        m_ActionTypes[m_ActionTypes.Length - 1].Name = ObjectNames.NicifyVariableName(actionTypes[index]) + " " + count;
                        reorderableList.ItemsSource = m_ActionTypes;

                        if (actionHelpBox != null) {
                            actionHelpBox.style.display = DisplayStyle.None;
                        }
                    });
                }
                addMenu.ShowAsContext();
            }, (int index) => // On Remove.
            {
                ArrayUtility.RemoveAt(ref m_ActionTypes, index);
                reorderableList.ItemsSource = m_ActionTypes;
            }, null);
            reorderableList.SetEnabled(m_ActionTemplate == null);
            scrollView.Add(reorderableList);

#if FIRST_PERSON_CONTROLLER
            // The slot IDs must match.
            if (m_FirstPersonCharacterItemSlot != null && m_ThirdPersonCharacterItemSlot != null && m_FirstPersonCharacterItemSlot.ID != m_ThirdPersonCharacterItemSlot.ID && canBuild) {
                ShowHelpBox(scrollView, "The first and third person ItemSlots must use the same ID.", canBuild);
                canBuild = false;
            }
#endif

            if (!m_AddFirstPersonPerspective && !m_AddThirdPersonPerspective) {
                ShowHelpBox(scrollView, "At least one perspective must be added.", canBuild);
                canBuild = false;
            }

            buildButton.text = buildItem ? "Build Item" : "Update Item";
            buildButton.SetEnabled(canBuild);
            scrollView.Add(buildButton);

            buildButton.clicked += () =>
            {
                if (m_ReferenceResolverWindow != null) {
                    m_ReferenceResolverWindow.Close();
                    m_ReferenceResolverWindow = null;
                }

                if (buildItem) {
                    BuildItem();
                } else {
                    UpdateItem();
                }
                ShowItem(container);
            };
        }

        /// <summary>
        /// Shows a HelpBox.
        /// </summary>
        private void ShowHelpBox(VisualElement container, string message, bool canBuild)
        {
            var helpBox = new HelpBox(message, HelpBoxMessageType.Error);
            helpBox.style.display = canBuild ? DisplayStyle.Flex : DisplayStyle.None;
            container.Add(helpBox);

            // When the name textbox is changed it doesn't refresh the entire container to keep the textbox in focus.
            // The error message can change based on if the name textbox has an error so keep a reference to the next
            // helpbox that should be shown.
            m_NextHelpBox = helpBox;
        }

        /// <summary>
        /// The item field has changed.
        /// </summary>
        /// <param name="closeReferenceWindow">Should the reference window be closed?</param>
        private void OnItemChange(bool closeReferenceWindow = false)
        {
            if (closeReferenceWindow && m_ReferenceResolverWindow != null) {
                m_ReferenceResolverWindow.Close();
                m_ReferenceResolverWindow = null;
            }

            if (m_Item == null) {
                m_FirstPersonBase = null;
                m_FirstPersonVisibleItem = null;
                m_ThirdPersonVisibleItem = null;
                m_Name = null;
                return;
            }

            if (m_Item.GetComponent<CharacterItem>() == null) {
                m_FirstPersonVisibleItem = m_Item;
                m_ThirdPersonVisibleItem = m_Item;
                m_Name = m_Item.name;
                return;
            }

            // Existing item.
            m_Name = m_Item.name;

            var characterItem = m_Item.GetComponent<CharacterItem>();
            m_ItemDefinition = characterItem.ItemDefinition;
            m_SlotID = characterItem.SlotID;
            m_AnimatorItemID = characterItem.AnimatorItemID;

            var characterLocomotion = m_Item.GetComponentInParent<Character.UltimateCharacterLocomotion>();
            if (characterLocomotion != null) {
                m_Character = characterLocomotion.gameObject;
            }

#if FIRST_PERSON_CONTROLLER
            var firstPersonPerspectiveItem = m_Item.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspectiveItem != null) {
                m_FirstPersonBase = firstPersonPerspectiveItem.Object;
                if (m_FirstPersonBase != null) {
                    var firstPersonAnimator = m_FirstPersonBase.GetComponent<Animator>();
                    if (firstPersonAnimator != null) {
                        m_FirstPersonBaseAnimatorController = firstPersonAnimator.runtimeAnimatorController;
                    }
                    var itemSlots = m_FirstPersonBase.GetComponentsInChildren<CharacterItemSlot>();
                    for (int i = 0; i < itemSlots.Length; ++i) {
                        if (itemSlots[i].ID == m_SlotID) {
                            m_FirstPersonParent = itemSlots[i].gameObject;
                            m_FirstPersonCharacterItemSlot = itemSlots[i];
                            break;
                        }
                    }
                }
                m_FirstPersonVisibleItem = firstPersonPerspectiveItem.VisibleItem;
                if (m_FirstPersonVisibleItem != null) {
                    var firstPersonAnimator = m_FirstPersonVisibleItem.GetComponent<Animator>();
                    if (firstPersonAnimator != null) {
                        m_FirstPersonVisibleItemAnimatorController = firstPersonAnimator.runtimeAnimatorController;
                    }
                }
            }
#endif

            var thirdPersonPerspectiveItem = m_Item.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonPerspectiveItem != null) {
                m_ThirdPersonVisibleItem = thirdPersonPerspectiveItem.Object;
                if (m_ThirdPersonVisibleItem != null) {
                    var thirdPersonAnimator = m_ThirdPersonVisibleItem.GetComponent<Animator>();
                    if (thirdPersonAnimator != null) {
                        m_ThirdPersonObjectAnimatorController = thirdPersonAnimator.runtimeAnimatorController;
                    }
                    var itemSlots = m_ThirdPersonVisibleItem.GetComponentsInChildren<CharacterItemSlot>();
                    for (int i = 0; i < itemSlots.Length; ++i) {
                        if (itemSlots[i].ID == m_SlotID) {
                            m_ThirdPersonParent = itemSlots[i].gameObject;
                            m_ThirdPersonCharacterItemSlot = itemSlots[i];
                            break;
                        }
                    }
                }
            }

            // Item Actions. Sort by ID.
            var itemActions = m_Item.GetComponents<CharacterItemAction>();
            System.Array.Sort(itemActions, CompareActions);
            m_ActionTypes = new ItemBuilder.ActionInfo[itemActions.Length];
            for (int i = 0; i < m_ActionTypes.Length; ++i) {
                var actionType = GetActionType(itemActions[i]);
                m_ActionTypes[i] = new ItemBuilder.ActionInfo() { Type = actionType, Name = itemActions[i].ActionName };
            }
        }

        /// <summary>
        /// Compares two actions for sorting.
        /// </summary>
        /// <param name="a1">The first action.</param>
        /// <param name="a2">The second action.</param>
        /// <returns>The comparison between the first and second action ID.</returns>
        private static int CompareActions(CharacterItemAction a1, CharacterItemAction a2)
        {
            return a1.ID - a2.ID;
        }

        /// <summary>
        /// Returns the ItemBuilder.ActionType based on the object type.
        /// </summary>
        /// <param name="itemAction">A reference to the CharacterItemAction.</param>
        /// <returns>The ItemBuilder.ActionType based on the object type.</returns>
        private ItemBuilder.ActionType GetActionType(CharacterItemAction itemAction)
        {
            var type = itemAction.GetType();
            if (type == typeof(ShootableAction)) {
                return ItemBuilder.ActionType.Shootable;
            } else if (type == typeof(MeleeAction)) {
                return ItemBuilder.ActionType.Melee;
            } else if (type == typeof(MagicAction)) {
                return ItemBuilder.ActionType.Magic;
            } else if (type == typeof(ThrowableAction)) {
                return ItemBuilder.ActionType.Throwable;
            } else if (type == typeof(ShieldAction)) {
                return ItemBuilder.ActionType.Shield;
            } else { // Usable.
                return ItemBuilder.ActionType.Usable;
            }
        }

#if FIRST_PERSON_CONTROLLER
        /// <summary>
        /// Shows the first person options.
        /// </summary>
        private bool ShowFirstPersonObjects(VisualElement baseContainer, VisualElement container, bool canBuild)
        {
            var firstPersonBaseField = new ObjectField("First Person Base");
            firstPersonBaseField.tooltip = "A reference to the base object that should be used by the first person item. This will usually be the character’s separated arms. " +
                                           "A single object can be used for multiple Items. If a First Person Visible Item is specified this object should be within the scene so the Item Slot can be specified.";
            firstPersonBaseField.objectType = typeof(GameObject);
            firstPersonBaseField.value = m_FirstPersonBase;
            firstPersonBaseField.RegisterValueChangedCallback(c =>
            {
                m_FirstPersonBase = (GameObject)c.newValue;
                if (m_FirstPersonBase != null && (m_FirstPersonCharacterItemSlot == null || !m_FirstPersonCharacterItemSlot.transform.IsChildOf(m_FirstPersonBase.transform))) {
                    // Find any existing Item components.
                    var itemSlots = m_FirstPersonBase.GetComponentsInChildren<CharacterItemSlot>();
                    for (int i = 0; i < itemSlots.Length; ++i) {
                        if (itemSlots[i].ID == m_SlotID) {
                            m_FirstPersonParent = itemSlots[i].gameObject;
                            m_FirstPersonCharacterItemSlot = itemSlots[i];
                            break;
                        }
                    }
                }
                ShowItem(baseContainer);
            });
            container.Add(firstPersonBaseField);
            if (m_Character != null && m_FirstPersonBase == null) {
                ShowHelpBox(container, "A First Person Base Object is required.", canBuild);
                canBuild = !m_AddFirstPersonPerspective && canBuild;
            } else if (m_FirstPersonBase != null) {
                if (EditorUtility.IsPersistent(m_FirstPersonBase) && (m_Item == null || (m_Item.GetComponent<CharacterItem>() != null && !m_FirstPersonBase.transform.IsChildOf(m_Item.transform)))) {
                    ShowHelpBox(container, "Please drag your First Person Base Object into the scene. The Item Manager cannot add components to prefabs.", canBuild);
                    canBuild = !m_AddFirstPersonPerspective && canBuild;
                } else {
                    if (m_FirstPersonBase.GetComponent<Character.UltimateCharacterLocomotion>() != null) {
                        ShowHelpBox(container, "The First Person Base Object cannot be a created character.", canBuild);
                        canBuild = !m_AddFirstPersonPerspective && canBuild;
                    } else {
                        Animator animator;
                        if ((animator = m_FirstPersonBase.GetComponent<Animator>()) == null || animator.runtimeAnimatorController == null) {
                            var animatorField = new ObjectField("Animator Controller");
                            animatorField.objectType = typeof(RuntimeAnimatorController);
                            animatorField.value = m_FirstPersonBaseAnimatorController;
                            animatorField.tooltip = "A reference to the Animator Controller used by the First Person Base field. This Animator Controller will only be active when the Item is equipped.";
                            animatorField.Q<Label>().AddToClassList("indent");
                            animatorField.RegisterValueChangedCallback(c =>
                            {
                                m_FirstPersonBaseAnimatorController = (RuntimeAnimatorController)c.newValue;
                            });
                            container.Add(animatorField);
                        }
                    }
                }
            }

            var firstPersonVisibleItemField = new ObjectField("First Person Visible Item");
            firstPersonVisibleItemField.tooltip = "Specifies the Item object that is actually visible and rendered to the screen, such as the assault rifle or sword. " +
                                                  "This field should be left blank if you are adding an item that is part of the character’s body such as a fist for punching.";
            firstPersonVisibleItemField.objectType = typeof(GameObject);
            firstPersonVisibleItemField.value = m_FirstPersonVisibleItem;
            firstPersonVisibleItemField.RegisterValueChangedCallback(c =>
            {
                if (m_FirstPersonVisibleItem == (GameObject)c.newValue) {
                    return;
                }

                m_FirstPersonVisibleItem = (GameObject)c.newValue;

                // Preselect the parent if the first person object is not null.
                if (m_FirstPersonVisibleItem != null && m_FirstPersonBase != null) {
                    var itemSlots = m_FirstPersonBase.GetComponentsInChildren<CharacterItemSlot>();
                    if (itemSlots.Length > 0) {
                        var itemSlot = m_ThirdPersonCharacterItemSlot;
                        var defaultItemSlotIndex = itemSlot != null ? itemSlot.ID : 0;
                        itemSlot = itemSlots[0];
                        for (int i = 1; i < itemSlots.Length; ++i) {
                            if (itemSlots[i].ID == defaultItemSlotIndex) {
                                itemSlot = itemSlots[i];
                                break;
                            }
                        }
                        m_FirstPersonParent = itemSlot.gameObject;
                    }
                }
                ShowItem(baseContainer);
            });
            container.Add(firstPersonVisibleItemField);

            if ((m_Character != null && m_FirstPersonBase == null && m_FirstPersonVisibleItem != null) ||
                (m_FirstPersonBase != null && m_FirstPersonVisibleItem != null && !m_FirstPersonVisibleItem.transform.IsChildOf(m_FirstPersonBase.transform))) {
                var itemParentContainer = new VisualElement();
                itemParentContainer.AddToClassList("horizontal-layout");
                container.Add(itemParentContainer);

                var invalidItemSlot = false;
                var invalidParent = false;
                var itemParentField = new ObjectField("Item Parent");
                itemParentField.tooltip = "Specifies the object that the First Person Visible Item should be parented to. This GameObject must have the ItemSlot component.";
                itemParentField.objectType = typeof(GameObject);
                itemParentField.value = m_FirstPersonParent;
                itemParentField.RegisterValueChangedCallback(c =>
                {
                    m_FirstPersonParent = (GameObject)c.newValue;
                    if (m_FirstPersonParent != null) {
                        var itemSlot = m_FirstPersonParent.GetComponentInChildren<CharacterItemSlot>();
                        if (itemSlot != null) {
                            m_FirstPersonParent = itemSlot.gameObject;
                        }
                    }
                    ShowItem(baseContainer);
                });
                itemParentField.Q<Label>().AddToClassList("indent");
                itemParentField.style.flexGrow = 1;
                itemParentContainer.Add(itemParentField);

                if (m_FirstPersonParent == null) {
                    invalidItemSlot = true;
                } else {
                    // The First Person Parent should be a child of the FirstPersonObjects component.
                    if ((m_FirstPersonBase == null && m_FirstPersonParent.GetComponentInParent<FirstPersonController.Character.FirstPersonObjects>() == null) ||
                        m_FirstPersonBase != null && !m_FirstPersonParent.transform.IsChildOf(m_FirstPersonBase.transform)) {
                        invalidItemSlot = true;
                        invalidParent = m_FirstPersonBase != null;
                    } else {
                        m_FirstPersonCharacterItemSlot = m_FirstPersonParent.GetComponent<CharacterItemSlot>();
                        if (m_FirstPersonCharacterItemSlot == null) {
                            // Allow for some leeway if there is only one child ItemSlot component.
                            var itemSlots = m_FirstPersonParent.GetComponentsInChildren<CharacterItemSlot>();
                            if (itemSlots.Length == 1) {
                                m_FirstPersonParent = itemSlots[0].gameObject;
                            } else {
                                invalidItemSlot = true;
                                // Allow the ItemSlot to be added.
                                var itemSlotButton = new Button();
                                itemSlotButton.text = "Add ItemSlot";
                                itemParentContainer.Add(itemSlotButton);
                                itemSlotButton.clicked += () =>
                                {
                                    m_FirstPersonParent = AddItemSlot(m_Character != null ? m_Character : m_FirstPersonBase, m_FirstPersonParent.transform, true);
                                    ShowItem(baseContainer);
                                };
                            }
                        }
                    }
                }
                if (invalidItemSlot) {
                    ShowHelpBox(container, "The first person Item Parent field does not specify a valid ItemSlot GameObject." + (invalidParent ?
                                            " Ensure the Item Parent is a child of the First Person Base." : string.Empty), canBuild);
                    canBuild = !m_AddFirstPersonPerspective && canBuild;
                }
            }

            if (m_FirstPersonVisibleItem != null) {
                var animatorField = new ObjectField("Animator Controller");
                animatorField.objectType = typeof(RuntimeAnimatorController);
                animatorField.value = m_FirstPersonVisibleItemAnimatorController;
                animatorField.tooltip = "Specifies the Animator Controller that should be used by the First Person Visible Item.";
                animatorField.Q<Label>().AddToClassList("indent");
                animatorField.RegisterValueChangedCallback(c =>
                {
                    m_FirstPersonVisibleItemAnimatorController = (RuntimeAnimatorController)c.newValue;
                });
                container.Add(animatorField);
            }

            // The visible item should not have the Item component.
            if (m_FirstPersonVisibleItem != null && m_FirstPersonVisibleItem.GetComponent<CharacterItem>()) {
                ShowHelpBox(container, "The visible item should not be an already created item. The visible item should be a model representing the item.", canBuild);
                canBuild = !m_AddFirstPersonPerspective && canBuild;
            }

            return canBuild;
        }
#endif

        /// <summary>
        /// Shows the third person options.
        /// </summary>
        private bool ShowThirdPersonObjects(VisualElement baseContainer, VisualElement container, bool canBuild)
        {
            var thirdPersonVisibleItemField = new ObjectField("Third Person Visible Item");
            thirdPersonVisibleItemField.tooltip = "Specifies the third person item object. " +
                                                  "This is the object that will be visible and rendered to the screen, such as the assault rifle or sword.";
            thirdPersonVisibleItemField.objectType = typeof(GameObject);
            thirdPersonVisibleItemField.value = m_ThirdPersonVisibleItem;
            thirdPersonVisibleItemField.RegisterValueChangedCallback(c =>
            {
                if (m_ThirdPersonVisibleItem == (GameObject)c.newValue) {
                    return;
                }

                m_ThirdPersonVisibleItem = (GameObject)c.newValue;

                if (m_Character != null) {
                    // Setup the default ItemSlot to be the same ID as the first person perspective.
                    var defaultItemSlotIndex = m_FirstPersonCharacterItemSlot != null ? m_FirstPersonCharacterItemSlot.ID : 0;
                    var itemSlots = m_Character.GetComponentsInChildren<CharacterItemSlot>();
                    var animatorMonitor = m_Character.GetComponentInChildren<Character.AnimatorMonitor>();
                    Animator animator = null;
                    if (animatorMonitor != null) {
                        animator = animatorMonitor.GetComponent<Animator>();
                    }
                    for (int i = 0; i < itemSlots.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                        if (itemSlots[i].GetComponentInParent<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>() != null) {
                            continue;
                        }
#endif
                        if (itemSlots[i].ID == defaultItemSlotIndex) {
                            m_ThirdPersonCharacterItemSlot = itemSlots[i];
                            m_ThirdPersonParent = m_ThirdPersonCharacterItemSlot.gameObject;

                            if (animator != null && animator.GetBoneTransform(HumanBodyBones.Head) != null) {
                                if (m_ThirdPersonParent.transform.IsChildOf(animator.GetBoneTransform(HumanBodyBones.RightHand))) {
                                    m_ThirdHumanoidParentHand = ThirdPersonHumanoidParentHand.Right;
                                } else {
                                    m_ThirdHumanoidParentHand = ThirdPersonHumanoidParentHand.Left;
                                }
                            }
                            break;
                        }
                    }
                }
                ShowItem(baseContainer);
            });
            container.Add(thirdPersonVisibleItemField);

            if (m_ThirdPersonVisibleItem != null) {
                // The object should not have the Item component.
                if (m_ThirdPersonVisibleItem.GetComponent<CharacterItem>()) {
                    ShowHelpBox(container, "The object should not be an already created item. The visible item should be a model representing the item.", canBuild);
                    canBuild = !m_AddThirdPersonPerspective && canBuild;
                } else {
                    if (m_Character != null) {
                        var invalidItemSlot = false;
                        var animatorMonitor = m_Character.GetComponentInChildren<Character.AnimatorMonitor>();
                        Animator animator = null;
                        if (animatorMonitor != null) {
                            animator = animatorMonitor.GetComponent<Animator>();
                        }

                        var itemParentContainer = new VisualElement();
                        itemParentContainer.AddToClassList("horizontal-layout");
                        container.Add(itemParentContainer);

                        // Show a dropdown for the humanoid characters.
                        if (animator != null && animator.GetBoneTransform(HumanBodyBones.Head) != null) {
                            if (m_ThirdPersonParent == null) {
                                var handTransform = animator.GetBoneTransform(m_ThirdHumanoidParentHand == ThirdPersonHumanoidParentHand.Right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
                                var itemSlot = handTransform.GetComponentInChildren<CharacterItemSlot>();
                                m_ThirdPersonParent = itemSlot != null ? itemSlot.gameObject : null;
                            }
                            var handPopup = new EnumField("Hand", m_ThirdHumanoidParentHand);
                            handPopup.RegisterValueChangedCallback(c =>
                            {
                                if (m_ThirdHumanoidParentHand == (ThirdPersonHumanoidParentHand)c.newValue) {
                                    return;
                                }
                                m_ThirdHumanoidParentHand = (ThirdPersonHumanoidParentHand)c.newValue;
                                var handTransform = animator.GetBoneTransform(m_ThirdHumanoidParentHand == ThirdPersonHumanoidParentHand.Right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
                                var itemSlot = handTransform.GetComponentInChildren<CharacterItemSlot>();
                                m_ThirdPersonParent = itemSlot != null ? itemSlot.gameObject : null;
                                ShowItem(baseContainer);
                            });
                            handPopup.Q<Label>().AddToClassList("indent");
                            container.Add(handPopup);
                        } else {
                            var itemParentField = new ObjectField("Item Parent");
                            itemParentField.tooltip = "Specifies the object that the Third Person Visible Item should be parented to. This GameObject must have the ItemSlot component.";
                            itemParentField.objectType = typeof(GameObject);
                            itemParentField.value = m_ThirdPersonParent;
                            itemParentField.RegisterValueChangedCallback(c =>
                            {
                                m_ThirdPersonParent = (GameObject)c.newValue;
                                if (m_ThirdPersonParent != null) {
                                    var itemSlot = m_ThirdPersonParent.GetComponentInChildren<CharacterItemSlot>();
                                    if (itemSlot != null) {
                                        m_ThirdPersonParent = itemSlot.gameObject;
                                    }
                                }
                                ShowItem(baseContainer);
                            });
                            itemParentField.style.flexGrow = 1;
                            itemParentField.Q<Label>().AddToClassList("indent");
                            itemParentContainer.Add(itemParentField);
                        }
                        if (m_ThirdPersonParent == null) {
                            invalidItemSlot = true;
                        } else {
#if FIRST_PERSON_CONTROLLER
                            // The Third Person Parent should not be a child of the FirstPersonObjects component.
                            if (m_ThirdPersonParent.GetComponentInParent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                                invalidItemSlot = true;
                            } else {
#endif
                                m_ThirdPersonCharacterItemSlot = m_ThirdPersonParent.GetComponent<CharacterItemSlot>();
                                if (m_ThirdPersonCharacterItemSlot == null) {
                                    // Allow for some leeway if there is only one child ItemSlot component.
                                    var itemSlots = m_ThirdPersonParent.GetComponentsInChildren<CharacterItemSlot>();
                                    if (itemSlots.Length == 1) {
                                        m_ThirdPersonParent = itemSlots[0].gameObject;
                                    } else {
                                        invalidItemSlot = true;
                                        // Allow the ItemSlot to be added.
                                        var itemSlotButton = new Button();
                                        itemSlotButton.text = "Add ItemSlot";
                                        itemParentContainer.Add(itemSlotButton);
                                        itemSlotButton.clicked += () =>
                                        {
                                            m_ThirdPersonParent = AddItemSlot(m_Character, m_ThirdPersonParent.transform, false);
                                            ShowItem(baseContainer);
                                        };
                                    }
                                }
#if FIRST_PERSON_CONTROLLER
                            }
#endif
                        }
                        if (invalidItemSlot) {
                            ShowHelpBox(container, "The third person Item Parent field does not specify a valid ItemSlot GameObject.", canBuild);
                            canBuild = !m_AddThirdPersonPerspective && canBuild;
                        }
                    }
                }
            }

            if (m_ThirdPersonVisibleItem != null) {
                var animatorField = new ObjectField("Animator Controller");
                animatorField.objectType = typeof(RuntimeAnimatorController);
                animatorField.value = m_ThirdPersonObjectAnimatorController;
                animatorField.tooltip = "Specifies the Animator Controller that should be used by the Third Person Object.";
                animatorField.Q<Label>().AddToClassList("indent");
                animatorField.RegisterValueChangedCallback(c =>
                {
                    m_ThirdPersonObjectAnimatorController = (RuntimeAnimatorController)c.newValue;
                });
                container.Add(animatorField);
            }

            // The visible item should not have the Item component.
            if (m_ThirdPersonVisibleItem != null && m_ThirdPersonVisibleItem.GetComponent<CharacterItem>()) {
                ShowHelpBox(container, "The object should not be an already created item. The object should be a model representing the item.", canBuild);
                canBuild = !m_AddThirdPersonPerspective && canBuild;
            }

            return canBuild;
        }

        /// <summary>
        /// Adds an ItemSlot child GameObject to the specified parent.
        /// </summary>
        /// <param name="baseParent">The object that is adding the item slot.</param>
        /// <param name="itemParent">The object to add the ItemSlot to.</param>
        /// <param name="firstPerson">Should a first person ItemSlot be added?</param>
        /// <returns>The added the ItemSlot GameObject (can be null).</returns>
        private GameObject AddItemSlot(GameObject baseParent, Transform itemParent, bool firstPerson)
        {
            // The new ItemSlot's ID should be unique.
            var allItemSlots = baseParent.GetComponentsInChildren<CharacterItemSlot>();
            var maxID = -1;
#if FIRST_PERSON_CONTROLLER
            var firstPersonObjects = baseParent.GetComponentInChildren<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>();
#endif
            for (int i = 0; i < allItemSlots.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                // The ItemSlot must match the perspective.
                if (firstPersonObjects != null && (allItemSlots[i].transform.IsChildOf(firstPersonObjects.transform) != firstPerson)) {
                    continue;
                }
#endif
                if (allItemSlots[i].ID > maxID) {
                    maxID = allItemSlots[i].ID;
                }
            }
            // Setup the new ItemSlot.
            var itemSlotGameObject = new GameObject("Items", new System.Type[] { typeof(CharacterItemSlot) });
            itemSlotGameObject.transform.SetParentOrigin(itemParent);
            var itemSlot = itemSlotGameObject.GetComponent<CharacterItemSlot>();
            // The new ID should be one greater than the previous max ID.
            itemSlot.ID = maxID + 1;
            return itemSlotGameObject;
        }

        /// <summary>
        /// Builds a new item.
        /// </summary>
        private void BuildItem()
        {
            var item = ItemBuilder.BuildItem(m_Name, m_ItemDefinition, m_AnimatorItemID, m_Character, m_SlotID, m_AddToDefaultLoadout, m_AddFirstPersonPerspective, m_FirstPersonBase, m_FirstPersonBaseAnimatorController,
                m_FirstPersonVisibleItem, m_FirstPersonCharacterItemSlot, m_FirstPersonVisibleItemAnimatorController, m_AddThirdPersonPerspective, m_ThirdPersonVisibleItem, m_ThirdPersonCharacterItemSlot, m_ThirdPersonObjectAnimatorController,
                m_InvisibleShadowCaster, m_ActionTemplate == null ? m_ActionTypes : null);

            var conflictingObjects = AddTemplateActions(item, () => { BuildItemComplete(item); });

            if (m_AddFirstPersonPerspective && !m_AddThirdPersonPerspective) {
                // First person items should not use animation events for equip/unequip.
                var createdItem = item.GetComponent<CharacterItem>();
                createdItem.EquipEvent = new AnimationSlotEventTrigger(false, 0);
                createdItem.EquipCompleteEvent = new AnimationSlotEventTrigger(false, 0.3f);
                createdItem.UnequipEvent = new AnimationSlotEventTrigger(false, 0.3f);
                Shared.Editor.Utility.EditorUtility.SetDirty(createdItem);
            } else if (m_AddFirstPersonPerspective && m_AddThirdPersonPerspective) {
                // A first and third person item is being created. Add a new state which has the correct first person properties.
                var preset = Shared.Editor.Utility.EditorUtility.LoadAsset<PersistablePreset>("50a5f74ba80091b47954d1f678ac7823");
                if (preset != null) {
                    var createdItem = item.GetComponent<CharacterItem>();
                    var states = createdItem.States;
                    System.Array.Resize(ref states, states.Length + 1);
                    // Default must always be at the end.
                    states[states.Length - 1] = states[0];
                    states[0] = new State("FirstPerson", preset, null);
                    createdItem.States = states;
                    Shared.Editor.Utility.EditorUtility.SetDirty(createdItem);
                }
            }

            // Ensure the animators have the required parameters.
            if (m_FirstPersonBaseAnimatorController != null) {
                AnimatorBuilder.AddParameters((UnityEditor.Animations.AnimatorController)m_FirstPersonBaseAnimatorController);
            }
            if (m_FirstPersonVisibleItemAnimatorController != null) {
                AnimatorBuilder.AddParameters((UnityEditor.Animations.AnimatorController)m_FirstPersonVisibleItemAnimatorController);
            }
            if (m_ThirdPersonObjectAnimatorController != null) {
                AnimatorBuilder.AddParameters((UnityEditor.Animations.AnimatorController)m_ThirdPersonObjectAnimatorController);
            }

            if (!conflictingObjects) {
                BuildItemComplete(item);
            }
        }

        /// <summary>
        /// The item has been built.
        /// </summary>
        /// <param name="item">The item that has been built.</param>
        private void BuildItemComplete(GameObject item)
        {
            // If the character is null then a prefab will be created.
            if (m_Character == null) {
                var success = false;
                var path = EditorUtility.SaveFilePanel("Save Item", "Assets", m_Name + ".prefab", "prefab");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                    success = true;
                    var relativePath = path.Replace(Application.dataPath, "");
                    Selection.activeGameObject = PrefabUtility.SaveAsPrefabAsset(item, "Assets" + relativePath);

                    // The prefab can be added to the ItemType.
                    if (m_AddToItemTypePrefab && m_ItemDefinition is ItemType) {
                        var itemType = m_ItemDefinition as ItemType;
                        if (itemType.Prefabs == null) {
                            itemType.Prefabs = new GameObject[] { Selection.activeGameObject };
                        } else {
                            var prefabs = itemType.Prefabs;
                            System.Array.Resize(ref prefabs, prefabs.Length + 1);
                            prefabs[prefabs.Length - 1] = Selection.activeGameObject;
                            itemType.Prefabs = prefabs;
                        }
                        Shared.Editor.Utility.EditorUtility.SetDirty(itemType);
                    }
                }
                Object.DestroyImmediate(item, true);
                if (success) {
                    item = Selection.activeGameObject;
                } else {
                    return;
                }
            } else {
                // Select the newly added item.
                Selection.activeGameObject = item;
            }

            // Remove the original objects if they are in the scene - this will prevent duplicate objects from existing.
            if (m_FirstPersonVisibleItem != null && !EditorUtility.IsPersistent(m_FirstPersonVisibleItem) &&
                (m_Character == null || !m_FirstPersonVisibleItem.transform.IsChildOf(m_Character.transform))) {
                Object.DestroyImmediate(m_FirstPersonVisibleItem, true);
                m_FirstPersonVisibleItem = null;
            }
            if (m_FirstPersonBase != null && !EditorUtility.IsPersistent(m_FirstPersonBase) &&
                (m_Character == null || !m_FirstPersonBase.transform.IsChildOf(m_Character.transform))) {
                Object.DestroyImmediate(m_FirstPersonBase, true);
                m_FirstPersonBase = null;
            }
            if (m_ThirdPersonVisibleItem != null && !EditorUtility.IsPersistent(m_ThirdPersonVisibleItem) &&
                (m_Character == null || !m_ThirdPersonVisibleItem.transform.IsChildOf(m_Character.transform))) {
                Object.DestroyImmediate(m_ThirdPersonVisibleItem, true);
                m_ThirdPersonVisibleItem = null;
            }

            // Select the new item.
            m_Item = item;
            OnItemChange(false);
        }

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        private void UpdateItem()
        {
            if (m_Name != m_Item.name) {
                m_Item.name = m_Name;
            }

            var characterItem = m_Item.GetComponent<CharacterItem>();
            if (m_ItemDefinition != characterItem.ItemDefinition) {
                characterItem.ItemDefinition = m_ItemDefinition;
            }

            if (m_AnimatorItemID != characterItem.AnimatorItemID) {
                characterItem.AnimatorItemID = m_AnimatorItemID;
            }

            AddTemplateActions(m_Item);

            // The action template will add the required actions.
            if (m_ActionTemplate != null) {
                return;
            }

            var existingActions = m_Item.GetComponents<CharacterItemAction>();
            System.Array.Sort(existingActions, CompareActions);
            for (int i = 0; i < existingActions.Length; ++i) {
                if (m_ActionTypes.Length < i + 1) {
                    break;
                }
                var actionType = GetActionType(existingActions[i]);
                if (actionType != m_ActionTypes[i].Type) {
                    Object.DestroyImmediate(existingActions[i], true);
                    ItemBuilder.AddAction(m_Item.gameObject, actionType, m_ActionTypes[i].Name);
                    continue;
                }

                if (existingActions[i].ActionName != m_ActionTypes[i].Name) {
                    existingActions[i].ActionName = m_ActionTypes[i].Name;
                }
            }

            if (existingActions.Length > m_ActionTypes.Length) {
                for (int i = existingActions.Length - 1; i >= m_ActionTypes.Length; --i) {
                    existingActions[i].RemoveAllModules();
                    Object.DestroyImmediate(existingActions[i], true);
                }
            } else if (m_ActionTypes.Length > existingActions.Length) {
                for (int i = existingActions.Length; i < m_ActionTypes.Length; ++i) {
                    ItemBuilder.AddAction(m_Item.gameObject, m_ActionTypes[i].Type, m_ActionTypes[i].Name);
                }
            }
        }

        /// <summary>
        /// Adds the actions from the template object to the item.
        /// </summary>
        /// <param name="item">A reference to the item GameObject.</param>
        /// <param name="onClose">Callback when the window has closed.</param>
        /// <returns>True if the template actions were added.</returns>
        private bool AddTemplateActions(GameObject item, System.Action onClose = null)
        {
            if (m_ActionTemplate == null) {
                return false;
            }

            var existingActions = item.GetComponents<CharacterItemAction>();
            // Start fresh with all of the actions.
            for (int i = existingActions.Length - 1; i >= 0; --i) {
                Object.DestroyImmediate(existingActions[i], true);
            }

            var conflictingObjects = new List<ReferenceResolverWindow.ConflictingObjects>();
            var templateActions = m_ActionTemplate.GetComponents<CharacterItemAction>();
            System.Array.Sort(templateActions, CompareActions);
            for (int i = 0; i < templateActions.Length; ++i) {
                var addedAction = ItemBuilder.AddAction(item, GetActionType(templateActions[i]), templateActions[i].ActionName);
                addedAction.InitializeModuleGroups(false);
                templateActions[i].InitializeModuleGroups(false);
                for (int j = 0; j < templateActions[i].AllModuleGroups.Count; ++j) {
                    var templateGroup = templateActions[i].AllModuleGroups[j];
                    for (int k = 0; k < templateGroup.ModuleCount; ++k) {
                        var type = templateGroup.GetBaseModuleAt(k).GetType();
                        addedAction.AllModuleGroups[j].AddModule(System.Activator.CreateInstance(type, true) as Items.Actions.Modules.ActionModule);
                    }
                }
                ReferenceResolverWindow.ResolveFields(templateActions[i].gameObject, addedAction.gameObject, typeof(Animator), templateActions[i], addedAction, conflictingObjects);
            }

            if (conflictingObjects.Count > 0) {
                m_ReferenceResolverWindow = EditorWindow.GetWindow<ReferenceResolverWindow>(true, "Reference Resolver");
                m_ReferenceResolverWindow.minSize = m_ReferenceResolverWindow.maxSize = new Vector2(600, 500);
                m_ReferenceResolverWindow.Initialize(conflictingObjects, m_ActionTemplate.gameObject, "item", onClose);
                return true;
            }
            return false;
        }
    }
}