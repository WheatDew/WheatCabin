/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Melee
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for modules that cause recoil.
    /// </summary>
    [Serializable]
    public abstract class MeleeRecoilModule : MeleeActionModule,
        IModuleStartItemUse, IModuleItemUseComplete, IModuleCanStopItemUse, IModuleStopItemUse, IModuleGetUseItemSubstateIndex
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;

        /// <summary>
        /// Checks if the object hit is a solid object. 
        /// </summary>
        /// <param name="dataStream">The melle use data stream.</param>
        /// <returns>True if the object that was hit is solid and should recoil.</returns>
        public abstract bool CheckForSolidObject(MeleeUseDataStream dataStream);
        
        /// <summary>
        /// Do play the recoil animation, sound, ect.
        /// </summary>
        /// <param name="dataStream">The melee use data stream.</param>
        public abstract void DoRecoil(MeleeUseDataStream dataStream);

        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public abstract void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData);

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public abstract void StartItemUse(Use useAbility);

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public abstract void ItemUseComplete();

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public abstract bool CanStopItemUse();

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public abstract void StopItemUse();
    }
    
    /// <summary>
    /// A base module dealing with recoil on melee attack impact.
    /// </summary>
    [Serializable]
    public class SimpleRecoil : MeleeRecoilModule
    {
        [Tooltip("The priority this comdule has over animation.")]
        [SerializeField] protected int m_SubstateIndexPriority = 500;
        [Tooltip("Specifies the animator and audio state from a recoil.")]
        [SerializeField] protected AnimatorAudioStateSet m_RecoilAnimatorAudioStateSet = new AnimatorAudioStateSet(0, new RandomRecoil(20));

        public int SubstateIndexPriority { get => m_SubstateIndexPriority; set => m_SubstateIndexPriority = value; }
        public AnimatorAudioStateSet RecoilAnimatorAudioStateSet { get { return m_RecoilAnimatorAudioStateSet; } set { m_RecoilAnimatorAudioStateSet = value; } }

        private bool m_HasRecoil;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_RecoilAnimatorAudioStateSet.Awake(CharacterItem, CharacterLocomotion);

        }


        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public override void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            if (streamData.Priority > m_SubstateIndexPriority) { return;}
            
            if (!m_HasRecoil) { return; }
            
            var substateIndex =  m_RecoilAnimatorAudioStateSet.GetItemSubstateIndex();
            streamData.AddSubstateData(substateIndex, m_SubstateIndexPriority, this);
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public override void StartItemUse(Use itemAbility)
        {
            m_HasRecoil = false;
        }

        /// <summary>
        /// Checks if the object hit is a solid object. 
        /// </summary>
        /// <param name="dataStream">The melee use data stream.</param>
        /// <returns>True if the object that was hit is solid and should recoil.</returns>
        public override bool CheckForSolidObject(MeleeUseDataStream dataStream)
        {
            var impactContext = dataStream.CollisionData.MeleeImpactCallbackContext;
            if (impactContext == null) {
                Debug.LogError("The impact context is null, make sure to assign the impact context to the melee collision data.");
                return false;
            }
            var impactCollisionData = impactContext.ImpactCollisionData;
            if (impactCollisionData == null) {
                Debug.LogError("The impact context has a null collision data, make sure to assign the impact context to the melee collision data.");
                return false;
            }
            
            var hitGameObject = impactContext.ImpactCollisionData.ImpactGameObject;
            
            // The shield can absorb some (or none) of the damage from the melee attack.
            ShieldCollider shieldCollider;
            if ((shieldCollider = hitGameObject.GetCachedComponent<ShieldCollider>()) != null) {
                if (shieldCollider.ShieldAction.DurabilityValue > 0) {
                    return true;
                }
            } else if (hitGameObject.GetCachedComponent<RecoilObject>() != null) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Do play the recoil animation, sound, ect.
        /// </summary>
        /// <param name="dataStream">The melee use data stream.</param>
        public override void DoRecoil(MeleeUseDataStream dataStream)
        {
            // Has recoil must start false for the get use item substate index to return the correct value.
            m_HasRecoil = false;
            
            var collisionData = dataStream.CollisionData.MeleeImpactCallbackContext.ImpactCollisionData;
            var hitCount = collisionData.HitCount;
            var collidersHit = collisionData.HitColliders;
            var useStateIndex = MeleeAction.GetUseItemSubstateIndex();
            
            // Has recoil must be set to true now such that the animation can play.
            m_HasRecoil = true;
                
            // If the active animator parameter state is a recoil state then notify the state of the colliders and collisions.
            var selector = m_RecoilAnimatorAudioStateSet.AnimatorAudioStateSelector;
            // The recoil AnimatorAudioState is starting.
            m_RecoilAnimatorAudioStateSet.StartStopStateSelection(true);
            if (selector is RecoilAnimatorAudioStateSelector recoilAnimatorAudioStateSelector) {
                recoilAnimatorAudioStateSelector.NextState(hitCount, collidersHit, useStateIndex);
            } else {
                m_RecoilAnimatorAudioStateSet.NextState();
            }

            UpdateItemAbilityAnimatorParameters();

            // Optionally play a recoil sound based upon the recoil animation.
            var visibleItem = CharacterItem.GetVisibleObject() != null ? CharacterItem.GetVisibleObject() : Character;
            m_RecoilAnimatorAudioStateSet.PlayAudioClip(visibleItem);
        }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public override void ItemUseComplete()
        {
            if (m_HasRecoil) {
                // The item has completed its recoil- inform the state set.
                m_RecoilAnimatorAudioStateSet.StartStopStateSelection(false);
            }
        }

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            var attacking = MeleeAction.IsAttacking;
            if (attacking && !m_HasRecoil) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stops the item use.
        /// </summary>
        public override void StopItemUse()
        {
            m_HasRecoil = false;
        }

        /// <summary>
        /// The item has been unequipped by the character.
        /// </summary>
        public override void Unequip()
        {
            base.Unequip();

            m_HasRecoil = false;
        }
        
        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_RecoilAnimatorAudioStateSet.OnDestroy();
        }
    }
}