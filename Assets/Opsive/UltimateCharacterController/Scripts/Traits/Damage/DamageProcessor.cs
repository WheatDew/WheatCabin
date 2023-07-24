/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits.Damage
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using UnityEngine;

    /// <summary>
    /// Container class which holds the data associated with damaging a target.
    /// </summary>
    public class DamageData
    {
        [Tooltip("The object that caused the damage.")]
        protected IDamageSource m_DamageSource;
        [Tooltip("The object that is the target.")]
        protected IDamageTarget m_DamageTarget;
        [Tooltip("The amount of damage that should be dealt.")]
        protected float m_Amount;
        [Tooltip("The hit position.")]
        protected Vector3 m_Position;
        [Tooltip("The hit direction.")]
        protected Vector3 m_Direction;
        [Tooltip("The magnitude of the damage force.")]
        protected float m_ForceMagnitude;
        [Tooltip("The number of frames that the force should be applied over.")]
        protected int m_Frames;
        [Tooltip("The radius of the force.")]
        protected float m_Radius;
        [Tooltip("The collider that was hit.")]
        protected Collider m_HitCollider;
        [Tooltip("The collider that was hit.")]
        protected ImpactCallbackContext m_ImpactContext;
        [Tooltip("Object allowing custom user data.")]
        protected object m_UserData;

        public IDamageSource DamageSource { get => m_DamageSource; set => m_DamageSource = value; }
        public IDamageTarget DamageTarget { get => m_DamageTarget; set => m_DamageTarget = value; }
        public float Amount { get => m_Amount; set => m_Amount = value; }
        public Vector3 Position { get => m_Position; set => m_Position = value; }
        public Vector3 Direction { get => m_Direction; set => m_Direction = value; }
        public float ForceMagnitude { get => m_ForceMagnitude; set => m_ForceMagnitude = value; }
        public int Frames { get => m_Frames; set => m_Frames = value; }
        public float Radius { get => m_Radius; set => m_Radius = value; }
        public Collider HitCollider { get => m_HitCollider; set => m_HitCollider = value; }
        public ImpactCallbackContext ImpactContext { get => m_ImpactContext; set => m_ImpactContext = value; }
        public object UserData { get => m_UserData; set => m_UserData = value; }

        private DefaultDamageSource m_CachedDamageSource = new DefaultDamageSource();
        
        /// <summary>
        /// Initializes the DamageData to the spciefied parameters.
        /// </summary>
        public virtual void SetDamage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, object attackerObject, Collider hitCollider)
        {
            m_ImpactContext = null;
            m_CachedDamageSource.OwnerDamageSource = null;
            m_CachedDamageSource.SourceOwner = attacker;
            if (attackerObject is Component component) {
                m_CachedDamageSource.SourceGameObject = component.gameObject;
                m_CachedDamageSource.SourceComponent = component;
            } else if (attackerObject is GameObject attackerGO) {
                m_CachedDamageSource.SourceGameObject = attackerGO;
                m_CachedDamageSource.SourceComponent = null;
            } else {
                m_CachedDamageSource.SourceGameObject = null;
                m_CachedDamageSource.SourceComponent = null;
            }
            m_DamageSource = m_CachedDamageSource;
            m_Amount = amount;
            m_Position = position;
            m_Direction = direction;
            m_ForceMagnitude = forceMagnitude;
            m_Frames = frames;
            m_Radius = radius;
            m_HitCollider = hitCollider;
        }

        /// <summary>
        /// Initializes the DamageData to the parameters.
        /// </summary>
        public virtual void SetDamage(ImpactCallbackContext impactContext, float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, Collider hitCollider)
        {
            m_ImpactContext = impactContext;
            var impactData = impactContext.ImpactCollisionData;
            m_DamageSource = impactData.DamageSource;
            m_Amount = amount;
            m_Position = position;
            m_Direction = direction;
            m_ForceMagnitude = forceMagnitude;
            m_Frames = frames;
            m_Radius = radius;
            m_HitCollider = hitCollider;
        }
        
        /// <summary>
        /// Initializes the DamageData to the parameters.
        /// </summary>
        public virtual void SetDamage(IDamageSource damageSource, float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, Collider hitCollider)
        {
            m_ImpactContext = null;
            m_DamageSource = damageSource;
            m_Amount = amount;
            m_Position = position;
            m_Direction = direction;
            m_ForceMagnitude = forceMagnitude;
            m_Frames = frames;
            m_Radius = radius;
            m_HitCollider = hitCollider;
        }

        /// <summary>
        /// Copies the specified DamageData to the current object.
        /// </summary>
        /// <param name="damageData">The DamageData that should be copied.</param>
        public virtual void Copy(DamageData damageData)
        {
            m_ImpactContext = damageData.ImpactContext;
            m_DamageSource = damageData.DamageSource;
            m_Amount = damageData.Amount;
            m_Position = damageData.Position;
            m_Direction = damageData.Direction;
            m_ForceMagnitude = damageData.ForceMagnitude;
            m_Frames = damageData.Frames;
            m_Radius = damageData.Radius;
            m_HitCollider = damageData.HitCollider;
            m_UserData = damageData.UserData;
        }
    }

    /// <summary>
    /// Specifies an object that can cause damage.
    /// </summary>
    public interface IDamageSource
    {
        // The Damage Source of the owner, when it is nested. For example, Source Character -> ItemAction -> Projectile -> Explosion.
        IDamageSource OwnerDamageSource { get; }
        // The owner of the damage source. For example, the turret for projectiles OR ItemAction for hitbox.
        GameObject SourceOwner { get; }
        // The Source GameObject of the damage. For example, the projectile or explosion GameObject.
        GameObject SourceGameObject { get; }
        // The Source Component of the damage. For example, the Projectile or Explosion Component.
        Component SourceComponent { get; }
    }

    /// <summary>
    /// A utility class used to retrieve specific objects from the DamageSource object
    /// </summary>
    public static class DamageSourceUtility
    {
        /// <summary>
        /// Try to get the character locomotion from a Damage Source.
        /// </summary>
        /// <param name="damageSource">The damage source to get the character locomotion from.</param>
        /// <param name="characterLocomotion">The character locomotion.</param>
        /// <returns>True if a character locomotion exists in the damage source.</returns>
        public static bool TryGetCharacterLocomotion(this IDamageSource damageSource, out CharacterLocomotion characterLocomotion)
        {
            characterLocomotion = damageSource.SourceOwner.GetCachedComponent<CharacterLocomotion>();
            if (characterLocomotion != null) {
                return true;
            }

            if (damageSource.OwnerDamageSource == null || damageSource.OwnerDamageSource == damageSource) {
                return false;
            }

            return TryGetCharacterLocomotion(damageSource.OwnerDamageSource, out characterLocomotion);
        }
        
        /// <summary>
        /// Get the root Damage Source.
        /// </summary>
        /// <param name="damageSource">The damage source to get the root from.</param>
        /// <returns>The root damage source.</returns>
        public static IDamageSource GetRootDamageSource(this IDamageSource damageSource)
        {
            var ownerSource = damageSource.OwnerDamageSource;
            if (ownerSource == null) {
                return damageSource;
            } else {
                return GetRootDamageSource(ownerSource);
            }
        }

        /// <summary>
        /// Get the root owner from the damage source.
        /// </summary>
        /// <param name="damageSource">The damage source to get the root owner from.</param>
        /// <returns>The root owner.</returns>
        public static GameObject GetRootOwner(this IDamageSource damageSource)
        {
            return GetRootDamageSource(damageSource).SourceOwner;
        }
    }

    /// <summary>
    /// Default implementation of IDamageSource.
    /// </summary>
    public class DefaultDamageSource : IDamageSource
    {
        // The Damage Source of the owner, when it is nested. Example Character -> ItemAction -> Projectile -> Explosion.
        private IDamageSource m_OwnerDamageSource;
        // The owner of the damage source. Example Turret for projectiles or ItemAction for hitbox.
        private GameObject m_SourceOwner;
        // The Source GameObject of the damage. Example Projectile or Explosion GameObject.
        private GameObject m_SourceGameObject;
        // The Source Component of the damage. Example Projectile or explosion component.
        private Component m_SourceComponent;

        public IDamageSource OwnerDamageSource { get => m_OwnerDamageSource; set => m_OwnerDamageSource = value; }
        public GameObject SourceOwner  { get => m_SourceOwner; set => m_SourceOwner = value; }
        public GameObject SourceGameObject  { get => m_SourceGameObject; set => m_SourceGameObject = value; }
        public Component SourceComponent  { get => m_SourceComponent; set => m_SourceComponent = value; }

        /// <summary>
        /// Reset the values to default.
        /// </summary>
        public void Reset()
        {
            m_OwnerDamageSource = null;
            m_SourceOwner = null;
            m_SourceGameObject = null;
            m_SourceComponent = null;
        }
    }

    /// <summary>
    /// Specifies an object that can receive damage.
    /// </summary>
    public interface IDamageTarget
    {
        // The GameObject that receives damage.
        GameObject Owner { get; }
        // The GameObject that was hit. This can be a child of the Owner.
        GameObject HitGameObject { get; }

        /// <summary>
        /// Damages the object.
        /// </summary>
        /// <param name="damageData">The damage received.</param>
        void Damage(DamageData damageData);

        /// <summary>
        /// Is the object alive?
        /// </summary>
        /// <returns>True if the object is alive.</returns>
        bool IsAlive();
    }

    /// <summary>
    /// A ScriptableObject which applies the damage to the IDamageTarget.
    /// </summary>
    public class DamageProcessor : ScriptableObject
    {
        private static DamageProcessor m_Default;
        public static DamageProcessor Default {
            get
            {
                if (m_Default == null) {
                    m_Default = CreateInstance<DamageProcessor>();
                }

                return m_Default;
            }
        }

        /// <summary>
        /// Processes the DamageData on the DamageTarget.
        /// </summary>
        /// <param name="target">The object receiving the damage.</param>
        /// <param name="damageData">The damage data to be applied to the target.</param>
        public virtual void Process(IDamageTarget target, DamageData damageData)
        {
            target.Damage(damageData);
        }
    }

    /// <summary>
    /// A small utility class which retrieves the IDamageTarget.
    /// </summary>
    public static class DamageUtility
    {
        /// <summary>
        /// Returns the IDamageTarget on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject that contains the IDamageTarget.</param>
        /// <returns>The IDamageTarget on the specified GameObject.</returns>
        public static IDamageTarget GetDamageTarget(GameObject gameObject)
        {
            var damageTarget = gameObject.GetCachedParentComponent<IDamageTarget>();
            if (damageTarget != null) {
                return damageTarget;
            }
            damageTarget = gameObject.GetCachedComponent<IDamageTarget>();
            
            // The hit object could be a collider in the first person hands.
#if FIRST_PERSON_CONTROLLER
            var firstpersonObjects = gameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>();
            if (firstpersonObjects != null) {
                return firstpersonObjects.Character.GetCachedComponent<IDamageTarget>();
            }
#endif
            
            return damageTarget;
        }
    }
}