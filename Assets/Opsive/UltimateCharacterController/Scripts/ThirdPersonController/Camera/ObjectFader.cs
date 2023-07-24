/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Can fade the character's materials if the camera gets too close or any materials which are obstructing the view between the camera and the character.
    /// </summary>
    public class ObjectFader : StateBehavior
    {
        [Tooltip("The name of the material property that should be faded.")]
        [SerializeField] protected string m_ColorPropertyName = "_Color";
        [Tooltip("Should the character fade out when the camera is near?")]
        [SerializeField] protected bool m_CharacterFade = true;
        [Tooltip("Should the character materials be cached at the start? If false the material values will be saved each time the character starts to fade.")]
        [SerializeField] protected bool m_CacheCharacterMaterials = true;
        [Tooltip("The distance between the character and camera that the character materials should start to fade.")]
        [SerializeField] protected float m_StartFadeDistance = 1.8f;
        [Tooltip("The distance between the character and camera that the character materials should be completely invisible.")]
        [SerializeField] protected float m_EndFadeDistance = 1;
        [Tooltip("Prevents the character change from updating for the specified number of seconds after a state change.")]
        [SerializeField] protected float m_CharacterFadeStateChangeCooldown = 0.3f;
        [Tooltip("Should any objects obstructing the camera's view fade out?")]
        [SerializeField] protected bool m_ObstructingObjectsFade;
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("Specifies the speed at which the obstructing material can fade.")]
        [SerializeField] protected float m_FadeSpeed = 0.02f;
        [Tooltip("The color that the obstructed object will fade to.")]
        [SerializeField] protected Color m_FadeColor = new Color(1, 1, 1, 0);
        [Tooltip("Should the material mode be set automatically when an object is obstructing the view?")]
        [SerializeField] protected bool m_AutoSetMode;
        [Tooltip("Should the obstructing object's collider be disabled when the material is faded?")]
        [SerializeField] protected bool m_DisableCollider = true;
        [Tooltip("The maximum number of obstructing colliders that can be faded at one time.")]
        [SerializeField] protected int m_MaxObstructingColliderCount = 20;
        [Tooltip("The maximum number of obstricting materials that can be faded at one time. This value should be greater than the collider count.")]
        [SerializeField] protected int m_MaxObstructingMaterialCount = 30;
        [Tooltip("The offset to apply to the character when determining if the character should fade/is considered obstructed.")]
        [SerializeField] protected Vector3 m_TransformOffset = new Vector3(0, 0.9f, 0);

        public bool CharacterFade
        {
            get { return m_CharacterFade; }
            set {
                m_CharacterFade = value; if (value) {
                    if (m_RegisteredMaterial == null) {
                        m_RegisteredMaterial = new HashSet<Material>();
                        m_MaterialModeSet = new HashSet<Material>();
                    }
                } else {
                    DisableCharacterFade();
                }
            }
        }
        public float StartFadeDistance { get { return m_StartFadeDistance; } set { m_StartFadeDistance = value; } }
        public float EndFadeDistance { get { return m_EndFadeDistance; } set { m_EndFadeDistance = value; } }
        public float CharacterFadeStateChangeCooldown { get { return m_CharacterFadeStateChangeCooldown; } set { m_CharacterFadeStateChangeCooldown = value; } }
        public bool ObstructingObjectsFade { get { return m_ObstructingObjectsFade; } set { m_ObstructingObjectsFade = value; if (!value) { DisableObstructingFade(); } } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public float FadeSpeed { get { return m_FadeSpeed; } set { m_FadeSpeed = value; } }
        public Color FadeColor { get { return m_FadeColor; } set { m_FadeColor = value; } }
        public bool AutoSetMode { get { return m_AutoSetMode; } set { m_AutoSetMode = value; } }
        public bool DisableCollider { get { return m_DisableCollider; } set { m_DisableCollider = value; } }
        public Vector3 TransformOffset { get { return m_TransformOffset; } set { m_TransformOffset = value; } }

        private Transform m_Transform;
        private CameraController m_CameraController;

        private GameObject m_Character;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private CharacterLayerManager m_CharacterLayerManager;
        private Transform m_CharacterTransform;
        private Material[] m_CharacterFadeMaterials;

        private int m_IndependentCharacterFadeCount;
        private int m_CharacterFadeMaterialsCount;
        private HashSet<Material> m_RegisteredMaterial;
        private Vector3 m_FadeOffset;
        private bool m_CharacterFaded;
        private Dictionary<Material, OriginalMaterialValue> m_OriginalMaterialValuesMap = new Dictionary<Material, OriginalMaterialValue>();
        private RaycastHit[] m_RaycastsHit;
        private Material[] m_ObstructingMaterials;
        private int m_ObstructingMaterialsCount;
        private Collider[] m_ObstructingColliders;
        private int m_ObstructingCollidersCount;
        private HashSet<Material> m_ObstructionHitSet;
        private Dictionary<Material, bool> m_CanObstructionFade;
        private HashSet<Material> m_MaterialModeSet;
        private float m_CharacterFadeCooldownElapsedTime;

        private int m_ColorID;

        public int IndependentCharacterFadeCount { get { return m_IndependentCharacterFadeCount; } set { m_IndependentCharacterFadeCount = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            // PropertyToID cannot be initialized within a MonoBehaviour constructor.
            m_ColorID = Shader.PropertyToID(m_ColorPropertyName);

            base.Awake();

            m_Transform = transform;
            m_CameraController = gameObject.GetCachedComponent<CameraController>();
            if (m_CharacterFade) {
                m_RegisteredMaterial = new HashSet<Material>();
                m_MaterialModeSet = new HashSet<Material>();
            }
            if (m_ObstructingObjectsFade) {
                m_RaycastsHit = new RaycastHit[m_MaxObstructingColliderCount];
                m_ObstructingMaterials = new Material[m_MaxObstructingMaterialCount];
                m_ObstructingColliders = new Collider[m_MaxObstructingMaterialCount];
                m_ObstructionHitSet = new HashSet<Material>();
                m_CanObstructionFade = new Dictionary<Material, bool>();
            }
            m_CharacterFadeCooldownElapsedTime = -m_CharacterFadeStateChangeCooldown;

            EventHandler.RegisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);

            // Enable after the character has been attached.
            enabled = false;
        }

        /// <summary>
        /// Attaches the component to the specified character.
        /// </summary>
        /// <param name="character">The handler to attach the camera to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            enabled = character != null && !m_CameraController.ActiveViewType.FirstPersonPerspective;

            // Disable the fade on the previous active character.
            if (m_CharacterFade) {
                if (m_Character != null && m_Character != character) {
                    DisableFades();

                    // Clear the previous mappings.
                    if (m_CharacterFadeMaterials != null) {
                        if (m_CacheCharacterMaterials) {
                            for (int i = 0; i < m_CharacterFadeMaterials.Length; ++i) {
                                if (m_CharacterFadeMaterials[i] == null || !m_OriginalMaterialValuesMap.ContainsKey(m_CharacterFadeMaterials[i])) {
                                    continue;
                                }
                                GenericObjectPool.Return(m_OriginalMaterialValuesMap[m_CharacterFadeMaterials[i]]);
                                m_OriginalMaterialValuesMap.Remove(m_CharacterFadeMaterials[i]);
                            }
                        }
                        m_CharacterFadeMaterials = null;
                    }
                    m_OriginalMaterialValuesMap.Clear();

                    EventHandler.UnregisterEvent<bool>(m_Character, "OnCameraChangePerspectives", OnChangePerspectives);
                    EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnCharacterIndependentFade", OnIndependentFade);
                    EventHandler.UnregisterEvent<CharacterItem>(m_Character, "OnInventoryAddItem", OnAddItem);
                    EventHandler.UnregisterEvent<GameObject, bool>(m_Character, "OnShootableWeaponShowProjectile", OnShowProjectile);
                    EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
                }
            }

            m_Character = character;

            if (m_Character != null) {
                m_CharacterTransform = m_Character.transform;
                m_CharacterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
                m_CharacterLayerManager = m_Character.GetCachedComponent<CharacterLayerManager>();
                if (m_CharacterFade) {
                    // Determine the number of renderers that will be faded so their materials can be cached.
                    m_RegisteredMaterial.Clear();
                    var count = 0;
                    var renderers = m_Character.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < renderers.Length; ++i) {
                        // The renderer fade can be ignored.
                        if (renderers[i].gameObject.GetCachedComponent<IgnoreFadeIdentifier>() != null) {
                            continue;
                        }
                        var materials = renderers[i].materials;
                        for (int j = 0; j < materials.Length; ++j) {
                            if (materials[j].HasProperty(m_ColorID)) {
                                count++;
                            }
                        }
                    }

                    if (count > 0) {
                        if (m_CharacterFadeMaterials == null) {
                            m_CharacterFadeMaterials = new Material[count];
                        } else if (m_CharacterFadeMaterials.Length != count) {
                            if (m_CacheCharacterMaterials) {
                                // The mapping may exist from a previous character.
                                for (int i = 0; i < m_CharacterFadeMaterials.Length; ++i) {
                                    GenericObjectPool.Return(m_OriginalMaterialValuesMap[m_CharacterFadeMaterials[i]]);
                                    m_OriginalMaterialValuesMap.Remove(m_CharacterFadeMaterials[i]);
                                }
                            }
                            System.Array.Resize(ref m_CharacterFadeMaterials, count);
                        }

                        // Cache a reference to all of the faded materials.
                        m_CharacterFadeMaterialsCount = 0;
                        for (int i = 0; i < renderers.Length; ++i) {
                            // The renderer fade can be ignored.
                            if (renderers[i].gameObject.GetCachedComponent<IgnoreFadeIdentifier>() != null) {
                                continue;
                            }

                            var materials = renderers[i].materials;
                            for (int j = 0; j < materials.Length; ++j) {
                                if (m_RegisteredMaterial.Contains(materials[j])) {
                                    continue;
                                }
                                if (materials[j].HasProperty(m_ColorID)) {
                                    if (materials[j].HasProperty(OriginalMaterialValue.ModeID)) {
                                        m_MaterialModeSet.Add(materials[j]);
                                    }
                                    m_CharacterFadeMaterials[m_CharacterFadeMaterialsCount] = materials[j];
                                    m_RegisteredMaterial.Add(materials[j]);
                                    m_CharacterFadeMaterialsCount++;

                                    if (m_CacheCharacterMaterials) {
                                        var originalMaterialValues = GenericObjectPool.Get<OriginalMaterialValue>();
                                        originalMaterialValues.Initialize(materials[j], m_ColorID, m_MaterialModeSet.Contains(materials[j]));
                                        m_OriginalMaterialValuesMap.Add(materials[j], originalMaterialValues);
                                    }
                                }
                            }
                        }
                    }

                    EventHandler.RegisterEvent<bool>(m_Character, "OnCameraChangePerspectives", OnChangePerspectives);
                    EventHandler.RegisterEvent<CharacterItem>(m_Character, "OnInventoryAddItem", OnAddItem);
                    EventHandler.RegisterEvent<GameObject, bool>(m_Character, "OnShootableWeaponShowProjectile", OnShowProjectile);
                    EventHandler.RegisterEvent<bool, bool>(m_Character, "OnCharacterIndependentFade", OnIndependentFade);
                    EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
                }

                // Fade the obstructing objects immediately after the character has been assigned.
                if (m_ObstructingObjectsFade) {
                    FadeObstructingObjects(true);
                }
            }
        }

        /// <summary>
        /// Update the fade values after the scene has finished positioning.
        /// </summary>
        private void Update()
        {
            FadeCharacter();

            FadeObstructingObjects(false);
        }

        /// <summary>
        /// Fade the character to prevent the camera from seeing inside the character if the camera gets too close.
        /// </summary>
        private void FadeCharacter()
        {
            if (!m_CharacterFade || Time.time < m_CharacterFadeCooldownElapsedTime) {
                return;
            }

            m_FadeOffset = MathUtility.InverseTransformPoint(m_CharacterTransform.TransformPoint(m_TransformOffset), m_CharacterTransform.rotation, m_Transform.position);
            // Other components can control when the materials fade.
            if (m_IndependentCharacterFadeCount != 0) {
                return;
            }
            if (m_FadeOffset.magnitude <= m_StartFadeDistance) {
                EnableCharacterFade(m_FadeOffset.magnitude);
            } else if (m_CharacterFaded) {
                // The camera is not near the character - no fade necessary.
                DisableCharacterFade();
            }
        }

        /// <summary>
        /// The MaterialSwapper component has started or stopped rendering.
        /// </summary>
        /// <param name="start">Did the MaterialSwapper component start to render?</param>
        public void MultiCameraRender(bool start)
        {
            if (!enabled) {
                return;
            }

            if (m_CharacterFade) {
                m_FadeOffset = MathUtility.InverseTransformPoint(m_CharacterTransform.TransformPoint(m_TransformOffset), m_CharacterTransform.rotation, m_Transform.position);
                if (start && m_FadeOffset.magnitude <= m_StartFadeDistance) {
                    EnableCharacterFade(m_FadeOffset.magnitude);
                } else if (!start) {
                    DisableCharacterFade();
                }
            }
        }

        /// <summary>
        /// Starts to fade the character materials.
        /// </summary>
        /// <param name="distance">The distance from the character and camera.</param>
        private void EnableCharacterFade(float distance)
        {
            if (m_CharacterFadeMaterials != null) {
                // Slowly fade the character away as the camera gets closer.
                var amount = Mathf.Clamp01((distance - m_EndFadeDistance) / (m_StartFadeDistance - m_EndFadeDistance));
                for (int i = 0; i < m_CharacterFadeMaterialsCount; ++i) {
                    if (!m_CharacterFaded) {
                        EnableFadeMaterial(m_CharacterFadeMaterials[i]);
                    }
                    var color = m_CharacterFadeMaterials[i].GetColor(m_ColorID);
                    color.a = Mathf.Lerp(0, 1, amount);
                    m_CharacterFadeMaterials[i].SetColor(m_ColorID, color);
                }
            }
            m_CharacterFaded = true;
        }

        /// <summary>
        /// Updates the shader to support a fade.
        /// </summary>
        /// <param name="material">The material to update.</param>
        private void EnableFadeMaterial(Material material)
        {
            // If the character's materials change at runtime then the values need to be saved every time fading is enabled.
            if (!m_CacheCharacterMaterials && !m_OriginalMaterialValuesMap.ContainsKey(material)) {
                var originalMaterialValues = GenericObjectPool.Get<OriginalMaterialValue>();
                originalMaterialValues.Initialize(material, m_ColorID, m_MaterialModeSet.Contains(material));
                m_OriginalMaterialValuesMap.Add(material, originalMaterialValues);
            }

            if (m_MaterialModeSet.Contains(material)) {
                material.SetFloat(OriginalMaterialValue.ModeID, 2);
                material.SetInt(OriginalMaterialValue.SrcBlendID, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt(OriginalMaterialValue.DstBlendID, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            material.EnableKeyword(OriginalMaterialValue.AlphaBlendString);
            material.renderQueue = 3000;
        }

        /// <summary>
        /// Reverts all of the character materials back to the original non-faded value.
        /// </summary>
        private void DisableCharacterFade()
        {
            if (m_CharacterFadeMaterials != null) {
                for (int i = 0; i < m_CharacterFadeMaterialsCount; ++i) {
                    if (m_OriginalMaterialValuesMap.TryGetValue(m_CharacterFadeMaterials[i], out var materialValue)) {
                        RevertMaterial(m_CharacterFadeMaterials[i], materialValue);
                        if (!m_CacheCharacterMaterials) {
                            m_OriginalMaterialValuesMap.Remove(m_CharacterFadeMaterials[i]);
                        }
                    }
                }
            }
            m_CharacterFaded = false;
        }

        /// <summary>
        /// Reverts the material back to the original non-faded value.
        /// </summary>
        /// <param name="material">The material to revert.</param>
        /// <param name="originalMaterialValue">A storage struct with the original values.</param>
        private void RevertMaterial(Material material, OriginalMaterialValue originalMaterialValue)
        {
            material.SetColor(m_ColorID, originalMaterialValue.Color);
            if (originalMaterialValue.ContainsMode) {
                material.SetFloat(OriginalMaterialValue.ModeID, originalMaterialValue.Mode);
                material.SetInt(OriginalMaterialValue.SrcBlendID, originalMaterialValue.SrcBlend);
                material.SetInt(OriginalMaterialValue.DstBlendID, originalMaterialValue.DstBlend);
            }
            if (!originalMaterialValue.AlphaBlend) {
                material.DisableKeyword(OriginalMaterialValue.AlphaBlendString);
            }
            material.renderQueue = originalMaterialValue.RenderQueue;
        }

        /// <summary>
        /// Fade any objects that get in the way between the character and the camera.
        /// </summary>
        /// <param name="immediateFade">Should the fade material be changed immediately?</param>
        private void FadeObstructingObjects(bool immediateFade)
        {
            if (!m_ObstructingObjectsFade) {
                return;
            }

            // Disable any obstructing colliders so the sphere cast can detect which objects are obstructing.
            for (int i = 0; i < m_ObstructingCollidersCount; ++i) {
                m_ObstructingColliders[i].enabled = true;
            }
            m_ObstructingCollidersCount = 0;

            var characterPosition = m_CharacterTransform.TransformPoint(m_TransformOffset);
            var direction = (m_Transform.position - characterPosition);
            var start = characterPosition - direction.normalized * m_CollisionRadius;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            // Fire a sphere to prevent the camera from colliding with other objects.
            var hitCount = Physics.SphereCastNonAlloc(start, m_CollisionRadius, direction.normalized, m_RaycastsHit,
                        direction.magnitude, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore);
            m_CharacterLocomotion.EnableColliderCollisionLayer(true);

            m_ObstructionHitSet.Clear();
            if (hitCount > 0) {
                // Loop through all of the hit colliders. For any collider that has been hit get all of the renderers. The materials that are used on the 
                // renderers then need to be checked to determine if they can be faded. If the material can be faded place it in a set which will then
                // be checked in the next block to determine if the material should be faded.
                for (int i = 0; i < hitCount; ++i) {
                    var renderers = m_RaycastsHit[i].transform.gameObject.GetCachedComponents<Renderer>();
                    var obstructing = false;
                    for (int j = 0; j < renderers.Length; ++j) {
                        var materials = renderers[j].materials;
                        for (int k = 0; k < materials.Length; ++k) {
                            if (!m_CanObstructionFade.TryGetValue(materials[k], out var canFade)) {
                                // Objects can fade if they have the color property and can be transparent.
                                canFade = materials[k].HasProperty(m_ColorID) && (m_AutoSetMode || materials[k].renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent);
                                m_CanObstructionFade.Add(materials[k], canFade);
                            }

                            if (canFade) {
                                var material = materials[k];
                                // Any material contained within the hit set should be faded.
                                m_ObstructionHitSet.Add(material);
                                if (m_DisableCollider && !obstructing) {
                                    obstructing = CanMaterialFade(material);
                                }

                                // The same material may be applied to multiple renderers.
                                if (!m_OriginalMaterialValuesMap.ContainsKey(material)) {
                                    // Don't set the mode automatically just because it has the property - not all objects in the environment should fade.
                                    if (m_AutoSetMode && material.HasProperty(OriginalMaterialValue.ModeID)) {
                                        m_MaterialModeSet.Add(material);
                                    }
                                    var originalMaterialValues = GenericObjectPool.Get<OriginalMaterialValue>();
                                    originalMaterialValues.Initialize(material, m_ColorID, m_MaterialModeSet.Contains(material));
                                    m_OriginalMaterialValuesMap.Add(material, originalMaterialValues);

                                    m_ObstructingMaterials[m_ObstructingMaterialsCount] = material;
                                    m_ObstructingMaterialsCount++;

                                    EnableFadeMaterial(material);
                                }
                            }
                        }
                    }

                    // If the object is faded then the collider has the option of being disabled to prevent it from causing collisions.
                    if (m_DisableCollider && obstructing) {
                        m_RaycastsHit[i].collider.enabled = false;
                        m_ObstructingColliders[m_ObstructingCollidersCount] = m_RaycastsHit[i].collider;
                        m_ObstructingCollidersCount++;
                    }
                }
            }

            // Once the obstructing objects have been found they should be faded. Note that this can cause a lot of overdraw so the FadeObject method can be 
            // overridden to provide a custom effect such as the one described on https://madewith.unity.com/stories/dissolving-the-world-part-1.
            for (int i = m_ObstructingMaterialsCount - 1; i >= 0; --i) {
                if (!FadeMaterial(m_ObstructingMaterials[i], m_ObstructionHitSet.Contains(m_ObstructingMaterials[i]), immediateFade, true)) {
                    RemoveObstructingMaterial(i);
                }
            }
        }

        /// <summary>
        /// Can the obstructing material fade?
        /// </summary>
        /// <param name="material">The material that may be able to fade.</param>
        /// <returns>True if the material can fade.</returns>
        public bool CanMaterialFade(Material material)
        {
            // The material can fade if:
            // - The fader can disable the obstructing material's collider AND
            // - The material has the Mode property AND
            // - The Mode property can be set to fade OR
            // - The Mode property is already set to fade.
            return m_DisableCollider && material.HasProperty(OriginalMaterialValue.ModeID) && (m_AutoSetMode || material.GetInt(OriginalMaterialValue.ModeID) == 2);
        }

        /// <summary>
        /// Fades the specified material by the amount.
        /// </summary>
        /// <param name="material">The material to fade.</param>
        /// <param name="fade">Is the object faded?</param>
        /// <param name="immediateFade">Should the fade material be changed immediately?</param>
        /// <param name="alphaFade">Should only the alpha channel be faded?</param>
        /// <returns>True if the object should continue to be faded, false if it is no longer faded.</returns>
        protected virtual bool FadeMaterial(Material material, bool fade, bool immediateFade, bool alphaFade)
        {
            // Set the alpha value to 0 if the object should be faded, otherwise set it to 1. As soon as the alpha value reaches 1 it should no longer be faded so
            // the method can return false.
            var color = material.GetColor(m_ColorID);
            var targetColor = fade ? m_FadeColor : Color.white;
            if (alphaFade) {
                targetColor.r = color.r;
                targetColor.g = color.g;
                targetColor.b = color.b;
            }
            if (immediateFade) {
                color = targetColor;
            } else {
                color.r = Mathf.MoveTowards(color.r, targetColor.r, m_FadeSpeed);
                color.g = Mathf.MoveTowards(color.g, targetColor.g, m_FadeSpeed);
                color.b = Mathf.MoveTowards(color.b, targetColor.b, m_FadeSpeed);
                color.a = Mathf.MoveTowards(color.a, targetColor.a, m_FadeSpeed);
            }
            material.SetColor(m_ColorID, color);

            return color.a != 1;
        }

        /// <summary>
        /// Removes the obstructing material at the specified index from the ObstructingMaterials array.
        /// </summary>
        /// <param name="index">The index of the obstructing material to remove.</param>
        private void RemoveObstructingMaterial(int index)
        {
            var originalMaterialValue = m_OriginalMaterialValuesMap[m_ObstructingMaterials[index]];
            RevertMaterial(m_ObstructingMaterials[index], originalMaterialValue);
            GenericObjectPool.Return(originalMaterialValue);
            m_OriginalMaterialValuesMap.Remove(m_ObstructingMaterials[index]);

            // The object is no longer faded. Move the array elements down one.
            for (int j = index; j < m_ObstructingMaterialsCount - 1; ++j) {
                m_ObstructingMaterials[j] = m_ObstructingMaterials[j + 1];
            }
            m_ObstructingMaterialsCount--;
        }

        /// <summary>
        /// Disables all of the faded obstructing materials.
        /// </summary>
        private void DisableObstructingFade()
        {
            for (int i = m_ObstructingMaterialsCount - 1; i >= 0; --i) {
                RemoveObstructingMaterial(i);
            }

            for (int i = 0; i < m_ObstructingCollidersCount; ++i) {
                if (m_ObstructingColliders[i] != null) {
                    m_ObstructingColliders[i].enabled = true;
                }
            }
            m_ObstructingCollidersCount = 0;
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // While in a first person view no objects should be faded.
            if (firstPersonPerspective) {
                DisableFades();
            }
            enabled = !firstPersonPerspective;
        }

        /// <summary>
        /// Disables the character and obstruction fades.
        /// </summary>
        private void DisableFades()
        {
            DisableCharacterFade();
            DisableObstructingFade();
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="characterItem">The item that was added.</param>
        private void OnAddItem(CharacterItem characterItem)
        {
            // All of the item's materials should be faded when it is added to the character.
            if (m_CharacterFade) {
                var perspectiveItem = characterItem.GetComponent<Items.ThirdPersonPerspectiveItem>();
                if (perspectiveItem == null || perspectiveItem.Object == null) {
                    return;
                }
                InitializeCharacterFadeRenderers(perspectiveItem.Object.GetComponentsInChildren<Renderer>(true));
            }
        }

        /// <summary>
        /// Another object has started or stopped taking control of the character fade.
        /// </summary>
        /// <param name="enable">Is the component controlling the fade?</param>
        /// <param name="revertFade">Should the fade be reverted?</param>
        private void OnIndependentFade(bool enable, bool revertFade)
        {
            m_IndependentCharacterFadeCount += enable ? 1 : -1;
            if (enable && revertFade && m_IndependentCharacterFadeCount == 1) {
                DisableCharacterFade();
            }
        }

        /// <summary>
        /// Initializes the materials on the renderers for character fade.
        /// </summary>
        /// <param name="renderers">The renderers that should be initilaizes for character fade.</param>
        private void InitializeCharacterFadeRenderers(Renderer[] renderers)
        {
            var count = 0;
            for (int i = 0; i < renderers.Length; ++i) {
                // The renderer fade can be ignored.
                if (renderers[i].gameObject.GetCachedComponent<IgnoreFadeIdentifier>() != null) {
                    continue;
                }
                var materials = renderers[i].materials;
                for (int j = 0; j < materials.Length; ++j) {
                    if (!m_RegisteredMaterial.Contains(materials[j]) && materials[j].HasProperty(m_ColorID)) {
                        count++;
                    }
                }
            }

            if (count > 0) {
                var totalCount = m_CharacterFadeMaterialsCount + count;
                if (m_CharacterFadeMaterials == null) {
                    m_CharacterFadeMaterials = new Material[totalCount];
                } else if (totalCount >= m_CharacterFadeMaterials.Length) {
                    System.Array.Resize(ref m_CharacterFadeMaterials, totalCount);
                }

                // Cache a reference to all of the faded materials.
                for (int i = 0; i < renderers.Length; ++i) {
                    // The renderer fade can be ignored.
                    if (renderers[i].gameObject.GetCachedComponent<IgnoreFadeIdentifier>() != null) {
                        continue;
                    }
                    var materials = renderers[i].materials;
                    for (int j = 0; j < materials.Length; ++j) {
                        if (!m_RegisteredMaterial.Contains(materials[j]) && materials[j].HasProperty(m_ColorID)) {
                            if (materials[j].HasProperty(OriginalMaterialValue.ModeID)) {
                                m_MaterialModeSet.Add(materials[j]);
                            }
                            m_CharacterFadeMaterials[m_CharacterFadeMaterialsCount] = materials[j];
                            m_RegisteredMaterial.Add(materials[j]);
                            m_CharacterFadeMaterialsCount++;

                            if (m_CacheCharacterMaterials && !m_OriginalMaterialValuesMap.ContainsKey(materials[j])) {
                                var originalMaterialValues = GenericObjectPool.Get<OriginalMaterialValue>();
                                originalMaterialValues.Initialize(materials[j], m_ColorID, m_MaterialModeSet.Contains(materials[j]));
                                m_OriginalMaterialValuesMap.Add(materials[j], originalMaterialValues);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The ShootableWeapon ItemAction has shown or hidden a projectile.
        /// </summary>
        /// <param name="projectile">The projectile shown or hidden.</param>
        /// <param name="show">Is the projectile being shown?</param>
        private void OnShowProjectile(GameObject projectile, bool show)
        {
            // The projectile may not have a renderer.
            var renderers = projectile.GetCachedComponents<Renderer>();
            if (renderers == null || renderers.Length == 0) {
                return;
            }

            if (show) {
                // Initalize all of the materials.
                InitializeCharacterFadeRenderers(renderers);
            } else {
                // Remove all of the materials from the fade.
                for (int i = renderers.Length - 1; i > -1; --i) {
                    var materials = renderers[i].materials;
                    for (int j = materials.Length - 1; j > -1; --j) {
                        if (!m_RegisteredMaterial.Contains(materials[j])) {
                            continue;
                        }
                        // Remove the material from the array. The array won't resize when the material is removed so start from the end to 
                        // reduce the number of likely iterations.
                        var index = -1;
                        for (int k = m_CharacterFadeMaterialsCount - 1; k > -1; --k) {
                            if (m_CharacterFadeMaterials[k] == materials[j]) {
                                index = k;
                                break;
                            }
                        }
                        for (int k = index; k < m_CharacterFadeMaterialsCount - 1; ++k) {
                            m_CharacterFadeMaterials[k] = m_CharacterFadeMaterials[k + 1];
                        }
                        m_CharacterFadeMaterialsCount--;
                        m_CharacterFadeMaterials[m_CharacterFadeMaterialsCount] = null;
                        m_RegisteredMaterial.Remove(materials[j]);

                        if (m_OriginalMaterialValuesMap.TryGetValue(materials[j], out var materialValue)) {
                            RevertMaterial(materials[j], materialValue);
                            if (!m_CacheCharacterMaterials) {
                                m_OriginalMaterialValuesMap.Remove(materials[j]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The character material has been updated - update the material reference.
        /// </summary>
        /// <param name="previousMaterial">The material that was faded.</param>
        /// <param name="targetMaterial">The material that can now be faded.</param>
        public void UpdateMaterials(Material previousMaterial, Material targetMaterial)
        {
            if (!m_CharacterFade) {
                return;
            }

            // Revert any alpha changes.
            if (m_CharacterFaded) {
                var color = previousMaterial.GetColor(m_ColorID);
                color.a = 1;
                previousMaterial.SetColor(m_ColorID, color);
            }

            // Remove the old.
            if (m_CacheCharacterMaterials && m_OriginalMaterialValuesMap.TryGetValue(previousMaterial, out var materialValue)) {
                if (m_CharacterFaded) {
                    RevertMaterial(previousMaterial, materialValue);
                }

                GenericObjectPool.Return(materialValue);
                m_OriginalMaterialValuesMap.Remove(previousMaterial);
            }
            m_MaterialModeSet.Remove(previousMaterial);
            m_RegisteredMaterial.Remove(previousMaterial);

            // Don't allow duplicates.
            if (m_RegisteredMaterial.Contains(targetMaterial)) {
                return;
            }

            // Add the new.
            for (int i = 0; i < m_CharacterFadeMaterials.Length; ++i) {
                if (m_CharacterFadeMaterials[i] == previousMaterial) {
                    m_CharacterFadeMaterials[i] = targetMaterial;
                    break;
                }
            }
            m_RegisteredMaterial.Add(targetMaterial);
            if (targetMaterial.HasProperty(OriginalMaterialValue.ModeID)) {
                m_MaterialModeSet.Add(targetMaterial);
            }
            if (m_CharacterFade) {
                var color = targetMaterial.GetColor(m_ColorID);
                color.a = 1;
                targetMaterial.SetColor(m_ColorID, color);
            }
            if (m_CacheCharacterMaterials) {
                var originalMaterialValues = GenericObjectPool.Get<OriginalMaterialValue>();
                originalMaterialValues.Initialize(targetMaterial, m_ColorID, m_MaterialModeSet.Contains(targetMaterial));
                m_OriginalMaterialValuesMap.Add(targetMaterial, originalMaterialValues);
            }

            // Update the material if the character is currently being faded.
            if (m_CharacterFaded) {
                EnableFadeMaterial(targetMaterial);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            // The character should no longer be faded.
            DisableFades();
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            if (m_CharacterFade && m_RegisteredMaterial == null) {
                m_RegisteredMaterial = new HashSet<Material>();
                m_MaterialModeSet = new HashSet<Material>();
            } else if (!m_CharacterFade) {
                DisableCharacterFade();
            }
            // In some cases the character fade should not update immediately - such as while zooming in third person to going back to a non-zoom state. This will prevent
            // the character from fading when the camera is moving back to the non-zoomed state.
            if (m_CharacterFadeStateChangeCooldown > 0) {
                m_CharacterFadeCooldownElapsedTime = Time.time + m_CharacterFadeStateChangeCooldown;
            }

            if (m_ObstructingObjectsFade && m_ObstructingMaterials == null) {
                m_RaycastsHit = new RaycastHit[m_MaxObstructingColliderCount];
                m_ObstructingMaterials = new Material[m_MaxObstructingMaterialCount];
                m_ObstructingColliders = new Collider[m_MaxObstructingMaterialCount];
                m_ObstructionHitSet = new HashSet<Material>();
                m_CanObstructionFade = new Dictionary<Material, bool>();
            } else if (!m_ObstructingObjectsFade) {
                DisableObstructingFade();
            }
        }

        /// <summary>
        /// The camera has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            OnAttachCharacter(null);

            EventHandler.UnregisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }
    }
}