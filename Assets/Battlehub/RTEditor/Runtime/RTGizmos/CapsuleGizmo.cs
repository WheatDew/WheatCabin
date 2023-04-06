using System;
using Battlehub.RTCommon;
using UnityEngine;
namespace Battlehub.RTGizmos
{
   
    public abstract class CapsuleGizmo : BaseGizmo
    {
        protected abstract Vector3 Center
        {
            get;
            set;
        }
        protected abstract float Radius
        {
            get;
            set;
        }
        protected abstract float Height
        {
            get;
            set;
        }
        protected abstract int Direction
        {
            get;
            set;
        }
        
        protected override Matrix4x4 HandlesTransform
        {
            get
            {
                return Matrix4x4.TRS(Target.TransformPoint(Center), Target.rotation, GetHandlesScale(true));
            }
        }

        

        protected override bool OnDrag(int index, Vector3 offset)
        {
            Vector3 axis;
            if(Direction == 0)
            {
                axis = Vector3.right;
            }
            else if(Direction == 1)
            {
                axis = Vector3.up;
            }
            else
            {
                axis = Vector3.forward;
            }

            if (Mathf.Abs(Vector3.Dot(offset.normalized, axis)) > 0.99f)
            {
                float sign = Math.Sign(Vector3.Dot(offset.normalized, HandlesNormals[index]));

                Height += 2 * offset.magnitude * sign;
                if(Height < 0)
                {
                    Height = 0;
                    return false;
                }
            }
            else
            {
                float maxHs = GetMaxHorizontalScale(true);
                Radius += (Vector3.Scale(offset, Target.lossyScale).magnitude / maxHs) * Mathf.Sign(Vector3.Dot(offset, HandlesNormals[index]));
                
                if(Radius < 0)
                {
                    Radius = 0;
                    return false;
                }
            }
            return true;
        }

        protected override void OnCommandBufferRefresh(IRTECamera camera)
        {
            base.OnCommandBufferRefresh(camera);
            float hs = GetMaxHorizontalScale(true);
            Vector3 scale = GetHandlesScale(true);

            GizmoUtility.DrawCubeHandles(camera.CommandBuffer, Target.TransformPoint(Center), Target.rotation, scale, HandleProperties);
            GizmoUtility.DrawWireCapsule(camera.CommandBuffer, Direction, GetHeight(), Radius, Target.TransformPoint(Center), Target.rotation, new Vector3(hs, hs, hs), LineProperties);
            if (IsDragging)
            {
                GizmoUtility.DrawSelection(camera.CommandBuffer, HandlesTransform.MultiplyPoint(HandlesPositions[DragIndex]), Target.rotation, Target.lossyScale, SelectionProperties);
            }
        }

        private float GetHeight()
        {
            float s;
            float hs = GetMaxHorizontalScale(true);
            if (Direction == 0)
            {
                s = Target.lossyScale.x;
            }
            else if (Direction == 1)
            {
                s = Target.lossyScale.y;
            }
            else
            {
                s = Target.lossyScale.z;
            }

            return Height * s / hs;
        }

        private Vector3 GetHandlesScale(bool multiplyByTargetScale)
        {
            float x;
            float y;
            float z;
            float hs = GetMaxHorizontalScale(multiplyByTargetScale);
            if (Direction == 0)
            {
                x = GetHandlesHeight(multiplyByTargetScale);
                y = hs * Radius; 
                z = hs * Radius;
            }
            else if (Direction == 1)
            {
                x = hs * Radius;
                y = GetHandlesHeight(multiplyByTargetScale);
                z = hs * Radius;
            }
            else
            {
                x = hs * Radius;
                y = hs * Radius;
                z = GetHandlesHeight(multiplyByTargetScale);
            }

            const float min = 0.001f;
            if(x < min && x > -min)
            {
                x = 0.001f;
            }
            if (y < min && y > -min)
            {
                y = 0.001f;
            }
            if (z < min && z > -min)
            {
                z = 0.001f;
            }
            return new Vector3(x, y, z);
        }

        private float GetHandlesHeight(bool multiplyByTargetScale)
        {
            if(!multiplyByTargetScale)
            {
                return MaxAbs(Height / 2, Radius);
            }

            if (Direction == 0)
            {
                return MaxAbs(Target.lossyScale.x * Height / 2, Radius * GetMaxHorizontalScale(multiplyByTargetScale));
            }
            else if (Direction == 1)
            {
                return MaxAbs(Target.lossyScale.y * Height / 2, Radius * GetMaxHorizontalScale(multiplyByTargetScale));
            }

            return MaxAbs(Target.lossyScale.z * Height / 2, Radius * GetMaxHorizontalScale(multiplyByTargetScale));

        }

        private float GetMaxHorizontalScale(bool multiplyByTargetScale)
        {
            if(!multiplyByTargetScale)
            {
                return 1;
            }

            if(Direction == 0)
            {
                return MaxAbs(Target.lossyScale.y, Target.lossyScale.z);
            }
            else if(Direction == 1)
            {
                return MaxAbs(Target.lossyScale.x, Target.lossyScale.z);
            }

            return MaxAbs(Target.lossyScale.x, Target.lossyScale.y);
        }

        private float MaxAbs(float a, float b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                return a;
            }
            return b;
        }

  

    }

}
