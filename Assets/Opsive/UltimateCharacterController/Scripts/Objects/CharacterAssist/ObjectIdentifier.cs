/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Represents a unique identifier for the object that this component is attached to, used by the Detect Object Ability Base ability.
    /// </summary>
    public class ObjectIdentifier : MonoBehaviour
    {
        [Tooltip("The value of the identifier.")]
        [SerializeField] protected uint m_ID;

        public uint ID { get { return m_ID; } set { m_ID = value; } }
    }
    
    /// <summary>
    /// Base class for an ID with a Unity Object.
    /// </summary>
    [Serializable]
    public abstract class IDObjectBase
    {
        [Tooltip("The value of the identifier.")]
        [SerializeField] protected int m_ID = -1;

        public int ID { get => m_ID; set => m_ID = value; }
        public abstract Object BaseObject { get; set; }
        public abstract Type ObjectType { get; }
    }
    
    /// <summary>
    /// Generic class for an ID with a Unity Object.
    /// </summary>
    [Serializable]
    public class IDObject<T> : IDObjectBase where T : UnityEngine.Object
    {
        [Tooltip("A reference to the mapped object.")]
        [SerializeField] protected T m_Object;

        // Use Has Value to avoid checking for null or re-searching for the object.
        [NonSerialized] protected bool m_HasValue;

        public T Obj { get => m_Object; set => m_Object = value; }
        public override Object BaseObject
        {
            get => m_Object ;
            set
            {
                // When a reference is set manually at runtime the ID is reset automatically.
                if (Application.isPlaying) {
                    m_ID = -1;
                    m_HasValue = true;
                }
                m_Object = value as T;
            }
        }

        public override Type ObjectType { get => typeof(T) ; }

        /// <summary>
        /// Get the Object reference with a match ObjectIdentifier ID as a child of the GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to look into.</param>
        /// <param name="obj">The object with matching ID found.</param>
        /// <param name="forceSearch">Force a search, or use the cached object if it exists.</param>
        /// <returns>True if an object was found.</returns>
        public bool TryGetObjectInChildren(GameObject gameObject, out T obj, bool forceSearch = false)
        {
            return TryGetObject(gameObject, true, out obj, forceSearch);
        }

        /// <summary>
        /// Get the Object reference with a match ObjectIdentifier ID as a child of the GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to look into.</param>
        /// <param name="forceSearch">Force a search, or use the cached object if it exists?</param>
        /// <returns>True if an object was found.</returns>
        public T GetObjectInChildren(GameObject gameObject, bool forceSearch = false)
        {
            TryGetObjectInChildren(gameObject, out var obj, forceSearch);
            return obj;
        }

        /// <summary>
        /// Get the Object reference with a match ObjectIdentifier ID as a parent of the GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to look into.</param>
        /// <param name="obj">The object with matching ID found.</param>
        /// <param name="forceSearch">Force a search, or use the cached object if it exists.</param>
        /// <returns>True if an object was found.</returns>
        public bool TryGetObjectInParent(GameObject gameObject, out T obj, bool forceSearch = false)
        {
            return TryGetObject(gameObject, false, out obj, forceSearch);
        }
        
        /// <summary>
        /// Get the Object reference with a match ObjectIdentifier ID as a parent of the gameobject.
        /// </summary>
        /// <param name="gameObject">The gameObject to look into.</param>
        /// <param name="forceSearch">Force a search, or use the cached object if it exists.</param>
        /// <returns>True if an object was found.</returns>
        public T GetObjectInParent(GameObject gameObject, bool forceSearch = false)
        {
            TryGetObjectInParent(gameObject, out var obj, forceSearch);
            return obj;
        }

        /// <summary>
        /// Get the Object reference with a match ObjectIdentifier ID as a parent or child of the GameObject.
        /// </summary>
        /// <param name="gameObject">The gameObject to look into.</param>
        /// <param name="inChildren">Search as children of the GameObject or as parent?</param>
        /// <param name="obj">The object with matching ID found.</param>
        /// <param name="forceSearch">Force a search, or use the cached object if it exists.</param>
        /// <returns>True if an object was found.</returns>
        public bool TryGetObject(GameObject gameObject, bool inChildren, out T obj, bool forceSearch = false)
        {
            if (!forceSearch && (m_HasValue || m_Object != null) ) {
                obj = m_Object;
                m_HasValue = true;
                return m_Object != null;
            }

            if (m_ID == -1) {
                obj = null;
                m_HasValue = true;
                return false;
            }

            if (gameObject == null) {
                obj = null;
                return false;
            }

            var objectIDs = inChildren
                ? gameObject.GetComponentsInChildren<ObjectIdentifier>(true)
                : gameObject.GetComponentsInParent<ObjectIdentifier>(true);
                    
            for (int i = 0; i < objectIDs.Length; ++i) {
                if (objectIDs[i].ID == m_ID) {
                    m_Object = objectIDs[i].GetComponent<T>();
                    obj = m_Object;
                    m_HasValue = true;
                    return true;
                }
            }
            
            obj = null;
            m_HasValue = true;
            return false;
        }

        /// <summary>
        /// Get the Object reference with a match ObjectIdentifier ID as a parent or child of the GameObject.
        /// </summary>
        /// <param name="gameObject">The gameObject to look into.</param>
        /// <param name="inChildren">Search as children of the GameObject or as parent?</param>
        /// <param name="forceSearch">Force a search, or use the cached object if it exists.</param>
        /// <returns>True if an object was found.</returns>
        public T GetObject(GameObject gameObject, bool inChildren, bool forceSearch = false)
        {
            TryGetObject(gameObject, inChildren, out var obj, forceSearch);
            return obj;
        }

        /// <summary>
        /// Reset the value such that it can be recomputed.
        /// </summary>
        public void ResetValue()
        {
            m_HasValue = false;
            m_Object = null;
        }
    }
}