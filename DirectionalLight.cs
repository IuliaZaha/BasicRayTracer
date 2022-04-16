using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
	class DirectionalLight : Light
	{
		Vector3 direction;

		public DirectionalLight(Vector3 direction, Vector3 color) : base(color)
		{
			this.direction = Vector3.Normalize(direction);
		}

		public override (Vector3, Vector3) CastShadowRay(World world, Vector3 origin)
		{
			// color, rayDirection
			Ray shadowRay = new Ray(origin, -direction);
			if (!AnyCollisions(world, shadowRay))
			{
				return (color, -direction);
			}
			return (Vector3.Zero, default);
		}
	}
}
