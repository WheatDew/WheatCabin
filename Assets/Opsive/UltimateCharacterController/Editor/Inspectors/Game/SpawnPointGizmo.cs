/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Game
{
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Shows the gizmo for the spawn point.
    /// </summary>
    public class SpawnPointGizmo
    {
        private static Collider[] s_ObstructionColliders;

        /// <summary>
        /// Draws the spawn point gizmo.
        /// </summary>
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawSpawnPointGizmo(SpawnPoint spawnPoint, GizmoType gizmoType)
        {
            var transform = spawnPoint.transform;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            Gizmos.color = spawnPoint.GizmoColor;
            var position = Vector3.zero;
            if (spawnPoint.Shape == SpawnPoint.SpawnShape.Point || spawnPoint.Shape == SpawnPoint.SpawnShape.Sphere) {
                var size = spawnPoint.Shape == SpawnPoint.SpawnShape.Sphere ? spawnPoint.Size : 0.2f;
                Gizmos.DrawSphere(position, size);

                // Draw the outline when the component is selected.
                if (MathUtility.InLayerMask((int)GizmoType.Selected, (int)gizmoType)) {
                    Gizmos.color = InspectorUtility.GetContrastColor(spawnPoint.GizmoColor);
                    Gizmos.DrawWireSphere(position, size);
                }
            } else if (spawnPoint.Shape == SpawnPoint.SpawnShape.Box) {
                var size = Vector3.zero;
                size.x = size.z = spawnPoint.Size;
                size.y = spawnPoint.GroundSnapHeight;
                position += spawnPoint.transform.up * size.y / 2;
                Gizmos.DrawCube(position, size);

                // Draw the outline when the component is selected.
                if (MathUtility.InLayerMask((int)GizmoType.Selected, (int)gizmoType)) {
                    Gizmos.color = InspectorUtility.GetContrastColor(spawnPoint.GizmoColor);
                    Gizmos.DrawWireCube(position, size);
                }
            }

            // Draw a label for each obstructing object.
            if (spawnPoint.CheckForObstruction) {
                if (s_ObstructionColliders == null) {
                    s_ObstructionColliders = new Collider[1];
                }
                var extents = Vector3.zero;
                extents.x = extents.z = spawnPoint.Size / 2;
                extents.y = spawnPoint.GroundSnapHeight / 2;
                var spawnTransform = spawnPoint.transform;
                var boxPosition = spawnTransform.TransformPoint(extents);
                var overlapCount = Physics.OverlapBoxNonAlloc(boxPosition, extents, s_ObstructionColliders, spawnTransform.rotation, spawnPoint.ObstructionLayers, QueryTriggerInteraction.Ignore);
                if (overlapCount != 0) {
                    Handles.Label(spawnPoint.transform.position, "Obstruction: " + s_ObstructionColliders[0].transform.gameObject.name);
                }
            }

            if (MathUtility.InLayerMask((int)GizmoType.Selected, (int)gizmoType)) {
                // The Gizmo class cannot draw a wire disk.
                Handles.color = InspectorUtility.GetContrastColor(spawnPoint.GizmoColor);
                var spawnTransform = spawnPoint.transform;
                Handles.DrawWireDisc(spawnTransform.position, spawnTransform.up, 1);

                // Draw directional arrows when selected.
                var rad = spawnPoint.Size > 0 ? spawnPoint.Size : 1;
                if (spawnPoint.RandomDirection) {
                    // Draw four big arrows, relative to the spawnpoint and perpendicular to each other.
                    Gizmos.DrawLine(2 * rad * Vector3.back, 2 * rad * Vector3.forward);
                    Gizmos.DrawLine(2 * rad * Vector3.left, 2 * rad * Vector3.right);
                    Gizmos.DrawLine(2 * rad * Vector3.forward, 1.5f * rad * Vector3.forward + 0.5f * rad * Vector3.left);
                    Gizmos.DrawLine((Vector3.forward * 2) * rad, 1.5f * rad * Vector3.forward + 0.5f * rad * Vector3.right);
                    Gizmos.DrawLine((Vector3.back * 2) * rad, 1.5f * rad * Vector3.back + 0.5f * rad * Vector3.left);
                    Gizmos.DrawLine((Vector3.back * 2) * rad, 1.5f * rad * Vector3.back + 0.5f * rad * Vector3.right);
                    Gizmos.DrawLine((Vector3.left * 2) * rad, 1.5f * rad * Vector3.left + 0.5f * rad * Vector3.forward);
                    Gizmos.DrawLine((Vector3.left * 2) * rad, 1.5f * rad * Vector3.left + 0.5f * rad * Vector3.back);
                    Gizmos.DrawLine((Vector3.right * 2) * rad, 1.5f * rad * Vector3.right + 0.5f * rad * Vector3.forward);
                    Gizmos.DrawLine((Vector3.right * 2) * rad, 1.5f * rad * Vector3.right + 0.5f * rad * Vector3.back);
                } else {
                    // Draw a single big arrow pointing in the spawnpoint's forward direction.
                    Gizmos.DrawLine(Vector3.zero, (Vector3.forward * 2) * rad);
                    Gizmos.DrawLine(2 * rad * Vector3.forward, 1.5f * rad * Vector3.forward + 0.5f * rad * Vector3.left);
                    Gizmos.DrawLine(2 * rad * Vector3.forward, 1.5f * rad * Vector3.forward + 0.5f * rad * Vector3.right);
                }
            }
        }
    }
}