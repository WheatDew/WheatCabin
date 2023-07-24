/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    
    /// <summary>
    /// The impact callback data contains impact data and a character item action which will listen to nay callbacks.
    /// </summary>
    public class ImpactCallbackContext
    {
        protected ImpactCollisionData m_ImpactCollisionData;
        protected CharacterItemAction m_CharacterItemAction;
        protected IImpactDamageData m_ImpactDamageData;

        public virtual CharacterItemAction CharacterItemAction => m_CharacterItemAction;
        public ImpactCollisionData ImpactCollisionData { get => m_ImpactCollisionData; set => m_ImpactCollisionData = value; }
        public IImpactDamageData ImpactDamageData { get => m_ImpactDamageData; set => m_ImpactDamageData = value; }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImpactCallbackContext() { }

        /// <summary>
        /// Construcor with the character item action that will receive callbacks.
        /// </summary>
        /// <param name="characterItemAction">The character item action that will receive callbacks.</param>
        public ImpactCallbackContext(CharacterItemAction characterItemAction)
        {
            SetCharacterItemAction(characterItemAction);
        }

        /// <summary>
        /// Set the character item action that will receive callbacks.
        /// </summary>
        /// <param name="characterItemAction">The character item action that will receive callbacks.</param>
        public virtual void SetCharacterItemAction(CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
        }

        /// <summary>
        /// Reset the data.
        /// </summary>
        public virtual void Reset()
        {
            m_ImpactCollisionData = null;
        }

        /// <summary>
        /// Invoke an interrupt callback on the character initiator.
        /// </summary>
        /// <param name="impactAction">A reference to the Impact Actiuon.</param>
        public virtual void InvokeInterruptCallback(ImpactAction impactAction)
        {
            var originator = m_ImpactCollisionData.SourceGameObject;
            if (originator == null) {
                Debug.LogWarning("Cannot call Impact event on a null originator.");
                return;
            }
            EventHandler.ExecuteEvent<ImpactCallbackContext, ImpactAction>(originator, "ImpactInteruptedCallback", this, impactAction);
        }

        /// <summary>
        /// Invoke an impact callback on the originator.
        /// </summary>
        public virtual void InvokeImpactOriginatorCallback()
        {
            var originator = m_ImpactCollisionData.SourceGameObject;
            if (originator == null) {
                Debug.LogWarning("Cannot call Impact event on a null originator.");
                return;
            }
            EventHandler.ExecuteEvent<ImpactCallbackContext>(originator, "OnObjectImpactSourceCallback", this);
        }
        
        /// <summary>
        /// Invoked an impact callback on the target.
        /// </summary>
        public virtual void InvokeImpactTargetCallback()
        {
            var target = m_ImpactCollisionData.ImpactGameObject;
            if (target == null) {
                Debug.LogWarning("Cannot call Impact event on a null target.");
                return;
            }
            EventHandler.ExecuteEvent<ImpactCallbackContext>(target, "OnObjectImpact", this);
        }
        
        /// <summary>
        /// String format to visualize the data.
        /// </summary>
        /// <returns>string format.</returns>
        public override string ToString()
        {
            return "Impact Callback Context:\n  " +
                   "Character Item Action: " + (m_CharacterItemAction == null? "(null)" : m_CharacterItemAction) + "\n  " +
                   m_ImpactCollisionData + "\n  " +
                   "Damage Data: "+(ImpactDamageData == null? "(null)" : ImpactDamageData);
        }

        
        /// <summary>
        /// Get a duplicate version of this object that is pooled pooled.
        /// </summary>
        /// <returns>The pooled duplicate.</returns>
        public virtual ImpactCallbackContext GetPooledDuplicate()
        {
            var duplicate = GenericObjectPool.Get<ImpactCallbackContext>();
            duplicate.PooledCopy(this);
            return duplicate;
        }

        /// <summary>
        /// Copy the contents of another impact callback context.
        /// </summary>
        /// <param name="other">The other callback context to copy the data from.</param>
        protected virtual void PooledCopy(ImpactCallbackContext other)
        {
            m_ImpactCollisionData = GenericObjectPool.Get<ImpactCollisionData>();
            m_ImpactCollisionData.Copy(other.ImpactCollisionData);
            m_ImpactDamageData = GenericObjectPool.Get<ImpactDamageData>();
            m_ImpactDamageData.Copy(other.ImpactDamageData);

            m_CharacterItemAction = other.CharacterItemAction;
        }

        /// <summary>
        /// Return this object to the pool.
        /// </summary>
        public virtual void ReturnToPool()
        {
            GenericObjectPool.Return(m_ImpactCollisionData);
            GenericObjectPool.Return(ImpactDamageData);
            GenericObjectPool.Return(this);
        }
    }

    /// <summary>
    /// The Impact data contains all relevant data about an impact.
    /// </summary>
    [Serializable]
    public class ImpactCollisionData
    {
        protected bool m_Initialized;
        public bool Initialized => m_Initialized;
        
        protected uint m_SourceID;
        protected LayerMask m_LayerMask;
        protected RaycastHit m_RaycastHit;
        protected Vector3 m_ImpactPosition;
        protected GameObject m_ImpactGameObject;
        protected Rigidbody m_ImpactRigidbody;
        protected Collider m_ImpactCollider;
        protected Vector3 m_ImpactDirection;
        protected float m_ImpactStrength;
        protected IDamageTarget m_DamageTarget;
        protected IDamageSource m_DamageSource;
        protected Component m_SourceComponent;
        protected GameObject m_SourceGameObject;
        protected CharacterLocomotion m_SourceCharacterLocomotion;
        protected int m_HitCount;
        protected ListSlice<Collider> m_HitColliders;
        protected UsableAction m_SourceItemAction;
        protected SurfaceImpact m_SurfaceImpact;

        private DefaultDamageSource m_CachedDamageSource = new DefaultDamageSource();

        /// <summary>
        /// The source id is used to identify the cause of the impact. For example it can be set to the hitbox index, magic cast index, etc.
        /// </summary>
        public uint SourceID { get => m_SourceID; set => m_SourceID = value; }
        /// <summary>
        /// Most impact have raycast hit.
        /// </summary>
        public RaycastHit RaycastHit { get => m_RaycastHit; set => m_RaycastHit = value; }
        /// <summary>
        /// The position the impact happened.
        /// </summary>
        public Vector3 ImpactPosition { get => m_ImpactPosition; set => m_ImpactPosition = value; }
        /// <summary>
        /// The gameobject that was impacted by the collision.
        /// </summary>
        public GameObject ImpactGameObject { get => m_ImpactGameObject; set => m_ImpactGameObject = value; }
        /// <summary>
        /// The rigidbody of the gameobject that was impacted.
        /// </summary>
        public Rigidbody ImpactRigidbody { get => m_ImpactRigidbody; set => m_ImpactRigidbody = value; }
        /// <summary>
        /// The collider that was impacted.
        /// </summary>
        public Collider ImpactCollider { get => m_ImpactCollider; set => m_ImpactCollider = value; }
        /// <summary>
        /// The direction of the impact.
        /// </summary>
        public Vector3 ImpactDirection { get => m_ImpactDirection; set => m_ImpactDirection = value; }
        /// <summary>
        /// The strength of the impact, this can be used as a multiplier for force or damage.
        /// </summary>
        public float ImpactStrength { get => m_ImpactStrength; set => m_ImpactStrength = value; }
        /// <summary>
        /// The damage target.
        /// </summary>
        public IDamageTarget DamageTarget { get => m_DamageTarget; set => m_DamageTarget = value; }
        /// <summary>
        /// The damage originator.
        /// </summary>
        public IDamageSource DamageSource { get => m_DamageSource; set => m_DamageSource = value; }
        /// <summary>
        /// The Component that caused the impact.
        /// </summary>
        public Component SourceComponent { get=> m_SourceComponent; set=> m_SourceComponent = value; }
        /// <summary>
        /// The GameObject that caused the impact.
        /// </summary>
        public GameObject SourceGameObject { get => m_SourceGameObject; set => m_SourceGameObject = value; }
        /// <summary>
        /// The character locomotion of the character that caused the impact.
        /// </summary>
        public CharacterLocomotion SourceCharacterLocomotion { get => m_SourceCharacterLocomotion; set => m_SourceCharacterLocomotion = value; }
        /// <summary>
        /// The Item Action that caused the impact
        /// </summary>
        public UsableAction SourceItemAction { get=> m_SourceItemAction; set=> m_SourceItemAction = value; }
        /// <summary>
        /// The surface impact.
        /// </summary>
        public SurfaceImpact SurfaceImpact { get=> m_SurfaceImpact; set => m_SurfaceImpact = value; }
        /// <summary>
        /// The layers that were used as layer mask to detect that collision.
        /// </summary>
        public LayerMask DetectLayers { get=> m_LayerMask; set=> m_LayerMask = value; }
        /// <summary>
        /// Count of the colliders that were detected during the collision
        /// </summary>
        public int HitCount { get=> m_HitCount; set=> m_HitCount = value; }
        /// <summary>
        /// All the colliders that were detected during the collision
        /// </summary>
        public ListSlice<Collider> HitColliders { get=> m_HitColliders; set=> m_HitColliders = value; }

        /// <summary>
        /// Reset the data such that the object can be reused.
        /// </summary>
        public virtual void Reset()
        {
            m_Initialized = false;

            m_SourceID = 0;
            m_ImpactPosition = Vector3.zero;
            m_ImpactGameObject = null;
            m_ImpactRigidbody = null;
            m_ImpactCollider = null;
            m_ImpactDirection = Vector3.zero;
            m_ImpactStrength = 0;
            m_SourceComponent = null;
            m_SourceGameObject = null;
            m_SourceCharacterLocomotion = null;
            m_SourceItemAction = null;
            m_RaycastHit = default;
            m_LayerMask = 0;
            m_HitCount = -1;
            m_HitColliders = default;
            m_DamageTarget = null;
            m_DamageSource = null;
            m_SurfaceImpact = null;
        }

        /// <summary>
        /// Copy the Impact collision dat from another impact collision data.
        /// </summary>
        /// <param name="other">The other impact collision data to copy.</param>
        public virtual void Copy(ImpactCollisionData other)
        {
            m_SourceID = other.SourceID;
            m_ImpactPosition = other.ImpactPosition;
            m_ImpactGameObject = other.ImpactGameObject;
            m_ImpactRigidbody = other.ImpactRigidbody;
            m_ImpactCollider = other.ImpactCollider;
            m_ImpactDirection = other.ImpactDirection;
            m_ImpactStrength = other.ImpactStrength;
            m_SourceComponent = other.SourceComponent;
            m_SourceGameObject = other.SourceGameObject;
            m_SourceCharacterLocomotion = other.SourceCharacterLocomotion;
            m_SourceItemAction = other.SourceItemAction;
            m_RaycastHit = other.RaycastHit;
            m_LayerMask = other.DetectLayers;
            m_HitCount = other.HitCount;
            m_HitColliders = other.HitColliders;
            m_DamageTarget = other.DamageTarget;
            m_DamageSource = other.DamageSource;
            m_SurfaceImpact = other.SurfaceImpact;
        }

        /// <summary>
        /// Initialize the data before it can be used again.
        /// </summary>
        public virtual void Initialize()
        {
            m_Initialized = true;
        }

        /// <summary>
        /// Set the raycast that defines the impact data.
        /// </summary>
        /// <param name="hit">The raycast hit.</param>
        public virtual void SetRaycast(RaycastHit hit)
        {
            m_RaycastHit = hit;
            m_ImpactPosition = hit.point;
            m_ImpactDirection = -hit.normal;
            
            SetImpactTarget(hit.collider);
        }
        
        /// <summary>
        /// Set the impact origin by specifying the item action that caused it.
        /// </summary>
        /// <param name="sourceItemAction">The usable action that caused the impact.</param>
        public void SetImpactSource(UsableAction sourceItemAction)
        {
            m_DamageSource = sourceItemAction;
            m_SourceComponent = sourceItemAction;
            m_SourceGameObject = sourceItemAction.gameObject;
            m_SourceItemAction = sourceItemAction;
            m_SourceCharacterLocomotion = sourceItemAction.CharacterLocomotion;
        }
        
        /// <summary>
        /// Set the impact origin.
        /// </summary>
        /// <param name="sourceComponent">The component that caused the impact.</param>
        /// <param name="sourceCharacter">The character that owns the component that caused the impact.</param>
        public void SetImpactSource(Component sourceComponent, GameObject sourceCharacter)
        {
            m_SourceComponent = sourceComponent;
            m_SourceGameObject = sourceComponent.gameObject;
            m_DamageSource = sourceComponent as IDamageSource;
            if (m_DamageSource == null) {
                m_DamageSource = sourceComponent.gameObject.GetCachedComponent<IDamageSource>();
            }
            m_SourceItemAction = sourceComponent as UsableAction;
            m_SourceCharacterLocomotion = sourceCharacter?.GetCachedComponent<CharacterLocomotion>();
            
            if (DamageSource == null) {
                m_CachedDamageSource.Reset();
                m_CachedDamageSource.SourceComponent = sourceComponent;
                m_CachedDamageSource.SourceGameObject = SourceGameObject;
                if (sourceCharacter == null) {
                    m_CachedDamageSource.SourceOwner = SourceGameObject;
                } else {
                    m_CachedDamageSource.SourceOwner = sourceCharacter;
                }
                m_DamageSource = m_CachedDamageSource;
            }
        }
        
        /// <summary>
        /// Set the impact origin.
        /// </summary>
        /// <param name="sourceGameObject">The originator gameobject.</param>
        /// <param name="sourceCharacter">The character originator gameobject.</param>
        public void SetImpactSource(GameObject sourceGameObject, GameObject sourceCharacter)
        {
            m_SourceGameObject = sourceGameObject;
            m_SourceItemAction = sourceGameObject.GetCachedComponent<UsableAction>();

            if (SourceItemAction != null) {
                m_DamageSource = SourceItemAction;
            } else {
                m_DamageSource = sourceGameObject.GetCachedComponent<IDamageSource>();
            }
            if (m_DamageSource is Component component) {
                SourceComponent = component;
            } else {
                SourceComponent = SourceItemAction;
            }
            m_SourceCharacterLocomotion = sourceCharacter?.GetCachedComponent<CharacterLocomotion>();
            
            if (DamageSource == null) {
                m_CachedDamageSource.Reset();
                m_CachedDamageSource.SourceComponent = SourceComponent;
                m_CachedDamageSource.SourceGameObject = SourceGameObject;
                if (sourceCharacter == null) {
                    m_CachedDamageSource.SourceOwner = SourceGameObject;
                } else {
                    m_CachedDamageSource.SourceOwner = sourceCharacter;
                }
                m_DamageSource = m_CachedDamageSource;
            }
        }
        
        /// <summary>
        /// Set the impact origin by specifying the damage originator and optionally the character originator.
        /// </summary>
        /// <param name="damageSource">The damage originator that caused the impact.</param>
        public void SetImpactSource(IDamageSource damageSource)
        {
            m_DamageSource = damageSource;
            if (DamageSource == null) {
                m_CachedDamageSource.Reset();
                m_DamageSource = m_CachedDamageSource;
                m_SourceComponent = null;
                m_SourceGameObject = null;
                m_SourceItemAction = null;
                m_SourceCharacterLocomotion = null;
                return;
            }
            
            m_SourceGameObject = damageSource.SourceGameObject;
            m_SourceComponent = damageSource.SourceComponent;
            
            if (DamageSource is UsableAction itemAction) {
                m_SourceItemAction = itemAction;
            } else {
                m_SourceItemAction = m_SourceGameObject.GetCachedComponent<UsableAction>();
            }
            if (m_SourceComponent == null) {
                m_SourceComponent = SourceItemAction;
            }

            damageSource.TryGetCharacterLocomotion(out m_SourceCharacterLocomotion);
        }

        /// <summary>
        /// Set the Impact target by specifying the impact collider and the impact gameobject.
        /// </summary>
        /// <param name="impactCollider">The collider that was impacted.</param>
        /// <param name="impactGameObject">The gameobject that was impacted (if null the attached rigid body is used).</param>
        public void SetImpactTarget(Collider impactCollider, GameObject impactGameObject = null)
        {
            m_ImpactCollider = impactCollider;
            m_ImpactRigidbody = impactCollider.attachedRigidbody;
            m_ImpactGameObject = impactGameObject == null ? impactCollider.gameObject : impactGameObject;
            m_DamageTarget = DamageUtility.GetDamageTarget(ImpactGameObject);
        }

        /// <summary>
        /// To string in an easy to read format
        /// </summary>
        /// <returns>Easy to read format.</returns>
        public override string ToString()
        {
            return "Impact Data: \n\t" +
                   "Source ID: " + m_SourceID + "\n\t" +
                   "\n\tIMPACT \n\t" +
                   "RaycastHit: " + m_RaycastHit + "\n\t"+
                   "Impact Position: " + m_ImpactPosition + "\n\t" +
                   "Impact GameObject: " + m_ImpactGameObject + "\n\t" +
                   "Impact Rigidbody: " + ImpactRigidbody + "\n\t"+
                   "Impact Collider: " + m_ImpactCollider + "\n\t"+
                   "Impact Direction: " + ImpactDirection + "\n\t"+
                   "Impact Strength: " + m_ImpactStrength + "\n\t"+
                   "DetectLayers: " + m_LayerMask + "\n\t"+
                   "HitCount: " + m_HitCount + "\n\t"+
                   "HitColliders: " + m_HitColliders.ToStringDeep()+ "\n\t"+
                   "SurfaceImpact: " + m_SurfaceImpact + "\n\t"+
                   "DamageTarget: " + m_DamageTarget + "\n\t"+
                   "\n\tSOURCE \n\t" +
                   "Source Component: " + m_SourceComponent + "\n\t"+
                   "Originator: " + m_SourceGameObject + "\n\t"+
                   "Character Locomotion Initiator: " + m_SourceCharacterLocomotion + "\n\t"+
                   "Item Action Initiator: " + m_SourceItemAction + "\n\t"+
                   "DamageOriginator: " + m_DamageSource + "\n\t";
        }
    }

    /// <summary>
    /// An interface that contains information about the damage caused by an impact.
    /// </summary>
    public interface IImpactDamageData
    {
        LayerMask LayerMask { get; set; }
        DamageProcessor DamageProcessor { get; set; }
        float DamageAmount { get; set; }
        float ImpactForce { get; set; }
        int ImpactForceFrames { get; set; }
        float ImpactRadius { get; set; }
        string ImpactStateName { get; set; }
        float ImpactStateDisableTimer { get; set; }
        SurfaceImpact SurfaceImpact { get; set; }

        void Copy(IImpactDamageData other);

        void Reset();
    }
    
    /// <summary>
    /// The default impact damage data class.
    /// </summary>
    [Serializable]
    public class ImpactDamageData : IImpactDamageData
    {
        [Tooltip("The Layer mask to which deal damage.")]
        [SerializeField] protected LayerMask m_LayerMask =
            ~( 1 << LayerManager.IgnoreRaycast
              | 1 << LayerManager.Water 
              | 1 << LayerManager.SubCharacter 
              | 1 << LayerManager.Overlay 
              | 1 << LayerManager.VisualEffect);
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
        [Tooltip("The name of the state to activate upon impact.")]
        [SerializeField] protected string m_ImpactStateName;
        [Tooltip("The number of seconds until the impact state is disabled. A value of -1 will require the state to be disabled manually.")]
        [SerializeField] protected float m_ImpactStateDisableTimer = 10;
        [Tooltip("The Surface Impact defines what effects happen on impact.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;
        
        public LayerMask LayerMask { get => m_LayerMask; set => m_LayerMask = value; }
        public DamageProcessor DamageProcessor { get => m_DamageProcessor; set => m_DamageProcessor = value; }
        public float DamageAmount { get => m_DamageAmount; set => m_DamageAmount = value; }
        public float ImpactForce { get => m_ImpactForce; set => m_ImpactForce = value; }
        public int ImpactForceFrames { get => m_ImpactForceFrames; set => m_ImpactForceFrames = value; }
        public float ImpactRadius { get => m_ImpactRadius; set => m_ImpactRadius = value; }
        public string ImpactStateName { get => m_ImpactStateName; set => m_ImpactStateName = value; }
        public float ImpactStateDisableTimer { get => m_ImpactStateDisableTimer; set => m_ImpactStateDisableTimer = value; }
        public SurfaceImpact SurfaceImpact { get => m_SurfaceImpact; set => m_SurfaceImpact = value; }

        /// <summary>
        /// Copy all the values of the another impact damage data.
        /// </summary>
        /// <param name="other">The other impact data data.</param>
        public void Copy(IImpactDamageData other)
        {
            m_LayerMask = other.LayerMask;
            m_DamageProcessor = other.DamageProcessor;
            m_DamageAmount = other.DamageAmount;
            m_ImpactForce = other.ImpactForce;
            m_ImpactForceFrames = other.ImpactForceFrames;
            m_ImpactRadius = other.ImpactRadius;
            m_ImpactStateName = other.ImpactStateName;
            m_ImpactStateDisableTimer = other.ImpactStateDisableTimer;
            m_SurfaceImpact = other.SurfaceImpact;
        }

        /// <summary>
        /// Reset the damage data.
        /// </summary>
        public void Reset()
        {
            m_LayerMask = -1;
            m_DamageProcessor = null;
            m_DamageAmount = 0;
            m_ImpactForce = 0;
            m_ImpactForceFrames = 0;
            m_ImpactRadius = 0;
            m_ImpactStateName = null;
            m_ImpactStateDisableTimer = 0;
            m_SurfaceImpact = null;
        }

        /// <summary>
        /// Returns a new string value.
        /// </summary>
        /// <returns>The new string value.</returns>
        public override string ToString()
        {
            return "Impact Damage Data: \n\t" +
                   "Layer Mask: " + m_LayerMask + "\n\t" +
                   "Damage Processor: " + m_DamageProcessor + "\n\t" +
                   "Damage Amount: " + m_DamageAmount + "\n\t" +
                   "Impact Force: " + m_ImpactForce + "\n\t" +
                   "Impact Force Frames: " + m_ImpactForceFrames + "\n\t" +
                   "Impact Radius: " + m_ImpactRadius + "\n\t" +
                   "Impact State Name: " + m_ImpactStateName + "\n\t" +
                   "Impact State Disable Timer: " + m_ImpactStateDisableTimer + "\n\t" +
                   "Surface Impact: " + m_SurfaceImpact + "\n\t";
        }
    }
}