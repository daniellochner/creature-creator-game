﻿using UnityEngine;

namespace DanielLochner.Assets
{
    public static class Vector3Utility
    {
        public static Vector3 Clamp(this Vector3 vector, float min, float max)
        {
            return new Vector3(Mathf.Clamp(vector.x, min, max), Mathf.Clamp(vector.y, min, max), Mathf.Clamp(vector.z, min, max));
        }
        public static Vector3 Inverse(this Vector3 vector)
        {
            return new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
        }
        public static Vector3 Multiply(this Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
        }
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            if (angles != Vector3.zero)
            {
                return Quaternion.Euler(angles) * (point - pivot) + pivot;
            }
            return point;
        }

        /// <summary>
        /// Checks if the distance between p1 and p2 is greater (dir = 1) or less (dir = -1) than d.
        /// </summary>
        public static bool SqrDistanceComp(Vector3 p1, Vector3 p2, float d)
        {
            return (p1 - p2).sqrMagnitude > (d * d);
        }
    }
}