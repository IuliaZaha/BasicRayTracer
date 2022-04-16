using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    class Material
    {
        public float specularity;
        public Shader shader;
        public float transparency;
        public float refractionIndex;
        public Vector3 absorption;

        public Material(Shader shader, float specularity){
            this.shader = shader;
            this.specularity = specularity;
            refractionIndex = 0.0f;
            transparency = 0.0f;
        }

        //add more materials
        public Material(Shader shader, float specularity, float transparency, float refractionIndex, Vector3 absorption)
        {
            this.shader = shader;
            this.specularity = specularity;
            this.transparency = transparency;
            this.refractionIndex = refractionIndex;
            this.absorption = absorption;
        }
    }
}
