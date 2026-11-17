using UnityEngine;

namespace CZCIT.GlassUI
{
    public static class CurvedPanelProjection
    {
        public static bool TryProject(Transform root, float radius, Ray ray, out Vector3 flatWorld)
        {
            flatWorld = default;
            var origin = root.InverseTransformPoint(ray.origin);
            var direction = root.InverseTransformVector(ray.direction);

            // Intersect the ray with the cylinder centered at local z = -radius.
            float a = direction.x * direction.x + direction.z * direction.z;
            if (a < 1e-8f)
                return false;

            float shiftedZ = origin.z + radius;
            float b = 2f * (origin.x * direction.x + shiftedZ * direction.z);
            float c = origin.x * origin.x + shiftedZ * shiftedZ - radius * radius;
            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
                return false;

            float rootDiscriminant = Mathf.Sqrt(discriminant);
            float near = (-b - rootDiscriminant) / (2f * a);
            float far = (-b + rootDiscriminant) / (2f * a);
            float distance = near > 0f ? near : far;
            if (distance <= 0f)
                return false;

            // Arc length recovers the original horizontal Canvas coordinate.
            var hit = origin + direction * distance;
            float x = Mathf.Atan2(hit.x, hit.z + radius) * radius;
            flatWorld = root.TransformPoint(new Vector3(x, hit.y, 0f));
            return true;
        }
    }
}
