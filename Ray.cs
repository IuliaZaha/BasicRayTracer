using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    struct Ray{
        public Vector3 origin;
        public Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction){
            this.origin = origin + direction * 0.0001f;
            this.direction = Vector3.Normalize(direction);
        }

        //add Reflection
        public Ray Reflect(Vector3 point, Vector3 normal){
            Vector3 reflected = direction - 2 * (Vector3.Dot(direction, normal) * normal);
            return new Ray(point, reflected);
        }
    }
}
