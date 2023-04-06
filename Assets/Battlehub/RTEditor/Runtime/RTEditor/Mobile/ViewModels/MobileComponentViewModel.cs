using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
    [Binding]
    public class MobileComponentViewModel
    {
        private IList<Component> m_components;

        [Binding]
        public IList<Component> Components
        {
            get { return m_components; }
        }

        private Type ComponentType
        {
            get { return m_components.First().GetType(); }
        }
    
        private string m_name;

        [Binding]
        public string Name
        {
            get { return m_name; }
        }

        private Sprite m_icon;

        [Binding]
        public Sprite Icon
        {
            get { return m_icon; }
        }

        [Binding]
        public bool CanPin
        {
            get { return IOC.Resolve<IMobileEditorModel>() == null; }
        }

        public static readonly MobileComponentViewModel Empty = new MobileComponentViewModel();
        private MobileComponentViewModel()
        {
        }

        public MobileComponentViewModel(IList<Component> components)
        {
            m_components = components;
           
            IRTE editor = IOC.Resolve<IRTE>();
            IComponentDescriptor componentDescriptor = GetComponentDescriptor();
            if (componentDescriptor != null)
            {
                m_name = componentDescriptor.GetHeaderDescriptor(editor).DisplayName;
            }
            else
            {
                string typeName = ComponentType.Name;
                ILocalization localization = IOC.Resolve<ILocalization>();
                m_name = localization != null ? localization.GetString($"ID_RTEditor_CD_{typeName}", typeName) : typeName;
            }

            ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
            BuiltInWindowsSettings settings;
            if (settingsComponent == null)
            {
                settings = BuiltInWindowsSettings.Default;
            }
            else
            {
                settings = settingsComponent.BuiltInWindowsSettings;
            }

            InspectorSettings.ComponentEditorSettings componentEditorSettings = settings.Inspector.ComponentEditor;            
            IComponentDescriptor descriptor = GetComponentDescriptor();
            Sprite icon = componentDescriptor != null ?
                       descriptor.GetHeaderDescriptor(editor).Icon :
                       componentEditorSettings.Icon;

            if (icon == null && settingsComponent.SelectedTheme != null)
            {
                icon = settingsComponent.SelectedTheme.GetIcon($"{ComponentType.Name} Icon");
                if (icon == null)
                {
                    icon = settingsComponent.SelectedTheme.GetIcon("RTE_Component_Default Icon");
                }
            }

            m_icon = icon;
        }

        private IComponentDescriptor GetComponentDescriptor()
        {
            IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
            IComponentDescriptor componentDescriptor;
            if (editorsMap.ComponentDescriptors.TryGetValue(ComponentType, out componentDescriptor))
            {
                return componentDescriptor;
            }
            return null;
        }
    }
}

