using Battlehub.RTEditor.ViewModels;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Examples.Scene7.ViewModels
{
    [Binding]
    public class CustomExample2ViewModel : ViewModel
    {
        [Binding]
        public string Text
        {
            get         
            {
                return "This is an example of a custom dialog. \n\r" + 
                    "To create your own, click Tools > RuntimeEditor > Create Custom Window  in the main menu of Unity Editor. \n\r" +
                    "\n\r" +
                    "Open created prefab, drag and drop ParentDialog.prefab under ContentFrame \n\r" +
                    "(Assets/Battlehub/RTEditor/Content/Runtime/RTEditor/Prefabs/Views/Dialogs/ParentDialog.prefab) \n\r" +
                    "\n\r" +
                    "Make sure to set Sub view-model property of ParentDialog.SubViewModelBinding component to ParentDialog property of your custom ViewModel.";
            }
        }

        private DialogViewModel m_parentDialog;
        [Binding]
        public DialogViewModel ParentDialog
        {
            get
            {
                if (m_parentDialog == null)
                {
                    m_parentDialog = new DialogViewModel();     
                }
                return m_parentDialog;
            }
        }

        protected override void Start()
        {
            base.Start();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = "Ok",
                CancelText = "Cancel",
                IsOkVisible = true,
                IsCancelVisible = true,
                IsOkInteractable = true,
                IsCancelInteractable = true,
            };

            ParentDialog.Ok += OnOK;
        }

        private void OnOK(object sender, DialogViewModel.CancelEventArgs e)
        {
            Debug.Log("On OK");
        }
    }
}


