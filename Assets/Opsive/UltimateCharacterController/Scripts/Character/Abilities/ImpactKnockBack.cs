/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Plays a full body animation in response to a melee counter attack.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultStopType(AbilityStopType.Manual)]
    [DefaultUseRootMotionPosition(AbilityBoolOverride.True)]
    [DefaultAbilityIndex(13)]
    public class ImpactKnockBack : Ability
    {
        [Tooltip("Specifies if the ability should stop when the OnAnimatorImpactKnockBackComplete event is received or wait the specified amount of time before ending the ability.")]
        [SerializeField] protected AnimationEventTrigger m_StopEvent = new AnimationEventTrigger(false, 0.2f);

        public AnimationEventTrigger StopEvent { get { return m_StopEvent; } set { m_StopEvent.CopyFrom(value); } }

        private int m_ResponseID;

        public override int AbilityIntData { get { return m_ResponseID; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_StopEvent.RegisterUnregisterAnimationEvent(true, m_GameObject, "OnAnimatorImpactKnockBackComplete", OnComplete);
        }

        /// <summary>
        /// The character has been counter attacked. Play a response animation.
        /// </summary>
        /// <param name="id">The ID of the counter attack.</param>
        public void StartKnockBackResponse(int id)
        {
            m_ResponseID = id;
            StartAbility();
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_StopEvent.WaitForEvent();
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return !(startingAbility is Ragdoll);
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (activeAbility is Items.Use) {
                return true;
            }
            return base.ShouldStopActiveAbility(activeAbility);
        }

        /// <summary>
        /// The animation is done playing - stop the ability.
        /// </summary>
        private void OnComplete()
        {
            m_StopEvent.CancelWaitForEvent();

            StopAbility();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_StopEvent.RegisterUnregisterAnimationEvent(false, m_GameObject, "OnAnimatorImpactKnockBackComplete", OnComplete);
        }
    }
}