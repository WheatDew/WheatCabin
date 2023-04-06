using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class SelectColorViewModel : ViewModel
    {
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

        private Color m_selectedColor = Color.white;
        [Binding]
        public Color SelectedColor
        {
            get { return m_selectedColor; }
            set
            {
                if(m_selectedColor != value)
                {
                    m_selectedColor = value;
                    RaisePropertyChanged(nameof(SelectedColor));
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_SelectColorDialog_Select", "Open"),
                CancelText = Localization.GetString("ID_RTEditor_SelectColorDialog_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };
        }
        
        protected override void OnDestroy()
        {
            m_parentDialog = null;
            base.OnDestroy();
        }
    }
}
