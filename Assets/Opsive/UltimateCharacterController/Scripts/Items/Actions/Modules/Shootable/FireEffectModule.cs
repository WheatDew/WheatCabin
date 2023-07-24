/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for fire effects invoked when a weapon is fired.
    /// </summary>
    [Serializable]
    public abstract class ShootableFireEffectModule : ShootableActionModule
    {
        /// <summary>
        /// Invoke the fire effects.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public abstract void InvokeEffects(ShootableUseDataStream dataStream);
    }

    /// <summary>
    /// A generic item effect module when firing a shootable action.
    /// </summary>
    [Serializable]
    public class GenericItemEffects : ShootableFireEffectModule
    {
        [Tooltip("The item effect group is a list of effects.")]
        [SerializeField] protected ItemEffectGroup m_EffectGroup;

        public ItemEffectGroup EffectGroup { get => m_EffectGroup; set => m_EffectGroup = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            
            m_EffectGroup.Initialize(this);
        }

        /// <summary>
        /// Adds any effects (muzzle flash, shell, recoil, etc) to the fire position.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void InvokeEffects(ShootableUseDataStream dataStream)
        {
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
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
            return GetToStringPrefix()+$"Generic ({m_EffectGroup.Effects.Length}): " + ListUtility.ToStringDeep(m_EffectGroup.Effects, true);
        }
    }
    
    /// <summary>
    /// Spawn a muzzle effect when the weapon fires.
    /// </summary>
    [Serializable]
    public class MuzzleEffect : ShootableFireEffectModule
    {
        [Tooltip("A reference to the muzzle flash prefab.")]
        [SerializeField] protected GameObject m_MuzzleFlash;
        [Tooltip("Should the muzzle flash be pooled? If false a single muzzle flash object will be used.")]
        [SerializeField] protected bool m_PoolMuzzleFlash = true;
        [Tooltip("The location of the muzzle flash.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_MuzzleLocation;

        public GameObject MuzzleFlash { get { return m_MuzzleFlash; } set { m_MuzzleFlash = value; } }
        public bool PoolMuzzleFlash { get { return m_PoolMuzzleFlash; } set { m_PoolMuzzleFlash = value; } }
        public ItemPerspectiveIDObjectProperty<Transform> MuzzleLocation { get { return m_MuzzleLocation; } set { m_MuzzleLocation = value; } }

        private GameObject m_SpawnedMuzzleFlash;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_MuzzleLocation.Initialize(itemAction);
        }

        /// <summary>
        /// Get the transform location of the muzzle flash.
        /// </summary>
        /// <returns>The muzzle flash location.</returns>
        public virtual Transform GetMuzzleFlashLocation()
        {
            return m_MuzzleLocation.GetValue();
        }

        /// <summary>
        /// Adds any effects (muzzle flash, shell, recoil, etc) to the fire position.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void InvokeEffects(ShootableUseDataStream dataStream)
        {
            // Spawn a muzzle flash.
            if (m_MuzzleFlash != null) {
                SpawnMuzzleFlash();
            }
        }

        /// <summary>
        /// Spawns the muzzle flash.
        /// </summary>
        public void SpawnMuzzleFlash()
        {
            if (m_PoolMuzzleFlash || m_SpawnedMuzzleFlash == null) {
                m_SpawnedMuzzleFlash = ObjectPoolBase.Instantiate(m_MuzzleFlash, Vector3.zero, Quaternion.identity);
            }
            var muzzleFlashLocation = GetMuzzleFlashLocation();
            m_SpawnedMuzzleFlash.transform.parent = muzzleFlashLocation;
            m_SpawnedMuzzleFlash.transform.localScale = m_MuzzleFlash.transform.localScale;
            m_SpawnedMuzzleFlash.transform.position = muzzleFlashLocation.position + m_MuzzleFlash.transform.position;
            // Choose a random z rotation angle.
            var eulerAngles = m_MuzzleFlash.transform.eulerAngles;
            eulerAngles.z = UnityEngine.Random.Range(0, 360);
            m_SpawnedMuzzleFlash.transform.localRotation = Quaternion.Euler(eulerAngles);

            var muzzleFlashObj = m_SpawnedMuzzleFlash.GetCachedComponent<MuzzleFlash>();
            if (muzzleFlashObj != null) {
                muzzleFlashObj.Show(CharacterItem, SlotID, m_MuzzleLocation, m_PoolMuzzleFlash, CharacterLocomotion);
            }
        }

        /// <summary>
        /// The module has been added to the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was added to.</param>
        public override void OnEditorModuleAdded(GameObject gameObject)
        {
            base.OnEditorModuleAdded(gameObject);

            m_MuzzleLocation = new ItemPerspectiveIDObjectProperty<Transform>();

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                return;
            }
#endif

#if FIRST_PERSON_CONTROLLER
            var firstPersonPerspectiveItem = gameObject.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspectiveItem != null && firstPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("MuzzleFlashLocation").transform;
                location.SetParentOrigin(firstPersonPerspectiveItem.GetVisibleObject().transform);
                m_MuzzleLocation.SetFirstPersonValue(location);
            }
#endif
            var thirdPersonPerspectiveItem = gameObject.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonPerspectiveItem != null && thirdPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("MuzzleFlashLocation").transform;
                location.SetParentOrigin(thirdPersonPerspectiveItem.GetVisibleObject().transform);
                m_MuzzleLocation.SetThirdPersonValue(location);
            }
        }

        /// <summary>
        /// The module has been removed from the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was removed from.</param>
        public override void OnEditorModuleRemoved(GameObject gameObject)
        {
            base.OnEditorModuleRemoved(gameObject);

            m_MuzzleLocation.OnEditorDestroyObjectCleanup(gameObject);
        }
    }
    
    /// <summary>
    /// Spawns shells when firing shots.
    /// </summary>
    [Serializable]
    public class ShellEffect : ShootableFireEffectModule
    {
        [Tooltip("A reference to the shell prefab.")]
        [SerializeField] protected GameObject m_Shell;
        [Tooltip("The velocity that the shell should eject at.")]
        [SerializeField] protected MinMaxVector3 m_ShellVelocity = new MinMaxVector3(new Vector3(3, 0, 0), new Vector3(4, 2, 0));
        [Tooltip("The torque that the projectile should initialize with.")]
        [SerializeField] protected MinMaxVector3 m_ShellTorque = new MinMaxVector3(new Vector3(-50, -50, -50), new Vector3(50, 50, 50));
        [Tooltip("Eject the shell after the specified delay.")]
        [SerializeField] protected float m_ShellEjectDelay;
        [Tooltip("The location that the shell ejects from.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_ShellLocation;

        public GameObject Shell { get => m_Shell; set => m_Shell = value; }
        public MinMaxVector3 ShellVelocity { get => m_ShellVelocity; set => m_ShellVelocity = value; }
        public MinMaxVector3 ShellTorque { get => m_ShellTorque; set => m_ShellTorque = value; }
        public float ShellEjectDelay { get => m_ShellEjectDelay; set => m_ShellEjectDelay = value; }
        public ItemPerspectiveIDObjectProperty<Transform> ShellLocation { get => m_ShellLocation; set => m_ShellLocation = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_ShellLocation.Initialize(itemAction);
        }

        /// <summary>
        /// Invoke the fire effects.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void InvokeEffects(ShootableUseDataStream dataStream)
        {
            // Spawn a shell.
            if (m_Shell != null) {
                Scheduler.Schedule(m_ShellEjectDelay, EjectShell);
            }
        }
        
        /// <summary>
        /// Ejects the shell.
        /// </summary>
        private void EjectShell()
        {
            var shellLocation = m_ShellLocation.GetValue();
            var shell = ObjectPoolBase.Instantiate(m_Shell, shellLocation.position, shellLocation.rotation);
            var shellObj = shell.GetCachedComponent<Shell>();
            if (shellObj != null) {
                var visibleObject = CharacterItem.ActivePerspectiveItem.GetVisibleObject();
                shellObj.Initialize(visibleObject.transform.TransformDirection(m_ShellVelocity.RandomValue), m_ShellTorque.RandomValue, Character);
            }
        }

        /// <summary>
        /// The module has been added to the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was added to.</param>
        public override void OnEditorModuleAdded(GameObject gameObject)
        {
            base.OnEditorModuleAdded(gameObject);

            m_ShellLocation = new ItemPerspectiveIDObjectProperty<Transform>();

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                return;
            }
#endif

#if FIRST_PERSON_CONTROLLER
            var firstPersonPerspectiveItem = gameObject.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspectiveItem != null && firstPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("ShellLocation").transform;
                location.SetParentOrigin(firstPersonPerspectiveItem.GetVisibleObject().transform);
                m_ShellLocation.SetFirstPersonValue(location);
            }
#endif
            var thirdPersonPerspectiveItem = gameObject.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonPerspectiveItem != null && thirdPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("ShellLocation").transform;
                location.SetParentOrigin(thirdPersonPerspectiveItem.GetVisibleObject().transform);
                m_ShellLocation.SetThirdPersonValue(location);
            }
        }

        /// <summary>
        /// The module has been removed from the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was removed from.</param>
        public override void OnEditorModuleRemoved(GameObject gameObject)
        {
            base.OnEditorModuleRemoved(gameObject);

            m_ShellLocation.OnEditorDestroyObjectCleanup(gameObject);
        }
    }
    
    /// <summary>
    /// A Shot effect that spawns a puff of smoke.
    /// </summary>
    [Serializable]
    public class SmokeEffect : ShootableFireEffectModule
    {
        [Tooltip("A reference to the smoke prefab.")]
        [SerializeField] protected GameObject m_Smoke;
        [Tooltip("Spawn the smoke after the specified delay.")]
        [SerializeField] protected float m_SmokeSpawnDelay;
        [Tooltip("The smoke location.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_SmokeLocation;
        
        public GameObject Smoke { get { return m_Smoke; } set { m_Smoke = value; } }
        public float SmokeSpawnDelay { get { return m_SmokeSpawnDelay; } set { m_SmokeSpawnDelay = value; } }
        public ItemPerspectiveIDObjectProperty<Transform> SmokeLocation { get => m_SmokeLocation; set => m_SmokeLocation = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_SmokeLocation.Initialize(itemAction);
        }
        
        /// <summary>
        /// Adds any effects (muzzle flash, shell, recoil, etc) to the fire position.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void InvokeEffects(ShootableUseDataStream dataStream)
        {
            // Spawn the smoke.
            if (m_Smoke != null) {
                Scheduler.Schedule(m_SmokeSpawnDelay, SpawnSmoke);
            }
        }
        
        /// <summary>
        /// Spawns the shell.
        /// </summary>
        private void SpawnSmoke()
        {
            var smokeLocation = GetSmokeLocation();
            var smoke = ObjectPoolBase.Instantiate(m_Smoke, smokeLocation.position, smokeLocation.rotation);
            var smokeObj = smoke.GetCachedComponent<Smoke>();
            if (smokeObj != null) {
                smokeObj.Show(CharacterItem, SlotID, m_SmokeLocation, CharacterLocomotion);
            }
        }

        /// <summary>
        /// Get the smoke transform location.
        /// </summary>
        /// <returns>The smoke location.</returns>
        public virtual Transform GetSmokeLocation()
        {
            return m_SmokeLocation.GetValue();
        }
    }
    
    /// <summary>
    /// A module that adds a recoil effect on each shot.
    /// </summary>
    [Serializable]
    public class RecoilEffect : ShootableFireEffectModule
    {
        [Tooltip("The amount of positional recoil to add to the item.")]
        [SerializeField] protected MinMaxVector3 m_PositionRecoil = new MinMaxVector3(new Vector3(0, 0, -0.3f), new Vector3(0, 0.01f, -0.1f));
        [Tooltip("The amount of rotational recoil to add to the item.")]
        [SerializeField] protected MinMaxVector3 m_RotationRecoil = new MinMaxVector3(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0));
        [Tooltip("The amount of positional recoil to add to the camera.")]
        [SerializeField] protected MinMaxVector3 m_PositionCameraRecoil;
        [Tooltip("The amount of rotational recoil to add to the camera.")]
        [SerializeField] protected MinMaxVector3 m_RotationCameraRecoil = new MinMaxVector3(new Vector3(-2f, -1f, 0), new Vector3(-1f, 1f, 0));
        [Tooltip("The percent of the recoil to accumulate to the camera's rest value.")]
        [Range(0, 1)] [SerializeField] protected float m_CameraRecoilAccumulation;
        [Tooltip("Is the recoil force localized to the direct parent?")]
        [SerializeField] protected bool m_LocalizeRecoilForce;
        
        public MinMaxVector3 PositionRecoil { get { return m_PositionRecoil; } set { m_PositionRecoil = value; } }
        public MinMaxVector3 RotationRecoil { get { return m_RotationRecoil; } set { m_RotationRecoil = value; } }
        public MinMaxVector3 PositionCameraRecoil { get { return m_PositionCameraRecoil; } set { m_PositionCameraRecoil = value; } }
        public MinMaxVector3 RotationCameraRecoil { get { return m_RotationCameraRecoil; } set { m_RotationCameraRecoil = value; } }
        public float CameraRecoilAccumulation { get { return m_CameraRecoilAccumulation; } set { m_CameraRecoilAccumulation = value; } }
        public bool LocalizeRecoilForce { get { return m_LocalizeRecoilForce; } set { m_LocalizeRecoilForce = value; } }
        
        /// <summary>
        /// Adds any effects (muzzle flash, shell, recoil, etc) to the fire position.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void InvokeEffects(ShootableUseDataStream dataStream)
        {
            Opsive.Shared.Events.EventHandler.ExecuteEvent(Character, "OnAddSecondaryForce", SlotID, m_PositionRecoil.RandomValue, m_RotationRecoil.RandomValue, !m_LocalizeRecoilForce);
            Opsive.Shared.Events.EventHandler.ExecuteEvent(Character, "OnAddSecondaryCameraForce", m_PositionCameraRecoil.RandomValue, m_RotationCameraRecoil.RandomValue, m_CameraRecoilAccumulation);
        }
    }

    /// <summary>
    /// This module allows crosshair spread when firing with the shootable weapon.
    /// </summary>
    [Serializable]
    public class CrosshairsSpread : ShootableFireEffectModule
    {
        /// <summary>
        /// Invoke the fire effects.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void InvokeEffects(ShootableUseDataStream dataStream)
        {
            Shared.Events.EventHandler.ExecuteEvent(Character, "OnAddCrosshairsSpread", true, true);
        }
    }
}