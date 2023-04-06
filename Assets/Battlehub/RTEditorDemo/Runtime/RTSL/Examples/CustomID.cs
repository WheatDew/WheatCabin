using Battlehub.RTSL;
using System;
using UnityEngine;

namespace Battlehub.RTSL.Demo
{
    [ProtoBuf.ProtoContract]
    [SerializeField]
    public struct CustomID : IEquatable<CustomID>, IComparable<CustomID>, IComparable
    {
        [ProtoBuf.ProtoMember(1)]
        public Guid Guid;

        public int CompareTo(CustomID other) =>
            Guid.CompareTo(other.Guid);

        public int CompareTo(object obj) =>
            obj is CustomID other ? CompareTo(other) : throw new ArgumentException($"Object is not a {nameof(CustomID)}");

        public bool Equals(CustomID other) => 
            Guid.Equals(other.Guid);

        public override bool Equals(object obj) => 
            obj is CustomID other && Equals(other);

        public override int GetHashCode() => 
            Guid.GetHashCode();

        public override string ToString() =>
            Guid.ToString();

        public static CustomID NewID()
        {
            return new CustomID { Guid = Guid.NewGuid() };
        }

        #if UNITY_EDITOR
        //[UnityEditor.InitializeOnLoadMethod]
        #endif
        static void Register()
        {
            //Debug.Log("CustomID registered");
            RTSLSettings.RegisterID<CustomID>();
        }
    }

    

}
