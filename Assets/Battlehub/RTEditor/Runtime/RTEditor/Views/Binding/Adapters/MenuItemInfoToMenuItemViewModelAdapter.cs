using Battlehub.RTEditor.ViewModels;
using Battlehub.UIControls.MenuControl;
using System.Collections.Generic;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{

    [Adapter(typeof(IEnumerable<MenuItemInfo>), typeof(IEnumerable<MenuItemViewModel>))]
    public class MenuItemInfoToMenuItemViewModelAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            if (valueIn == null)
            {
                return null;
            }

            IEnumerable<MenuItemInfo> sourceItems = (IEnumerable<MenuItemInfo>)valueIn;
            List<MenuItemViewModel> targetItems = new List<MenuItemViewModel>();
            foreach (MenuItemInfo source in sourceItems)
            {
                MenuItemViewModel target = new MenuItemViewModel
                {
                    Path = source.Path,
                    Text = source.Text,
                    Icon = source.Icon,
                    TypeIndex = source.PrefabIndex,

                    Command = source.Command,
                };

                MenuItemValidationArgs sourceArgs = new MenuItemValidationArgs(source.Command, false);
                target.Action = arg => source.Action?.Invoke(arg);
                target.Validate = targetArgs =>
                {
                    sourceArgs.IsValid = true;
                    source.Validate?.Invoke(sourceArgs);
                    targetArgs.IsValid = sourceArgs.IsValid;
                };

                targetItems.Add(target);
            }

            return targetItems;
        }
    }

    [Adapter(typeof(IEnumerable<MenuItemViewModel>), typeof(IEnumerable<MenuItemInfo>))]
    public class MenuItemViewModelToMenuItemInfoAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            if (valueIn == null)
            {
                return null;
            }

            IEnumerable<MenuItemViewModel> sourceItems = (IEnumerable<MenuItemViewModel>)valueIn;
            List<MenuItemInfo> targetItems = new List<MenuItemInfo>();
            foreach(MenuItemViewModel source in sourceItems)
            {
                MenuItemInfo target = new MenuItemInfo
                {
                    Path = source.Path,
                    Text = source.Text,
                    Icon = source.Icon,
                    PrefabIndex = source.TypeIndex,

                    Command = source.Command,
                    Action = new MenuItemEvent(),
                    Validate = new MenuItemValidationEvent(),
                };

                MenuItemViewModel.ValidationArgs sourceArgs = new MenuItemViewModel.ValidationArgs();
                target.Action.AddListener(arg => source.Action?.Invoke(arg));
                target.Validate.AddListener(targetArgs =>
                {
                    sourceArgs.IsValid = true;
                    source.Validate?.Invoke(sourceArgs);
                    targetArgs.IsValid = sourceArgs.IsValid;
                });

                targetItems.Add(target);
            }

            return targetItems;
        }
    }
}
