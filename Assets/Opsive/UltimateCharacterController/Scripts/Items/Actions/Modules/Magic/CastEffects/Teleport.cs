/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.CastEffects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;

    /// <summary>
    /// Teleports the character.
    /// </summary>
    [System.Serializable]
    public class Teleport : MagicMultiTargetCastEffectModule
    {
        [Tooltip("Should the character's animator be snapped?")]
        [SerializeField] protected bool m_SnapAnimator;

        public bool SnapAnimator { get { return m_SnapAnimator; } set { m_SnapAnimator = value; } }

        private CharacterLayerManager m_CharacterLayerManager;


        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_CharacterLayerManager = CharacterLocomotion.gameObject.GetCachedComponent<CharacterLayerManager>();
        }

        /// <summary>
        /// Is the specified position a valid target position?
        /// </summary>
        /// <param name="useDataStream">The data associated with the cast.</param>
        /// <param name="position">The position that may be a valid target position.</param>
        /// <param name="normal">The normal of the position.</param>
        /// <returns>True if the specified position is a valid target position.</returns>
        public override bool IsValidTargetPosition(MagicUseDataStream useDataStream, Vector3 position, Vector3 normal)
        {
            // The slope must be less than the slope limit.
            if (Vector3.Angle(CharacterLocomotion.Up, normal) > CharacterLocomotion.SlopeLimit) {
                return false;
            }

            // There must be enough space to stand.
            if (Physics.SphereCast(new Ray(position - CharacterLocomotion.Up * CharacterLocomotion.Radius, CharacterLocomotion.Up), CharacterLocomotion.Radius, 
                    CharacterLocomotion.Height, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream, contains the cast data.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            m_CastID = (uint)useDataStream.CastData.CastID;
            
            var targetPosition = useDataStream.CastData.CastTargetPosition;
            var direction = Vector3.ProjectOnPlane(targetPosition - GameObject.transform.position, CharacterLocomotion.Up);
            CharacterLocomotion.SetPositionAndRotation(targetPosition, Quaternion.LookRotation(direction), m_SnapAnimator, false);
        }
    }
}