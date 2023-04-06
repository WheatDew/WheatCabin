using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.DockPanels;
using Battlehub.UIControls.MenuControl;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene8
{
    /// <summary>
    /// This is an example of window management(creating, closing, docking) click somewhere to create a popup window
    /// </summary>
    [MenuDefinition]
    public class WindowManagementExample : EditorExtension
    {
        private IWindowManager m_wm;
        private IRTE m_editor;

        protected override void OnInit()
        {
            base.OnInit();

            m_editor = IOC.Resolve<IRTE>();
            m_wm = IOC.Resolve<IWindowManager>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_editor = null;
            m_wm = null;
        }

        [MenuCommand("Example/Standard Dialogs/Show MessageBox")]
        public void ShowMessageBox()
        {
            m_wm.MessageBox(Resources.Load<Sprite>("RTE_Settings"), "Message Box Header", "Hit C to close dialog programmatically", (sender, okArgs) =>
            {
                Debug.Log("On MessageBox OK button click");
            });

            StartCoroutine(CoCloseDialog());
        }

        [MenuCommand("Example/Standard Dialogs/Show Confirmation")]
        public void ShowConfirmation()
        {
            m_wm.Confirmation(Resources.Load<Sprite>("RTE_Settings"), "Confirmation Header", "Hit C to close dialog programmatically", 
                (sender, okArgs) =>
                {
                    Debug.Log("On Confirmation OK button click");
                },
                (sender, cancelArgs) =>
                {
                    Debug.Log("On Confirmation Cancel button click");
                },
                "OK", 
                "Cancel");

            StartCoroutine(CoCloseDialog());
        }

        [MenuCommand("Example/Standard Dialogs/Show Prompt")]
        public void ShowPrompt()
        {
            m_wm.Prompt(Resources.Load<Sprite>("RTE_Settings"), "Prompt Header", "Input",
                (sender, okArgs) =>
                {
                    Debug.Log($"On Prompt OK button click {okArgs.Text}");
                },
                (sender, cancelArgs) =>
                {
                    Debug.Log($"On Prompt Cancel button click {cancelArgs.Text}");
                },
                "Submit",
                "Cancel");

            StartCoroutine(CoCloseDialog());
        }

        [MenuCommand("Example/Standard Dialogs/Show Dialog")]
        public void ShowCustomDialog()
        {
            RectTransform contentTransform = new GameObject("DialogContent").AddComponent<RectTransform>();
            contentTransform.gameObject.AddComponent<WindowOverlay>();
            contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
            contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200);

            RectTransform textTransform = new GameObject("Text").AddComponent<RectTransform>();
            TextMeshProUGUI text = textTransform.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = "This is the programmatically created content for a dialog opened using IWindowManager.Dialog method";
            text.fontSize = 13;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.overflowMode = TextOverflowModes.Masking;

            textTransform.SetParent(contentTransform, false);
            textTransform.Stretch();

            m_wm.Dialog(Resources.Load<Sprite>("RTE_Settings"), "Custom Dialog", contentTransform,
                (sender, okArgs) =>
                {
                    Debug.Log("On Custom Dialog OK button click");
                },
                "OK",
                (sender, cancelArgs) =>
                {
                     Debug.Log("On Custom Dialog Cancel button click");
                }, 
                "Cancel", 
                canResize: true,
                minWidth: 200,
                minHeight: 200);

            StartCoroutine(CoCloseDialog());
        }

        private IEnumerator CoCloseDialog()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            while(m_wm.IsDialogOpened)
            {
                if(editor.Input.GetKeyDown(KeyCode.C))
                {
                    m_wm.DestroyDialogWindow();
                }
                yield return null;
            }
        }

        [MenuCommand("Example/Window/Create Floating/Scene")]
        public void CreateFloatingScene()
        {
            m_wm.CreateWindow(BuiltInWindowNames.Scene, true);
        }

        [MenuCommand("Example/Window/Create Floating/Complex")]
        public void CreateFloatingComplex()
        {
            Transform hierarchy = m_wm.CreateWindow(BuiltInWindowNames.Hierarchy, true);
            m_wm.CreateWindow(BuiltInWindowNames.Inspector, false, RegionSplitType.Right, 0.3f, hierarchy);
        }

        [MenuCommand("Example/Window/Create Docked/Scene")]
        public void CreateDockedScene()
        {
            m_wm.CreateWindow(BuiltInWindowNames.Scene, false, RegionSplitType.None);
        }

        [MenuCommand("Example/Window/Create Docked/Left Scene")]
        public void CreateLeftScene()
        {
            m_wm.CreateWindow(BuiltInWindowNames.Scene, false, RegionSplitType.Left);
        }

        [MenuCommand("Example/Window/Create Docked/Right Scene")]
        public void CreateRightScene()
        {
            m_wm.CreateWindow(BuiltInWindowNames.Scene, false, RegionSplitType.Right);
        }

        [MenuCommand("Example/Window/Create Docked/Top Scene")]
        public void CreateTopScene()
        {
            m_wm.CreateWindow(BuiltInWindowNames.Scene, false, RegionSplitType.Top);
        }

        [MenuCommand("Example/Window/Create Docked/Bottom Scene")]
        public void CreateBottomScenes()
        {
            m_wm.CreateWindow(BuiltInWindowNames.Scene, false, RegionSplitType.Bottom);
        }

        [MenuCommand("Example/Window/Create Popup")]
        public void CreatePopup()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.CursorHelper.SetCursor(this, Utils.KnownCursor.DropAllowed);
            StartCoroutine(CoCreatePopup(editor));
        }

        private IEnumerator CoCreatePopup(IRTE editor)
        {
            while (true)
            {
                if(editor.Input.GetPointerDown(0))
                {
                    Vector3 popupTopLeftScreenPoint = editor.Input.GetPointerXY(0);

                    if (m_wm.ScreenPointToLocalPointInRectangle(m_wm.PopupRoot, popupTopLeftScreenPoint, out Vector2 position))
                    {
                        m_wm.CreatePopup(BuiltInWindowNames.Console, position, true);
                    }

                    editor.CursorHelper.ResetCursor(this);
                    break;
                }
              
                yield return null;
            }
        }

        [MenuCommand("Example/Window/Destroy/Scene")]
        public void DestroyScene()
        {
            var scene = m_wm.GetWindow(BuiltInWindowNames.Scene);
            if(scene != null)
            {
                m_wm.DestroyWindow(scene);
            }
        }

        [MenuCommand("Example/Window/Destroy/Scenes")]
        public void DestroyWindowsOfType()
        {
            m_wm.DestroyWindowsOfType(BuiltInWindowNames.Scene);
        }

        [MenuCommand("Example/Window/Destroy/Active Window")]
        public void DestroyActiveWindow()
        {
            if (m_editor.ActiveWindow != null)
            {
                m_wm.DestroyWindow(m_editor.ActiveWindow.transform);
            }
        }

        [MenuCommand("Example/Misc/Maximize Active Docked Window")]
        public void Maximize()
        {
            var scene = m_wm.GetWindow(BuiltInWindowNames.Scene);
            if (scene != null)
            {
                Region region = Region.FindRegion(scene);
                region.Maximize(true);
            }
        }

        [MenuCommand("Example/Misc/Minimize Active Docked Window")]
        public void Minimize()
        {
            var scene = m_wm.GetWindow(BuiltInWindowNames.Scene);
            if (scene != null)
            {
                Region region = Region.FindRegion(scene);
                region.Maximize(false);
            }
        }
    }
}
