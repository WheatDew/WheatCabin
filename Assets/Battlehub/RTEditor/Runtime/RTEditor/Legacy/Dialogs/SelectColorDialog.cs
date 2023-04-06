using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class SelectColorDialog : RuntimeWindow
    {
        [HideInInspector]
        public Color SelectedColor = Color.white;

        [SerializeField]
        private ColorPicker m_colorPicker = null;

        private Dialog m_parentDialog;

        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.SelectColor;
            base.AwakeOverride();

            m_localization = IOC.Resolve<ILocalization>();
        }

        private void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.Ok += OnOk;
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SelectColorDialog_Select", "Select");
            m_parentDialog.IsCancelVisible = true;
            m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_SelectColorDialog_Cancel", "Cancel");
            m_colorPicker.CurrentColor = SelectedColor;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if (m_parentDialog != null) m_parentDialog.Ok -= OnOk;

        }
        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            SelectedColor = m_colorPicker.CurrentColor;
        }
    }
}
