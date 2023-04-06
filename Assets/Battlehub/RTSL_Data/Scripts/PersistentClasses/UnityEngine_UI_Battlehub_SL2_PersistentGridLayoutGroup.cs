using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentGridLayoutGroup<TID> : PersistentLayoutGroup<TID>
    {
        [ProtoMember(262)]
        public GridLayoutGroup.Corner startCorner;

        [ProtoMember(263)]
        public GridLayoutGroup.Axis startAxis;

        [ProtoMember(264)]
        public PersistentVector2<TID> cellSize;

        [ProtoMember(265)]
        public PersistentVector2<TID> spacing;

        [ProtoMember(266)]
        public GridLayoutGroup.Constraint constraint;

        [ProtoMember(267)]
        public int constraintCount;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GridLayoutGroup uo = (GridLayoutGroup)obj;
            startCorner = uo.startCorner;
            startAxis = uo.startAxis;
            cellSize = uo.cellSize;
            spacing = uo.spacing;
            constraint = uo.constraint;
            constraintCount = uo.constraintCount;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            GridLayoutGroup uo = (GridLayoutGroup)obj;
            uo.startCorner = startCorner;
            uo.startAxis = startAxis;
            uo.cellSize = cellSize;
            uo.spacing = spacing;
            uo.constraint = constraint;
            uo.constraintCount = constraintCount;
            return uo;
        }
    }
}

