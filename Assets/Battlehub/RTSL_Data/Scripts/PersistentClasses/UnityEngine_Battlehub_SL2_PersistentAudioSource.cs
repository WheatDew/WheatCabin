using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Audio;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentAudioSource<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public float volume;

        [ProtoMember(257)]
        public float pitch;

        [ProtoMember(260)]
        public TID clip;

        [ProtoMember(261)]
        public TID outputAudioMixerGroup;

        [ProtoMember(262)]
        public bool loop;

        [ProtoMember(264)]
        public bool playOnAwake;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            AudioSource uo = (AudioSource)obj;
            volume = uo.volume;
            pitch = uo.pitch;
            clip = ToID(uo.clip);
            outputAudioMixerGroup = ToID(uo.outputAudioMixerGroup);
            loop = uo.loop;
            playOnAwake = uo.playOnAwake;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            AudioSource uo = (AudioSource)obj;
            uo.volume = volume;
            uo.pitch = pitch;
            uo.clip = FromID(clip, uo.clip);
            uo.outputAudioMixerGroup = FromID(outputAudioMixerGroup, uo.outputAudioMixerGroup);
            uo.loop = loop;
            uo.playOnAwake = playOnAwake;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(clip, context);
            AddDep(outputAudioMixerGroup, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            AudioSource uo = (AudioSource)obj;
            AddDep(uo.clip, context);
            AddDep(uo.outputAudioMixerGroup, context);
        }
    }
}

