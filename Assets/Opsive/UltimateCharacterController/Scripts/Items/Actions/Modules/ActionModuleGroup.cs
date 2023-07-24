/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An attribute to add on a Character Item Action Module Group to specify how to draw it in the custom inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ActionModuleGroupAttribute : Attribute
    {
        public string IconGuid;

        /// <summary>
        /// Constructor of the attribute.
        /// </summary>
        /// <param name="iconGuid">The Icon Guid for the module group.</param>
        public ActionModuleGroupAttribute(string iconGuid)
        {
            this.IconGuid = iconGuid;
        }
    }

    /// <summary>
    /// The base class for Character Item Action Module Groups. Used to group modules together.
    /// </summary>
    [Serializable]
    public abstract class ActionModuleGroupBase
    {
        [Tooltip("The ID used is to identify the group easily.")]
        [System.NonSerialized] protected int m_ID = -1;
        
        protected CharacterItemAction m_CharacterItemAction;

        public int ID { get => m_ID; set => m_ID = value; }

        public abstract int ModuleCount { get; }

        public abstract IReadOnlyList<ActionModule> BaseModules { get; }
        public abstract IReadOnlyList<ActionModule> EnabledBaseModules { get; }
        public abstract IReadOnlyList<ActionModule> DisabledBaseModules { get; }
        public abstract ActionModule FirstEnabledBaseModule { get; }
        public abstract ActionModule FirstDisabledBaseModule { get; }

        /// <summary>
        /// Initialize the module group.
        /// </summary>
        /// <param name="characterItemAction">The character item action of this module group.</param>
        public abstract void Initialize(CharacterItemAction characterItemAction);

        /// <summary>
        /// Get the module at the index provided.
        /// </summary>
        /// <param name="index">The index of the module.</param>
        /// <returns>Returns the module.</returns>
        public abstract ActionModule GetBaseModuleAt(int index);
        
        /// <summary>
        /// Get the module with the matching ID.
        /// </summary>
        /// <param name="moduleID">The id of the module to get.</param>
        /// <returns>Returns the module with the ID provided (Can be null).</returns>
        public abstract ActionModule GetBaseModuleByID(int moduleID);

        /// <summary>
        /// Get the index of the module within the list.
        /// </summary>
        /// <param name="module">The module to get the index of.</param>
        /// <returns>Returns the module index (Can be -1).</returns>
        public abstract int IndexOfModule(ActionModule module);
        
        /// <summary>
        /// Get the module with the matching ID.
        /// </summary>
        /// <param name="moduleID">The id of the module to get.</param>
        /// <returns>Returns the module index of the module with the ID provided (Can be -1).</returns>
        public abstract int IndexOfModule(int moduleID);

        /// <summary>
        /// Remove the module at the index.
        /// </summary>
        /// <param name="index">The index of the module to remove.</param>
        /// <param name="gameObject">An optional parameter specifying the GameObject that the model has been attached to.</param>
        public void RemoveModuleAt(int index, GameObject gameObject = null)
        {
            RemoveBaseModule(GetBaseModuleAt(index), gameObject);
        }

        /// <summary>
        /// Remove the module specified.
        /// </summary>
        /// <param name="moduleToRemove">The module to remove.</param>
        /// <param name="gameObject">An optional parameter specifying the GameObject that the model has been attached to.</param>
        public abstract void RemoveBaseModule(ActionModule moduleToRemove, GameObject gameObject = null);

        /// <summary>
        /// Set the list of modules in the group.
        /// </summary>
        /// <param name="modules">The modules list.</param>
        public abstract void SetModulesAsBase(IReadOnlyList<ActionModule> modules);

        /// <summary>
        /// Get the module type.
        /// </summary>
        /// <returns>The module Type.</returns>
        public abstract Type GetModuleType();

        /// <summary>
        /// Get the first enabled module with the type provided.
        /// </summary>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>The first enabled module.</returns>
        public abstract TModule GetFirstEnabledModuleWithType<TModule>();

        /// <summary>
        /// Get the first disabled module with the type provided.
        /// </summary>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>The first disabled module.</returns>
        public abstract TModule GetFirstDisabledModuleWithType<TModule>();

        /// <summary>
        /// Get the enabled modules with the specified type.
        /// </summary>
        /// <param name="moduleList">The module list.</param>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>A list slice of the modules with the type provided.</returns>
        public abstract ListSlice<TModule> GetEnabledModulesWithType<TModule>(List<TModule> moduleList);

        /// <summary>
        /// Get the disabled modules with the specified type.
        /// </summary>
        /// <param name="moduleList">The module list.</param>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>A list slice of the modules with the type provided.</returns>
        public abstract ListSlice<TModule> GetDisabledModulesWithType<TModule>(List<TModule> moduleList);

        /// <summary>
        /// Get the modules with the specified type.
        /// </summary>
        /// <param name="moduleList">The module list.</param>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>A list slice of the modules with the type provided.</returns>
        public abstract ListSlice<TModule> GetModulesWithType<TModule>(List<TModule> moduleList);

        /// <summary>
        /// Set the module at the provided index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="actionModule">The module to set in the index.</param>
        public abstract void SetModuleAsBase(int index, ActionModule actionModule);

        /// <summary>
        /// Add a module to the group.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <param name="gameObject">An optional parameter specifying the GameObject that the model has been attached to.</param>
        public abstract void AddModule(ActionModule module, GameObject gameObject = null);
        
        /// <summary>
        /// Notify that a module was added.
        /// </summary>
        /// <param name="module">The module that was added.</param>
        /// <param name="gameObject">An optional module specifying the GameObject that the module was attached to.</param>
        public abstract void OnModuleAdded(ActionModule module, GameObject gameObject = null);
        
        /// <summary>
        /// Remove all the modules in the group.
        /// </summary>
        public void RemoveAllModules()
        {
            for (int i = ModuleCount - 1; i >= 0; i--) {
                RemoveModuleAt(i);
            }
        }

        /// <summary>
        /// This function will find the first enabled module and disable it.
        /// The module in the next available index will be enabled
        /// </summary>
        /// <param name="loop">If the index of the current module is the last index should the module at index 0 be activated?</param>
        public void SwitchToNextModule(bool loop = true)
        {
            ActionModule moduleToEnable = null;
            var currentModule = FirstEnabledBaseModule;
            if (currentModule == null) {
                moduleToEnable = FirstDisabledBaseModule;
            } else {
                var index = IndexOfModule(currentModule);
                var modules = BaseModules;
                if (modules.Count > index + 1) {
                    moduleToEnable = modules[index + 1];
                } else if(loop && modules.Count > 0){
                    moduleToEnable = modules[0];
                }

                currentModule.Enabled = false;
            }

            if (moduleToEnable != null) {
                moduleToEnable.Enabled = true;
            }
        }
        
        /// <summary>
        /// This function will find the first enabled module and disable it.
        /// The module in the previous available index will be enabled
        /// </summary>
        /// <param name="loop">If the index of the current module is 0 should the last module be activated?</param>
        public void SwitchToPreviousModule(bool loop = true)
        {
            ActionModule moduleToEnable = null;
            var currentModule = FirstEnabledBaseModule;
            if (currentModule == null) {
                moduleToEnable = FirstDisabledBaseModule;
            } else {
                var index = IndexOfModule(currentModule);
                var modules = BaseModules;
                if (0 <= index - 1) {
                    moduleToEnable = modules[index - 1];
                } else if(loop && modules.Count > 0){
                    moduleToEnable = modules[modules.Count-1];
                }

                currentModule.Enabled = false;
            }

            if (moduleToEnable != null) {
                moduleToEnable.Enabled = true;
            }
        }

        /// <summary>
        /// Clean up on destroy.
        /// </summary>
        public abstract void OnDestroy();
    }

    /// <summary>
    /// The base class for Character Item Action Module Groups. Used to group modules of the same type together.
    /// </summary>
    [Serializable]
    public class ActionModuleGroup<T> : ActionModuleGroupBase where T : ActionModule
    {
        public event Action<ActionModule> OnModuleAddedE;
        public event Action<ActionModule> OnModuleRemovedE;
    
        [Tooltip("The modules list.")]
        [SerializeReference] protected List<T> m_Modules;

        [System.NonSerialized] protected List<T> m_EnabledModules;
        [System.NonSerialized] protected List<T> m_DisabledModules;

        /// <summary>
        /// Get the module type.
        /// </summary>
        /// <returns>The module Type.</returns>
        public override Type GetModuleType() { return typeof(T); }

        public IReadOnlyList<T> Modules { get => m_Modules; }
        public IReadOnlyList<T> EnabledModules { get => m_EnabledModules; }
        public IReadOnlyList<T> DisabledModules { get => m_DisabledModules; }

        public override int ModuleCount => m_Modules.Count;
        public override IReadOnlyList<ActionModule> BaseModules => m_Modules;
        public override IReadOnlyList<ActionModule> EnabledBaseModules => m_EnabledModules;
        public override IReadOnlyList<ActionModule> DisabledBaseModules => m_DisabledModules;

        public T FirstEnabledModule => m_EnabledModules.Count > 0 ? m_EnabledModules[0] : null;
        public T FirstDisabledModule => m_DisabledModules.Count > 0 ? m_DisabledModules[0] : null;
        public override ActionModule FirstEnabledBaseModule => FirstEnabledModule;
        public override ActionModule FirstDisabledBaseModule => FirstDisabledModule;

        protected bool m_IsInitialized;

        /// <summary>
        /// Initialize the module group.
        /// </summary>
        /// <param name="characterItemAction">The character item action of this module group.</param>
        public override void Initialize(CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
            if (m_CharacterItemAction == null) {
                Debug.LogError("The Character Item Action is null! The modules cannot be initailized.");
                return;
            }

            m_IsInitialized = true;

            if (!m_CharacterItemAction.AllModuleGroups.Contains(this)) {
                // The ID is set to be unique.
                m_ID = m_CharacterItemAction.AllModuleGroups.Count;
                m_CharacterItemAction.AllModuleGroups.Add(this);
                if (m_CharacterItemAction.ModuleGroupsByID.ContainsKey(m_ID)) {
                    Debug.LogWarning($"Module Groups in '{characterItemAction}' must have unique IDs such that they can be organized in a dictionary.\n" +
                                     $"The ID {m_ID} '{this}' conflicts with {m_CharacterItemAction.ModuleGroupsByID[m_ID]}.", characterItemAction);
                }

                m_CharacterItemAction.ModuleGroupsByID[m_ID] = this;
            }

            if (m_EnabledModules == null) {
                m_EnabledModules = new List<T>();
            }
            if (m_DisabledModules == null) {
                m_DisabledModules = new List<T>();
            }
            if (m_Modules == null) {
                m_Modules = new List<T>();
            }

            for (int i = 0; i < m_Modules.Count; i++) {
                var module = m_Modules[i];
                if (module == null) {
                    Debug.LogWarning($"The module at index {i} is null.");
                    continue;
                }
                OnModuleAdded(module);
            }
        }

        /// <summary>
        /// Get the module at the index provided.
        /// </summary>
        /// <param name="index">The index of the module.</param>
        /// <returns>Returns the module.</returns>
        public override ActionModule GetBaseModuleAt(int index)
        {
            return GetModuleAt(index);
        }
        
        /// <summary>
        /// Get the module at the index provided.
        /// </summary>
        /// <param name="index">The index of the module.</param>
        /// <returns>Returns the module.</returns>
        public T GetModuleAt(int index)
        {
            return m_Modules[index];
        }

        /// <summary>
        /// Get the module with the matching ID.
        /// </summary>
        /// <param name="moduleID">The id of the module to get.</param>
        /// <returns>Returns the module with the ID provided (Can be null).</returns>
        public override ActionModule GetBaseModuleByID(int moduleID)
        {
            return GetModuleByID(moduleID);
        }
        
        /// <summary>
        /// Get the module with the matching ID.
        /// </summary>
        /// <param name="id">The id of the module to get.</param>
        /// <returns>Returns the module with the ID provided (Can be null).</returns>
        public T GetModuleByID(int id)
        {
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i].ID == id) {
                    return m_Modules[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Get the index of the module within the list.
        /// </summary>
        /// <param name="module">The module to get the index of.</param>
        /// <returns>Returns the module index (Can be -1).</returns>
        public override int IndexOfModule(ActionModule module)
        {
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] == module) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the module with the matching ID.
        /// </summary>
        /// <param name="moduleID">The id of the module to get.</param>
        /// <returns>Returns the module index of the module with the ID provided (Can be -1).</returns>
        public override int IndexOfModule(int moduleID)
        {
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i].ID == moduleID) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Remove the module specified.
        /// </summary>
        /// <param name="moduleToRemove">The module to remove.</param>
        public override void RemoveBaseModule(ActionModule moduleToRemove, GameObject gameObject = null)
        {
            var module = moduleToRemove as T;
            RemoveModule(module, gameObject);
        }
        
        /// <summary>
        /// Add a module to the group.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <param name="gameObject">An optional parameter specifying the GameObject that the model has been attached to.</param>
        public override void AddModule(ActionModule module, GameObject gameObject = null)
        {
            if (module is T moduleT) {
                AddModule(moduleT, gameObject);
            } else {
                Debug.LogError($"The module {module} is not of the correct type {typeof(T)}.");
            }
        }

        /// <summary>
        /// Add a module to the group.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <param name="gameObject">An optional parameter specifying the GameObject that the model has been attached to.</param>
        public void AddModule(T module, GameObject gameObject = null)
        {
            if (m_Modules == null) { m_Modules = new List<T>(); }
            m_Modules.Add(module);
            OnModuleAdded(module, gameObject);
        }

        /// <summary>
        /// Remove a module from the group.
        /// </summary>
        /// <param name="module">The module to remove from the group.</param>
        /// <param name="gameObject">An optional parameter specifying the GameObject that the model has been attached to.</param>
        public void RemoveModule(T module, GameObject gameObject = null)
        {
            m_Modules.Remove(module);
            OnModuleRemoved(module, gameObject);
        }

        /// <summary>
        /// Notify that a module was added.
        /// </summary>
        /// <param name="module">The module that was added.</param>
        /// <param name="gameObject">An optional module specifying the GameObject that the module was attached to.</param>
        public override void OnModuleAdded(ActionModule module, GameObject gameObject = null)
        {
            if (module is T moduleT && m_Modules.Contains(moduleT)) {
                OnModuleAdded(moduleT, gameObject);
            } else {
                Debug.LogError($"The module {module} was not able to be added.");
            }
        }

        /// <summary>
        /// Notify that a module was added.
        /// </summary>
        /// <param name="module">The module that was added.</param>
        /// <param name="gameObject">An optional module specifying the GameObject that the module was attached to.</param>
        public void OnModuleAdded(T module, GameObject gameObject = null)
        {
            if (module == null) {
                Debug.LogError("Null module cannot be added.");
                return;
            }
            var itemGameObject = m_CharacterItemAction != null ? m_CharacterItemAction.gameObject : gameObject;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(itemGameObject))) {
                if (module.GetType().GetMethod("OnModuleAdded")?.DeclaringType == module.GetType()) {
                    Debug.LogWarning("GameObjects cannot be added to prefabs. The module was unable to automatically add the location GameObjects. " +
                        "Ensure all of the module references to locations are valid by manually adding those GameObjects.");
                    return;
                }
            }
