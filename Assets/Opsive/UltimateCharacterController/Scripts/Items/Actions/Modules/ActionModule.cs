/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules
{
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Networking;
#endif
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items.Actions.Bindings;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for Character Item Action Modules.
    /// </summary>
    [Serializable]
    public abstract class ActionModule : BoundStateObject, IActionModule
    {
        protected override GameObject BoundGameObject => m_CharacterItemAction?.gameObject ?? m_StateBoundGameObject;
        
        public event Action<ActionModule, bool> OnEnabledChange;

        [Tooltip("Is the module enabled?")]
        [HideInInspector] [SerializeField] protected bool m_Enabled = true;
        [Tooltip("The action module ID is used find a module easily within a module Group.")]
        [SerializeField] protected int m_ID = -1;
        [Tooltip("The action module name is used find a module easily within a module Group.")]
        [SerializeField] protected string m_Name = "";

        public int ID { get => m_ID; set => m_ID = value; }
        public string Name => m_Name;

        protected ActionModuleGroupBase m_ModuleGroup;
        protected CharacterItemAction m_CharacterItemAction;
        protected bool m_AllModulesPreInitialized;
        protected bool m_Initialized;
        protected bool m_ListeningToEvents;

        public ActionModuleGroupBase ModuleGroup => m_ModuleGroup;
        public CharacterItemAction CharacterItemAction => m_CharacterItemAction;
        public bool AllModulesPreInitialized => m_AllModulesPreInitialized;
        public bool Initialized => m_Initialized;

        public bool IsEquipped => CharacterItemAction.IsEquipped;
        public UltimateCharacterLocomotion CharacterLocomotion => m_CharacterItemAction.CharacterLocomotion;
        public GameObject GameObject => m_CharacterItemAction.GameObject;
        public Transform Transform => m_CharacterItemAction.transform;
        public CharacterItem CharacterItem => m_CharacterItemAction.CharacterItem;
        public int SlotID => CharacterItem.SlotID;
        public GameObject Character => m_CharacterItemAction.Character;
        public Transform CharacterTransform => m_CharacterItemAction.Character.transform;
        public InventoryBase Inventory => m_CharacterItemAction.Inventory;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        public INetworkInfo NetworkInfo => m_CharacterItemAction.NetworkInfo;
        public INetworkCharacter NetworkCharacter => m_CharacterItemAction.NetworkCharacter;
#endif

        public bool Enabled
        {
            get => m_Enabled;
            set {
                if (m_Enabled == value) { return; }

                if (Application.isPlaying) {
                    if (value) {
                        OnEnable();
                    } else {
                        OnDisable();
                    }
                }

                m_Enabled = value;

            }
        }
        public virtual bool IsActiveOnlyIfFirstEnabled => false;
        public virtual bool IsActive
        {
            get {
                if (!Enabled) {
                    return false;
                }

                if (IsActiveOnlyIfFirstEnabled && m_ModuleGroup != null) {
                    return m_ModuleGroup.FirstEnabledBaseModule == this;
                }

                return true;
            }
        }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        public virtual bool NetworkSync { get => false; }
#endif

        /// <summary>
        /// Notify that the module is now enabled.
        /// </summary>
        public virtual void OnEnable()
        {
            m_Enabled = true;
            OnEnabledChange?.Invoke(this, m_Enabled);
            OnEnableInternal();
        }

        /// <summary>
        /// Notify that the module is now enabled.
        /// </summary>
        protected virtual void OnEnableInternal()
        {
            UpdateRegisteredEvents();
        }

        /// <summary>
        /// Notify that the module is now disabled.
        /// </summary>
        public virtual void OnDisable()
        {
            m_Enabled = false;
            OnEnabledChange?.Invoke(this, m_Enabled);
            OnDisableInternal();
        }

        /// <summary>
        /// Notify that the module is now disabled.
        /// </summary>
        protected virtual void OnDisableInternal()
        {
            UpdateRegisteredEvents();
        }

        /// <summary>
        /// Set that the module has been enabled/disabled without sending an event.
        /// </summary>
        /// <param name="enable">Enable or Disable the module?</param>
        public void SetEnabledWithoutNotify(bool enable)
        {
            m_Enabled = enable;
        }

        /// <summary>
        /// Initialize the character item action module use the item action and the module group. 
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        /// <param name="moduleGroup">The parent module group.</param>
        public void Initialize(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup)
        {
            if (m_Initialized) {
                return;
            }

            m_ModuleGroup = moduleGroup;
            Initialize(itemAction);
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        protected virtual void Initialize(CharacterItemAction itemAction)
        {
            if (m_Initialized) {
                return;
            }

            m_CharacterItemAction = itemAction;

            // Initialize the states.
            Initialize(GameObject);
            InitializeInternal();

            m_Initialized = true;
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected virtual void InitializeInternal()
        {
            // To be overriden.
        }

        /// <summary>
        /// Awake is called after all of the actions have been initialized.
        /// </summary>
        public virtual void OnAllModulesPreInitialized()
        {
            m_AllModulesPreInitialized = true;
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected void UpdateRegisteredEvents()
        {
            var register = m_Enabled && IsEquipped;
            if (m_ListeningToEvents == register) {
                return;
            }

            m_ListeningToEvents = register;
            UpdateRegisteredEventsInternal(register);
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected virtual void UpdateRegisteredEventsInternal(bool register) { }

        /// <summary>
        /// Update item ability animator parameters.
        /// </summary>
        /// <param name="forceChange">Force the trigger to be changed?</param>
        protected virtual void UpdateItemAbilityAnimatorParameters(bool forceChange = false)
        {
            m_CharacterItemAction.UpdateItemAbilityAnimatorParameters(forceChange);
        }

        /// <summary>
        /// Write the module name in an easy to read format for debugging.
        /// </summary>
        /// <returns>The string representation of the module.</returns>
        public override string ToString()
        {
            return GetToStringPrefix() + GetType().Name.Replace("CharacterItemActionModule", "");
        }

        /// <summary>
        /// Return the to string prefix with the id and name.
        /// </summary>
        /// <returns>The string prefix.</returns>
        protected virtual string GetToStringPrefix()
        {
            var prefix = "";
            var ignoreID = m_ID == -1;
            var ignoreName = string.IsNullOrWhiteSpace(m_Name);
            if (!ignoreID || !ignoreName) {
                prefix = $"[{(ignoreID ? "" : m_ID.ToString())}{(!ignoreID && !ignoreName ? " " : "")}{(ignoreName ? "" : m_Name)}] ";
            }

            return prefix;
        }

        /// <summary>
        /// The item was picked up.
        /// </summary>
        public virtual void Pickup() { }

        /// <summary>
        /// The item will be equipped.
        /// </summary>
        public virtual void WillEquip() { }

        /// <summary>
        /// The item was equipped.
        /// </summary>
        public virtual void Equip()
        {
            UpdateRegisteredEvents();
        }

        /// <summary>
        /// The item will start unequipping.
        /// </summary>
        public virtual void StartUnequip() { }

        /// <summary>
        /// The Item was unequipped.
        /// </summary>
        public virtual void Unequip()
        {
            UpdateRegisteredEvents();
            ResetModule(false);
        }

        /// <summary>
        /// The item was removed from the character.
        /// </summary>
        public virtual void RemoveItem()
        {
            ResetModule(true);
        }
        
        /// <summary>
        /// Reset the module after the item has been unequipped or removed.
        /// </summary>
        /// <param name="force">Force the reset.</param>
        public virtual void ResetModule(bool force) { }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            m_Initialized = false;
        }

        /// <summary>
        /// The module has been added to the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was added to.</param>
        public virtual void OnEditorModuleAdded(GameObject gameObject) { }

        /// <summary>
        /// The moduel has been removed from the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was removed from.</param>
        public virtual void OnEditorModuleRemoved(GameObject gameObject){ }
    }
}