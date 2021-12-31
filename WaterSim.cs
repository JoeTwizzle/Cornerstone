using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TGELayerDraw;

namespace Cornerstone
{
    class WaterSim : DrawableBuffer<float>
    {
        float[] domainBufferA;
        float persistance = 0.4f;
        public WaterSim(int x, int y)
        {
            width = x;
            height = y;
            domainBufferA = new float[x * y];
            Data = new float[x * y];
        }

        public void Clear()
        {
            Clear(0f);
            SwapBuffers();
            Clear(0f);
            SwapBuffers();
        }

        public void SetWater(Vector2i p0, float value)
        {
            SetWater(p0.X, p0.Y, value);
        }

        public void SetWater(int x0, int y0, float value)
        {
            DrawPixel(x0, y0, value, BlendMode.None);
        }

        public void SetWater(Vector2i p0, Vector2i p1, float value)
        {
            SetWater(p0.X, p0.Y, p1.X, p1.Y, value);
        }

        public void SetWater(int x0, int y0, int x1, int y1, float value)
        {
            DrawLine(x0, y0, x1, y1, value, BlendMode.None);
        }

        void SwapBuffers()
        {
            var temp = Data;
            Data = domainBufferA;
            domainBufferA = temp;
        }

        public void Update(in float dt)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Data[x + y * width] = (
                        (domainBufferA[Math.Max(x - 1, 0) + y * width]
                        + domainBufferA[Math.Min(x + 1, width - 1) + y * width]
                        + domainBufferA[x + Math.Min(y + 1, height - 1) * width]
                        + domainBufferA[x + Math.Max(y - 1, 0) * width]) / 2
                        - Data[x + y * width] ) * persistance;
                }
            }
            SwapBuffers();
        }

        public void Draw(Layer layer)
        {
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    float color = MathHelper.Lerp(184f / 360f, 240f / 360f, domainBufferA[x + y * width] / 255f);
                    layer.DrawPixel(x, y, Color4.FromHsv(new Vector4(color, 1, 1, MathHelper.Clamp((domainBufferA[x + y * width] * domainBufferA[x + y * width]) / 255f, 0f, 1f))), BlendMode.Alpha, false);
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void DrawPixelChecked(in int x, in int y, in float color)
        {
            int pos = x + y * width;
            if (x >= 0 && x < width && pos >= 0 && pos < Data.Length)
            {
                Data[pos] = color;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void DrawPixelUnchecked(in int x, in int y, in float color)
        {
            Data[x + y * width] = color;
        }
        public override void DrawPixelCheckedAdd(in int x, in int y, in float color)
        {
            throw new NotImplementedException();
        }
        public override void DrawPixelUncheckedAdd(in int x, in int y, in float color)
        {
            throw new NotImplementedException();
        }
        public override void DrawPixelCheckedClip(in int x, in int y, in float color)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixelCheckedAlpha(in int x, in int y, in float srcColor)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixelUncheckedClip(in int x, in int y, in float color)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixelUncheckedAlpha(in int x, in int y, in float srcColor)
        {
            throw new NotImplementedException();
        }
    }
}
