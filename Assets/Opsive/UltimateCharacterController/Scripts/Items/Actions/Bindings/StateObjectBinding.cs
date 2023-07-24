/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Bindings
{
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Traits;
    using System;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// A Bound State Object contains State Object Bindings used to bind properties of the State Object to external values.
    /// </summary>
    [Serializable]
    public class BoundStateObject : StateObject
    {
        [Tooltip("The state object bindings used to bind properties with external values.")]
        [HideInInspector] [SerializeField] protected StateObjectBindingGroup m_Bindings;

        //The Bound GamObject can be overriden by sub classes.
        protected virtual GameObject BoundGameObject => StateBoundGameObject;
        
        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject this object is attached to.</param>
        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            InitializeStateObjectBindings();
        }

        /// <summary>
        /// Initialize the state object bindings.
        /// </summary>
        public virtual void InitializeStateObjectBindings()
        {
            // IMPORTANT: the bound game object might be different from the stateObject game object.
            // For example the state object game object could be the character and the bound gameobject could be the weapon.
            InitializeStateObjectBindings(BoundGameObject);
        }
        
        /// <summary>
        /// Initialize the binding group.
        /// </summary>
        /// <param name="boundGameObject">The game object to bind.</param>
        public virtual void InitializeStateObjectBindings(GameObject boundGameObject)
        {
            m_Bindings.Initialize(this, boundGameObject);
        }
    }

    /// <summary>
    /// The State Object Binding Group is an array of State Object Bindings.
    /// </summary>
    [Serializable]
    public class StateObjectBindingGroup
    {
        [Tooltip("The array of State Object Bindings.")]
        [SerializeReference] protected StateObjectBinding[] m_Bindings = new StateObjectBinding[0];

        protected StateObject m_StateObject;
        protected GameObject m_BoundGameObject;
        
        public StateObject StateObject { get => m_StateObject; set => m_StateObject = value; }
        public GameObject BoundGameObject { get => m_BoundGameObject; set => m_BoundGameObject = value; }
        public StateObjectBinding[] Bindings { get => m_Bindings; set => m_Bindings = value; }

        /// <summary>
        /// Initialize the binding group.
        /// </summary>
        /// <param name="boundStateObject">The state Object to bind.</param>
        /// <param name="boundGameObject">The game object to bind.</param>
        public virtual void Initialize(StateObject boundStateObject, GameObject boundGameObject)
        {
            m_StateObject = boundStateObject;
            m_BoundGameObject = boundGameObject;
            
            if (Bindings == null) { return; }

            for (int i = 0; i < Bindings.Length; i++) {
                if(Bindings[i] == null){ continue; }
                
                Bindings[i].Initialize(boundStateObject, boundGameObject);
            }
        }
    }

    /// <summary>
    /// A State Object Binding is a base class for binding state object properties to external values.
    /// </summary>
    [Serializable]
    public abstract class StateObjectBinding
    {
        protected StateObject m_StateObject;
        protected GameObject m_BoundGameObject;
        
        public StateObject StateObject => m_StateObject;
        public GameObject BoundGameObject => m_BoundGameObject;
        
        /// <summary>
        /// Initialize the state binding with the bound state object.
        /// </summary>
        /// <param name="boundStateObject">The state object to bind.</param>
        /// <param name="boundGameObject">The game object to bind.</param>
        public virtual void Initialize(StateObject boundStateObject, GameObject boundGameObject)
        {
            m_StateObject = boundStateObject;
            m_BoundGameObject = boundGameObject;
            if (!Application.isPlaying) { return; }

            InitializeInternal();
        }

        /// <summary>
        /// Initialize the binding object.
        /// </summary>
        protected abstract void InitializeInternal();

        /// <summary>
        /// Override to simply the name.
        /// </summary>
        /// <returns>A simple readable name.</returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }
    
    /// <summary>
    /// A State Object Binding is a base class for binding state object properties to external values.
    /// </summary>
    [Serializable]
    public class AttributeBinding : StateObjectBinding
    {
        [Tooltip("The Attribute Manager containing the attribute to bind.")]
        [SerializeField] protected AttributeManager m_AttributeManager;
        [Tooltip("The name of the Attribute to bind.")]
        [SerializeField] protected string m_AttributeName;
        [Tooltip("The property path.")]
        [SerializeField] protected string m_PropertyPath;

        private Traits.Attribute m_Attribute;
        private PropertyInfo m_BoundPropertyInfo;
        
        /// <summary>
        /// Initialize the state binding with the bound state object.
        /// </summary>
        protected override void InitializeInternal()
        {
            if (!Application.isPlaying) {
                return;
            }
            
            m_Attribute = m_AttributeManager?.GetAttribute(m_AttributeName);
            if (m_Attribute == null) { 
                return; 
            }

            m_BoundPropertyInfo = m_StateObject.GetType().GetProperty(m_PropertyPath);
            if (m_BoundPropertyInfo == null) {
                Debug.LogWarning($"Warning: Unable to find the property path {m_PropertyPath} on object {m_StateObject}.");
                return;
            }
            Shared.Events.EventHandler.RegisterEvent<Traits.Attribute>(m_AttributeManager.gameObject, "OnAttributeUpdateValue", OnAttributeChange);
        }

        /// <summary>
        /// An attribute changed on the bound object.
        /// </summary>
        /// <param name="attributeThatChanged">The attribute that changed.</param>
        private void OnAttributeChange(Traits.Attribute attributeThatChanged)
        {
            if (attributeThatChanged != m_Attribute) { return; }

            m_BoundPropertyInfo.SetValue(m_StateObject, m_Attribute.Value);
        }

        /// <summary>
        /// Binding destructor.
        /// </summary>
        ~AttributeBinding()
        {
            if (m_AttributeManager == null) { return; }

            Shared.Events.EventHandler.RegisterEvent<Traits.Attribute>(m_AttributeManager.gameObject, "OnAttributeUpdateValue", OnAttributeChange);

        }
    }
}
