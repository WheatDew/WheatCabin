/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Melee
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Melee collision data.
    /// </summary>
    public class MeleeCollisionData
    {
        protected MeleeImpactCallbackContext m_MeleeImpactCallbackContext;
        
        public MeleeImpactCallbackContext MeleeImpactCallbackContext { get => m_MeleeImpactCallbackContext; set => m_MeleeImpactCallbackContext = value; }

        /// <summary>
        /// Reset the data.
        /// </summary>
        public virtual void Reset()
        {
            if (m_MeleeImpactCallbackContext == null) {
                m_MeleeImpactCallbackContext = new MeleeImpactCallbackContext();
            } else {
                m_MeleeImpactCallbackContext.Reset();
            }
        }
    }
    
    /// <summary>
    /// Extends the Hitbox class for use by melee weapons.
    /// </summary>
    [System.Serializable]
    public class MeleeHitbox : Hitbox
    {
        [Tooltip("The ID of the Object Identifier component if the collider is null.")]
        [SerializeField] protected int m_ColliderObjectID = -1;
        [Tooltip("The hitbox can detect collisions if the local vertical offset is greater than the specified value relative to the character's position.")]
        [SerializeField] protected float m_MinimumYOffset;
        [Tooltip("The hitbox can detect collisions if the local depth offset is greater than the specified value relative to the character's position.")]
        [SerializeField] protected float m_MinimumZOffset;
        [Tooltip("Does the hitbox require movement in order for it to register a hit?")]
        [SerializeField] protected bool m_RequireMovement;
        [Tooltip("Should the hitbox only register a hit once per use?")]
        [SerializeField] protected bool m_SingleHit;
        [Tooltip("The Surface Impact triggered when the hitbox collides with an object. This will override the MeleeWeapon's SurfaceImpact.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;

        public int ColliderObjectID { get { return m_ColliderObjectID; } }
        public SurfaceImpact SurfaceImpact { get { return m_SurfaceImpact; } }

        [System.NonSerialized] private GameObject m_GameObject;
        private Transform m_Transform;
        private Transform m_CharacterTransform;

        private Vector3 m_PreviousPosition;
        private Quaternion m_PreviousRotation;
        private bool m_HitCollider;

        public GameObject GameObject { get { return m_GameObject; } }
        public Transform Transform { get { return m_Transform; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MeleeHitbox() { }

        /// <summary>
        /// Single parameter constructor.
        /// </summary>
        /// <param name="collider">The collider that represents the hitbox.</param>
        public MeleeHitbox(Collider collider) : base(collider) { }

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="meleeWeaponObject">The MeleeWeapon GameObject that the hitbox is being initialized to.</param>
        /// <param name="characterTransform">The transform of the character that the hitbox is being initialized to.</param>
        /// <returns>True if the Hitbox was initialized successfully.</returns>
        public bool Initialize(GameObject meleeWeaponObject, Transform characterTransform)
        {
            if (m_Collider == null) {
                // The item may be picked up at runtime. Use the ObjectIdentifier to find the collider.
                if (m_ColliderObjectID != -1) {
                    var objectIdentifiers = characterTransform.GetComponentsInChildren<Objects.ObjectIdentifier>();
                    for (int i = 0; i < objectIdentifiers.Length; ++i) {
                        if (objectIdentifiers[i].ID == m_ColliderObjectID) {
                            m_Collider = objectIdentifiers[i].GetComponent<Collider>();
                            if (m_Collider != null) {
                                break;
                            }
                        }
                    }

#if FIRST_PERSON_CONTROLLER
                    // The identifier may be located under the first person objects.
                    if (m_Collider == null && meleeWeaponObject != null) {
                        objectIdentifiers = meleeWeaponObject.GetComponentsInChildren<Objects.ObjectIdentifier>();
                        for (int i = 0; i < objectIdentifiers.Length; ++i) {
                            if (objectIdentifiers[i].ID == m_ColliderObjectID) {
                                m_Collider = objectIdentifiers[i].GetComponent<Collider>();
                                if (m_Collider != null) {
                                    break;
                                }
                            }
                        }
                    }
#endif
                }

                if (m_Collider == null) {
                    return false;
                }
            }
            m_GameObject = m_Collider.gameObject;
            m_Transform = m_Collider.transform;
            m_CharacterTransform = characterTransform;

            Reset(true);
            return true;
        }

        /// <summary>
        /// Resets the position and rotation of the transform. This will be done immediately before the item is started to be used.
        /// </summary>
        /// <param name="startUse">Is the item just starting to be used?</param>
        public void Reset(bool startUse)
        {
            if (m_Transform != null) {
                m_PreviousPosition = m_Transform.position;
                m_PreviousRotation = m_Transform.rotation;
            }
            
            if (startUse) {
                m_HitCollider = false;
            }
        }

        /// <summary>
        /// Can the hitbox be used?
        /// </summary>
        /// <returns>True if the hitbox can be used.</returns>
        public bool CanUse()
        {
            // The hitbox may only allow a single collision.
            if (m_SingleHit && m_HitCollider) {
                return false;
            }
            
            // If the collider transform is null it can't be used.
            if (m_Transform == null) {
                return false;
            }

            // The position must be greater than the minimum offset. An example usage is preventing a kick from causing a collision while on the ground.
            var localOffset = m_CharacterTransform.InverseTransformPoint(m_Transform.position);
            if (localOffset.y < m_MinimumYOffset || localOffset.z < m_MinimumZOffset) {
                return false;
            }

            var moving = !m_RequireMovement;
            if (m_RequireMovement) {
                // The collider must be moving in order for a new collision to occur.
                var position = m_Transform.position;
                var rotation = m_Transform.rotation;
                if ((m_PreviousPosition - position).sqrMagnitude > 0.01f) {
                    moving = true;
                }
                if (!moving && Quaternion.Angle(m_PreviousRotation, rotation) > 0.01f) {
                    moving = true;
                }

                m_PreviousPosition = position;
                m_PreviousRotation = rotation;
            }

            return moving;
        }

        /// <summary>
        /// The hitbox caused a melee impact.
        /// </summary>
        public void HitCollider()
        {
            m_HitCollider = true;
        }
    }
    
    /// <summary>
    /// base class for modules that detect melee colisions.
    /// </summary>
    [Serializable]
    public abstract class MeleeCollisionModule : MeleeActionModule,
        IModuleStartItemUse
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;

        [Tooltip("The gizmos settings used to draw debug gizmos.")]
        [SerializeField] protected GizmoSettings m_GizmoSettings = 
            new GizmoSettings(false, true, Color.red, new Color(1,0.1f,0.1f,0.3f));
        
        protected MeleeCollisionData m_MeleeCollisionData;
        protected MeleeImpactCallbackContext m_MeleeImpactCallbackContext;
        
        public MeleeCollisionData MeleeCollisionData { get { return m_MeleeCollisionData; } set => m_MeleeCollisionData = value; }
        public MeleeImpactCallbackContext MeleeImpactCallbackContext { get { return m_MeleeImpactCallbackContext; } set => m_MeleeImpactCallbackContext = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_MeleeCollisionData = CreateCollisionData();
            m_MeleeImpactCallbackContext = CreateMeleeImpactCallbackData();
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            if (register) {
                m_CharacterItemAction.OnDrawGizmosHybridE += DrawGizmosHybrid;
            } else {
                m_CharacterItemAction.OnDrawGizmosHybridE -= DrawGizmosHybrid;
            }
            
        }

        /// <summary>
        /// Create a new collision data which can be cached.
        /// </summary>
        /// <returns>The collision data to cache.</returns>
        public virtual MeleeCollisionData CreateCollisionData()
        {
            return new MeleeCollisionData();
        }
        
        /// <summary>
        /// Create a new melee impact callback data which can be cached.
        /// </summary>
        /// <returns>The melee impact callback data to cache.</returns>
        public virtual MeleeImpactCallbackContext CreateMeleeImpactCallbackData()
        {
            var impactCallbackData = new MeleeImpactCallbackContext();
            impactCallbackData.MeleeAction = MeleeAction;
            impactCallbackData.ImpactCollisionData = new ImpactCollisionData();
            return impactCallbackData;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public abstract void StartItemUse(Use useAbility);
        
        /// <summary>
        /// Check for collisions and notify if something is hit.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public abstract void CheckCollisions(MeleeUseDataStream dataStream);

        /// <summary>
        /// Draw Gizmo Hybrid.
        /// </summary>
        /// <param name="onSelected">Draw Gizmos on selected?</param>
        private void DrawGizmosHybrid(bool onSelected)
        {
            if (m_Enabled == false) { return; }
            if(m_GizmoSettings.SetGizmoSettingsLocal(onSelected, CharacterTransform) == false){return;}
            DrawGizmosHybridInternal(onSelected);
        }

        /// <summary>
        /// Draw Gizmo Hybrid.
        /// </summary>
        /// <param name="onSelected">Draw Gizmos on selected?</param>
        protected virtual void DrawGizmosHybridInternal(bool onSelected) { }
    }

    /// <summary>
    /// The base class for melee collisions with some useful functionality.
    /// </summary>
    [Serializable]
    public abstract class MeleeCollisionBase : MeleeCollisionModule
    {
        [Tooltip("If multiple hits can be registered, specifies the minimum frame count between each hit.")]
        [SerializeField] protected int m_MultiHitFrameCount = 50;
        [Tooltip("A LayerMask of the layers that can be hit by the weapon.")]
        [SerializeField] protected LayerMask m_ImpactLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.UI | 1 << LayerManager.Overlay);
        [Tooltip("Specifies if the melee weapon can detect triggers.")]
        [SerializeField] protected QueryTriggerInteraction m_TriggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("The maximum number of collision points which the melee weapon can make contact with.")]
        [SerializeField] protected int m_MaxCollisionCount = 20;
        [Tooltip("The sensitivity amount for how much the character must be looking at the hit object in order to detect if the shield should be used (-1 is the most sensitive and 1 is least).")]
        [Range(-1, 1)] [SerializeField] protected float m_ForwardShieldSensitivity = -0.75f;
        
        public int MaxCollisionCount { get { return m_MaxCollisionCount; } }
        public int MultiHitFrameCount { get => m_MultiHitFrameCount; set => m_MultiHitFrameCount = value; }
        public LayerMask ImpactLayers { get => m_ImpactLayers; set => m_ImpactLayers = value; }
        public QueryTriggerInteraction TriggerInteraction { get => m_TriggerInteraction; set => m_TriggerInteraction = value; }
        public float ForwardShieldSensitivity { get { return m_ForwardShieldSensitivity; } set { m_ForwardShieldSensitivity = value; } }

        protected Collider[] m_CollidersHit;
        protected RaycastHit[] m_CollisionsHit;
        protected HashSet<GameObject> m_HitList = new HashSet<GameObject>();
        protected int m_LastHitFrame;
        protected Dictionary<Collider,int> m_LastHitFrameByCollider;
        protected bool m_GizmoCheckCollisionThisFrame;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_CollidersHit = new Collider[m_MaxCollisionCount];
            m_CollisionsHit = new RaycastHit[m_MaxCollisionCount];
            m_LastHitFrameByCollider = new Dictionary<Collider, int>();
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="usaAbility">The item ability that is using the item.</param>
        public override void StartItemUse(Use usaAbility)
        {
            m_HitList.Clear();
            m_LastHitFrameByCollider.Clear();
            m_LastHitFrame = -m_MultiHitFrameCount;
        }

        /// <summary>
        /// Called when the Attack starts its active state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public override void OnActiveAttackStart(MeleeUseDataStream meleeUseDataStream)
        {
            base.OnActiveAttackStart(meleeUseDataStream);
            
            // We reset the hit list on every attack.
            m_HitList.Clear();
            m_LastHitFrameByCollider.Clear();
            m_LastHitFrame = -m_MultiHitFrameCount;
        }

        /// <summary>
        /// A collision happened, check if that collision should cause a Hit.
        /// </summary>
        /// <param name="dataStream">The melee use data stream.</param>
        /// <param name="hitboxIndex">The index of the hitbox that caused the hit..</param>
        /// <param name="hitColliders">The lis of colliders hit by the hitbox.</param>
        protected virtual void OnCheckCollisionHit(MeleeUseDataStream dataStream,  int hitboxIndex, ListSlice<Collider> hitColliders)
        {
            var hitCount = hitColliders.Count;
            // An object interested - retrieve the RaycastHit and apply the melee damage/effects.
            if (hitCount<= 0) { return; }
#if UNITY_EDITOR
            if (hitCount == m_MaxCollisionCount) {
                Debug.LogWarning(
                    $"Warning: The maximum number of colliders have been hit by {MeleeAction.gameObject.name}. Consider increasing the Max Collision Count value.",
                    MeleeAction);
            }
#endif
            for (int j = 0; j < hitCount; ++j) {
                var hitCollider = hitColliders[j];
                var hitGameObject = hitCollider.gameObject;
                // Don't allow the same GameObejct to continuously be hit multiple times.
                if (m_HitList.Contains(hitGameObject)) { continue; }

                // The melee weapon cannot hit the character that it belongs to.
                var characterLocomotion = hitGameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
                if (characterLocomotion != null && characterLocomotion == CharacterLocomotion) { continue; }

#if FIRST_PERSON_CONTROLLER
                // The cast should not hit any colliders who are a child of the camera.
                if (hitGameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                    continue;
                }
#endif

                // The collider was hit. ComputePenetration needs to be used to retrieve more information.
                if (HitCollider(dataStream, hitboxIndex, hitColliders, j, hitCollider, characterLocomotion, out var impactResult)) {

                    OnBeforeHit(dataStream, hitboxIndex, impactResult);
                    
                    m_MeleeCollisionData.MeleeImpactCallbackContext = impactResult;
                    
                    //Call this in HitCollider continue? or here is fine?
                    MeleeAction.OnAttackImpact(impactResult);

                    // An object was hit continue to check for other hits?
                    if (OnHitColliderContinue(m_MeleeCollisionData) == false) {
                        // Cancel the attack.
                        MeleeAction.MainMeleeAttackModule?.AttackCanceled(dataStream);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Make some adjustments just before the attack impact happens.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hitboxIndex">The hitbox index that caused the impact.</param>
        /// <param name="impactContext">The impact context.</param>
        protected virtual void OnBeforeHit(MeleeUseDataStream dataStream, int hitboxIndex, MeleeImpactCallbackContext impactContext)
        {
            //Do nothing.
        }

        /// <summary>
        /// Should the attack continue to check for collisions after this collision.
        /// Return false to interrupt or stop the attack. 
        /// </summary>
        /// <param name="collisionData">The collision data so far.</param>
        /// <returns>Returns true to continue the attack.</returns>
        protected virtual bool OnHitColliderContinue(MeleeCollisionData collisionData)
        {
            return MeleeAction.OnHitColliderContinue(collisionData);
        }

        /// <summary>
        /// The melee weapon hit a collider.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        /// <param name="hitboxIndex">The index of the hitbox that caused the collision.</param>
        /// <param name="hitColliders">All the colliders that where hit.</param>
        /// <param name="hitColliderIndex">The index within the list of colliders that was hit.</param>
        /// <param name="hitCollider">The collider that was hit.</param>
        /// <param name="hitCharacterLocomotion">The hit Ultimate Character motor component.</param>
        /// <param name="impactResult">Output the Impact result.</param>
        /// <returns>True if the hit was successfully registered.</returns>
        protected virtual bool HitCollider(MeleeUseDataStream dataStream, int hitboxIndex, ListSlice<Collider> hitColliders, int hitColliderIndex, Collider hitCollider, UltimateCharacterLocomotion hitCharacterLocomotion, out MeleeImpactCallbackContext impactResult)
        {
            var castCollisions = DoCollisionCast(dataStream, hitboxIndex, hitCollider);
            
            for (int i = 0; i < castCollisions.Count; ++i) {
                var raycastHit = castCollisions[i];
                var castHitCollider = raycastHit.collider;
                
                // Th cast hit collider must match the hit collider.
                if (castHitCollider != hitCollider) {
                    continue;
                }
                
                castHitCollider = OverrideHitCollider(hitCharacterLocomotion, castHitCollider);
                
                //Check if that collider was already hit in the last few frames:
                if(m_LastHitFrameByCollider.TryGetValue(castHitCollider, out var lastHitFrame)){
                    if (m_MultiHitFrameCount > 0 && Time.frameCount - lastHitFrame <= m_MultiHitFrameCount) {
                        continue;
                    }
                }

                // The same parent GameObject should only be damaged once per use.
                var hitGameObject = castHitCollider.gameObject;
                var hitDamageTarget = DamageUtility.GetDamageTarget(hitGameObject);
                GameObject hitListGameObject = null;
                if (hitDamageTarget != null) {
                    hitListGameObject = hitDamageTarget.HitGameObject;
                } else if (hitCharacterLocomotion != null) {
                    hitListGameObject = hitCharacterLocomotion.gameObject;
                } else {
                    hitListGameObject = hitGameObject;
                }

                if (m_HitList.Contains(hitListGameObject)) {
                    continue;
                }
                m_HitList.Add(hitListGameObject);
                m_LastHitFrame = Time.frameCount;
                m_LastHitFrameByCollider[castHitCollider] = Time.frameCount;

                //Initialize the impact collision data.
                var impactData = m_MeleeImpactCallbackContext.ImpactCollisionData;
                impactData.Reset();
                impactData.Initialize();
                impactData.SetRaycast(raycastHit);
                impactData.SetImpactSource(MeleeAction);
                impactData.SetImpactTarget(castHitCollider, hitListGameObject);
                {
                    impactData.SourceID = (uint)hitboxIndex;
                    impactData.HitCount = hitColliders.Count;
                    impactData.HitColliders = hitColliders;
                    //Damage multiplier on the impact strength.
                    SetAdditionalImpactData(dataStream, hitboxIndex, impactData);
                };

                impactResult = m_MeleeImpactCallbackContext;
                return true;
            }

            impactResult = null;
            return false;
        }

        /// <summary>
        /// Do a cast when hitting a collider to get the raycast hit data.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hitboxIndex">The index of the hitbox that caused the collision.</param>
        /// <param name="hitCollider">The collider that was hit.</param>
        /// <returns>A list of raycast hits.</returns>
        protected abstract ListSlice<RaycastHit> DoCollisionCast(MeleeUseDataStream dataStream, int hitboxIndex, Collider hitCollider);

        /// <summary>
        /// Set some additional data to the impact collision data.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hitboxIndex">The hitbox index that caused the collision.</param>
        /// <param name="impactData">The impact data to add the data to.</param>
        protected virtual void SetAdditionalImpactData(MeleeUseDataStream dataStream, int hitboxIndex, ImpactCollisionData impactData)
        {
            impactData.ImpactStrength = dataStream.TriggerData.Force * dataStream.AttackData.StrengthMultiplier;
        }

        /// <summary>
        /// In some cases the collider that was hit is not the actual collider that should be set for the impact data.
        /// For example if the hit character has a shield, use the shield collider instead if the angle is right.
        /// </summary>
        /// <param name="hitCharacterLocomotion">The hit character locomotion.</param>
        /// <param name="hitCollider">The hit collider.</param>
        /// <returns>The validated collider. (can be null.)</returns>
        protected virtual Collider OverrideHitCollider(UltimateCharacterLocomotion hitCharacterLocomotion, Collider hitCollider)
        {
            
            if (hitCharacterLocomotion == null) { return hitCollider; }

            // If the hit character has a shield equipped and the current character is facing the shield then the shield should be used instead.
            var angleDot = Vector3.Dot(hitCharacterLocomotion.transform.forward, CharacterTransform.forward);
            if (!(angleDot < m_ForwardShieldSensitivity)) {
                return hitCollider;
            }

            var hitInventory = hitCharacterLocomotion.gameObject.GetCachedComponent<InventoryBase>();
            var hasShieldCollider = false;
            if (hitInventory == null) { return hitCollider; }

            for (int k = 0; k < hitInventory.SlotCount; ++k) {
                var equippedItem = hitInventory.GetActiveCharacterItem(k);
                if (equippedItem == null) { continue; }

                // The equipped item must be a shield with a collider.
                for (int m = 0; m < equippedItem.ItemActions.Length; ++m) {
                    var itemAction = equippedItem.ItemActions[m];

                    if (!(itemAction is ShieldAction shield)) { continue; }

                    if (shield.RequireAim && !hitCharacterLocomotion.IsAbilityTypeActive<Aim>()) { continue; }

                    var visibleObject = equippedItem.ActivePerspectiveItem.GetVisibleObject();
                    Collider visibleObjectCollider;
                    if (visibleObject != null &&
                        (visibleObjectCollider = visibleObject.GetCachedComponent<Collider>())) {
                        // The item has a shield with a collider. Use the shield collider instead.
                        hitCollider = visibleObjectCollider;
                        hasShieldCollider = true;
                        break;
                    }
                }

                if (hasShieldCollider) { break; }
            }

            return hitCollider;
        }
    }
    
    /// <summary>
    /// Detect collisions using hitboxes.
    /// </summary>
    [Serializable]
    public class HitboxCollision : MeleeCollisionBase
    {
        [Tooltip("An array of hitboxes that the MeleeWeapon detects collisions with.")]
        [SerializeField] protected ItemPerspectiveProperty<MeleeHitbox[]> m_Hitboxes;
        
        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_Hitboxes.Initialize(MeleeAction);
#if THIRD_PERSON_CONTROLLER
            var thirdPersonHitboxes = m_Hitboxes.GetThirdPersonValue();
            if (thirdPersonHitboxes != null) {
                for (int i = 0; i < thirdPersonHitboxes.Length; ++i) {
                    thirdPersonHitboxes[i].Initialize(m_Hitboxes.PerspectiveVisibleObject, CharacterTransform);
                }
            } else {
                m_Hitboxes.SetValue(new MeleeHitbox[0], false);
            }
#endif
#if FIRST_PERSON_CONTROLLER
            var firstPersonHitboxes = m_Hitboxes.GetFirstPersonValue();
            if (firstPersonHitboxes != null) {
                for (int i = 0; i < firstPersonHitboxes.Length; ++i) {
                    firstPersonHitboxes[i].Initialize(m_Hitboxes.PerspectiveVisibleObject, CharacterTransform);
                }
            } else {
                m_Hitboxes.SetValue(new MeleeHitbox[0], true);
            }
#endif
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public override void StartItemUse(Use itemAbility)
        {
            base.StartItemUse(itemAbility);

            // The hitbox needs to be reset to account for the latest values.
            var hitboxes = m_Hitboxes.GetValue();
            if (hitboxes != null) {
                for (int i = 0; i < hitboxes.Length; ++i) {
                    hitboxes[i].Reset(true);
                }
            } else {
                m_Hitboxes.SetValue(new MeleeHitbox[0]);
            }
        }

        /// <summary>
        /// Check for collisions and notify if something is hit.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void CheckCollisions(MeleeUseDataStream dataStream)
        {
            m_GizmoCheckCollisionThisFrame = true;
            // Check for an objects which intersects the item's collider.
            var hitboxes = m_Hitboxes.GetValue();
            for (int i = 0; i < hitboxes.Length; ++i) {
                // Don't do any collision testing if the hitbox can't be used. This will reduce the amount of physic calls that be made done.
                var meleeHitbox = hitboxes[i];
                if (!meleeHitbox.CanUse()) {
                    continue;
                }

                var hitCount = DoPhysicsOverlap(meleeHitbox, i, m_CollidersHit, m_ImpactLayers, m_TriggerInteraction);

                var hitColliders = new ListSlice<Collider>(m_CollidersHit,0, hitCount);
                OnCheckCollisionHit(dataStream, i, hitColliders);
            }
        }

        /// <summary>
        /// Make some adjustments just before the attack impact happens.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hitboxIndex">The hitbox index that caused the impact.</param>
        /// <param name="impactContext">The impact context.</param>
        protected override void OnBeforeHit(MeleeUseDataStream dataStream, int hitboxIndex, MeleeImpactCallbackContext impactContext)
        {
            base.OnBeforeHit(dataStream, hitboxIndex, impactContext);
            var hitbox = m_Hitboxes.GetValue()[hitboxIndex];
            hitbox.HitCollider();
        }

        /// <summary>
        /// Do a physics overlap.
        /// </summary>
        /// <param name="meleeHitbox">The melee hitbox used for checking the collision.</param>
        /// <param name="hitboxIndex">The hibox index.</param>
        /// <param name="collidersHit">The result of the Colliders hit by the hitbox.</param>
        /// <param name="impactLayers">THe impact layers.</param>
        /// <param name="triggerInteraction">Allow trigger interraction?</param>
        /// <returns>The count of the number of collider hit.</returns>
        protected virtual int DoPhysicsOverlap(MeleeHitbox meleeHitbox, int hitboxIndex, Collider[] collidersHit, int impactLayers, QueryTriggerInteraction triggerInteraction)
        {
            var hitboxCollider = meleeHitbox.Collider;
            var hitboxTransform = meleeHitbox.Transform;

            // The melee weapon cannot hit the parent character.
            var collisionEnabled = CharacterLocomotion.CollisionLayerEnabled;
            CharacterLocomotion.EnableColliderCollisionLayer(false);
            hitboxCollider.enabled = false;
            var hitCount = 0;
            if (hitboxCollider is BoxCollider) {
                var boxCollider = hitboxCollider as BoxCollider;
                var center = hitboxTransform.TransformPoint(boxCollider.center);
                var halfExtents = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) / 2f;
                hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, collidersHit, hitboxTransform.rotation,
                    impactLayers, triggerInteraction);
            } else if (hitboxCollider is SphereCollider) {
                var sphereCollider = hitboxCollider as SphereCollider;
                var center = hitboxTransform.TransformPoint(sphereCollider.center);
                var radius = sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider);
                hitCount = Physics.OverlapSphereNonAlloc(center, radius, collidersHit, impactLayers, triggerInteraction);
            } else if (hitboxCollider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = hitboxCollider as CapsuleCollider;
                var center = hitboxTransform.TransformPoint(capsuleCollider.center);
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, center, hitboxTransform.rotation, out startEndCap,
                    out endEndCap);
                var radius = capsuleCollider.radius * MathUtility.CapsuleColliderHeightMultiplier(capsuleCollider);
                hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, radius, collidersHit, impactLayers,
                    triggerInteraction);
            }

            hitboxCollider.enabled = true;
            CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);
            return hitCount;
        }

        /// <summary>
        /// Do a cast when hitting a collider to get the raycast hit data.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hitboxIndex">The index of the hitbox that caused the collision.</param>
        /// <param name="hitCollider">The collider that was hit.</param>
        /// <returns>A list of raycast hits.</returns>
        protected override ListSlice<RaycastHit> DoCollisionCast(MeleeUseDataStream dataStream, int hitboxIndex, Collider hitCollider)
        {
            var other = hitCollider;
             var hitbox = m_Hitboxes.GetValue()[hitboxIndex];
            var hitCount = 0;
            Vector3 direction;
            float distance;
            
            // A RaycastHit should be retrieved based off of the collision.
            if (((other is BoxCollider) || (other is SphereCollider) || (other is CapsuleCollider) || ((other is MeshCollider) && (other as MeshCollider).convex)) && 
                Physics.ComputePenetration(hitbox.Collider, hitbox.Transform.position, hitbox.Transform.rotation, other, other.transform.position, other.transform.rotation, out direction, out distance)) {
                // ComputePenetration doesn't return the closest point on the collider that was hit. Use ClosestPoint to determine that point.
                var offset = direction * (distance + CharacterLocomotion.ColliderSpacing * 2);
                var otherTransform = other.transform;
                var closestPoint = Physics.ClosestPoint(hitbox.Transform.position + offset, other, otherTransform.position, otherTransform.rotation);

                // Fire a spherecast instead of a raycast from the closest point because the closest point may be on an edge which would prevent the raycast from
                // hitting the object.
                hitCount = Physics.SphereCastNonAlloc(closestPoint + offset, CharacterLocomotion.ColliderSpacing, -direction, m_CollisionsHit,
                                                            distance + CharacterLocomotion.ColliderSpacing * 2, 1 << other.gameObject.layer, m_TriggerInteraction);
            } else {
                // If ComputePenetration cannot retrive the location (such as because of a concave MeshCollider) then a cast should be used from the character's position.
                var hitboxCollider = hitbox.Collider;
                var hitboxTransform = hitbox.Transform;

                // Convert the collider's position to be relative to the character's z location.
                var position = hitboxTransform.position;
                var localPosition = Character.transform.InverseTransformPoint(position);
                distance = localPosition.z + CharacterLocomotion.ColliderSpacing;
                localPosition.z = 0;
                position = CharacterTransform.TransformPoint(localPosition);

                // Perform the raycast from the character's local z position. This will prevent the cast from overlapping the object.
                var rotation = hitboxTransform.rotation;
                if (hitboxCollider is BoxCollider) {
                    var boxCollider = hitboxCollider as BoxCollider;
                    hitCount = Physics.BoxCastNonAlloc(MathUtility.TransformPoint(position, rotation, boxCollider.center), boxCollider.size / 2, CharacterTransform.forward,
                                                        m_CollisionsHit, rotation, distance, 1 << other.gameObject.layer, m_TriggerInteraction);
                } else if (hitboxCollider is SphereCollider) {
                    var sphereCollider = hitboxCollider as SphereCollider;
                    hitCount = Physics.SphereCastNonAlloc(MathUtility.TransformPoint(position, rotation, sphereCollider.center), 
                                                        sphereCollider.radius, CharacterTransform.forward, m_CollisionsHit, distance, 1 << other.gameObject.layer, m_TriggerInteraction);
                } else if (hitboxCollider is CapsuleCollider) {
                    var capsuleCollider = hitboxCollider as CapsuleCollider;
                    var radius = capsuleCollider.radius;
                    Vector3 firstEndCap, secondEndCap;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, position, rotation, out firstEndCap, out secondEndCap);
                    hitCount = Physics.CapsuleCastNonAlloc(firstEndCap, secondEndCap, radius, CharacterTransform.forward, m_CollisionsHit, 
                        radius + distance, 1 << other.gameObject.layer, m_TriggerInteraction);
                }
            }

            return new ListSlice<RaycastHit>(m_CollisionsHit, 0, hitCount);
        }

        /// <summary>
        /// Set some additional data to the impact collision data.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hitboxIndex">The hitbox index that caused the collision.</param>
        /// <param name="impactData">The impact data to add the data to.</param>
        protected override void SetAdditionalImpactData(MeleeUseDataStream dataStream, int hitboxIndex, ImpactCollisionData impactData)
        {
            var hitbox = m_Hitboxes.GetValue()[hitboxIndex];
            impactData.ImpactStrength = dataStream.TriggerData.Force *
                                        dataStream.AttackData.StrengthMultiplier *
                                        hitbox.DamageMultiplier;
            impactData.SurfaceImpact = hitbox.SurfaceImpact;
        }

        /// <summary>
        /// Draw Gizmo Hybrid.
        /// </summary>
        /// <param name="onSelected">Draw Gizmos on selected?</param>
        protected override void DrawGizmosHybridInternal(bool onSelected)
        {
            if (MeleeAction.IsAttacking == false) {
                return;
            }
            
            base.DrawGizmosHybridInternal(onSelected);
            
            if (m_GizmoCheckCollisionThisFrame) {
                Gizmos.color = m_GizmoSettings.Color1;
            } else {
                Gizmos.color = m_GizmoSettings.Color2;
            }
            m_GizmoCheckCollisionThisFrame = false;

            var hitboxes = m_Hitboxes.GetValue();
            for (int i = 0; i < hitboxes.Length; ++i) {
                var hitbox = hitboxes[i];
                
                GizmosUtility.DrawColliderGizmos(hitbox.Collider);
            }
        }

        /// <summary>
        /// The module has been added to the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was added to.</param>
        public override void OnEditorModuleAdded(GameObject gameObject)
        {
            base.OnEditorModuleAdded(gameObject);

            m_Hitboxes = new ItemPerspectiveProperty<MeleeHitbox[]>();

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                return;
            }
