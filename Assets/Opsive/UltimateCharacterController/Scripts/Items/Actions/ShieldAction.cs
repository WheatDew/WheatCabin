/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The shield action is used to reduce damage taken by the character.
    /// </summary>
    public class ShieldAction : CharacterItemAction
    {
        [Tooltip("Does the shield only protect the player when the character is aiming?")]
        [SerializeField] protected bool m_RequireAim;
        [Tooltip("Determines how much damage the shield absorbs. A value of 1 will absorb all of the damage, a value of 0 will not absorb any of the damage.")]
        [Range(0, 1)] [SerializeField] protected float m_AbsorptionFactor = 1;
        [Tooltip("Should the shield absorb damage caused by explosions?")]
        [SerializeField] protected bool m_AbsorbExplosions;
        [Tooltip("Should an impact be applied when the weapon is hit by another object?")]
        [SerializeField] protected bool m_ApplyImpact = true;
        [Tooltip("Specifies the animator and audio state for when the shield is impacted by another object.")]
        [SerializeField] protected AnimatorAudioStateSet m_ImpactAnimatorAudioStateSet = new AnimatorAudioStateSet();
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemImpactComplete animation event or wait for the specified duration before completing the impact.")]
        [SerializeField] protected AnimationSlotEventTrigger m_ImpactCompleteEvent = new AnimationSlotEventTrigger(false, 0.2f);
        [Tooltip("The name of the shield's durability attribute. When the durability reaches 0 the shield will not absorb any damage.")]
        [SerializeField] protected string m_DurabilityAttributeName = "Durability";
        [Tooltip("Should the item be dropped from the character when the durability is depleted?")]
        [SerializeField] protected bool m_DropWhenDurabilityDepleted;

        public bool RequireAim { get { return m_RequireAim; } set { m_RequireAim = value; } }
        public float AbsorptionFactor { get { return m_AbsorptionFactor; } set { m_AbsorptionFactor = value; } }
        public bool AbsorbExplosions { get { return m_AbsorbExplosions; } set { m_AbsorbExplosions = value; } }
        public bool ApplyImpact { get { return m_ApplyImpact; } set { m_ApplyImpact = value; } }
        public AnimatorAudioStateSet ImpactAnimatorAudioStateSet { get { return m_ImpactAnimatorAudioStateSet; } set { m_ImpactAnimatorAudioStateSet = value; } }
        public AnimationSlotEventTrigger ImpactCompleteEvent { get { return m_ImpactCompleteEvent; } set { m_ImpactCompleteEvent.CopyFrom(value); } }
        public string DurabilityAttributeName
        {
            get { return m_DurabilityAttributeName; }
            set
            {
                m_DurabilityAttributeName = value;
                if (Application.isPlaying) {
                    if (!string.IsNullOrEmpty(m_DurabilityAttributeName) && m_AttributeManager != null) {
                        m_DurabilityAttribute = m_AttributeManager.GetAttribute(m_DurabilityAttributeName);
                    } else {
                        m_DurabilityAttribute = null;
                    }
                }
            }
        }
        public bool DropWhenDurabilityDepleted { get { return m_DropWhenDurabilityDepleted; } set { m_DropWhenDurabilityDepleted = value; } }

        private AttributeManager m_AttributeManager;
        private Attribute m_DurabilityAttribute;
        private bool m_Aiming;
        private bool m_HasImpact;

        public float DurabilityValue { get { return (m_DurabilityAttribute != null ? m_DurabilityAttribute.Value : 0); } }

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        protected override void InitializeActionInternal(bool force)
        {
            base.InitializeActionInternal(force);
            
            m_AttributeManager = GetComponent<AttributeManager>();
            if (!string.IsNullOrEmpty(m_DurabilityAttributeName)) {
                if (m_AttributeManager == null) {
                    Debug.LogError("Error: The shield " + m_GameObject.name + " has a durability attribute specified but no Attribute Manager component.");
                } else {
                    m_DurabilityAttribute = m_AttributeManager.GetAttribute(m_DurabilityAttributeName);

                    if (m_DurabilityAttribute != null) {
                        EventHandler.RegisterEvent(m_DurabilityAttribute, "OnAttributeReachedDestinationValue", DurabilityDepleted);
                    }
                }
            }
            
            m_ImpactAnimatorAudioStateSet.Awake(CharacterItem, m_Character.GetCachedComponent<UltimateCharacterLocomotion>());
            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
        }

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        public int GetItemSubstateIndex()
        {
            if (m_HasImpact) {
                return m_ImpactAnimatorAudioStateSet.GetItemSubstateIndex();
            }
            return -1;
        }

        /// <summary>
        /// Damages the shield.
        /// </summary>
        /// <param name="ctx">The impact call back context, which caused the shield from being damaged.</param>
        /// <param name="amount">The amount of damage to apply/</param>
        /// <returns>The amount of damage remaining which should be applied to the character.</returns>
        public float Damage(ImpactCallbackContext ctx, float amount)
        {
            // The shield can't absorb damage if it requires the character to be aiming and the character isn't aiming.
            if (!m_Aiming && m_RequireAim) {
                return amount;
            }
            
            // The shield may not be able to absorb damage caused by explosions.
            if ((ctx.ImpactCollisionData.SourceComponent is Objects.Explosion) && !m_AbsorbExplosions) {
                return amount;
            }
            
            if (m_ApplyImpact) {
                m_HasImpact = true;
                m_ImpactAnimatorAudioStateSet.StartStopStateSelection(true);
                m_ImpactAnimatorAudioStateSet.NextState();
                var visibleItem = m_CharacterItem.GetVisibleObject() != null ? m_CharacterItem.GetVisibleObject() : m_Character;
                m_ImpactAnimatorAudioStateSet.PlayAudioClip(visibleItem);
                EventHandler.ExecuteEvent<ShieldAction, ImpactCallbackContext>(m_Character, "OnShieldImpact", this, ctx);
            }

            // If the shield is invincible then no damage is applied to it and the resulting absorption factor should be returned.
            if (m_DurabilityAttribute == null) {
                return amount * (1 - m_AbsorptionFactor);
            }

            // If the shield's durability is depleted then the entire damage amount should be applied to the character.
            if (m_DurabilityAttribute.Value == m_DurabilityAttribute.MinValue) {
                return amount;
            }

            // Damage the shield and amount of damage which be applied to the character.
            var absorbedDamage = Mathf.Min(amount * m_AbsorptionFactor, m_DurabilityAttribute.Value);
            m_DurabilityAttribute.Value -= absorbedDamage;

            // The shield may be dropped if the damage reaches the minimum value.
            if (m_DurabilityAttribute.Value == m_DurabilityAttribute.MinValue && m_DropWhenDurabilityDepleted) {
                m_Inventory.RemoveItemIdentifier(CharacterItem.ItemIdentifier, CharacterItem.SlotID, 1, false);
                CharacterItem.Drop(0, true);
            }

            // The remaining damage should be applied to the character.
            return amount - absorbedDamage;
        }

        /// <summary>
        /// The block animation has played - reset the impact.
        /// </summary>
        public void StopBlockImpact()
        {
            m_HasImpact = false;
            m_ImpactAnimatorAudioStateSet.StartStopStateSelection(false);
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart && !(CharacterItem.CharacterLocomotion.LookSource is LocalLookSource)) {
                return;
            }
            m_Aiming = aim;
        }

        /// <summary>
        /// The shield is no longer durable.
        /// </summary>
        private void DurabilityDepleted()
        {
            if (!m_DropWhenDurabilityDepleted) {
                return;
            }

            // Remove the item from the inventory before dropping it. This will ensure the dropped prefab does not contain any ItemIdentifier amount so the
            // item can't be picked up again.
            m_Inventory.RemoveItemIdentifier(CharacterItem.ItemIdentifier, CharacterItem.SlotID, 1, false);
            CharacterItem.Drop(0, true);
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_ImpactAnimatorAudioStateSet.OnDestroy();
            EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnBlockAbilityStart", OnAim);
            if (m_DurabilityAttribute != null) {
                EventHandler.UnregisterEvent(m_DurabilityAttribute, "OnAttributeReachedDestinationValue", DurabilityDepleted);
            }
            EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
        }
    }
}