#endif

            // Only call this function when the object is added for the first time in the editor.
            if (Application.isPlaying == false) {
                module.OnEditorModuleAdded(itemGameObject);
            }
            
            OnModuleAddedE?.Invoke(module);

            if (!m_IsInitialized || m_CharacterItemAction == null) { return; }

            // Only initialize the module at runtime.
            if (Application.isPlaying) {
                module.Initialize(m_CharacterItemAction, this);
            }

            if (module.Enabled) { m_EnabledModules.Add(module); } else { m_DisabledModules.Add(module); }

            m_CharacterItemAction.OnModuleAdded(module);

            module.OnEnabledChange += HandleModuleEnableChange;
        }

        /// <summary>
        /// Notify that a module was removed.
        /// </summary>
        /// <param name="module">The module that was removed.</param>
        /// <param name="gameObject">An optional parameter specifying the GameObject that the model has been attached to.</param>
        private void OnModuleRemoved(T module, GameObject gameObject = null)
        {
            if (module == null) {
                return;
            }
            
            var itemGameObject = m_CharacterItemAction != null ? m_CharacterItemAction.gameObject : gameObject;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(itemGameObject))) {
                if (module.GetType()?.GetMethod("OnModuleAdded")?.DeclaringType == module.GetType()) {
                    Debug.LogWarning("GameObjects cannot be removed from prefabs. The GameObjects referenced by the module should be manually removed.");
                    return;
                }
            }
