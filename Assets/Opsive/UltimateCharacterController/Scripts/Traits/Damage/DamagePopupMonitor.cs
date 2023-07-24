/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits.Damage
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Objects;
    using UnityEngine;

    /// <summary>
    /// Interface for showing a damage popup.
    /// </summary>
    public interface IDamagePopup
    {
        GameObject gameObject { get; }

        /// <summary>
        /// Opens the popup with the specified DamageData.
        /// </summary>
        /// <param name="damageData">Specifies the damage location/amount.</param>
        void Open(DamageData damageData);

        /// <summary>
        /// Opens the popup at the specified position with the specified amount.
        /// </summary>
        /// <param name="position">The position that the popup should open at.</param>
        /// <param name="amount">The amount of damage dealt.</param>
        void Open(Vector3 position, float amount);
    }

    public class DamagePopupMonitor : MonoBehaviour
    {
        [Tooltip("The unique ID of the Damage Popup Manager. The Health component can specify the ID of the popup used.")]
        [SerializeField] protected uint m_ID;
        [Tooltip("The prefab for showing the damage popup.")]
        [SerializeField] protected GameObject m_DefaultDamagePrefab;
        [Tooltip("The prefab for showing the health popup.")]
        [SerializeField] protected GameObject m_DefaultHealPrefab;
        [Tooltip("Match a damage prefab to an IDObject ID set on the Hit Collider.")]
        [SerializeField] protected IDObject<GameObject>[] m_ColliderIDObjectsDamagePrefab;

        /// <summary>
        /// Initailizes the default values.
        /// </summary>
        protected virtual void Awake()
        {
            GlobalDictionary.Set(this, m_ID);
        }

        /// <summary>
        /// Opens the damage popup.
        /// </summary>
        /// <param name="damageData">The data associated with the damage.</param>
        public virtual void OpenDamagePopup(DamageData damageData)
        {
            var damagePrefab = m_DefaultDamagePrefab;
            var foundMatch = false;
            
            // First check if the hit collider has a matching ID.
            var hitColliderObjectIDs = damageData.HitCollider?.gameObject.GetCachedComponents<ObjectIdentifier>();
            if (hitColliderObjectIDs != null && hitColliderObjectIDs.Length > 0) {
                for (int i = 0; i < hitColliderObjectIDs.Length; i++) {
                    var colliderID = hitColliderObjectIDs[i].ID;
                    
                    for (int j = 0; j < m_ColliderIDObjectsDamagePrefab.Length; j++) {
                        var prefabID = m_ColliderIDObjectsDamagePrefab[j].ID;

                        if (colliderID != prefabID) { continue; }

                        // Found a match
                        foundMatch = true;
                        damagePrefab = m_ColliderIDObjectsDamagePrefab[j].Obj;
                        break;
                    }

                    if (foundMatch) {
                        break;
                    }
                }
            }
            
            // popup the damage prefab.
            var popupGameObject = ObjectPool.Instantiate(damagePrefab, transform);
            var popup = popupGameObject.GetCachedComponent<IDamagePopup>();
            if (popup != null) {
                popup.Open(damageData);
            }
        }

        /// <summary>
        /// Opens the heal popup.
        /// </summary>
        /// <param name="position">The position of the popup.</param>
        /// <param name="amount">The amount of health restored.</param>
        public virtual void OpenHealPopup(Vector3 position, float amount)
        {
            var popupGameObject = ObjectPool.Instantiate(m_DefaultHealPrefab, transform);
            var popup = popupGameObject.GetCachedComponent<IDamagePopup>();
            if (popup != null) {
                popup.Open(position, amount);
            }
        }
    }
}