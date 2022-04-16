using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
	class PointLight : Light
	{
		public Vector3 position;

		public PointLight(Vector3 position, Vector3 color) : base(color)
		{
			this.position = position;
		}

		public override (Vector3, Vector3) CastShadowRay(World world, Vector3 origin)
		{
			// color, rayDirection
			Vector3 lightVector = position - origin;
			Ray shadowRay = new Ray(origin, lightVector);
			if (!AnyCollisions(world, shadowRay, lightVector.LengthSquared))
			{
				return (color / lightVector.LengthSquared, shadowRay.direction);
			}
			return (Vector3.Zero, default);
		}
	}
}
