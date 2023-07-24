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
    using Opsive.UltimateCharacterController.Inventory;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The ItemTypeManager will draw any ItemType properties
    /// </summary>
    [Serializable]
    [OrderedEditorItem("Item Types", 3)]
    public class ItemTypeManager : Manager
    {
        // Specifies the height of the row.
        private const float c_RowHeight = 28;
        // Specifies the height of the selected ItemType row.
        private const float c_SelectedItemTypeRowHeight = 112;
        // Specifies the height for each addition prefab element.
        private const float c_PrefabRowHeight = 20;
        // Specifies the height of the selected category row.
        private const float c_SelectedCategoryRowHeight = 48;

        private string[] m_ToolbarStrings = { "Item Types", "Categories" };
        [SerializeField] private ItemCollection m_ItemCollection;
        [SerializeField] private bool m_DrawItemTypes = true;
        [SerializeField] private string m_CategoryName; 
        [SerializeField] private string m_ItemTypeName;

        private ReorderableList m_ReorderableList;
        private int m_SelectedItemType;
        private int m_SelectedCategory;

        /// <summary>
        /// Initializes the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            // Look for the ItemCollection within the scene if it isn't already populated.
            m_ItemCollection = ManagerUtility.FindItemCollection(m_MainManagerWindow);

            // The ItemCollection may have been serialized.
            if (m_ItemCollection != null) {
                // The category may be invalid.
                var categories = m_ItemCollection.Categories;
                if (categories != null) {
                    for (int i = categories.Length - 1; i > -1; --i) {
                        if (categories[i] != null) {
                            continue;
                        }
                        ArrayUtility.RemoveAt(ref categories, i);
                    }
                    m_ItemCollection.Categories = categories;
                }
                m_SelectedCategory = m_SelectedItemType = 0;
            }
        }

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var itemTypesContainer = new VisualElement();
            var categoriesContainer = new VisualElement();
            var tabToolbar = new TabToolbar(m_ToolbarStrings, m_DrawItemTypes ? 0 : 1, (int selected) =>
            {
                m_DrawItemTypes = selected == 0;
                if (m_DrawItemTypes) {
                    categoriesContainer.Clear();
                    ShowItemTypes(itemTypesContainer);
                } else {
                    itemTypesContainer.Clear();
                    ShowCategories(categoriesContainer);
                }
            }, true);
            m_ManagerContentContainer.Add(tabToolbar);

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.flexGrow = 0;
            horizontalLayout.style.flexShrink = 0;
            m_ManagerContentContainer.Add(horizontalLayout);

            // The ItemCollection field will show for both ItemTypes and Categories.
            var itemCollectionField = new ObjectField("Item Collection");
            itemCollectionField.objectType = typeof(ItemCollection);
            itemCollectionField.value = m_ItemCollection;
            itemCollectionField.allowSceneObjects = false;
            itemCollectionField.AddToClassList("flex-grow");
            itemCollectionField.RegisterValueChangedCallback(c =>
            {
                m_ItemCollection = (ItemCollection)c.newValue;

                if (m_ItemCollection != null) {
                    EditorPrefs.SetString(ManagerUtility.LastItemCollectionGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_ItemCollection)));
                }

                if (m_DrawItemTypes) {
                    ShowItemTypes(itemTypesContainer);
                } else {
                    ShowCategories(categoriesContainer);
                }
            });
            horizontalLayout.Add(itemCollectionField);

            var createItemCollectionButton = new Button();
            createItemCollectionButton.text = "Create";
            createItemCollectionButton.style.width = 75;
            createItemCollectionButton.clicked += () =>
            {
                var path = EditorUtility.SaveFilePanel("Save Item Collection", "Assets", "ItemCollection.asset", "asset");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                    m_ItemCollection = ScriptableObject.CreateInstance<ItemCollection>();
                    var category = Category.Create("Items");
                    m_ItemCollection.Categories = new Category[] { category };

                    // Save the collection.
                    path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.CreateAsset(m_ItemCollection, path);
                    AssetDatabase.AddObjectToAsset(category, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                    EditorPrefs.SetString(ManagerUtility.LastItemCollectionGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_ItemCollection)));

                    // Add an item set rule at the same time to help new users setup.
                    var itemSetRule = ScriptableObject.CreateInstance<IndividualItemSetRule>();

                    var index = path.LastIndexOf('/');
                    var fileName = path[(index + 1)..];
                    if (fileName.Contains("collection")) {
                        fileName = fileName.Replace("collection", "rule");
                    } else if (fileName.Contains("Collection")) {
                        fileName = fileName.Replace("Collection", "Rule");
                    } else {
                        fileName = fileName.Replace(".asset", "Rule.asset");
                    }
                    
                    path = path[..index]+"/"+fileName;
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.CreateAsset(itemSetRule, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                    EditorPrefs.SetString(ManagerUtility.LastItemSetRuleGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(itemSetRule)));

                    itemCollectionField.value = m_ItemCollection;

                    m_SelectedCategory = m_SelectedItemType = 0;
                    if (m_DrawItemTypes) {
                        ShowItemTypes(itemTypesContainer);
                    } else {
                        ShowCategories(categoriesContainer);
                    }
                }
            };
            horizontalLayout.Add(createItemCollectionButton);

            m_ManagerContentContainer.Add(itemTypesContainer);
            m_ManagerContentContainer.Add(categoriesContainer);

            if (m_DrawItemTypes) {
                ShowItemTypes(itemTypesContainer);
            } else {
                ShowCategories(categoriesContainer);
            }
        }

        /// <summary>
        /// Shows all of the ItemTypes.
        /// </summary>
        private void ShowItemTypes(VisualElement container)
        {
            container.Clear();

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.flexShrink = 0;
            horizontalLayout.SetEnabled(m_ItemCollection != null);
            container.Add(horizontalLayout);

            // New ItemTypes can be added.
            var addItemTypeNameField = new TextField("Name");
            var addItemTypeButton = new Button();
            addItemTypeNameField.value = m_ItemTypeName;
            addItemTypeNameField.AddToClassList("flex-grow");
            addItemTypeNameField.RegisterValueChangedCallback(c =>
            {
                m_ItemTypeName = c.newValue;
                addItemTypeButton.SetEnabled(IsUniqueItemTypeName(m_ItemCollection, m_ItemTypeName));

            });
            horizontalLayout.Add(addItemTypeNameField);

            addItemTypeButton.text = "Add";
            addItemTypeButton.style.width = 75;
            addItemTypeButton.SetEnabled(IsUniqueItemTypeName(m_ItemCollection, m_ItemTypeName));
            addItemTypeButton.clicked += () =>
            {
                AddItemType(m_ItemCollection, m_ItemTypeName);
                m_ReorderableList.ItemsSource = m_ItemCollection.ItemTypes;
                m_SelectedItemType = m_ReorderableList.SelectedIndex = m_ItemCollection.ItemTypes.Length - 1;

                // Reset.
                EditorUtility.SetDirty(m_ItemCollection);
                m_ItemTypeName = string.Empty;
                addItemTypeNameField.SetValueWithoutNotify(m_ItemTypeName);
                addItemTypeButton.SetEnabled(false);
                GUI.FocusControl("");
            };
            horizontalLayout.Add(addItemTypeButton);

            if (m_ItemCollection == null) {
                // ItemCollection must be populated in order to create Categories/ItemTypes.
                var helpBox = new HelpBox("An ItemCollection must be selected. Use the \"Create\" button to create a new collection.", HelpBoxMessageType.Error);
                container.Add(helpBox);
                return;
            }

            // The ReorderabeList is responsible for showing all of the ItemTypes.
            m_ReorderableList = new ReorderableList(m_ItemCollection.ItemTypes,
            (VisualElement element, int index) => // Make Item.
            {
                element.Add(new ItemTypeRowElement(m_ReorderableList, m_ItemCollection));
            }, (VisualElement element, int index) => // Bind Item.
            {
                var itemTypeElement = element.Q<ItemTypeRowElement>();
                itemTypeElement.BindItem(index, m_ReorderableList.SelectedIndex == index);
            }, null, (int index) => // Element Height.
            {
                if (index == m_ReorderableList.SelectedIndex) {
                    return c_SelectedItemTypeRowHeight +
                    (m_ItemCollection.ItemTypes[index].Prefabs != null ? m_ItemCollection.ItemTypes[index].Prefabs.Length * c_PrefabRowHeight : 0);
                }
                return c_RowHeight;
            }, (int index) => // Selection.
            {
                m_SelectedItemType = index;
                m_ReorderableList.Refresh();
            }, null, null, null);
            m_ReorderableList.HighlightSelectedItem = false;
            if (m_ReorderableList.ListItems != null && m_ReorderableList.ListItems.Count > m_SelectedCategory) {
                m_ReorderableList.SelectedIndex = m_SelectedItemType;
            }
            container.Add(m_ReorderableList);
        }

        /// <summary>
        /// Adds the ItemType with the specified name to the ItemCollection.
        /// </summary>
        /// <param name="itemCollection">A reference to the ItemCollection.</param>
        /// <param name="name">The name of the item.</param>
        /// <returns>The created ItemType.</returns>
        public static ItemType AddItemType(ItemCollection itemCollection, string name)
        {
            // Ensure the name is unique.
            if (!IsUniqueItemTypeName(itemCollection, name)) {
                // Generate a unique name for the item.
                var index = 1;
                var originalName = name;
                do {
                    name = $"{originalName} ({index})";
                    index++;
                } while (!ItemTypeManager.IsUniqueItemTypeName(itemCollection, name));
            }

            // Create the new ItemType.
            var itemType = ScriptableObject.CreateInstance<ItemType>();
            itemType.name = name;
            if (itemCollection.Categories != null && itemCollection.Categories.Length > 0) {
                itemType.CategoryIDs = new uint[] { itemCollection.Categories[0].ID };
            }

            // Add the ItemType to the ItemCollection.
            var itemTypes = itemCollection.ItemTypes;
            Array.Resize(ref itemTypes, itemTypes != null ? itemTypes.Length + 1 : 1);
            itemType.ID = (uint)itemTypes.Length - 1;
            itemTypes[itemTypes.Length - 1] = itemType;
            itemCollection.ItemTypes = itemTypes;
            AssetDatabase.AddObjectToAsset(itemType, itemCollection);
            AssetDatabase.SaveAssets();

            return itemType;
        }

        /// <summary>
        /// Refreshes the content for the current manager.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            m_ManagerContentContainer.Clear();
            var selectedIndex = 0;
            if (m_ReorderableList != null) {
                selectedIndex = m_ReorderableList.SelectedIndex;
            }
            BuildVisualElements();
            if (m_ReorderableList != null) {
                m_ReorderableList.SelectedIndex = selectedIndex;
            }
        }

        /// <summary>
        /// The Visual Element for each ItemType row in the ReorderableList.
        /// </summary>
        private class ItemTypeRowElement : VisualElement
        {
            private ReorderableList m_ReorderableList;
            private ItemCollection m_ItemCollection;

            private Label m_Title;
            private VisualElement m_SelectedContainer;
            private TextField m_NameField;
            private MaskField m_CategoryPopup;
            private IntegerField m_CapacityField;
            private VisualElement m_CharacterItemPrefabContainer;

            private int m_Index;

            /// <summary>
            /// Constructor.
            /// </summary>
            public ItemTypeRowElement(ReorderableList reorderableList, ItemCollection itemCollection)
            {
                m_ReorderableList = reorderableList;
                m_ItemCollection = itemCollection;
                name = "item-type-row-element";

                // Setup the initial layout.
                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                horizontalLayout.AddToClassList(EditorGUIUtility.isProSkin ? "item-type-row-element-header-dark" : "item-type-row-element-header-light");
                Add(horizontalLayout);

                m_Title = new Label();
                m_Title.style.unityFontStyleAndWeight = FontStyle.Bold;
                m_Title.style.unityTextAlign = TextAnchor.MiddleCenter;
                horizontalLayout.Add(m_Title);

                // Identify, duplicate, and remove buttons.
                var buttonLayout = new VisualElement();
                buttonLayout.AddToClassList("horizontal-layout");
                buttonLayout.style.flexDirection = FlexDirection.RowReverse;
                horizontalLayout.Add(buttonLayout);

                var removeButton = new Button();
                removeButton.AddToClassList(EditorGUIUtility.isProSkin ? "delete-dark-icon" : "delete-light-icon");
                removeButton.tooltip = "Remove";
                removeButton.clicked += () =>
                {
                    // Remove the ItemType.
                    var itemTypes = new List<ItemType>(m_ItemCollection.ItemTypes);
                    AssetDatabase.RemoveObjectFromAsset(m_ItemCollection.ItemTypes[m_Index]);
                    Undo.DestroyObjectImmediate(m_ItemCollection.ItemTypes[m_Index]);
                    itemTypes.RemoveAt(m_Index);
                    m_ReorderableList.ItemsSource = m_ItemCollection.ItemTypes = itemTypes.ToArray();
                    EditorUtility.SetDirty(m_ItemCollection);

                    // Update all of the ItemIDs.
                    for (int i = 0; i < itemTypes.Count; ++i) {
                        m_ItemCollection.ItemTypes[i].ID = (uint)i;
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                };
                buttonLayout.Add(removeButton);

                var duplicateButton = new Button();
                duplicateButton.AddToClassList(EditorGUIUtility.isProSkin ? "duplicate-dark-icon" : "duplicate-light-icon");
                duplicateButton.tooltip = "Duplicate";
                duplicateButton.clicked += () =>
                {
                    var itemType = m_ItemCollection.ItemTypes[m_Index];
                    var clonedItemType = UnityEngine.Object.Instantiate(itemType);
                    // Generate a unique name for the item.
                    var index = 1;
                    string name;
                    do {
                        name = itemType.name + " (" + index + ")";
                        index++;
                    } while (!ItemTypeManager.IsUniqueItemTypeName(m_ItemCollection, name));
                    clonedItemType.name = name;

                    // Add the ItemType to the ItemCollection.
                    var itemTypes = m_ItemCollection.ItemTypes;
                    Array.Resize(ref itemTypes, itemTypes.Length + 1);
                    clonedItemType.ID = (uint)itemTypes.Length - 1;
                    itemTypes[itemTypes.Length - 1] = clonedItemType;
                    m_ReorderableList.ItemsSource = m_ItemCollection.ItemTypes = itemTypes;
                    m_ReorderableList.SelectedIndex = m_ItemCollection.ItemTypes.Length - 1;
                    AssetDatabase.AddObjectToAsset(clonedItemType, m_ItemCollection);
                    EditorUtility.SetDirty(m_ItemCollection);
                    AssetDatabase.SaveAssets();
                };
                buttonLayout.Add(duplicateButton);

                var identifyButton = new Button();
                identifyButton.AddToClassList(EditorGUIUtility.isProSkin ? "info-dark-icon" : "info-light-icon");
                identifyButton.tooltip = "Identify";
                identifyButton.clicked += () =>
                {
                    Selection.activeObject = m_ItemCollection.ItemTypes[m_Index];
                    EditorGUIUtility.PingObject(Selection.activeObject);
                };
                buttonLayout.Add(identifyButton);

                m_SelectedContainer = new VisualElement();
                Add(m_SelectedContainer);

                // ItemTypes can update their names.
                m_NameField = new TextField();
                m_NameField.label = "Name";
                m_NameField.RegisterValueChangedCallback(c =>
                {
                    if (!ItemTypeManager.IsUniqueItemTypeName(m_ItemCollection, c.newValue)) {
                        m_NameField.SetValueWithoutNotify(c.previousValue);
                        return;
                    }

                    m_ItemCollection.ItemTypes[m_Index].name = c.newValue;
                    m_Title.text = c.newValue;
                    EditorUtility.SetDirty(m_ItemCollection.ItemTypes[m_Index]);
                });
                m_SelectedContainer.Add(m_NameField);

                // The category name can be changed. Get a list of all of the categories.
                var categoryNames = new List<string>();
                for (int i = 0; i < m_ItemCollection.Categories.Length; ++i) {
                    if (m_ItemCollection.Categories[i] == null) {
                        continue;
                    }
                    categoryNames.Add(m_ItemCollection.Categories[i].name);
                }

                m_CategoryPopup = new MaskField("Category", categoryNames, 0);
                m_CategoryPopup.label = "Category";
                m_CategoryPopup.RegisterValueChangedCallback(c =>
                {
                    var selectedIDs = new List<uint>();
                    for (int i = 0; i < categoryNames.Count; ++i) {
                        if ((c.newValue & (1 << i)) == (1 << i)) {
                            selectedIDs.Add(m_ItemCollection.Categories[i].ID);
                        }
                    }
                    m_ItemCollection.ItemTypes[m_Index].CategoryIDs = selectedIDs.ToArray();
                    EditorUtility.SetDirty(m_ItemCollection.ItemTypes[m_Index]);
                });
                m_SelectedContainer.Add(m_CategoryPopup);
                m_CapacityField = new IntegerField();
                m_CapacityField.label = "Capacity";
                m_CapacityField.RegisterValueChangedCallback(c =>
                {
                    m_ItemCollection.ItemTypes[m_Index].Capacity = c.newValue;
                    EditorUtility.SetDirty(m_ItemCollection.ItemTypes[m_Index]);
                });
                m_SelectedContainer.Add(m_CapacityField);

                m_CharacterItemPrefabContainer = new VisualElement();
                m_SelectedContainer.Add(m_CharacterItemPrefabContainer);
            }

            /// <summary>
            /// Binds the row to the ItemType element.
            /// </summary>
            public void BindItem(int index, bool selected)
            {
                m_Index = index;

                m_Title.text = m_ItemCollection.ItemTypes[index].name;

                m_SelectedContainer.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;

                // Shows the details for the ItemType.
                if (selected) {
                    m_NameField.SetValueWithoutNotify(m_ItemCollection.ItemTypes[index].name);

                    // Use a mask to determine what categories the ItemType belongs to.
                    var categoryMask = 0;
                    for (int i = 0; i < m_ItemCollection.Categories.Length; ++i) {
                        var categoryIDs = m_ItemCollection.ItemTypes[m_Index].CategoryIDs;
                        for (int j = 0; j < categoryIDs.Length; ++j) {
                            if (categoryIDs[j] == m_ItemCollection.Categories[i].ID) {
                                categoryMask |= 1 << i;
                            }
                        }
                    }
                    m_CategoryPopup.value = categoryMask;

                    m_CapacityField.SetValueWithoutNotify(m_ItemCollection.ItemTypes[index].Capacity);

                    // Create a special control that allows for the array to be visualized without the standard array control.
                    m_CharacterItemPrefabContainer.Clear();
                    var prefabs = m_ItemCollection.ItemTypes[m_Index].Prefabs;
                    for (int i = 0; i < (prefabs != null ? prefabs.Length : 0) + 1; ++i) {
                        var horizontalContainer = new VisualElement();
                        horizontalContainer.AddToClassList("horizontal-layout");
                        var prefabIndex = i;
                        var prefabsField = new ObjectField(i == 0 ? "Prefabs" : " ");
                        prefabsField.AddToClassList("flex-grow");
                        prefabsField.AddToClassList("flex-shrink");
                        prefabsField.allowSceneObjects = false;
                        prefabsField.objectType = typeof(GameObject);
                        prefabsField.value = (prefabs != null && i < prefabs.Length) ? prefabs[i] : null;
                        prefabsField.RegisterValueChangedCallback(c =>
                        {
                            if (prefabs == null) {
                                prefabs = new GameObject[1];
                            } else if (prefabIndex == prefabs.Length) {
                                Array.Resize(ref prefabs, prefabs.Length + 1);
                            }

                            prefabs[prefabIndex] = (GameObject)c.newValue;
                            m_ItemCollection.ItemTypes[m_Index].Prefabs = prefabs;
                            EditorUtility.SetDirty(m_ItemCollection.ItemTypes[m_Index]);
                            m_ReorderableList.Refresh();
                        });
                        horizontalContainer.Add(prefabsField);

                        // The last element cannot be removed.
                        if (prefabs != null && prefabIndex < prefabs.Length) {
                            var removeButton = new Button();
                            removeButton.text = "-";
                            removeButton.clicked += () =>
                            {
                                ArrayUtility.RemoveAt(ref prefabs, prefabIndex);
                                m_ItemCollection.ItemTypes[m_Index].Prefabs = prefabs;
                                EditorUtility.SetDirty(m_ItemCollection.ItemTypes[m_Index]);
                                m_ReorderableList.Refresh();
                            };
                            horizontalContainer.Add(removeButton);
                        }

                        m_CharacterItemPrefabContainer.Add(horizontalContainer);
                    }
                }
            }
        }

        /// <summary>
        /// Is the item type name unique?
        /// </summary>
        /// <param name="itemCollection">A reference to the item collection.</param>
        /// <param name="name">The name of the item type.</param>
        /// <returns>True if the item type is unique.</returns>
        public static bool IsUniqueItemTypeName(ItemCollection itemCollection, string name)
        {
            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            if (itemCollection.ItemTypes == null) {
                return true;
            }
            for (int i = 0; i < itemCollection.ItemTypes.Length; ++i) {
                if (String.Compare(itemCollection.ItemTypes[i].name.ToLower(), name.ToLower(), StringComparison.Ordinal) == 0) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Shows all of the categories.
        /// </summary>
        /// <param name="container">The parent container.</param>
        private void ShowCategories(VisualElement container)
        {
            container.Clear();

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.flexShrink = 0;
            horizontalLayout.SetEnabled(m_ItemCollection != null);
            container.Add(horizontalLayout);

            // Define the ReoderableList early so it can be used in the lambda functions.
            ReorderableList reorderableList = null;

            // New categories can be added.
            var addCategoryNameField = new TextField("Name");
            var addCategoryButton = new Button();
            addCategoryNameField.value = m_CategoryName;
            addCategoryNameField.AddToClassList("flex-grow");
            addCategoryNameField.RegisterValueChangedCallback(c =>
            {
                m_CategoryName = c.newValue;
                addCategoryButton.SetEnabled(IsUniqueCategoryName(m_ItemCollection, m_CategoryName));

            });
            horizontalLayout.Add(addCategoryNameField);

            addCategoryButton.text = "Add";
            addCategoryButton.style.width = 75;
            addCategoryButton.SetEnabled(IsUniqueCategoryName(m_ItemCollection, m_CategoryName));
            addCategoryButton.clicked += () =>
            {
                // Create the new Category.
                var category = Category.Create(m_CategoryName);

                // Add the Category to the ItemCollection.
                var categories = m_ItemCollection.Categories;
                Array.Resize(ref categories, categories.Length + 1);
                categories[categories.Length - 1] = category;
                reorderableList.ItemsSource = m_ItemCollection.Categories = categories;
                m_SelectedCategory = reorderableList.SelectedIndex = categories.Length - 1;
                AssetDatabase.AddObjectToAsset(category, m_ItemCollection);

                // Reset.
                EditorUtility.SetDirty(m_ItemCollection);
                AssetDatabase.SaveAssets();
                m_CategoryName = string.Empty;
                addCategoryNameField.SetValueWithoutNotify(m_CategoryName);
                addCategoryButton.SetEnabled(false);
                GUI.FocusControl("");
            };
            horizontalLayout.Add(addCategoryButton);

            if (m_ItemCollection == null) {
                // ItemCollection must be populated in order to create Categories/ItemTypes.
                var helpBox = new HelpBox("An ItemCollection must be selected. Use the \"Create\" button to create a new collection.", HelpBoxMessageType.Error);
                container.Add(helpBox);
                return;
            }

            // The ReorderabeList is responsible for showing all of the categories.
            reorderableList = new ReorderableList(m_ItemCollection.Categories,
            (VisualElement element, int index) => // Make Item.
            {
                element.Add(new CategoryRowElement(reorderableList, m_ItemCollection));
            }, (VisualElement element, int index) => // Bind Item.
            {
                var categoryElement = element.Q<CategoryRowElement>();
                categoryElement.BindItem(index, reorderableList.SelectedIndex == index);
            }, null,
            (int index) => // Element Height.
            {
                if (index == reorderableList.SelectedIndex) {
                    return c_SelectedCategoryRowHeight;
                }
                return c_RowHeight;
            },
            (int index) => // Selection.
            {
                m_SelectedCategory = index;
                reorderableList.Refresh();
            }, null, null, null);
            reorderableList.HighlightSelectedItem = false;
            if (reorderableList.ListItems != null && reorderableList.ListItems.Count > m_SelectedCategory) {
                reorderableList.SelectedIndex = m_SelectedCategory;
            }
            container.Add(reorderableList);
        }

        /// <summary>
        /// The Visual Element for each category row in the ReorderableList.
        /// </summary>
        private class CategoryRowElement : VisualElement
        {
            private ReorderableList m_ReorderableList;
            private ItemCollection m_ItemCollection;

            private Label m_Title;
            private VisualElement m_SelectedContainer;
            private TextField m_NameField;

            private int m_Index;

            /// <summary>
            /// Constructor.
            /// </summary>
            public CategoryRowElement(ReorderableList reorderableList, ItemCollection itemCollection)
            {
                m_ReorderableList = reorderableList;
                m_ItemCollection = itemCollection;
                name = "item-type-row-element";

                // Setup the initial layout.
                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                horizontalLayout.AddToClassList(EditorGUIUtility.isProSkin ? "item-type-row-element-header-dark" : "item-type-row-element-header-light");
                Add(horizontalLayout);

                m_Title = new Label();
                m_Title.style.unityFontStyleAndWeight = FontStyle.Bold;
                m_Title.style.unityTextAlign = TextAnchor.MiddleCenter;
                horizontalLayout.Add(m_Title);

                // Duplicate and remove buttons.
                var buttonLayout = new VisualElement();
                buttonLayout.AddToClassList("horizontal-layout");
                buttonLayout.style.flexDirection = FlexDirection.RowReverse;
                horizontalLayout.Add(buttonLayout);

                var removeButton = new Button();
                removeButton.AddToClassList(EditorGUIUtility.isProSkin ? "delete-dark-icon" : "delete-light-icon");
                removeButton.tooltip = "Remove";
                removeButton.clicked += () =>
                {
                    var categoryToRemove = m_ItemCollection.Categories[m_Index];
                    // The category can't be deleted if other ItemTypes depend on it.
                    var canRemove = true;
                    for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                        var categoryIDs = m_ItemCollection.ItemTypes[i].CategoryIDs;
                        for (int j = 0; j < categoryIDs.Length; ++j) {
                            if (categoryIDs[j] == categoryToRemove.ID) {
                                EditorUtility.DisplayDialog("Unable to Delete", "Unable to delete the category: the ItemType " + m_ItemCollection.ItemTypes[i].name + " uses this category", "OK");
                                canRemove = false;
                                break;
                            }
                        }
                        if (!canRemove) {
                            break;
                        }
                    }

                    if (canRemove) {
                        // Remove the category.
                        Undo.RecordObject(categoryToRemove,"Destroy");
                        AssetDatabase.RemoveObjectFromAsset(categoryToRemove);
                        Undo.DestroyObjectImmediate(categoryToRemove);
                        var categories = new List<Category>(m_ItemCollection.Categories);
                        categories.RemoveAt(m_Index);
                        m_ReorderableList.ItemsSource = m_ItemCollection.Categories = categories.ToArray();
                        EditorUtility.SetDirty(m_ItemCollection);
                        AssetDatabase.SaveAssets();
                    }
                };
                buttonLayout.Add(removeButton);

                var duplicateButton = new Button();
                duplicateButton.AddToClassList(EditorGUIUtility.isProSkin ? "duplicate-dark-icon" : "duplicate-light-icon");
                duplicateButton.tooltip = "Duplicate";
                duplicateButton.clicked += () =>
                {
                    // Generate a unique name for the category.
                    var categories = m_ItemCollection.Categories;
                    var category = categories[m_Index];
                    var index = 1;
                    string name;
                    do {
                        name = category.name + " (" + index + ")";
                        index++;
                    } while (!ItemTypeManager.IsUniqueCategoryName(m_ItemCollection, name));

                    var clonedCategory = Category.Create(name);
                    AssetDatabase.AddObjectToAsset(clonedCategory, m_ItemCollection);

                    // Add the Category to the ItemCollection.
                    Array.Resize(ref categories, categories.Length + 1);
                    categories[categories.Length - 1] = clonedCategory;
                    m_ReorderableList.ItemsSource = m_ItemCollection.Categories = categories;
                    EditorUtility.SetDirty(m_ItemCollection);
                    AssetDatabase.SaveAssets();
                };
                buttonLayout.Add(duplicateButton);

                m_SelectedContainer = new VisualElement();
                Add(m_SelectedContainer);

                // Categories can update their name.
                m_NameField = new TextField();
                m_NameField.label = "Name";
                m_NameField.RegisterValueChangedCallback(c =>
                {
                    if (!ItemTypeManager.IsUniqueCategoryName(m_ItemCollection, c.newValue)) {
                        m_NameField.SetValueWithoutNotify(c.previousValue);
                        return;
                    }

                    m_ItemCollection.Categories[m_Index].name = c.newValue;
                    m_Title.text = c.newValue;
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemCollection);
                });
                m_SelectedContainer.Add(m_NameField);
            }

            /// <summary>
            /// Binds the row to the category element.
            /// </summary>
            public void BindItem(int index, bool selected)
            {
                m_Index = index;

                m_Title.text = m_ItemCollection.Categories[index].name;

                m_SelectedContainer.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;

                // Shows the details for the category.
                if (selected) {
                    m_NameField.SetValueWithoutNotify(m_ItemCollection.Categories[index].name);
                }
            }
        }

        /// <summary>
        /// Is the category name unique?
        /// </summary>
        /// <param name="itemCollection">A reference to the item collection.</param>
        /// <param name="name">The name of the category.</param>
        /// <returns>True if the category is unique.</returns>
        public static bool IsUniqueCategoryName(ItemCollection itemCollection, string name)
        {
            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            if (itemCollection.Categories == null) {
                return true;
            }
            for (int i = 0; i < itemCollection.Categories.Length; ++i) {
                if (String.Compare(itemCollection.Categories[i].name.ToLower(), name.ToLower(), StringComparison.Ordinal) == 0) {
                    return false;
                }
            }
            return true;
        }
    }
}