using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace template {

class Game{
		World world;
		Camera cam;
		float t = 0;
		public static float ratio;

		private Stopwatch sw = new Stopwatch();
		public float lastFrameTime;
		public static float deltaTime;
		bool moving = true;

		private bool saved = true;
		Vector3[,] pixelColors;

		public Surface screen;

		Vector3[,] PixelColors
		{
			get
			{
				return pixelColors;
			}
			set
			{
				saved = false;
				pixelColors = value;
			}
		}

		RenderSettings performanceSettings = new RenderSettings
		{
			maxDepth = 1,
			renderResolution = 0.25f,
			AA = 1,
			showProgress = false
		};

		RenderSettings renderSettings = new RenderSettings
		{
			maxDepth = 10,
			renderResolution = 1f,
			AA = 1,
			showProgress = true
		};


		public void Init()
	{
			//screen.Clear( 0x29922ff );
			Console.WriteLine(Directory.GetCurrentDirectory());
			sw.Start();
			ratio = (float)screen.height / screen.width;
			world = new World();
			cam = new Camera(new Vector3(0f, 5.5f, 100f), Vector3.UnitZ, 1f);
			world.camera = cam;
			world.skyColor = new Vector3(3.8f, 3.8f, 6f);

			world.renderableObjects.Add(new Sphere(new Vector3(-7f, 1f, 2f), 1f, new Material(new Shader(Vector3.One), 0f, 1f, 1.52f, new Vector3(1f, 1f, 1f))));
			world.renderableObjects.Add(new Sphere(new Vector3(-7f, 1f, 2f), 0.3f, new Material(new Shader(Vector3.One), 0f, 1f, 2.2f, new Vector3(1f, 1f, 1f))));

			//LoadOBJ(Directory.GetCurrentDirectory() + @"\..\..\instruments\cellotr.obj", new Vector3(0f, 0f, -300f), 1.3f, new Material(new Shader(Vector3.One), 0f, 1f, 1.52f, new Vector3(1f, 1f, 0.8f) * 1.3f));
			//LoadOBJ(Directory.GetCurrentDirectory() + @"\..\..\instruments\piano06.obj", new Vector3(0f, 0f, -300f), 1.3f, new Material(new Shader(Vector3.One), 0f, 1f, 1.52f, new Vector3(1f, 1f, 0.8f) * 1.3f));

			world.renderableObjects.Add(new Triangle(new Vector3(-1000f, 0f, -1000f), new Vector3(-1000f, 0f, 1000f), new Vector3(1000f, 0f, -1000f), new Vector3(0f, 1f, 0f), new Material(new Checkerboard(new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f)), 0.1f)));
			world.renderableObjects.Add(new Triangle(new Vector3(1000f, 0f, -1000f), new Vector3(-1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f), new Vector3(0f, 1f, 0f), new Material(new Checkerboard(new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f)), 0.1f)));

			BVH.Construct(world.renderableObjects.ToArray());
		}
	public void Tick()
	{
			//screen.Print( "hello world!", 2, 2, 0xffffff );
			float currentFrameTime = (float)sw.Elapsed.TotalSeconds;
			deltaTime = currentFrameTime - lastFrameTime;

			var keyboard = Keyboard.GetState();
			if (keyboard[Key.Space])
			{
				moving = false;

				PixelColors = cam.Render(world, screen.width, screen.height, renderSettings);

				PostProcessing.PostProcess(PixelColors);
				PlotColors(PixelColors);
			}

			if (moving)
			{
				Console.WriteLine(1f / deltaTime);

				PixelColors = cam.Render(world, screen.width, screen.height, performanceSettings);
				PlotColors(PixelColors);

				cam.CamUpdate(3f);
				cam.CamTilt(0.5f);

				t += 1;

			}
			else
			{
				if (!saved && keyboard[Key.P])
				{
					Console.WriteLine("Saving...");
					saved = true;

					Bitmap bitmap = new Bitmap(screen.width, screen.height);
					for (int x = 0; x < screen.width; x++)
					{
						for (int y = 0; y < screen.height; y++)
						{
							Color c = Vec3ColorToColor(PixelColors[x, y]);
							bitmap.SetPixel(x, y, c);
						}
					}

					for (int i = 0; i < 5000; i++)
					{
						string path = Directory.GetCurrentDirectory() + @"\..\..\Images\IMG_" + i + ".png";
						if (!File.Exists(path))
						{
							bitmap.Save(path, ImageFormat.Png);
							break;
						}
					}
					Console.WriteLine("Done saving!");

				}

				int numKeys = (int)Key.LastKey;

				for (int i = 0; i < numKeys; ++i)
				{
					Key iAsKey = (Key)i;

					if (iAsKey != Key.Space && iAsKey != Key.P && keyboard[iAsKey])
					{
						moving = true;
					}
				}
			}
			lastFrameTime = currentFrameTime;
		}
	public void Render()
	{
		// render stuff over the backbuffer (OpenGL, sprites)
	}

		private void PlotColors(Vector3[,] PixelColors)
		{
			for (int x = 0; x < PixelColors.GetLength(0); x++)
			{
				for (int y = 0; y < PixelColors.GetLength(1); y++)
				{
					int color = Vec3ColorToInt(PixelColors[x, y]);
					screen.Plot(x, y, color);
				}
			}
		}

		private Color Vec3ColorToColor(Vector3 color)
		{
			var rgb = Vec3ColorToInts(color);
			return Color.FromArgb(rgb.Item1, rgb.Item2, rgb.Item3);
		}

		private int Vec3ColorToInt(Vector3 color)
		{
			var rgb = Vec3ColorToInts(color);
			return (rgb.Item1 << 16) + (rgb.Item2 << 8) + rgb.Item3;
		}

		private (int, int, int) Vec3ColorToInts(Vector3 color)
		{
			int red = (int)(Math.Min(color.X, 1f) * 255f);
			int green = (int)(Math.Min(color.Y, 1f) * 255f);
			int blue = (int)(Math.Min(color.Z, 1f) * 255f);
			return (red, green, blue);
		}

		private void LoadOBJ(string path, Vector3 origin, float scale, Material material)
		{
			Console.WriteLine("Loading");
			string[] lines = File.ReadAllLines(path);

			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();

			bool initialV = true;
			float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f, minZ = 0f, maxZ = 0f;
			List<RenderableObject> renderableObjects = new List<RenderableObject>();

			foreach (string line in lines)
			{
				if (line != "")
				{
					string[] parts = line.Split();
					switch (parts[0])
					{
						case "v":
							Vector3 v = ParseCoordinates(parts) * scale + origin;
							vertices.Add(v);
							if (initialV)
							{
								initialV = false;
								minX = v.X;
								maxX = v.X;
								minY = v.Y;
								maxY = v.Y;
								minZ = v.Z;
								maxZ = v.Z;
							}
							minX = Math.Min(minX, v.X);
							maxX = Math.Max(maxX, v.X);
							minY = Math.Min(minY, v.Y);
							maxY = Math.Max(maxY, v.Y);
							minZ = Math.Min(minZ, v.Z);
							maxZ = Math.Max(maxZ, v.Z);
							break;
						case "vn":
							normals.Add(ParseCoordinates(parts));
							break;
						case "f":
							Vector3 v0 = vertices[int.Parse(parts[1].Split('/')[0]) - 1];
							Vector3 v1 = vertices[int.Parse(parts[2].Split('/')[0]) - 1];
							Vector3 v2 = vertices[int.Parse(parts[3].Split('/')[0]) - 1];

							Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v2, v0 - v2));
							renderableObjects.Add(new Triangle(v0, v1, v2, -normal, material));

							break;
					}
				}
			}

			Console.WriteLine(renderableObjects.Count + " triangles.");
			world.renderableObjects.AddRange(renderableObjects);

			Console.WriteLine("Done");

			//return boundingBox;
		}

		private Vector3 ParseCoordinates(string[] parts)
		{
			return new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
		}
	}

} // namespace Template
