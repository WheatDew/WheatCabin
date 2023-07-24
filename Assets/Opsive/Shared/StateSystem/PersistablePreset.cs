/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.StateSystem
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Allows the Preset component to serialized the property value.
    /// </summary>
    [FormerlySerializedAs("Opsive.UltimateCharacterController.StateSystem.PersistablePreset")]
    public class PersistablePreset : Preset
    {
        [Tooltip("The serialized properties.")]
        [SerializeField] protected Serialization m_Data;

        public Serialization Data { get { return m_Data; } set { m_Data = value; } }

        /// <summary>
        /// Creates a persistable preset based off of the specified component.
        /// </summary>
        /// <param name="obj">The object to retrieve the property values of.</param>
        /// <returns>The created preset. Null if no properties have been found to create the preset with.</returns>
        public static PersistablePreset CreatePreset(object obj)
        {
            return CreatePreset(obj, MemberVisibility.None);
        }

        /// <summary>
        /// Creates a persistable preset based off of the specified component and visibility.
        /// </summary>
        /// <param name="obj">The object to retrieve the property values of.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The created preset. Null if no properties have been found to create the preset with.</returns>
        public static PersistablePreset CreatePreset(object obj, MemberVisibility visibility)
        {
            var data = new Serialization();
            data.Serialize(obj, false, visibility);
            var preset = CreateInstance<PersistablePreset>();
            preset.Data = data;
            return preset;
        }

        /// <summary>
        /// Initializes the preset with the specified visiblity. The preset must be initialized before the preset values are applied so the delegates can be created.
        /// </summary>
        /// <param name="obj">The object to map the delegates to.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public override void Initialize(object obj, MemberVisibility visibility)
        {
            Dictionary<long, int> valuePositionMap;
            if (m_Data.LongValueHashes != null && m_Data.LongValueHashes.Length > 0) {
                valuePositionMap = new Dictionary<long, int>(m_Data.LongValueHashes.Length);
                for (int i = 0; i < m_Data.LongValueHashes.Length; ++i) {
                    valuePositionMap.Add(m_Data.LongValueHashes[i], i);
                }
            } else if (m_Data.ValueHashes != null){
                valuePositionMap = new Dictionary<long, int>(m_Data.ValueHashes.Length);
                for (int i = 0; i < m_Data.ValueHashes.Length; ++i) {
                    valuePositionMap.Add(m_Data.ValueHashes[i], i);
                }
            } else {
                valuePositionMap = new Dictionary<long, int>();
            }
            m_Delegates = new BaseDelegate[valuePositionMap.Count];

            var valueCount = 0;
            var properties = Serialization.GetSerializedProperties(obj.GetType(), visibility);
            var hashType = Serialization.GetHashType(m_Data.Version);
            for (int i = 0; i < properties.Length; ++i) {
                long hash;
                if (hashType == Serialization.HashType.BitwiseLong) {
                    hash = Serialization.StringHash(properties[i].PropertyType.FullName) + Serialization.StringHash(properties[i].Name);
                } else {
                    hash = Serialization.StringIntHash(properties[i].PropertyType.FullName) + Serialization.StringIntHash(properties[i].Name);
                }
                int position;
                if (!valuePositionMap.TryGetValue(hash, out position)) {
                    continue;
                }

                // Create a generic delegate based on the property type.
                var genericDelegateType = typeof(GenericDelegate<>).MakeGenericType(properties[i].PropertyType);
                m_Delegates[valueCount] = Activator.CreateInstance(genericDelegateType) as BaseDelegate;

                // Initialize the delegate.
                if (m_Delegates[valueCount] != null) {
                    m_Delegates[valueCount].Initialize(obj, properties[i], valuePositionMap, m_Data, visibility);
                } else {
                    Debug.LogWarning("Warning: Unable to create preset of type " + properties[i].PropertyType);
                }
                valueCount++;
            }

            // The delegate length may not match if a property has been added but no longer exists.
            if (m_Delegates.Length != valueCount) {
                Array.Resize(ref m_Delegates, valueCount);
            }
        }
    }
}