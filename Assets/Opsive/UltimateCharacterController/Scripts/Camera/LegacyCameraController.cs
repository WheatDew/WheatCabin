/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Camera
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using UnityEngine;

    /// <summary>
    /// Stores the version 2 legacy data for updating.
    /// </summary>
    public class LegacyCameraController : CameraController
    {
        [Tooltip("The serialization data for the ViewTypes.")]
        [SerializeField] protected Serialization[] m_ViewTypeData;

        public bool HasViewTypeData => (m_ViewTypeData != null && m_ViewTypeData.Length > 0);

        private ViewType[] m_LegacyViewTypes;

        /// <summary>
        /// Returns the deserialized view types.
        /// </summary>
        /// <returns>The deserialized view types.</returns>
        public ViewType[] GetDeserializedViewTypes()
        {
            if (m_LegacyViewTypes == null && m_ViewTypeData != null && m_ViewTypeData.Length > 0) {
                m_LegacyViewTypes = new ViewType[m_ViewTypeData.Length];
                for (int i = 0; i < m_ViewTypeData.Length; ++i) {
                    m_LegacyViewTypes[i] = m_ViewTypeData[i].DeserializeFields(MemberVisibility.Public) as ViewType;
                }
            }
            return m_LegacyViewTypes;
        }
    }
}