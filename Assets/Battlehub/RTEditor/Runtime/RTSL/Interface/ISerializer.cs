using System;
using System.IO;


namespace Battlehub.RTSL.Interface
{
    public interface ISerializer
    {
        TData DeepClone<TData>(TData data);

        TData Deserialize<TData>(Stream stream);

        TData Deserialize<TData>(byte[] b);

        TData Deserialize<TData>(byte[] b, TData obj);

        object Deserialize(byte[] b, Type type);

        object Deserialize(Stream stream, Type type, long length = -1);

        void Serialize<TData>(TData data, Stream stream);

        byte[] Serialize<TData>(TData data);
    }
}