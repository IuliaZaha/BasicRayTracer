using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template{
    abstract class Light{
        public Vector3 color;

        public Light(Vector3 color){
            this.color = color;
        }

        public abstract (Vector3, Vector3) CastShadowRay(World world, Vector3 origin);
        
        protected bool AnyCollisions(World world, Ray ray){
               return BVH.CheckAnyIntersection(ray, float.MaxValue);
        }

        protected bool AnyCollisions(World world, Ray ray, float maxDistance2){
               return BVH.CheckAnyIntersection(ray, (float)Math.Sqrt(maxDistance2));
        }
    }
}