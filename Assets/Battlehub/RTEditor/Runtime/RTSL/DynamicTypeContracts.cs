using ProtoBuf;
using System.Collections.Generic;


namespace Battlehub.RTSL
{
    public static class DictionaryExt
    {
        public static U Get<T, U>(this Dictionary<T, U> dict, T key)
        {
            U val;
            if (dict.TryGetValue(key, out val))
            {
                return val;
            }
            return default(U);
        }
    }

    [ProtoContract]
    public abstract class PrimitiveContract
    {
        public static PrimitiveContract<T> Create<T>(T value)
        {
            return new PrimitiveContract<T>(value);
        }

        public object ValueBase
        {
            get { return ValueImpl; }
            set { ValueImpl = value; }
        }
        protected abstract object ValueImpl { get; set; }
        protected PrimitiveContract() { }
    }

    [ProtoContract]
    public class PrimitiveContract<T> : PrimitiveContract
    {
        public PrimitiveContract() { }
        public PrimitiveContract(T value) { Value = value; }
        [ProtoMember(1)]
        public T Value { get; set; }
        protected override object ValueImpl
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }
}

