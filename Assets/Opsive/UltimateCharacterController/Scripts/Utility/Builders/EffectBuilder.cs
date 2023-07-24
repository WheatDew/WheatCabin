/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Effects;
    using System;

    /// <summary>
    /// Adds UltimateCharacterLocomotion effects.
    /// </summary>
    public static class EffectBuilder
    {
        /// <summary>
        /// Adds the effect to the specified Ultimate Character Locomotion.
        /// </summary>
        /// <param name="characterLocomotion">A reference to the Ultimate Character Locomotion that should have the effect added.</param>
        /// <param name="type">The type of effect.</param>
        /// <returns>The added effect.</returns>
        public static Effect AddEffect(UltimateCharacterLocomotion characterLocomotion, Type type)
        {
            var effects = characterLocomotion.Effects;
            if (effects == null) {
                effects = new Effect[1];
            } else {
                Array.Resize(ref effects, effects.Length + 1);
            }
            effects[effects.Length - 1] = Activator.CreateInstance(type) as Effect;
            characterLocomotion.Effects = effects;
            return effects[effects.Length - 1];
        }
    }
}