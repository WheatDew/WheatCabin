using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlehub.RTSL
{
    public class RTSLSettings
    {
        /// <summary>
        /// Use RuntimeInitializeOnLoadMethod to register dependencies (see Init method in Assets\Battlehub\RTEditor\Runtime\RTSL\RTSLDeps.cs)
        /// </summary>
        public static bool RuntimeInitializeOnLoad = true;

        /// <summary>
        /// Save only objects with RTSLInclude component. Default: false
        /// </summary>
        public static bool SaveIncludedObjectsOnly = false;

        /// <summary>
        /// Create Separate RTSLTypeModel.dll For Each Build Target. Default: false
        /// </summary>
        [Obsolete]
        public static bool CreateSeparateTypeModelForEachBuiltTarget = false;

        /// <summary>
        /// Custom Serialization enables serialization using ICustomSerialization.Serialize. ICustomSerialization.Deserialize methods as an alternative to standard serializer (protobuf.net)
        /// </summary>
        public static bool IsCustomSerializationEnabled = false;

        public static int PersistentPrefabReadsPerBatch = int.MaxValue;
        public static int PersistentPrefabWritesPerBatch = int.MaxValue;
        public static int ProjectReadsPerBatch = int.MaxValue;
        public static int ProjectFindDeepDependenciesPerBatch = int.MaxValue;

        private static readonly HashSet<Type> m_registeredIDTypes = new HashSet<Type>
        {
            typeof(Guid),
            typeof(long),
        };

        public static Type[] IDTypes
        {
            get { return m_registeredIDTypes.ToArray(); }
        }

        public static void RegisterID<TID>() where TID : struct, IEquatable<TID>, IComparable<TID>, IComparable
        {
            m_registeredIDTypes.Add(typeof(TID));
        }
        public static void UnregisterID<TID>()
        {
            m_registeredIDTypes.Remove(typeof(TID));
        }
        public static void ClearIDs()
        {
            m_registeredIDTypes.Clear();
        }
    }
}

