using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    static class BVH
    {
        public static int[] indices;
        public static BVHNode[] pool;
        public static int poolPtr;
        public static RenderableObject[] renderableObjects;

        public static void Construct(RenderableObject[] renderableObjects)
        {
            BVH.renderableObjects = renderableObjects;
            int N = renderableObjects.Length;

            // create index array
            indices = new int[N];
            for (int i = 0; i < N; i++)
            {
                indices[i] = i;
            }

            // allocate BVH root node
            pool = new BVHNode[N * 2 - 1];
            BVHNode root = pool[0];
            poolPtr = 2;

            // subdivide root node
            root.leftFirst = 0;
            root.count = N;
            root.bounds = CalculateBounds(root.leftFirst, root.count);
            root.Subdivide(0);
        }

        public static AABB CalculateBounds(int first, int count)
        {
            if (count == 0)
            {
                return new AABB();
            }
            else
            {
                AABB bounds = renderableObjects[indices[first]].GetBounds();
                for (int i = first + 1; i < first + count; i++)
                {
                    AABB newBounds = renderableObjects[indices[i]].GetBounds();
                    bounds.minX = Math.Min(bounds.minX, newBounds.minX);
                    bounds.maxX = Math.Max(bounds.maxX, newBounds.maxX);
                    bounds.minY = Math.Min(bounds.minY, newBounds.minY);
                    bounds.maxY = Math.Max(bounds.maxY, newBounds.maxY);
                    bounds.minZ = Math.Min(bounds.minZ, newBounds.minZ);
                    bounds.maxZ = Math.Max(bounds.maxZ, newBounds.maxZ);
                }
                return bounds;
            }
        }

        public static (float, Vector3, Vector3, Material, bool) GetRayIntersection(Ray ray, float maxDistance)
        {
            return pool[0].GetRayIntersection(ray, maxDistance);
        }

        public static bool CheckAnyIntersection(Ray ray, float maxDistance)
        {
            return pool[0].CheckAnyIntersection(ray, maxDistance);
        }
    }
}
