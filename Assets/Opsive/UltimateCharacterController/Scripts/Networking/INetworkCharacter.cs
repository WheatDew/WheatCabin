/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Character
{
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Melee;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
    using UnityEngine;

    /// <summary>
    /// Acts as a bridge between the character controller and the underlying networking implementation.
    /// </summary>
    public interface INetworkCharacter
    {
        /// <summary>
        /// Loads the inventory's default loadout.
        /// </summary>
        void LoadDefaultLoadout();

        /// <summary>
        /// Equips or unequips the item with the specified ItemIdentifier and slot.
        /// </summary>
        /// <param name="itemIdentifierID">The ID of the ItemIdentifier that should be equipped.</param>
        /// <param name="slotID">The slot of the item that should be equipped.</param>
        /// <param name="equip">Should the item be equipped? If false it will be unequipped.</param>
        void EquipUnequipItem(uint itemIdentifierID, int slotID, bool equip);

        /// <summary>
        /// The ItemIdentifier has been picked up.
        /// </summary>
        /// <param name="itemIdentifierID">The ID of the ItemIdentifier that was picked up.</param>
        /// <param name="amount">The number of ItemIdentifier picked up.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        void ItemIdentifierPickup(uint itemIdentifierID, int amount, int slotID, bool immediatePickup, bool forceEquip);

        /// <summary>
        /// Remove an item amount from the inventory.
        /// </summary>
        /// <param name="itemIdentifierID">The ID of the ItemIdentifier that was removed.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        /// <param name="drop">Should the item be dropped?</param>
        /// <param name="removeCharacterItem">Should the character item be removed?</param>
        /// <param name="destroyCharacterItem">Should the character item be destroyed?</param>
        void RemoveItemIdentifierAmount(uint itemIdentifierID, int slotID, int amount, bool drop, bool removeCharacterItem, bool destroyCharacterItem);

        /// <summary>
        /// Removes all of the items from the inventory.
        /// </summary>
        void RemoveAllItems();

        /// <summary>
        /// Invokes the Shootable Action Fire Effect modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="data">The data being sent to the module.</param>
        void InvokeShootableFireEffectModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, ShootableUseDataStream data);

        /// <summary>
        /// Invokes the Shootable Action Dry Fire Effect modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="data">The data being sent to the module.</param>
        void InvokeShootableDryFireEffectModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, ShootableUseDataStream data);

        /// <summary>
        /// Invokes the Shootable Action Impact modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="context">The context being sent to the module.</param>
        void InvokeShootableImpactModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, ShootableImpactCallbackContext context);

        /// <summary>
        /// Starts to reload the item.
        /// </summary>
        /// <param name="module">The module that is being reloaded.</param>
        void StartItemReload(ShootableReloaderModule module);

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="module">The module that is being reloaded.</param>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        void ReloadItem(ShootableReloaderModule itemAction, bool fullClip);

        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="module">The module that is being reloaded.</param>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        void ItemReloadComplete(ShootableReloaderModule module, bool success, bool immediateReload);

        /// <summary>
        /// Invokes the Melee Action Attack module.
        /// </summary>
        /// <param name="module">The module that is being invoked.</param>
        /// <param name="data">The data being sent to the module.</param>
        void InvokeMeleeAttackModule(MeleeAttackModule module, MeleeUseDataStream data);

        /// <summary>
        /// Invokes the Melee Action Attack Effect module.
        /// </summary>
        /// <param name="module">The module that is being invoked.</param>
        /// <param name="data">The data being sent to the module.</param>
        void InvokeMeleeAttackEffectModule(ActionModule module, MeleeUseDataStream data);

        /// <summary>
        /// Invokes the Melee Action Impact modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="context">The context being sent to the module.</param>
        void InvokeMeleeImpactModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, MeleeImpactCallbackContext context);

        /// <summary>
        /// Invokes the Throwable Action Effect modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="data">The data being sent to the module.</param>
        void InvokeThrowableEffectModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, ThrowableUseDataStream data);

        /// <summary>
        /// Enables the object mesh renderers for the Throwable Action.
        /// </summary>
        /// <param name="module">The module that is having the renderers enabled.</param>
        /// <param name="enable">Should the renderers be enabled?</param>
        void EnableThrowableObjectMeshRenderers(ActionModule module, bool enable);

        /// <summary>
        /// Invokes the Magic Action Begin or End modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="start">Should the module be started? If false the module will be stopped.</param>
        /// <param name="data">The data being sent to the module.</param>
        void InvokeMagicBeginEndModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, bool start, MagicUseDataStream data);

        /// <summary>
        /// Specifies the cast state. Used by the magic cast effects.
        /// </summary>
        public enum CastEffectState : short
        {
            Start,  // The cast has started.
            Update, // The cast has updated.
            End     // The cast has ended.
        }
        /// <summary>
        /// Invokes the Magic Cast Effects modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="state">Specifies the state of the cast.</param>
        /// <param name="data">The data being sent to the module.</param>
        void InvokeMagicCastEffectsModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, CastEffectState state, MagicUseDataStream data);

        /// <summary>
        /// Invokes the Magic Action Impact modules.
        /// </summary>
        /// <param name="itemAction">The Item Action that is invoking the modules.</param>
        /// <param name="moduleGroup">The group that the modules belong to.</param>
        /// <param name="invokedBitmask">The bitmask of the invoked modules.</param>
        /// <param name="context">The context being sent to the module.</param>
        void InvokeMagicImpactModules(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup, int invokedBitmask, ImpactCallbackContext context);

        /// <summary>
        /// Invokes the Usable Action Geenric Effect module.
        /// </summary>
        /// <param name="module">The module that should be invoked.</param>
        void InvokeGenericEffectModule(ActionModule module);

        /// <summary>
        /// Invokes the Use Attribute Modifier Toggle module.
        /// </summary>
        /// <param name="module">The module that should be invoked.</param>
        /// <param name="on">Should the module be toggled on?</param>
        void InvokeUseAttributeModifierToggleModule(ActionModule module, bool on);

        /// <summary>
        /// Pushes the target Rigidbody in the specified direction.
        /// </summary>
        /// <param name="targetRigidbody">The Rigidbody to push.</param>
        /// <param name="force">The amount of force to apply.</param>
        /// <param name="point">The point at which to apply the push force.</param>
        void PushRigidbody(Rigidbody targetRigidbody, Vector3 force, Vector3 point);

        /// <summary>
        /// Sets the rotation of the character.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        void SetRotation(Quaternion rotation, bool snapAnimator);

        /// <summary>
        /// Sets the position of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        void SetPosition(Vector3 position, bool snapAnimator);

        /// <summary>
        /// Resets the rotation and position to their default values.
        /// </summary>
        void ResetRotationPosition();

        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        /// <param name="stopAllAbilities">Should all abilities be stopped?</param>
        void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool snapAnimator, bool stopAllAbilities);

        /// <summary>
        /// Changes the character model.
        /// </summary>
        /// <param name="modelIndex">The index of the model within the ModelManager.</param>
        void ChangeModels(int modelIndex);

        /// <summary>
        /// Activates or deactivates the character.
        /// </summary>
        /// <param name="active">Is the character active?</param>
        /// <param name="uiEvent">Should the OnShowUI event be executed?</param>
        void SetActive(bool active, bool uiEvent);

        /// <summary>
        /// Executes a bool event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="value">The bool value.</param>
        void ExecuteBoolEvent(string eventName, bool value);
    }
}