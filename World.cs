using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace template
{
    class World
    {
        public Camera camera;
        public List<RenderableObject> renderableObjects=new List<RenderableObject>();
        public List<Light> lights = new List<Light>();
        public Vector3 skyColor;
    }
}
