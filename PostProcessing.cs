using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    static class PostProcessing
    {
        public static void PostProcess(Vector3[,] input)
        {
            CorrectGamma(input);
            Vignette(input, 1.5f, 0.8f);
            ChromaticAbberate(input, 1f, 4f);
        }

        public static void CorrectGamma(Vector3[,] input, float gamma = 0.8f)
        {
            Parallel.For(0, input.GetLength(0), x => {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    float r = (float)Math.Pow(input[x, y].X, gamma);
                    float g = (float)Math.Pow(input[x, y].Y, gamma);
                    float b = (float)Math.Pow(input[x, y].Z, gamma);
                    input[x, y] = new Vector3(r, g, b);
                }
            });
        }

        public static void Vignette(Vector3[,] input, float power, float strength)
        {
            float[,] vignette = GetVignette(input, power, strength);
            Parallel.For(0, input.GetLength(0), x => {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    input[x, y] *= 1f - Math.Min(vignette[x, y], 1f);
                }
            });
        }

        public static void ChromaticAbberate(Vector3[,] input, float power, float strength)
        {
            int centerX = input.GetLength(0) / 2;
            int centerY = input.GetLength(1) / 2;

            Vector3[,] result = new Vector3[input.GetLength(0), input.GetLength(1)];
            Parallel.For(0, input.GetLength(0), x => {
                float factorX = (x < centerX ? -1f : 1f) * (float)Math.Pow(((float)Math.Abs(x - centerX)) / centerX, power) * strength;
                int newXR = Math.Max(0, Math.Min(input.GetLength(0) - 1, x + (int)Math.Round(factorX)));
                int newXG = Math.Max(0, Math.Min(input.GetLength(0) - 1, x + (int)Math.Round(factorX * 0.5f)));

                for (int y = 0; y < input.GetLength(1); y++)
                {
                    float factorY = (y < centerY ? -1f : 1f) * (float)Math.Pow(((float)Math.Abs(y - centerY)) / centerY, power) * strength;
                    int newYR = Math.Max(0, Math.Min(input.GetLength(1) - 1, y + (int)Math.Round(factorY)));
                    int newYG = Math.Max(0, Math.Min(input.GetLength(1) - 1, y + (int)Math.Round(factorY * 0.5f)));

                    result[x, y] = new Vector3(input[newXR, newYR].X, input[newXG, newYG].Y, input[x, y].Z);
                }
            });
            Parallel.For(0, input.GetLength(0), x => {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    input[x, y] = result[x, y];
                }
            });
        }

        public static float[,] GetVignette(Vector3[,] input, float power, float strength)
        {
            float[,] result = new float[input.GetLength(0), input.GetLength(1)];
            int centerX = input.GetLength(0) / 2;
            int centerY = input.GetLength(1) / 2;
            float maxDistance = centerX + centerY;
            Parallel.For(0, input.GetLength(0), x => {
                int distanceX = Math.Abs(x - centerX);
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    int distanceY = Math.Abs(y - centerY);
                    result[x, y] = (float)Math.Pow(((float)(distanceX + distanceY)) / maxDistance, power) * strength;
                }
            });
            return result;
        }
    }
}
