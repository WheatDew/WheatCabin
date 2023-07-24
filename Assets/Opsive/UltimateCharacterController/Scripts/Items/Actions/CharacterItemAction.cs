/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Game;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Networking;
#endif
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The Character Item Action is the base class to define functionality for Character Item.
    /// They are usually interacted with by Item Abilities.
    /// </summary>
    public abstract class CharacterItemAction : StateBehavior
    {
        // Info keys are used for debugging character item actions.
        public const string InfoKey_CanEquip  = "Action/CanEquip";
        public const string InfoKey_CanActivateVisibleObject  = "Action/CanActivateVisibleObject";
        
        public event Action<ActionModule> OnModuleAddedE;
        public event Action<ActionModule> OnModuleRemovedE;
        public event Action OnDrawGizmosE;
        public event Action OnDrawGizmosSelectedE;
        public event Action<bool> OnDrawGizmosHybridE;
        public event Action OnFixedUpdateE;
        public event Action OnLateUpdateE;

        [Tooltip("The ID of the action. Used with the item abilities to allow multiple actions to exist on the same item.")]
        [SerializeField] protected int m_ID;
        [Tooltip("The name of the action.")]
        [SerializeField] protected string m_ActionName;
        [MultilineAttribute]
        [Tooltip("A description of the action.")]
        [SerializeField] protected string m_ActionDescription;
        [Tooltip("A component used to manage what information gets logged.")]
        [SerializeField] protected CharacterItemActionLogger m_DebugLogger;
        
        public bool IsDebugging => m_DebugLogger.IsDebugging;
        
        [Shared.Utility.NonSerialized] public int ID { get => m_ID; set => m_ID = value; }
        [Shared.Utility.NonSerialized] public string ActionName { get => m_ActionName; set => m_ActionName = value; }
        [Shared.Utility.NonSerialized] public string ActionDescription { get => m_ActionDescription; set => m_ActionDescription = value; }

        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected CharacterItem m_CharacterItem;
        protected InventoryBase m_Inventory;
        protected GameObject m_Character;
        protected Transform m_CharacterTransform;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        protected INetworkInfo m_NetworkInfo;
        protected INetworkCharacter m_NetworkCharacter;
#endif
        protected bool m_UsingFirstPerspective;
        protected bool m_IsEquipped;

        public bool IsEquipped => m_IsEquipped;
        protected bool m_IsInitialized = false;
        protected List<ItemIdentifierAmount> m_CachedItemIdentifierAmounts;

        protected List<ActionModuleGroupBase> m_AllModuleGroups;
        protected Dictionary<int, ActionModuleGroupBase> m_ModuleGroupsByID;
        public List<ActionModuleGroupBase> AllModuleGroups { get => m_AllModuleGroups; }
        public Dictionary<int, ActionModuleGroupBase> ModuleGroupsByID { get => m_ModuleGroupsByID; }

        public GameObject GameObject => m_GameObject;
        public Transform Transform => m_Transform;
        public CharacterItem CharacterItem => m_CharacterItem;
        public InventoryBase Inventory => m_Inventory;
        public GameObject Character => m_Character;
        public Transform CharacterTransform => m_CharacterTransform;
        public UltimateCharacterLocomotion CharacterLocomotion => m_CharacterLocomotion;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        public INetworkInfo NetworkInfo => m_NetworkInfo;
        public INetworkCharacter NetworkCharacter => m_NetworkCharacter;
#endif
        public bool UsingFirstPerspective => m_UsingFirstPerspective;

        public CharacterItemActionLogger DebugLogger => m_DebugLogger;

        public bool IsInitialized => m_IsInitialized;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            InitializeAction(false);
        }

        /// <summary>
        /// Initializes any values that require on other components to first initialize.
        /// </summary>
        protected virtual void Start()
        {
            // Some modules are independent so only check if they are valid after they have started.
            CheckIfValid(true);
        }

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        public virtual void InitializeAction(bool force)
        {
            if(m_IsInitialized && !force){ return; }

            if (m_IsInitialized == false) {
                base.Awake();
            }

            m_DebugLogger.Initialize(this);

            m_GameObject = gameObject;
            m_Transform = transform;
            m_CharacterItem = m_GameObject.GetCachedComponent<CharacterItem>();
            m_CharacterLocomotion = m_GameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_Character = m_CharacterLocomotion.gameObject;
            m_CharacterTransform = m_Character.transform;
            m_Inventory = m_Character.GetCachedComponent<InventoryBase>();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_Character.GetCachedComponent<INetworkInfo>();
            m_NetworkCharacter = m_Character.GetCachedComponent<INetworkCharacter>();
#endif

            m_UsingFirstPerspective = m_CharacterLocomotion.FirstPersonPerspective;
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            
            InitializeActionInternal(force);
            InitializeModuleGroups(force);

            m_IsInitialized = true;
        }

        /// <summary>
        /// Check if the item action is valid.
        /// </summary>
        /// <param name="log">Should the invalid message be logged in the console?</param>
        /// <returns>Returns a tuple containing if the action is valid and a string warning message.</returns>
        public (bool isValid, string message) CheckIfValid(bool log = true)
        {
            var (isValid, message) = CheckIfValidInternal();

            if (log && isValid == false) {
                Debug.LogWarning($"The Item Action '{this}' is not valid:\n" + message, gameObject);
            }
            
            return (isValid, message);
        }

        /// <summary>
        /// Check if the item action is valid.
        /// </summary>
        /// <returns>Returns a tuple containing if the action is valid and a string warning message.</returns>
        public virtual (bool isValid, string message) CheckIfValidInternal()
        {
            var isValid = true;
            var message = "";

            if (m_CharacterItem == null) {
                isValid = false;
                message += "The Character Item cannot be null.\n";
            }
            
            if (m_CharacterLocomotion == null) {
                isValid = false;
                message += "The Character Locomotion cannot be null.\n";
            }
            
            if (m_Inventory == null) {
                isValid = false;
                message += "The Inventory cannot be null.\n";
            }

            return (isValid, message);
        }

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        protected virtual void InitializeActionInternal(bool force)
        {
            // Do nothing.
        }

        /// <summary>
        /// Initialize the module groups.
        /// </summary>
        /// <param name="force">Force Initialize the module groups?</param>
        public virtual void InitializeModuleGroups(bool force)
        {
            if (m_IsInitialized && !force) {
                return;
            }
            m_AllModuleGroups = new List<ActionModuleGroupBase>();
            m_ModuleGroupsByID = new Dictionary<int, ActionModuleGroupBase>();

            // A temporary list is used when initializing.
            var tempModuleGroupList = new List<ActionModuleGroupBase>();
            GetAllModuleGroups(tempModuleGroupList);
            for (int i = 0; i < tempModuleGroupList.Count; i++) {
                tempModuleGroupList[i].Initialize(this);
            }
            
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                // Initialize all modules, not just the active/enabled ones
                var modules = m_AllModuleGroups[i].BaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    modules[j].OnAllModulesPreInitialized();
                }
            }
        }

        /// <summary>
        /// Get all the module groups and add them to the list.
        /// </summary>
        /// <param name="groupsResult">The module group list where the groups will be added.</param>
        public virtual void GetAllModuleGroups(List<ActionModuleGroupBase> groupsResult)
        {
            // To be overriden.   
        }

        /// <summary>
        /// A module was added inside one of the groups of the Character Item Action.
        /// </summary>
        /// <param name="module">The module that was added.</param>
        public virtual void OnModuleAdded(ActionModule module)
        {
            OnModuleAddedE?.Invoke(module);
        }
        
        /// <summary>
        /// A module was removed inside one of the groups of the Character Item Action.
        /// </summary>
        /// <param name="module">The module that was removed.</param>
        public virtual void OnModuleRemoved(ActionModule module)
        {
            OnModuleRemovedE?.Invoke(module);
        }

        /// <summary>
        /// Remove all the modules in the item action.
        /// </summary>
        public void RemoveAllModules()
        {
            if (m_AllModuleGroups == null) {
                InitializeModuleGroups(false);
            }

            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                m_AllModuleGroups[i].RemoveAllModules();
            }
        }
        
        /// <summary>
        /// Get the first active module with the type.
        /// </summary>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <returns>The first active module with that type found.</returns>
        public Tm GetFirstActiveModule<Tm>() where Tm : IActionModule
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    return moduleT;
                } 
            }

            return default;
        }
        
        /// <summary>
        /// Get all modules active modules of a certain type.
        /// </summary>
        /// <param name="result">The list where the resulting modules will be stored.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <returns>A list slice of the active modules with that type found.</returns>
        public ListSlice<Tm> GetActiveModules<Tm>(List<Tm> result) where Tm : IActionModule
        {
            if (result == null) {
                result = new List<Tm>();
            }
            var startCount = result.Count;
            
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    result.Add(moduleT);
                } 
            }

            return new ListSlice<Tm>(result, startCount, result.Count);
        }

        /// <summary>
        /// Invoke a function on the modules of a specific type.
        /// </summary>
        /// <param name="action">The action the invoke on all active modules with the type specified.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        public void InvokeOnModulesWithType<Tm>(Action<Tm> action)
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    action(moduleT);
                } 
            }
        }

        /// <summary>
        /// Invoke a function on the modules of a specific type.
        /// </summary>
        /// <param name="i1">The first parameter to pass.</param>
        /// <param name="action">The action the invoke on all active modules with the type specified.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        public void InvokeOnModulesWithType<Tm,T1>(T1 i1, Action<Tm,T1> action)
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    action(moduleT, i1);
                } 
            }
        }
        
        /// <summary>
        /// Invoke a function on the modules of a specific type.
        /// </summary>
        /// <param name="i1">The first parameter to pass.</param>
        /// <param name="i2">The second parameter to pass.</param>
        /// <param name="action">The action the invoke on all active modules with the type specified.</param>
        /// <param name="includeInactive">Should the method also be invoked on inactive modules?</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        /// <typeparam name="T2">The second parameter type.</typeparam>
        public void InvokeOnModulesWithType<Tm,T1,T2>(T1 i1, T2 i2, Action<Tm,T1,T2> action, bool includeInactive = false)
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                var modules = includeInactive ? moduleGroup.BaseModules : moduleGroup.EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    var module = modules[j];
                    if ((!includeInactive && !module.IsActive) || !(module is Tm moduleT)) { continue; }

                    action(moduleT, i1, i2);
                } 
            }
        }
        
        /// <summary>
        /// Invoke a function that returns a value of the same type as the parameter to feedback the result on the modules of a specific type.
        /// </summary>
        /// <param name="i1">The first starting parameter.</param>
        /// <param name="action">The feedback function called on all the modules.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        /// <returns></returns>
        public T1 InvokeOnModulesWithTypeFeedback<Tm,T1>(T1 i1, Func<Tm,T1, T1> action)
        {
            var i1Feedback = i1;
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    i1Feedback = action(moduleT, i1Feedback);
                } 
            }

            return i1Feedback;
        }
        
        /// <summary>
        /// Invoke a function that returns a value of the same type as the parameter to feedback the result on the modules of a specific type.
        /// </summary>
        /// <param name="i1">The first starting parameter.</param>
        /// <param name="i2">The second starting parameter.</param>
        /// <param name="action">The feedback function called on all the modules.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        /// <typeparam name="T2">The second parameter type.</typeparam>
        /// <returns></returns>
        public T2 InvokeOnModulesWithTypeFeedback<Tm,T1,T2>(T1 i1, T2 i2, Func<Tm,T1,T2, T2> action)
        {
            var i2Feedback = i2;
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    i2Feedback = action(moduleT, i1, i2Feedback);
                } 
            }

            return i2Feedback;
        }

        /// <summary>
        /// Invoke a function on the modules of a specific type only if they pass the condition function.
        /// </summary>
        /// <param name="condition">The condition function.</param>
        /// <param name="action">The action to invoked if the condition is passed.</param>
        /// <param name="skip">Choose to either skip or break if the condition is false.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        public void InvokeOnModulesWithType<Tm>(Func<Tm,bool> condition,  Action<Tm> action, bool skip)
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    if (!condition(moduleT)) {
                        if (skip) {
                            continue;
                        } else {
                            break;
                        }
                    }

                    action(moduleT);
                } 
            }
        }

        /// <summary>
        /// Invoke a function on the modules which returns if the module passed or failed.
        /// </summary>
        /// <param name="action">The action that returns if the module passed or failed.</param>
        /// <param name="returnOnTrue">Break and return if the module passed or failed the condition.</param>
        /// <param name="includeInactive">Should the method also be invoked on inactive modules?</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <returns>(The action returned True/False, module that returned early if there is one).</returns>
        public (bool, Tm) InvokeOnModulesWithTypeConditional<Tm>(Func<Tm, bool> action, bool returnOnTrue, bool includeInactive = false)
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                var modules = includeInactive ? moduleGroup.BaseModules : moduleGroup.EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    var module = modules[j];
                    if ((!includeInactive && !module.IsActive) || !(module is Tm moduleT)) { continue; }

                    var result = action(moduleT);

                    if (!returnOnTrue && !result) {
                        return (false, moduleT);
                    }

                    if (returnOnTrue && result) {
                        return (true, moduleT);
                    }
                }
            }

            return (!returnOnTrue, default);
        }
        
        /// <summary>
        /// Invoke a function on the modules which returns if the module passed or failed.
        /// </summary>
        /// <param name="i1">The first parameter.</param>
        /// <param name="action">The action that returns if the module passed or failed.</param>
        /// <param name="returnOnTrue">Break and return if the module passed or failed the condition.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        /// <returns>(The action returned True/False, module that returned early if there is one).</returns>
        public (bool, Tm) InvokeOnModulesWithTypeConditional<Tm,T1>(T1 i1, Func<Tm,T1, bool> action, bool returnOnTrue)
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    var result = action(moduleT, i1);

                    if (!returnOnTrue && !result) {
                        return (false,moduleT);
                    }
                    
                    if (returnOnTrue &&result) {
                        return (true,moduleT);
                    }
                } 
            }

            return (!returnOnTrue, default);
        }
        
        /// <summary>
        /// Invoke a function on the modules which returns if the module passed or failed.
        /// </summary>
        /// <param name="i1">The first parameter.</param>
        /// <param name="i2">The second parameter.</param>
        /// <param name="action">The action that returns if the module passed or failed.</param>
        /// <param name="returnOnTrue">Break and return if the module passed or failed the condition.</param>
        /// <typeparam name="Tm">The module type.</typeparam>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        /// <typeparam name="T2">The second parameter type.</typeparam>
        /// <returns>(The action returned True/False, module that returned early if there is one).</returns>
        public (bool, Tm) InvokeOnModulesWithTypeConditional<Tm,T1,T2>(T1 i1, T2 i2, Func<Tm,T1,T2, bool> action, bool returnOnTrue)
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var moduleGroup = m_AllModuleGroups[i];
                for (int j = 0; j < moduleGroup.EnabledBaseModules.Count; j++) {
                    var module = moduleGroup.EnabledBaseModules[j];
                    if (!module.IsActive || !(module is Tm moduleT)) { continue; }

                    var result = action(moduleT, i1, i2);
                    
                    if (!returnOnTrue && !result) {
                        return (false,moduleT);
                    }
                    
                    if (returnOnTrue && result) {
                        return (true,moduleT);
                    }
                } 
            }

            return (!returnOnTrue, default);
        }

        /// <summary>
        /// The item has been picked up by the character.
        /// </summary>
        public virtual void Pickup()
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var modules = m_AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    modules[j].Pickup();
                }
            }
        }

        /// <summary>
        /// Can the visible object be activated? An example of when it shouldn't be activated is when a grenade can be thrown but it is not the primary item
        /// so it shouldn't be thrown until after the throw action has started.
        /// </summary>
        /// <returns>True if the visible object can be activated.</returns>
        public virtual bool CanActivateVisibleObject()
        {
            var (modulesCanUse, moduleThatStopped) = InvokeOnModulesWithTypeConditional(
                (IModuleCanActivateVisibleObject module) => module.CanActivateVisibleObject(), false);

            if (!modulesCanUse) {
                DebugLogger.SetInfo(InfoKey_CanActivateVisibleObject, "(No) because of module "+moduleThatStopped);
                DebugLogger.Log("Cannot Activate Visible Object because of module "+moduleThatStopped);
                return false;
            }
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_CanActivateVisibleObject, "(Yes)");
            }
            return true;
        }
        
        /// <summary>
        /// Can the item be equipped.
        /// </summary>
        /// <returns>True if the item can be equipped.</returns>
        public virtual bool CanEquip()
        {
            var (modulesCanUse, moduleThatStopped) = InvokeOnModulesWithTypeConditional(
                (IModuleCanEquip module) => module.CanEquip(), false);

            if (!modulesCanUse) {
                DebugLogger.SetInfo(InfoKey_CanEquip, "(No) because of module "+moduleThatStopped);
                DebugLogger.Log("Cannot Equip because of module "+moduleThatStopped);
                return false;
            }
            
            DebugLogger.SetInfo(InfoKey_CanEquip, "(Yes)");
            return true;
        }

        /// <summary>
        /// The item will be equipped.
        /// </summary>
        public virtual void WillEquip()
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var modules = m_AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    modules[j].WillEquip();
                }
            }
        }

        /// <summary>
        /// The item has been equipped by the character.
        /// </summary>
        public virtual void Equip()
        {
            m_IsEquipped = true;
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var modules = m_AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    modules[j].Equip();
                }
            }
        }

        /// <summary>
        /// Helper method for calling the required event for updating the item ability parameters.
        /// </summary>
        /// <param name="forceChange">Force the trigger to be changed?</param>
        public virtual void UpdateItemAbilityAnimatorParameters(bool forceChange = false)
        {
            EventHandler.ExecuteEvent(m_CharacterLocomotion.gameObject, "OnCharacterUpdateItemAbilityParameters", forceChange);
        }

        /// <summary>
        /// Invoked fixed update. This event can be listened to by any module.
        /// </summary>
        public void FixedUpdate()
        {
            OnFixedUpdateE?.Invoke();
        }

        /// <summary>
        /// Invoked late update. This event can be listened to by any module.
        /// </summary>
        public void LateUpdate()
        {
            OnLateUpdateE?.Invoke();
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person view?</param>
        protected virtual void OnChangePerspectives(bool firstPersonPerspective)
        {
            m_UsingFirstPerspective = firstPersonPerspective;
            InvokeOnModulesWithType(firstPersonPerspective,
                (IModuleOnChangePerspectives module, bool i1) => module.OnChangePerspectives(i1));
        }

        /// <summary>
        /// The item has started to be unequipped by the character.
        /// </summary>
        public virtual void StartUnequip()
        {
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var modules = m_AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    modules[j].StartUnequip();
                }
            }
        }

        /// <summary>
        /// The item has been unequipped by the character.
        /// </summary>
        public virtual void Unequip()
        {
            m_IsEquipped = false;
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var modules = m_AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    modules[j].Unequip();
                }
            }
        }

        /// <summary>
        /// Get the other items to drop, when the character item drops.
        /// </summary>
        public virtual ListSlice<ItemIdentifierAmount> GetItemsToDrop()
        {
            if (m_CachedItemIdentifierAmounts == null) {
                m_CachedItemIdentifierAmounts = new List<ItemIdentifierAmount>();
            } else {
                m_CachedItemIdentifierAmounts.Clear();
            }
            InvokeOnModulesWithType(m_CachedItemIdentifierAmounts,
                (IModuleGetItemsToDrop module, List<ItemIdentifierAmount> i1) => module.GetItemsToDrop(i1));

            return m_CachedItemIdentifierAmounts;
        }

        /// <summary>
        /// The item has been removed by the character.
        /// </summary>
        public virtual void Remove()
        {
            if(m_IsEquipped) {
                Unequip();
            }
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                var modules = m_AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < modules.Count; j++) {
                    modules[j].RemoveItem();
                }
            }
        }

        /// <summary>
        /// A name containing the action name and the id of the action making it easily recognizable.
        /// </summary>
        /// <returns>The string format for the action.</returns>
        public override string ToString()
        {
            return base.ToString()+$" [{m_ID}]'{m_ActionName}'";
        }

        /// <summary>
        /// Unity callback to draw the editor gizmos.
        /// </summary>
        public virtual void OnDrawGizmos()
        {
            OnDrawGizmosE?.Invoke();
            OnDrawGizmosHybridE?.Invoke(false);
        }

        /// <summary>
        /// Unity callback to draw the editor gizmos when selected.
        /// </summary>
        public virtual void OnDrawGizmosSelected()
        {
            OnDrawGizmosSelectedE?.Invoke();
            OnDrawGizmosHybridE?.Invoke(true);
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            for (int i = 0; i < m_AllModuleGroups.Count; i++) {
                m_AllModuleGroups[i].OnDestroy();
            }
        }
    }
}