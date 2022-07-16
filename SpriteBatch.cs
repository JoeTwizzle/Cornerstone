using Cornerstone.UI;
using GLGraphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TGELayerDraw;

namespace Cornerstone
{
    public class SpriteBatch
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Vertex
        {
            public Vector2 pos;
            public Vector2 uv;
            public Color4 color;

            public Vertex(Vector2 pos, Vector2 uv, Color4 color)
            {
                this.pos = pos;
                this.uv = uv;
                this.color = color;
            }
        }
        const string vert = @"#version 450
        layout(location=0) in vec2 vs_pos;
        layout(location=1) in vec2 vs_uv;
        layout(location=2) in vec4 vs_color;

        layout(location=0) out vec2 fs_pos;
        layout(location=1) out vec2 fs_uv;
        layout(location=2) out vec4 fs_color;
        void main()
        {
            fs_color = vs_color;
            fs_pos = vs_pos;
            fs_uv = vs_uv;
            gl_Position = vec4(vs_pos.x,-vs_pos.y, 0, 1);
        }";
        const string frag = @"#version 450
        layout(location=0) in vec2 fs_pos;
        layout(location=1) in vec2 fs_uv;
        layout(location=2) in vec4 fs_color;

        layout(binding=0) uniform sampler2D _MainTex;
        layout(location=0) out vec4 color;
        void main()
        {
            color = vec4(texture(_MainTex, fs_uv)) * fs_color;
        }";

        public GLProgram GLProgram;
        VertexArray vao;
        GLBuffer VertexBuffer;
        GLBuffer IndexBuffer;
        public bool IsRunning { get; private set; }
        public Texture2D? Texture = null;

        int maxSpritesInBatch;
        int spriteCount = 0;

        int[] indices;
        Vertex[] vertices;
        public SpriteBatch()
        {
            CreateShader(vert, frag);
        }
        public SpriteBatch(string frag)
        {
            CreateShader(vert, frag);
        }
        public SpriteBatch(string vert, string frag)
        {
            CreateShader(vert, frag);
        }
        void CreateShader(string vert, string frag)
        {
            GLProgram = new GLProgram();
            var vertexShader = new GLShader(ShaderType.VertexShader);
            var fragmentShader = new GLShader(ShaderType.FragmentShader);
            vertexShader.SetSource(vert);
            fragmentShader.SetSource(frag);
            GLProgram.AddShader(vertexShader);
            GLProgram.AddShader(fragmentShader);
            GLProgram.LinkProgram();
            vertexShader.Dispose();
            fragmentShader.Dispose();
            vao = new VertexArray();
            vao.SetIndex(0, 2, VertexAttribType.Float, 0);
            vao.SetIndex(1, 2, VertexAttribType.Float, 2 * sizeof(float));
            vao.SetIndex(2, 4, VertexAttribType.Float, 4 * sizeof(float));
            vao.SetStride(8 * sizeof(float));
            MaxSpritesInBatch = 1024;
        }



        public int MaxSpritesInBatch
        {
            get => maxSpritesInBatch; set
            {
                if (IsRunning)
                {
                    throw new Exception("Cannot change MaxSpritesInBatch while spritebatch is running");
                }
                maxSpritesInBatch = value;
                indices = new int[maxSpritesInBatch * 6];
                vertices = new Vertex[maxSpritesInBatch * 4];
                VertexBuffer?.Dispose();
                VertexBuffer = new GLBuffer();
                VertexBuffer.Init(BufferType.ArrayBuffer, Unsafe.SizeOf<Vertex>(), maxSpritesInBatch * 4);
                IndexBuffer?.Dispose();
                IndexBuffer = new GLBuffer();
                IndexBuffer.Init(BufferType.ElementArrayBuffer, sizeof(uint), maxSpritesInBatch * 6);
            }
        }

        public void Begin()
        {
            if (IsRunning)
            {
                throw new Exception("Cannot call Begin() while spritebatch is still running");
            }
            spriteCount = 0;
            IsRunning = true;
        }

        void UpdateBuffers()
        {
            VertexBuffer.UpdateData(vertices);
            IndexBuffer.UpdateData(indices);
        }


        public void Draw(Vector2 pos, Vector2 size, Vector4 uvs, Color4 color)
        {
            if (spriteCount >= maxSpritesInBatch)
            {
                return;
            }

            vertices[spriteCount * 4 + 0] = new Vertex(pos, new Vector2(uvs.X, uvs.Y), color);
            vertices[spriteCount * 4 + 1] = new Vertex(new Vector2(pos.X + size.X, pos.Y), new Vector2(uvs.Z, uvs.Y), color);
            vertices[spriteCount * 4 + 2] = new Vertex(new Vector2(pos.X + size.X, pos.Y + size.Y), new Vector2(uvs.Z, uvs.W), color);
            vertices[spriteCount * 4 + 3] = new Vertex(new Vector2(pos.X, pos.Y + size.Y), new Vector2(uvs.X, uvs.W), color);
            indices[spriteCount * 6 + 0] = spriteCount * 4 + 0;
            indices[spriteCount * 6 + 1] = spriteCount * 4 + 1;
            indices[spriteCount * 6 + 2] = spriteCount * 4 + 3;
            indices[spriteCount * 6 + 3] = spriteCount * 4 + 1;
            indices[spriteCount * 6 + 4] = spriteCount * 4 + 2;
            indices[spriteCount * 6 + 5] = spriteCount * 4 + 3;
            spriteCount++;
        }
        public void End()
        {
            if (!IsRunning)
            {
                throw new Exception("Cannot call End() while spritebatch is not running");
            }
            UpdateBuffers();
            IsRunning = false;
        }
        public void Display()
        {
            if (IsRunning)
            {
                throw new Exception("Cannot call Display() while spritebatch is still running");
            }
            if (Texture != null)
            {
                vao.SetIndexBuffer(IndexBuffer);
                vao.SetVertexBuffer(VertexBuffer);
                vao.Bind();
                GLProgram.Bind();
                Texture.Bind();
                GL.DrawElements(PrimitiveType.Triangles, spriteCount * 6, DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}
