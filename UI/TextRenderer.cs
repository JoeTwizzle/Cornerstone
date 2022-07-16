using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLGraphics;
using ImageMagick;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TGELayerDraw;

namespace Cornerstone.UI
{
    public enum TextLayout
    {
        LeftAlign,
        CenterAlign,
        RightAlign,
    }
    public class TextRenderer
    {
        //const string oldfrag =
        //   @"#version 460
        //    layout(location = 0) in vec2 v_UV;

        //    layout(binding = 0) uniform sampler2D _AtlasTex;
        //    layout(location  = 0) uniform vec4 _Res;
        //    layout(binding = 1, std430) buffer _IndexBuffer
        //    {
        //    	int indices[];
        //    };
        //    layout(binding = 2, std430) buffer _UVBuffer
        //    {
        //    	vec4 uvs[];
        //    };

        //    out vec4 color;

        //    float median(float r, float g, float b) 
        //    {
        //        return max(min(r, g), min(max(r, g), b));
        //    }

        //    float remapRange(float value, float from1, float to1, float from2, float to2)
        //    {
        //        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        //    }

        //    void main()
        //    {
        //        ivec2 pos = ivec2(v_UV.x*_Res.x,v_UV.y*_Res.y);
        //        uint index = uint(pos.x+pos.y*_Res.x);
        //        vec4 coords = uvs[indices[index]];
        //        vec2 dims = vec2(coords.z - coords.x, coords.w - coords.y);
        //        vec2 percentageCell = fract(vec2(v_UV.x*_Res.x, (1-v_UV.y)*_Res.x));
        //        vec2 texCoord=vec2(coords.x, 1-coords.w) + vec2(percentageCell.x * dims.x,(1-percentageCell.y) * dims.y);
        //        vec3 msd = texture(_AtlasTex, texCoord).rgb;
        //        float sd = median(msd.r, msd.g, msd.b);
        //        float screenPxDistance = _Res.w*(sd - 0.5);
        //        float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
        //        float c = mix(0,1,opacity);
        //        color = vec4(c, c, c, 1);
        //    }";
        const string frag =
           @"#version 460
            layout(location = 1) in vec2 v_UV;
            layout(location=2) in vec4 fs_color;

            layout(binding = 0) uniform sampler2D _AtlasTex;
            layout(location  = 0) uniform float _Res;
            out vec4 color;
            
            float median(float r, float g, float b) 
            {
                return max(min(r, g), min(max(r, g), b));
            }

            float remapRange(float value, float from1, float to1, float from2, float to2)
            {
                return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            }
            
            void main()
            {
                vec4 msd = texture(_AtlasTex, vec2(v_UV.x,v_UV.y));
                float sd = median(msd.r, msd.g, msd.b);
                float screenPxDistance = _Res * (sd - 0.5);
                float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);

                color = vec4(fs_color.rgb, fs_color.a * opacity);
            }";
        public Vector2 TextScale = new Vector2(80);
        public float Sharpness = 1.8f;
        Game game;
        SpriteBatch SpriteBatch;
        public TextRenderer(Game game, string path)
        {
            this.game = game;
            SpriteBatch = new SpriteBatch(frag);
            LoadMSDF(path);
            SpriteBatch.Texture = FontAtlas;
            SpriteBatch.Begin();
        }
        Dictionary<char, int> charIndexMapping = new Dictionary<char, int>();
        struct CharInfo
        {
            public float Advance;
            public Vector4 UV;
            public Vector4 PlaneCoords;
            public Vector4 Layout;

            public CharInfo(float advance, Vector4 uV, Vector4 planeCoords, Vector4 layout)
            {
                Advance = advance;
                UV = uV;
                PlaneCoords = planeCoords;
                Layout = layout;
            }
        }
        List<CharInfo> infos = new List<CharInfo>();
        Texture2D FontAtlas = new Texture2D();



        //public void DrawChar(Vector2 pos, char c, Color4 color, TextLayout textLayout = TextLayout.LeftAlign)
        //{
        //    DrawChar(pos.X, pos.Y, c, color, textLayout);
        //}

        //public void DrawChar(float x, float y, char c, Color4 color, TextLayout textLayout = TextLayout.LeftAlign)
        //{
        //    float offset = textLayout switch
        //    {
        //        TextLayout.LeftAlign => 0,
        //        TextLayout.CenterAlign => TextScale.X / 2f,
        //        TextLayout.RightAlign => TextScale.X,
        //        _ => 0
        //    };
        //    if (charIndexMapping.TryGetValue(c, out int index))
        //    {
        //        var info = infos[index];
        //        var uv = info.UV;
        //        SpriteBatch.Draw(new Vector2(-1, -1) + Vector2.Divide(new Vector2(offset + x, y), game.GameArea), Vector2.Divide(new Vector2(1, 1), TextScale * (Width / (float)Height)), new Vector4(uv.X, 1 - uv.W, uv.Z, 1 - uv.Y), color);
        //    }
        //}


        public Vector2 GetScreenPos(Vector2i pos)
        {
            return GetScreenPos(pos.X, pos.Y);
        }

