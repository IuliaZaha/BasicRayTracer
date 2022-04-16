using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template{
    class Sphere : RenderableObject{
        public float radius;
        public float radius2;
        public Vector3 position;

        public Sphere(Vector3 position, float radius, Material material){
            this.position = position;
            this.radius = radius;
            this.radius2 = radius*radius;
            this.material = material;
        }

        public override AABB GetBounds()
        {
            return new AABB(position.X - radius, position.X + radius, position.Y - radius, position.Y + radius, position.Z - radius, position.Z + radius);
        }

        public override Vector3 GetBVHPoint()
        {
            return position;
        }

        public override (float, Vector3, Vector3, Material, bool) GetRayIntersection(Ray ray, float maxDistance)
        {
			// hitAnything, distance, collisionPoint, normal, material, enter
			Vector3 delta = position - ray.origin;
			float dot = Vector3.Dot(delta, ray.direction);

			if ((ray.origin - position).LengthSquared > radius2)
			{
				Vector3 q = delta - dot * ray.direction;
				float p2 = Vector3.Dot(q, q);

				if (p2 > radius2)
				{
					return (-1f, default, default, default, default);
				}

				float t = dot - (float)Math.Sqrt(radius2 - p2);

				if (t < 0 || t > maxDistance)
				{
					return (-1f, default, default, default, default);
				}

				Vector3 collisionPoint = GetCollisionPoint(ray, t);
				Vector3 normal = Vector3.Normalize(collisionPoint - position);

				return (t, collisionPoint, Vector3.Normalize(normal), material, true);
			}
			else
			{
				float a = Vector3.Dot(ray.direction, ray.direction);
				Vector3 diff = -delta;
				float b = Vector3.Dot(2f * ray.direction, diff);
				float c = Vector3.Dot(diff, diff) - radius2;

				float k2 = b * b - 4 * a * c;
				float k = (float)Math.Sqrt(k2);
				float tp = (-b + k) / (2 * a);

				if (tp > maxDistance)
				{
					return (-1f, default, default, default, default);
				}
			
				Vector3 collisionPoint = GetCollisionPoint(ray, tp);
				Vector3 normal = Vector3.Normalize(collisionPoint - position);
				return (tp, collisionPoint, -normal, material, false);
			}
		}
    }
}
