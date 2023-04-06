using Battlehub.RTCommon;
using System.Reflection;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace Battlehub.RTEditor.Binding
{
    [RequireComponent(typeof(PropertyEditor))]
    public class PropertyEditorBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string ViewModelPropertyName
        {
            get { return viewModelPropertyName; }
            set { viewModelPropertyName = value; }
        }

        [SerializeField]
        private string viewModelPropertyName;
        private PropertyWatcher m_viewModelWatcher;
        private PropertyEditor m_propertyEditor;

        [SerializeField]
        private string m_label = null;
        public string Label
        {
            get { return m_label;  }
            set { m_label = value; }
        }

        [SerializeField]
        private bool m_isLabelVisible = true;
        public bool IsLabelVisible
        {
            get { return m_isLabelVisible; }
            set { m_isLabelVisible = value; }
        }

        [SerializeField]
        private bool m_enableUndo = false;
        public bool EnableUndo
        {
            get { return m_enableUndo;  }
            set { m_enableUndo = false; }
        }

        public override void Connect()
        {
            m_propertyEditor = GetComponent<PropertyEditor>();
            m_propertyEditor.IsAutoReloadEnabled = false;
            m_propertyEditor.IsLabelVisible = IsLabelVisible;
            
            string propertyName;
            object viewModel;
            ParseViewModelEndPointReference(viewModelPropertyName, out propertyName, out viewModel);

            PropertyEndPoint viewModelEndPoint = MakeViewModelEndPoint(ViewModelPropertyName, null, null);
            m_viewModelWatcher = viewModelEndPoint.Watch(
                () => m_propertyEditor.Reload(true)
            );

            string label = Label;
            ILocalization localization = IOC.Resolve<ILocalization>();
            if(localization != null && !string.IsNullOrEmpty(label))
            {
                label = localization.GetString(label, label);
            }

            PropertyInfo propertyInfo = viewModel.GetType().GetProperty(propertyName);
            m_propertyEditor.Init(viewModel, propertyInfo, label, EnableUndo);
        }

        public override void Disconnect()
        {
            if(m_propertyEditor != null)
            {
                m_propertyEditor.Init((object)null, null, string.Empty);
                m_propertyEditor = null;
            }
            
            if(m_viewModelWatcher != null)
            {
                m_viewModelWatcher.Dispose();
                m_viewModelWatcher = null;
            }
        }

 
    }

}