        public Vector2 GetScreenPos(int x, int y)
        {
            return new Vector2((x / (float)game.ActiveLayer.Width) * game.ClientSize.X, (y / (float)game.ActiveLayer.Height) * game.ClientSize.Y);
        }

        public void DrawText(Vector2 pos, string text, Color4 color, TextLayout textLayout = TextLayout.LeftAlign)
        {
            DrawText(pos.X, pos.Y, text, color, textLayout);
        }

        public void DrawText(Vector2 pos, ReadOnlySpan<char> text, Color4 color, TextLayout textLayout = TextLayout.LeftAlign)
        {
            DrawText(pos.X, pos.Y, text, color, textLayout);
        }

        public void DrawText(float x, float y, string text, Color4 color, TextLayout textLayout = TextLayout.LeftAlign)
        {
            DrawText(x, y, text.AsSpan(), color, textLayout);
        }

        public void DrawText(float x, float y, ReadOnlySpan<char> text, Color4 color, TextLayout textLayout = TextLayout.LeftAlign)
        {
            var chars = text;
            float length = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (charIndexMapping.TryGetValue(chars[i], out int index))
                {
                    var info = infos[index];
                    length += info.Advance * TextScale.X * 0.5f;
                }
            }
            for (int i = 0; i < chars.Length; i++)
            {
                if (charIndexMapping.TryGetValue(chars[i], out int index))
                {
                    var info = infos[index];
                    var advance = info.Advance;
                    var uv = info.UV;
                    var offsets = info.PlaneCoords;
                    float formattingOffset = textLayout switch
                    {
                        TextLayout.LeftAlign => 0,
                        TextLayout.CenterAlign => -length * 0.5f,
                        TextLayout.RightAlign => -length,
                        _ => 0
                    };
                    Vector2 charScale = new Vector2(offsets.Z - offsets.X, offsets.W - offsets.Y);
                    Vector2 pos = new Vector2(-1, -1) + Vector2.Divide(new Vector2(x + formattingOffset, y), game.GameArea) * 2f;
                    Vector2 offset = Vector2.Divide(new Vector2(offsets.X, (1 - offsets.W)) * TextScale, game.GameArea);
                    Vector2 globalScale = Vector2.Divide(new Vector2(1, 1), game.GameArea);
                    Vector2 scale = globalScale * TextScale * charScale;
                    Vector4 uvs = new Vector4(uv.X, 1 - uv.W, uv.Z, 1 - uv.Y);
                    SpriteBatch.Draw(pos + offset, scale, uvs, color);
                    x += advance * TextScale.X * 0.5f;
                }
            }
        }


        public void Display()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            SpriteBatch.GLProgram.SetUniform1(0, Sharpness);
            SpriteBatch.End();
            SpriteBatch.Display();
            SpriteBatch.Begin();
            GL.Disable(EnableCap.Blend);
        }
        void LoadMSDF(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            var extention = Path.GetExtension(path).ToLower();

            string pngPath;
            string csvPath;
            if (extention == ".png")
            {
                pngPath = path;
                csvPath = Path.Combine(string.IsNullOrWhiteSpace(dir) ? "" : dir, fileName) + ".csv";
            }
            else
            {
                csvPath = path;
                pngPath = Path.Combine(string.IsNullOrWhiteSpace(dir) ? "" : dir, fileName) + ".png";
            }
            using var img = new MagickImage(pngPath);
            int channels = img.ChannelCount;
            FontAtlas.Init(img.Width, img.Height);
            FontAtlas.Filter = GLGraphics.TextureFilter.Bilinear;
            var pixels = img.GetPixelsUnsafe();
            FontAtlas.SetImage(pixels.GetAreaPointer(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, PixelFormat.Rgba, PixelType.UnsignedByte);
            FontAtlas.WrapModeU = TextureWrapMode.ClampToBorder;
            FontAtlas.WrapModeV = TextureWrapMode.ClampToBorder;
            var layouts = File.ReadAllLines(csvPath);
            var culture = CultureInfo.InvariantCulture;
            int i = 0;
            foreach (var line in layouts)
            {
                var array = line.Split(',');
                if (array.Length != 10)
                {
                    throw new FormatException("Invalid csv format");
                }
                char c = (char)int.Parse(array[0], culture);
                float advance = float.Parse(array[1], culture);

                //idk
                float planeBoundsL = float.Parse(array[2], culture);
                float planeBoundsB = float.Parse(array[3], culture);
                float planeBoundsR = float.Parse(array[4], culture);
                float planeBoundsT = float.Parse(array[5], culture);

                float atlasBoundsL = float.Parse(array[6], culture) / img.Width;
                float atlasBoundsB = float.Parse(array[7], culture) / img.Height;
                float atlasBoundsR = float.Parse(array[8], culture) / img.Width;
                float atlasBoundsT = float.Parse(array[9], culture) / img.Height;
                infos.Add(new CharInfo(advance, new Vector4(atlasBoundsL, atlasBoundsB, atlasBoundsR, atlasBoundsT), new Vector4(planeBoundsL, planeBoundsB, planeBoundsR, planeBoundsT), new Vector4(planeBoundsL, planeBoundsB, planeBoundsR, planeBoundsT)));
                charIndexMapping.Add(c, i++);
            }
        }
    }
}
