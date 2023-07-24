/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules
{
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.StartStopActions;
    using Opsive.UltimateCharacterController.Traits;
    using System;
    using UnityEngine;
    using UnityEngine.Events;
    using Attribute = Opsive.UltimateCharacterController.Traits.Attribute;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The base class for action modules used in a Usable Action.
    /// </summary>
    [Serializable]
    public abstract class UsableActionModule : ActionModule
    { }

    [Serializable]
    public abstract class BasicUsableActionModule : UsableActionModule, IModuleCanStartUseItem, IModuleUseItem
    {
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public virtual bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            return true;
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public virtual void UseItem()
        {

        }
    }

    /// <summary>
    /// Generic Item Effects when an item is used, completed or updated.
    /// </summary>
    [Serializable]
    public class GenericItemEffects : BasicUsableActionModule, IModuleStartItemUse, IModuleItemUseComplete, IModuleUseItemUpdate
    {
        [Tooltip("Invoke the item effects on start use?")]
        [SerializeField] private bool m_OnStartUse;
        [Tooltip("Invoke the item effects on use?")]
        [SerializeField] private bool m_OnUse = true;
        [Tooltip("Invoke the item effects on use update?")]
        [SerializeField] private bool m_OnUseUpdate;
        [Tooltip("Invoke the item effects on use complete?")]
        [SerializeField] private bool m_OnUseComplete;
        [Tooltip("The item effects to invoke.")]
        [SerializeField] protected ItemEffectGroup m_EffectGroup;

        public ItemEffectGroup EffectGroup => m_EffectGroup;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_EffectGroup.Initialize(this);
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public void StartItemUse(Use useAbility)
        {
            if (!m_OnStartUse) { return; }
            m_EffectGroup.InvokeEffects();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (CharacterItemAction.NetworkInfo != null && CharacterItemAction.NetworkInfo.HasAuthority()) {
                CharacterItemAction.NetworkCharacter.InvokeGenericEffectModule(this);
            }
#endif
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItem()
        {
            if (!m_OnUse) { return; }
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// Tick every use item update.
        /// </summary>
        public void UseItemUpdate()
        {
            if (!m_OnUseUpdate) { return; }
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public void ItemUseComplete()
        {
            if (!m_OnUseComplete) { return; }
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_EffectGroup.OnDestroy();
        }

        /// <summary>
        /// Write the module name in an easy to read format for debugging.
        /// </summary>
        /// <returns>The string representation of the module.</returns>
        public override string ToString()
        {
            if (m_EffectGroup == null || m_EffectGroup.Effects == null) {
                return base.ToString();
            }
            return GetToStringPrefix() + $"Generic ({m_EffectGroup.Effects.Length}): " + ListUtility.ToStringDeep(m_EffectGroup.Effects, true);
        }
    }

    /// <summary>
    /// Use an attribute on the item when the item is used.
    /// </summary>
    [Serializable]
    public class UseAttribute : BasicUsableActionModule
    {
        [Tooltip("The name of the attribute that should be adjusted when the item is used.")]
        [SerializeField] protected string m_UseAttributeName;
        [Tooltip("The amount to adjust the attribute by when the item is used.")]
        [SerializeField] protected float m_UseAttributeAmount;
        [Tooltip("Should the item be dropped when the attribute is depleted?")]
        [SerializeField] protected bool m_DropWhenUseDepleted;

        private AttributeManager m_AttributeManager;
        protected Attribute m_UseAttribute;

        public string UseAttributeName
        {
            get { return m_UseAttributeName; }
            set {
                m_UseAttributeName = value;
                if (Application.isPlaying) {
                    if (!string.IsNullOrEmpty(m_UseAttributeName) && m_AttributeManager != null) {
                        m_UseAttribute = m_AttributeManager.GetAttribute(m_UseAttributeName);
                    } else {
                        m_UseAttribute = null;
                    }
                }
            }
        }
        public float UseAttributeAmount { get { return m_UseAttributeAmount; } set { m_UseAttributeAmount = value; } }
        public bool DropWhenUseDepleted { get { return m_DropWhenUseDepleted; } set { m_DropWhenUseDepleted = value; } }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            m_AttributeManager = GameObject.GetComponent<AttributeManager>();
            if (m_AttributeManager == null) {
                Debug.LogWarning("The item does not have an AttributeManager component", CharacterItem);
                return;
            }
            m_UseAttribute = m_AttributeManager.GetAttribute(m_UseAttributeName);

            if (m_UseAttribute == null) {
                Debug.LogWarning($"The Use Attribute Module, Attribute Name '{m_UseAttributeName}' does not match any attributes on the AttributeManager.", m_AttributeManager);
                return;
            }

            EventHandler.RegisterUnregisterEvent(register, m_UseAttribute, "OnAttributeReachedDestinationValue", UseDepleted);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the ability using the item</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var baseValue = base.CanStartUseItem(useAbility, abilityState);
            if (baseValue == false) {
                return false;
            }

            return (m_UseAttribute != null && m_UseAttribute.IsValid(-m_UseAttributeAmount));
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItem()
        {
            base.UseItem();
            if (m_UseAttribute != null) {
                m_UseAttribute.Value -= m_UseAttributeAmount;
            }
        }

        /// <summary>
        /// The item has depleted its use attribute.
        /// </summary>
        private void UseDepleted()
        {
            if (!m_DropWhenUseDepleted) {
                return;
            }

            // Remove the item from the inventory before dropping it. This will ensure the dropped prefab does not contain any ItemIdentifier amount so the
            // item can't be picked up again.
            Inventory.RemoveItemIdentifier(CharacterItem.ItemIdentifier, CharacterItem.SlotID, int.MaxValue, false);
            CharacterItem.Drop(int.MaxValue, true);
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (m_UseAttribute != null) {
                Shared.Events.EventHandler.UnregisterEvent(m_UseAttribute, "OnAttributeReachedDestinationValue", UseDepleted);
            }
        }
    }

    /// <summary>
    /// Use an attribute on the character when the item is used.
    /// </summary>
    [Serializable]
    public class CharacterUseAttribute : BasicUsableActionModule
    {
        [Tooltip("The name of the character attribute that should be adjusted when the item is used.")]
        [SerializeField] protected string m_CharacterUseAttributeName;
        [Tooltip("The amount to adjust the Character Use Attribute by when the item is used.")]
        [SerializeField] protected float m_CharacterUseAttributeAmount;

        private AttributeManager m_CharacterUseAttributeManager;
        protected Attribute m_CharacterUseAttribute;

        public string CharacterUseAttributeName
        {
            get { return m_CharacterUseAttributeName; }
            set {
                m_CharacterUseAttributeName = value;
                if (Application.isPlaying) {
                    if (!string.IsNullOrEmpty(m_CharacterUseAttributeName) && m_CharacterUseAttributeManager != null) {
                        m_CharacterUseAttribute = m_CharacterUseAttributeManager.GetAttribute(m_CharacterUseAttributeName);
                    } else {
                        m_CharacterUseAttribute = null;
                    }
                }
            }
        }
        public float CharacterUseAttributeAmount { get { return m_CharacterUseAttributeAmount; } set { m_CharacterUseAttributeAmount = value; } }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);

            m_CharacterUseAttributeManager = Character.GetCachedComponent<AttributeManager>();
            if (m_CharacterUseAttributeManager != null) {
                m_CharacterUseAttribute = m_CharacterUseAttributeManager.GetAttribute(m_CharacterUseAttributeName);
            }
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the ability using the item</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var baseValue = base.CanStartUseItem(useAbility, abilityState);
            if (baseValue == false) {
                return false;
            }

            return (m_CharacterUseAttribute != null && m_CharacterUseAttribute.IsValid(-m_CharacterUseAttributeAmount));
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItem()
        {
            base.UseItem();
            if (m_CharacterUseAttribute != null) {
                m_CharacterUseAttribute.Value -= m_CharacterUseAttributeAmount;
            }
        }
    }

    /// <summary>
    /// A module used to change the item substate depending on the aim state.
    /// </summary>
    [Serializable]
    public class AimSubstate : BasicUsableActionModule, IModuleGetUseItemSubstateIndex
    {
        [Tooltip("The value to add to the Item Substate Index when the character is aiming.")]
        [SerializeField] protected ItemSubstateIndexData m_SubstateIndexData = new ItemSubstateIndexData(100, 150, true);
        [Tooltip("Invoke some effects when starting to aim.")]
        [SerializeField] protected ItemEffectGroup m_OnStartAimEffectGroup;
        [Tooltip("Invoked some effects when stopping to aim.")]
        [SerializeField] protected ItemEffectGroup m_OnStopAimEffectGroup;

        public ItemSubstateIndexData SubstateIndexData { get => m_SubstateIndexData; set => m_SubstateIndexData = value; }

        public ItemEffectGroup StartAimEffectGroup
        {
            get => m_OnStartAimEffectGroup;
            set {
                m_OnStartAimEffectGroup = value;
                if (m_OnStartAimEffectGroup != null) {
                    m_OnStartAimEffectGroup.Initialize(m_CharacterItemAction);
                }
            }
        }

        public ItemEffectGroup StopAimEffectGroup
        {
            get => m_OnStopAimEffectGroup;
            set {
                m_OnStopAimEffectGroup = value;
                if (m_OnStopAimEffectGroup != null) {
                    m_OnStopAimEffectGroup.Initialize(m_CharacterItemAction);
                }
            }
        }

        protected bool m_Aiming;
        public bool Aiming => m_Aiming;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_OnStartAimEffectGroup.Initialize(itemAction);
            m_OnStopAimEffectGroup.Initialize(itemAction);
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            Shared.Events.EventHandler.RegisterUnregisterEvent<bool, bool>(register, Character, "OnAimAbilityStart", OnAim);
        }

        /// <summary>
        /// Get the item substate index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            if (!m_Aiming) {
                return;
            }

            streamData.TryAddSubstateData(this, m_SubstateIndexData);
        }

        /// <summary>
        /// The aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) {
                return;
            }

            if (m_Aiming != aim) {
                if (aim) {
                    m_OnStartAimEffectGroup.InvokeEffects();
                } else {
                    m_OnStopAimEffectGroup.InvokeEffects();
                }
            }

            m_Aiming = aim;
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_OnStartAimEffectGroup.OnDestroy();
            m_OnStopAimEffectGroup.OnDestroy();
        }
    }

    /// <summary>
    /// A module used to modify an attribute when and/or while an item is used.
    /// It can be used to prevent the item from being used if the attribute is invalid.
    /// It also enables or disables a GameObjects depending if the item is being used or not.
    /// </summary>
    [Serializable]
    public class UseAttributeModifierToggle : BasicUsableActionModule, IModuleOnChangePerspectives
    {
        [Tooltip("Prevent the item from being used if the Attribute is not valid?")]
        [SerializeField] protected bool m_PreventUseIfAttributeNotValid = true;
        [Tooltip("The attribute modifier which is active while the item is being used.")]
        [SerializeField] protected Traits.AttributeModifier m_UseModifier = new Traits.AttributeModifier("Battery", 0, Traits.Attribute.AutoUpdateValue.Decrease);
        [Tooltip("The game object to toggle on or off depending if the item is being used or not.")]
        [SerializeField] protected PerspectiveGameObjectToggle[] m_GameObjectToggle;

        public bool PreventUseIfAttributeNotValid { get => m_PreventUseIfAttributeNotValid; set => m_PreventUseIfAttributeNotValid = value; }
        public AttributeModifier UseModifier { get => m_UseModifier; set => m_UseModifier = value; }
        public PerspectiveGameObjectToggle[] GameObjectToggle { get => m_GameObjectToggle; set => m_GameObjectToggle = value; }

        protected bool m_On;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            if (m_UseModifier != null) {
                if (m_UseModifier.Initialize(GameObject)) {
                    EventHandler.RegisterEvent(m_UseModifier.Attribute, "OnAttributeReachedDestinationValue", OnAttributeEmpty);
                }
            }

            for (int i = 0; i < m_GameObjectToggle.Length; i++) {
                m_GameObjectToggle[i].Initialize(m_CharacterItemAction);
            }

            // Initialize the game object the way it should be active.
            ToggleBothPerspectiveGameObjects(false);
        }

        /// <summary>
        /// When the attribute becomes empty toggle the object off.
        /// </summary>
        protected virtual void OnAttributeEmpty()
        {
            Toggle(false);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the ability using the item.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var baseValue = base.CanStartUseItem(useAbility, abilityState);
            if (baseValue == false) {
                return false;
            }

            // The object can't be used if there is no attribute amount left.
            if (m_PreventUseIfAttributeNotValid && m_UseModifier != null && !m_UseModifier.IsValid()) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItem()
        {
            base.UseItem();

            // Only toggle if attribute is positive.
            if (m_UseModifier?.Attribute != null && m_UseModifier.Attribute.Value <= 0) {
                return;
            }

            Toggle(!m_On);
        }

        /// <summary>
        /// Toggle the object on or off.
        /// </summary>
        /// <param name="on">Should the toggle be turned on?</param>
        public virtual void Toggle(bool on)
        {
            m_On = on;

            ToggleGameObjects(m_On);

            if (m_UseModifier != null) {
                m_UseModifier.EnableModifier(on);
            }
        }

        /// <summary>
        /// The item will start unequipping.
        /// </summary>
        public override void StartUnequip()
        {
            base.StartUnequip();
            ToggleGameObjects(false);
        }

        /// <summary>
        /// Toggle the gameobjects active state.
        /// </summary>
        /// <param name="on">Should the toggle be turned on?</param>
        public virtual void ToggleGameObjects(bool on)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (CharacterItemAction.NetworkInfo != null && CharacterItemAction.NetworkInfo.HasAuthority()) {
                CharacterItemAction.NetworkCharacter.InvokeUseAttributeModifierToggleModule(this, on);
            }
#endif

            for (int i = 0; i < m_GameObjectToggle.Length; i++) {
                var gameObject = m_GameObjectToggle[i].GetValue();
                if (gameObject == null) { continue; }

                var defaultState = m_GameObjectToggle[i].On == false;
                gameObject.SetActive(on ? !defaultState : defaultState);
            }
        }

        /// <summary>
        /// Toggle the gameobjects active state.
        /// </summary>
        public virtual void ToggleBothPerspectiveGameObjects(bool toggleOn)
        {
            for (int i = 0; i < m_GameObjectToggle.Length; i++) {
                var defaultState = m_GameObjectToggle[i].On == false;

                var firstPersonGameObject = m_GameObjectToggle[i].GetValue(true);
                if (firstPersonGameObject != null) {
                    firstPersonGameObject.SetActive(toggleOn ? !defaultState : defaultState);
                }

                var thirdPersonGameObject = m_GameObjectToggle[i].GetValue(false);
                if (thirdPersonGameObject != null) {
                    thirdPersonGameObject.SetActive(toggleOn ? !defaultState : defaultState);
                }
            }
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public virtual void OnChangePerspectives(bool firstPersonPerspective)
        {
            for (int i = 0; i < m_GameObjectToggle.Length; i++) {
                var previousPerspectiveGameObject = m_GameObjectToggle[i].GetValue(!firstPersonPerspective);
                var newPerspectiveGameObject = m_GameObjectToggle[i].GetValue(firstPersonPerspective);

                var previousWasActive = m_GameObjectToggle[i].On == false;
                if (previousPerspectiveGameObject != null) {
                    previousWasActive = previousPerspectiveGameObject.activeSelf;
                    previousPerspectiveGameObject.SetActive(m_GameObjectToggle[i].On == false);
                }

                if (newPerspectiveGameObject != null) {
                    newPerspectiveGameObject.SetActive(previousWasActive);
                }
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_UseModifier != null && m_UseModifier.Attribute != null) {
                EventHandler.UnregisterEvent(m_UseModifier.Attribute, "OnAttributeReachedDestinationValue", OnAttributeEmpty);
            }
        }
    }

    /// <summary>
    /// The base class for a module switcher.
    /// Used for looping through a list of options with always one selected
    /// </summary>
    [Serializable]
    public abstract class ModuleSwitcherBase : UsableActionModule, IModuleSwitcher
    {
        public event Action OnSwitchE;

        [Tooltip("Should the switcher loop indexes when the limits are reached? 0 -> Max, Max -> 0.")]
        [SerializeField] protected bool m_Loop = true;
        [Tooltip("The index of the switcher.")]
        [SerializeField] protected int m_Index = 0;
        [Tooltip("The index of the switcher.")]
        [SerializeField] protected UnityEvent m_OnSwitch;

        public GameObject gameObject => m_CharacterItemAction.gameObject;
        public abstract int MaxIndex { get; }

        public bool Loop { get => m_Loop; set => m_Loop = value; }
        public int Index { get => m_Index; set => SwitchTo(value); }

        /// <summary>
        /// Make sure to enabled disable the states after ALL modules have been initialized.
        /// </summary>
        public override void OnAllModulesPreInitialized()
        {
            base.OnAllModulesPreInitialized();

            if (Application.isPlaying) {
                SwitchTo(m_Index);
            }
        }

        /// <summary>
        /// The name to display for the currently selected index.
        /// </summary>
        /// <returns>The name for the current index.</returns>
        public abstract string GetIndexName();

        /// <summary>
        /// The icon to display for the currently selected index.
        /// </summary>
        /// <returns>The icon for the current index.</returns>
        public abstract Sprite GetIndexIcon();

        /// <summary>
        /// Switch to the specified index.
        /// </summary>
        /// <param name="index">The index to switch to.</param>
        public void SwitchTo(int index)
        {
            if (index < 0 || index > MaxIndex) {
                return;
            }
            var previousIndex = m_Index;
            m_Index = index;
            SwitchToInternal(previousIndex, index);
            m_OnSwitch?.Invoke();
            OnSwitchE?.Invoke();
            EventHandler.ExecuteEvent<int>(gameObject, "ModuleSwitcher_OnSwitch_Index", m_Index);
        }

        /// <summary>
        /// Switch to the specified index.
        /// </summary>
        /// <param name="previousIndex">The index to switch from.</param>
        /// <param name="newIndex">The index to switch to.</param>
        public abstract void SwitchToInternal(int previousIndex, int newIndex);

        /// <summary>
        /// Switch to the previous index.
        /// </summary>
        public void SwitchToPrevious()
        {
            if (m_Index <= 0 && m_Loop) {
                SwitchTo(MaxIndex);
            } else {
                SwitchTo(m_Index - 1);
            }
        }

        /// <summary>
        /// Switch to the next index.
        /// </summary>
        public void SwitchToNext()
        {
            if (m_Index >= MaxIndex && m_Loop) {
                SwitchTo(0);
            } else {
                SwitchTo(m_Index + 1);
            }
        }
    }

    /// <summary>
    /// A switcher module used to change the active state.
    /// </summary>
    [Serializable]
    public class ModuleStateSwitcher : ModuleSwitcherBase
    {
        /// <summary>
        /// A state containing a list of module group Subsets.
        /// </summary>
        [Serializable]
        public class SwitchState
        {
            [Tooltip("The index name to display.")]
            [SerializeField] protected string m_IndexName;
            [Tooltip("The index icon to display.")]
            [SerializeField] protected Sprite m_IndexIcon;
            [Tooltip("The state name.")]
            [StateName] [SerializeField] private string m_State;

            public string IndexName => m_IndexName;
            public Sprite IndexIcon => m_IndexIcon;
            public string State => m_State;
        }

        [Tooltip("The states to switch to.")]
        [SerializeField] protected SwitchState[] m_SwitchStates;

        public override int MaxIndex => m_SwitchStates.Length - 1;

        /// <summary>
        /// The name to display for the currently selected index.
        /// </summary>
        /// <returns>The name for the current index.</returns>
        public override string GetIndexName()
        {
            if (m_Index < 0 || m_Index >= m_SwitchStates.Length) {
                return null;
            }
            return m_SwitchStates[m_Index].IndexName;
        }

        /// <summary>
        /// The icon to display for the currently selected index.
        /// </summary>
        /// <returns>The icon for the current index.</returns>
        public override Sprite GetIndexIcon()
        {
            if (m_Index < 0 || m_Index >= m_SwitchStates.Length) {
                return null;
            }
            return m_SwitchStates[m_Index].IndexIcon;
        }

        /// <summary>
        /// Switch to the specified index.
        /// </summary>
        /// <param name="previousIndex">The index to switch from.</param>
        /// <param name="newIndex">The index to switch to.</param>
        public override void SwitchToInternal(int previousIndex, int newIndex)
        {
            for (int i = 0; i < m_SwitchStates.Length; i++) {
                var state = m_SwitchStates[i];
                var active = i == newIndex;

                AssignState(state.State, active);
            }
        }

        /// <summary>
        /// Set the state.
        /// </summary>
        /// <param name="state">The state name to set.</param>
        /// <param name="active">Set active or inactive?</param>
        protected virtual void AssignState(string state, bool active)
        {
            if (string.IsNullOrWhiteSpace(state)) { return; }

            // Set the state on both the character and the character item.
            StateManager.SetState(m_CharacterItemAction.Character, state, active);
            StateManager.SetState(m_CharacterItemAction.CharacterItem.gameObject, state, active);
        }
    }

    /// <summary>
    /// Activate or deactivate states while the item is equipped or being used.
    /// </summary>
    [Serializable]
    public class ActivateStates : UsableActionModule, IModuleStartItemUse, IModuleUseItem, IModuleItemUseComplete
    {
        [Tooltip("Activate the state while equipped.")]
        [SerializeField] private bool m_WhileEquipped = true;
        [Tooltip("Activate the state between start use and use.")]
        [SerializeField] private bool m_WhileStartUse = true;
        [Tooltip("Activate the state between use and use complete.")]
        [SerializeField] private bool m_WhileUse = true;
        [Tooltip("The state names to enable.")]
        [StateName] [SerializeField] private string[] m_StateNames;

        private bool m_StartUsing = false;
        private bool m_Using = false;
        private bool m_StatesActive = false;
        
        public string[] StateNames
        {
            get => m_StateNames;
            set {
                if (Application.isPlaying == false) {
                    m_StateNames = value;
                    return;
                }

                var previousNames = m_StateNames;
                m_StateNames = value;

                // Enable, disable or keep the states.
                if (m_StatesActive == false) { return; }

                if (m_StateNames == null) { // Disable all previous states.
                    ActivateStateNames(false, previousNames);
                    return;
                }

                if (previousNames == null) { // Enable all new states.
                    ActivateStateNames(false, m_StateNames);
                    return;
                }

                // Keep the states that match.
                for (int i = 0; i < m_StateNames.Length; i++) {
                    if (previousNames.IndexOf(m_StateNames[i]) != -1) {
                        continue;
                    }
                    StateManager.SetState(Character, m_StateNames[i], true);
                }
                for (int i = 0; i < previousNames.Length; i++) {
                    if (m_StateNames.IndexOf(previousNames[i]) != -1) {
                        continue;
                    }
                    StateManager.SetState(Character, m_StateNames[i], false);
                }
            }
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            ActivateStateNames(register, m_StateNames);
        }
        
        /// <summary>
        /// Refresh which states are active.
        /// </summary>
        protected void RefreshStates()
        {
            if (IsActive == false) {
                ActivateStateNames(false, m_StateNames);
                return;
            }

            var activate = false;
            if (m_StartUsing) {
                if (m_WhileStartUse) {
                    activate = true;
                }
            }else if (m_Using) {
                if (m_WhileUse) {
                    activate = true;
                }
            }else if (m_WhileEquipped) {
                activate = true;
            }

            ActivateStateNames(activate, m_StateNames);
        }

        /// <summary>
        /// Activate or Deactivate the states in the array of names.
        /// </summary>
        /// <param name="activate">Activate or Deactivate?</param>
        /// <param name="names">The state names.</param>
        private void ActivateStateNames(bool activate, ListSlice<string> names)
        {
            m_StatesActive = activate;
            for (int i = 0; i < names.Count; i++) {
                StateManager.SetState(Character, names[i], activate);
            }
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public void StartItemUse(Use useAbility)
        {
            m_StartUsing = true;
            m_Using = false;
            RefreshStates();
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public void UseItem()
        {
            m_StartUsing = false;
            m_Using = true;
            RefreshStates();
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public void ItemUseComplete()
        {
            m_StartUsing = false;
            m_Using = false;
            RefreshStates();
        }
    }

    /// <summary>
    /// A simple module which has a boolean to enable can start.
    /// </summary>
    [Serializable]
    public class EnableCanStartUse : BasicUsableActionModule
    {
        [Tooltip("Can the item start use?")]
        [SerializeField] private bool m_CanStartUse = true;
        
        public bool CanStartUse { get => m_CanStartUse; set => m_CanStartUse = value; }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            if (m_CanStartUse == false) {
                return false;
            }
            
            return base.CanStartUseItem(useAbility, abilityState);
            
        }
    }
    
    /// <summary>
    /// A simple module which has a boolean to enable can start.
    /// </summary>
    [Serializable]
    public class DebugUse : UsableActionModule, IModuleUseItem
    {
        [Tooltip("The message to print on use.")]
        [SerializeField] private string m_Message = "Hello :)";

        /// <summary>
        /// Use the item.
        /// </summary>
        public void UseItem()
        {
            Debug.Log(m_Message);
        }
    }
}