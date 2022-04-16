using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    abstract class RenderableObject
    {
        public Material material;

        public abstract (float, Vector3, Vector3, Material, bool) GetRayIntersection(Ray ray, float maxDistance);
        // distance, collisionPoint, normal, material, enter

        public abstract AABB GetBounds();

        public abstract Vector3 GetBVHPoint();

        public Vector3 GetCollisionPoint(Ray ray, float t)
        {
            return ray.origin + ray.direction * t;
        }
    }
}
