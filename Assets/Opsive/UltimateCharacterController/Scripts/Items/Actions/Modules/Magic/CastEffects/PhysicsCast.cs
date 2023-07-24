/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.CastEffects
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Uses the physics system to determine the impacted object.
    /// </summary>
    [System.Serializable]
    public class PhysicsCast : MagicMultiTargetCastEffectModule
    {
        /// <summary>
        /// The type of cast to perform.
        /// </summary>
        public enum CastMode
        {
            Raycast,        // Performs a raycast.
            SphereCast,     // Performs a sphere cast.
            OverlapSphere,  // Performs an overlap sphere check.
        }

        [Tooltip("Specifies the type of cast to perform.")]
        [SerializeField] protected CastMode m_Mode;
        [Tooltip("Should the look source be used when determining the cast position? If false the origin will be used.")]
        [SerializeField] protected bool m_UseLookSourcePosition;
        [Tooltip("The offset to add to the physics cast position.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The distance of the cast.")]
        [SerializeField] protected float m_Distance = 5;
        [Tooltip("The radius of the cast.")]
        [SerializeField] protected float m_Radius = 5;
        [Tooltip("The layers that the cast can detect.")]
        [SerializeField] protected LayerMask m_Layers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.UI | 1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("Specifies if the cast intersect with triggers.")]
        [SerializeField] protected QueryTriggerInteraction m_TriggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("The maximum number of collisions that the cast can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 50;
        [Tooltip("Allow this character to be impacted by the cast.")]
        [SerializeField] protected bool m_AllowSelfImpact = false;
        [Tooltip("The gizmo settings are used to draw gizmos.")]
        [SerializeField] protected GizmoSettings m_GizmoSettings = new GizmoSettings(false,true, Color.red);

        public CastMode Mode { get { return m_Mode; } set { m_Mode = value; } }
        public bool UseLookSourcePosition { get { return m_UseLookSourcePosition; } set { m_UseLookSourcePosition = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public float Distance { get { return m_Distance; } set { m_Distance = value; } }
        public float Radius { get { return m_Radius; } set { m_Radius = value; } }
        public LayerMask Layers { get { return m_Layers; } set { m_Layers = value; } }
        public QueryTriggerInteraction TriggerInteraction { get { return m_TriggerInteraction; } set { m_TriggerInteraction = value; } }
        public int MaxCollisionCount { get => m_MaxCollisionCount; set => m_MaxCollisionCount = value; }
        public bool AllowSelfImpact { get => m_AllowSelfImpact; set => m_AllowSelfImpact = value; }
        public GizmoSettings GizmoSettings { get => m_GizmoSettings; set => m_GizmoSettings = value; }

        private RaycastHit[] m_HitRaycasts;
        private Collider[] m_HitColliders;

        protected bool m_DrawGizmos;
        protected Vector3 m_Origin;
        protected Vector3 m_Direction;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            MagicAction.OnDrawGizmosHybridE += OnDrawGizmosHybrid;
        }

        /// <summary>
        /// Draw the gizmos to show the detection area.
        /// </summary>
        /// <param name="onSelected">Show gizmos on select?</param>
        private void OnDrawGizmosHybrid(bool onSelected)
        {
            if (m_GizmoSettings.SetGizmoSettings(onSelected) == false) {
                return;
            }
            
            if(m_DrawGizmos == false){ return; }
            m_DrawGizmos = false;

            if (m_Mode == CastMode.OverlapSphere) {
                Gizmos.DrawSphere(m_Origin, m_Radius);
                return;
            }
            
            Gizmos.DrawRay(m_Origin, m_Direction*m_Distance);
            if (m_Mode == CastMode.SphereCast) {
                Gizmos.DrawSphere(m_Origin, m_Radius);
                Gizmos.DrawSphere(m_Origin+m_Direction*m_Distance, m_Radius);
            }
        }

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream, contains the cast data.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            m_DrawGizmos = true;
            
            Transform origin = useDataStream.CastData.CastOrigin;
            Vector3 direction = useDataStream.CastData.Direction;
            Vector3 targetPosition = useDataStream.CastData.CastTargetPosition;
            m_CastID = (uint)useDataStream.CastData.CastID;
            
            var position = m_UseLookSourcePosition ? CharacterLocomotion.LookSource.LookPosition(true) : origin.position;
            position = MathUtility.TransformPoint(position, CharacterTransform.rotation, m_PositionOffset);
            
            m_Origin = position;
            m_Direction = direction;
            
            int hitCount;
            // Raycast and Spherecast use RaycastHit[], while OverlapSphere uses Collider[].
            if (m_Mode == CastMode.Raycast || m_Mode == CastMode.SphereCast) {
                if (m_HitRaycasts == null) {
                    m_HitRaycasts = new RaycastHit[m_MaxCollisionCount];
                }
                if (m_Mode == CastMode.Raycast) {
                    hitCount = Physics.RaycastNonAlloc(position, direction, m_HitRaycasts, m_Distance, m_Layers, m_TriggerInteraction);
                } else {
                    m_Origin = position - direction * m_Radius;
                    
                    hitCount = Physics.SphereCastNonAlloc(m_Origin, m_Radius, direction, m_HitRaycasts, m_Distance, m_Layers, m_TriggerInteraction);
                }
                for (int i = 0; i < hitCount; ++i) {
                    MagicAction.PerformImpact(m_CastID, GameObject, m_HitRaycasts[i].transform.gameObject, m_HitRaycasts[i]);
                }
                return;
            }

            // OverlapSphere.
            if (m_HitColliders == null) {
                m_HitColliders = new Collider[m_MaxCollisionCount];
            }
            hitCount = Physics.OverlapSphereNonAlloc(position, m_Radius, m_HitColliders, m_Layers, m_TriggerInteraction);
            for (int i = 0; i < hitCount; ++i) {
                var hitCollider = m_HitColliders[i];
                
                // The cast cannot hit the current character.
                var isCharacterCollider = hitCollider.transform.IsChildOf(CharacterTransform);
                if (!m_AllowSelfImpact && isCharacterCollider) {
                    continue;
                }

                // The object must be within view.
                Vector3 colliderPosition;
                PivotOffset pivotOffset;
                if ((pivotOffset = hitCollider.gameObject.GetCachedComponent<PivotOffset>()) != null) {
                    colliderPosition = hitCollider.transform.TransformPoint(pivotOffset.Offset);
                } else {
                    colliderPosition = hitCollider.transform.position;
                }

                if (Physics.Linecast(position, colliderPosition, out var raycastHit, m_Layers, m_TriggerInteraction) == false) {
                    
                    // Perform the impact anyways but on the collider.
                    var hitDirection = (colliderPosition - position).normalized;
                    MagicAction.PerformImpact(m_CastID, origin.gameObject, hitCollider.gameObject, hitCollider, colliderPosition, hitDirection);
                    continue;
                }

                // The object is valid - perform the impact.
                MagicAction.PerformImpact(m_CastID, origin.gameObject, hitCollider.gameObject, raycastHit);
            }
        }
        
        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void StopCast()
        {
            for (int i = 0; i < MagicAction.ImpactModuleGroup.EnabledModules.Count; i++) {
                var module = MagicAction.ImpactModuleGroup.EnabledModules[i];
                if(module == null){ continue; }
                module.Reset(m_CastID);
            }

            base.StopCast();
        }
    }
}