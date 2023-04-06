using Battlehub.RTEditor.ViewModels;
using Battlehub.UIControls.Dialogs.Binding;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(DialogViewModel.Settings), typeof(DialogBinding.Settings))]
    public class DialogViewModelSettingsToDialogBindingSettings : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            DialogViewModel.Settings settings = (DialogViewModel.Settings)valueIn;

            return new DialogBinding.Settings
            {
                OkText = settings.OkText,
                CancelText = settings.CancelText,
                IsOkVisible = settings.IsOkVisible,
                IsCancelVisible = settings.IsCancelVisible,
                IsOkInteractable = settings.IsOkInteractable,
                IsCancelInteractable = settings.IsCancelInteractable
            };
        }
    }

}
