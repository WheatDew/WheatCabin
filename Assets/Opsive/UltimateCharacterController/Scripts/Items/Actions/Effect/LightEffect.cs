/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Effect
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Used to set some settings on a light component, such as the intensity.
    /// </summary>
    [Serializable]
    public class LightEffect : ItemEffect
    {
        [Tooltip("The light to affect.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Light> m_Light;
        [Tooltip("The light intensity curve, used to evaluate to light intensity when the light intensity time changes.")]
        [SerializeField] protected AnimationCurve m_LightIntensityCurve;
        [Tooltip("The light intensity time, used to a parameter to evaluate the light intensity.")]
        [SerializeField] protected float m_LightIntensityTime;

        [Shared.Utility.NonSerialized] public Light Light { get => m_Light.GetValue(); set => m_Light.SetValue(value); }
        [Shared.Utility.NonSerialized] public ItemPerspectiveIDObjectProperty<Light> PerspectiveLight { get => m_Light; set => m_Light = value; }
        public AnimationCurve LightIntensityCurve { get => m_LightIntensityCurve; set => m_LightIntensityCurve = value; }
        public float LightIntensityTime
        {
            get => m_LightIntensityTime;
            set { 
                m_LightIntensityTime = value;
                m_Light.GetValue().intensity = m_LightIntensityCurve.Evaluate(m_LightIntensityTime);
            }
        }

        /// <summary>
        /// Initialize the effect.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_Light.Initialize(m_CharacterItemAction);
        }

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanInvokeEffect()
        {
            return true;
        }

        /// <summary>
        /// Invoke the effect.
        /// </summary>
        protected override void InvokeEffectInternal()
        {
            base.InvokeEffectInternal();

            m_Light.GetValue().intensity = m_LightIntensityCurve.Evaluate(m_LightIntensityTime);
        }
    }
}