/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using UnityEngine;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using UnityEngine.Events;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    
    /// <summary>
    /// A component that detects impact from an weapon attack.
    /// </summary>
    public class DebugObjectImpactReceiver : MonoBehaviour
    {
        [Tooltip("The even that gets invoked when the object was impacted.")]
        [SerializeField] protected UnityEvent m_OnObjectImpact;
        [Tooltip("Debug the Impact Callback context.")]
        [SerializeField] protected bool m_DebugLogImpact;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public void Awake()
        {
            EventHandler.RegisterEvent<ImpactCallbackContext>(gameObject, "OnObjectImpact", OnObjectImpact);
        }

        /// <summary>
        /// The magic cast has collided with another object.
        /// </summary>
        /// <param name="ctx">The Impact callback context.</param>
        private void OnObjectImpact(ImpactCallbackContext ctx)
        {
            if (m_DebugLogImpact) {
                Debug.Log(ctx);
            }
            
            m_OnObjectImpact.Invoke();
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public void OnDestroy()
        {
            EventHandler.UnregisterEvent<ImpactCallbackContext>(gameObject, "OnObjectImpact", OnObjectImpact);
        }
    }
}