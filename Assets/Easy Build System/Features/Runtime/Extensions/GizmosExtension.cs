﻿/// <summary>
/// Project : Easy Build System
/// Class : GizmosExtension.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Extensions
{
    public static class GizmosExtension
    {
        public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
    }
}