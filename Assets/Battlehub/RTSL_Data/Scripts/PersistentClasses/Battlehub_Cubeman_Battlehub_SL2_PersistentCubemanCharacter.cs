using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.Cubeman;
using Battlehub.Cubeman.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.Cubeman.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentCubemanCharacter<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public bool Enabled;

        [ProtoMember(257)]
        public float m_MovingTurnSpeed;

        [ProtoMember(258)]
        public float m_StationaryTurnSpeed;

        [ProtoMember(259)]
        public float m_JumpPower;

        [ProtoMember(260)]
        public float m_GravityMultiplier;

        [ProtoMember(261)]
        public float m_RunCycleLegOffset;

        [ProtoMember(262)]
        public float m_MoveSpeedMultiplier;

        [ProtoMember(263)]
        public float m_AnimSpeedMultiplier;

        [ProtoMember(264)]
        public float m_GroundCheckDistance;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            CubemanCharacter uo = (CubemanCharacter)obj;
            Enabled = uo.Enabled;
            m_MovingTurnSpeed = uo.m_MovingTurnSpeed;
            m_StationaryTurnSpeed = uo.m_StationaryTurnSpeed;
            m_JumpPower = uo.m_JumpPower;
            m_GravityMultiplier = uo.m_GravityMultiplier;
            m_RunCycleLegOffset = uo.m_RunCycleLegOffset;
            m_MoveSpeedMultiplier = uo.m_MoveSpeedMultiplier;
            m_AnimSpeedMultiplier = uo.m_AnimSpeedMultiplier;
            m_GroundCheckDistance = uo.m_GroundCheckDistance;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            CubemanCharacter uo = (CubemanCharacter)obj;
            uo.Enabled = Enabled;
            uo.m_MovingTurnSpeed = m_MovingTurnSpeed;
            uo.m_StationaryTurnSpeed = m_StationaryTurnSpeed;
            uo.m_JumpPower = m_JumpPower;
            uo.m_GravityMultiplier = m_GravityMultiplier;
            uo.m_RunCycleLegOffset = m_RunCycleLegOffset;
            uo.m_MoveSpeedMultiplier = m_MoveSpeedMultiplier;
            uo.m_AnimSpeedMultiplier = m_AnimSpeedMultiplier;
            uo.m_GroundCheckDistance = m_GroundCheckDistance;
            return uo;
        }
    }
}

