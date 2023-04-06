using Battlehub.UIControls.DockPanels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.Dialogs
{
   
    public class DialogCancelArgs
    {
        public virtual bool Cancel
        {
            get;
            set;
        }
    }

    public delegate void DialogAction(Dialog sender);
    public delegate void DialogAction<T>(Dialog sender, T args);

    public enum DialogResult
    {
        None,
        OK,
        Cancel,
        Alt,
    }

    public class Dialog : MonoBehaviour
    {
        private Region m_parentRegion;
        public Region ParentRegion
        {
            get { return m_parentRegion; }
        }

        public event DialogAction<DialogCancelArgs> Ok;
        public event DialogAction<DialogCancelArgs> Alt;
        public event DialogAction<DialogCancelArgs> Cancel;
        public event DialogAction<bool?> Closed;

        public DialogAction<DialogCancelArgs> OkAction;
        public DialogAction<DialogCancelArgs> AltAction;
        public DialogAction<DialogCancelArgs> CancelAction;

        [SerializeField]
        private Transform m_headerRoot = null;
        public Transform HeaderRoot
        {
            get { return m_headerRoot; }
        }

        [SerializeField]
        private Transform m_contentParent = null;

        [SerializeField]
        private Image m_headerIcon = null;

        [SerializeField]
        private TextMeshProUGUI m_headerText = null;

        [SerializeField]
        private TextMeshProUGUI m_contentText = null;

        [SerializeField]
        private Transform m_buttonsRoot = null;

        [SerializeField]
        private Button m_okButton = null;

        [SerializeField]
        private Button m_cancelButton = null;

        [SerializeField]
        private Button m_altButton = null;

        [SerializeField]
        private Button m_closeButton = null;

        public Sprite Icon
        {
            set
            {
                if(m_headerIcon != null)
                {
                    m_headerIcon.sprite = value;
                    if(value != null)
                    {
                        m_headerIcon.gameObject.SetActive(true);
                    }
                }
            }
        }

        public string HeaderText
        {
            set
            {
                if(m_headerText != null)
                {
                    m_headerText.text = value;
                }
            }
        }

        public string ContentText
        {
            set
            {
                m_contentText.text = value;
            }
        }

        public Transform Content
        {
            get
            {
                if(m_contentParent == null)
                {
                    return null;
                }
                if(m_contentParent.childCount == 0)
                {
                    return null;
                }

                return m_contentParent.GetChild(0);
            }
            set
            {
                if(m_contentParent != null)
                {
                    foreach(Transform child in m_contentParent)
                    {
                        Destroy(child.gameObject);
                    }
                    
                    value.SetParent(m_contentParent, false);

                    RectTransform rt = (RectTransform)value;
                    rt.Stretch();
                }
            }
        }

        public string OkText
        {
            set
            {
                if(m_okButton != null)
                {
                    TextMeshProUGUI okButtonText = m_okButton.GetComponentInChildren<TextMeshProUGUI>(true);
                    if(okButtonText != null)
                    {
                        okButtonText.text = value;
                    }
                }
            }
        }

        public string CancelText
        {
            set
            {
                if (m_cancelButton != null)
                {
                    TextMeshProUGUI cancelButtonText = m_cancelButton.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (cancelButtonText != null)
                    {
                        cancelButtonText.text = value;
                    }
                }
            }
        }

        public string AltText
        {
            set
            {
                if(m_altButton != null)
                {
                    TextMeshProUGUI altButtonText = m_altButton.GetComponentInChildren<TextMeshProUGUI>(true);
                    if(altButtonText != null)
                    {
                        altButtonText.text = value;
                    }
                }
            }
        }

        public bool IsInteractable
        {
            get { return IsOkInteractable || IsCancelInteractable || IsAltInteractable || IsCloseButtonInteractable; }
            set
            {
                IsOkInteractable = true;
                IsCancelInteractable = true;
                IsAltInteractable = true;
                IsCloseButtonInteractable = true;
            }
        }
      
  
        public bool IsOkInteractable
        {
            get
            {
                if(m_okButton == null)
                {
                    return false;
                }

                return m_okButton.interactable;
            }
            set
            {
                if(m_okButton != null)
                {
                    m_okButton.interactable = value;
                }
            }
        }

        public bool IsOkVisible
        {
            set
            {
                bool wasVisible = false;
                if(m_okButton != null)
                {
                    wasVisible = m_okButton.gameObject.activeSelf;
                    m_okButton.gameObject.SetActive(value);
                }

                if(m_buttonsRoot != null)
                {
                    m_buttonsRoot.gameObject.SetActive(m_cancelButton != null && m_cancelButton.gameObject.activeSelf || m_okButton != null && m_okButton.gameObject.activeSelf);
                }

                if(m_okButton != null)
                {
                    if (value && !wasVisible)
                    {
                        m_okButton.Select();
                    }
                }
            }
        }

        public bool IsCancelInteractable
        {
            get
            {
                if(m_cancelButton == null)
                {
                    return false;
                }

                return m_cancelButton.interactable;
            }
            set
            {
                if(m_cancelButton != null)
                {
                    m_cancelButton.interactable = value;
                }
            }
        }

        public bool IsCancelVisible
        {
            set
            {
                if(m_cancelButton != null)
                {
                    m_cancelButton.gameObject.SetActive(value);
                }

                if (m_buttonsRoot != null)
                {
                    m_buttonsRoot.gameObject.SetActive(m_cancelButton != null && m_cancelButton.gameObject.activeSelf || m_okButton != null && m_okButton.gameObject.activeSelf);
                }
            }
        }

        public bool IsAltInteractable
        {
            get
            {
                if (m_altButton == null)
                {
                    return false;
                }

                return m_altButton.interactable;
            }
            set
            {
                if (m_altButton != null)
                {
                    m_altButton.interactable = value;
                }
            }
        }

        public bool IsAltVisible
        {
            set
            {
                if (m_altButton != null)
                {
                    m_altButton.gameObject.SetActive(value);
                }
            }
        }

        public bool IsCloseButtonInteractable
        {
            get
            {
                if(m_closeButton == null)
                {
                    return false;
                }

                return m_closeButton.interactable;
            }
            set
            {
                if(m_closeButton != null)
                {
                    m_closeButton.interactable = value;
                }
            }
        }

        public bool IsCloseButtonVisible
        {
            set
            {
                if(m_closeButton != null)
                {
                    m_closeButton.gameObject.SetActive(value);
                }
            }
        }


        private void Start()
        {
            if(m_okButton != null)
            {
                m_okButton.onClick.AddListener(OnOkClick);
            }

            if(m_altButton != null)
            {
                m_altButton.onClick.AddListener(OnAltClick);
            }

            if(m_cancelButton != null)
            {
                m_cancelButton.onClick.AddListener(OnCancelClick);
            }

            if(m_closeButton != null)
            {
                m_closeButton.onClick.AddListener(OnCancelClick);
            }

            m_parentRegion = GetComponentInParent<Region>();

            if (m_okButton != null && m_okButton.gameObject.activeSelf)
            {
                m_okButton.Select();
            }
        }

        private void OnDestroy()
        {
            if (m_okButton != null)
            {
                m_okButton.onClick.RemoveListener(OnOkClick);
            }

            if (m_altButton != null)
            {
                m_altButton.onClick.RemoveListener(OnAltClick);
            }

            if (m_cancelButton != null)
            {
                m_cancelButton.onClick.RemoveListener(OnCancelClick);
            }

            if (m_closeButton != null)
            {
                m_closeButton.onClick.RemoveListener(OnCancelClick);
            }
        }

        private void OnOkClick()
        {
            Close(DialogResult.OK);
        }

        private void OnAltClick()
        {
            Close(DialogResult.Alt);
        }

        private void OnCancelClick()
        {
            Close(DialogResult.Cancel);
        }     
        
        public void Hide()
        {
            if(m_parentRegion == null)
            {
                m_parentRegion = GetComponentInParent<Region>();
            }

            m_parentRegion.gameObject.SetActive(false);
        }

        public void Show()
        {
            if(m_parentRegion != null)
            {
                m_parentRegion.gameObject.SetActive(true);
            }
        }

        public void Close(bool? bResult = null, bool raiseEvents = true, bool invokeActions = true)
        {
            DialogResult result = DialogResult.None;
            if(bResult.HasValue)
            {
                if(bResult.Value)
                {
                    result = DialogResult.OK;
                }
                else
                {
                    result = DialogResult.Cancel;
                }
            }

            Close(result, raiseEvents, invokeActions);
        }

        public void Close(DialogResult result, bool raiseEvents = true, bool invokeActions = true)
        {
            if (m_parentRegion == null)
            {
                Debug.LogWarning("m_parentRegion == null");
                return;
            }

            if (result != DialogResult.None)
            {
                if (result == DialogResult.Cancel)
                {
                    if (Cancel != null && raiseEvents)
                    {
                        DialogCancelArgs args = new DialogCancelArgs();
                        Cancel(this, args);
                        if (args.Cancel)
                        {
                            return;
                        }
                    }

                    if (CancelAction != null && invokeActions)
                    {
                        DialogCancelArgs args = new DialogCancelArgs();
                        CancelAction(this, args);
                        if (args.Cancel)
                        {
                            return;
                        }
                    }
                }
                else if (result == DialogResult.OK)
                {
                    if (Ok != null && raiseEvents)
                    {
                        DialogCancelArgs args = new DialogCancelArgs();
                        Ok(this, args);
                        if (args.Cancel)
                        {
                            return;
                        }
                    }

                    if (OkAction != null && invokeActions)
                    {
                        DialogCancelArgs args = new DialogCancelArgs();
                        OkAction(this, args);
                        if (args.Cancel)
                        {
                            return;
                        }
                    }
                }
                else if (result == DialogResult.Alt)
                {
                    if (Alt != null && raiseEvents)
                    {
                        DialogCancelArgs args = new DialogCancelArgs();
                        Alt(this, args);
                        if (args.Cancel)
                        {
                            return;
                        }
                    }

                    if (AltAction != null && invokeActions)
                    {
                        DialogCancelArgs args = new DialogCancelArgs();
                        AltAction(this, args);
                        if (args.Cancel)
                        {
                            return;
                        }
                    }
                }
            }

            Destroy(m_parentRegion.gameObject);
            if (Closed != null)
            {
                bool? bResult = null;
                if(result == DialogResult.OK)
                {
                    bResult = true;
                }
                else if(result == DialogResult.Cancel)
                {
                    bResult = false;
                }
                Closed(this, bResult);
            }
        }
    }
}
