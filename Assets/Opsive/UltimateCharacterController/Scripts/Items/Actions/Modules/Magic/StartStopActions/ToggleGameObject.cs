/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.StartStopActions
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A data class containing a game object to toggle on or off.
    /// </summary>
    [Serializable]
    public class PerspectiveGameObjectToggle
    {
        [Tooltip("Toggle the game object on?")]
        [SerializeField] protected bool m_On = true;
        [Tooltip("The game object to toggle.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<GameObject> m_GameObject;
        
        public bool On { get => m_On; set => m_On = value; }

        [Shared.Utility.NonSerialized] public ItemPerspectiveIDObjectProperty<GameObject> GameObject
        {
            get => m_GameObject;
            set => m_GameObject = value;
        }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="characterItemAction">The character item action attached.</param>
        public void Initialize(CharacterItemAction characterItemAction)
        {
            m_GameObject.Initialize(characterItemAction);
        }

        /// <summary>
        /// Get the game object.
        /// </summary>
        /// <returns>The gameobject.</returns>
        public GameObject GetValue()
        {
            return m_GameObject.GetValue();
        }
        
        /// <summary>
        /// Get the gameobject in first or third person perspective.
        /// </summary>
        /// <param name="firstPerson">Get the gameobject in first person perspective?</param>
        /// <returns>The gameobject.</returns>
        public GameObject GetValue(bool firstPerson)
        {
            return m_GameObject.GetValue(firstPerson);
        }
    }
    
    /// <summary>
    /// Toggle a game object on or off.
    /// </summary>
    [System.Serializable]
    public class ToggleGameObject : MagicStartStopModule
    {
        [Tooltip("Toggle the gameobject on and off.")]
        [SerializeField] protected PerspectiveGameObjectToggle[] m_Toggle;
        [Tooltip("Toggle the game object on start.")]
        [SerializeField] protected bool m_ToggleOnStart;
        [Tooltip("Toggle the game object on stop.")]
        [SerializeField] protected bool m_ToggleOnStop;
        
        public PerspectiveGameObjectToggle[] Toggle { get => m_Toggle; set => m_Toggle = value; }
        public bool ToggleOnStart { get => m_ToggleOnStart; set => m_ToggleOnStart = value; }
        public bool ToggleOnStop { get => m_ToggleOnStop; set => m_ToggleOnStop = value; }

        /// <summary>
        /// Initialize to check if this is a begin or end action.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            for (int i = 0; i < m_Toggle.Length; i++) {
                m_Toggle[i].Initialize(m_CharacterItemAction);
            }
        }
        
        /// <summary>
        /// Activates or deactives the flashlight.
        /// </summary>
        /// <param name="active">Should the flashlight be activated?</param>
        public virtual void ToggleGameObjects()
        {
            for (int i = 0; i < m_Toggle.Length; i++) {
                var gameObject = m_Toggle[i].GetValue();
                if(gameObject == null){ continue; }

                gameObject.SetActive(!gameObject.activeSelf);
            }
        }
        
        /// <summary>
        /// Reset the gameobjects active state to the default state.
        /// </summary>
        public virtual void ResetGameObjects()
        {
            for (int i = 0; i < m_Toggle.Length; i++) {
                var gameObject = m_Toggle[i].GetValue();
                if(gameObject == null){ continue; }

                gameObject.SetActive(m_Toggle[i].On == false);
            }
        }

        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="useDataStream">The location that the cast originates from.</param>
        public override void Start(MagicUseDataStream useDataStream)
        {
            if(m_ToggleOnStart == false){ return; }

            ToggleGameObjects();
        }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        public override void Stop(MagicUseDataStream useDataStream)
        {
            if(m_ToggleOnStop == false){ return; }

            ToggleGameObjects();
        }

        /// <summary>
        /// The item will start Unequipping.
        /// </summary>
        public override void StartUnequip()
        {
            base.StartUnequip();
            ResetGameObjects();
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public override void OnChangePerspectives(bool firstPersonPerspective)
        {
            base.OnChangePerspectives(firstPersonPerspective);
            
            for (int i = 0; i < m_Toggle.Length; i++) {
                var previousPerspectiveGameObject = m_Toggle[i].GetValue(!firstPersonPerspective);
                var newPerspectiveGameObject = m_Toggle[i].GetValue(firstPersonPerspective);

                var previousWasActive = m_Toggle[i].On == false;
                if (previousPerspectiveGameObject != null) {
                    previousWasActive = previousPerspectiveGameObject.activeSelf;
                    previousPerspectiveGameObject.SetActive(m_Toggle[i].On == false);
                }
                
                if (newPerspectiveGameObject != null) {
                    newPerspectiveGameObject.SetActive(previousWasActive);
                }
            }
        }
    }
}