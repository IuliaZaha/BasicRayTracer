using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Input;

namespace template
{
	class Camera
	{
		public Vector3 position;
		public float distance;

		public float movement;
		public float speed;

		public Vector3 p0;
		public Vector3 p1;
		public Vector3 p2;

		public Vector3 right = Vector3.UnitX;
		public Vector3 up = Vector3.UnitY;
		public Vector3 forward = Vector3.UnitZ;

		public float degreeX = 0;
		public float degreeY = (float)Math.PI / 2f;

		public float fishEyeFactor = 1.3f;

		public Camera(Vector3 position, Vector3 forward, float distance)
		{
			this.position = position;
			this.forward = forward;
			this.distance = distance;
		}

		public (Vector3, Vector3, Vector3) CamPlane(Vector3 p0, Vector3 p1, Vector3 p2, RenderSettings settings)
		{
			var keyboard = Keyboard.GetState();

			forward = new Vector3((float)Math.Cos(degreeX) * (float)Math.Cos(degreeY), (float)Math.Sin(degreeX), (float)Math.Cos(degreeX) * (float)Math.Sin(degreeY));
			right = -new Vector3((float)Math.Cos(degreeY + 0.5f * Math.PI), 0, (float)Math.Sin(degreeY + 0.5f * Math.PI));
			up = Vector3.Cross(-right, forward);


			p0 = position + forward * distance + 0.5f * (-right) + up * 0.5f * Game.ratio;
			p1 = position + forward * distance + 0.5f * (right) + up * 0.5f * Game.ratio;
			p2 = position + forward * distance + 0.5f * (-right) - up * 0.5f * Game.ratio;

			(Vector3, Vector3, Vector3) cameraPlane = (p0, p1, p2);
			return cameraPlane;
		}

		public Vector3[,] Render(World world, int scrWidth, int scrHeight, RenderSettings settings)
		{
			Vector3 c = position + forward * distance;

			var camPlane = CamPlane(p0, p1, p2, settings);
			Vector3 u = camPlane.Item2 - camPlane.Item1;
			Vector3 v = camPlane.Item3 - camPlane.Item1;

			int xPixels = (int)Math.Round((float)scrWidth * settings.renderResolution) * settings.AA;
			int yPixels = (int)Math.Round((float)scrHeight * settings.renderResolution) * settings.AA;

			Vector3[,] colors = new Vector3[xPixels, yPixels];

			ParallelOptions options = new ParallelOptions();
			options.MaxDegreeOfParallelism = 7;

			ProgressBar progressBar = new ProgressBar();
			DateTime start = DateTime.Now;
			if (settings.showProgress)
			{
				progressBar.Maximum = colors.GetLength(0);
			}

			Parallel.For(0, colors.GetLength(0), options, x => {
				float fx = ((float)x / (float)xPixels);
				for (int y = 0; y < colors.GetLength(1); y++)
				{
					float fy = ((float)y / (float)yPixels);
					Vector3 p = camPlane.Item1 + u * fx + v * fy;

					Vector3 dir = p - position;
					float angle = Vector3.CalculateAngle(dir, forward);
					Vector3 axis = Vector3.Cross(dir, forward);
					Quaternion rot = Quaternion.FromAxisAngle(axis, -angle * fishEyeFactor);

					Vector3 test = Vector3.Transform(forward, rot);
					Ray ray = new Ray(p, test);

					colors[x, y] = CastRay(world, settings, ray, new Stack<float>(new float[1] { 1f }), Vector3.Zero, 0);
				}
				if (settings.showProgress)
				{
					progressBar.Value++;
					if (progressBar.Value % 10 == 0)
					{
						Console.WriteLine(String.Format("Progress: {0:P2}", (float)progressBar.Value / (float)progressBar.Maximum));
					}
				}
			});

			if (settings.showProgress)
			{
				Console.WriteLine("Render duration: " + DateTime.Now.Subtract(start).TotalSeconds);
			}

			if (settings.AA > 1)
			{
				return downscaleAA(colors, settings.AA);
			}

			if (settings.renderResolution == 1f)
			{
				return colors;
			}
			else
			{
				//scale to correct size

				Vector3[,] scaledColors = new Vector3[scrWidth, scrHeight];
				ScaleResolion(colors, scaledColors);

				return scaledColors;
			}
		}

