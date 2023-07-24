/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.Shared.Audio;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The reloader defines the clip size and how to reload.
    /// </summary>
    [Serializable]
    public abstract class ShootableReloaderModule : ShootableActionModule, IReloadableItemModule, IModuleCanStartUseItem
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public abstract bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState);

        /// <summary>
        /// Is the shootable action reloading?
        /// </summary>
        /// <returns>True if it is currently reloading the item.</returns>
        public abstract bool IsReloading();

        /// <summary>
        /// Has the shootable action reloaded the clip?
        /// </summary>
        /// <returns>True if the item has added the ammo to the clip.</returns>
        public abstract bool HasReloaded();

        /// <summary>
        /// Start reloading the item.
        /// </summary>
        public abstract void StartItemReload();

        /// <summary>
        /// Can the item reload?
        /// </summary>
        /// <param name="checkEquipStatus">Check if the item is equipped?</param>
        /// <returns>True if it can be reloaded.</returns>
        public abstract bool CanReloadItem(bool checkEquipStatus);

        /// <summary>
        /// Reload the item.
        /// </summary>
        /// <param name="fullClip">Reload the full clip or just one ammo?</param>
        public abstract void ReloadItem(bool fullClip);

        /// <summary>
        /// The item reload has completed.
        /// </summary>
        /// <param name="success">Did it complete successfully?</param>
        /// <param name="immediateReload">Was it an immediate reload?</param>
        public abstract void ItemReloadComplete(bool success, bool immediateReload);

        /// <summary>
        /// Should the item be reloaded?
        /// </summary>
        /// <param name="ammoItemIdentifier">The ItemIdentifier that is being reloaded.</param>
        /// <param name="fromPickup">Was the item identifier added from pickup?</param>
        /// <returns>True if the item should reload.</returns>
        public abstract bool ShouldReload(IItemIdentifier ammoItemIdentifier, bool fromPickup);

        /// <summary>
        /// Get the reload item substate index used to animate the item.
        /// </summary>
        /// <returns>The reload item substate index.</returns>
        public abstract void GetReloadItemSubstateIndex(ItemSubstateIndexStreamData streamData);
    }

    /// <summary>
    /// A generic module used to reload a shootable weapon.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "SimpleReloader", sourceNamespace: "Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable")]
    public class GenericReloader : ShootableReloaderModule, IModuleItemUseComplete
    {
        /// <summary>
        /// Specifies how the clip should be reloaded.
        /// </summary>
        public enum ReloadClipType
        {
            Full,   // Reload the entire clip.
            Single  // Reload a single bullet.
        }

        [SerializeField] protected ItemSubstateIndexData m_SubstateIndexData = new ItemSubstateIndexData(0, 100);
        [Tooltip("Specifies when the item should be automatically reloaded.")]
        [SerializeField] protected Reload.AutoReloadType m_AutoReload = Reload.AutoReloadType.Pickup | Reload.AutoReloadType.Empty;
        [Tooltip("Specifies how the clip should be reloaded.")]
        [SerializeField] protected ReloadClipType m_ReloadType = ReloadClipType.Full;
        [Tooltip("Can the camera zoom during a reload?")]
        [SerializeField] protected bool m_ReloadCanCameraZoom = true;
        [Tooltip("Should the crosshairs spread during a recoil?")]
        [SerializeField] protected bool m_ReloadCrosshairsSpread = true;
        [Tooltip("Specifies the animator and audio state from a reload.")]
        [SerializeField] protected AnimatorAudioStateSet m_ReloadAnimatorAudioStateSet = new AnimatorAudioStateSet();
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReload animation event or wait for the specified duration before reloading.")]
        [SerializeField] protected AnimationSlotEventTrigger m_ReloadEvent = new AnimationSlotEventTrigger(true, 0);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReloadComplete animation event or wait for the specified duration before completing the reload.")]
        [SerializeField] protected AnimationSlotEventTrigger m_ReloadCompleteEvent = new AnimationSlotEventTrigger(true, 0);
        [Tooltip("The clip that should be played after the item has finished reloading.")]
        [SerializeField] protected AudioClipSet m_ReloadCompleteAudioClipSet;
        [Tooltip("Should the reload clip be used as the drop clip?")]
        [SerializeField] protected bool m_InstantiateReloadClip;
        [Tooltip("Should the reload clip be reset to its parent position and rotation when detached?")]
        [SerializeField] protected bool m_ResetClipTransformOnDetach;
        [Tooltip("The animation event used to now when to make the clip reapear when faking the drop clip.")]
        [SerializeField] protected AnimationSlotEventTrigger m_ReactivateClipEvent = new AnimationSlotEventTrigger(true, 0.1f);
        [Tooltip("Should the weapon clip be detached and attached when reloaded?")]
        [SerializeField] protected bool m_ReloadDetachAttachClip;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReloadDetachClip animation event or wait for the specified duration before detaching the clip from the weapon.")]
        [SerializeField] protected AnimationEventTrigger m_ReloadDetachClipEvent = new AnimationEventTrigger(true, 0.4f);
        [Tooltip("The prefab that is dropped when the character is reloading.")]
        [SerializeField] protected GameObject m_ReloadDropClip;
        [Tooltip("The amount of time after the clip has been removed to change the layer.")]
        [SerializeField] protected float m_ReloadClipLayerChangeDelay = 0.1f;
        [Tooltip("The layer that the clip object should change to after being reloaded.")]
        [Shared.Utility.Layer] [SerializeField] protected int m_ReloadClipTargetLayer = LayerManager.VisualEffect;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReloadDropClip animation event or wait for the specified duration before dropping the clip from the weapon.")]
        [SerializeField] protected AnimationEventTrigger m_ReloadDropClipEvent = new AnimationEventTrigger(true, 0.1f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReloadAttachClip animation event or wait for the specified duration before attaching the clip back to the weapon.")]
        [SerializeField] protected AnimationEventTrigger m_ReloadAttachClipEvent = new AnimationEventTrigger(true, 0.4f);
        [Tooltip("The reoloadable clip visual to move when reloading.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_ReloadableClip;
        [Tooltip("The reoloadable clip attachement position.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_ReloadableClipAttachment;

        private bool m_Reloading;
        protected bool m_Reloaded;

        private bool m_ReloadInitialized;

        private bool m_DroppedClip;
        private Transform m_FirstPersonReloadableClipParent;
        private Vector3 m_FirstPersonReloadableClipLocalPosition;
        private Quaternion m_FirstPersonReloadableClipLocalRotation;
        private Transform m_ThirdPersonReloadableClipParent;
        private Vector3 m_ThirdPersonReloadableClipLocalPosition;
        private Quaternion m_ThirdPersonReloadableClipLocalRotation;
        private GameObject m_FirstPersonReloadAddClip;
        private GameObject m_ThirdPersonReloadAddClip;

        public ItemSubstateIndexData SubstateIndexData { get => m_SubstateIndexData; set => m_SubstateIndexData = value; }
        public bool ReloadCanCameraZoom { get => m_ReloadCanCameraZoom; set => m_ReloadCanCameraZoom = value; }
        public ItemPerspectiveIDObjectProperty<Transform> ReloadableClip { get => m_ReloadableClip; set => m_ReloadableClip = value; }
        public ItemPerspectiveIDObjectProperty<Transform> ReloadableClipAttachment { get => m_ReloadableClipAttachment; set => m_ReloadableClipAttachment = value; }
        public Reload.AutoReloadType AutoReload { get { return m_AutoReload; } set { m_AutoReload = value; } }
        public ReloadClipType ReloadType { get { return m_ReloadType; } set { m_ReloadType = value; } }
        public bool CanCameraZoom { get { return m_ReloadCanCameraZoom; } set { m_ReloadCanCameraZoom = value; } }
        public bool ReloadCrosshairsSpread { get { return m_ReloadCrosshairsSpread; } set { m_ReloadCrosshairsSpread = value; } }
        public AnimatorAudioStateSet ReloadAnimatorAudioStateSet { get { return m_ReloadAnimatorAudioStateSet; } set { m_ReloadAnimatorAudioStateSet = value; } }
        public AnimationSlotEventTrigger ReloadEvent { get { return m_ReloadEvent; } set { m_ReloadEvent.CopyFrom(value); } }
        public AnimationSlotEventTrigger ReloadCompleteEvent { get { return m_ReloadCompleteEvent; } set { m_ReloadCompleteEvent.CopyFrom(value); } }
        public AudioClipSet ReloadCompleteAudioClipSet { get { return m_ReloadCompleteAudioClipSet; } set { m_ReloadCompleteAudioClipSet = value; } }
        public bool InstantiateReloadClip { get { return m_InstantiateReloadClip; } set { m_InstantiateReloadClip = value; } }
        public AnimationSlotEventTrigger ReactivateClipEvent { get { return m_ReactivateClipEvent; } set { m_ReactivateClipEvent.CopyFrom(value); } }
        public bool ReloadDetachAttachClip
        {
            get { return m_ReloadDetachAttachClip; }
            set {
                if (m_Reloading && m_ReloadDetachAttachClip && !value) {
#if FIRST_PERSON_CONTROLLER
                    DetachAttachClip(false, true);
#endif
                    DetachAttachClip(false, false);
                }
                m_ReloadDetachAttachClip = value;
            }
        }
        public AnimationEventTrigger ReloadDetachClipEvent { get { return m_ReloadDetachClipEvent; } set { m_ReloadDetachClipEvent.CopyFrom(value); } }
        public GameObject ReloadDropClip { get { return m_ReloadDropClip; } set { m_ReloadDropClip = value; } }
        public float ReloadClipLayerChangeDelay { get { return m_ReloadClipLayerChangeDelay; } set { m_ReloadClipLayerChangeDelay = value; } }
        public int ReloadClipTargetLayer { get { return m_ReloadClipTargetLayer; } set { m_ReloadClipTargetLayer = value; } }
        public AnimationEventTrigger ReloadDropClipEvent { get { return m_ReloadDropClipEvent; } set { m_ReloadDropClipEvent.CopyFrom(value); } }
        public AnimationEventTrigger ReloadAttachClipEvent { get { return m_ReloadAttachClipEvent; } set { m_ReloadAttachClipEvent.CopyFrom(value); } }

        /// <summary>
        /// Should the item reload when the item is equipped?
        /// </summary>
        public override void OnAllModulesPreInitialized()
        {
            if (m_AllModulesPreInitialized) {
                return;
            }

            base.OnAllModulesPreInitialized();

            if ((m_AutoReload & Reload.AutoReloadType.Equip) != 0) {
                ShootableAction.ReloadClip(true, true);
            }
        }

        /// <summary>
        /// Should the item reload when the item is equipped?
        /// </summary>
        public override void Equip()
        {
            base.Equip();

            // The first time the item is equipped, not all the modules might be initialized.
            if (m_AllModulesPreInitialized == false) {
                return;
            }

            if ((m_AutoReload & Reload.AutoReloadType.Equip) != 0) {
                ShootableAction.ReloadClip(true, true);
            }
        }

        /// <summary>
        /// Get the reloadable clip location transform.
        /// </summary>
        /// <param name="firstPerson">In first person perspective?</param>
        /// <returns>The reloadable clip transform.</returns>
        public virtual Transform GetReloadableClip(bool firstPerson)
        {
            return m_ReloadableClip.GetValue(firstPerson);
        }

        /// <summary>
        /// Get the reloadable clip attachment location.
        /// </summary>
        /// <param name="firstPerson">In first person perspective?</param>
        /// <returns>The reloadable clip attachment.</returns>
        public virtual Transform GetReloadableClipAttachment(bool firstPerson)
        {
            return m_ReloadableClipAttachment.GetValue(firstPerson);
        }

        /// <summary>
        /// Get the reloadable clip location transform.
        /// </summary>
        /// <returns>The reloadable clip transform.</returns>
        public virtual Transform GetReloadableClip()
        {
            return m_ReloadableClip.GetValue();
        }

        /// <summary>
        /// Get the reloadable clip attachment location.
        /// </summary>
        /// <returns>The reloadable clip attachment.</returns>
        public virtual Transform GetReloadableClipAttachment()
        {
            return m_ReloadableClipAttachment.GetValue();
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            m_ReloadAnimatorAudioStateSet.Awake(CharacterItem, CharacterLocomotion);
            m_ReloadableClip.Initialize(m_CharacterItemAction);
            m_ReloadableClipAttachment.Initialize(m_CharacterItemAction);
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            var eventTarget = Character;

            m_ReloadEvent.RegisterUnregisterEvent(register, eventTarget, "OnAnimatorItemReload", SlotID, HandleReloadItemAnimationSlotEvent);
            m_ReloadCompleteEvent.RegisterUnregisterEvent(register, eventTarget, "OnAnimatorItemReloadComplete", SlotID, HandleReloadItemCompleteAnimationSlotEvent);
            m_ReloadDetachClipEvent.RegisterUnregisterAnimationEvent(register, eventTarget, "OnAnimatorItemReloadDetachClip", DetachClip);
            m_ReloadAttachClipEvent.RegisterUnregisterAnimationEvent(register, eventTarget, "OnAnimatorItemReloadAttachClip", AttachClip);
            m_ReloadDropClipEvent.RegisterUnregisterAnimationEvent(register, eventTarget, "OnAnimatorItemReloadDropClip", DropClip);
            m_ReactivateClipEvent.RegisterUnregisterAnimationEvent(register, eventTarget, "OnAnimatorItemReactivateClip", ReactivateClip);
        }

        /// <summary>
        /// Handle the reload animation event.
        /// </summary>
        protected void HandleReloadItemAnimationSlotEvent()
        {
            m_ReloadEvent.CancelWaitForEvent();
            NotifyReload();
        }

        /// <summary>
        /// Handle the reload item complete animation event.
        /// </summary>
        protected void HandleReloadItemCompleteAnimationSlotEvent()
        {
            m_ReloadCompleteEvent.CancelWaitForEvent();
            NotifyReloadComplete();
        }

        /// <summary>
        /// Notify that the item is reloading.
        /// </summary>
        public void NotifyReload()
        {
            EventHandler.ExecuteEvent<IReloadableItem>(Character, "OnItemReload", ShootableAction);
        }

        /// <summary>
        /// Notify that the item finished reloading.
        /// </summary>
        public void NotifyReloadComplete()
        {
            EventHandler.ExecuteEvent<IReloadableItem>(Character, "OnItemReloadComplete", ShootableAction);
        }

        /// <summary>
        /// Is the shootable action reloading.
        /// </summary>
        /// <returns>True if it is currently reloading the item.</returns>
        public override bool IsReloading()
        {
            return m_Reloading;
        }

        /// <summary>
        /// Has the shootable action reloaded the clip.
        /// </summary>
        /// <returns>True if the item has added the ammo to the clip.</returns>
        public override bool HasReloaded()
        {
            return m_Reloaded;
        }

        /// <summary>
        /// The item will be equipped.
        /// </summary>
        public override void WillEquip()
        {
            base.WillEquip();

            // When the weapon is equipped for the first time it needs to reload so it doesn't need to be reloaded manually.
            if (!m_ReloadInitialized) {
                var inventory = Inventory;
                var itemSetManager = inventory.gameObject.GetCachedComponent<ItemSetManagerBase>();
                var ammoModule = ShootableAction.MainAmmoModule;

                // If the ammo module is null we cannot reload until it becomes set.
                if (ammoModule == null) {
                    return;
                }

                var isAmmoShared = ammoModule.IsAmmoShared();

                // Consumable ItemIdentifiers that are shared may need to have their amount redistributed on the first equip. This will allow all items to have an equal
                // amount on the first run.
                if (isAmmoShared && itemSetManager != null) {
                    for (int i = 0; i < inventory.SlotCount; ++i) {
                        // Use the ItemSetManager to determine which ItemIdentifier will be equipped. The current slot cannot be used from the inventory because not all
                        // items may have been equipped yet.
                        var itemIdentifier = itemSetManager.GetNextItemIdentifier(i, out var groupIndex);
                        if (itemIdentifier == null) {
                            continue;
                        }

                        // Skip if the item is null or matches the current item. The current item can't give ammo to itself.
                        var item = inventory.GetCharacterItem(itemIdentifier, i);
                        if (item == null || item == CharacterItem) {
                            continue;
                        }

                        // Find any ShootableWeapons that may be sharing the same Consumable ItemIdentifier.
                        var itemActions = item.ItemActions;
                        for (int j = 0; j < itemActions.Length; ++j) {
                            var otherShootableAction = itemActions[j] as ShootableAction;
                            // The ShootableWeapon has to share the Consumable ItemIdentifier. The other shootable weapon must have ammo in the clip otherwise it isn't of any use.
                            if (otherShootableAction == null || ammoModule.DoesAmmoSharedMatch(otherShootableAction.MainAmmoModule) == false ||
                                    otherShootableAction.ClipRemainingCount == 0) {
                                continue;
                            }

                            // The Consumable ItemIdentifier doesn't need to be shared if there is plenty of ammo for all weapons.
                            var totalInventoryAmount = ammoModule.GetAmmoRemainingCount();
                            if (otherShootableAction.ClipSize + ShootableAction.ClipSize < totalInventoryAmount) {
                                continue;
                            }

                            // The ShootableWeapon needs to share the ammo. Take half of the ammo and return it to the inventory. When the current item is reloaded it will 
                            // then take the Consumable ItemIdentifier that was returned to the inventory.
                            var totalConsumable = otherShootableAction.ClipRemainingCount + ShootableAction.ClipRemainingCount + totalInventoryAmount;
                            var returnConsumable = Mathf.FloorToInt(otherShootableAction.ClipRemainingCount - (totalConsumable - (totalConsumable / 2)));
                            if (returnConsumable > 0) {
                                //Remove the ammo from the clip and add it back to the ammo pool.
                                otherShootableAction.MainClipModule.RemoveAmmo(returnConsumable);
                                otherShootableAction.MainAmmoModule.AdjustAmmoAmount(returnConsumable);
                            }
                        }
                    }
                }

                if (CanReloadItem(false)) {
                    ReloadItem(true);
                }
                m_ReloadInitialized = true;
            }
        }

        /// <summary>
        /// Should the item be reloaded? An IReloadableItem reference will be returned if the item can be reloaded.
        /// </summary>
        /// <param name="ammoItemIdentifier">The ItemIdentifier that is being reloaded.</param>
        /// <param name="fromPickup">Was the item identifier added from pickup?</param>
        /// <returns>True if the item should reload.</returns>
        public override bool ShouldReload(IItemIdentifier ammoItemIdentifier, bool fromPickup)
        {
            if (!m_Initialized || ShootableAction.MainAmmoModule == null || !ShootableAction.MainAmmoModule.HasAmmoRemaining()) { return false; }
            
            if (fromPickup) {
                // The Ammo Item Definition has to match.
                if ( !(ammoItemIdentifier == null && ShootableAction.MainAmmoModule.AmmoItemDefinition == null )
                     && ShootableAction.MainAmmoModule.AmmoItemDefinition != ammoItemIdentifier?.GetItemDefinition()) {
                    return false;
                }
            }

            var autoReload = false;
            var clipIsEmpty = ShootableAction.ClipRemainingCount == 0;
            if ((AutoReload & Reload.AutoReloadType.Empty) != 0 && clipIsEmpty) {
                // The item is empty.
                autoReload = true;
            } else if ((AutoReload & Reload.AutoReloadType.Pickup) != 0 && fromPickup) {
                // The item was just picked up for the first time.
                autoReload = true;
            }

            // Don't automatically reload if the item says that it shouldn't.
            if (!autoReload) {
                return false;
            }

            // Don't reload if the reloadable item can't reload.
            if (!CanReloadItem(true)) {
                return false;
            }

            // Reload.
            return true;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            // The weapon can't be used if it is reloading and out of ammo.
            if (m_Reloading && (m_ReloadType == ReloadClipType.Full || ShootableAction.ClipRemainingCount == 0)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Can the item be reloaded?
        /// </summary>
        /// <param name="sharedCheck">Should extra checks be performed ensuring the reload is valid for shared items?</param>
        /// <returns>True if the item can be reloaded.</returns>
        public override bool CanReloadItem(bool sharedCheck)
        {
            var clipModule = ShootableAction.MainClipModule;

            if (clipModule == null || clipModule.IsClipFull()) {
                return false;
            }

            var ammoModule = ShootableAction.MainAmmoModule;
            if (ammoModule == null || ammoModule.HasAmmoRemaining() == false) {
                return false;
            }

            // Don't reload if the consumed item type is shared and the item isn't equipped. This will prevent an unequipped shootable weapon from taking ammo.
            if (sharedCheck && ammoModule.IsAmmoShared() && Inventory.GetActiveCharacterItem(CharacterItem.SlotID) != CharacterItem) {
                return false;
            }

            // Can't reload if the weapon hasn't been added to the inventory yet.
            if (Inventory.GetItemIdentifierAmount(CharacterItem.ItemIdentifier, true) == 0) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts to reload the item.
        /// </summary>
        public override void StartItemReload()
        {
            m_Reloaded = false;
            ReloadEvent.WaitForEvent(true);

            m_Reloading = true;
            m_DroppedClip = false;

            var detachClip = false;
#if FIRST_PERSON_CONTROLLER
            detachClip = GetReloadableClip(true) != null;
#endif
            if (!detachClip) {
                detachClip = GetReloadableClip(false) != null;
            }

            if (detachClip) {
                m_ReloadDetachClipEvent.WaitForEvent(false);
            }

            // The reload AnimatorAudioState is starting.
            m_ReloadAnimatorAudioStateSet.StartStopStateSelection(true);
            m_ReloadAnimatorAudioStateSet.NextState();

            // Optionally play a reload sound based upon the reload animation.
            if (m_ReloadType == ReloadClipType.Full) {
                m_ReloadAnimatorAudioStateSet.PlayAudioClip(CharacterItem.GetVisibleObject());
            }

            // The crosshairs should be set to the max spread.
            if (m_ReloadCrosshairsSpread) {
                Shared.Events.EventHandler.ExecuteEvent(Character, "OnAddCrosshairsSpread", true, false);
            }

            // Using Force Change true.
            // This makes sure the weapon isn't stuck within the reload animation if the transitions are set properly.
            // If your item gets stuck while spamming the button, add a transition using the SlotXItemStatChange Trigger.
            UpdateItemAbilityAnimatorParameters(true);

            Shared.Events.EventHandler.ExecuteEvent(Character, "OnStartReload");
        }

        /// <summary>
        /// The clip has been detached from the weapon.
        /// </summary>
        protected virtual void DetachClip()
        {
            if (!m_Reloading) {
                return;
            }

            // Attach the clip to the attachment transform. Attach both first and third person in case there is a perspective switch.
#if FIRST_PERSON_CONTROLLER
            DetachAttachClip(true, true);
#endif
            DetachAttachClip(true, false);

            // Clean up from the detach event.
            m_ReloadDetachClipEvent.CancelWaitForEvent();
            m_ReloadAttachClipEvent.CancelWaitForEvent();

            // The clip can actually be dropped.
            if (m_ReloadDropClip != null) {
                m_ReloadDropClipEvent.WaitForEvent(false);
            }

            // Schedule the event which will attach clip back to the weapon.
            m_ReloadAttachClipEvent.WaitForEvent(false);
        }

        /// <summary>
        /// Detaches or attaches the clip from the weapon.
        /// </summary>
        /// <param name="detach">Should the clip be detached? If false the clip will be attached.</param>
        /// <param name="firstPerson">Is the first person perspective being affected?</param>
        private void DetachAttachClip(bool detach, bool firstPerson)
        {
            // Don't do anything if the clip doesn't exist or the clip shouldn't be detached.
            var clip = GetReloadableClip(firstPerson);
            if (clip == null || (!m_ReloadDetachAttachClip && detach)) {
                return;
            }

            // Some people might use a skinned mesh by mistake.
            if (clip.gameObject.GetCachedComponent<SkinnedMeshRenderer>() != null) {
                Debug.LogWarning("The ReloadableClip Transform cannot be a skinnedMeshRenderer. Please replace it by a MeshRenderer instead.", clip);
            }

            // If detaching then set the clip's parent from the weapon to the attachment object. Attaching will set the clip's parent from the attachment
            // object back to the weapon.
            if (detach) {
                if (firstPerson) {
                    if (m_FirstPersonReloadableClipParent == null) {
                        m_FirstPersonReloadableClipParent = clip.parent;
                        m_FirstPersonReloadableClipLocalPosition = clip.localPosition;
                        m_FirstPersonReloadableClipLocalRotation = clip.localRotation;
                    }
                } else {
                    if (m_ThirdPersonReloadableClipParent == null) {
                        m_ThirdPersonReloadableClipParent = clip.parent;
                        m_ThirdPersonReloadableClipLocalPosition = clip.localPosition;
                        m_ThirdPersonReloadableClipLocalRotation = clip.localRotation;
                    }
                }
                clip.parent = GetReloadableClipAttachment(firstPerson);
                if (m_ResetClipTransformOnDetach) {
                    clip.localPosition = Vector3.zero;
                    clip.localRotation = Quaternion.identity;
                }
            } else {
                if (firstPerson) {
                    if (m_FirstPersonReloadableClipParent != null) {
                        clip.parent = m_FirstPersonReloadableClipParent;
                        clip.localPosition = m_FirstPersonReloadableClipLocalPosition;
                        clip.localRotation = m_FirstPersonReloadableClipLocalRotation;
                        m_FirstPersonReloadableClipParent = null;
                    }
                } else {
                    if (m_ThirdPersonReloadableClipParent != null) {
                        clip.parent = m_ThirdPersonReloadableClipParent;
                        clip.localPosition = m_ThirdPersonReloadableClipLocalPosition;
                        clip.localRotation = m_ThirdPersonReloadableClipLocalRotation;
                        m_ThirdPersonReloadableClipParent = null;
                    }
                }
            }
        }

        /// <summary>
        /// Drops the weapon's clip.
        /// </summary>
        private void DropClip()
        {
            if (!m_Reloading || m_ReloadDropClip == null || m_DroppedClip) {
                return;
            }

            // Hide the existing clip and drop a new prefab.
            var clip = GetReloadableClip();
            if (clip == null) {
                return;
            }

            if (m_ReloadDetachAttachClip) {
                clip.gameObject.SetActive(false);
                ReactivateClipEvent.WaitForEvent(true);
            }

            var dropClip = ObjectPoolBase.Instantiate(m_ReloadDropClip, clip.position, clip.rotation);
            // The first person perspective requires the clip to be on the overlay layer so the fingers won't render in front of the clip.
            dropClip.transform.SetLayerRecursively(CharacterLocomotion.FirstPersonPerspective ? LayerManager.Overlay : clip.gameObject.layer);
            Scheduler.Schedule(m_ReloadClipLayerChangeDelay, UpdateDropClipLayer, dropClip);

            // If the clip has a trajectory object attached then it needs to be initialized.
            var trajectoryClipObject = dropClip.GetCachedComponent<TrajectoryObject>();
            if (trajectoryClipObject != null) {
                trajectoryClipObject.Initialize(Vector3.zero, Vector3.zero, Character);
            }

            // Cleanup from the event.
            m_ReloadDropClipEvent.CancelWaitForEvent();
            m_DroppedClip = true;
        }

        /// <summary>
        /// Reactivate the clip after is was deactivated.
        /// </summary>
        private void ReactivateClip()
        {
            var clip = GetReloadableClip();
            if (clip == null) {
                return;
            }
            if (!m_InstantiateReloadClip) {
                clip.gameObject.SetActive(true);
            }
            m_ReactivateClipEvent.CancelWaitForEvent();
        }

        /// <summary>
        /// Updates the dropped clip layer.
        /// </summary>
        /// <param name="dropClip">The object that should have its layer updated.</param>
        private void UpdateDropClipLayer(GameObject dropClip)
        {
            dropClip.transform.SetLayerRecursively(m_ReloadClipTargetLayer);
        }

        /// <summary>
        /// The clip has been detached form the weapon.
        /// </summary>
        private void AttachClip()
        {
            if (!m_Reloading) {
                return;
            }

            // Attach the clip back to the original transform. Attach both first and third person in case there is a perspective switch.
#if FIRST_PERSON_CONTROLLER
            DetachAttachClip(false, true);
            AddRemoveReloadableClip(false, true);
#endif
            DetachAttachClip(false, false);
            AddRemoveReloadableClip(false, false);

            // Clean up from the event.
            m_ReloadDetachClipEvent.CancelWaitForEvent();
            m_ReloadAttachClipEvent.CancelWaitForEvent();
        }

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        public override void ReloadItem(bool fullClip)
        {
            m_ReloadEvent.CancelWaitForEvent();

            if (!fullClip) {
                EventHandler.ExecuteEvent(Character, "OnAddCrosshairsSpread", false, false);
            }

            // Reload the clip.
            ShootableAction.ReloadClip(true, fullClip || m_ReloadType == ReloadClipType.Full);

            if (!fullClip && m_ReloadType == ReloadClipType.Single) {
                m_ReloadAnimatorAudioStateSet.PlayAudioClip(CharacterItem.GetVisibleObject());

                // If the item cannot be reloaded any more then the complete animation will play.
                if (CanReloadItem(false)) {
                    m_ReloadAnimatorAudioStateSet.NextState();
                }
            }

            if (!fullClip && m_ReloadDropClip != null) {
                // When the item is reloaded the clip should also be replaced.
#if FIRST_PERSON_CONTROLLER
                AddRemoveReloadableClip(true, true);
#endif
                AddRemoveReloadableClip(true, false);
            }

            // The item may need to be reloaded again if the reload type is single and the inventory still has ammo.
            if (CanReloadItem(true)) {
                UpdateItemAbilityAnimatorParameters();
                StartItemReload();
            } else {
                m_Reloaded = true;
                UpdateItemAbilityAnimatorParameters();
                // The reload ability isn't done until the ReloadItemComplete method is called.
                ReloadCompleteEvent.WaitForEvent(false);
            }
        }

        /// <summary>
        /// Adds or removes the instantiated reloadable clip.
        /// </summary>
        /// <param name="add">Should the reloadable clip be instantiated?</param>
        /// <param name="firstPerson">Is the first person perspective being affected?</param>
        private void AddRemoveReloadableClip(bool add, bool firstPerson)
        {
            // If the perspective properties is null then that perspective isn't setup for the character.
            if (m_ReloadDropClip == null) {
                return;
            }

            // If the clip can't be detached then the weapon's clip shouldn't be disabled.
            if (!m_ReloadDetachAttachClip) {
                return;
            }

            var reloadableClip = GetReloadableClip(firstPerson);
            if (reloadableClip == null) {
                return;
            }

            var reloadableClipAttachment = GetReloadableClipAttachment(firstPerson);
            if (add) {
                var clip = ObjectPoolBase.Instantiate(m_ReloadDropClip, reloadableClip.position, reloadableClip.rotation);
                var remover = clip.GetCachedComponent<Remover>();
                if (remover != null) {
                    remover.CancelRemoveEvent();
                }
                // The first person perspective requires the clip to be on the overlay layer so the fingers won't render in front of the clip.
                clip.transform.SetLayerRecursively(firstPerson ? LayerManager.Overlay : clip.layer);
                clip.transform.SetParentOrigin(reloadableClipAttachment);
                clip.transform.SetPositionAndRotation(reloadableClip.position, reloadableClip.rotation);
                if (firstPerson) {
                    m_FirstPersonReloadAddClip = clip;
                    m_FirstPersonReloadAddClip.SetActive(CharacterLocomotion.FirstPersonPerspective);
                } else {
                    m_ThirdPersonReloadAddClip = clip;
                    m_ThirdPersonReloadAddClip.SetActive(!CharacterLocomotion.FirstPersonPerspective);
                }
            } else {
                var clip = firstPerson ? m_FirstPersonReloadAddClip : m_ThirdPersonReloadAddClip;
                if (clip != null) {
                    ObjectPoolBase.Destroy(clip);
                    if (firstPerson) {
                        m_FirstPersonReloadAddClip = null;
                    } else {
                        m_ThirdPersonReloadAddClip = null;
                    }
                }
                reloadableClip.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        public override void ItemReloadComplete(bool success, bool immediateReload)
        {
            m_ReloadCompleteEvent.CancelWaitForEvent();

            if (!success) {
                // The weapon will not be successfully reloaded if the Reload ability stopped early.
                AttachClip();
                m_ReloadDropClipEvent.CancelWaitForEvent();

            } else if (!immediateReload) {
                m_ReloadCompleteAudioClipSet.PlayAudioClip(CharacterItem.GetVisibleObject());
            }

            m_Reloading = false;

            // The item has been reloaded - inform the state set.
            m_ReloadAnimatorAudioStateSet.StartStopStateSelection(false);
        }

        /// <summary>
        /// The item has been removed by the character.
        /// </summary>
        public override void RemoveItem()
        {
            base.RemoveItem();
            m_ReloadInitialized = false;
        }

        /// <summary>
        /// Get the reload item substate index used to animate the item.
        /// </summary>
        /// <returns>The reload item substate index.</returns>
        public override void GetReloadItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            var audioStateIndex = ReloadAnimatorAudioStateSet.GetItemSubstateIndex();
            var index = m_Reloaded ? 0 : audioStateIndex + m_SubstateIndexData.Index;
            var data = new ItemSubstateIndexData(index, m_SubstateIndexData);

            streamData.TryAddSubstateData(this, data);
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public void ItemUseComplete()
        {
            // When the clip is empty the weapon should be reloaded if specified.
            if (ShootableAction.ClipRemainingCount == 0 && (m_AutoReload & Reload.AutoReloadType.Empty) != 0) {
                EventHandler.ExecuteEvent<int, IItemIdentifier, IItemIdentifier, bool, bool>(Character, "OnItemTryReload", CharacterItem.SlotID, CharacterItem.ItemIdentifier, null, false, true);
            }
        }
    }
}