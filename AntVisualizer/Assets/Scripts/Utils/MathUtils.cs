using System;

namespace ElectricServiceCompany
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class MathUtils
    {
        public static Quaternion SpreadToRotation(float spread)
        {
            return Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));
        }

        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            if (Time.deltaTime < Mathf.Epsilon) return rot;
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;

            // ensure deriv is tangent
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        public static float AngleTargetYZero(Transform owner, Transform target, bool sign = false)
        {
            Vector3 pivotYZero = owner.transform.position;
            pivotYZero.y = 0.0f;
            Vector3 targetYZero = target.position;
            targetYZero.y = 0.0f;
            Vector3 direction = (targetYZero - pivotYZero).normalized;
            if (sign)
            {
                return Vector3.SignedAngle(direction, owner.forward, Vector3.up);
            }
            else
            {
                return Vector3.Angle(owner.forward, direction);
            }
        }

        public static Matrix4x4 CreateRotationAroundArbitraryAxis(Vector3 axis, float angle)
        {
            var nv= Vector3.Normalize(new Vector3(axis.x, axis.y, axis.z));
            var dX = nv.x;
            var dY = nv.y;
            var dZ = nv.z;
            var ar = angle * Math.PI / 180;
            var cos = (float)Math.Cos(ar);
            var sin = (float)Math.Sin(ar);
            var rm = new Matrix4x4(new Vector4(cos + dX*dX*(1 - cos), dX * dY * (1 - cos) - dZ * sin, dX * dZ * (1 - cos) + dY * sin, 0f),
                new Vector4(dY * dX * (1 - cos) + dZ * sin, cos + dY*dY * (1 - cos), dY * dZ * (1 - cos) - dX * sin, 0),
                new Vector4(dZ * dX * (1 - cos) - dY * sin, dZ * dY * (1 - cos) + dX * sin, cos + dZ*dZ * (1 - cos), 0),
                new Vector4(0, 0, 0, 1));
            
            return rm;
        }

        public static Vector3 RotatePointAroundAxis(Vector3 point,Vector3 axisCenter, Vector3 axis, float angle)
        {
            var rm = CreateRotationAroundArbitraryAxis(axis, angle);
            return ApplyRotationAroundAxis(rm,point,axisCenter);
        }

        public static Vector3 ApplyRotationAroundAxis(Matrix4x4 rotationMatrix, Vector3 point, Vector3 axisCenter)
        {
            var diff = point - axisCenter;
            diff = rotationMatrix.MultiplyVector(diff);
            return axisCenter + diff;
        }

        public static Vector3 TransformPoint(Vector3 transformPos, Vector3 forward, Vector3 point) {
            return TransformPoint(transformPos, Quaternion.LookRotation(forward), Vector3.one, point);
        }
        public static Vector3 TransformPoint(Vector3 transformPos, Quaternion transformRotation, Vector3 transformScale,
            Vector3 point)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transformPos, transformRotation, transformScale);
            return matrix.MultiplyPoint3x4(point);
        }
        
        public static Vector3 TransformDirection(Vector3 transformPos, Vector3 forward, Vector3 direction) {
            return TransformDirection(transformPos, Quaternion.LookRotation(forward), Vector3.one, direction);
        }
        public static Vector3 TransformDirection(Vector3 transformPos, Quaternion transformRotation, Vector3 transformScale,
            Vector3 direction)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transformPos, transformRotation, transformScale);
            return matrix.MultiplyVector(direction);
        }

        public static Vector3 InverseTransformPoint(Vector3 transformPos, Vector3 forward, Vector3 pos) {
            return InverseTransformPoint(transformPos, Quaternion.LookRotation(forward), Vector3.one, pos);
        }
        
        public static Vector3 InverseTransformPoint(Vector3 transformPos, Quaternion transformRotation, Vector3 transformScale, Vector3 pos) {
            Matrix4x4 matrix = Matrix4x4.TRS(transformPos, transformRotation, transformScale);
            Matrix4x4 inverse = matrix.inverse;
            return inverse.MultiplyPoint3x4(pos);
        }
        
        public static Vector3 InverseTransformDirection(Vector3 transformPos, Vector3 forward, Vector3 direction) {
            return InverseTransformPoint(transformPos, Quaternion.LookRotation(forward), Vector3.one, direction);
        }
        
        public static Vector3 InverseTransformDirection(Vector3 transformPos, Quaternion transformRotation, Vector3 transformScale, Vector3 direction) {
            Matrix4x4 matrix = Matrix4x4.TRS(transformPos, transformRotation, transformScale);
            Matrix4x4 inverse = matrix.inverse;
            return inverse.MultiplyVector(direction);
        }

        public static bool LookAtPoint(Transform owner, Transform point)
        {
            float angle = 130.0f;
            Vector3 dir = (point.position - owner.position).normalized;
            return Vector3.Angle(owner.forward, dir) < angle * 0.5f;
        }
    }
}