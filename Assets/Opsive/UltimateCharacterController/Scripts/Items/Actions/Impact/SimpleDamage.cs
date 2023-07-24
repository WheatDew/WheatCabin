/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// Spawn Surface effects an impact.
    /// </summary>
    [Serializable]
    public class SpawnSurfaceEffect : ImpactAction
    {
        [Tooltip("Use the impact damage data from the context if it is possible?")]
        [SerializeField] protected bool m_UseContextData;
        [Tooltip("The Surface Impact triggered when the weapon hits an object.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;

        public bool UseContextData { get => m_UseContextData; set => m_UseContextData = value; }
        public SurfaceImpact SurfaceImpact { get { return m_SurfaceImpact; } set { m_SurfaceImpact = value; } }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SpawnSurfaceEffect() { }

        /// <summary>
        /// Overloaded constructor with use context data.
        /// </summary>
        /// <param name="useContextData">Use the context data rather than the local data?</param>
        public SpawnSurfaceEffect(bool useContextData) { m_UseContextData = useContextData; }
        
        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var impactData = ctx.ImpactCollisionData;
            var surfaceImpact = (!m_UseContextData || ctx.ImpactDamageData == null || ctx.ImpactDamageData.SurfaceImpact == null) ? m_SurfaceImpact : ctx.ImpactDamageData.SurfaceImpact;
            var originator = impactData.SourceItemAction?.CharacterItem?.GetVisibleObject() ?? impactData.SourceGameObject;
            
            // The surface manager will apply effects based on the type of impact.
            SurfaceManager.SpawnEffect(
                impactData.RaycastHit, 
                surfaceImpact, 
                impactData.SourceCharacterLocomotion?.GravityDirection ?? Vector3.up, 
                impactData.SourceCharacterLocomotion?.TimeScale ?? 1,
                originator);
        }
    }
    
    /// <summary>
    /// An impact action that sets a state on the impacted object.
    /// </summary>
    [Serializable]
    public class StateImpact : ImpactAction
    {
        [Tooltip("Use the impact damage data from the context if it is possible?")]
        [SerializeField] protected bool m_UseContextData;
        [Tooltip("The name of the state to activate upon impact.")]
        [SerializeField] protected string m_ImpactStateName;
        [Tooltip("The number of seconds until the impact state is disabled. A value of -1 will require the state to be disabled manually.")]
        [SerializeField] protected float m_ImpactStateDisableTimer = 10;
        
        public bool UseContextData { get => m_UseContextData; set => m_UseContextData = value; }
        public string ImpactStateName { get { return m_ImpactStateName; } set { m_ImpactStateName = value; } }
        public float ImpactStateDisableTimer { get { return m_ImpactStateDisableTimer; } set { m_ImpactStateDisableTimer = value; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StateImpact() { }

        /// <summary>
        /// Overloaded constructor with use context data.
        /// </summary>
        /// <param name="useContextData">Use the context data rather than the local data?</param>
        public StateImpact(bool useContextData) { m_UseContextData = useContextData; }
        
        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var impactData = ctx.ImpactCollisionData;
            
            var stateName = m_ImpactStateName;
            var disabletimer = m_ImpactStateDisableTimer;
            if (m_UseContextData && ctx.ImpactDamageData != null) {
                stateName = ctx.ImpactDamageData.ImpactStateName;
                disabletimer = ctx.ImpactDamageData.ImpactStateDisableTimer;
            }

            // An optional state can be activated on the hit object.
            if (!string.IsNullOrEmpty(stateName)) {
                var characterLocomotion = impactData.ImpactGameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
                StateManager.SetState(characterLocomotion != null ? characterLocomotion.gameObject : impactData.ImpactGameObject, stateName, true);
                // If the timer isn't -1 then the state should be disabled after a specified amount of time. If it is -1 then the state
                // will have to be disabled manually.
                if (disabletimer != -1) {
                    StateManager.DeactivateStateTimer(impactData.ImpactGameObject, stateName, disabletimer);
                }
            }
        }
    }

    /// <summary>
    /// An impact module used to deal damage and forces to the target.
    /// </summary>
    [Serializable]
    public class SimpleDamage : ImpactAction
    {
        [Tooltip("Use the impact damage data from the context if it is possible?")]
        [SerializeField] protected bool m_UseContextData;
        [Tooltip("Use the impact damage data from the context if it is possible?")]
        [SerializeField] protected bool m_SetDamageImpactData = true;
        [Tooltip("Use the impact damage data from the context if it is possible?")]
        [SerializeField] protected bool m_InvokeOnObjectImpact;
        [Tooltip("Processes the damage dealt to a Damage Target.")]
        [SerializeField] protected DamageProcessor m_DamageProcessor;
        [Tooltip("The amount of damage to apply to the hit object.")]
        [SerializeField] protected float m_DamageAmount = 10;
        [Tooltip("The amount of force to apply to the hit object.")]
        [SerializeField] protected float m_ImpactForce = 2;
        [Tooltip("The number of frames to add the impact force to.")]
        [SerializeField] protected int m_ImpactForceFrames = 15;
        [Tooltip("The impact radius.")]
        [SerializeField] protected float m_ImpactRadius;
        [Tooltip("Should the damage be scaled by the impact strength? Use this for explosions!")]
        [SerializeField] protected bool m_ScaleDamageByImpactStrength = false;
        
        public bool UseContextData { get => m_UseContextData; set => m_UseContextData = value; }
        public DamageProcessor DamageProcessor { get { return m_DamageProcessor; } set { m_DamageProcessor = value; } }
        public float DamageAmount { get { return m_DamageAmount; } set { m_DamageAmount = value; } }
        public float ImpactForce { get { return m_ImpactForce; } set { m_ImpactForce = value; } }
        public int ImpactForceFrames { get { return m_ImpactForceFrames; } set { m_ImpactForceFrames = value; } }

        protected ImpactDamageData m_CachedImpactDamageData;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SimpleDamage() { }

        /// <summary>
        /// Overloaded constructor with use context data.
        /// </summary>
        /// <param name="useContextData">Use the context data rather than the local data?</param>
        public SimpleDamage(bool useContextData) { m_UseContextData = useContextData; }
        
        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var impactData = ctx.ImpactCollisionData;
            
            var damageAmount = m_DamageAmount;
            var damageProcessor = m_DamageProcessor;
            var impactForce = m_ImpactForce;
            var impactforceframes = m_ImpactForceFrames;
            var radius = m_ImpactRadius;
            if (m_UseContextData && ctx.ImpactDamageData != null) {
                damageAmount = ctx.ImpactDamageData.DamageAmount;
                damageProcessor = ctx.ImpactDamageData.DamageProcessor;
                impactForce = ctx.ImpactDamageData.ImpactForce;
                impactforceframes = ctx.ImpactDamageData.ImpactForceFrames;
                radius = ctx.ImpactDamageData.ImpactRadius;
            }

            if (m_ScaleDamageByImpactStrength) {
                damageAmount *= impactData.ImpactStrength;
            }

            // The shield can absorb some (or none) of the damage from the hitscan.
            var shieldCollider = impactData.ImpactCollider.gameObject.GetCachedComponent<ShieldCollider>();
            if (shieldCollider != null) {
                damageAmount = shieldCollider.ShieldAction.Damage(ctx, damageAmount);
            }

            var impactForceMagnitude = impactForce * impactData.ImpactStrength;
            var impactDirectionalForce = impactForceMagnitude * impactData.ImpactDirection;

            var target = impactData.ImpactGameObject;
            var damageTarget = DamageUtility.GetDamageTarget(impactData.ImpactGameObject);
            if (damageTarget != null) {
                target = damageTarget.HitGameObject;
                
                // If the shield didn't absorb all of the damage then it should be applied to the character.
                if (damageAmount > 0) {
                    // First get the damage data and initialize it.
                    var pooledDamageData = GenericObjectPool.Get<DamageData>();
                    pooledDamageData.SetDamage(ctx, damageAmount, impactData.ImpactPosition,
                        impactData.ImpactDirection, impactForceMagnitude, impactforceframes,
                        radius, impactData.ImpactCollider);
                    pooledDamageData.DamageSource = impactData.SourceComponent as IDamageSource;
                    
                    // Then find how to apply this damage data, through a damage processor or processor module.
                    var damageProcessorModule = impactData.SourceCharacterLocomotion?.gameObject?.GetCachedComponent<DamageProcessorModule>();
                    if (damageProcessorModule != null) {
                        damageProcessorModule.ProcessDamage(damageProcessor, damageTarget, pooledDamageData);
                    } else {
                        if (damageProcessor == null) { damageProcessor = DamageProcessor.Default; }
                        damageProcessor.Process(damageTarget, pooledDamageData);
                    }

                    GenericObjectPool.Return(pooledDamageData);
                }
            } else {
                var forceObject = impactData.ImpactGameObject.GetCachedParentComponent<IForceObject>();
                if (forceObject != null) {
                    forceObject.AddForce(impactDirectionalForce);
                } else if(impactForceMagnitude > 0 && impactData.ImpactRigidbody != null && !impactData.ImpactRigidbody.isKinematic) {
                    // If the damage target exists it will apply a force to the rigidbody in addition to procesing the damage.
                    // Otherwise just apply the force to the rigidbody. If the radius is bigger than 0 than it must be explosive.
                    if (radius > 0) {
                        impactData.ImpactRigidbody.AddExplosionForce(impactForceMagnitude * MathUtility.RigidbodyForceMultiplier, impactData.ImpactPosition, radius);
                    } else {
                        impactData.ImpactRigidbody.AddForceAtPosition(impactDirectionalForce * MathUtility.RigidbodyForceMultiplier, impactData.ImpactPosition);
                    }
                }
            }
            
            // Set the Damage Impact data to the context.
            if (m_SetDamageImpactData) {
                
                // Create a new damage data to avoid changing the original one.
                if (m_CachedImpactDamageData == null) {
                    m_CachedImpactDamageData = new ImpactDamageData();
                }

                if (ctx.ImpactDamageData != null) {
                    m_CachedImpactDamageData.Copy(ctx.ImpactDamageData);
                }
                var ctxImpactData = m_CachedImpactDamageData;

                ctx.ImpactCollisionData.ImpactGameObject = target;
                ctxImpactData.DamageAmount = damageAmount;
                ctxImpactData.DamageProcessor = damageProcessor;
                ctxImpactData.ImpactForce = impactForce;
                ctxImpactData.ImpactForceFrames = impactforceframes;
                ctxImpactData.ImpactRadius = radius;
                    
                ctx.ImpactDamageData = ctxImpactData;
            }

            // Send the event to the collider and its rigidbody.
            if (m_InvokeOnObjectImpact) {
                target = ctx.ImpactCollisionData.ImpactGameObject;

                var collider = ctx.ImpactCollisionData.ImpactCollider;
                if (collider != null) {
                    ctx.ImpactCollisionData.ImpactGameObject = collider.gameObject;
                    ctx.InvokeImpactTargetCallback();
                    if (collider.attachedRigidbody != null &&
                        collider.attachedRigidbody.gameObject != collider.gameObject) {
                        ctx.ImpactCollisionData.ImpactGameObject = collider.attachedRigidbody.gameObject;
                        ctx.InvokeImpactTargetCallback();
                    }
                    
                    if (target != collider.gameObject) {
                        ctx.ImpactCollisionData.ImpactGameObject = target;
                        ctx.InvokeImpactTargetCallback();
                    }
                } else {
                    ctx.InvokeImpactTargetCallback();
                }
                
                ctx.ImpactCollisionData.ImpactGameObject = target;
            }
        }
    }
}