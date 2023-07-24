/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for extra shootable modules, used to add additional functionality to shootable actions.
    /// </summary>
    [Serializable]
    public abstract class ShootableExtraModule : ShootableActionModule
    { }
    
    /// <summary>
    /// An extra module used to enable a scope object on a weapon.
    /// </summary>
    [Serializable]
    public class Scope : ShootableExtraModule, IModuleOnChangePerspectives, IModuleOnAim
    {
        [Tooltip("Should the camera's scope camera be disabled when the character isn't aiming?")]
        [SerializeField] protected bool m_DisableScopeCameraOnNoAim;
        [Tooltip("The gameobject with the scope camera.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<GameObject> m_ScopeCamera;

        public bool DisableScopeCameraOnNoAim { get { return m_DisableScopeCameraOnNoAim; } set { m_DisableScopeCameraOnNoAim = value; } }

        protected bool m_Aiming;
        public bool Aiming => m_Aiming;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_ScopeCamera.Initialize(ShootableAction);
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            if (register) {
                var aimAbility = CharacterLocomotion.GetItemAbility<Aim>();
                if (aimAbility != null) {
                    m_Aiming = aimAbility.IsActive;
                }
            
                DetermineVisibleScopeCamera();
            } else {
                m_Aiming = false;
                DetermineVisibleScopeCamera();
            }
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
            
            DetermineVisibleScopeCamera();
        }
        
        /// <summary>
        /// Determines if the scope camera should be visible.
        /// </summary>
        private void DetermineVisibleScopeCamera()
        {
            var scopeCamera = m_ScopeCamera.GetValue();
            if (scopeCamera == null) { return; }

            var setActive = (!m_DisableScopeCameraOnNoAim || m_Aiming) && Inventory.GetActiveCharacterItem(SlotID) == CharacterItem;
            scopeCamera.SetActive(setActive);
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public void OnChangePerspectives(bool firstPersonPerspective)
        {
            var firstPersonScopeCamera = m_ScopeCamera.GetValue(true);
            if (firstPersonScopeCamera != null) {
                firstPersonScopeCamera.SetActive(false);
            }
            
            var thirdPersonScopeCamera = m_ScopeCamera.GetValue(false);
            if (thirdPersonScopeCamera != null) {
                thirdPersonScopeCamera.SetActive(false);
            }
            
            DetermineVisibleScopeCamera();
        }
    }
    
    /// <summary>
    /// This module allows you to set the animation substate to animate the item when it is out of ammo.
    /// </summary>
    [Serializable]
    public class DryFireSubstate : ShootableExtraModule,
        IModuleGetUseItemSubstateIndex, IModuleCanStartUseItem
    {
        [Tooltip("What priority does the dry subtate index have?")]
        [SerializeField] protected ItemSubstateIndexData m_SubstateIndexData = new ItemSubstateIndexData(11, 200);
        [Tooltip("Prevent Auto or Burst fire while dry.")]
        [SerializeField] protected bool m_PreventAutoWhileDry = true;

        public ItemSubstateIndexData SubstateIndexData { get => m_SubstateIndexData; set => m_SubstateIndexData = value; }
        public bool PreventAutoWhileDry { get => m_PreventAutoWhileDry; set => m_PreventAutoWhileDry = value; }

        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            var clipRemaining = ShootableAction.ClipRemainingCount;
            if (clipRemaining > 0) {
                return;
            }

            if (ShootableAction.IsTriggering == false || ShootableAction.WasTriggered) {
                return;
            }

            streamData.TryAddSubstateData(this, SubstateIndexData);
        }

        /// <summary>
        /// Can you start using the item?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The use state.</param>
        /// <returns>True if it can start.</returns>
        public bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            if (m_PreventAutoWhileDry == false) { return true; }

            var clipRemaining = ShootableAction.ClipRemainingCount;
            if (clipRemaining > 0) {
                return true;
            }

            if (ShootableAction.WasTriggered) {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Add look sensitivity to the shootable weapon.
    /// Avoiding the item from being used when it is not looking in the fire direction.
    /// </summary>
    [Serializable]
    public class LookSensitivity : ShootableExtraModule, IModuleCanUseItem, IModuleCanStartUseItem, IModuleIsItemUsePending, IModuleItemUseComplete
    {
        [Tooltip("The sensitivity amount for how much the weapon must be looking in the look source direction (-1 is least sensitive and 1 is most).")]
        [SerializeField] protected ItemPerspectiveProperty<float> m_LookSensitivity = new ItemPerspectiveProperty<float>(0.97f, 0.97f);

#if UNITY_EDITOR
        private float m_LastLookSensitivity;
        private int m_ConsistantLookSensitivityCount;
#endif

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_LookSensitivity.Initialize(CharacterItemAction);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public bool CanUseItem()
        {
            var canFire = CanFire();
            
            // Always return true otherwise it makes the weapon stuck.
            return true;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            // It can start, but continuing is optional.
            if (abilityState != UsableAction.UseAbilityState.Update) { return true; }

            return CanFire();
        }

        /// <summary>
        /// Can the weapon be fired?
        /// </summary>
        /// <returns>True if the item can be fired.</returns>
        private bool CanFire()
        {
            var fireInLookSourceDirection = ShootableAction.MainShooterModule.FireInLookSourceDirection;
            var notIndependentLook = !CharacterLocomotion.ActiveMovementType.UseIndependentLook(true);

            fireInLookSourceDirection &= notIndependentLook;

            // The object has to be facing in the same general direction as the character. When the ability is not active the direction shouldn't prevent
            // the ability from starting. This will allow the weapon to move to the correct direction while the ability is active.
            if (fireInLookSourceDirection) {
                var lookSensitivity = 0f;
                if (m_LookSensitivity.UsingFirstPersonPerspective) {
                    var firstPersonPerspective = CharacterItem.FirstPersonPerspectiveItem;
                    var visibleTransform = firstPersonPerspective.GetVisibleObject().transform;
                    lookSensitivity = Vector3.Dot(visibleTransform.forward, ShootableAction.LookSource.LookDirection(false));
                } else {
                    var thirdPersonPerspective = CharacterItem.ThirdPersonPerspectiveItem;
                    var visibleTransform = thirdPersonPerspective.GetVisibleObject().transform;
                    lookSensitivity = Vector3.Dot(visibleTransform.forward,
                        ShootableAction.LookSource.LookDirection(visibleTransform.position, false, 0, true, false));
                }

#if UNITY_EDITOR
                // A common cause for the weapon not being able to fire is because of the look sensitivity. Add a check to display a warning if the look sensitivity is blocking the firing.
                if (lookSensitivity <= m_LookSensitivity.GetValue() && m_ConsistantLookSensitivityCount != -1) {
                    if (Mathf.Abs(m_LastLookSensitivity - lookSensitivity) < 0.05f) {
                        m_ConsistantLookSensitivityCount++;
                        if (m_ConsistantLookSensitivityCount > 1) {

                            Debug.LogWarning(
                                $"Warning: The ShootableWeapon {GameObject.name} is unable to fire because of the Look Sensitivity on the ShootableWeaponProperties. See this page for more info: " +
                                "https://opsive.com/support/documentation/ultimate-character-controller/items-inventory/character-item/item-actions/usable/shootable/",
                                ShootableAction);
                            m_ConsistantLookSensitivityCount = -1;
                        }
                    } else { m_ConsistantLookSensitivityCount = 0; }

                    m_LastLookSensitivity = lookSensitivity;
                }
#endif
                return lookSensitivity > m_LookSensitivity.GetValue();
            }

            return true;
        }

        /// <summary>
        /// Is the item use pending, meaning it has started but isn't ready to be used just yet.
        /// </summary>
        /// <returns>True if the item is use pending.</returns>
        public bool IsItemUsePending()
        {
            // Returning was locked will ensure that the item does not get locked in a non-usable state.
            return false;
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public void ItemUseComplete()
        {
            // Do nothing.
        }
    }
    
    /// <summary>
    /// A Character Item Action Module used to define what information to show in the Slot Item Monitor.
    /// </summary>
    [Serializable]
    public class SlotItemMonitorModule : ShootableExtraModule, IModuleSlotItemMonitor
    {
        [Tooltip("The priority of this module over other Item Monitor modules.")]
        [SerializeField] protected int m_Priority;
        [Tooltip("Show the loaded amount of ammo?")]
        [SerializeField] protected bool m_ShowLoaded = true;
        [Tooltip("Show the unloaded amount of ammo?")]
        [SerializeField] protected bool m_ShowUnloaded = true;

        public int Priority { get => m_Priority; set => m_Priority = value; }
        public bool ShowLoaded { get => m_ShowLoaded; set => m_ShowLoaded = value; }
        public bool ShowUnloaded { get => m_ShowUnloaded; set => m_ShowUnloaded = value; }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            var target = Character;
            
            Shared.Events.EventHandler.RegisterUnregisterEvent<CharacterItem, ShootableClipModule>(register, target, "OnShootableItemClipChange", OnShootableItemClipChange);
            Shared.Events.EventHandler.RegisterUnregisterEvent<CharacterItem, ShootableAmmoModule>(register, target, "OnShootableItemAmmoChange", OnShootableItemAmmoChange);
            if (register) {
                RefreshSlotItemMonitor();
            }
        }

        /// <summary>
        /// On ammo changed.
        /// </summary>
        /// <param name="characterItem">The character item.</param>
        /// <param name="ammoModule">The ammo module.</param>
        protected virtual void OnShootableItemAmmoChange(CharacterItem characterItem, ShootableAmmoModule ammoModule)
        {
            RefreshSlotItemMonitor();
        }

        /// <summary>
        /// On the item content changed.
        /// </summary>
        /// <param name="characterItem">The character item.</param>
        /// <param name="clipModule">The clip module.</param>
        protected virtual void OnShootableItemClipChange(CharacterItem characterItem, ShootableClipModule clipModule)
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
            var clipModule = ShootableAction.MainClipModule;
            var remaining = clipModule?.ClipRemainingCount ?? 0;

            loadedCount = remaining == int.MaxValue ? "∞" :remaining.ToString();
            return m_ShowLoaded;
        }

        /// <summary>
        /// Try get the unloaded count.
        /// </summary>
        /// <param name="unloadedCount">The unloaded count.</param>
        /// <returns>True if there is an unloaded count.</returns>
        public bool TryGetUnLoadedCount(out string unloadedCount)
        {
            var ammoModule = ShootableAction.MainAmmoModule;
            var ammoLeft = ammoModule?.GetAmmoRemainingCount() ?? 0;

            unloadedCount = ammoLeft == int.MaxValue ? "∞" :ammoLeft.ToString();
            return m_ShowUnloaded;
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
    
    /// <summary>
    /// This module will prevent the shootable action from dry firing by preventing start use if there is no ammo.
    /// </summary>
    [Serializable]
    public class PreventDryFire : ShootableExtraModule, IModuleCanStartUseItem
    {
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            var noAmmoLeft = ShootableAction.MainClipModule.ClipRemainingCount == 0 && ShootableAction.MainAmmoModule.HasAmmoRemaining() == false;
            return noAmmoLeft == false;
        }
    }
}