using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace template
{
    class Checkerboard : Shader
    {
        protected Vector3 altColor;

        public Checkerboard(Vector3 color, Vector3 altColor) : base(color)
        {
            this.altColor = altColor;
        }

        public override Vector3 GetColor(Material material, Vector3 point, Vector3 normal)
        {
            int sum = (int)Math.Floor(point.X) + (int)Math.Floor(point.Z);
            return ((sum & 1) == 1) ? color : altColor;
        }
    }
}
