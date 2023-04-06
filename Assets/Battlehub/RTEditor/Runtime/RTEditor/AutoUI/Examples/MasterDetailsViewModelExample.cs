using Battlehub.UIControls;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.UI
{
    public class ItemExample
    {
        public long ID;
        public Sprite Icon;
        public string Name;
        public string Instructions;

        public ItemExample(long id, Sprite icon, string name, string instructions)
        {
            ID = id;
            Icon = icon;
            Name = name;
            Instructions = instructions;
        }
    }

    public class DataModelExample : ObservableCollection<ItemExample>
    {
        public DataModelExample()
        {
            Sprite[] sprites = new[]
            {
                Resources.Load<Sprite>("sushi-100"),
                Resources.Load<Sprite>("pizza-100"),
                Resources.Load<Sprite>("steak-100"),
            };

            string[] names = new[]
            {
                "Sushi",
                "Pizza",
                "Steak"
            };

            string[] instructions = new[]
            {
                  "Prepare your sushi rice \n" 
                + "Place your nori sheet on your sushi mat, and place a small amount of rice in the centre, and spread it out with the back of a wooden spoon, leaving a 2 inch margin on the sides \n" 
                + "Place 1/2 cup of the rice, and spread out evenly \n" 
                + "Top your rice with 1/4 cup of the rice, and sprinkle 1/2 teaspoon of sesame seeds on top \n"
                + "Using a wooden spoon, and a rolling motion, roll up your roll \n"
                + "Cut your roll into 6 pieces with a sharp knife \n"
                + "Repeat with remaining rice, rice, and sesame seeds \n"
                + "Serve your sesame rolls with the soy sauce, wasabi, and pickled ginger \n"
                + "Enjoy!",

                  "Step 1\n"
                + "Spoon tomato-and-basil pasta sauce evenly over crust, leaving a 1-inch border around edges. Top with half of pepperoni slices. Sprinkle with cheese. Top with remaining pepperoni.\n"
                + "Step 2\n"
                + "Bake pizza at 450° directly on oven rack 11 to 12 minutes or until crust is golden and cheese is melted. Cut into 6 slices. Serve immediately.\n"
                + "Step 3\n"
                + "Note: For testing purposes only, we used Boboli 100% Whole Wheat Thin Pizza Crust and Classico Tomato & Basil Pasta Sauce.\n",

                  "Season steak with salt and grill\n" 
                + "Serve hot with your favorite sauce"
            };


            for(int i = 0; i < 100; ++i)
            {
                Add(new ItemExample(i, sprites[i % 3], names[i % 3], instructions[i % 3]));
            }
        }

        public void Create()
        {
            Add(new ItemExample(Count, Resources.Load<Sprite>("pizza-100"), "Name", "Description"));
        }

        public void Update(ItemExample item)
        {
            SetItem(IndexOf(item), item);
        }

    }

    public class ViewModelExample : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class MasterDetailsViewModelExample 
    {
        [DialogOkAction(caption:"Save")]
        public void OnOK()
        {
            Debug.Log("Save");
        }

        [DialogCancelAction(caption:"Cancel")]
        public void OnCancel()
        {
            Debug.Log("Cancel");
        }

        [HorizontalLayoutGroup(childForceExpandWidth:false)]
        public RootPanelViewModel Panel { get; set; }
        public class RootPanelViewModel  : ViewModelExample
        {
            [VerticalLayoutGroup(childForceExpandHeight:false), Layout(preferredWidth:150)]
            public LeftPanelViewModel LeftPanel { get; set; }
            public class LeftPanelViewModel : ViewModelExample
            {
                [HorizontalLayoutGroup(childForceExpandHeight: false, padding:5, spacing:5), Layout(flexibleHeight: 0)]
                public ButtonsPanelViewModel ButtonsPanel { get; set; }
                public class ButtonsPanelViewModel : ViewModelExample
                {
                    [Inject]
                    public DataModelExample Model { get; set; }

                    [PropertyChangedEventHandler(typeof(TreePanelViewModel))]
                    public void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
                    {
                        if (args.PropertyName == nameof(TreePanelViewModel.SelectedItemIndex))
                        {
                            TreePanelViewModel leftPanel = (TreePanelViewModel)sender;
                            SelectedItemIndex = leftPanel.SelectedItemIndex;
                            IsRemoveInteractable = SelectedItemIndex >= 0;
                        }
                    }

                    private int m_selectedItemIndex = -1;
                    private int SelectedItemIndex
                    {
                        get { return m_selectedItemIndex; }
                        set { m_selectedItemIndex = value; }
                    }

                    private bool m_isRemoveInteractable;
                    public bool IsRemoveInteractable
                    {
                        get { return m_isRemoveInteractable; }
                        set
                        {
                            if (m_isRemoveInteractable != value)
                            {
                                m_isRemoveInteractable = value;
                                RaisePropertyChange(nameof(IsRemoveInteractable));
                            }
                        }
                    }

                    [Action(caption: "Create"), Layout(preferredHeight: 22)]
                    public void OnCreate()
                    {
                        Debug.Log(nameof(OnCreate));
                        Model.Create();
                    }

                    [Action(caption: "Remove", isInteractableProperty: nameof(IsRemoveInteractable)), Layout(preferredHeight: 22)]
                    public void OnRemove()
                    {
                        Debug.Log(nameof(OnRemove));
                        Model.RemoveAt(SelectedItemIndex);
                    }
                }

                [VerticalLayoutGroup(padding: 1), Layout(flexibleHeight: 1)]
                public TreePanelViewModel TreePanel { get; set; }
                public class TreePanelViewModel : ViewModelExample
                {
                    private DataModelExample m_model;
                    [Inject]
                    public DataModelExample Model
                    {
                        get { return m_model; }
                        set
                        {
                            if (m_model != value)
                            {
                                m_model = value;

                                HierarchicalDataCollection<ItemViewModel> items = new HierarchicalDataCollection<ItemViewModel>();
                                foreach (ItemExample item in m_model)
                                {
                                    items.Add(null, new ItemViewModel(item));
                                }

                                Items = items;
                            }
                        }
                    }

                    public class ItemViewModel
                    {
                        public long ID
                        {
                            get { return m_item.ID; }
                        }

                        [ItemLayout(flexibleWidth: 0, preferredWidth: 30)]
                        [Property]
                        public Sprite Icon
                        {
                            get { return m_item.Icon; }
                        }

                        [ItemLayout(flexibleWidth: 1)]
                        [Property]
                        public string Name
                        {
                            get { return m_item.Name; }
                        }

                        private ItemExample m_item;
                        public ItemExample Item
                        {
                            get { return m_item; }
                        }
                        
                        public ItemViewModel(ItemExample item)
                        {
                            m_item = item;
                        }
                    }

                    private int m_selectedIndex = -1;
                    public int SelectedItemIndex
                    {
                        get { return m_selectedIndex; }
                        set
                        {
                            if (m_selectedIndex != value)
                            {
                                m_selectedIndex = value;
                                RaisePropertyChange(nameof(SelectedItemIndex));
                            }
                        }
                    }

                    private HierarchicalDataCollection<ItemViewModel> m_items;
                    [ItemHorizontalLayoutGroup(childForceExpandWidth: false), Layout(flexibleHeight: 1)]
                    [CollectionProperty(selectedIndexProperty: nameof(SelectedItemIndex))]
                    public HierarchicalDataCollection<ItemViewModel> Items
                    {
                        get { return m_items; }
                        set
                        {
                            if (m_items != value)
                            {
                                m_items = value;
                                RaisePropertyChange(nameof(Items));
                            }
                        }
                    }

                    [CollectionChangedEventHandler(typeof(DataModelExample))]
                    public void DataModelChanged(object sender, NotifyCollectionChangedEventArgs args)
                    {
                        switch (args.Action)
                        {
                            case NotifyCollectionChangedAction.Add:

                                foreach (ItemExample item in args.NewItems)
                                {
                                    Items.Add(null, new ItemViewModel(item));
                                }

                                Items.ScrollIntoViewSelected = true;
                                SelectedItemIndex = Model.Count - 1;
                                Items.ScrollIntoViewSelected = false;

                                break;
                            case NotifyCollectionChangedAction.Remove:

                                for (int i = 0; i < args.OldItems.Count; ++i)
                                {
                                    Items.RemoveAt(null, args.OldStartingIndex);

                                    if (SelectedItemIndex == args.OldStartingIndex + i)
                                    {
                                        SelectedItemIndex = -1;
                                    }
                                }

                                break;
                            case NotifyCollectionChangedAction.Replace:

                                int index = args.NewStartingIndex;
                                foreach (ItemExample item in args.NewItems)
                                {
                                    Items.ResetAt(null, index);
                                    index++;
                                }

                                break;
                        }
                    }

                    [Procedural(bindingMethodName:nameof(TreePanelViewModel.Bind))]
                    public static void AddBackground(RectTransform panel)
                    {
                        AutoUI autoUI = new AutoUI(panel);
                        var (image, layout) = autoUI.Image(true, false);

                        layout.ignoreLayout = true;
                        image.color = new Color32(0x27, 0x27, 0x27, 0xFF);
                        image.rectTransform.SetSiblingIndex(0);
                        image.rectTransform.Stretch();
                    }

                    public void Bind(RectTransform panel)
                    {
                        Debug.Log("Bind");
                    }
                }
            }

            [VerticalLayoutGroup(childForceExpandHeight:false, padding:5, spacing:5), Layout(flexibleWidth:float.MaxValue)]
            public RightPanelViewModel RightPanel { get; set; }
            public class RightPanelViewModel : ViewModelExample
            {
                [Inject]
                public DataModelExample Model { get; set; }

                private ItemExample m_selectedItem;

                [PropertyChangedEventHandler(typeof(LeftPanelViewModel.TreePanelViewModel))]
                public void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
                {
                    if (args.PropertyName == nameof(LeftPanelViewModel.TreePanelViewModel.SelectedItemIndex))
                    {
                        LeftPanelViewModel.TreePanelViewModel treePanel = (LeftPanelViewModel.TreePanelViewModel)sender;
                        IsActive = treePanel.SelectedItemIndex >= 0;
                        if (IsActive)
                        {
                            m_selectedItem = Model[treePanel.SelectedItemIndex];
                            ItemName = m_selectedItem.Name;
                            ItemDescription = m_selectedItem.Instructions;
                        }
                        else
                        {
                            m_selectedItem = null;
                            ItemName = null;
                            ItemDescription = null;
                        }

                        RaisePropertyChange(nameof(ItemName));
                        RaisePropertyChange(nameof(ItemDescription));
                    }
                }

                private bool m_isActive;
                public bool IsActive
                {
                    get { return m_isActive; }
                    set
                    {
                        if (m_isActive != value)
                        {
                            m_isActive = value;
                            RaisePropertyChange(nameof(IsActive));
                        }
                    }
                }

                [Property(isActiveProperty:nameof(IsActive)), Layout(flexibleHeight: 0, preferredHeight: 22)]
                public string ItemName
                {
                    get { return m_selectedItem != null ? m_selectedItem.Name : null; }
                    set
                    {
                        if (m_selectedItem != null)
                        {
                            m_selectedItem.Name = value;
                            Model.Update(m_selectedItem);
                        }
                    }
                }

                [Property(isActiveProperty: nameof(IsActive)), Layout(flexibleHeight: 1)]
                public string ItemDescription
                {
                    get { return m_selectedItem != null ? m_selectedItem.Instructions : null; }
                    set
                    {
                        if(m_selectedItem != null)
                        {
                            m_selectedItem.Instructions = value;
                            Model.Update(m_selectedItem);
                        }
                    }
                }

                [Style(color:"#272727FF")]
                [Property(isActiveProperty: nameof(IsActive), invertIsActiveProperty: true), Layout(ignoreLayout:true)]
                public Space Space
                {
                    get;
                }

                [Style(textAlignment: TextAlignmentOptions.Center)]
                [Property(isActiveProperty:nameof(IsActive), invertIsActiveProperty:true), Layout(ignoreLayout:true)]
                public string NothingSelected
                {
                   get { return "nothing selected"; }
                }
            };
        }
    }

}

