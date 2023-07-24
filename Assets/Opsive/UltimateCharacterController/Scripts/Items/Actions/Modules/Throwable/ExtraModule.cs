/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for Extra module for throwable actions. Used to add functionality to throwable actions.
    /// </summary>
    [Serializable]
    public abstract class ThrowableExtraModule : ThrowableActionModule
    {
        
    }
    
    /// <summary>
    /// Visualize the trajectory of the throw using this module.
    /// </summary>
    [Serializable]
    public class ThrowableVisualizeTrajectory : ThrowableExtraModule,
        IModuleOnAim, IModuleStartItemUse
    {
        [Tooltip("Should the item's trajectory be shown when the character aims?")]
        [SerializeField] protected bool m_ShowTrajectoryOnAim = true;
        
        private TrajectoryObject m_TrajectoryObject;
        protected bool m_Aiming;

        public ILookSource LookSource => ThrowableAction.LookSource;
        
        public bool ShowTrajectoryOnAim { get { return m_ShowTrajectoryOnAim; } set { m_ShowTrajectoryOnAim = value; } }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_TrajectoryObject = CharacterItem.GetComponent<TrajectoryObject>();
            
            if (m_TrajectoryObject == null) {
                Debug.LogError($"Error: A TrajectoryObject must be added to the {GameObject.name} GameObject in order for the trajectory to be shown.");
            }
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            if (register) {
                m_CharacterItemAction.OnFixedUpdateE += OnLateUpdate;
            } else {
                m_CharacterItemAction.OnDrawGizmosE -= OnLateUpdate;
            }
        }

        /// <summary>
        /// Late update called from the character item action late update.
        /// </summary>
        public void OnLateUpdate()
        {
            if ((m_Aiming && m_ShowTrajectoryOnAim) == false) {
                return;
            }
            
            var isThrowing = ThrowableAction.IsThrowing;
            var isReequipping = ThrowableAction.IsReequipping;

            var canShow = !isThrowing && !isReequipping && m_TrajectoryObject != null;

            if (m_CharacterItemAction.IsDebugging) {
                m_CharacterItemAction.DebugLogger.SetInfo("Throwable/VisualizeTrajectory/CanShow",
                    canShow+ $" Why? (isThrowing:{isThrowing}, isReequiping:{isReequipping}, TrajectoryObject:{m_TrajectoryObject})"); 
            }

            if (!canShow) {
                return;
            }

            var throwPreviewData = ThrowableAction.GetThrowPreviewData();
            var trajectoryTransform = throwPreviewData.TrajectoryTransform;
            var lookDirection = LookSource.LookDirection(trajectoryTransform.TransformPoint(throwPreviewData.TrajectoryOffset), false, throwPreviewData.ImpactLayers, true, true);
            var velocity = MathUtility.TransformDirection(throwPreviewData.Velocity, Quaternion.LookRotation(lookDirection, CharacterTransform.up));
            
            // Prevent the item from being thrown behind the character. This can happen if the character is looking straight up and there is a positive
            // y velocity. Gravity will cause the thrown object to go in the opposite direction.
            if (Vector3.Dot(velocity.normalized, CharacterTransform.forward) < 0) {
                velocity = CharacterTransform.up * velocity.magnitude;
            }
            m_TrajectoryObject.SimulateTrajectory(Character,
                trajectoryTransform.TransformPoint(throwPreviewData.TrajectoryOffset) + CharacterLocomotion.DesiredMovement, 
                Quaternion.identity,
                velocity + (CharacterTransform.forward * CharacterLocomotion.LocalVelocity.z), Vector3.zero);
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        public void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) {
                return;
            }
            
            m_Aiming = aim;
            
            if (!m_Aiming && m_ShowTrajectoryOnAim && m_TrajectoryObject != null) {
                m_TrajectoryObject.ClearTrajectory();
            }
        }

        /// <summary>
        /// The Item was Unequipped.
        /// </summary>
        public override void Unequip()
        {
            base.Unequip();
            if (m_ShowTrajectoryOnAim && m_TrajectoryObject != null) {
                m_TrajectoryObject.ClearTrajectory();
            }
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public void StartItemUse(Use useAbility)
        {
            if (m_Aiming && m_ShowTrajectoryOnAim && m_TrajectoryObject != null) {
                m_TrajectoryObject.ClearTrajectory();
            }
        }
    }

    /// <summary>
    /// A module used to remove the pin of a grenade throwable projectile.
    /// </summary>
    [Serializable]
    public class ThrowableGrenade : ThrowableExtraModule, IModuleStartItemUse
    {
        [Tooltip("Is the pin removal animated?")]
        [SerializeField] protected bool m_AnimatePinRemoval = true;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemRemovePin animation event or wait for the specified duration before removing the pin from the object.")]
        [SerializeField] protected AnimationEventTrigger m_RemovePinEvent = new AnimationEventTrigger(true, 0.4f);
        [Tooltip("The Transform that the pin attaches to.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_PinAttachmentLocation;

        public bool AnimatePinRemoval { get { return m_AnimatePinRemoval; } set { m_AnimatePinRemoval = value; } }
        public AnimationEventTrigger RemovePinEvent { get { return m_RemovePinEvent; } set { m_RemovePinEvent.CopyFrom(value); } }

        private Grenade m_InstantiatedGrenade;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_PinAttachmentLocation.Initialize(CharacterItemAction);
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            m_RemovePinEvent.RegisterUnregisterAnimationEvent(register, Character,"OnAnimatorItemRemovePin", RemovePin);
        }

        /// <summary>
        /// Get the location of the pin transform.
        /// </summary>
        /// <returns></returns>
        public virtual Transform GetPinAttachmentLocation()
        {
            return m_PinAttachmentLocation.GetValue();
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public void StartItemUse(Use itemAbility)
        {
            // An Animator Audio State Set may prevent the item from being used.
            if (!ThrowableAction.IsItemInUse()) {
                return;
            }

            // Grenades can be cooked (and explode while still in the character's hands).
            m_InstantiatedGrenade = ThrowableAction.InstantiatedTrajectoryObject as Grenade;
            m_InstantiatedGrenade.StartCooking(Character, ThrowableAction);

            // If a pin is specified then it can optionally be removed when the grenade is being thrown.
            if (m_InstantiatedGrenade.Pin != null) {
                if (m_AnimatePinRemoval && ThrowableAction.ThrowableObjectIsVisible) {
                    m_RemovePinEvent.WaitForEvent(false);
                }
            }
        }

        /// <summary>
        /// The pin has been removed from the grenade.
        /// </summary>
        private void RemovePin()
        {
            m_RemovePinEvent.CancelWaitForEvent();

            // Attach the pin to the attachment transform. Attach both first and third person in case there is a perspective switch.
            m_InstantiatedGrenade.DetachAttachPin(GetPinAttachmentLocation());
        }
    }
    
    /// <summary>
    /// A Character Item Action Module used to define what information to show in the Slot Item Monitor.
    /// </summary>
    [Serializable]
    public class SlotItemMonitorModule : ThrowableExtraModule, IModuleSlotItemMonitor
    {
        [Tooltip("The priority of this module over other Item Monitor modules.")]
        [SerializeField] protected int m_Priority;
        [Tooltip("Show the loaded amount of items?")]
        [SerializeField] protected bool m_Show = true;

        public int Priority { get => m_Priority; set => m_Priority = value; }
        public bool Show { get => m_Show; set => m_Show = value; }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            var target = Character;
            Shared.Events.EventHandler.RegisterUnregisterEvent<CharacterItem, ThrowableAmmoModule>(register, target, "OnThrowableItemAmmoChange", OnThrowableItemAmmoChange);
            RefreshSlotItemMonitor();
        }

        /// <summary>
        /// On ammo changed.
        /// </summary>
        /// <param name="characterItem">The character item.</param>
        /// <param name="throwableAmmoModule">The ammo module.</param>
        protected virtual void OnThrowableItemAmmoChange(CharacterItem characterItem, ThrowableAmmoModule throwableAmmoModule)
        {
            RefreshSlotItemMonitor();
        }

        /// <summary>
        /// Refresh the slot item monitor using an event.
        /// </summary>
        public void RefreshSlotItemMonitor()
        {
            Shared.Events.EventHandler.ExecuteEvent<CharacterItem>(Character, "OnRefreshSlotItemMonitor", CharacterItem);
        }

        /// <summary>
        /// Try get the loaded number of ammo in the clip.
        /// </summary>
        /// <param name="loadedCount">The loaded count in the clip.</param>
        /// <returns>True if the loaded count exists.</returns>
        public bool TryGetLoadedCount(out string loadedCount)
        {
            var remaining = ThrowableAction.MainAmmoModule.GetAmmoRemainingCount();

            loadedCount = remaining == int.MaxValue ? "∞" :remaining.ToString();
            return m_Show;
        }

        /// <summary>
        /// Try get the unloaded count.
        /// </summary>
        /// <param name="unloadedCount">The unloaded count.</param>
        /// <returns>True if there is an unloaded count.</returns>
        public bool TryGetUnLoadedCount(out string unloadedCount)
        {
            unloadedCount = null;
            return false;
        }

        /// <summary>
        /// Try get the item icon.
        /// </summary>
        /// <param name="itemIcon">The item icon.</param>
        /// <returns>True if the item icon exists.</returns>
        public bool TryGetItemIcon(out Sprite itemIcon)
        {
            itemIcon = null;
            return false;
        }
    }
}