#endif

#if FIRST_PERSON_CONTROLLER
            var firstPersonPerspectiveItem = gameObject.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspectiveItem != null && firstPersonPerspectiveItem.GetVisibleObject() != null) {
                var visibleObject = firstPersonPerspectiveItem.GetVisibleObject();
                BoxCollider boxCollider;
                if ((boxCollider = visibleObject.GetComponent<BoxCollider>()) == null) {
                    boxCollider = visibleObject.AddComponent<BoxCollider>();
                }
                m_Hitboxes.SetFirstPersonValue(new MeleeHitbox[] { new MeleeHitbox(boxCollider) });
            }
#endif
            var thirdPersonPerspectiveItem = gameObject.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonPerspectiveItem != null && thirdPersonPerspectiveItem.GetVisibleObject() != null) {
                var visibleObject = thirdPersonPerspectiveItem.GetVisibleObject();
                BoxCollider boxCollider;
                if ((boxCollider = visibleObject.GetComponent<BoxCollider>()) == null) {
                    boxCollider = visibleObject.AddComponent<BoxCollider>();
                }
                m_Hitboxes.SetThirdPersonValue(new MeleeHitbox[] { new MeleeHitbox(boxCollider) });
            }
        }

        /// <summary>
        /// The module has been removed from the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was removed from.</param>
        public override void OnEditorModuleRemoved(GameObject gameObject)
        {
            base.OnEditorModuleRemoved(gameObject);

#if FIRST_PERSON_CONTROLLER
            if (m_Hitboxes.GetFirstPersonValue() != null) {
                var hitboxes = m_Hitboxes.GetFirstPersonValue();
                for (int i = hitboxes.Length - 1; i >= 0; --i) {
                    UnityEngine.Object.DestroyImmediate(hitboxes[i].GameObject, true);
                }
                m_Hitboxes.SetFirstPersonValue(null);
            }
#endif
            if (m_Hitboxes.GetThirdPersonValue() != null) {
                var hitboxes = m_Hitboxes.GetThirdPersonValue();
                for (int i = hitboxes.Length - 1; i >= 0; --i) {
                    UnityEngine.Object.DestroyImmediate(hitboxes[i].GameObject, true);
                }
                m_Hitboxes.SetThirdPersonValue(null);
            }
        }
    }
    
    /// <summary>
    /// Uses the previous collider position to create a lerp of physics overlap such that if the melee collider moves fast it can still hit.
    /// </summary>
    [Serializable]
    public class LerpedHitboxCollision : HitboxCollision
    {
        [Tooltip("The number of physic overlap to do between the previous position and the current one.")]
        [SerializeField] protected int m_Density;

        public int Density { get => m_Density; set => m_Density = value; }

        protected Vector3[] m_HitboxPreviousPosition;
        protected Quaternion[] m_HitboxPreviousRotation;
        protected bool m_FirstCheckSinceStart = false;
        protected List<Collider> m_CachedHitColliders;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_CachedHitColliders = new List<Collider>();
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public override void StartItemUse(Use itemAbility)
        {
            base.StartItemUse(itemAbility);
            var hitboxCount = m_Hitboxes.GetValue().Length;
            if (m_HitboxPreviousPosition == null) {
                m_HitboxPreviousPosition = new Vector3[hitboxCount];
            }else if (m_HitboxPreviousPosition.Length < hitboxCount) {
                Array.Resize(ref m_HitboxPreviousPosition, hitboxCount);
            }

            if (m_HitboxPreviousRotation == null) {
                m_HitboxPreviousRotation = new Quaternion[hitboxCount];
            }else if (m_HitboxPreviousRotation.Length < hitboxCount) {
                Array.Resize(ref m_HitboxPreviousRotation, hitboxCount);
            }

            m_FirstCheckSinceStart = true;
        }


        /// <summary>
        /// Do a physics overlap.
        /// </summary>
        /// <param name="meleeHitbox">The melee hitbox used for checking the collision.</param>
        /// <param name="hitboxIndex">The hibox index.</param>
        /// <param name="collidersHit">The result of the Colliders hit by the hitbox.</param>
        /// <param name="impactLayers">THe impact layers.</param>
        /// <param name="triggerInteraction">Allow trigger interraction?</param>
        /// <returns>The count of the colliders hit.</returns>
        protected override int DoPhysicsOverlap(MeleeHitbox meleeHitbox, int hitboxIndex, Collider[] collidersHit, int impactLayers, QueryTriggerInteraction triggerInteraction)
        {
            var hitboxCollider = meleeHitbox.Collider;
            var hitboxTransform = meleeHitbox.Transform;
            var currentPosition = hitboxTransform.position;
            var currentRotation = hitboxTransform.rotation;
            var previousPosition = m_HitboxPreviousPosition[hitboxIndex];
            var previousRotation = m_HitboxPreviousRotation[hitboxIndex];
            if (m_FirstCheckSinceStart) {
                previousPosition = hitboxTransform.position;
                previousRotation = hitboxTransform.rotation;
            }

            if (m_Density < 0) {
                m_Density = 0;
            }

            // The melee weapon cannot hit the parent character.
            var collisionEnabled = CharacterLocomotion.CollisionLayerEnabled;
            CharacterLocomotion.EnableColliderCollisionLayer(false);
            hitboxCollider.enabled = false;
            var hitCount = 0;
            m_CachedHitColliders.Clear();
            
            // Lerp the collider position and rotation by the density factor. 
            for (int i = 0; i < m_Density+1; i++) {
                var lerpT = (float)i / (m_Density + 1);
                var position = Vector3.Lerp(currentPosition, previousPosition , lerpT);
                var rotation = Quaternion.Lerp(currentRotation, previousRotation , lerpT);

                hitboxTransform.position = position;
                hitboxTransform.rotation = rotation;

                var iterationHitCount = 0;
                
                if (hitboxCollider is BoxCollider) {
                    var boxCollider = hitboxCollider as BoxCollider;
                    var center = hitboxTransform.TransformPoint(boxCollider.center);
                    var halfExtents = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) / 2f;
                    iterationHitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, collidersHit, hitboxTransform.rotation,
                        impactLayers, triggerInteraction);
                } else if (hitboxCollider is SphereCollider) {
                    var sphereCollider = hitboxCollider as SphereCollider;
                    var center = hitboxTransform.TransformPoint(sphereCollider.center);
                    var radius = sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider);
                    iterationHitCount = Physics.OverlapSphereNonAlloc(center, radius, collidersHit, impactLayers, triggerInteraction);
                } else if (hitboxCollider is CapsuleCollider) {
                    Vector3 startEndCap, endEndCap;
                    var capsuleCollider = hitboxCollider as CapsuleCollider;
                    var center = hitboxTransform.TransformPoint(capsuleCollider.center);
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, center, hitboxTransform.rotation, out startEndCap,
                        out endEndCap);
                    var radius = capsuleCollider.radius * MathUtility.CapsuleColliderHeightMultiplier(capsuleCollider);
                    iterationHitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, radius, collidersHit, impactLayers,
                        triggerInteraction);
                }

                for (int j = 0; j < iterationHitCount; j++) {
                    if (m_CachedHitColliders.Contains(collidersHit[j]) == false) {
                        m_CachedHitColliders.Add(collidersHit[j]);
                    }
                }
            }
            
            //Set the position and rotation back in place
            hitboxTransform.position = currentPosition;
            hitboxTransform.rotation = currentRotation;

            // Set the colliders hit in the array.
            hitCount = m_CachedHitColliders.Count;
            for (int i = 0; i < m_CachedHitColliders.Count; i++) {
                collidersHit[i] = m_CachedHitColliders[i];
            }
            
            hitboxCollider.enabled = true;
            CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);

            m_HitboxPreviousPosition[hitboxIndex] = currentPosition;
            m_HitboxPreviousRotation[hitboxIndex] = currentRotation;

            return hitCount;
        }
    }
    
    /// <summary>
    /// A collision module which checks collision within a sphere overlap.
    /// </summary>
    [Serializable]
    public class SphereOverlapCollision : MeleeCollisionBase
    {
        [Tooltip("The sphere center position offset from the character position, in local space.")]
        [SerializeField] protected Vector3 m_SphereCenter = new Vector3(0,1,1);
        [Tooltip("The sphere radius used for the sphere overlap.")]
        [SerializeField] protected float m_SphereRadius = 1;
        
        public Vector3 SphereCenter { get => m_SphereCenter; set => m_SphereCenter = value; }
        public float SphereRadius { get => m_SphereRadius; set => m_SphereRadius = value; }

        /// <summary>
        /// Check for collisions and notify if something is hit.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void CheckCollisions(MeleeUseDataStream dataStream)
        {
            m_GizmoCheckCollisionThisFrame = true;
            // Don't do any collision testing if the hitbox can't be used. This will reduce the amount of physic calls that be made done.

            // The melee weapon cannot hit the parent character.
            var collisionEnabled = CharacterLocomotion.CollisionLayerEnabled;
            CharacterLocomotion.EnableColliderCollisionLayer(false);
            var hitCount = 0;
            var center = CharacterTransform.TransformPoint(m_SphereCenter);
            var radius = m_SphereRadius;
            hitCount = Physics.OverlapSphereNonAlloc(center, radius, m_CollidersHit, m_ImpactLayers, m_TriggerInteraction);
            CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);

            var hitColliders = new ListSlice<Collider>(m_CollidersHit, 0, hitCount);
            OnCheckCollisionHit(dataStream,0, hitColliders);
        }

        /// <summary>
        /// Do a cast when hitting a collider to get the raycast hit data.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hitboxIndex">The index of the hitbox that caused the collision.</param>
        /// <param name="hitCollider">The collider that was hit.</param>
        /// <returns>A list of raycast hits.</returns>
        protected override ListSlice<RaycastHit> DoCollisionCast(MeleeUseDataStream dataStream, int hitboxIndex, Collider hitCollider)
        {
            var hitCount = 0;
            Vector3 direction;
            float distance;

            var characterTransform = Character.transform;
            var center = characterTransform.TransformPoint(m_SphereCenter);

            // A RaycastHit should be retrieved based off of the collision.
            if (((hitCollider is BoxCollider) || (hitCollider is SphereCollider) || (hitCollider is CapsuleCollider) ||
                 ((hitCollider is MeshCollider) && (hitCollider as MeshCollider).convex))) {
                var hitColliderTransform = hitCollider.transform;
                
                direction = characterTransform.position - center;
                distance = direction.magnitude;
                direction = direction.normalized;
                
                var offset = direction * (distance + CharacterLocomotion.ColliderSpacing * 2);
                var closestPoint = Physics.ClosestPoint(center + offset, hitCollider, hitColliderTransform.position, hitColliderTransform.rotation);

                hitCount = Physics.SphereCastNonAlloc(closestPoint + offset, CharacterLocomotion.ColliderSpacing, -direction, m_CollisionsHit,
                    distance + CharacterLocomotion.ColliderSpacing * 2, 1 << hitCollider.gameObject.layer, m_TriggerInteraction);

                for (int i = 0; i < hitCount; i++) {
                    var hit = m_CollisionsHit[i];
                    // SphereCastNonAlloc returns point Vector3.zero when the sphere overlaps. The closest point should be used instead.
                    if (hit.distance == 0) {
                        hit.point = closestPoint;
                        hit.distance = (closestPoint - (center + offset)).magnitude;
                    }

                    m_CollisionsHit[i] = hit;
                }
            } else {
                // Convert the collider's position to be relative to the character's z location.
                var position = center;
                var localPosition = CharacterTransform.InverseTransformPoint(position);
                distance = localPosition.z + CharacterLocomotion.ColliderSpacing;
                localPosition.z = 0;
                position = CharacterTransform.TransformPoint(localPosition);

                // Perform the raycast from the character's local z position. This will prevent the cast from overlapping the object.
                var rotation = CharacterTransform.rotation;
                var originPoint = MathUtility.TransformPoint(position, rotation, m_SphereCenter);
                hitCount = Physics.SphereCastNonAlloc(originPoint,
                    m_SphereRadius, CharacterTransform.forward, m_CollisionsHit, distance, 1 << hitCollider.gameObject.layer,
                    m_TriggerInteraction);

                for (int i = 0; i < hitCount; i++) {
                    var hit = m_CollisionsHit[i];
                    // SphereCastNonAlloc returns point Vector3.zero when the sphere overlaps. The closest point should be used instead.
                    if (hit.distance == 0) {
                        hit.point = originPoint;
                        hit.distance = (originPoint - center).magnitude;
                    }

                    m_CollisionsHit[i] = hit;
                }
            }

            return new ListSlice<RaycastHit>(m_CollisionsHit,0,hitCount);
        }

        /// <summary>
        /// Draw Gizmo Hybrid.
        /// </summary>
        /// <param name="onSelected">Draw Gizmos on selected?</param>
        protected override void DrawGizmosHybridInternal(bool onSelected)
        {
            if (MeleeAction.IsAttacking == false) {
                return;
            }
            
            base.DrawGizmosHybridInternal(onSelected);
            
            if (m_GizmoCheckCollisionThisFrame) {
                Gizmos.color = m_GizmoSettings.Color1;
            } else {
                Gizmos.color = m_GizmoSettings.Color2;
            }
            m_GizmoCheckCollisionThisFrame = false;

            Gizmos.DrawSphere(m_SphereCenter, m_SphereRadius);
        }
    }
}