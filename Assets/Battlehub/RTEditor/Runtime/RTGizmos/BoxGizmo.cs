using UnityEngine;
using Battlehub.RTCommon;

namespace Battlehub.RTGizmos
{
    public class BoxGizmo : BaseGizmo
    {
        protected virtual Bounds Bounds
        {
            get { return new Bounds(Vector3.zero, Vector3.one); }
            set { }
        }

        protected override Matrix4x4 HandlesTransform
        {
            get { return Matrix4x4.TRS(Target.TransformPoint(Bounds.center), Target.rotation, Vector3.Scale(Bounds.extents, Target.lossyScale)); }
        }

        protected override bool OnDrag(int index, Vector3 offset)
        {
            Bounds bounds = Bounds;
            bounds.center += offset / 2;
            bounds.extents += Vector3.Scale(offset / 2, HandlesPositions[index]);
            Bounds = bounds;
            return true;
        }

        protected override void OnCommandBufferRefresh(IRTECamera camera)
        {
            base.OnCommandBufferRefresh(camera);

            Bounds bounds = Bounds;
            Vector3 parentScale = Target.parent == null ? Vector3.one : Target.parent.lossyScale;
            Vector3 scale = Vector3.Scale(Vector3.Scale(bounds.extents, Target.localScale), parentScale);

            GizmoUtility.DrawCubeHandles(camera.CommandBuffer, Target.TransformPoint(bounds.center), Target.rotation, scale, HandleProperties);
            GizmoUtility.DrawWireCube(camera.CommandBuffer, bounds, Target.TransformPoint(bounds.center), Target.rotation, Target.lossyScale, LineProperties);

            if(IsDragging)
            {
                GizmoUtility.DrawSelection(camera.CommandBuffer, Target.TransformPoint(bounds.center + Vector3.Scale(HandlesPositions[DragIndex], bounds.extents)), Target.rotation, Target.lossyScale, SelectionProperties);
            }
        }
    }
}
