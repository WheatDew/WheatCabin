/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.StartStopActions
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Fades the materials on the character.
    /// </summary>
    [System.Serializable]
    public class FadeMaterials : MagicStartStopModule
    {
        [Tooltip("The name of the material property that should be faded.")]
        [SerializeField] protected string m_ColorPropertyName = "_Color";
        [Tooltip("The alpha color that the materials should fade to.")]
        [SerializeField] protected float m_TargetAlpha = 0;
        [Tooltip("The speed of the fade.")]
        [SerializeField] protected float m_FadeSpeed = 0.02f;
        [Tooltip("Should the fade be reverted when the action stops?")]
        [SerializeField] protected bool m_RevertFadeOnStop;

        public string ColorPropertyName { get { return m_ColorPropertyName; } set { m_ColorPropertyName = value; } }
        public float TargetAlpha { get { return m_TargetAlpha; } set { m_TargetAlpha = value; } }
        public float FadeSpeed { get { return m_FadeSpeed; } set { m_FadeSpeed = value; } }
        public bool RevertFadeOnStop { get => m_RevertFadeOnStop; set => m_RevertFadeOnStop = value; }

        private int m_ColorID;
        private bool m_Active;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private ScheduledEventBase m_UpdateEvent;
#endif

        private FadeMaterials m_BeginFadeMaterials;
        private List<Material> m_Materials = new List<Material>();
        private HashSet<Material> m_ActiveMaterials = new HashSet<Material>();
        private Dictionary<Material, OriginalMaterialValue> m_OriginalMaterialValuesMap = new Dictionary<Material, OriginalMaterialValue>();
        public List<Material> Materials { get { return m_Materials; } }
        public HashSet<Material> ActiveMaterials { get { return m_ActiveMaterials; } }
        public Dictionary<Material, OriginalMaterialValue> OriginalMaterialValuesMap { get { return m_OriginalMaterialValuesMap; } }

        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void Start(MagicUseDataStream useDataStream)
        {
            var origin = useDataStream.CastData.CastOrigin;
            
            // Initialize any starting values after all of the actions have been deserialized.
            if (m_ColorID == 0) {
                m_ColorID = Shader.PropertyToID(m_ColorPropertyName);
                var fadeMaterialModules = MagicAction.BeginModuleGroup.Modules;
                
                if (!m_BeginAction) {
                    for (int i = 0; i < fadeMaterialModules.Count; ++i) {
                        if (fadeMaterialModules[i] is FadeMaterials fadeMaterials) {
                            m_BeginFadeMaterials = fadeMaterials;
                            break;
                        }
                    }
                }
            }

            // The Object Fader should reset.
            EventHandler.ExecuteEvent(Character, "OnCharacterIndependentFade", true, true);
            if (m_BeginFadeMaterials == null) {
                // Return the previous objects.
                if (m_OriginalMaterialValuesMap.Count > 0) {
                    for (int i = 0; i < m_Materials.Count; ++i) {
                        GenericObjectPool.Return(m_OriginalMaterialValuesMap[m_Materials[i]]);
                        m_OriginalMaterialValuesMap.Remove(m_Materials[i]);
                    }
                }
                m_Materials.Clear();
                m_ActiveMaterials.Clear();

                EnableRendererFade();
            } else {
                m_Materials = m_BeginFadeMaterials.Materials;
                m_ActiveMaterials = m_BeginFadeMaterials.ActiveMaterials;
                m_OriginalMaterialValuesMap = m_BeginFadeMaterials.OriginalMaterialValuesMap;
            }
            m_Active = true;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // Update isn't called automatically for the remote players.
            if (NetworkInfo != null && !NetworkInfo.IsLocalPlayer()) {
                m_UpdateEvent = Scheduler.Schedule<MagicUseDataStream>(0.001f, Update, null);
            }
#endif
        }

        /// <summary>
        /// Enables fading on the renderers.
        /// </summary>
        private void EnableRendererFade()
        {
            // Fade all of the active renderers.
            var renderers = Character.GetComponentsInChildren<Renderer>(false);
            for (int i = 0; i < renderers.Length; ++i) {
                // The fade can be ignored.
                if (renderers[i].gameObject.GetCachedComponent<IgnoreFadeIdentifier>() != null) {
                    continue;
                }

                var materials = renderers[i].materials;
                for (int j = 0; j < materials.Length; ++j) {
                    var material = materials[j];
                    if (m_ActiveMaterials.Contains(material) || !material.HasProperty(m_ColorID)) {
                        continue;
                    }

                    m_Materials.Add(material);
                    m_ActiveMaterials.Add(material);

                    // Cache the original values so they can be reverted.
                    var originalMaterialValues = GenericObjectPool.Get<OriginalMaterialValue>();
                    originalMaterialValues.Initialize(material, m_ColorID, material.HasProperty(OriginalMaterialValue.ModeID));
                    m_OriginalMaterialValuesMap.Add(material, originalMaterialValues);

                    // The material should be able to fade.
                    material.SetFloat(OriginalMaterialValue.ModeID, 2);
                    material.SetInt(OriginalMaterialValue.SrcBlendID, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt(OriginalMaterialValue.DstBlendID, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.EnableKeyword(OriginalMaterialValue.AlphaBlendString);
                    material.renderQueue = 3000;

                    // If the action is already active then the material is being faded when the perspective is switching. Set the alpha to the 
                    // same alpha value as the rest of the materials.
                    if (m_Active) {
                        var color = material.GetColor(m_ColorID);
                        color.a = m_Materials[0].GetColor(m_ColorID).a;
                        material.SetColor(m_ColorID, color);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the action.
        /// </summary>
        /// <param name="useDataStream">The data stream with information about the magic cast.</param>
        public override void Update(MagicUseDataStream useDataStream)
        {
            if (!m_Active) {
                return;
            }

            var active = false;
            for (int i = 0; i < m_Materials.Count; ++i) {
                var color = m_Materials[i].GetColor(m_ColorID);
                color.a = Mathf.MoveTowards(color.a, m_TargetAlpha, m_FadeSpeed);
                m_Materials[i].SetColor(m_ColorID, color);
                if (color.a != m_TargetAlpha) {
                    active = true;
                }
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // Update isn't called automatically for the remote players.
            if (active && NetworkInfo != null && !NetworkInfo.IsLocalPlayer()) {
                m_UpdateEvent = Scheduler.Schedule<MagicUseDataStream>(0.001f, Update, null);
            }
#endif

            m_Active = active;
        }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        public override void Stop(MagicUseDataStream useDataStream)
        {
            if (!m_Active) {
                return;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (NetworkInfo != null && !NetworkInfo.IsLocalPlayer()) {
                Scheduler.Cancel(m_UpdateEvent);
                m_UpdateEvent = null;
            }
#endif

            EventHandler.ExecuteEvent(Character, "OnCharacterIndependentFade", false, false);
            m_Active = false;
            if (!m_RevertFadeOnStop) {
                return;
            }

            // Revert the values back to the original values.
            var fade = m_BeginFadeMaterials != null ? m_BeginFadeMaterials : this;
            var originalMaterialValues = fade.OriginalMaterialValuesMap;
            var materials = fade.Materials;
            for (int i = 0; i < materials.Count; ++i) {
                if (!originalMaterialValues.TryGetValue(materials[i], out var originalMaterialValue)) {
                    continue;
                }

                // Revert the material back to the starting value.
                materials[i].SetColor(m_ColorID, originalMaterialValue.Color);
                if (originalMaterialValue.ContainsMode) {
                    materials[i].SetFloat(OriginalMaterialValue.ModeID, originalMaterialValue.Mode);
                    materials[i].SetInt(OriginalMaterialValue.SrcBlendID, originalMaterialValue.SrcBlend);
                    materials[i].SetInt(OriginalMaterialValue.DstBlendID, originalMaterialValue.DstBlend);
                }
                if (!originalMaterialValue.AlphaBlend) {
                    materials[i].DisableKeyword(OriginalMaterialValue.AlphaBlendString);
                }
                materials[i].renderQueue = originalMaterialValue.RenderQueue;
            }
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        public override void OnChangePerspectives(bool firstPersonPerspective)
        {
            if (firstPersonPerspective || !m_Active) {
                return;
            }

            EnableRendererFade();
        }
    }
}