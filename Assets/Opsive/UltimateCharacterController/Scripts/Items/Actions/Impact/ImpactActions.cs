/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using Opsive.Shared.Audio;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    [Serializable]
    public class ImpactCallbackUnityEvent : UnityEvent<ImpactCallbackContext>{ }
    
    /// <summary>
    /// This action will print the impact context in the console. 
    /// </summary>
    [Serializable]
    public class DebugImpactContext : ImpactAction
    {
        [SerializeField] protected string m_Message;
        
        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            Debug.Log(m_Message+"\n"+ctx);
        }
    }
    
    /// <summary>
    /// Calls a unity event on impact.
    /// </summary>
    [Serializable]
    public class ImpactUnityEvent : ImpactAction
    {
        [Tooltip("Unity event invoked when the destructable hits another object.")]
        [SerializeField] protected ImpactCallbackUnityEvent m_OnImpactEvent;

        public ImpactCallbackUnityEvent OnImpactEvent { get { return m_OnImpactEvent; } set { m_OnImpactEvent = value; } }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            if (m_OnImpactEvent != null) {
                m_OnImpactEvent.Invoke(ctx);
            }
        }
    }
    
    /// <summary>
    /// Calls a events on impact.
    /// </summary>
    [Serializable]
    public class ImpactEvent : ImpactAction
    {
        [Tooltip("Call the impact callback event on the originator?")]
        [SerializeField] protected bool m_CallImpactCallbackOnOriginator = true;
        [Tooltip("Call the impact callback event on the target?")]
        [SerializeField] protected bool m_CallImpactCallbackOnTarget = true;

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            if (m_CallImpactCallbackOnOriginator) {
                ctx.InvokeImpactOriginatorCallback();
            }

            if (m_CallImpactCallbackOnTarget) {
                ctx.InvokeImpactTargetCallback();
            }
        }
    }
    
    /// <summary>
    /// Generic Item Effects invoked when the impact happens
    /// </summary>
    [Serializable]
    public class GenericItemEffects : ImpactAction
    {
        [Tooltip("A list of effects to invoke.")]
        [SerializeField] protected ItemEffectGroup m_EffectGroup;

        public ItemEffectGroup EffectGroup
        {
            get => m_EffectGroup;
            set
            {
                m_EffectGroup = value;
                if (m_EffectGroup != null) {
                    m_EffectGroup.Initialize(m_CharacterItemAction);
                }
            }
        }

        /// <summary>
        /// Initializes the ImpactAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="characterItemAction">The Item Action that the ImpactAction belongs to.</param>
        public override void Initialize(GameObject character, CharacterItemAction characterItemAction)
        {
            base.Initialize(character, characterItemAction);
            
            m_EffectGroup.Initialize(characterItemAction);
        }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// The action has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_EffectGroup.OnDestroy();
        }
        
        /// <summary>
        /// Write the module name in an easy to read format for debugging.
        /// </summary>
        /// <returns>The string representation of the module.</returns>
        public override string ToString()
        {
            if (m_EffectGroup == null || m_EffectGroup.Effects == null) {
                return base.ToString();
            }
            return $"Generic ({m_EffectGroup.Effects.Length}): " + ListUtility.ToStringDeep(m_EffectGroup.Effects, true);
        }
    }

    /// <summary>
    /// Adds a force to the impacted object.
    /// </summary>
    [Serializable]
    public class AddForce : ImpactAction
    {
        [Tooltip("Use impact direction or source direction?")]
        [SerializeField] protected bool m_UseSourceDirection;
        [Tooltip("The amount of force that should be added to the impact object. Z is forward, Y is up, X is right.")]
        [SerializeField] protected Vector3 m_Amount;
        [Tooltip("The if the object hit is a character how many frames should the force be applied for.")]
        [SerializeField] protected int m_Frames = 5;
        [Tooltip("Specifies how to apply the force.")]
        [SerializeField] protected ForceMode m_Mode;
        [Tooltip("Should the force be applied at the impact position?")]
        [SerializeField] protected bool m_AddForceAtPosition;

        public Vector3 Amount { get { return m_Amount; } set { m_Amount = value; } }
        public ForceMode Mode { get { return m_Mode; } set { m_Mode = value; } }
        public bool AddForceAtPosition { get { return m_AddForceAtPosition; } set { m_AddForceAtPosition = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var impactData = ctx.ImpactCollisionData;
            var source = ctx.ImpactCollisionData.SourceGameObject;
            var localImpactForce = m_Amount * impactData.ImpactStrength;
            var impactDirectionalForce = m_UseSourceDirection ? source.transform.TransformDirection(localImpactForce)
                                                                : Vector3.Scale(localImpactForce, impactData.ImpactDirection);
            
            var impactGameObject = ctx.ImpactCollisionData.ImpactGameObject;
            var opponentLocomotion = impactGameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            if (opponentLocomotion != null) {
                opponentLocomotion.AddForce(impactDirectionalForce, m_Frames);
                return;
            }
            
            var forceObject = impactData.ImpactGameObject.GetCachedParentComponent<IForceObject>();
            if (forceObject != null) {
                forceObject.AddForce(impactDirectionalForce, m_Frames);
                return;
            }

            if (impactData.ImpactRigidbody != null && !impactData.ImpactRigidbody.isKinematic) {
                var rigidbody = impactData.ImpactRigidbody;
                
                // If the damage target exists it will apply a force to the rigidbody in addition to procesing the damage.
                // Otherwise just apply the force to the rigidbody. If the radius is bigger than 0 than it must be explosive.
                var radius = ctx.ImpactDamageData?.ImpactRadius ?? 0;
                if (radius > 0) {
                    rigidbody.AddExplosionForce(localImpactForce.sqrMagnitude * MathUtility.RigidbodyForceMultiplier, impactData.ImpactPosition, radius);

                } else {
                    var amount = impactDirectionalForce * MathUtility.RigidbodyForceMultiplier;
                    if (m_AddForceAtPosition) {
                        rigidbody.AddForceAtPosition(amount, impactData.ImpactPosition, m_Mode);
                    } else {
                        rigidbody.AddForce(amount, m_Mode);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Adds a torque to the impacted object.
    /// </summary>
    [Serializable]
    public class AddTorque : ImpactAction
    {
        [Tooltip("The amount of torque that should be added to the impact object.")]
        [SerializeField] protected Vector3 m_Amount;
        [Tooltip("Specifies how to apply the torque.")]
        [SerializeField] protected ForceMode m_Mode;

        public Vector3 Amount { get { return m_Amount; } set { m_Amount = value; } }
        public ForceMode Mode { get { return m_Mode; } set { m_Mode = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var target = ctx.ImpactCollisionData.ImpactGameObject;
            var rigidbody = target.GetCachedComponent<Rigidbody>();
            if (rigidbody == null) {
                return;
            }

            rigidbody.AddTorque(m_Amount, m_Mode);
        }
    }
    
    /// <summary>
    /// Heals the impacted object.
    /// </summary>
    [Serializable]
    public class Heal : ImpactAction
    {
        [Tooltip("The amount that should be added to the Health component.")]
        [SerializeField] protected float m_Amount = 10;
        [Tooltip("Should the subsequent Impact Actions be interrupted if the Health component doesn't exist?")]
        [SerializeField] protected bool m_InterruptImpactOnNullHealth = true;

        public float Amount { get { return m_Amount; } set { m_Amount = value; } }
        public bool InterruptImpactOnNullHealth { get { return m_InterruptImpactOnNullHealth; } set { m_InterruptImpactOnNullHealth = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var target = ctx.ImpactCollisionData.ImpactGameObject;
            var health = target.GetCachedComponent<Health>();
            if (health == null) {
                health = target.GetCachedParentComponent<Health>();
            }
          
            if (health == null || !health.Heal(m_Amount)) {
                if (m_InterruptImpactOnNullHealth) {
                    ctx.InvokeInterruptCallback(this);
                }
            }
        }
    }
    
    /// <summary>
    /// Modifies the specified attribute on the impacted object.
    /// </summary>
    [Serializable]
    public class ModifyAttribute : ImpactAction
    {
        [Tooltip("The attribute that should be modified.")]
        [SerializeField] protected AttributeModifier m_AttributeModifier = new AttributeModifier();

        public AttributeModifier AttributeModifier { get { return m_AttributeModifier; } set { m_AttributeModifier = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var target = ctx.ImpactCollisionData.ImpactGameObject;
            var targetAttributeManager = target.GetCachedParentComponent<AttributeManager>();
            if (targetAttributeManager == null) {
                return;
            }

            // The impact action can collide with multiple objects. Use a pooled version of the AttributeModifier for each collision.
            var attributeModifier = GenericObjectPool.Get<AttributeModifier>();
            if (!attributeModifier.Initialize(m_AttributeModifier, targetAttributeManager)) {
                GenericObjectPool.Return(attributeModifier);
                return;
            }

            // The attribute exists. Enable the modifier. Return the modifier as soon as it is complete (which may be immediate).
            attributeModifier.EnableModifier(true);
            if (attributeModifier.AutoUpdating && attributeModifier.AutoUpdateDuration > 0) {
                Shared.Events.EventHandler.RegisterEvent<AttributeModifier, bool>(attributeModifier, "OnAttributeModifierAutoUpdateEnable", ModifierAutoUpdateEnabled);
            } else {
                GenericObjectPool.Return(attributeModifier);
            }
        }

        /// <summary>
        /// The AttributeModifier auto updater has been enabled or disabled.
        /// </summary>
        /// <param name="attributeModifier">The modifier that has been enabled or disabled.</param>
        /// <param name="enable">True if the modifier has been enabled.</param>
        private void ModifierAutoUpdateEnabled(AttributeModifier attributeModifier, bool enable)
        {
            if (enable) {
                return;
            }

            EventHandler.UnregisterEvent<AttributeModifier, bool>(attributeModifier, "OnAttributeModifierAutoUpdateEnable", ModifierAutoUpdateEnabled);
            GenericObjectPool.Return(attributeModifier);
        }
    }
    
    /// <summary>
    /// Plays an AudioClip on the impacted object.
    /// </summary>
    [Serializable]
    public class PlayAudioClip : ImpactAction
    {
        [Tooltip("The AudioClip that should be played when the impact occurs. A random AudioClip will be selected.")]
        [SerializeField] protected AudioClipSet m_AudioClipSet = new AudioClipSet();

        public AudioClipSet AudioClipSet { get { return m_AudioClipSet; } set { m_AudioClipSet = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            m_AudioClipSet.PlayAtPosition(ctx.ImpactCollisionData.RaycastHit.point);
        }
    }
    
    /// <summary>
    /// The Ricochet action will invoke the OnRicochet even when it detects target on impact, creating a ricochet effect.
    /// </summary>
    [Serializable]
    public class Ricochet : ImpactAction
    {
        /// <summary>
        /// The ricochet data contains information about the ricochet.
        /// </summary>
        public class RicochetData
        {
            public Ricochet Ricochet { get; set; }
            public ImpactCollisionData SourceImpactCollisionData { get; set; }
            public Collider HitCollider { get; set; }
            public Vector3 Direction { get; set; }
            public Vector3 SourcePosition { get; set; }
            public Vector3 TargetPosition { get; set; }
        }

        public event Action<RicochetData> OnRicochet;
        
        [Tooltip("The radius of the ricochet.")]
        [SerializeField] protected float m_Radius = 10;
        [Tooltip("The maximum number of ricochets that can occur from a single cast. Set to -1 to disable.")]
        [SerializeField] protected int m_MaxChainCount = 1;
        [Tooltip("The maximum number of objects that the ricochet can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 50;
        [UnityEngine.Serialization.FormerlySerializedAs("m_AddImpactDetectLayers")]
        [Tooltip("Merge the Collision Data layers with the local detect layers?")]
        [SerializeField] protected bool m_MergeDataLayers = true;
        [Tooltip("The detect Layer mask.")]
        [SerializeField] protected LayerMask m_DetectLayers;

        protected int m_CurrentChainCount;
        
        public bool MergeDataLayers { get => m_MergeDataLayers; set => m_MergeDataLayers = value; }
        public LayerMask DetectLayers { get => m_DetectLayers; set => m_DetectLayers = value; }

        public float Radius { get { return m_Radius; } set { m_Radius = value; } }

        private Collider[] m_HitColliders;
        private Dictionary<uint, int> m_ChainCountMap = new Dictionary<uint, int>();

        protected RicochetData m_RicochetData;

        /// <summary>
        /// Initializes the ImpactAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the ImpactAction belongs to.</param>
        public override void Initialize(GameObject character, CharacterItemAction characterItemAction)
        {
            base.Initialize(character, characterItemAction);
            m_HitColliders = new Collider[m_MaxCollisionCount];
            m_RicochetData = new RicochetData();
        }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var castID = ctx.ImpactCollisionData.SourceID;
            var source = ctx.ImpactCollisionData.SourceGameObject;
            var target = ctx.ImpactCollisionData.ImpactGameObject;
            var hit = ctx.ImpactCollisionData.RaycastHit;
            LayerMask detectLayers = m_MergeDataLayers ? (ctx.ImpactCollisionData.DetectLayers | m_DetectLayers) : m_DetectLayers;

            // Prevent the ricochet from bouncing between too many objects.
            if (m_ChainCountMap.TryGetValue(castID, out var count)) {
                if (m_MaxChainCount != -1 && count >= m_MaxChainCount) {
                    return;
                }
                m_ChainCountMap[castID] = count + 1;
            } else {
                m_ChainCountMap.Add(castID, 1);
            }

            var hitCount = Physics.OverlapSphereNonAlloc(hit.point, m_Radius, m_HitColliders, detectLayers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
            if (hitCount == m_HitColliders.Length) {
                Debug.LogWarning("Warning: The hit count is equal to the max collider array size. This will cause objects to be missed. Consider increasing the max collision count size.");
            }
#endif
            
            // Perform the cast action in the direction of each hit object.
            for (int i = 0; i < hitCount; ++i) {
                var hitTransform = m_HitColliders[i].transform;
                if (HasImpacted(hitTransform)) {
                    continue;
                }
                
                Vector3 position;
                PivotOffset pivotOffset;
                if ((pivotOffset = hitTransform.gameObject.GetCachedComponent<PivotOffset>()) != null) {
                    position = hitTransform.TransformPoint(pivotOffset.Offset);
                } else {
                    position = hitTransform.position;
                }

                if (m_CharacterItemAction.IsDebugging) {
                    Debug.DrawLine(hit.point,position,Color.magenta, 0.1f);
                }

                m_RicochetData.Ricochet = this;
                m_RicochetData.SourceImpactCollisionData = ctx.ImpactCollisionData;
                m_RicochetData.HitCollider = m_HitColliders[i];
                m_RicochetData.Direction = (position - hit.point).normalized;
                m_RicochetData.SourcePosition = hit.point;
                m_RicochetData.TargetPosition = position;
                
                OnRicochet?.Invoke(m_RicochetData);
            }
        }

        /// <summary>
        /// Geth the number of chain effect for the cast id.
        /// </summary>
        /// <param name="castID">The cast id.</param>
        /// <returns>The current number of chains effects.</returns>
        public int GetChainCountFor(uint castID)
        {
            if (m_ChainCountMap.TryGetValue(castID, out var count)) {
                return count;
            }

            return 0;
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast to reset.</param>
        public override void Reset(uint castID)
        {
            base.Reset(castID);

            if (m_ChainCountMap.ContainsKey(castID)) {
                m_ChainCountMap[castID] = 0;
            }
        }
    }
    
    /// <summary>
    /// Spawns a ParticleSystem upon impact.
    /// </summary>
    [Serializable]
    public class SpawnParticle : ImpactAction
    {
        [Tooltip("The particle prefab that should be spawned.")]
        [SerializeField] protected GameObject m_ParticlePrefab;
        [Tooltip("The positional offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the particle be parented to the object that was hit by the cast?")]
        [SerializeField] protected bool m_ParentToImpactedObject;

        public GameObject ParticlePrefab { get { return m_ParticlePrefab; } set { m_ParticlePrefab = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToImpactedObject { get { return m_ParentToImpactedObject; } set { m_ParentToImpactedObject = value; } }

        private Dictionary<uint, ParticleSystem> m_CastIDParticleMap = new Dictionary<uint, ParticleSystem>();

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="ctx">The context of the impact.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var sourceID = ctx.ImpactCollisionData.SourceID;
            var target = ctx.ImpactCollisionData.ImpactGameObject;
            var impactPosition = ctx.ImpactCollisionData.ImpactPosition;
            var impactDirection = ctx.ImpactCollisionData.ImpactDirection;

            if (m_ParticlePrefab == null) {
                Debug.LogError("Error: A Particle Prefab must be specified.", m_CharacterItemAction);
                return;
            }

            var rotation = Quaternion.LookRotation(impactDirection) * Quaternion.Euler(m_RotationOffset);
            var position = MathUtility.TransformPoint(impactPosition, rotation, m_PositionOffset);

            if (m_CastIDParticleMap.TryGetValue(sourceID, out var existingParticleSystem)) {
                existingParticleSystem.transform.SetPositionAndRotation(position, rotation);
                return;
            }

            var obj = ObjectPoolBase.Instantiate(m_ParticlePrefab, position, rotation, m_ParentToImpactedObject ? target.transform : null);
            var particleSystem = obj.GetCachedComponent<ParticleSystem>();
            if (particleSystem == null) {
                Debug.LogError($"Error: A Particle System must be specified on the particle {m_ParticlePrefab}.", m_CharacterItemAction);
                return;
            }

            // If the particle loops then the same particle should be used instead of spawning new ones until it is reset.
            if (particleSystem.main.loop) {
                particleSystem.Clear(true);
                m_CastIDParticleMap.Add(sourceID, particleSystem);
            }
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="sourceID">The ID of the cast to reset.</param>
        public override void Reset(uint sourceID)
        {
            base.Reset(sourceID);

            // Stop the particle system from emitting.
            if (m_CastIDParticleMap.TryGetValue(sourceID, out var particleSystem)) {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    /// <summary>
    /// An Impact action used to start a knock back response on the impacted character.
    /// </summary>
    [Serializable]
    public class CharacterKnockBack : ImpactAction
    {
        [Tooltip("The id used by the Impact KnockBack ability to know what animation to play.")]
        [SerializeField] protected int m_ImpactKnockBackID;
        
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var impactGameObject = ctx.ImpactCollisionData.ImpactGameObject;
            var opponentLocomotion = impactGameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            if (opponentLocomotion == null) { return; }
            
            // The opponent should play an animation which responds to the counter attack.
            var opponentResponseAbility = opponentLocomotion.GetAbility<Character.Abilities.ImpactKnockBack>();
            if (opponentResponseAbility == null) {
                return;
            }
            
            opponentResponseAbility.StartKnockBackResponse(m_ImpactKnockBackID);
        }
    }
}