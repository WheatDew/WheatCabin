using System;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTGizmos
{
    public class DirectionalLightGizmo : BaseGizmo
    {
        protected override Matrix4x4 HandlesTransform
        {
            get
            {
                return Target != null ? Target.localToWorldMatrix : Matrix4x4.identity;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            RefreshOnCameraChanged = true;
        }
 
        protected override void OnCommandBufferRefresh(IRTECamera camera)
        {
            base.OnCommandBufferRefresh(camera);
            if (Target == null)
            {
                return;
            }
            GizmoUtility.DrawDirectionalLight(camera.CommandBuffer, camera.Camera, Target.position, Target.rotation, Vector3.one, LineProperties);
        }

        public override void Reset()
        {
            base.Reset();
            LineColor = new Color(1, 1, 0.5f, 0.5f);
            HandlesColor = new Color(1, 1, 0.35f, 0.95f);
            SelectionColor = new Color(1.0f, 1.0f, 0, 1.0f);
        }
    }

}
