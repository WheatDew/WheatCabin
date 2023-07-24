/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.CharacterAssist
{
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Shows the gizmo for the start location.
    /// </summary>
    public class MoveTowardsLocationGizmo
    {
        /// <summary>
        /// Draws the gizmo.
        /// </summary>
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawMoveTowardsLocationGizmo(MoveTowardsLocation startLocation, GizmoType gizmoType)
        {
            var transform = startLocation.transform;
            Handles.matrix = Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            // Wire lines will indicate the valid starting positions.
            Gizmos.color = new Color(1, 0.6f, 0, 0.7f);
            Gizmos.DrawWireCube(startLocation.Offset, startLocation.Size);

            // Draw an arc indicating the direction that the character can face.
            Handles.matrix = Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(startLocation.Offset), transform.rotation * Quaternion.Euler(0, startLocation.YawOffset, 0), Vector3.one);
            Handles.color = new Color(0, 1, 0, 0.7f);
            var radius = Mathf.Min(0.5f, Mathf.Max(Mathf.Abs(startLocation.Offset.z), 0.2f));
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius);
            Handles.color = new Color(0, 1, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero, Vector3.up, Quaternion.AngleAxis(startLocation.Angle / 2 - startLocation.Angle, Vector3.up) * Vector3.forward,
                startLocation.Angle, radius);

            // Draw arrows pointing in the direction that the character can face.
            radius /= 2;
            Handles.color = new Color(0, 1, 0, 0.7f);
            Handles.DrawLine(Vector3.zero, (Vector3.forward * 2f) * radius);
            Handles.DrawLine(2 * radius * Vector3.forward, ((1.5f * radius * Vector3.forward) + 0.5f * radius * Vector3.left));
            Handles.DrawLine(2 * radius * Vector3.forward, ((1.5f * radius * Vector3.forward) + 0.5f * radius * Vector3.right));
            if (startLocation.Angle >= 180) {
                Handles.DrawLine(2 * radius * Vector3.left, (2 * radius* Vector3.right));
                Handles.DrawLine(2 * radius * Vector3.left, ((1.5f * radius * Vector3.left) + 0.5f * radius * Vector3.forward));
                Handles.DrawLine(2 * radius * Vector3.left, ((1.5f * radius * Vector3.left) + 0.5f * radius * Vector3.back));
                Handles.DrawLine(2 * radius * Vector3.right, ((1.5f * radius * Vector3.right) + 0.5f * radius * Vector3.back));
                Handles.DrawLine(2 * radius * Vector3.right, ((1.5f * radius * Vector3.right) + 0.5f * radius * Vector3.back));
                if (startLocation.Angle == 360) {
                    Handles.DrawLine(Vector3.zero, (Vector3.back * 2f) * radius);
                    Handles.DrawLine(2 * radius * Vector3.back, ((1.5f * radius * Vector3.back) + (Vector3.left * 0.5f) * radius));
                    Handles.DrawLine(2 * radius * Vector3.back, ((1.5f * radius * Vector3.back) + (Vector3.right * 0.5f) * radius));
                }
            }
        }
    }
}