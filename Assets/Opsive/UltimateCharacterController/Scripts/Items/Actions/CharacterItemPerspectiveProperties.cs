/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
#if FIRST_PERSON_CONTROLLER
    using Opsive.UltimateCharacterController.FirstPersonController.Items;
#endif
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.ThirdPersonController.Items;
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// The base class for an Item perspective property.
    /// It contains a property value for both first and third person perspective
    /// </summary>
    [Serializable]
    public abstract class ItemPerspectiveProperty
    {
        protected CharacterItemAction m_CharacterItemAction;
        protected ModelManager m_ModelManager;

#if FIRST_PERSON_CONTROLLER
        protected FirstPersonPerspectiveItem m_FirstPersonPerspectiveItem;
#endif
        protected Transform m_FirstPersonVisibleTransform;
        protected GameObject m_FirstPerspectiveItemObjectParent;

        protected ThirdPersonPerspectiveItem m_ThirdPersonPerspectiveItem;
        protected Transform m_ThirdPersonObjectTransform;
        protected GameObject m_ThirdPerspectiveItemObjectParent;

        public GameObject Character => m_CharacterItemAction.Character;
#if FIRST_PERSON_CONTROLLER
        public GameObject FirstPersonObject => m_FirstPersonPerspectiveItem.Object;
#endif
        public GameObject ThirdPersonObject => m_ThirdPersonPerspectiveItem.Object;
        public GameObject PerspectiveVisibleObject
        {
            get {
                return m_CharacterItemAction.UsingFirstPerspective ?
#if FIRST_PERSON_CONTROLLER
                        FirstPersonObject
#else
                        null
#endif
                        : ThirdPersonObject;
            }
        }

        public bool UsingFirstPersonPerspective => m_CharacterItemAction.UsingFirstPerspective;

        protected bool m_Initialized;

        /// <summary>
        /// Initialize the perspective property.
        /// </summary>
        /// <param name="characterItemAction">The character item action.</param>
        public virtual void Initialize(CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
            m_ModelManager = characterItemAction.Character.GetCachedComponent<ModelManager>();

            var foundFirst = false;
            var foundThird = false;
            var perspectiveItems = characterItemAction.CharacterItem.GetComponents<PerspectiveItem>();
            for (int i = 0; i < perspectiveItems.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                if (perspectiveItems[i].FirstPersonItem) {
                    if (foundFirst) {
                        continue;
                    }

                    m_FirstPersonPerspectiveItem = perspectiveItems[i] as FirstPersonPerspectiveItem;
                    var visibleItem = (m_FirstPersonPerspectiveItem).VisibleItem;
                    m_FirstPersonVisibleTransform = visibleItem != null ? visibleItem.transform : m_FirstPersonPerspectiveItem.transform;

                    // The model should be used if using the Model Manager.
                    var firstPersonObjects = m_ModelManager == null
                        ? m_FirstPersonPerspectiveItem.GetFirstPersonObjects(characterItemAction.Character)
                        : m_ModelManager.GetFirstPersonObjects(m_ModelManager.ActiveModel);

                    // The character might not have first person objects, for example an AI character.
                    if (firstPersonObjects != null) {
                        m_FirstPerspectiveItemObjectParent = firstPersonObjects.gameObject;
                        foundFirst = true;
                    }
                }
#endif
                if (!perspectiveItems[i].FirstPersonItem) {
                    if (foundThird) {
                        continue;
                    }

                    m_ThirdPersonPerspectiveItem = perspectiveItems[i] as ThirdPersonPerspectiveItem;
                    m_ThirdPersonObjectTransform = m_ThirdPersonPerspectiveItem.transform;

                    // The model should be used if using the Model Manager.
                    if (m_ModelManager == null) {
                        m_ThirdPerspectiveItemObjectParent = characterItemAction.Character;
                    } else {
                        m_ThirdPerspectiveItemObjectParent = m_ModelManager.ActiveModel;
                    }

                    foundThird = true;
                }

                if (foundFirst && foundThird) {
                    break;
                }
            }

            if (m_ModelManager != null) {
                Shared.Events.EventHandler.RegisterEvent<GameObject>(Character, "OnCharacterSwitchModels", OnCharacterSwitchModels);
            }

            m_Initialized = true;
        }

        /// <summary>
        /// The character model has changed.
        /// </summary>
        /// <param name="characterModel">The new character model.</param>
        protected virtual void OnCharacterSwitchModels(GameObject characterModel)
        {
            // With the model changes the FirstPersonObject also may have changed. Retrieve the updated object.
#if FIRST_PERSON_CONTROLLER
            if (m_ModelManager != null) {
                m_FirstPerspectiveItemObjectParent = m_ModelManager.GetFirstPersonObjects(characterModel).gameObject;
            } else {
                m_FirstPerspectiveItemObjectParent = m_FirstPersonPerspectiveItem.GetFirstPersonObjects(characterModel).gameObject;
            }
#endif

            m_ThirdPerspectiveItemObjectParent = characterModel;
        }
    }

    /// <summary>
    /// The perspective property interface gives access to a first or third person property value.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    public interface IPerspectiveProperty<T>
    {
        public bool UsingFirstPersonPerspective { get; }

        /// <summary>
        /// Get the property value of the current perspective.
        /// </summary>
        /// <returns>The property valur of the current perspective.</returns>
        public T GetValue()
        {
            return GetValue(UsingFirstPersonPerspective);
        }

        /// <summary>
        /// Set the property valur of the current perspective.
        /// </summary>
        /// <param name="value">The property value.</param>
        public void SetValue(T value)
        {
            SetValue(value, UsingFirstPersonPerspective);
        }

        /// <summary>
        /// Get the property value of the first or third person perspective.
        /// </summary>
        /// <param name="firstPerson">The first person perspective?</param>
        /// <returns>The property value of the first or third person perspective.</returns>
        public T GetValue(bool firstPerson)
        {
            if (firstPerson) {
                return GetFirstPersonValue();
            } else {
                return GetThirdPersonValue();
            }
        }

        /// <summary>
        ///  Set the property valur of the first or third person perspective.
        /// </summary>
        /// <param name="value">The property value to set.</param>
        /// <param name="firstPerson">The first person perspective?</param>
        public void SetValue(T value, bool firstPerson)
        {
            if (firstPerson) {
                SetFirstPersonValue(value);
            } else {
                SetThirdPersonValue(value);
            }
        }

        /// <summary>
        /// Get the first person property value.
        /// </summary>
        /// <returns>The first person property value.</returns>
        public T GetFirstPersonValue();

        /// <summary>
        /// Set the first person property value.
        /// </summary>
        /// <param name="value">The first person property value to set.</param>
        public void SetFirstPersonValue(T value);

        /// <summary>
        /// Get the third person property value.
        /// </summary>
        /// <returns>The third person property value.</returns>
        public T GetThirdPersonValue();

        /// <summary>
        /// Set the third person property value.
        /// </summary>
        /// <param name="value">The third person property value to set.</param>
        public void SetThirdPersonValue(T value);
    }

    /// <summary>
    /// A generic item perspective property using an object of type T for first and third person.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    [Serializable]
    public class ItemPerspectiveProperty<T> : ItemPerspectiveProperty, IPerspectiveProperty<T>
    {
#if FIRST_PERSON_CONTROLLER
        [Tooltip("The first person property value.")]
        [SerializeField] protected T m_FirstPerson;
#endif
        [Tooltip("The third person property value.")]
        [SerializeField] protected T m_ThirdPerson;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemPerspectiveProperty()
        { }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="value">The first and third person value.</param>>
        public ItemPerspectiveProperty(T value)
        {
#if FIRST_PERSON_CONTROLLER
            m_FirstPerson = value;
#endif
            m_ThirdPerson = value;
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="firstPersonValue">The first person value.</param>
        /// <param name="thirdPersonValue">The third person value.</param>
        public ItemPerspectiveProperty(T firstPersonValue, T thirdPersonValue)
        {
#if FIRST_PERSON_CONTROLLER
            m_FirstPerson = firstPersonValue;
#endif
            m_ThirdPerson = thirdPersonValue;
        }

        /// <summary>
        /// Get the property value of the current perspective.
        /// </summary>
        /// <returns>The property valur of the current perspective.</returns>
        public T GetValue()
        {
            if (!m_Initialized) {
                Debug.LogError("The Item Perspective Property is not initialized.");
                return default;
            }
            return GetValue(m_CharacterItemAction.UsingFirstPerspective);
        }

        /// <summary>
        /// Set the property valur of the current perspective.
        /// </summary>
        /// <param name="value">The property value.</param>
        public void SetValue(T value)
        {
            if (!m_Initialized) {
                Debug.LogError("The Item Perspective Property is not initialized.");
                return;
            }
            SetValue(value, m_CharacterItemAction.UsingFirstPerspective);
        }

        /// <summary>
        /// Get the property value of the first or third person perspective.
        /// </summary>
        /// <param name="firstPerson">The first person perspective?</param>
        /// <returns>The property value of the first or third person perspective.</returns>
        public T GetValue(bool firstPerson)
        {
            if (firstPerson) {
                return GetFirstPersonValue();
            } else {
                return GetThirdPersonValue();
            }
        }

        /// <summary>
        ///  Set the property valur of the first or third person perspective.
        /// </summary>
        /// <param name="value">The property value to set.</param>
        /// <param name="firstPerson">The first person perspective?</param>
        public void SetValue(T value, bool firstPerson)
        {
            if (firstPerson) {
                SetFirstPersonValue(value);
            } else {
                SetThirdPersonValue(value);
            }
        }

        /// <summary>
        /// Get the first person property value.
        /// </summary>
        /// <returns>The first person property value.</returns>
        public T GetFirstPersonValue()
        {
#if FIRST_PERSON_CONTROLLER
            return m_FirstPerson;
#else
            return default(T);
#endif
        }

        /// <summary>
        /// Set the first person property value.
        /// </summary>
        /// <param name="value">The first person property value to set.</param>
        public void SetFirstPersonValue(T value)
        {
#if FIRST_PERSON_CONTROLLER
            m_FirstPerson = value;
#endif
        }

        /// <summary>
        /// Get the third person property value.
        /// </summary>
        /// <returns>The third person property value.</returns>
        public T GetThirdPersonValue()
        {
            return m_ThirdPerson;
        }

        /// <summary>
        /// Set the third person property value.
        /// </summary>
        /// <param name="value">The third person property value to set.</param>
        public void SetThirdPersonValue(T value)
        {
            m_ThirdPerson = value;
        }
    }

    /// <summary>
    /// A generic Item Perspective with an IDObject property.
    /// Used for Unity Object references which may be assigned an Object Identifier with a specific ID.
    /// This object will look for the ObjectIdentifier use the (First/Third)Person PerspectiveItemObject parent.
    /// </summary>
    [Serializable]
    public class ItemPerspectiveIDObjectProperty<T> : ItemPerspectiveProperty, IPerspectiveProperty<T> where T : Object
    {
#if FIRST_PERSON_CONTROLLER
        [Tooltip("The first person property value.")]
        [SerializeField] protected IDObject<T> m_FirstPerson = new IDObject<T>();
#endif
        [Tooltip("The third person property value.")]
        [SerializeField] protected IDObject<T> m_ThirdPerson = new IDObject<T>();

#if FIRST_PERSON_CONTROLLER
        public IDObject<T> FirstPersonIDObject => m_FirstPerson;
#endif
        public IDObject<T> ThirdPersonIDObject => m_ThirdPerson;

        /// <summary>
        /// Get the property value of the current perspective.
        /// </summary>
        /// <returns>The property valur of the current perspective.</returns>
        public T GetValue()
        {
            if (!m_Initialized) {
                Debug.LogError("The Item Perspective Property is not initialized.");
                return default;
            }
            return GetValue(m_CharacterItemAction.UsingFirstPerspective);
        }

        /// <summary>
        /// Set the property valur of the current perspective.
        /// </summary>
        /// <param name="value">The property value.</param>
        public void SetValue(T value)
        {
            if (!m_Initialized) {
                Debug.LogError("The Item Perspective Property is not initialized.");
                return;
            }
            SetValue(value, m_CharacterItemAction.UsingFirstPerspective);
        }

        /// <summary>
        /// Get the property value of the first or third person perspective.
        /// </summary>
        /// <param name="firstPerson">The first person perspective?</param>
        /// <returns>The property value of the first or third person perspective.</returns>
        public T GetValue(bool firstPerson)
        {
            if (firstPerson) {
                return GetFirstPersonValue();
            } else {
                return GetThirdPersonValue();
            }
        }

        /// <summary>
        ///  Set the property valur of the first or third person perspective.
        /// </summary>
        /// <param name="value">The property value to set.</param>
        /// <param name="firstPerson">The first person perspective?</param>
        public void SetValue(T value, bool firstPerson)
        {
            if (firstPerson) {
                SetFirstPersonValue(value);
            } else {
                SetThirdPersonValue(value);
            }
        }

        /// <summary>
        /// Get the first person property value.
        /// </summary>
        /// <returns>The first person property value.</returns>
        public T GetFirstPersonValue()
        {
#if FIRST_PERSON_CONTROLLER
            if (!m_Initialized) {
                Debug.LogError("The Item Perspective Property is not initialized.");
                return default;
            }
            return m_FirstPerson.GetObjectInChildren(m_FirstPerspectiveItemObjectParent);
#else
            return null;
#endif
        }

        /// <summary>
        /// Set the first person property value.
        /// </summary>
        /// <param name="value">The first person property value to set.</param>
        public void SetFirstPersonValue(T value)
        {
#if FIRST_PERSON_CONTROLLER
            m_FirstPerson.Obj = value;
#endif
        }

        /// <summary>
        /// Get the third person property value.
        /// </summary>
        /// <returns>The third person property value.</returns>
        public T GetThirdPersonValue()
        {
            if (!m_Initialized) {
                Debug.LogError("The Item Perspective Property is not initialized.");
                return default;
            }
            return m_ThirdPerson.GetObjectInChildren(m_ThirdPerspectiveItemObjectParent);
        }

        /// <summary>
        /// Set the third person property value.
        /// </summary>
        /// <param name="value">The third person property value to set.</param>
        public void SetThirdPersonValue(T value)
        {
            m_ThirdPerson.Obj = value;
        }

        /// <summary>
        /// The character model has changed.
        /// </summary>
        /// <param name="characterModel">The new character model.</param>
        protected override void OnCharacterSwitchModels(GameObject characterModel)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPerson.Obj is Component firstPersonComponent && firstPersonComponent != null) {
                if (firstPersonComponent.transform.IsChildOf(m_FirstPerspectiveItemObjectParent.transform) && m_FirstPerson.ID > 0) {
                    // If the object is a child of the previous parent it does need to be reset.
                    m_FirstPerson.ResetValue();
                }
            }
#endif

            if (m_ThirdPerson.Obj is Component thirdPersonComponent && thirdPersonComponent != null) {
                if (thirdPersonComponent.transform.IsChildOf(m_ThirdPerspectiveItemObjectParent.transform) && m_ThirdPerson.ID > 0) {
                    // If the object is a child of the previous parent it does need to be reset.
                    m_ThirdPerson.ResetValue();
                }
            }

            // Call the base function after the reset.
            base.OnCharacterSwitchModels(characterModel);
        }

        /// <summary>
        /// The module has been removed from the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was removed from.</param>
        public void OnEditorDestroyObjectCleanup(GameObject gameObject)
        {
#if FIRST_PERSON_CONTROLLER
            if (m_FirstPerson != null) {
                var obj = m_FirstPerson.Obj;
                if (obj != null) {
                    if (obj is Component component) {
                        UnityEngine.Object.DestroyImmediate(component.gameObject, true);
                    } else {
                        UnityEngine.Object.DestroyImmediate(obj, true);
                    }

                    m_FirstPerson.Obj = null;
                }
            }
#endif
            if (m_ThirdPerson != null) {
                var obj = m_ThirdPerson.Obj;
                if (obj != null) {
                    if (obj is Component component) {
                        UnityEngine.Object.DestroyImmediate(component.gameObject, true);
                    } else {
                        UnityEngine.Object.DestroyImmediate(obj, true);
                    }
                    
                    m_ThirdPerson.Obj = null;
                }
            }
        }
    }
}