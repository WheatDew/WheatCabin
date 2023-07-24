/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for usable character item actions.
    /// </summary>
    [CustomEditor(typeof(UsableAction), true)]
    public class UsableCharacterItemActionInspector : CharacterItemActionInspector
    {
        protected override List<string> ExcludedFields => new List<string>()
        {
            "m_ID", "m_ActionName", "m_ActionDescription", "m_DebugLogger"
        };

        protected UsableAction m_UsableAction;
        protected VisualElement m_DebugVisualElement;

        protected HelpBox m_HelpBox;
        protected ModuleDebugInfosVisualElement m_Infos;

        protected ModuleInterfaceInfosVisualElement m_InterfaceInfos;


        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_UsableAction = target as UsableAction;
            m_UsableAction.DebugLogger.InspectorActive = true;
            base.InitializeInspector();
        }

        /// <summary>
        /// Adds the custom UIElements to the top of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowHeaderElements(VisualElement container)
        {
            base.ShowHeaderElements(container);
            FieldInspectorView.AddField(target, target, "m_ID", container, null, null);
            FieldInspectorView.AddField(target, target, "m_ActionName", container, null, null);
            FieldInspectorView.AddField(target, target, "m_ActionDescription", container, null, null);

            Foldout m_Foldout = new Foldout();
            m_Foldout.text = "Debug";
            m_DebugVisualElement = new VisualElement();
            m_Foldout.Add(m_DebugVisualElement);
            container.Add(m_Foldout);
            
            FieldInspectorView.AddField(target, target, "m_DebugLogger", m_DebugVisualElement, null, null);

            m_HelpBox = new HelpBox("The logger contains information about the state of your usable item. " +
                                    "This is useful for determining why the action isn't working as expected " +
                                    "or logging the status to the console.", HelpBoxMessageType.Info);
            m_DebugVisualElement.Add(m_HelpBox);

            if (!Application.isPlaying || !m_CharacterItemAction.IsInitialized) {
                return;
            }

            m_Infos = new ModuleDebugInfosVisualElement(m_UsableAction);
            m_DebugVisualElement.Add(m_Infos);

            m_InterfaceInfos = new ModuleInterfaceInfosVisualElement(m_UsableAction);
            m_DebugVisualElement.Add(m_InterfaceInfos);

            AddBasicDebugInfos();

            m_Infos.RefreshAllInfos();
        }

        /// <summary>
        /// The default debug infos to display to get a starting order, as the other infos will be added dynamically.
        /// </summary>
        protected virtual void AddBasicDebugInfos()
        {
            AddInfo(UsableAction.InfoKey_ItemUseState, "The use state of the item.");
            AddInfo(UsableAction.InfoKey_UseAbilityActive, "True if the item ability is active.");
            AddInfo(UsableAction.InfoKey_CanStartUseAbility, "The use ability will check if it can start.");
            AddInfo(UsableAction.InfoKey_CanStartUseItem, "The use ability will check if it can start using the item.");
            AddInfo(UsableAction.InfoKey_CanUseItem, "Can the item be used?");
            AddInfo(UsableAction.InfoKey_IsItemUsePending, "Is the item use pending.");
            AddInfo(UsableAction.InfoKey_CanStopItemUse, "Can the item be stopped.");
            AddInfo(UsableAction.InfoKey_CanStopAbility, "Can the item ability be stopped.");
            AddInfo(UsableAction.InfoKey_UseItemSubstateIndex, "The item substate index used for the animation.");
            AddInfo(UsableAction.InfoKey_StartUseCountSinceAbilityStart, "");
            AddInfo(UsableAction.InfoKey_UseCountSinceAbilityStart, "");
            AddInfo(UsableAction.InfoKey_UseCompleteCountSinceAbilityStart, "");
        }

        /// <summary>
        /// Add a new debug info to display.
        /// </summary>
        /// <param name="infoKey">The info key.</param>
        /// <param name="infoTooltip">The info tooltip.</param>
        protected virtual void AddInfo(string infoKey, string infoTooltip)
        {
            m_Infos.AddInfo(infoKey, infoTooltip);
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            if (m_UsableAction != null) {
                m_UsableAction.DebugLogger.InspectorActive = false;
            }

            if (m_Infos != null) {
                m_Infos.OnDestroy();
            }

            if (m_InterfaceInfos != null) {
                m_InterfaceInfos.OnDestroy();
            }
        }
    }

    /// <summary>
    /// A visual Element used to show the Debug Infos of the modules in the inspector.
    /// </summary>
    public class ModuleDebugInfosVisualElement : VisualElement
    {
        protected UsableAction m_Action;
        public UsableAction Action => m_Action;

        protected Dictionary<string, ActionInfoLabel> m_InfoLabels;
        protected Dictionary<string, Foldout> m_InfoFoldouts;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">The usable action.</param>
        public ModuleDebugInfosVisualElement(UsableAction action)
        {
            m_Action = action;
            m_InfoLabels = new Dictionary<string, ActionInfoLabel>();
            m_InfoFoldouts = new Dictionary<string, Foldout>();

            Action.DebugLogger.OnInfoChange += HandleInfoChange;
        }

        /// <summary>
        /// Refresh the infos shown.
        /// </summary>
        public void RefreshAllInfos()
        {
            var infoDictionary = m_Action.DebugLogger.InfoDictionary;
            if(infoDictionary == null){ return; }

            foreach (var infoKeyValue in infoDictionary) {
                HandleInfoChange(infoKeyValue.Key);
            }
        }

        /// <summary>
        /// Update the info displayed when there is a change.
        /// </summary>
        /// <param name="infoKey">The info key that changed.</param>
        private void HandleInfoChange(string infoKey)
        {
            var infoDictionary = Action.DebugLogger.InfoDictionary;

            if (m_InfoLabels.TryGetValue(infoKey, out var infoLabel)) {
                infoLabel.SetInfoMessage(infoDictionary[infoKey]);
            } else {
                AddInfo(infoKey, infoDictionary[infoKey]);
            }
        }

        /// <summary>
        /// Add a new debug info to display.
        /// </summary>
        /// <param name="infoKey">The info key.</param>
        /// <param name="infoTooltip">The info tooltip.</param>
        public void AddInfo(string infoKey, string infoTooltip)
        {
            // Already added.
            if (m_InfoLabels.TryGetValue(infoKey, out var infoLabel)) {
                return;
            }

            var index = infoKey.LastIndexOf('/');
            var category = index == -1 ? "Other" : infoKey.Substring(0, index);
            var title = index == -1 ? infoKey : infoKey.Substring(index + 1);
            title = ObjectNames.NicifyVariableName(title);

            infoLabel = new ActionInfoLabel(Action, title, infoTooltip);

            if (m_Action.DebugLogger.InfoDictionary.TryGetValue(infoKey, out var info)) {
                infoLabel.SetInfoMessage(info);
            }

            var foldout = GetOrCreateFoldout(category);
            foldout.Add(infoLabel);
            m_InfoLabels[infoKey] = infoLabel;
        }

        /// <summary>
        /// Get the foldout or create a new one if none exist, for the category key.
        /// </summary>
        /// <param name="categoryKey">The category key of the foldout to return.</param>
        /// <returns>The foldout fo the category key.</returns>
        private Foldout GetOrCreateFoldout(string categoryKey)
        {
            Foldout foldout = null;
            if (m_InfoFoldouts.TryGetValue(categoryKey, out foldout)) {
                return foldout;
            }

            foldout = new Foldout();
            m_InfoFoldouts[categoryKey] = foldout;

            var index = categoryKey.LastIndexOf('/');
            if (index == -1) {
                foldout.text = ObjectNames.NicifyVariableName(categoryKey);
                Add(foldout);
                return foldout;
            }

            var parentCategorykey = categoryKey.Substring(0, index);
            var parentFoldout = GetOrCreateFoldout(parentCategorykey);

            var thisCategory = categoryKey.Substring(index + 1);
            foldout.text = ObjectNames.NicifyVariableName(thisCategory);
            parentFoldout.Add(foldout);

            return foldout;
        }

        /// <summary>
        /// Stop listening to the event when the object is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            Action.DebugLogger.OnInfoChange -= HandleInfoChange;
        }

        /// <summary>
        /// A custom label for showing an action info.
        /// </summary>
        public class ActionInfoLabel : Label
        {
            protected UsableAction m_UsableAction;
            protected string m_Title;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="action">The usable action object.</param>
            /// <param name="title">The title of the info.</param>
            /// <param name="tooltip">The tooltip of the info.</param>
            public ActionInfoLabel(UsableAction action, string title, string tooltip)
            {
                m_UsableAction = action;
                m_Title = title;
                this.tooltip = tooltip;
                this.text = m_Title + ": 'Uninitialized' ";
            }

            /// <summary>
            /// Update the info message.
            /// </summary>
            /// <param name="message">The new info message.</param>
            public void SetInfoMessage(string message)
            {
                this.text = m_Title + ": " + message;
            }
        }
    }

    /// <summary>
    /// A visual element used to show all the interfaces used by each module in the Usable Action.
    /// </summary>
    public class ModuleInterfaceInfosVisualElement : VisualElement
    {
        protected UsableAction m_Action;
        public UsableAction Action => m_Action;

        protected static List<Type> m_CharacterItemActionModuleInterfaces;
        protected static Dictionary<Type, List<ActionModule>> m_ModulesByInterface;
        protected Foldout m_InterfacesInfoFoldout;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">The usable action.</param>
        public ModuleInterfaceInfosVisualElement(UsableAction action)
        {
            m_Action = action;

            Action.OnModuleAddedE += HandleModuleAdded;
            Action.OnModuleRemovedE += HandleModuleRemoved;

            m_InterfacesInfoFoldout = new Foldout();
            m_InterfacesInfoFoldout.text = "Interfaces info (Right Click to print more detail.)";
            m_InterfacesInfoFoldout.value = false;
            Add(m_InterfacesInfoFoldout);

            RefreshInterfacesInfo();
        }

        /// <summary>
        /// Handle a module being removed from the usable action.
        /// </summary>
        /// <param name="module">The module that was removed.</param>
        private void HandleModuleRemoved(ActionModule module)
        {
            RefreshInterfacesInfo();
        }

        /// <summary>
        /// Handle a module being added from the usable action.
        /// </summary>
        /// <param name="module">The module that was added.</param>
        private void HandleModuleAdded(ActionModule module)
        {
            RefreshInterfacesInfo();
        }

        /// <summary>
        /// Refresh the interfaces info displayed.
        /// </summary>
        private void RefreshInterfacesInfo()
        {
            m_InterfacesInfoFoldout.Clear();

            if (m_CharacterItemActionModuleInterfaces == null) {
                var moduleType = typeof(IActionModule);
                m_CharacterItemActionModuleInterfaces = new List<Type>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++) {
                    var types = assemblies[i].GetTypes();
                    for (int j = 0; j < types.Length; j++) {
                        var type = types[j];
                        if (moduleType.IsAssignableFrom(type) && moduleType != type && type.IsInterface) {
                            m_CharacterItemActionModuleInterfaces.Add(type);
                        }
                    }
                }
            }

            m_ModulesByInterface = new Dictionary<Type, List<ActionModule>>();

            var allModuleGroups = Action.AllModuleGroups;
            // This can happen if the inspector is opened on a prefab at runtime.
            if (allModuleGroups == null) {
                return;
            }

            for (int i = 0; i < allModuleGroups.Count; i++) {
                var moduleGroup = allModuleGroups[i];
                for (int j = 0; j < moduleGroup.BaseModules.Count; j++) {
                    var module = moduleGroup.BaseModules[j];

                    for (int k = 0; k < m_CharacterItemActionModuleInterfaces.Count; k++) {
                        var interfaceType = m_CharacterItemActionModuleInterfaces[k];

                        if (interfaceType.IsInstanceOfType(module)) {
                            if (m_ModulesByInterface.TryGetValue(interfaceType, out var moduleList)) {

                            } else {
                                moduleList = new List<ActionModule>();
                                m_ModulesByInterface[interfaceType] = moduleList;
                            }
                            moduleList.Add(module);
                        }
                    }
                }
            }

            foreach (var keyValuePair in m_ModulesByInterface) {
                var interfaceType = keyValuePair.Key;
                var moduleList = keyValuePair.Value;

                var infoLabel =
                    new ModuleInterfaceInfoLabel(Action, interfaceType, moduleList);
                m_InterfacesInfoFoldout.Add(infoLabel);
            }
        }

        /// <summary>
        /// Stop listening to events on destroy.
        /// </summary>
        public void OnDestroy()
        {
            Action.OnModuleAddedE -= HandleModuleAdded;
            Action.OnModuleRemovedE -= HandleModuleRemoved;
        }

        /// <summary>
        /// A custom label used for displaying the module interfaces info.
        /// </summary>
        public class ModuleInterfaceInfoLabel : Label
        {
            protected UsableAction m_UsableAction;
            private Type m_InterfaceType;
            protected List<ActionModule> m_Modules;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="action">The usable action.</param>
            /// <param name="interfaceType">The interface type used by the module.</param>
            /// <param name="modules">The list of modules using that interface.</param>
            public ModuleInterfaceInfoLabel(UsableAction action, Type interfaceType, List<ActionModule> modules)
            {
                m_UsableAction = action;
                m_InterfaceType = interfaceType;
                m_Modules = modules;

                RegisterCallback<MouseUpEvent>(HandleRightClick);

                this.text = interfaceType.Name + $" ({modules.Count})";
                var infoTooltip =
                    $"There are {modules.Count} modules that inherit the interface '{interfaceType.Name}', right click to log them:\n";

                for (int i = 0; i < modules.Count; i++) {
                    var module = modules[i];
                    infoTooltip += module.ToString() + "\n";
                }

                tooltip = infoTooltip;
            }

            /// <summary>
            /// Handle a right click to print the modules using that interface.
            /// </summary>
            /// <param name="evt">The click event.</param>
            private void HandleRightClick(MouseUpEvent evt)
            {
                if (evt.button != (int)MouseButton.RightMouse)
                    return;

                var targetElement = evt.target as VisualElement;
                if (targetElement == null)
                    return;

                var menu = new GenericMenu();

                if (m_InterfaceType != null) {
                    // Add a single menu item.
                    menu.AddItem(new GUIContent($"Log modules with interface '{m_InterfaceType.Name}'."), false,
                        LogModulesWithInterface);
                }


                // Get position of menu on top of target element.
                var menuPosition = new Vector2(targetElement.layout.xMin, targetElement.layout.height);
                menuPosition = this.LocalToWorld(menuPosition);
                var menuRect = new Rect(menuPosition, Vector2.zero);

                menu.DropDown(menuRect);
            }

            /// <summary>
            /// Log the modules using the selected interface in the console.
            /// </summary>
            private void LogModulesWithInterface()
            {
                var log = "";

                for (int i = 0; i < m_Modules.Count; i++) {
                    var module = m_Modules[i];
                    var active = module.IsActive ? "[Active]  " : "[Inactive]";
                    var enabled = module.Enabled ? "[Enabled] " : "[Disabled]";
                    log += $"{active}{enabled} {module.ToString()}\n";
                }

                var logHeader = $"The ({ m_Modules.Count}) Modules inheriting the '{m_InterfaceType.Name}' interface are listed below:\n";
                Debug.Log(logHeader + log);
            }
        }
    }
}