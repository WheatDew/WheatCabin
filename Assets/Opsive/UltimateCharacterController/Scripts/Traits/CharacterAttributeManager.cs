/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Sublcasses AttributeManager allowing the manager to be aware of the CharacterInitializer.
    /// </summary>
    public class CharacterAttributeManager : AttributeManager
    {
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnAwake += AwakeInternal;
                return;
            }

            AwakeInternal();
        }

        /// <summary>
        /// Initializes the attributes.
        /// </summary>
        protected override void AwakeInternal()
        {
            if (Game.CharacterInitializer.Instance) {
                Game.CharacterInitializer.Instance.OnAwake -= AwakeInternal;
            }
            
            base.AwakeInternal();
        }
    }
}