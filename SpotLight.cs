using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace template
{
    class SpotLight : PointLight
    {
        Vector3 direction;
        float halfAngle;

        public SpotLight(Vector3 position, Vector3 color, Vector3 direction, float angle) : base(position, color)
        {
            this.direction = Vector3.Normalize(direction);
            this.halfAngle = angle * (float)Math.PI / 360f;
        }

        public override (Vector3, Vector3) CastShadowRay(World world, Vector3 origin)
        {
            var baseResult = base.CastShadowRay(world, origin);
            Vector3 color = baseResult.Item1;
            Vector3 rayDirection = baseResult.Item2;
            if (color == Vector3.Zero)
            {
                return baseResult;
            }
            else
            {
                float rayAngle = Vector3.CalculateAngle(rayDirection, -direction);
                if (rayAngle <= halfAngle)
                {
                    return (color * (1 - (rayAngle / halfAngle)), rayDirection);
                }
                else
                {
                    return (Vector3.Zero, default);
                }
            }
        }
    }
}
