using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template{
    class Shader{
        protected Vector3 color;

        public Shader(Vector3 color){
            this.color = color;
        }

        public virtual Vector3 GetColor(Material material, Vector3 point, Vector3 normal){
            return color;
        }
    }
}
