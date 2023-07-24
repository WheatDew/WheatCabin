/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules
{
    using Opsive.Shared.Audio;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// The trigger data contains information about how the action was triggered.
    /// </summary>
    public class TriggerData
    {
        protected float m_Force;
        protected int m_Index;
        protected bool m_FireInLookSourceDirection;

        public float Force { get => m_Force; set => m_Force=value; }
        public int Index { get => m_Index; set => m_Index=value; }
    }
    
    /// <summary>
    /// The base trigger module used to trigger item actions.
    /// </summary>
    [Serializable]
    public abstract class TriggerModule : ActionModule, IModuleTrigger
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;
        
        protected UsableAction m_UsableAction;
        public UsableAction UsableAction => m_UsableAction;

        protected TriggerData m_TriggerData;
        public TriggerData TriggerData { get { return m_TriggerData; } set => m_TriggerData = value; }
        
        protected bool m_IsTriggering = false;
        protected bool m_WasTriggered = false;
        public virtual bool IsTriggering => m_IsTriggering;
        public virtual bool WasTriggered => m_WasTriggered;

        /// <summary>
        /// Creates the trigger data that will be cached.
        /// </summary>
        /// <returns>The created trigger data.</returns>
        public virtual TriggerData CreateTriggerData()
        {
            return new TriggerData();
        }

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            if (itemAction is UsableAction usableCharacterItemAction) {
                m_UsableAction = usableCharacterItemAction;
            } else {
                Debug.LogError($"The module type '{GetType()}' does not match the character item action type: '{itemAction?.GetType()}'.");
            }

            base.Initialize(itemAction);
            m_TriggerData = CreateTriggerData();
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            m_IsTriggering = false;
            m_WasTriggered = false;
            base.UpdateRegisteredEventsInternal(register);
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public abstract void UseItemTrigger();

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public virtual bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            if (useAbility != null && useAbility.IsUseInputTryingToStop()) { return false; }

            return true;
        }

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public abstract bool CanStopItemUse();

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public abstract void StartItemUse(Use useAbility);

        /// <summary>
        /// Use the item.
        /// </summary>
        public abstract void UseItem();

        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public abstract void UseItemUpdate();

        /// <summary>
        /// Is the item use pending, meaning it has started but isn't ready to be used just yet.
        /// </summary>
        /// <returns>True if the item is use pending.</returns>
        public abstract bool IsItemUsePending();

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public abstract void ItemUseComplete();

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public abstract void StopItemUse();

        /// <summary>
        /// The item is trying to stop the use.
        /// </summary>
        public abstract void TryStopItemUse();

        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public abstract void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData);

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public virtual bool CanUseItem()
        {
            return true;
        }
        
        /// <summary>
        /// Reset the module after the item has been unequipped or removed.
        /// </summary>
        /// <param name="force">Force the reset.</param>
        public override void ResetModule(bool force)
        {
            base.ResetModule(force);
            m_IsTriggering = false;
            m_WasTriggered = false;
        }
    }
    
    /// <summary>
    /// An abstract trigger module which uses animation audio state set.
    /// </summary>
    [Serializable]
    public abstract class TriggerModuleAnimatorAudioStateSet : TriggerModule
    {
        [Tooltip("Should the audio play when the item starts to be used? If false it will be played when the item is used.")]
        [SerializeField] protected bool m_PlayAudioOnStartUse;
        [Tooltip("Specifies the animator and audio state that should be triggered when the item is started.")]
        [SerializeField] protected ItemSubstateIndexData m_SubstateIndexData = new ItemSubstateIndexData(0,100);
        [Tooltip("Specifies the animator and audio state that should be triggered when the item is used.")]
        [SerializeField] protected AnimatorAudioStateSet m_UseAnimatorAudioStateSet = new AnimatorAudioStateSet(2);

        protected Use m_UseItemAbility;
        
        public bool PlayAudioOnStartUse { get => m_PlayAudioOnStartUse; set => m_PlayAudioOnStartUse = value; }

        public AnimatorAudioStateSet UseAnimatorAudioStateSet
        {
            get => m_UseAnimatorAudioStateSet;
            set => m_UseAnimatorAudioStateSet = value;
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_UseAnimatorAudioStateSet.Awake(CharacterItem, CharacterLocomotion);
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use itemAbility)
        {
            m_UseItemAbility = itemAbility;
            // The use AnimatorAudioState is starting.
            m_UseAnimatorAudioStateSet.StartStopStateSelection(true);
            var use = m_UseAnimatorAudioStateSet.NextState();
            if (use && m_PlayAudioOnStartUse) {
                var visibleObject = CharacterItem.GetVisibleObject() != null ? CharacterItem.GetVisibleObject() : Character;
                m_UseAnimatorAudioStateSet.PlayAudioClip(visibleObject);
            }
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItem()
        {
            // Optionally play a use sound based upon the use animation.
            if (!m_PlayAudioOnStartUse) {
                var visibleObject = CharacterItem.GetVisibleObject() != null ? CharacterItem.GetVisibleObject() : Character;
                m_UseAnimatorAudioStateSet.PlayAudioClip(visibleObject);
            }
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            // The item has been used- inform the state set.
            m_UseAnimatorAudioStateSet.StartStopStateSelection(false);
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_UseAnimatorAudioStateSet.OnDestroy();
        }

        /// <summary>
        /// Write the module name in an easy to read format for debugging.
        /// </summary>
        /// <returns>The string representation of the module.</returns>
        public override string ToString()
        {
            if (m_UseAnimatorAudioStateSet != null && m_UseAnimatorAudioStateSet.States != null &&
                m_UseAnimatorAudioStateSet.States.Length > 0) {
                var states = m_UseAnimatorAudioStateSet.States;
                var audioAnimatorSubtateString = states[0].ItemSubstateIndex.ToString();
                for (int i = 1; i < states.Length; i++) {
                    audioAnimatorSubtateString += ", " + states[i].ItemSubstateIndex;
                }

                if (m_SubstateIndexData.Index != 0) {
                    return base.ToString()+$" ({m_SubstateIndexData.Index}+[{audioAnimatorSubtateString}])";
                } else {
                    return base.ToString()+$" ({audioAnimatorSubtateString})";
                }
               
            } else {
                return base.ToString();
            }
        }
        
        /// <summary>
        /// Reset the module after the item has been unequipped or removed.
        /// </summary>
        /// <param name="force">Force the reset.</param>
        public override void ResetModule(bool force)
        {
            base.ResetModule(force);
            m_UseItemAbility = null;
        }
    }
    
    /// <summary>
    /// A base class for simple trigger which execute the action once.
    /// </summary>
    [Serializable]
    public abstract class SimpleBaseTrigger : TriggerModuleAnimatorAudioStateSet
    {
        [Tooltip("If True and the use ability input stops, the module will say that it can stop early.")]
        [SerializeField] protected bool m_CanStopIfInputStop = false;
        [Tooltip("If the usable action can stop, add this index to the amount.")]
        [SerializeField] protected int m_CanStopSubstateIndexAddition = 10;

        private bool m_InputActive = false;
        
        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public override void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            int index;
            
            if (!m_IsTriggering) {
                index = -1;
            } else {
                var audioStateIndex = UseAnimatorAudioStateSet.GetItemSubstateIndex();
                var canStopSubstateIndexAddition = UsableAction.CanStopItemUse() ? m_CanStopSubstateIndexAddition : 0;
                index = m_SubstateIndexData.Index + audioStateIndex + canStopSubstateIndexAddition;
            }

            var data = new ItemSubstateIndexData(index, m_SubstateIndexData);
            streamData.TryAddSubstateData(this, data);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var baseResult = base.CanStartUseItem(useAbility, abilityState);
            if (!baseResult) { return false; }
            
            if (m_CanStopIfInputStop && useAbility.IsUseInputTryingToStop()) {
                return false;
            }
            
            if (m_WasTriggered) {
                return false;
            }
            
            if (abilityState == UsableAction.UseAbilityState.Start && m_IsTriggering) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use useAbility)
        {
            base.StartItemUse(useAbility);
            
            m_IsTriggering = true;
            
            // Using Force Change true.
            // This makes sure the weapon isn't stuck within the Use Animation if the transitions are set properly.
            // If your item gets stuck while spamming the button, add a transition using the SlotXItemStatChange Trigger.
            UpdateItemAbilityAnimatorParameters(true);
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItemTrigger()
        {
            m_TriggerData.Force = 1;
            UsableAction.TriggerItemAction(m_TriggerData);
            m_WasTriggered = true;
        }

        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public override void UseItemUpdate()
        {
            if(m_UseItemAbility == null){ return; }
            if(!m_IsTriggering){ return; }

            // Make sure to update the Animator parameters when the input changes.
            var isUseInputTryingToStop = m_UseItemAbility.IsUseInputTryingToStop();
            if (m_InputActive && isUseInputTryingToStop) {
                // The input was active, but it is now trying to change.
                m_InputActive = false;
                UpdateItemAbilityAnimatorParameters();
            }else if (!m_InputActive && !isUseInputTryingToStop) {
                m_InputActive = true;
                UpdateItemAbilityAnimatorParameters();
            }
        }

        /// <summary>
        /// Is the item use pending, meaning it has started but isn't ready to be used just yet.
        /// </summary>
        /// <returns>True if the item is use pending.</returns>
        public override bool IsItemUsePending()
        {
            return false;
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public override void ItemUseComplete()
        {
            // Stop the animation on complete.
            m_IsTriggering = false;
            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// The item is trying to stop the use.
        /// </summary>
        public override void TryStopItemUse()
        {
            
        }

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            if (m_CanStopIfInputStop && (m_UseItemAbility != null && m_UseItemAbility.IsUseInputTryingToStop())) {
                return true;
            }
            if (m_IsTriggering) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            base.StopItemUse();

            if (!m_WasTriggered) {
                // If the item is stopped before it is thrown then the character may have died. The item should be released.
                Opsive.Shared.Events.EventHandler.ExecuteEvent(Character, "OnItemActionTriggerStoppedEarly", this);
            }
            
            //Stop the animation on stop item use.
            m_WasTriggered = false;
            m_IsTriggering = false;
            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// Reset the module after the item has been unequipped or removed.
        /// </summary>
        /// <param name="force">Force the reset.</param>
        public override void ResetModule(bool force)
        {
            base.ResetModule(force);
            m_InputActive = false;
        }
    }

    /// <summary>
    /// A combo trigger module allows for actions with different states to activate in sequence.
    /// </summary>
    [Serializable]
    public abstract class ComboTriggerModule : TriggerModuleAnimatorAudioStateSet
    {
        [Tooltip("Can the next use state play between the ItemUsed and ItemUseComplete events?")]
        [SerializeField] protected bool m_AllowAttackCombos = true;

        protected bool m_StartedUseAttack;
        protected bool m_UsedAttack;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);

            //Don't wait for use complete such that attacks can chain between use and use complete.
            UsableAction.WaitForUseCompleteBeforeCanStartAgain = false;
        }

        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public override void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            int index;

            if (!m_StartedUseAttack) {
                index = -1;
            } else {
                index = m_SubstateIndexData.Index + m_UseAnimatorAudioStateSet.GetItemSubstateIndex();
            }

            var data = new ItemSubstateIndexData(index, m_SubstateIndexData);
            streamData.TryAddSubstateData(this, data);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var baseResult = base.CanStartUseItem(useAbility, abilityState);
            if (!baseResult) { return false; }
            
            if (abilityState == UsableAction.UseAbilityState.Start) {
                if (m_StartedUseAttack) {
                    
                    if (m_UseAnimatorAudioStateSet.States.Length == 1) {
                        Debug.LogError("Error: The MeleeWeapon cannot be used again. Another state should be added to the Use Animator Audio State Set. See the first troubleshooting tip for more info: " +
                                       "https://opsive.com/support/documentation/ultimate-character-controller/items/actions/usable/melee-weapon/.");
                    }
                    
                    if(m_UsedAttack && m_AllowAttackCombos) {

                        return true;
                    }

                    return false;
                }
            }

            // The weapon can be used.
            return true;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use useAbility)
        {
            base.StartItemUse(useAbility);
            m_UsedAttack = false;
            m_StartedUseAttack = true;
            
            //Stop waiting for the use complete event in case the combo attack is started early.
            UsableAction.UseCompleteEvent.CancelWaitForEvent();
            
            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItemTrigger()
        {
            m_UsedAttack = true;
            m_TriggerData.Force = 1;
            UsableAction.TriggerItemAction(m_TriggerData);
        }

        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public override void UseItemUpdate()
        {
        }

        /// <summary>
        /// Is the item use pending, meaning it has started but isn't ready to be used just yet.
        /// </summary>
        /// <returns>True if the item is use pending.</returns>
        public override bool IsItemUsePending()
        {
            return false;
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public override void ItemUseComplete()
        {
            m_StartedUseAttack = false;

            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public override bool CanStopItemUse()
        {           
            if (m_StartedUseAttack) {
                return false;
            }

            return true;
        }


        /// <summary>
        /// The item is trying to stop the use.
        /// </summary>
        public override void TryStopItemUse()
        {
            
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            base.StopItemUse();
            m_StartedUseAttack = false;
            m_UsedAttack = false;
            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// Reset the module after the item has been unequipped or removed.
        /// </summary>
        /// <param name="force">Force the reset.</param>
        public override void ResetModule(bool force)
        {
            base.ResetModule(force);
            m_StartedUseAttack = false;
            m_UsedAttack = false;
        }
    }

    /// <summary>
    /// A simple trigger which execute the action once.
    /// </summary>
    [Serializable]
    public class Simple : SimpleBaseTrigger
    { }
    
    /// <summary>
    /// A trigger module which automatically repeats the action for as long as the use ability is active.
    /// </summary>
    [Serializable]
    public class Repeat : SimpleBaseTrigger
    {
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            if (m_CanStopIfInputStop && (useAbility != null && useAbility.IsUseInputTryingToStop())) {
                return true;
            }
            
            if (abilityState == UsableAction.UseAbilityState.Start && m_IsTriggering) {
                return false;
            }

            // Cannot repeat the use until it was actually used.
            if (abilityState == UsableAction.UseAbilityState.Update && !m_WasTriggered) {
                return false;
            }
            
            return true;
        }

        public override bool CanStopItemUse()
        {
            if (m_CanStopIfInputStop && (m_UseItemAbility != null && m_UseItemAbility.IsUseInputTryingToStop())) {
                return true;
            }
            if (m_IsTriggering) {
                return false;
            }

            if (m_UseItemAbility != null && !m_UseItemAbility.IsUseInputTryingToStop()){
                return false;
            }

            return true;
        }
    }
    
    /// <summary>
    /// A trigger which execute the action a predefined amount of times before stopping.
    /// </summary>
    [Serializable]
    public class Burst : SimpleBaseTrigger, IModuleReloadClip
    {
        [Tooltip("The number of shot per input press.")]
        [SerializeField] protected int m_BurstSize = 5;
        [Tooltip("Can the Use ability stop before all the bursts shots have been fired?")]
        [SerializeField] protected bool m_CancelBurstOnStop = false;
        [Tooltip("The time to wait between bursts before repeating. (-1) to not repeat.")]
        [SerializeField] protected float m_BurstRepeatDelay = -1;
        
        protected int m_FiredBurstCount = 0;
        protected int m_FiredCompleteCount = 0;
        protected float m_LastTriggerTime = 0;

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            if (m_FiredCompleteCount >= m_BurstSize) {

                if (m_BurstRepeatDelay >= 0 && m_LastTriggerTime + m_BurstRepeatDelay <= Time.time) {
                    if (m_CanStopIfInputStop && (useAbility != null && useAbility.IsUseInputTryingToStop())) {
                        return false;
                    } else {
                        // Start the burst again.
                        ResetBurstCount();
                        return true;
                    }
                }
                
                return false;
            }
            
            if (abilityState == UsableAction.UseAbilityState.Start && m_IsTriggering) {
                return false;
            }
            
            // Cannot repeat the use until it was actually used.
            if (abilityState == UsableAction.UseAbilityState.Update && m_FiredBurstCount != m_FiredCompleteCount) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets the burst count.
        /// </summary>
        private void ResetBurstCount()
        {
            m_FiredBurstCount = 0;
            m_FiredCompleteCount = 0;
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItemTrigger()
        {
            m_LastTriggerTime = Time.time;
            m_FiredBurstCount += 1;
            base.UseItemTrigger();
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public override void ItemUseComplete()
        {
            base.ItemUseComplete();
            m_FiredCompleteCount += 1;
        }

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            if (m_CanStopIfInputStop && (m_UseItemAbility != null && m_UseItemAbility.IsUseInputTryingToStop())) {
                return true;
            }
            
            if (m_IsTriggering) {
                return false;
            }

            if (!m_CancelBurstOnStop && m_FiredCompleteCount < m_BurstSize) {
                return false;
            }
            
            // If burst repeat, then don't stop unless the input is trying to stop.
            if (m_BurstRepeatDelay > 0 && (m_UseItemAbility != null && !m_UseItemAbility.IsUseInputTryingToStop())) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            ResetBurstCount();
            base.StopItemUse();
        }

        /// <summary>
        /// Reset the Fired burst count when reloading.
        /// </summary>
        /// <param name="fullClip">Reload the clip completely?</param>
        public void ReloadClip(bool fullClip)
        {
            ResetBurstCount();
        }
    }

     /// <summary>
    /// The Charge module lets you charge up a fire, it can auto fire, or fire on TryStop
    /// The fire does not happen on Use, instead it happens on charge complete or TryStop, and the charge starts in Use.
    /// </summary>
    [Serializable]
    public class Charged : TriggerModuleAnimatorAudioStateSet
    {
        // Info keys for debugging.
        public const string InfoKey_IsCharging  = "Trigger/ChargedTrigger/IsCharging";
        
        [Tooltip("A set of AudioClips that can be played when the weapon is charging.")]
        [SerializeField] protected AudioClipSet m_ChargeAudioClipSet = new AudioClipSet();
        [Tooltip("The minimum force of the charge.")]
        [SerializeField] protected float m_MinChargeForce = 0.1f;
        [Tooltip("The maximum force of the charge.")]
        [SerializeField] protected float m_MaxChargeForce = 1f;
        [Tooltip("Normalizes the time between the start and complete charging times.")]
        [SerializeField] protected float m_ChargeTimeForceNormalizer = 1;
        [SerializeField] protected ItemSubstateIndexData m_ChargeSubstateParameterValue = new ItemSubstateIndexData(0,100);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemMinChargeComplete animation event or wait for the specified duration before being used.")]
        [SerializeField] protected AnimationSlotEventTrigger m_MinChargeCompleteEvent = new AnimationSlotEventTrigger(true, 0.2f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemMaxChargeComplete animation event or wait for the specified duration before being used.")]
        [SerializeField] protected AnimationSlotEventTrigger m_MaxChargeCompleteEvent = new AnimationSlotEventTrigger(true, 0.2f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemDepleteCharge animation event or wait for the specified duration before being used.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_UnChargeSubstateParameterValue")]
        [SerializeField] protected ItemSubstateIndexData m_DepleteChargeSubstateParameterValue = new ItemSubstateIndexData(0,100);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemDepleteChargeComplete animation event or wait for the specified duration before being used.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_UnchargeCompleteEvent")]
        [SerializeField] protected AnimationSlotEventTrigger m_DepleteChargeCompleteEvent = new AnimationSlotEventTrigger(true, 0.2f);
        [Tooltip("Should the charge automatically fire when it is fully charged?")]
        [SerializeField] protected bool m_AutoFireOnFullCharge;
        [Tooltip("Can the fire be repeated?")]
        [SerializeField] protected bool m_RepeatFire;
        [Tooltip("Should the module deplete when it is stopped early?")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_UnchargeOnStopEarly")]
        [SerializeField] protected bool m_DepleteChargeOnStopEarly = true;

        protected bool m_IsCharging = false;
        protected bool m_RegisteredToEvents = false;
        protected bool m_TryStop;
        protected bool m_Stopping = false;

        protected float m_StartChargeTime = -1;

        public float StartChargeTime => m_StartChargeTime;

        public virtual bool IsCharging
        {
            get => m_IsCharging;
            protected set
            {
                m_IsCharging = value;
                var message = m_IsCharging ? "is charging" : "is not charging";
                CharacterItemAction.DebugLogger.SetInfo(InfoKey_IsCharging,message);
            }
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            if (register == m_RegisteredToEvents) {
                return;
            }
            
            base.UpdateRegisteredEventsInternal(register);

            m_RegisteredToEvents = register;

            var eventTarget = Character;
            m_MinChargeCompleteEvent.RegisterUnregisterEvent(register, eventTarget,"OnAnimatorItemMinChargeComplete",SlotID, HandleItemUseMinChargeComplete);
            m_MaxChargeCompleteEvent.RegisterUnregisterEvent(register, eventTarget,"OnAnimatorItemMaxChargeComplete",SlotID, HandleItemUseMaxChargeComplete);
            m_DepleteChargeCompleteEvent.RegisterUnregisterEvent(register, eventTarget,"OnAnimatorItemDepleteChargeComplete",SlotID, HandleItemUseUnchargeComplete);
        }

        /// <summary>
        /// Handle the uncharge item animation event.
        /// </summary>
        private void HandleItemUseUnchargeComplete()
        {
            IsCharging = false;
            m_IsTriggering = false;
            // Stop the item once it has finished uncharging.
            m_UseItemAbility.SetCanStopAbility(m_UsableAction, true);
        }

        /// <summary>
        /// Handle the max charge item animation event.
        /// </summary>
        protected virtual void HandleItemUseMaxChargeComplete()
        {
            // Do nothing.
        }

        /// <summary>
        /// Handle the min charge item animation event.
        /// </summary>
        protected virtual void HandleItemUseMinChargeComplete()
        {
            // Do nothing.
        }

        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public override void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            if (m_DepleteChargeCompleteEvent.IsWaiting) {
                streamData.TryAddSubstateData(this, m_DepleteChargeSubstateParameterValue);
                return;
            }
            
            if (m_IsCharging) {
                streamData.TryAddSubstateData(this, m_ChargeSubstateParameterValue);
                return;
            }
            
            if (!m_IsTriggering) {
                var notTriggeringData = new ItemSubstateIndexData(-1, m_SubstateIndexData.Priority, false);
                streamData.TryAddSubstateData(this, notTriggeringData);
                return;
            }

            var audioStateIndex = UseAnimatorAudioStateSet.GetItemSubstateIndex();
            var index = m_SubstateIndexData.Index + audioStateIndex;
            var data = new ItemSubstateIndexData(index, m_SubstateIndexData);
            streamData.TryAddSubstateData(this, data);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var baseResult = base.CanStartUseItem(useAbility, abilityState);
            if (!baseResult) { return false; }
            
            if ((!m_RepeatFire && m_WasTriggered) || m_IsCharging) {
                return false;
            }
            
            if (abilityState == UsableAction.UseAbilityState.Start && m_IsTriggering) {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use itemAbility)
        {
            base.StartItemUse(itemAbility);
            
            m_IsTriggering = true;
            m_TryStop = false;
            
            // Start Charging
            m_StartChargeTime = Time.time;
            m_IsCharging = true;
            
            //Play the Charge Audio
            m_ChargeAudioClipSet.PlayAudioClip(CharacterItem.GetVisibleObject());
            //Start waiting for the charge complete events
            m_MinChargeCompleteEvent.WaitForEvent(true);
            m_MaxChargeCompleteEvent.WaitForEvent(true);
            
            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public override bool CanUseItem()
        {
            var baseResult = base.CanUseItem();
            if (!baseResult) {
                return false;
            }

            if (m_IsCharging || m_MinChargeCompleteEvent.IsWaiting) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public override void UseItemTrigger()
        {
            // This is called when charging is false
            if (m_IsCharging || m_MinChargeCompleteEvent.IsWaiting) {
                Debug.LogWarning("The item shouldn't be used while the trigger is being charged.");
                return;
            }
            
            UsableAction.TriggerItemAction(m_TriggerData);
            m_WasTriggered = true;
        }

        /// <summary>
        /// Release the charge.
        /// </summary>
        protected virtual void ReleaseCharge()
        {
            // Setting charging to false will cause the item to be used.
            m_IsCharging = false;

            if (m_ChargeTimeForceNormalizer <= 0) {
                Debug.LogWarning($"The m_ChargeTimeForceNormalizer must be a positive value: {m_ChargeTimeForceNormalizer}", CharacterItem);
                m_ChargeTimeForceNormalizer = 1;
            }
            
            var normalizedShotDeltaTime = Mathf.Clamp01((Time.time - m_StartChargeTime) / m_ChargeTimeForceNormalizer);
            var shotForce = m_MinChargeForce + normalizedShotDeltaTime * (m_MaxChargeForce - m_MinChargeForce);
            m_TriggerData.Force = shotForce;
        }

        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public override void UseItemUpdate()
        {
            if (m_IsCharging && !m_MinChargeCompleteEvent.IsWaiting) {
                var canStop = m_TryStop || (m_UseItemAbility?.IsUseInputTryingToStop() ?? m_TryStop);
                // The module wasn't able to be stopped earlier because it wasn't finished. Fire the module now.
                if (canStop) {
                    ReleaseCharge();
                    return;
                }
                
                if(m_AutoFireOnFullCharge && !m_MaxChargeCompleteEvent.IsWaiting) {
                    // Done charging.
                    ReleaseCharge();
                    return;
                }
            }
        }

        /// <summary>
        /// Is the item use pending, meaning it has started but isn't ready to be used just yet.
        /// </summary>
        /// <returns>True if the item is use pending.</returns>
        public override bool IsItemUsePending()
        {
            return m_IsCharging;
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public override void ItemUseComplete()
        {
            // Stop the animation on complete.
            m_IsTriggering = false;
            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// The item is trying to stop the use.
        /// </summary>
        public override void TryStopItemUse()
        {
            m_TryStop = m_UseItemAbility?.IsUseInputTryingToStop() ?? true;

            if (m_Stopping || !m_TryStop) {
                return;
            }

            m_Stopping = true;
            
            if (!m_IsCharging){ return; }

            // Can't stop if the minimum charge amount hasn't been reached.
            if (m_MinChargeCompleteEvent.IsWaiting) {
                if (m_DepleteChargeOnStopEarly) {
                    m_MinChargeCompleteEvent.CancelWaitForEvent();
                    m_MaxChargeCompleteEvent.CancelWaitForEvent();
                    m_DepleteChargeCompleteEvent.WaitForEvent(false);
                }
                
                return;
            }

            m_MaxChargeCompleteEvent.CancelWaitForEvent();
            ReleaseCharge();
        }

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            if ( (m_IsTriggering && !m_WasTriggered) || m_IsCharging) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            base.StopItemUse();
            m_WasTriggered = false;
            m_TryStop = false;
            m_Stopping = false;
            
            m_MinChargeCompleteEvent.CancelWaitForEvent();
            m_MaxChargeCompleteEvent.CancelWaitForEvent();
            m_DepleteChargeCompleteEvent.CancelWaitForEvent();
            
            //Stop the animation on stop item use.
            m_IsTriggering = false;
            IsCharging = false;
            UpdateItemAbilityAnimatorParameters();
        }
    }
    
    /// <summary>
    /// This trigger module allows for combos and execute actions once.
    /// </summary>
    [Serializable]
    public class SimpleCombo : ComboTriggerModule
    {
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var canStart = base.CanStartUseItem(useAbility, abilityState);
            if (!canStart) {
                return false;
            }
            
            if (abilityState == UsableAction.UseAbilityState.Update) {
                if (m_StartedUseAttack) {
                    return true;
                }
                return false;
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// A combo trigger module which automatically repeats the action for as long as the use ability is active.
    /// </summary>
    [Serializable]
    public class RepeatCombo : ComboTriggerModule
    {
       
    }
    
    /// <summary>
    /// A simple trigger which requires the character to be in the air to start.
    /// </summary>
    [Serializable]
    public class InAir : SimpleBaseTrigger,
        IModuleCanStartAbility
    {
        [Tooltip("Does the weapon require the In Air Melee Use Item Ability in order to be used while in the air?")]
        [SerializeField] protected bool m_RequireInAirAbilityInAir = true;

        /// <summary>
        /// Can the ability start?
        /// </summary>
        /// <param name="ability">The ability trying to start.</param>
        /// <returns>True if it can start.</returns>
        public bool CanStartAbility(Ability ability)
        {
            if (!(ability is Jump)) {
                return true;
            }

            // The ability is a the Jump ability. 
            if (m_RequireInAirAbilityInAir) {
                return false;
            }

            // The ability can start if RequireGrounded is false.
            var useStateIndex = UseAnimatorAudioStateSet.GetStateIndex();
            if (useStateIndex == -1) {
                return true;
            }
            return !UseAnimatorAudioStateSet.States[useStateIndex].RequireGrounded;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            // The MeleeWeapon may require the InAirMeleeUse ability in order for it to be used while in the air.
            if (m_RequireInAirAbilityInAir && !(useAbility is InAirMeleeUse) &&
                (CharacterLocomotion.UsingGravity && !CharacterLocomotion.Grounded || CharacterLocomotion.IsAbilityTypeActive<Character.Abilities.Jump>())) {
                return false;
            }
            
            return base.CanStartUseItem(useAbility, abilityState);
        }
    }
}