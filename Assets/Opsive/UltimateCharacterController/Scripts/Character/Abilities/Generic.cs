/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The Generic ability allows for new animations without having to explicitly code a new ability. The ability will end after a specified duration or 
    /// the OnAnimatorGenericAbilityComplete event is sent.
    /// </summary>
    [AllowDuplicateTypes]
    [DefaultAbilityIndex(10000)]
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Action")]
    public class Generic : Ability
    {
        [Tooltip("The value of the Ability Int Data parameter.")]
        [SerializeField] protected int m_AbilityIntDataValue = -1;
        [Tooltip("Specifies if the ability should stop when the OnAnimatorGenericAbilityComplete event is received or wait the specified amount of time before ending the ability.")]
        [SerializeField] protected AnimationEventTrigger m_StopEvent = new AnimationEventTrigger(false, 0.5f);

        public override int AbilityIntData { get => m_AbilityIntDataValue; set { m_AbilityIntDataValue = value; if (IsActive) { SetAbilityIntDataParameter(value); } }}
        public AnimationEventTrigger StopEvent { get => m_StopEvent; set => m_StopEvent.CopyFrom(value); }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_StopEvent.RegisterUnregisterAnimationEvent(true, m_GameObject, "OnAnimatorGenericAbilityComplete", OnComplete);
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

            m_StopEvent.RegisterUnregisterAnimationEvent(false, m_GameObject, "OnAnimatorGenericAbilityComplete", OnComplete);
        }
    }
}