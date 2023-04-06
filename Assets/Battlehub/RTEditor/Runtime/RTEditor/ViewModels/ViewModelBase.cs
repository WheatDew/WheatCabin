using System;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    public abstract class ViewModelBase : MonoBehaviour, INotifyPropertyChanged, IViewModelProvider
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        public static void ReplaceWith<T>(UnityEngine.Component component) where T : ViewModelBase
        {
            ReplaceWith(typeof(T), component.gameObject);
        }

        public static void ReplaceWith<T>(GameObject go) where T : ViewModelBase
        {
            ReplaceWith(typeof(T), go);
        }

        public static void ReplaceWith(Type type, UnityEngine.Component component)
        {
            ReplaceWith(type, component.gameObject);
        }

        public static void ReplaceWith(Type type, GameObject go)
        {
            ViewModelBase oldViewModel = go.GetComponent<ViewModelBase>();
            if (oldViewModel == null)
            {
                Debug.Log($"ViewModel to replace was not found");
                return;
            }

            AbstractMemberBinding[] bindings = oldViewModel.GetComponentsInChildren<AbstractMemberBinding>();

            ViewModelBase viewModel = (ViewModelBase)go.AddComponent(type);
            IViewModelProvider viewModelProvider = oldViewModel;
            viewModel.m_viewModelTypeName = viewModelProvider.GetViewModelTypeName();

            DestroyImmediate(oldViewModel);

            foreach (var binding in bindings)
            {
                binding.Init();
            }
        }

        object IViewModelProvider.GetViewModel()
        {
            return this;
        }

        private string m_viewModelTypeName;
        string IViewModelProvider.GetViewModelTypeName()
        {
            if (string.IsNullOrEmpty(m_viewModelTypeName))
            {
                m_viewModelTypeName = GetType().FullName;
            }
            return m_viewModelTypeName;
        }

    }

}
