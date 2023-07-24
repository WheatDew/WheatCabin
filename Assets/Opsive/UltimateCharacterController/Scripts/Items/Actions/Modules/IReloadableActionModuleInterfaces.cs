/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules
{
    using Opsive.Shared.Inventory;

    /// <summary>
    /// Interface for modules that can reload.
    /// </summary>
    public interface IModuleReloadClip : IActionModule
    {
        /// <summary>
        /// Reload the clip.
        /// </summary>
        /// <param name="fullClip">Reload the clip completely?</param>
        void ReloadClip(bool fullClip);
    }

    /// <summary>
    /// Interface for modules that can reload.
    /// </summary>
    public interface IModuleGetReloadItemSubstateIndex : IActionModule
    {
        /// <summary>
        /// Get the reload item substate index used to animate the item.
        /// </summary>
        /// <returns>The reload item substate index.</returns>
        public void GetReloadItemSubstateIndex(ItemSubstateIndexStreamData streamData);
    }
    
    /// <summary>
    /// Interface for modules that can reload.
    /// </summary>
    public interface IModuleStartItemReload : IActionModule
    {
        /// <summary>
        /// Starts to reload the item.
        /// </summary>
        public void StartItemReload();
    }
    
    /// <summary>
    /// Interface for modules that can reload.
    /// </summary>
    public interface IModuleCanReloadItem : IActionModule
    {
        /// <summary>
        /// Can the item be reloaded?
        /// </summary>
        /// <param name="checkEquipStatus">Should the reload ensure the item is equipped?</param>
        /// <returns>True if the item can be reloaded.</returns>
        public bool CanReloadItem(bool checkEquipStatus);
    }
    
    /// <summary>
    /// Interface for modules that can reload.
    /// </summary>
    public interface IModuleReloadItem : IActionModule
    {
        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        public void ReloadItem(bool fullClip);
    }
    
    /// <summary>
    /// Interface for modules that can reload.
    /// </summary>
    public interface IModuleItemReloadComplete : IActionModule
    {
        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        public void ItemReloadComplete(bool success, bool immediateReload);
    }
    
    /// <summary>
    /// Interface for modules that can reload.
    /// </summary>
    public interface IModuleShouldReload : IActionModule
    {
        /// <summary>
        /// Should the item be reloaded? An IReloadableItem reference will be returned if the item can be reloaded.
        /// </summary>
        /// <param name="ammoItemIdentifier">The ItemIdentifier that is being reloaded.</param>
        /// <param name="fromPickup">Is the item being reloaded from a pickup?</param>
        /// <returns>A reference to the IReloadableItem if the item can be reloaded. Null if the item cannot be reloaded.</returns>
        public bool ShouldReload(IItemIdentifier ammoItemIdentifier, bool fromPickup);
    }
}