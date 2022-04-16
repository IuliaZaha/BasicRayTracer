using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    struct BVHNode
    {
        static int i = 0;

        public AABB bounds;
        public int leftFirst;
        public int count;

        public void Subdivide(int poolIndex)
        {
            if (count < 3) return;
            int left = BVH.poolPtr;
            BVH.poolPtr += 2;
            Partition(left);
            leftFirst = left;
            BVH.pool[poolIndex] = this;
            BVH.pool[leftFirst].Subdivide(leftFirst);
            BVH.pool[leftFirst + 1].Subdivide(leftFirst + 1);
            return;
        }

        public void Partition(int left)
        {
            //Split on the average BVHPoint for now
            float sizeX = bounds.maxX - bounds.minX;
            float sizeY = bounds.maxY - bounds.minY;
            float sizeZ = bounds.maxZ - bounds.minZ;

            float split = 0;
            int swapped = 0;
            int tested = 0;
            int dimension;

            if (sizeX > sizeY && sizeX > sizeZ)
            {
                //split along X
                dimension = 0;
            }
            else if (sizeY > sizeZ)
            {
                //split along Y
                dimension = 1;
            }
            else
            {
                //split along Z
                dimension = 2;
            }

            for (int i = leftFirst; i < leftFirst + count; i++)
            {
                split += GetValue(BVH.renderableObjects[BVH.indices[i]].GetBVHPoint(), dimension);
            }
            split /= (float)count;

            i++;

            while (tested < count - swapped)
            {
                if (GetValue(BVH.renderableObjects[BVH.indices[leftFirst + tested]].GetBVHPoint(), dimension) < split - 0.000001f)
                {
                    tested++;
                }
                else
                {
                    int newIndex = BVH.indices[leftFirst + count - swapped - 1];

                    BVH.indices[leftFirst + count - swapped - 1] = BVH.indices[leftFirst + tested];
                    BVH.indices[leftFirst + tested] = newIndex;
                    swapped++;
                }
            }

            //TODO: check performance of this vs making it a leaf node
            if (tested == 0)
            {
                tested = swapped / 2;
                swapped -= tested;
            }

            BVH.pool[left].leftFirst = leftFirst;
            BVH.pool[left].count = tested;
            BVH.pool[left].bounds = BVH.CalculateBounds(leftFirst, tested);

            BVH.pool[left + 1].leftFirst = leftFirst + tested;
            BVH.pool[left + 1].count = swapped;
            BVH.pool[left + 1].bounds = BVH.CalculateBounds(leftFirst + tested, swapped);

            count = 0;
        }

        float GetValue(Vector3 vector, int dimension)
        {
            if (dimension == 0)
            {
                return vector.X;
            }
            else if (dimension == 1)
            {
                return vector.Y;
            }
            else
            {
                return vector.Z;
            }
        }

        public (float, Vector3, Vector3, Material, bool) GetRayIntersection(Ray ray, float maxDistance)
        {
            if (!CheckBoundingBoxIntersection(ray, maxDistance))
            {
                return (-1f, default, default, default, default);
            }

            if (count > 0)
            {
                bool hitAnything = false;
                (float, Vector3, Vector3, Material, bool) closestIntersection = (maxDistance, default, default, default, default);

                for (int i = leftFirst; i < leftFirst + count; i++)
                {
                    RenderableObject renderableObject = BVH.renderableObjects[BVH.indices[i]];
                    var intersection = renderableObject.GetRayIntersection(ray, closestIntersection.Item1);
                    if (intersection.Item1 > 0f)
                    {
                        hitAnything = true;
                        closestIntersection = intersection;
                    }
                }

                if (hitAnything)
                {
                    return closestIntersection;
                }
                else
                {
                    return (-1f, default, default, default, default);
                }
            }
            else
            {
                var leftIntersection = BVH.pool[leftFirst].GetRayIntersection(ray, maxDistance);
                if (leftIntersection.Item1 < 0)
                {
                    return BVH.pool[leftFirst + 1].GetRayIntersection(ray, maxDistance);
                }
                else
                {
                    var rightIntersection = BVH.pool[leftFirst + 1].GetRayIntersection(ray, leftIntersection.Item1);
                    if (rightIntersection.Item1 < 0)
                    {
                        return leftIntersection;
                    }
                    else
                    {
                        return leftIntersection.Item1 < rightIntersection.Item1 ? leftIntersection : rightIntersection;
                    }
                }
            }
        }

        public bool CheckBoundingBoxIntersection(Ray ray, float maxDistance)
        {
            Vector3 dirFrac = new Vector3(
                1.0f / ray.direction.X,
                1.0f / ray.direction.Y,
                1.0f / ray.direction.Z);

            // lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
            // r.org is origin of ray
            float t1 = (bounds.minX - ray.origin.X) * dirFrac.X;
            float t2 = (bounds.maxX - ray.origin.X) * dirFrac.X;
            float t3 = (bounds.minY - ray.origin.Y) * dirFrac.Y;
            float t4 = (bounds.maxY - ray.origin.Y) * dirFrac.Y;
            float t5 = (bounds.minZ - ray.origin.Z) * dirFrac.Z;
            float t6 = (bounds.maxZ - ray.origin.Z) * dirFrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0)
            {
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                return false;
            }

            if (tmin > maxDistance)
            {
                return false;
            }

            return true;
        }

        public bool CheckAnyIntersection(Ray ray, float maxDistance)
        {
            if (!CheckBoundingBoxIntersection(ray, maxDistance))
            {
                return false;
            }

            if (count > 0)
            {

                for (int i = leftFirst; i < leftFirst + count; i++)
                {
                    RenderableObject renderableObject = BVH.renderableObjects[BVH.indices[i]];
                    var intersection = renderableObject.GetRayIntersection(ray, maxDistance);
                    if (intersection.Item1 > 0f)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return BVH.pool[leftFirst].CheckAnyIntersection(ray, maxDistance) || BVH.pool[leftFirst + 1].CheckAnyIntersection(ray, maxDistance);
            }
        }
    }
}
