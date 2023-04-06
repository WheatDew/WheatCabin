using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.UI
{
    public partial class AutoUI 
    {
        public void Build(Transform layoutRoot, object viewModel, params object[] dependencies)
        {
            Build((RectTransform)layoutRoot, viewModel, dependencies);
        }

        public void Build(RectTransform layoutRoot, object viewModel, params object[] dependencies)
        {
            BuildUI(layoutRoot, viewModel.GetType());
            BuildViewModel(viewModel, dependencies);
            Bind(layoutRoot, viewModel);
        }

        public void BuildUI(RectTransform layoutRoot, Type viewModelType)
        {
            m_controls.Clear();
            m_panelStack.Clear();
            m_panelStack.Push(layoutRoot);

            BuildUI(new Stack<int>(), new Stack<MemberInfo>(), viewModelType);
        }

        private void BuildUI(Stack<int> controlPath, Stack<MemberInfo> viewModelPropertyPath, Type viewModelType)
        {
            foreach (MemberInfo memberInfo in viewModelType.GetMembers())
            {
                MemberAttribute memberAttribute = memberInfo.GetCustomAttribute<MemberAttribute>();
                if (memberAttribute != null)
                {
                    if (memberAttribute is ActionAttribute)
                    {
                        var layoutAttribute = memberInfo.GetCustomAttribute<LayoutAttribute>();
                        var styleAttribute = memberInfo.GetCustomAttribute<StyleAttribute>();
                        var (button, layoutElement) = Button(layoutAttribute != null, true, styleAttribute);
                        if (memberAttribute.Caption == null)
                        {
                            button.SetText(memberInfo.Name);
                        }
                        else
                        {
                            button.SetText(memberAttribute.Caption);
                        }

                        RegisterControl(controlPath, button.transform.parent.GetSiblingIndex(), button.GetType(), viewModelPropertyPath, memberInfo, memberAttribute, layoutElement, layoutAttribute);
                    }
                    else if (memberAttribute is CollectionPropertyAttribute)
                    {
                        PropertyInfo propertyInfo = PropertyInfo(memberInfo);

                        var collectionAttribute = (CollectionPropertyAttribute)memberAttribute;
                        var layoutAttribute = propertyInfo.GetCustomAttribute<LayoutAttribute>();
                        var styleAttribute = memberInfo.GetCustomAttribute<StyleAttribute>();

                        if (collectionAttribute.SelectedIndexProperty != null)
                        {
                            ItemLayoutAttribute itemLayoutAttribute = GetItemLayoutAttribute(propertyInfo);
                            LayoutGroupAttribute itemLayoutGroupAttribute = GetItemLayoutGroupAttribute(propertyInfo);

                            var (treeView, treeViewItem, layoutElement) = TreeView(layoutAttribute != null, styleAttribute);

                            if(itemLayoutGroupAttribute != null)
                            {
                                var hvLayoutGroup = itemLayoutGroupAttribute as HorizontalOrVerticalLayoutGroupAttribute;
                                if(hvLayoutGroup != null)
                                {
                                    var layoutGroup = treeViewItem.ItemPresenter.GetComponent<HorizontalOrVerticalLayoutGroup>();
                                    if(layoutGroup != null)
                                    {
                                        hvLayoutGroup.CopyTo(layoutGroup);
                                    }
                                }   
                            }

                            RegisterControl(controlPath, treeView.transform.parent.GetSiblingIndex(), treeView.GetType(), viewModelPropertyPath, memberInfo, memberAttribute, layoutElement, layoutAttribute);
                        }
                        else
                        {
                            throw new NotImplementedException("CollectionPropertyAttribute.SelectionIndexProperty is null");
                        }
                    }
                    else if (memberAttribute is PropertyAttribute)
                    {
                        PropertyInfo propertyInfo = PropertyInfo(memberInfo);
                        var layoutAttribute = propertyInfo.GetCustomAttribute<LayoutAttribute>();
                        var styleAttribute = memberInfo.GetCustomAttribute<StyleAttribute>();
                        if (propertyInfo.GetSetMethod() != null)
                        {
                            var (propertyEditor, layoutElement) = PropertyEditor(propertyInfo);

                            HorizontalOrVerticalLayoutGroup propertyEditorLayoutGroup = propertyEditor.GetComponent<HorizontalOrVerticalLayoutGroup>();
                            if(propertyEditorLayoutGroup != null)
                            {
                                propertyEditorLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
                            }

                            RegisterControl(controlPath, propertyEditor.transform.parent.GetSiblingIndex(), propertyEditor.GetType(), viewModelPropertyPath, propertyInfo, memberAttribute, layoutElement, layoutAttribute);
                        }
                        else
                        {
                            if(propertyInfo.PropertyType == typeof(Sprite))
                            {
                                var (image, layoutElement) = Image(layoutAttribute != null, true, styleAttribute);
                                RegisterControl(controlPath, image.transform.parent.GetSiblingIndex(), image.GetType(), viewModelPropertyPath, propertyInfo, memberAttribute, layoutElement, layoutAttribute);
                            }
                            else if(propertyInfo.PropertyType == typeof(Space))
                            {
                                var (space, layoutElement) = Space(layoutAttribute != null, styleAttribute);
                                RegisterControl(controlPath, space.GetSiblingIndex(), space.GetType(), viewModelPropertyPath, propertyInfo, memberAttribute, layoutElement, layoutAttribute);
                            }
                            else if(propertyInfo.PropertyType == typeof(string))
                            {
                                var (label, layoutElement) = Label(layoutAttribute != null, true, styleAttribute);
                                RegisterControl(controlPath, label.transform.parent.GetSiblingIndex(), label.GetType(), viewModelPropertyPath, propertyInfo, memberAttribute, layoutElement, layoutAttribute);
                            }
                        }
                    }
                }
                else
                {
                    ProceduralAttribute proceduralAttribute = memberInfo.GetCustomAttribute<ProceduralAttribute>();
                    if(proceduralAttribute != null)
                    {
                        var proceduralWithRectTransform = (Action<RectTransform>)Delegate.CreateDelegate(typeof(Action<RectTransform>), (MethodInfo)memberInfo, false);
                        if(proceduralWithRectTransform != null)
                        {
                            RectTransform panel = m_panelStack.Peek();
                            proceduralWithRectTransform.Invoke(panel);

                            RegisterControl(controlPath, -1, panel.GetType(), viewModelPropertyPath, memberInfo, proceduralAttribute);
                        }
                        else
                        {
                            var proceduralWithAutoUI = (Action<AutoUI>)Delegate.CreateDelegate(typeof(Action<AutoUI>), (MethodInfo)memberInfo, false);
                            if(proceduralWithAutoUI != null)
                            {
                                RectTransform panel = m_panelStack.Peek();
                                RegisterControl(controlPath, -1, panel.GetType(), viewModelPropertyPath, memberInfo, proceduralAttribute);

                                proceduralWithAutoUI.Invoke(this);
                            }
                        }
                    }
                    else
                    {
                        LayoutGroupAttribute layoutGroupAttribute = memberInfo is PropertyInfo ? GetLayoutGroupAttribute(PropertyInfo(memberInfo)) : null;
                        if (layoutGroupAttribute != null)
                        {
                            if (layoutGroupAttribute is HorizontalLayoutGroupAttribute)
                            {
                                BuildHorizontalOrVerticalLayout(BeginHorizontalLayout, EndHorizontalLayout, controlPath, viewModelPropertyPath, memberInfo, layoutGroupAttribute);
                            }
                            else if (layoutGroupAttribute is VerticalLayoutGroupAttribute)
                            {
                                BuildHorizontalOrVerticalLayout(BeginVerticalLayout, EndVerticalLayout, controlPath, viewModelPropertyPath, memberInfo, layoutGroupAttribute);
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                    }
                }
            }
        }

        private void RegisterControl(Stack<int> controlPath, int controlSiblingIndex, Type controlType, Stack<MemberInfo> propertyPath, MemberInfo memberInfo, Attribute memberAttribute, LayoutElement layoutElement = null, LayoutAttribute layoutAttribute = null)
        {
            if (layoutElement != null)
            {
                if (layoutAttribute != null)
                {
                    layoutAttribute.CopyTo(layoutElement);
                }
                else
                {
                    ResetLayoutElement(layoutElement);
                }

                if(layoutElement.ignoreLayout)
                {
                    RectTransform rt = (RectTransform)layoutElement.transform;
                    rt.Stretch();
                }
            }

            m_controls.Add((controlPath.Reverse().Skip(1).ToArray(), controlSiblingIndex, controlType, propertyPath.Reverse().ToArray(), memberInfo, memberAttribute));
        }

        private void BuildHorizontalOrVerticalLayout<T>(Func<bool, Tuple<T, LayoutElement>> beginLayout,  Action endLayout, Stack<int> controlPath,  Stack<MemberInfo> viewModelPropertyPath, MemberInfo memberInfo, LayoutGroupAttribute layoutGroupAttribute) where T : HorizontalOrVerticalLayoutGroup
        {
            var layoutAttribute = PropertyInfo(memberInfo).GetCustomAttribute<LayoutAttribute>();
            var (layoutGroup, layoutElement) = beginLayout(layoutAttribute != null);

            viewModelPropertyPath.Push(memberInfo);
            controlPath.Push(layoutGroup.transform.GetSiblingIndex());

            layoutGroup.name = memberInfo.Name;

            if (layoutGroupAttribute != null)
            {
                var hvLayoutGroup = (HorizontalOrVerticalLayoutGroupAttribute)layoutGroupAttribute;
                hvLayoutGroup.CopyTo(layoutGroup);
            }

            if (layoutElement != null)
            {
                layoutAttribute.CopyTo(layoutElement);
            }

            BuildUI(controlPath, viewModelPropertyPath, PropertyInfo(memberInfo).PropertyType);
            endLayout();

            viewModelPropertyPath.Pop();
            controlPath.Pop();
        }

        public void BuildViewModel(object viewModel, params object[] dependencies)
        {
            IList<object> depsList = new List<object>(dependencies);
            InstantiateLayoutGroups(viewModel, depsList);
            InjectDependencies(viewModel, depsList);
            WireUpEvents(viewModel, depsList);
        }

        private void InstantiateLayoutGroups(object viewModel, IList<object> layoutGroups)
        {
            foreach (PropertyInfo propertyInfo in viewModel.GetType().GetProperties())
            {
                LayoutGroupAttribute layoutGroupAttribute = GetLayoutGroupAttribute(propertyInfo);
                if (layoutGroupAttribute != null)
                {
                    object layoutGroupViewModel = propertyInfo.GetValue(viewModel);
                    if (layoutGroupViewModel == null)
                    {
                        layoutGroupViewModel = Activator.CreateInstance(propertyInfo.PropertyType);
                        propertyInfo.SetValue(viewModel, layoutGroupViewModel);
                    }
                    layoutGroups.Add(layoutGroupViewModel);
                    InstantiateLayoutGroups(layoutGroupViewModel, layoutGroups);
                }
            }
        }


        private void InjectDependencies(object viewModel, IList<object> deps)
        {
            foreach (PropertyInfo propertyInfo in viewModel.GetType().GetProperties())
            {
                InjectAttribute injectAttribute = propertyInfo.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
                {
                    object injectPropertyValue = propertyInfo.GetValue(viewModel);
                    if (injectPropertyValue == null)
                    {
                        injectPropertyValue = deps.Where(dep => dep.GetType() == propertyInfo.PropertyType).FirstOrDefault();
                        propertyInfo.SetValue(viewModel, injectPropertyValue);
                    }
                }

                LayoutGroupAttribute layoutGroupAttribute = GetLayoutGroupAttribute(propertyInfo);
                if (layoutGroupAttribute != null)
                {
                    InjectDependencies(propertyInfo.GetValue(viewModel), deps);
                }
            }
        }

        private void WireUpEvents(object viewModel, IList<object> deps)
        {
            foreach (MemberInfo memberInfo in viewModel.GetType().GetMembers())
            {
                if(memberInfo is MethodInfo)
                {
                    PropertyChangedEventHandlerAttribute propChangedEventHandlerAttribute = memberInfo.GetCustomAttribute<PropertyChangedEventHandlerAttribute>();
                    if (propChangedEventHandlerAttribute != null)
                    {
                        Type senderType = propChangedEventHandlerAttribute.Sender;

                        for (int i = 0; i < deps.Count; ++i)
                        {
                            if (deps[i].GetType() == senderType)
                            {
                                INotifyPropertyChanged notifyPropertyChanged = deps[i] as INotifyPropertyChanged;
                                if (notifyPropertyChanged == null)
                                {
                                    Debug.LogWarning("INotifyPropertyChanged is not implemented on " + notifyPropertyChanged);
                                }
                                else
                                {
                                    var eventHandler = (PropertyChangedEventHandler)Delegate.CreateDelegate(typeof(PropertyChangedEventHandler), viewModel, (MethodInfo)memberInfo);
                                    notifyPropertyChanged.PropertyChanged += eventHandler;
                                }
                            }
                        }
                    }
                    CollectionChangedEventHandlerAttribute collectionChangedEventHandlerAttribute = memberInfo.GetCustomAttribute<CollectionChangedEventHandlerAttribute>();
                    if(collectionChangedEventHandlerAttribute != null)
                    {
                        Type senderType = collectionChangedEventHandlerAttribute.Sender;
                        for (int i = 0; i < deps.Count; ++i)
                        {
                            if (deps[i].GetType() == senderType)
                            {
                                INotifyCollectionChanged notifyCollectionChanged = deps[i] as INotifyCollectionChanged;
                                if (notifyCollectionChanged == null)
                                {
                                    Debug.LogWarning("INotifyCollectionChanged is not implemented on " + notifyCollectionChanged);
                                }
                                else
                                {
                                    var eventHandler = (NotifyCollectionChangedEventHandler)Delegate.CreateDelegate(typeof(NotifyCollectionChangedEventHandler), viewModel, (MethodInfo)memberInfo);
                                    notifyCollectionChanged.CollectionChanged += eventHandler;
                                }
                            }
                        }
                    }

                }
                else if(memberInfo is PropertyInfo)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                    LayoutGroupAttribute layoutGroupAttribute = GetLayoutGroupAttribute(propertyInfo);
                    if (layoutGroupAttribute != null)
                    {
                        WireUpEvents(propertyInfo.GetValue(viewModel), deps);
                    }
                }
            }
        }

        private static PropertyInfo PropertyInfo(MemberInfo memberInfo)
        {
            return (PropertyInfo)memberInfo;
        }

        private static LayoutGroupAttribute GetLayoutGroupAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes<LayoutGroupAttribute>().Where(attr => !attr.GetType().Name.StartsWith("Item")).FirstOrDefault();
        }

        private static LayoutGroupAttribute GetItemLayoutGroupAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes<LayoutGroupAttribute>().Where(attr => attr.GetType().Name.StartsWith("Item")).FirstOrDefault();
        }

        private static LayoutAttribute GetLayoutAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes<LayoutAttribute>().Where(attr => attr.GetType() == typeof(LayoutAttribute)).FirstOrDefault();
        }

        private static ItemLayoutAttribute GetItemLayoutAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes<ItemLayoutAttribute>().Where(attr => attr.GetType() == typeof(ItemLayoutAttribute)).FirstOrDefault();
        }

        private static void ResetLayoutElement(LayoutElement le)
        {
            le.minWidth = -1;
            le.minHeight = -1;
            le.flexibleHeight = -1;
            le.flexibleWidth = -1;
            le.preferredWidth = -1;
            le.preferredHeight = -1;
        }

    }

}