		private Vector3[,] downscaleAA(Vector3[,] input, int AA)
		{
			int resultSizeX = input.GetLength(0) / AA;
			int resultSizeY = input.GetLength(1) / AA;
			Vector3[,] result = new Vector3[resultSizeX, resultSizeY];

			float AA2 = (float)(AA * AA);

			for (int x = 0; x < resultSizeX; x++)
			{
				for (int y = 0; y < resultSizeY; y++)
				{
					Vector3 color = Vector3.Zero;
					for (int AAx = 0; AAx < AA; AAx++)
					{
						for (int AAy = 0; AAy < AA; AAy++)
						{
							result[x, y] += input[x * AA + AAx, y * AA + AAy] / AA2;
						}
					}
				}
			}
			return result;
		}

		private void ScaleResolion(Vector3[,] input, Vector3[,] output)
		{
			int scrWidth = output.GetLength(0);
			int scrHeight = output.GetLength(1);

			int xPixels = input.GetLength(0);
			int yPixels = input.GetLength(1);

			ParallelOptions options = new ParallelOptions();
			options.MaxDegreeOfParallelism = 7;
			Parallel.For(0, scrWidth, options, x => {
				float fx = ((float)x / (float)scrWidth * (float)(xPixels - 1));
				int floorx = (int)Math.Floor(fx);
				int ceilx = (int)Math.Ceiling(fx);
				float restx = fx - floorx;
				for (int y = 0; y < scrHeight; y++)
				{
					float fy = ((float)y / (float)scrHeight * (float)(yPixels - 1));
					int floory = (int)Math.Floor(fy);
					int ceily = (int)Math.Ceiling(fy);
					float resty = fy - floory;

					Vector3 bottomLeftColor = input[floorx, floory];
					Vector3 bottomRightColor = input[ceilx, floory];
					Vector3 topLeftColor = input[floorx, ceily];
					Vector3 topRightColor = input[ceilx, ceily];

					Vector3 bottomColor = Vector3.Lerp(bottomLeftColor, bottomRightColor, restx);
					Vector3 topColor = Vector3.Lerp(topLeftColor, topRightColor, restx);

					Vector3 scaledColor = Vector3.Lerp(bottomColor, topColor, resty);

					output[x, y] = scaledColor;
				}
			});
		}

		private Vector3 CastRay(World world, RenderSettings settings, Ray ray, Stack<float> currRefractionIndex, Vector3 absorption, int currDepth)
		{
			var intersection = GetNearestIntersection(world, ray, float.MaxValue);
			float distance = intersection.Item1;
			Vector3 collisionPoint = intersection.Item2;
			Vector3 normal = intersection.Item3;
			Material material = intersection.Item4;
			bool enter = intersection.Item5;

			if (distance < 0f)
			{
				return world.skyColor;
			}

			Vector3 color = Vector3.Zero;
			float fresnel = 0;
			float transparency = material.transparency;

			if (material.transparency > 0)
			{
				//transparent color
				float cos1 = Vector3.Dot(normal, -ray.direction);
				float newRefractionIndex = enter ? material.refractionIndex : 1f;
				float refractionIndexFactor = currRefractionIndex.Peek() / newRefractionIndex;
				float k = 1 - refractionIndexFactor * refractionIndexFactor * (1 - cos1 * cos1);

				if (k < 0)
				{
					if (currDepth < settings.maxDepth)
					{
						color += material.transparency * CastRay(world, settings, ray.Reflect(collisionPoint, normal), currRefractionIndex, absorption, currDepth + 1);
					}
				}
				else
				{
					if (currDepth < settings.maxDepth)
					{
						Vector3 refractedDirection = refractionIndexFactor * ray.direction + normal * (refractionIndexFactor * cos1 - (float)Math.Sqrt(k));

						//Calculate Fresnel
						float cos2 = (float)Math.Sqrt(1 - (refractionIndexFactor * Math.Sin(Math.Acos(cos1))));

						float sPolarized = (currRefractionIndex.Peek() * cos1 - newRefractionIndex * cos2) / (currRefractionIndex.Peek() * cos1 + newRefractionIndex * cos2);
						float pPolarized = (currRefractionIndex.Peek() * cos2 - newRefractionIndex * cos1) / (currRefractionIndex.Peek() * cos2 + newRefractionIndex * cos1);
						fresnel = (sPolarized * sPolarized + pPolarized * pPolarized) / 2f;

						transparency = material.transparency * (1f - fresnel);

						//color = Vector3.One * fresnel;
						Vector3 newAbsorption = enter ? material.absorption : Vector3.Zero;
						if (enter)
						{
							currRefractionIndex.Push(newRefractionIndex);
						}
						else
						{
							if (currRefractionIndex.Count > 1)
							{
								currRefractionIndex.Pop();
							}
						}
						color += transparency * (1f - fresnel) * CastRay(world, settings, new Ray(collisionPoint, refractedDirection), currRefractionIndex, newAbsorption, currDepth + 1);
					}
				}
			}

			float specularity = material.specularity + fresnel * material.transparency;
			if (specularity > 0f && currDepth < settings.maxDepth)
			{
				//Specular color
				color += specularity * material.shader.GetColor(material, collisionPoint, normal) * CastRay(world, settings, ray.Reflect(collisionPoint, normal), currRefractionIndex, absorption, currDepth + 1);
			}

			if ((1f - transparency - specularity) > 0f)
			{
				//diffuse color
				color += (1f - transparency - specularity) * material.shader.GetColor(material, collisionPoint, normal) * GetDirectIllumination(world, collisionPoint, normal);
			}

			//Absorption
			if (absorption != Vector3.Zero)
			{
				color.X *= (float)Math.Exp(-absorption.X * distance);
				color.Y *= (float)Math.Exp(-absorption.Y * distance);
				color.Z *= (float)Math.Exp(-absorption.Z * distance);
			}
			return color;
		}

