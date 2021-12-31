using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGELayerDraw;

namespace Cornerstone.UI
{
    public struct Panel
    {
        public int PosX;
        public int PosY;
        public int Width;
        public int Height;
        public Color4 BackColor;
        public Color4 OutlineColor;

        public static Panel FromCenter(int posX, int posY, int width, int height, Color4 backColor, Color4 outlineColor)
        {
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            int nposX = posX - halfWidth;
            int nposY = posY - halfHeight;
            Panel panel = new Panel();
            panel.PosX = nposX;
            panel.PosY = nposY;
            panel.Width = width;
            panel.Height = height;
            panel.BackColor = backColor;
            panel.OutlineColor = outlineColor;
            return panel;
        }

        public Panel(int posX, int posY, int width, int height, Color4 backColor, Color4 outlineColor)
        {
            PosX = posX;
            PosY = posY;
            Width = width;
            Height = height;
            BackColor = backColor;
            OutlineColor = outlineColor;
        }

        public bool IsInside(Vector2i pos)
        {
            return IsInside(pos.X, pos.Y);
        }

        public bool IsInside(int x, int y)
        {
            return x >= PosX && x < PosX + Width && y >= PosY && y < PosY + Height;
        }

        public void Draw(Layer layer, BlendMode blendMode = BlendMode.None)
        {
            layer.FillBox(PosX, PosY, Width, Height, BackColor, blendMode);
            layer.DrawBox(PosX, PosY, Width, Height, OutlineColor, blendMode);
        }
    }
}