#endif

            // Only call this function when the object is removed for the first time in the editor.
            if (Application.isPlaying == false) {
                module.OnEditorModuleRemoved(itemGameObject);
            }

            OnModuleRemovedE?.Invoke(module);
            
            if(!m_IsInitialized || m_CharacterItemAction == null){ return; }

            module.OnEnabledChange -= HandleModuleEnableChange;

            if (module.Enabled) { m_EnabledModules.Remove(module); } else { m_DisabledModules.Remove(module); }

            m_CharacterItemAction.OnModuleRemoved(module);

            module.OnDestroy();
        }

        /// <summary>
        /// Handle a module that was enabled or disabled.
        /// </summary>
        /// <param name="module">The module that changed enable state.</param>
        /// <param name="enabled">Is the module now enabled or disabled?</param>
        private void HandleModuleEnableChange(ActionModule module, bool enabled)
        {
            var typedModule = module as T;
            if (typedModule == null) {
                Debug.LogWarning("This should never happen");
                return;
            }

            // Refresh the order since some newly enabled module might have priority over previous ones.
            RefreshCachedEnabledModulesList();
        }

        /// <summary>
        /// Refresh the cached list of modules.
        /// </summary>
        private void RefreshCachedEnabledModulesList()
        {
            if (m_EnabledModules == null) {
                m_EnabledModules = new List<T>();
            } else {
                m_EnabledModules.Clear();
            }
            if (m_DisabledModules == null) {
                m_DisabledModules = new List<T>();
            } else {
                m_DisabledModules.Clear();
            }

            for (int i = 0; i < m_Modules.Count; i++) {
                var module = m_Modules[i];
                if (module.Enabled) {
                    m_EnabledModules.Add(module);
                } else {
                    m_DisabledModules.Add(module);
                }
            }
        }

        /// <summary>
        /// Set the modules for this group.
        /// </summary>
        /// <param name="modules">The modules.</param>
        public void SetModules(IReadOnlyList<T> modules)
        {
            //Remove the previous modules.
            if (Application.isPlaying) {
                for (int i = m_Modules.Count - 1; i >= 0; i--) {
                    var module = m_Modules[i];
                    if (module == null) { continue; }
                    RemoveModule(module);
                }
            }

            if (m_Modules == null) {
                m_Modules = new List<T>();
            }
            m_Modules.Clear();

            // Add modules and notify in playmode.
            if (Application.isPlaying) {
                for (int i = 0; i < modules.Count; i++) {
                    AddModule(modules[i]);
                }

                RefreshCachedEnabledModulesList();
            } else {
                m_Modules.AddRange(modules);
            }
        }

        /// <summary>
        /// Set the list of modules in the group.
        /// </summary>
        /// <param name="modules">The modules list.</param>
        public override void SetModulesAsBase(IReadOnlyList<ActionModule> modules)
        {
            //Remove the previous modules.
            if (Application.isPlaying) {
                for (int i = m_Modules.Count - 1; i >= 0; i--) {
                    var module = m_Modules[i];
                    if (module == null) { continue; }
                    RemoveModule(module);
                }
            }

            if (m_Modules == null) {
                m_Modules = new List<T>();
            }
            m_Modules.Clear();

            // Add modules and notify in playmode.
            if (Application.isPlaying) {
                for (int i = 0; i < modules.Count; i++) {
                    if (modules[i] is T correctTypeModule) {
                        AddModule(correctTypeModule);
                    }
                }

                RefreshCachedEnabledModulesList();
            } else {
                //this is used in edit mode. Reordering the modules and setting enable/disable is important.
                for (int i = 0; i < modules.Count; i++) {
                    if (modules[i] is T correctTypeModule) {
                        m_Modules.Add(correctTypeModule);
                    }
                }
            }
        }

        /// <summary>
        /// Get the first enabled module with the type provided.
        /// </summary>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>The first enabled module.</returns>
        public override TModule GetFirstEnabledModuleWithType<TModule>()
        {
            for (int i = 0; i < m_EnabledModules.Count; i++) {
                if (m_EnabledModules[i] is TModule module) {
                    return module;
                }
            }

            return default;
        }

        /// <summary>
        /// Get the first disabled module with the type provided.
        /// </summary>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>The first disabled module.</returns>
        public override TModule GetFirstDisabledModuleWithType<TModule>()
        {
            for (int i = 0; i < m_DisabledModules.Count; i++) {
                if (m_DisabledModules[i] is TModule module) {
                    return module;
                }
            }

            return default;
        }

        /// <summary>
        /// Get the enabled modules with the specified type.
        /// </summary>
        /// <param name="moduleList">The module list.</param>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>A list slice of the modules with the type provided.</returns>
        public override ListSlice<TModule> GetEnabledModulesWithType<TModule>(List<TModule> moduleList)
        {
            var startIndex = moduleList.Count;
            for (int i = 0; i < m_EnabledModules.Count; i++) {
                if (m_EnabledModules[i] is TModule module) {
                    moduleList.Add(module);
                }
            }

            return new ListSlice<TModule>(moduleList, startIndex, moduleList.Count);
        }

        /// <summary>
        /// Get the disabled modules with the specified type.
        /// </summary>
        /// <param name="moduleList">The module list.</param>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>A list slice of the modules with the type provided.</returns>
        public override ListSlice<TModule> GetDisabledModulesWithType<TModule>(List<TModule> moduleList)
        {
            var startIndex = moduleList.Count;
            for (int i = 0; i < m_DisabledModules.Count; i++) {
                if (m_DisabledModules[i] is TModule module) {
                    moduleList.Add(module);
                }
            }

            return new ListSlice<TModule>(moduleList, startIndex, moduleList.Count);
        }

        /// <summary>
        /// Get the modules with the specified type.
        /// </summary>
        /// <param name="moduleList">The module list.</param>
        /// <typeparam name="TModule">The module type.</typeparam>
        /// <returns>A list slice of the modules with the type provided.</returns>
        public override ListSlice<TModule> GetModulesWithType<TModule>(List<TModule> moduleList)
        {
            var startIndex = moduleList.Count;
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is TModule module) {
                    moduleList.Add(module);
                }
            }

            return new ListSlice<TModule>(moduleList, startIndex, moduleList.Count);
        }

        /// <summary>
        /// Set the module at the provided index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="actionModule">The module to set in the index.</param>
        public override void SetModuleAsBase(int index, ActionModule actionModule)
        {
            if(m_Modules[index] == actionModule){ return; }
            
            // Make sure to listen to the change events if this is done at runtime.
            if (Application.isPlaying) {
                OnModuleRemoved(m_Modules[index]);
                m_Modules[index] = actionModule as T;
                OnModuleAdded(m_Modules[index]);
            } else {
                m_Modules[index] = actionModule as T;
            }
        }

        /// <summary>
        /// Clean up on destroy.
        /// </summary>
        public override void OnDestroy()
        {
            if (m_CharacterItemAction != null) {
                m_CharacterItemAction.AllModuleGroups.Remove(this);
            }

            for (int i = 0; i < m_Modules.Count; i++) {
                var module = m_Modules[i];
                if (module == null) { continue; }
                module.OnEnabledChange -= HandleModuleEnableChange;
                module.OnDestroy();
            }
        }
    }
}