using System;
using System.ComponentModel;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class DialogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler<CancelEventArgs> Ok;
        public event EventHandler<CancelEventArgs> Cancel;
        public event EventHandler<CloseEventArgs> Closed;

        public class CancelEventArgs : EventArgs
        {
            public bool Cancel
            {
                get;
                set;
            }

        }

        public class CloseEventArgs : EventArgs
        {
            public bool? Result
            {
                get;
            }

            public CloseEventArgs(bool? result)
            {
                Result = result;
            }
        }

        public class Settings
        {
            public string OkText = "OK";
            public string CancelText = "Cancel";
            public bool IsOkVisible = true;
            public bool IsCancelVisible = true;
            public bool IsOkInteractable = true;
            public bool IsCancelInteractable = true;
        }

        private Settings m_dialogSettings = new Settings();
        [Binding]
        public Settings DialogSettings
        {
            get { return m_dialogSettings; }
            set
            {
                if (m_dialogSettings != value)
                {
                    m_dialogSettings = value;
                    RaisePropertyChanged(nameof(DialogSettings));
                }
            }
        }

        public string OkText
        {
            set
            {
                if (m_dialogSettings.OkText != value)
                {
                    m_dialogSettings.OkText = value;
                    RaisePropertyChanged(nameof(DialogSettings));
                }
            }
        }

        public string CancelText
        {
            set
            {
                if (m_dialogSettings.CancelText != value)
                {
                    m_dialogSettings.CancelText = value;
                    RaisePropertyChanged(nameof(DialogSettings));
                }
            }
        }

        public bool IsOkVisible
        {
            set
            {
                if (m_dialogSettings.IsOkVisible != value)
                {
                    m_dialogSettings.IsOkVisible = value;
                    RaisePropertyChanged(nameof(DialogSettings));
                }
            }
        }

        public bool IsCancelVisible
        {
            set
            {
                if (m_dialogSettings.IsCancelVisible != value)
                {
                    m_dialogSettings.IsCancelVisible = value;
                    RaisePropertyChanged(nameof(DialogSettings));
                }
            }
        }

        public bool IsOkInteractable
        {
            set
            {
                if(m_dialogSettings.IsOkInteractable != value)
                {
                    m_dialogSettings.IsOkInteractable = value;
                    RaisePropertyChanged(nameof(DialogSettings));
                }
            }
        }

        public bool IsCancelInteractable
        {
            set
            {
                if(m_dialogSettings.IsCancelInteractable != value)
                {
                    m_dialogSettings.IsCancelInteractable = value;
                    RaisePropertyChanged(nameof(DialogSettings));
                }
            }
        }

        private bool m_isInteractable = true;
        [Binding]
        public bool IsInteractable
        {
            get { return m_isInteractable; }
            set
            {
                if(m_isInteractable != value)
                {
                    m_isInteractable = value;
                    RaisePropertyChanged(nameof(IsInteractable));
                }
            }
        }


        private bool m_isCancelled;
        [Binding]
        public bool IsCancelled
        {
            get { return m_isCancelled; }
            set
            {
                if (m_isCancelled != value)
                {
                    m_isCancelled = value;
                    RaisePropertyChanged(nameof(IsCancelled));
                    m_isCancelled = false;
                }
            }
        }

        private bool? m_result;
        [Binding]
        public bool? Result
        {
            get { return m_result; }
            set
            {
                if (m_result != value)
                {
                    m_result = value;
                    RaisePropertyChanged(nameof(Result));
                }
            }
        }

        [Binding]
        public void OnOk()
        {
            CancelEventArgs args = new CancelEventArgs();
            Ok?.Invoke(this, args);
            IsCancelled = args.Cancel;
        }

        [Binding]
        public void OnCancel()
        {
            CancelEventArgs args = new CancelEventArgs();
            Cancel?.Invoke(this, args);
            IsCancelled = args.Cancel;
        }

        [Binding]
        public void OnClosed()
        {
            CloseEventArgs args = new CloseEventArgs(Result);
            Closed?.Invoke(this, args);
        }

        public void Close(bool? result = null)
        {
            m_result = result;
            RaisePropertyChanged(nameof(Result));
        }
    }
}

