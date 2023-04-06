using Battlehub.UIControls;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.UI
{
    public partial class AutoUI
    {
        public readonly Dictionary<string, string> ControlToEventName = new Dictionary<string, string>
        {
            { nameof(UnityEngine.UI.Button), nameof(UnityEngine.UI.Button.onClick) }
        };

        public readonly Dictionary<string, string> ControlToOneWayPropertyName = new Dictionary<string, string>()
        {
            { nameof(UnityEngine.UI.Image), nameof(UnityEngine.UI.Image.sprite) },
            { nameof(TextMeshProUGUI), nameof(TextMeshProUGUI.text) }
        };

        public object GetViewModel(object viewModel, MemberInfo[] path)
        {
            if(viewModel is Type)
            {
                return null;
            }

            for(int i = 0; i < path.Length; ++i)
            {
                MemberInfo memberInfo = path[i];
                viewModel = PropertyInfo(memberInfo).GetValue(viewModel);
            }

            return viewModel;
        }

        public string GetViewModelFullName(object viewModel, MemberInfo[] path)
        {
            if (viewModel is Type)
            {
                if(path.Length == 0)
                {
                    return ((Type)viewModel).FullName;
                }

                return PropertyInfo(path.Last()).PropertyType.FullName;
            }

            for (int i = 0; i < path.Length; ++i)
            {
                MemberInfo memberInfo = path[i];
                viewModel = PropertyInfo(memberInfo).GetValue(viewModel);
            }

            return viewModel.GetType().FullName;
        }


        public Tuple<Component, Transform> GetControl(Transform rootControl, int[] controlPath, int siblingIndex, Type type)
        {
            for(int i = 0; i < controlPath.Length; ++i)
            {
                rootControl = rootControl.GetChild(controlPath[i]);
            }

            if(siblingIndex >= 0)
            {
                rootControl = rootControl.GetChild(siblingIndex);
            }
            
            return new Tuple<Component, Transform>(rootControl.GetComponentInChildren(type, true), rootControl);
        }
        
        public void Bind(Transform rootControl, object rootViewModel)
        {
            List<(Template Template, object ViewModel)> templates = new List<(Template Template, object ViewModel)>();
            foreach (var controlInfo in m_controls)
            {
                var (control, wrapper) = GetControl(rootControl, controlInfo.ControlPath, controlInfo.SiblingIndex, controlInfo.ControlType);
                string controlName = control.GetType().Name;

                MemberInfo[] path = controlInfo.MemberPath;
                MemberInfo memberInfo = controlInfo.MemberInfo;
                Attribute attribute = controlInfo.Attribute;

                object viewModel = GetViewModel(rootViewModel, path);
                string viewModelFullName = GetViewModelFullName(rootViewModel, path);

                if (wrapper.transform.parent.GetComponent<Template>() == null)
                {
                    Template template = wrapper.transform.parent.gameObject.AddComponent<Template>();
                    template.ViewModelTypeName = viewModelFullName;
                    templates.Add((template, viewModel));
                }

                if (attribute is ActionAttribute)
                {
                    ActionAttribute actionAttribute = (ActionAttribute)attribute;
                    wrapper.gameObject.SetActive(false);
                    EventBinding actionBinding = control.gameObject.AddComponent<EventBinding>();
                    actionBinding.ViewModelMethodName = $"{viewModelFullName}.{memberInfo.Name}";
                    actionBinding.ViewEventName = $"{controlName}.{ControlToEventName[controlName]}";

                    if(actionAttribute.IsInteractableProperty != null && control is Selectable)
                    {
                        OneWayPropertyBinding isInteractableBinding = control.gameObject.AddComponent<OneWayPropertyBinding>();
                        isInteractableBinding.ViewModelPropertyName = $"{viewModelFullName}.{actionAttribute.IsInteractableProperty}";
                        isInteractableBinding.ViewPropertyName = $"{controlName}.{nameof(Selectable.interactable)}";
                    }
                }
                else if(attribute is CollectionPropertyAttribute)
                {
                    CollectionPropertyAttribute collectionAttribute = (CollectionPropertyAttribute)attribute;
                    wrapper.gameObject.SetActive(false);

                    Type iHierarchicalDataInterface = PropertyInfo(memberInfo).PropertyType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHierarchicalData<>)).FirstOrDefault();
                    bool isHierarchicalData = iHierarchicalDataInterface != null;
                    if (isHierarchicalData)
                    {
                        VirtualizingTreeViewBinding binding = control.gameObject.AddComponent<VirtualizingTreeViewBinding>();
                        binding.ViewModelPropertyName = $"{viewModelFullName}.{memberInfo.Name}";

                        if(collectionAttribute.SelectedIndexProperty != null)
                        {
                            binding.SelectionChanged = new UnityEvent();
                            binding.m_twoWayPropertyBindings = new[]
                            {
                                new ControlBinding.TwoWayPropertyBindingSlim
                                {
                                    ViewEventName = $"{nameof(VirtualizingTreeViewBinding)}.{nameof(VirtualizingTreeViewBinding.SelectionChanged)}",
                                    ViewPropertyName = $"{nameof(VirtualizingTreeView)}.{nameof(VirtualizingTreeView.SelectedIndex)}",
                                    ViewModelPropertyName = $"{viewModelFullName}.{collectionAttribute.SelectedIndexProperty}"
                                }
                            };
                        }
                       
                        
                        VirtualizingScrollRect scrollRect = control.GetComponent<VirtualizingScrollRect>();
                        if(scrollRect != null)
                        {
                            VirtualizingItemContainer itemContainer = scrollRect.ContainerPrefab.GetComponent<VirtualizingItemContainer>();
                            Type itemViewModelType = iHierarchicalDataInterface.GetGenericArguments()[0];

                            itemContainer.EditorPresenter.AddComponent<Template>();
                            RectTransform itemLayoutRoot = (RectTransform)itemContainer.ItemPresenter.transform;
                            AutoUI autoUI = new AutoUI();
                            autoUI.BuildUI(itemLayoutRoot, itemViewModelType);
                            autoUI.Bind(itemLayoutRoot, itemViewModelType);   
                        }
                    }
                }
                else if(attribute is PropertyAttribute)
                {
                    PropertyInfo propertyInfo = PropertyInfo(memberInfo);
                    if (propertyInfo.GetSetMethod() != null)
                    {
                        PropertyAttribute propertyAttribute = (PropertyAttribute)attribute;
                        PropertyEditor propertyEditor = control.GetComponentInChildren<PropertyEditor>();
                        if (propertyEditor != null && viewModel != null)
                        {
                            propertyEditor.Init(viewModel, memberInfo, propertyAttribute.Caption, false);
                            propertyEditor.IsLabelVisible = !string.IsNullOrEmpty(propertyAttribute.Caption);

                            if (!propertyEditor.IsLabelVisible)
                            {
                                Transform right = propertyEditor.transform.Find("Right");
                                if (right != null)
                                {
                                    LayoutElement rightLayoutElement = right.GetComponent<LayoutElement>();
                                    if (rightLayoutElement != null)
                                    {
                                        ResetLayoutElement(rightLayoutElement);
                                    }
                                }
                            }

                            wrapper.gameObject.SetActive(false);
                            TryAddIsActiveBinding(wrapper, viewModelFullName, propertyAttribute);
                        }
                        else
                        {
                            Debug.LogWarning("viewModel is null");
                        }
                    }
                    else
                    {
                        wrapper.gameObject.SetActive(false);

                        if (ControlToOneWayPropertyName.TryGetValue(controlName, out string dataPropertyName))
                        {
                            
                            OneWayPropertyBinding oneWayPropertyBinding = control.gameObject.AddComponent<OneWayPropertyBinding>();
                            oneWayPropertyBinding.ViewModelPropertyName = $"{viewModelFullName}.{memberInfo.Name}";
                            oneWayPropertyBinding.ViewPropertyName = $"{controlName}.{dataPropertyName}";
                        }

                        PropertyAttribute propertyAttribute = (PropertyAttribute)attribute;
                        TryAddIsActiveBinding(wrapper, viewModelFullName, propertyAttribute);
                    }
                }
                else if(attribute is ProceduralAttribute)
                {
                    ProceduralAttribute procedural = (ProceduralAttribute)attribute;
                    if(procedural.BindingMethodName != null)
                    {
                        MethodInfo bindMethodInfo = viewModel.GetType().GetMethod(procedural.BindingMethodName);
                        if(bindMethodInfo != null)
                        {
                            bindMethodInfo.Invoke(viewModel, new[] { control });
                        }
                    }
                }
            }

            foreach (var template in templates)
            {
                if(template.ViewModel != null)
                {
                    template.Template.InitChildBindings(template.ViewModel);
                }
            }

            foreach (var controlInfo in m_controls)
            {
                var (control, wrapper) = GetControl(rootControl, controlInfo.ControlPath, controlInfo.SiblingIndex, controlInfo.ControlType);

                control.gameObject.SetActive(true);
                if (control != wrapper)
                {
                    wrapper.gameObject.SetActive(true);
                }
            }

            m_controls.Clear();

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)rootControl);
        }

        private static void TryAddIsActiveBinding(Component control, string viewModelFullName, PropertyAttribute propertyAttribute)
        {
            if (propertyAttribute.IsActiveProperty != null)
            {
                IsActivePropertyBinding isActivePropertyBinding = control.gameObject.AddComponent<IsActivePropertyBinding>();
                isActivePropertyBinding.ViewModelPropertyName = $"{viewModelFullName}.{propertyAttribute.IsActiveProperty}";
                isActivePropertyBinding.ViewPropertyName = $"{nameof(IsActivePropertyBinding)}.{nameof(IsActivePropertyBinding.IsActive)}";
                if (propertyAttribute.InvertIsActiveProperty)
                {
                    isActivePropertyBinding.ViewAdapterTypeName = "UnityWeld.Binding.Adapters.BoolInversionAdapter";
                }
            }
        }


    }
}
