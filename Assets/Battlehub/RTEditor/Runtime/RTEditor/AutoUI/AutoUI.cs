using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor.UI
{
    public partial class AutoUI : IDisposable
    {
        private List<(int[] ControlPath, int SiblingIndex, Type ControlType, MemberInfo[] MemberPath, MemberInfo MemberInfo, Attribute Attribute)> m_controls;
        private Stack<RectTransform> m_panelStack;
        private IEditorsMap m_editorsMap;
        private IControlsMap m_controlsMap;
        public AutoUI()
        {
            m_panelStack = new Stack<RectTransform>();
            m_controls = new List<(int[] ControlPath, int SiblingIndex, Type ControlType, MemberInfo[] MemberPath, MemberInfo MemberInfo, Attribute Attribute)>();
            m_editorsMap = IOC.Resolve<IEditorsMap>();
            m_controlsMap = IOC.Resolve<IControlsMap>();
        }

        public AutoUI(RectTransform panel) : this()
        {
            m_panelStack.Push(panel);
        }

        public void Dispose()
        {
            m_controls = null;
            m_panelStack = null;
            m_editorsMap = null;
            m_controlsMap = null;
        }

        public Transform CreateDialog(object viewModel, params object[] dependencies)
        {
            return CreateDialog(250, 250, 700, 400, true, viewModel, dependencies);
        }

        public Transform CreateDialog(float width, float height, object viewModel, params object[] dependencies)
        {
            return CreateDialog(width, height, false, viewModel, dependencies);
        }

        public Transform CreateDialog(float width, float height, bool canResize, object viewModel, params object[] dependencies)
        {
            return CreateDialog(width, height, width, height, canResize, viewModel, dependencies);
        }

        public Transform CreateDialog(float minWidth, float minHeight, float preferredWith, float preferredHeight, bool canResize, object viewModel, params object[] dependencies)
        {
            List<Tuple<string, Action>> okActions = new List<Tuple<string, Action>>();
            List<Tuple<string, Action>> cancelActions = new List<Tuple<string, Action>>();
            List<Tuple<string, Action>> altActions = new List<Tuple<string, Action>>();
            GetDialogActions(viewModel, okActions, cancelActions, altActions);

            DialogAction<DialogCancelArgs> okAction = null;
            if(okActions.Count > 0)
            {
                okAction = (dlg, args) =>
                {
                    foreach(var action in okActions)
                    {
                        action.Item2.Invoke();
                    }
                };
            }

            DialogAction<DialogCancelArgs> cancelAction = null;
            if(cancelActions.Count > 0)
            {
                cancelAction = (dlg, args) =>
                {
                    foreach (var action in cancelActions)
                    {
                        action.Item2.Invoke();
                    }
                };
            }

            DialogAction<DialogCancelArgs> altAction = null;
            if(altActions.Count > 0)
            {
                altAction = (dlg, args) =>
                {
                    foreach(var action in altActions)
                    {
                        action.Item2.Invoke();
                    }
                };
            }

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            Transform layoutRoot = wm.CreateDialogWindow(RuntimeWindowType.EmptyDialog.ToString(), "Auto UI Example", okAction, cancelAction, minWidth, minHeight, preferredWith, preferredHeight, canResize);
            Dialog dialog = layoutRoot.GetComponentInParent<Dialog>();
            dialog.IsOkVisible = okAction != null;
            if(okAction != null)
            {
                dialog.OkText = okActions.Where(a => a.Item1 != null).Select(a => a.Item1).FirstOrDefault();
            }
            dialog.IsCancelVisible = cancelAction != null;
            if (cancelAction != null)
            {
                dialog.CancelText = cancelActions.Where(a => a.Item1 != null).Select(a => a.Item1).FirstOrDefault();
            }
            dialog.IsAltVisible = altAction != null;
            if(altAction != null)
            {
                dialog.AltText = altActions.Where(a => a.Item1 != null).Select(a => a.Item1).FirstOrDefault();
                dialog.AltAction = altAction;
            }

            Build(layoutRoot.GetComponentInChildren<RuntimeWindow>().transform, viewModel, dependencies);
            return layoutRoot;
        }

        private void GetDialogActions(object viewModel, List<Tuple<string, Action>> okActions, List<Tuple<string, Action>> cancelActions, List<Tuple<string, Action>> altActions)
        {
            foreach (MemberInfo memberInfo in viewModel.GetType().GetMembers())
            {
                if(memberInfo is MethodInfo)
                {
                    DialogOkActionAttribute okAttribute = memberInfo.GetCustomAttribute<DialogOkActionAttribute>();
                    if (okAttribute != null)
                    {
                        Action action = (Action)Delegate.CreateDelegate(typeof(Action), viewModel, (MethodInfo)memberInfo, false);
                        if(action != null)
                        {
                            okActions.Add(new Tuple<string, Action>(okAttribute.Caption, action));
                        }
                    }

                    DialogCancelActionAttribute cancelAttribute = memberInfo.GetCustomAttribute<DialogCancelActionAttribute>();
                    if (cancelAttribute != null)
                    {
                        Action action = (Action)Delegate.CreateDelegate(typeof(Action), viewModel, (MethodInfo)memberInfo, false);
                        if (action != null)
                        {
                            cancelActions.Add(new Tuple<string, Action>(cancelAttribute.Caption, action));
                        }
                    }

                    DialogAltActionAttribute altAttribute = memberInfo.GetCustomAttribute<DialogAltActionAttribute>();
                    if(altAttribute != null)
                    {
                        Action action = (Action)Delegate.CreateDelegate(typeof(Action), viewModel, (MethodInfo)memberInfo, false);
                        if(action != null)
                        {
                            altActions.Add(new Tuple<string, Action>(altAttribute.Caption, action));
                        }
                    }
                }
                else if(memberInfo is PropertyInfo)
                {
                    object childViewModel = PropertyInfo(memberInfo).GetValue(viewModel);
                    if(childViewModel != null)
                    {
                        GetDialogActions(childViewModel, okActions, cancelActions, altActions);
                    }
                }
            }
        }
    }
}

