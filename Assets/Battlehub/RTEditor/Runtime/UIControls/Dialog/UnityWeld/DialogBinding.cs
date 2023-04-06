using Battlehub.UIControls.Binding;
using UnityEngine;
using UnityEngine.Events;
using UnityWeld.Binding;

namespace Battlehub.UIControls.Dialogs.Binding
{
    public class DialogBinding : ControlBinding
    {
        [HideInInspector]
        public UnityEvent Ok;
        [HideInInspector]
        public UnityEvent Cancel;
        [HideInInspector]
        public UnityEvent Closed;
        [HideInInspector]
        public UnityEvent ResultChanged;

        private bool? m_result = null;
        public bool? Result
        {
            get { return m_result; }
            set
            {
                if(m_result == null || m_result.Value != value)
                {
                    m_result = value;
                    if (m_dialog != null && m_dialog.ParentRegion != null)
                    {
                        try
                        {
                            m_dialog.Closed -= OnClosed;
                            m_dialog.Close(m_result);
                        }
                        finally
                        {
                            m_dialog.Closed += OnClosed;
                        }
                    }
                }
            }
        }

        public bool IsCancelled
        {
            get;
            set;
        }

        public class Settings
        {
            public string OkText;
            public string CancelText;
            public bool IsOkVisible;
            public bool IsCancelVisible;
            public bool IsOkInteractable;
            public bool IsCancelInteractable;
        }

        private Settings m_dialogSettings;
        public Settings DialogSettings
        {
            get { return m_dialogSettings; }
            set
            {
                if(m_dialogSettings != value)
                {
                    m_dialogSettings = value;
                    if(m_dialog != null)
                    {
                        m_dialog.OkText = m_dialogSettings.OkText;
                        m_dialog.CancelText = m_dialogSettings.CancelText;
                        m_dialog.IsOkVisible = m_dialogSettings.IsOkVisible;
                        m_dialog.IsCancelVisible = m_dialogSettings.IsCancelVisible;
                        m_dialog.IsOkInteractable = m_dialogSettings.IsOkInteractable;
                        m_dialog.IsCancelInteractable = m_dialogSettings.IsCancelInteractable;
                    }
                }
            }
        }

        public bool IsInteractable
        {
            get { return m_dialog != null ? m_dialog.IsInteractable : false; }
            set
            {
                if(m_dialog != null && m_dialog.IsInteractable != value)
                {
                    m_dialog.IsInteractable = value;
                }
            }
        }

        public override Component TargetControl
        {
            get { return this; }
        }

        private Dialog m_dialog;

        public override void Connect()
        {
            m_dialog = GetComponentInParent<Dialog>();

            base.Connect();

            if(m_dialog != null)
            {
                m_dialog.Ok += OnOk;
                m_dialog.Cancel += OnCancel;
                m_dialog.Closed += OnClosed;
            }
        }


        public override void Disconnect()
        {
            base.Disconnect();

            if(m_dialog != null)
            {
                m_dialog.Ok -= OnOk;
                m_dialog.Cancel -= OnCancel;
                m_dialog.Closed -= OnClosed;
                m_dialog = null;
            }
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            if(m_closing)
            {
                return;
            }

            Ok?.Invoke();
            args.Cancel = IsCancelled;
            IsCancelled = false;
        }

        private void OnCancel(Dialog sender, DialogCancelArgs args)
        {
            if (m_closing)
            {
                return;
            }

            Cancel?.Invoke();
            args.Cancel = IsCancelled;
            IsCancelled = false;
        }

        private bool m_closing = false;
        private void OnClosed(Dialog sender, bool? result)
        {
            try
            {
                m_closing = true;
                m_result = result;
                ResultChanged?.Invoke();
                Closed?.Invoke();
            }
            finally
            {
                m_closing = false;

            }
        }
    }

}

