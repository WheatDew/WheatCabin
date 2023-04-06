using Battlehub.RTSL.Battlehub.SL2;
using System;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using UnityEngine.Events;
using UnityEngine.Events.Battlehub.SL2;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL.Interface
{
    public interface ITypeMapCreator
    {
        void Create(ITypeMap typeMap);
    }

    public class DefaultTypeMapCreator : ITypeMapCreator
    {
        public void Create(ITypeMap typeMap)
        {
            typeMap.Register(typeof(RuntimePrefab), typeof(PersistentRuntimePrefab<>), new Guid("430451d7-d09d-45e0-8fac-3d061e10f654"), new Guid("6c20bf67-8156-4e43-99f9-025d03e7d0a5"));
            typeMap.Register(typeof(GameObject), typeof(PersistentGameObject<>), new Guid("2e76e2f0-289d-4c48-936f-56033397359c"), new Guid("163584cb-bfc6-423c-ab02-e43c961af6d6"));
            typeMap.Register(typeof(Transform), typeof(PersistentTransform<>), new Guid("b542d079-2da8-4468-80b4-ba4c6adaa225"), new Guid("3b5d0310-44df-44a1-8689-245048c8151a"));
            typeMap.Register(typeof(UnityObject), typeof(PersistentObject<>), new Guid("9cdd70ef-9948-4b85-8432-09eaf0faf4b7"), new Guid("b4abccaa-7fd8-4035-8c90-2d46ac7278ed"));
            typeMap.Register(typeof(Component), typeof(PersistentComponent<>), new Guid("d19c5e1f-80d6-4294-9be4-713150ba5152"), new Guid("f7be1b4c-1306-4074-8076-f8bef011ab72"));
            typeMap.Register(typeof(UnityEvent), typeof(PersistentUnityEvent<>), new Guid("3ed54cee-4405-475c-8954-a5eaed086ad7"), new Guid("9f432f32-23f3-432c-992d-6cc43b8edbad"));
            typeMap.Register(typeof(UnityEventBase), typeof(PersistentUnityEventBase<>), new Guid("9e9f6774-aeb8-4cc5-b149-597a62077a89"), new Guid("1f0e42fc-2817-49d9-af67-1fd15dd6f9ed"));
            typeMap.Register(typeof(Color), typeof(PersistentColor<>), new Guid("c5aaef8c-fce2-4ce9-8474-7094e24e7ea6"), new Guid("baea5d8e-ae9a-4eb7-bbf4-e6e80e0c85d3"));
            typeMap.Register(typeof(Vector3), typeof(PersistentVector3<>), new Guid("28d91efe-0de0-478d-9e7b-c76f1ba91807"), new Guid("d1b8fcf4-3b8a-48f1-836f-86666243ece8"));
            typeMap.Register(typeof(Vector4), typeof(PersistentVector4<>), new Guid("210fd541-e678-45dc-bb96-69333099ddf4"), new Guid("dce295a4-7b33-408c-b87e-6b85a3358dbf"));
            typeMap.Register(typeof(Quaternion), typeof(PersistentQuaternion<>), new Guid("a2c70442-63fd-4b71-a7ff-ce1749c513f3"), new Guid("7e0cfd74-0fa9-4160-bdba-2498f6069b4b"));
        }
    }
}

