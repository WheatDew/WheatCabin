/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits.Damage
{
    using UnityEngine;

    /// <summary>
    /// An optional module on the character GameObject allowing a default DamageProcessor to be specified.
    /// </summary>
    public class DamageProcessorModule : MonoBehaviour
    {
        [Tooltip("The default Damage Processor if one is not specified.")]
        [SerializeField] protected DamageProcessor m_DefaultDamageProcessor;

        public DamageProcessor DefaultDamageProcessor
        {
            get => m_DefaultDamageProcessor;
            set => m_DefaultDamageProcessor = value;
        }

        /// <summary>
        /// Process the damage using the default damage processor.
        /// </summary>
        /// <param name="damageTarget">The damage target.</param>
        /// <param name="damageData">The damage data containing all the information about the source of the damage.</param>
        public virtual void ProcessDamage( IDamageTarget damageTarget, DamageData damageData)
        {
            ProcessDamage(m_DefaultDamageProcessor, damageTarget, damageData);
        }

        /// <summary>
        /// Process the damage dealt to the target.
        /// </summary>
        /// <param name="damageProcessor">The damage processor that should be used, if none, the default one is used.</param>
        /// <param name="damageTarget">The damage target.</param>
        /// <param name="damageData">The damage data containing all the information about the source of the damage.</param>
        public virtual void ProcessDamage(DamageProcessor damageProcessor, IDamageTarget damageTarget, DamageData damageData)
        {
            if (damageProcessor == null) {
                if (m_DefaultDamageProcessor == null) {
                    damageProcessor = DamageProcessor.Default;
                } else {
                    damageProcessor = m_DefaultDamageProcessor;
                }
            }
            damageProcessor.Process(damageTarget, damageData);
        }
    }
}