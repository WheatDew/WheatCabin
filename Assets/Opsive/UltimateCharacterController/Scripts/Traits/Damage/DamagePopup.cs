/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits.Damage
{
    using UnityEngine;

    /// <summary>
    /// Popup that appears when an object is damaged/healed.
    /// </summary>
    public class DamagePopup : DamagePopupBase
    {
        [Tooltip("The Text showing the damage.")]
        [SerializeField] protected Shared.UI.Text m_Text;

        /// <summary>
        /// Sets the popup text.
        /// </summary>
        /// <param name="text">The popup text.</param>
        public override void SetText(string text)
        {
            m_Text.text = text;
        }
    }
}