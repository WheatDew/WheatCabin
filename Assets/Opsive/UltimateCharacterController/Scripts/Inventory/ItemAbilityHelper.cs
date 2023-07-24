/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items;

    /// <summary>
    /// A static class used to easily call Item Abilities on a character through code.
    /// </summary>
    public static class ItemAbilityHelper
    {
        /// <summary>
        /// Try to use an item.
        /// </summary>
        /// <param name="characterItem">The character item to use.</param>
        /// <returns>True if the item will be used.</returns>
        public static bool TryUseItem(CharacterItem characterItem)
        {
            if (characterItem == null) {
                return false;
            }

            var characterLocomotion = characterItem.CharacterLocomotion;
            
            Use useAbility = null;
            var useAbilities = characterLocomotion.GetAbilities<Use>();
            for (int i = 0; i < useAbilities.Length; i++) {
                if (useAbilities[i].SlotID == characterItem.SlotID) {
                    useAbility = useAbilities[i];
                    break;
                }
            }
            
            return characterLocomotion.TryStartAbility(useAbility);
        }

        /// <summary>
        /// Try to drop an item.
        /// </summary>
        /// <param name="characterItem">The character item to drop.</param>
        /// <returns>True if the item can be dropped.</returns>
        public static bool TryDropItem(CharacterItem characterItem)
        {
            if (characterItem == null) {
                return false;
            }

            var characterLocomotion = characterItem.CharacterLocomotion;
            
            Drop dropAbility = null;
            var dropAbilities = characterLocomotion.GetAbilities<Drop>();
            for (int i = 0; i < dropAbilities.Length; i++) {
                var ability = dropAbilities[i];
                if (ability.SlotID == characterItem.SlotID || ability.SlotID == -1) {
                    dropAbility = ability;
                    break;
                }
            }
            
            return characterLocomotion.TryStartAbility(dropAbility);
        }
        
        /// <summary>
        /// Try reload an item.
        /// </summary>
        /// <param name="characterItem">The character item to reload.</param>
        /// <returns>True if the item can be reloaded.</returns>
        public static bool TryReloadItem(CharacterItem characterItem)
        {
            if (characterItem == null) {
                return false;
            }

            var characterLocomotion = characterItem.CharacterLocomotion;
            
            Reload reloadAbility = null;
            var reloadAbilities = characterLocomotion.GetAbilities<Reload>();
            for (int i = 0; i < reloadAbilities.Length; i++) {
                if (reloadAbilities[i].SlotID == characterItem.SlotID) {
                    reloadAbility = reloadAbilities[i];
                    break;
                }
            }
            
            return characterLocomotion.TryStartAbility(reloadAbility);
        }
    }
}