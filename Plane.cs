using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    abstract class Plane : RenderableObject
    {
        public Vector3 point;
        public Vector3 normal;
        private float d;

        public Plane(Vector3 point, Vector3 normal, Material material)
        {
            this.point = point;
            this.normal = Vector3.Normalize(normal);
            this.d = -Vector3.Dot(point, normal);
            this.material = material;
        }

        public override AABB GetBounds()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetBVHPoint()
        {
            throw new NotImplementedException();
        }

        public override (float, Vector3, Vector3, Material, bool) GetRayIntersection(Ray ray, float maxDistance)
        {
            float dot = Vector3.Dot(ray.direction, normal);
            float t = -(Vector3.Dot(ray.origin, normal) + d) / dot;
            if (t <= 0 || t > maxDistance)
            {
                return (-1f, default, default, default, default);
            }
            else
            {
                Vector3 collisionPoint = GetCollisionPoint(ray, t);
                bool frontFace = dot < 0;
                return (t, collisionPoint, frontFace ? normal : -normal, material, frontFace);
            }
        }
    }
}
