using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template{
    struct AABB{
        public float minX, minY, minZ, maxX, maxY, maxZ;

        public AABB(float minX, float maxX, float minY, float maxY, float minZ, float maxZ){
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            this.minZ = minZ;
            this.maxZ = maxZ;
        }
    }
}
