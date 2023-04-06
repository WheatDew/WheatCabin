using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.Models;
using Battlehub.RTEditor.ViewModels;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
    [Binding]
    public class MobileMenuViewModel : HierarchicalDataViewModel<MobileMenuViewModel.ItemViewModel>
    {
        [Binding]
        public class ItemViewModel
        {
            public class ValidationArgs
            {
                public bool IsValid
                {
                    get;
                    set;
                }

                public string Command
                {
                    get;
                    private set;
                }

                public bool HasChildren
                {
                    get;
                    private set;
                }

                public ValidationArgs(string command, bool hasChildren)
                {
                    Command = command;
                    IsValid = true;
                    HasChildren = hasChildren;
                }
            }

            private Action<string> m_actionCallback;
            private Action<ValidationArgs> m_validateCallback;

            [Binding]
            public string Text
            {
                get;
                private set;
            }

            [Binding]
            public Sprite Icon
            {
                get;
                private set;
            }

            [Binding]
            public string Command
            {
                get;
                private set;
            }

            public bool HasChildren
            {
                get { return Children != null && Children.Count > 0; }
            }

            public ItemViewModel Parent
            {
                get;
                set;
            }


            public List<ItemViewModel> Children
            {
                get;
                private set;
            }

            public ItemViewModel(string text, Action<string> actionCallback, Action<ValidationArgs> validateCallback = null)
                : this(text, null, null, actionCallback, validateCallback)
            {
            }

            public ItemViewModel(string text, Sprite icon, string command, Action<string> actionCallback, Action<ValidationArgs> validateCallback = null)
            {
                Text = text;
                Icon = icon;
                Command = command;
                m_actionCallback = actionCallback;
                m_validateCallback = validateCallback != null ? validateCallback : args => { };
                Children = new List<ItemViewModel>();
            }

            public void Action()
            {
                m_actionCallback?.Invoke(Command);
            }

            public bool Validate()
            {
                ValidationArgs arg = new ValidationArgs(Command, HasChildren);
                m_validateCallback?.Invoke(arg);
                return arg.IsValid;
            }
        }


        private ItemViewModel[] m_menuItems;
        public ItemViewModel[] MenuItems
        {
            get { return m_menuItems; }
            set
            {
                if (m_menuItems != value)
                {
                    m_menuItems = value;
                    BindData();
                }
            }
        }

        private bool m_isActionExecuted;
        [Binding]
        public bool IsActionExecuted
        {
            get { return m_isActionExecuted; }
            set
            {
                if (m_isActionExecuted != value)
                {
                    m_isActionExecuted = value;

                    RaisePropertyChanged(nameof(IsActionExecuted));
                }
            }
        }

        private bool m_canClose;
        [Binding]
        public bool CanClose
        {
            get { return m_canClose; }
            set
            {
                if(m_canClose != value)
                {
                    m_canClose = value;
                    RaisePropertyChanged(nameof(CanClose));
                }
            }
        }

        public MobileMenuItemModel[] MenuItemModels
        {
            get;
            set;
        }

        protected override void Start()
        {
            base.Start();

            if(MenuItemModels == null)
            {
                IMobileEditorModel mobileEditorModel = IOC.Resolve<IMobileEditorModel>();
                MenuItemModels = mobileEditorModel.MainMenuItems;
            }

            MenuItems = MenuItemModels.Select(item => ToItemViewModel(item)).ToArray();
        }

        private ItemViewModel ToItemViewModel(MobileMenuItemModel item)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();
            ItemViewModel itemViewModel = new ItemViewModel(
                lc.GetString(item.Text, item.Text),
                item.Icon,
                item.Command, 
                args => item.Action(),
                args => args.IsValid = item.Validate());
          
            if (!item.HasChildren)
            {
                return itemViewModel;
            }

            for (int i = 0; i < item.Children.Count; ++i)
            {
                ItemViewModel childViewModel = ToItemViewModel(item.Children[i]);
                childViewModel.Parent = itemViewModel;
                itemViewModel.Children.Add(childViewModel);
            }

            return itemViewModel;               
        }

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.None;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ItemViewModel item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override bool HasChildren(ItemViewModel parent)
        {
            return parent.Children.Where(item => item.HasChildren || item.Validate()).Any();
        }

        public override ItemViewModel GetParent(ItemViewModel item)
        {
            return item.Parent;
        }
        public override IEnumerable<ItemViewModel> GetChildren(ItemViewModel parent)
        {
            if(parent == null)
            {
                return m_menuItems != null ? m_menuItems.Where(item => item.HasChildren && item.Children.All(child => child.Validate()) || item.Validate()) : null;
            }

            return parent.Children != null ? parent.Children.Where(item => item.HasChildren && item.Children.All(child => child.Validate()) || item.Validate()) : null;
        }
        #endregion

        #region Bound UnityEvent Handlers

        public override void OnItemClick()
        {
            if(SelectedItem != null && !SelectedItem.HasChildren)
            {
                if (SelectedItem.Validate())
                {
                    SelectedItem.Action();
                    IsActionExecuted = true;
                }
            }
        }

        [Binding]
        public void OnClose()
        {
            IsActionExecuted = true;
        }

        #endregion
    }

}
