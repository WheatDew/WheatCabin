/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Items.Actions.Bindings;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An Impact Action group is an array of impact action which has a custom inspector.
    /// </summary>
    [Serializable]
    public class ImpactActionGroup
    {
        [Tooltip("The list of impact actions.")]
        [SerializeReference] protected ImpactAction[] m_ImpactActions;

        private ActionModule m_ActionModule;

        public ImpactAction[] ImpactActions
        {
            get => m_ImpactActions;
            set => m_ImpactActions = value;
        }
        
        public int Count { get => m_ImpactActions == null ? 0 : m_ImpactActions.Length; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImpactActionGroup()
        {
            m_ImpactActions = new ImpactAction[0];
        }
        
        /// <summary>
        /// Overload constructor.
        /// </summary>
        /// <param name="action">The first action.</param>
        public ImpactActionGroup(ImpactAction action)
        {
            m_ImpactActions = new[] { action };
        }

        /// <summary>
        /// Overload constructor.
        /// </summary>
        /// <param name="actions">The starting actions.</param>
        public ImpactActionGroup(ImpactAction[] actions)
        {
            m_ImpactActions = actions;
        }

        /// <summary>
        /// A static constructor for the default damage impacts use to quickly setup a component.
        /// </summary>
        /// <param name="useContextData">Use the context data?</param>
        /// <returns>The new impact action group setup for damage.</returns>
        public static ImpactActionGroup DefaultDamageGroup(bool useContextData)
        {
            return new ImpactActionGroup(new ImpactAction[]
            {
                new SimpleDamage(useContextData),
                new SpawnSurfaceEffect(useContextData),
                new ImpactEvent(),
            });
        }
        
        /// <summary>
        /// Initialize the impact actions.
        /// </summary>
        /// <param name="actionModule">The character item action module.</param>
        public void Initialize(ActionModule actionModule)
        {
            m_ActionModule = actionModule;

            if (m_ImpactActions == null) { return; }

            for (int i = 0; i < m_ImpactActions.Length; i++) {
                if(m_ImpactActions[i] == null){ continue; }
                
                if (m_ActionModule != null) {
                    m_ImpactActions[i].Initialize(m_ActionModule.Character, m_ActionModule.CharacterItemAction);
                } else {
                    m_ImpactActions[i].Initialize(null, null);
                }
            }
        }
        
        /// <summary>
        /// Initialize the impact actions.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="characterItemAction">The Item Action that the ImpactAction belongs to.</param>
        public void Initialize(GameObject character, CharacterItemAction characterItemAction)
        {
            m_ActionModule = null;
            
            if (m_ImpactActions == null) { return; }

            for (int i = 0; i < m_ImpactActions.Length; i++) {
                if(m_ImpactActions[i] == null){ continue; }
                
                m_ImpactActions[i].Initialize(character, characterItemAction);
            }
        }
        
        /// <summary>
        /// On impact call invoke all impact actions.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        /// <param name="forceImpact">Force the impact?</param>
        public void OnImpact(ImpactCallbackContext impactCallbackContext, bool forceImpact)
        {
            if (impactCallbackContext == null) {
                Debug.LogError("impactCallbackData should not be null", m_ActionModule?.CharacterItemAction);
                return;
            }

            if (m_ImpactActions == null) { return; }
            
            OnImpactInternal(impactCallbackContext, forceImpact);
        }

        /// <summary>
        /// On impact call invoke all impact actions.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        /// <param name="forceImpact">Force the impact?</param>
        protected virtual void OnImpactInternal(ImpactCallbackContext impactCallbackContext, bool forceImpact)
        {
            for (int i = 0; i < m_ImpactActions.Length; i++) {
                if (m_ImpactActions[i] == null) { continue; }

                m_ImpactActions[i].TryInvokeOnImpact(impactCallbackContext, forceImpact);
            }
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="sourceID">The ID of the cast to reset.</param>
        public virtual void Reset(uint sourceID)
        {
            if(m_ImpactActions == null){ return; }
            
            for (int i = 0; i < m_ImpactActions.Length; i++) {
                if(m_ImpactActions[i] == null){ continue; }
                m_ImpactActions[i].Reset(sourceID);
            }
        }

        /// <summary>
        /// Destroy all impact action objects.
        /// </summary>
        public void OnDestroy()
        {
            if (m_ImpactActions == null) { return; }

            for (int i = 0; i < m_ImpactActions.Length; i++) {
                if(m_ImpactActions[i] == null){ continue; }
                m_ImpactActions[i].OnDestroy();
            }
        }
    }

    /// <summary>
    /// The impact action is the base class for generic actions in the context of an impact usually caused by an item.
    /// </summary>
    [Serializable]
    public abstract class ImpactAction : BoundStateObject
    {
        /// <summary>
        /// A struct containing a scheduled event and a pooled callback context.
        /// Used to avoid changing the context when multiple events are scheduled at the same time with different data.
        /// </summary>
        public struct ImpactScheduledCallbackData
        {
            public ScheduledEventBase Schedule;
            public ImpactCallbackContext ImpactCallbackContext;
        }
        
        [Tooltip("Is the effect enabled?")]
        [SerializeField] protected bool m_Enabled = true;
        [Tooltip("Should the invoke effect after a certain delay.")]
        [SerializeField] protected float m_Delay = 0;
        [Tooltip("If Allow multiple hits is set to false, the action won't be invoked multiple times until reset.")]
        [SerializeField] protected bool m_AllowMultiHits;
        
        public bool Enabled { get => m_Enabled; set => m_Enabled = value; }
        public float Delay { get => m_Delay; set => m_Delay = value; }
        public bool AllowMultiHits { get => m_AllowMultiHits; set => m_AllowMultiHits = value; }
        
        protected override GameObject BoundGameObject => m_CharacterItemAction?.gameObject ?? m_StateBoundGameObject;
        protected CharacterItemAction m_CharacterItemAction;
        
        private Dictionary<uint, HashSet<Transform>> m_ImpactedObjectsMap = new Dictionary<uint, HashSet<Transform>>();
        private HashSet<Transform> m_ImpactedObjects;

        protected Action<ImpactCallbackContext> m_CachedScheduledEventAction;
        protected List<ImpactScheduledCallbackData> m_CachedScheduledEventsCallbacks;

        /// <summary>
        /// Initializes the ImpactAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="characterItemAction">The Item Action that the ImpactAction belongs to.</param>
        public virtual void Initialize(GameObject character, CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
            m_CachedScheduledEventAction = ScheduledInvoke;
            m_CachedScheduledEventsCallbacks = new List<ImpactScheduledCallbackData>();
            base.Initialize(character);
            InitializeInternal();
        }
        
        /// <summary>
        /// Initialize the effect.
        /// </summary>
        protected virtual void InitializeInternal()
        {
            // To be overriden.
        }
        
        /// <summary>
        /// Can the effect be invoked?
        /// </summary>
        /// <returns>True if it can be invoked.</returns>
        public virtual bool CanInvokeOnImpact(ImpactCallbackContext ctx, bool forceImpact)
        {
            return m_Enabled;
        }

        /// <summary>
        /// Invoke the impact action.
        /// </summary>
        /// <param name="ctx">The impact callback data.</param>
        /// <param name="forceImpact">Force the impact?</param>
        public virtual void TryInvokeOnImpact(ImpactCallbackContext ctx, bool forceImpact)
        {
            if (!CanInvokeOnImpact(ctx, forceImpact)) {
                return;
            }
            
            var id = ctx.ImpactCollisionData.SourceID;
            var target = ctx.ImpactCollisionData.ImpactGameObject;
            
            if (!m_ImpactedObjectsMap.TryGetValue(id, out m_ImpactedObjects)) {
                m_ImpactedObjects = new HashSet<Transform>();
                m_ImpactedObjectsMap.Add(id, m_ImpactedObjects);
            }
            
            // Don't call impact if the object has already been impacted by the same ID.
            if (m_ImpactedObjects.Contains(target.transform)) {
                if (!m_AllowMultiHits && !forceImpact) {
                    return;
                }
            } else {
                m_ImpactedObjects.Add(target.transform);
            }

            OnImpact(ctx);
        }
        
        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected virtual void OnImpact(ImpactCallbackContext ctx)
        {
            if (m_Delay <= 0) {
                OnImpactInternal(ctx);
            } else {
                
                if (m_CachedScheduledEventAction == null) {
                    m_CachedScheduledEventAction = ScheduledInvoke;
                }

                if (m_CachedScheduledEventsCallbacks == null) {
                    m_CachedScheduledEventsCallbacks = new List<ImpactScheduledCallbackData>();
                }

                // This is required, because the context could change between the time it was scheduled
                // and the time it was actually invoked
                var ctxDuplicate = ctx.GetPooledDuplicate();

                var schedule = Scheduler.Schedule(m_Delay, m_CachedScheduledEventAction, ctxDuplicate);
                
                // Add the context and schedule in a list such that they can be canceled later if necessary.
                m_CachedScheduledEventsCallbacks.Add(new ImpactScheduledCallbackData()
                {
                    ImpactCallbackContext = ctxDuplicate,
                    Schedule = schedule
                });
            }
        }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected void ScheduledInvoke(ImpactCallbackContext ctx)
        {
            if (m_CachedScheduledEventsCallbacks != null) {
                for (int i = 0; i < m_CachedScheduledEventsCallbacks.Count; i++) {
                    if (m_CachedScheduledEventsCallbacks[i].ImpactCallbackContext == ctx) {
                        // Found match.
                        m_CachedScheduledEventsCallbacks.RemoveAt(i);
                        break;
                    }
                }
            }

            OnImpactInternal(ctx);
            
            // The context must be returned to the pool.
            if (ctx != null) {
                ctx.ReturnToPool();
            }
        }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected abstract void OnImpactInternal(ImpactCallbackContext ctx);

        /// <summary>
        /// Has the specified object been impacted?
        /// </summary>
        /// <param name="obj">The object that may have been impacted.</param>
        /// <returns>True if the specified object has been impacted.</returns>
        protected bool HasImpacted(Transform obj)
        {
            if (m_ImpactedObjects == null) {
                return false;
            }
            return m_ImpactedObjects.Contains(obj);
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="sourceID">The ID of the cast to reset.</param>
        public virtual void Reset(uint sourceID)
        {
            if (m_ImpactedObjectsMap.TryGetValue(sourceID, out var impactedObjects)) {
                impactedObjects.Clear();
            }
        }

        /// <summary>
        /// The action has been destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            // Cancel all scheduled events.
            if (m_CachedScheduledEventsCallbacks != null) {
                for (int i = 0; i < m_CachedScheduledEventsCallbacks.Count; i++) {
                    
                    // The scheduled event must be canceled
                    var scheduledEventBase = m_CachedScheduledEventsCallbacks[i].Schedule;
                    if (scheduledEventBase != null) {
                        Scheduler.Cancel(scheduledEventBase);
                    }
                    
                    // The context must be returned to the pool.
                    var pooledImpactCallback = m_CachedScheduledEventsCallbacks[i].ImpactCallbackContext;
                    if (pooledImpactCallback != null) {
                        pooledImpactCallback.ReturnToPool();
                    }
                }
                m_CachedScheduledEventsCallbacks.Clear();
            }
        }

        /// <summary>
        /// To string writes the type name.
        /// </summary>
        /// <returns>The string name.</returns>
        public override string ToString()
        {
            return GetType()?.Name ?? "(null)";
        }
    }
}