		private Vector3 GetDirectIllumination(World world, Vector3 point, Vector3 normal)
		{
			Vector3 color = Vector3.Zero;

			foreach (Light light in world.lights)
			{
				var shadowRayResult = light.CastShadowRay(world, point);
				Vector3 lightColor = shadowRayResult.Item1;
				Vector3 lightDirection = shadowRayResult.Item2;
				if (lightColor != Vector3.Zero)
				{
					color += lightColor * Math.Max(0, Vector3.Dot(normal, lightDirection));
				}
			}

			return color;
		}

		private (float, Vector3, Vector3, Material, bool) GetNearestIntersection(World world, Ray ray, float maxDistance)
		{
			return BVH.GetRayIntersection(ray, maxDistance);

			//The old ray intersection used before BVH was implemented.

			//// distance, collisionPoint, normal, material, enter
			//bool hitAnything = false;
			//(float, Vector3, Vector3, Material, bool) closestIntersection = (maxDistance, default, default, default, default);

			//foreach (RenderableObject renderableObject in world.renderableObjects) {
			//    var intersection = renderableObject.GetRayIntersection(ray, closestIntersection.Item1);
			//    if (intersection.Item1 > 0f) {
			//        hitAnything = true;
			//        closestIntersection = intersection;
			//    }
			//}
			//if (hitAnything) {
			//    return closestIntersection;
			//} else {
			//    return (-1f, default, default, default, default);
			//}
		}

		public Vector3 CamUpdate(float cspeed)
		{
			var keyboard = Keyboard.GetState();
			if (keyboard[Key.D]) position = position + (right * cspeed * Game.deltaTime);
			if (keyboard[Key.A]) position = position + (-right * cspeed * Game.deltaTime);
			if (keyboard[Key.E]) position = position + (up * cspeed * Game.deltaTime);
			if (keyboard[Key.Q]) position = position + (-up * cspeed * Game.deltaTime);
			if (keyboard[Key.W]) position = position + (forward * cspeed * Game.deltaTime);
			if (keyboard[Key.S]) position = position + (-forward * cspeed * Game.deltaTime);
			return position;
		}

		public Vector3 CamTilt(float cspeed)
		{
			var keyboard = Keyboard.GetState();
			if (keyboard[Key.Right]) degreeY = degreeY - (cspeed * Game.deltaTime);
			if (keyboard[Key.Left]) degreeY = degreeY + (cspeed * Game.deltaTime);
			if (keyboard[Key.Up]) degreeX = degreeX + (cspeed * Game.deltaTime);
			if (keyboard[Key.Down]) degreeX = degreeX - (cspeed * Game.deltaTime);
			return position;
		}

		public static float Distance(Vector3 a, Vector3 b)
		{
			Vector3 vector = new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
			return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
		}
	}
}
