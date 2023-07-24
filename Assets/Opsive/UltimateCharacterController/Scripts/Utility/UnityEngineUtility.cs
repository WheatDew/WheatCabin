/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    using Opsive.Shared.Game;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Contains a set of utility functions useful for interacting with the Unity Engine.
    /// </summary>
    public class UnityEngineUtility
    {
        public static HashSet<object> s_ObjectUpdated = new HashSet<object>();
        public static ScheduledEventBase s_ObjectClearEvent;

        /// <summary>
        /// Returns a display name for the specified type.
        /// </summary>
        /// <param name="type">The type to retieve the name of.</param>
        /// <returns>A display name for the specified type.</returns>
        public static string GetDisplayName(Type type)
        {
            return GetDisplayName(type.FullName, type.Name);
        }

        /// <summary>
        /// Returns a display name for the specified type.
        /// </summary>
        /// <param name="fullName">The full name of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>A display name for the specified type.</returns>
        public static string GetDisplayName(string fullName, string name)
        {
            if (fullName.Contains("FirstPersonController")) {
                return "First Person " + name;
            } else if (fullName.Contains("ThirdPersonController")) {
                return "Third Person " + name;
            }
            return name;
        }

        /// <summary>
        /// Returns true if the specified object has been updated.
        /// </summary>
        /// <param name="obj">The object to check if it has been updated.</param>
        /// <returns>True if the specified object has been updated.</returns>
        public static bool HasUpdatedObject(object obj)
        {
            return s_ObjectUpdated.Contains(obj);
        }

        /// <summary>
        /// Adds the specified object to the set.
        /// </summary>
        /// <param name="obj">The object that has been updated.</param>
        public static void AddUpdatedObject(object obj)
        {
            AddUpdatedObject(obj, false);
        }

        /// <summary>
        /// Adds the specified object to the set.
        /// </summary>
        /// <param name="obj">The object that has been updated.</param>
        /// <param name="autoClear">Should the object updated map be automatically cleared on the next tick?</param>
        public static void AddUpdatedObject(object obj, bool autoClear)
        {
            s_ObjectUpdated.Add(obj);

            if (autoClear && s_ObjectClearEvent == null) {
                s_ObjectClearEvent = Scheduler.Schedule(0.0001f, ClearUpdatedObjectsEvent);
            }
        }

        /// <summary>
        /// Removes all of the objects from the set.
        /// </summary>
        public static void ClearUpdatedObjects()
        {
            s_ObjectUpdated.Clear();
        }

        /// <summary>
        /// Removes all of the objects from the set and sets the event to null.
        /// </summary>
        private static void ClearUpdatedObjectsEvent()
        {
            ClearUpdatedObjects();
            s_ObjectClearEvent = null;
        }

        /// <summary>
        /// Change the size of the RectTransform according to the size of the sprite.
        /// </summary>
        /// <param name="sprite">The sprite that the RectTransform should change its size to.</param>
        /// <param name="spriteRectTransform">A reference to the sprite's RectTransform.</param>
        public static void SizeSprite(Sprite sprite, RectTransform spriteRectTransform)
        {
            if (sprite != null) {
                var sizeDelta = spriteRectTransform.sizeDelta;
                sizeDelta.x = sprite.textureRect.width;
                sizeDelta.y = sprite.textureRect.height;
                spriteRectTransform.sizeDelta = sizeDelta;
            }
        }

        /// <summary>
        /// Clears the Unity Engine Utility cache.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearCache()
        {
            if (s_ObjectUpdated != null) { s_ObjectUpdated.Clear(); }
        }

        /// <summary>
        /// Allows for comparison between RaycastHit objects.
        /// </summary>
        public class RaycastHitComparer : IComparer<RaycastHit>
        {
            /// <summary>
            /// Compare RaycastHit x to RaycastHit y. If x has a smaller distance value compared to y then a negative value will be returned.
            /// If the distance values are equal then 0 will be returned, and if y has a smaller distance value compared to x then a positive value will be returned.
            /// </summary>
            /// <param name="x">The first RaycastHit to compare.</param>
            /// <param name="y">The second RaycastHit to compare.</param>
            /// <returns>The resulting difference between RaycastHit x and y.</returns>
            public int Compare(RaycastHit x, RaycastHit y)
            {
                if (x.transform == null) {
                    return int.MaxValue;
                }
                if (y.transform == null) {
                    return int.MinValue;
                }
                return x.distance.CompareTo(y.distance);
            }
        }

        /// <summary>
        /// Allows for equity comparison checks between RaycastHit objects.
        /// </summary>
        public struct RaycastHitEqualityComparer : IEqualityComparer<RaycastHit>
        {
            /// <summary>
            /// Determines if RaycastHit x is equal to RaycastHit y.
            /// </summary>
            /// <param name="x">The first RaycastHit to compare.</param>
            /// <param name="y">The second RaycastHit to compare.</param>
            /// <returns>True if the raycasts are equal.</returns>
            public bool Equals(RaycastHit x, RaycastHit y)
            {
                if (x.distance != y.distance) {
                    return false;
                }
                if (x.point != y.point) {
                    return false;
                }
                if (x.normal != y.normal) {
                    return false;
                }
                if (x.transform != y.transform) {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Returns a hash code for the RaycastHit.
            /// </summary>
            /// <param name="hit">The RaycastHit to get the hash code of.</param>
            /// <returns>The hash code for the RaycastHit.</returns>
            public int GetHashCode(RaycastHit hit)
            {
                // Don't use hit.GetHashCode because that has boxing. This hash function won't always prevent duplicates but it's fine for what it's used for.
                return ((int)(hit.distance * 10000)) ^ ((int)(hit.point.x * 10000)) ^ ((int)(hit.point.y * 10000)) ^ ((int)(hit.point.z * 10000)) ^
                        ((int)(hit.normal.x * 10000)) ^ ((int)(hit.normal.y * 10000)) ^ ((int)(hit.normal.z * 10000));
            }
        }

        /// <summary>
        /// Removes all of the null elements in the array.
        /// </summary>
        /// <param name="array">The array that should have the null elements removed.</param>
        public static void RemoveNullElements<T>(ref T[] array)
        {
            if (array == null) {
                return;
            }

            for (int i = array.Length - 1; i >= 0; --i) {
                if (array[i] != null) {
                    continue;
                }

                for (int j = i; j < array.Length - 1; ++j) {
                    array[j] = array[j + 1];
                }

                Array.Resize(ref array, array.Length - 1);
            }
        }
    }

    /// <summary>
    /// A container for a min and max Vector3 value.
    /// </summary>
    [Serializable]
    public struct MinMaxVector3
    {
        [Tooltip("The minimum Vector3 value.")]
        [SerializeField] private Vector3 m_MinValue;
        [Tooltip("The maximum Vector3 value.")]
        [SerializeField] private Vector3 m_MaxValue;
        [Tooltip("The minimum magnitude value when determining a random value.")]
        [SerializeField] private Vector3 m_MinMagnitude;

        public Vector3 MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
        public Vector3 MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }
        public Vector3 MinMagnitude { get { return m_MinMagnitude; } set { m_MinMagnitude = value; } }

        public Vector3 RandomValue
        {
            get {
                var value = Vector3.zero;
                value.x = GetRandomFloat(m_MinValue.x, m_MaxValue.x, m_MinMagnitude.x);
                value.y = GetRandomFloat(m_MinValue.y, m_MaxValue.y, m_MinMagnitude.y);
                value.z = GetRandomFloat(m_MinValue.z, m_MaxValue.z, m_MinMagnitude.z);
                return value;
            }
        }

        /// <summary>
        /// MinMaxVector3 constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum Vector3 value.</param>
        /// <param name="maxValue">The maximum Vector3 value.</param>
        public MinMaxVector3(Vector3 minValue, Vector3 maxValue)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_MinMagnitude = Vector3.zero;
        }

        /// <summary>
        /// MinMaxVector3 constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum Vector3 value.</param>
        /// <param name="maxValue">The maximum Vector3 value.</param>
        /// <param name="minMagnitude">The minimum magnitude of the random value.</param>
        public MinMaxVector3(Vector3 minValue, Vector3 maxValue, Vector3 minMagnitude)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_MinMagnitude = minMagnitude;
        }

        /// <summary>
        /// Returns a random float between the min and max value with the specified minimum magnitude.
        /// </summary>
        /// <param name="minValue">The minimum float value.</param>
        /// <param name="maxValue">The maximum float value.</param>
        /// <param name="minMagnitude">The minimum magnitude of the random value.</param>
        /// <returns>A random float between the min and max value.</returns>
        private float GetRandomFloat(float minValue, float maxValue, float minMagnitude)
        {
            if (minMagnitude != 0 && Mathf.Sign(m_MinValue.x) != Mathf.Sign(m_MaxValue.x)) {
                if (Mathf.Sign(UnityEngine.Random.Range(m_MinValue.x, m_MaxValue.x)) > 0) {
                    return UnityEngine.Random.Range(minMagnitude, Mathf.Max(minMagnitude, maxValue));
                }
                return UnityEngine.Random.Range(-minMagnitude, Mathf.Min(-minMagnitude, minValue));
            } else {
                return UnityEngine.Random.Range(minValue, maxValue);
            }
        }
    }

    /// <summary>
    /// Represents the object which can be spawned.
    /// </summary>
    [System.Serializable]
    public class ObjectSpawnInfo
    {
#pragma warning disable 0649
        [Tooltip("The object that can be spawned.")]
        [SerializeField] private GameObject m_Object;
        [Tooltip("The probability that the object can be spawned.")]
        [Range(0, 1)] [SerializeField] private float m_Probability = 1;
        [Tooltip("Should a random spin be applied to the object after it has been spawned?")]
        [SerializeField] private bool m_RandomSpin;
#pragma warning restore 0649

        public GameObject Object { get { return m_Object; } }
        public float Probability { get { return m_Probability; } }
        public bool RandomSpin { get { return m_RandomSpin; } }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="position">The position to instantiate the object at.</param>
        /// <param name="normal">The normal of the instantiated object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <returns>The instantiated object (can be null). </returns>
        public GameObject Instantiate(Vector3 position, Vector3 normal, Vector3 gravityDirection)
        {
            if (m_Object == null) {
                return null;
            }

            // There is a random chance that the object cannot be spawned.
            if (UnityEngine.Random.value < m_Probability) {
                var rotation = Quaternion.LookRotation(normal);
                // A random spin can be applied so the rotation isn't the same every hit.
                if (m_RandomSpin) {
                    rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), normal);
                }
                var instantiatedObject = ObjectPoolBase.Instantiate(m_Object, position, rotation);
                // If the DirectionalConstantForce component exists then the gravity direction should be set so the object will move in the correct direction.
                var directionalConstantForce = instantiatedObject.GetCachedComponent<Traits.DirectionalConstantForce>();
                if (directionalConstantForce != null) {
                    directionalConstantForce.Direction = gravityDirection;
                }
                return instantiatedObject;
            }
            return null;
        }
    }

    /// <summary>
    /// Struct which stores the material values to revert back to after the material has been faded.
    /// </summary>
    public struct OriginalMaterialValue
    {
        [Tooltip("The color of the material.")]
        private Color m_Color;
        [Tooltip("Does the material have a mode property?")]
        private bool m_ContainsMode;
        [Tooltip("The render mode of the material.")]
        private float m_Mode;
        [Tooltip("The SourceBlend BlendMode of the material.")]
        private int m_SrcBlend;
        [Tooltip("The DestinationBlend BlendMode of the material.")]
        private int m_DstBlend;
        [Tooltip("Is alpha blend enabled?")]
        private bool m_AlphaBlend;
        [Tooltip("The render queue of the material.")]
        private int m_RenderQueue;

        public Color Color { get { return m_Color; } set { m_Color = value; } }
        public bool ContainsMode { get { return m_ContainsMode; } set { m_ContainsMode = value; } }
        public float Mode { get { return m_Mode; } set { m_Mode = value; } }
        public int SrcBlend { get { return m_SrcBlend; } set { m_SrcBlend = value; } }
        public int DstBlend { get { return m_DstBlend; } set { m_DstBlend = value; } }
        public bool AlphaBlend { get { return m_AlphaBlend; } set { m_AlphaBlend = value; } }
        public int RenderQueue { get { return m_RenderQueue; } set { m_RenderQueue = value; } }

        private static int s_ModeID;
        private static int s_SrcBlendID;
        private static int s_DstBlendID;
        private static string s_AlphaBlendString = "_ALPHABLEND_ON";

        public static int ModeID { get { return s_ModeID; } }
        public static int SrcBlendID { get { return s_SrcBlendID; } }
        public static int DstBlendID { get { return s_DstBlendID; } }
        public static string AlphaBlendString { get { return s_AlphaBlendString; } }

        /// <summary>
        /// Initializes the OriginalMaterialValue.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            s_ModeID = Shader.PropertyToID("_Mode");
            s_SrcBlendID = Shader.PropertyToID("_SrcBlend");
            s_DstBlendID = Shader.PropertyToID("_DstBlend");
        }

        /// <summary>
        /// Initializes the OriginalMaterialValue to the material values.
        /// </summary>
        /// <param name="material">The material to initialize.</param>
        /// <param name="colorID">The id of the color property.</param>
        /// <param name="containsMode">Does the material have a Mode property?</param>
        public void Initialize(Material material, int colorID, bool containsMode)
        {
            m_Color = material.GetColor(colorID);
            m_AlphaBlend = material.IsKeywordEnabled(s_AlphaBlendString);
            m_RenderQueue = material.renderQueue;
            m_ContainsMode = containsMode;
            if (containsMode) {
                m_Mode = material.GetFloat(s_ModeID);
                m_SrcBlend = material.GetInt(s_SrcBlendID);
                m_DstBlend = material.GetInt(s_DstBlendID);
            }
        }
    }

    /// <summary>
    /// Attribute which allows the same type to be added multiple times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AllowDuplicateTypes : Attribute
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Attribute which specifies the field should be drawn with the AdjustableArrayAttributeControl.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AdjustableStringArrayAttribute : Attribute
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Attribute which specifies the field should be drawn with the DropdownEffectAttributeControl.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DropdownEffectAttribute : Attribute
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Attribute which specifies the field should be drawn with the ReorderableStateListAttributeControl.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReorderableObjectListAttribute : Attribute
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Attribute which prevents the class from being copied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IgnoreTemplateCopy : Attribute
    {
        // Intentionally left blank.
    }
}