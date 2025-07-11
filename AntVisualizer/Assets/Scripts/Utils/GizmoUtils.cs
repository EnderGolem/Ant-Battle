using Unity.Mathematics;

namespace ElectricServiceCompany
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class GizmoUtils
    {
        private const float GIZMO_DISK_THICKNESS = 0.01f;
        public static void DrawWireDisk(Vector3 position, float radius, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1, GIZMO_DISK_THICKNESS, 1));
            Gizmos.DrawWireSphere(Vector3.zero, radius);
            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;
        }

        public static void DrawDisk(Vector3 position, float radius, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1, GIZMO_DISK_THICKNESS, 1));
            Gizmos.DrawSphere(Vector3.zero, radius);
            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;
        }

        public static void DrawWireConeOfView(Vector3 position, Vector3 direction,Vector3 rotation, float distance, float angle, int segmentCount = 8)
        {
            direction = Matrix4x4.Rotate(quaternion.Euler(rotation * Mathf.Deg2Rad)).MultiplyVector(direction);
            
            var tangent = Vector3.right;
            var biNormal = Vector3.up;
            Vector3.OrthoNormalize(ref direction, ref tangent, ref biNormal);
            
            var point = position + direction * distance;

            var curPos = MathUtils.RotatePointAroundAxis(point, position,biNormal, angle);
            
            
            var mat = MathUtils.CreateRotationAroundArbitraryAxis(direction, 360/segmentCount);
            var lastPos = curPos;
            for (int i = 0; i < segmentCount; i++)
            {
                Gizmos.DrawLine(position, curPos);
                curPos = MathUtils.ApplyRotationAroundAxis(mat, curPos, position);
                Gizmos.DrawLine(curPos, lastPos);
                lastPos = curPos;
            }
        }
    }
}