using UnityEngine;
using Battlehub.UIControls.DockPanels;
using System.Collections.Generic;

namespace Battlehub.UIControls.Dialogs
{
    public class DialogManager : MonoBehaviour
    {
        [SerializeField]
        private DockPanel m_dockPanels = null;

        [SerializeField]
        private Dialog m_dialogPrefab = null;

        private Stack<Dialog> m_dialogStack = new Stack<Dialog>();

        public event DialogAction DialogCreated;
        public event DialogAction DialogDestroyed;

        public bool IsDialogOpened
        {
            get { return m_dialogStack.Count > 0; }
        }

        public void CloseDialog()
        {
            if(IsDialogOpened)
            {
                m_dialogStack.Peek().Close(false);
            }
        }

        public Dialog ShowDialog(Sprite icon, string header, Transform content,
             DialogAction<DialogCancelArgs> okAction = null, string okText = "OK",
             DialogAction<DialogCancelArgs> cancelAction = null, string cancelText = "Cancel",
             float minWidth = 100,
             float minHeight = 100,
             float preferredWidth = 700, 
             float preferredHeight = 400, 
             bool canResize = true)
        {
            return ShowComplexDialog(icon, header, content, okAction, okText, cancelAction, cancelText, null, "Alt", minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public Dialog ShowComplexDialog(Sprite icon, string header, Transform content,
            DialogAction<DialogCancelArgs> okAction = null, string okText = "OK",
            DialogAction<DialogCancelArgs> cancelAction = null, string cancelText = "Cancel",
            DialogAction<DialogCancelArgs> altAction = null, string altText = "Alt",
            float minWidth = 100,
            float minHeight = 100,
            float preferredWidth = 700,
            float preferredHeight = 400,
            bool canResize = true)
        {
            Dialog dialog = ShowDialog(icon, header, okAction, okText, cancelAction, cancelText, altAction, altText);
            dialog.Content = content;
            m_dockPanels.AddModalRegion(dialog.HeaderRoot, dialog.transform, minWidth, minHeight, new Rect(0, 0, preferredWidth, preferredHeight), true, canResize);
            m_dialogStack.Push(dialog);
            DialogCreated?.Invoke(dialog);
            return dialog;
        }

        public Dialog ShowDialog(Sprite icon, string header, string content,
            DialogAction<DialogCancelArgs> okAction = null, string okText = "OK",
            DialogAction<DialogCancelArgs> cancelAction = null, string cancelText = "Cancel",
            float minWidth = 100,
            float minHeight = 100,
            float preferredWidth = 350,
            float preferredHeight = 100,
            bool canResize = false)
        {
            return ShowComplexDialog(icon, header, content, okAction, okText, cancelAction, cancelText, null, "Alt", minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public Dialog ShowComplexDialog(Sprite icon, string header, string content,
            DialogAction<DialogCancelArgs> okAction = null, string okText = "OK",
            DialogAction<DialogCancelArgs> cancelAction = null, string cancelText = "Cancel",
            DialogAction<DialogCancelArgs> altAction = null, string altText = "Alt",
            float minWidth = 100,
            float minHeight = 100,
            float preferredWidth = 350,
            float preferredHeight = 100,
            bool canResize = false)
        {
            Dialog dialog = ShowDialog(icon, header, okAction, okText, cancelAction, cancelText, altAction, altText);
            dialog.ContentText = content;
            m_dockPanels.AddModalRegion(dialog.HeaderRoot, dialog.transform, minWidth, minHeight, new Rect(0, 0, preferredWidth, preferredHeight), true, canResize);
            m_dialogStack.Push(dialog);
            DialogCreated?.Invoke(dialog);
            return dialog;
        }

        private Dialog ShowDialog(Sprite icon, string header,
            DialogAction<DialogCancelArgs> okAction = null, string okText = "OK",
            DialogAction<DialogCancelArgs> cancelAction = null, string cancelText = "Cancel",
            DialogAction<DialogCancelArgs> altAction = null, string altText = "Alt")
        {
            if(m_dialogStack.Count > 0)
            {
                Dialog previousDialog = m_dialogStack.Peek();
                previousDialog.Hide();
            }

            Dialog dialog = Instantiate(m_dialogPrefab);
            dialog.name = "Dialog " + header;
            dialog.Icon = icon;
            dialog.HeaderText = header;
            
            dialog.OkAction = okAction;
            dialog.OkText = okText;
            if(cancelAction != null)
            {
                dialog.CancelAction = cancelAction;
                dialog.CancelText = cancelText;
                dialog.IsCancelVisible = true;
            }
            else
            {
                dialog.IsCancelVisible = false;
            }

            if(altAction != null)
            {
                dialog.AltAction = altAction;
                dialog.AltText = altText;
                dialog.IsAltVisible = true;
                dialog.IsAltInteractable = true;
            }
            else
            {
                dialog.IsAltVisible = false;
                dialog.IsAltInteractable = false;
            }

            dialog.Closed += OnDestroyed;
            return dialog;      
        }

        private void OnDestroyed(Dialog sender, bool? result)
        {
            sender.Closed -= OnDestroyed;
            if (m_dialogStack.Contains(sender))
            {
                while(m_dialogStack.Count > 0)
                {
                    Dialog dialog = m_dialogStack.Pop();
                    
                    if (sender == dialog)
                    {
                        if (DialogDestroyed != null)
                        {
                            DialogDestroyed(dialog);
                        }
                        dialog.Close();
                        if (m_dialogStack.Count > 0)
                        {
                            Dialog previousDialog = m_dialogStack.Peek();
                            previousDialog.Show();
                        }
                        break;
                    }

                    dialog.Close();
                }
            }
        }
    }
}

