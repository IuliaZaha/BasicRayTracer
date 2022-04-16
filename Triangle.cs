using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    class Triangle : Plane
    {
        public Vector3 v0, v1, v2;

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 normal, Material material) : base(v0, normal, material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }

        public override (float, Vector3, Vector3, Material, bool) GetRayIntersection(Ray ray, float maxDistance)
        {
            var baseResult = base.GetRayIntersection(ray, maxDistance);
            if (baseResult.Item1 < 0)
            {
                return baseResult;
            }

            Vector3 P = baseResult.Item2;

            Vector3 edge0 = v1 - v0;
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v0 - v2;
            Vector3 C0 = P - v0;
            Vector3 C1 = P - v1;
            Vector3 C2 = P - v2;

            if (Vector3.Dot(normal, Vector3.Cross(edge0, C0)) > 0 &&
                Vector3.Dot(normal, Vector3.Cross(edge1, C1)) > 0 &&
                Vector3.Dot(normal, Vector3.Cross(edge2, C2)) > 0)
            {
                return baseResult;
            }
            else
            {
                return (-1f, default, default, default, default);
            }
        }

        public override AABB GetBounds()
        {
            return new AABB(
                Math.Min(Math.Min(v0.X, v1.X), v2.X),
                Math.Max(Math.Max(v0.X, v1.X), v2.X),
                Math.Min(Math.Min(v0.Y, v1.Y), v2.Y),
                Math.Max(Math.Max(v0.Y, v1.Y), v2.Y),
                Math.Min(Math.Min(v0.Z, v1.Z), v2.Z),
                Math.Max(Math.Max(v0.Z, v1.Z), v2.Z));
        }

        public override Vector3 GetBVHPoint()
        {
            return (v0 + v1 + v2) / 3f;
        }
    }
}
