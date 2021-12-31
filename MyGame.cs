using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Leopotam.EcsLite;
using ALAudio;
using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Di;
using TGELayerDraw;
using Cornerstone.Systems;
using Cornerstone.Events;
using Cornerstone.UI;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;

namespace Cornerstone
{
    class MyGame : Game
    {
        public MyGame(Vector2i size) : base(size)
        {
            CreateSystems();
        }

        public MyGame(int sizeX, int sizeY) : base(sizeX, sizeY)
        {
            CreateSystems();
        }
        public AudioManager AudioManager;
        public ulong Score = 0;

        byte[] dataA = null!;
        byte[] dataB = null!;
        int iconW = 128;
        int iconH => iconW;
        public string GPUVendor;
        public string GPUVersion;
        void CreateSystems()
        {
            GPUVendor = GL.GetString(StringName.Vendor);
            GPUVersion = GL.GetString(StringName.Version);
            using var icon = new ImageMagick.MagickImage("SpriteSheets/Boss-1.png");
            icon.InterpolativeResize(iconW * 2, iconH, ImageMagick.PixelInterpolateMethod.Nearest);
            icon.Page.Y += 64;
            var pixels = icon.GetPixelsUnsafe();
            dataA = pixels.ToByteArray(0, 0, iconW, iconH, ImageMagick.PixelMapping.RGBA)!;
            dataB = pixels.ToByteArray(iconW, 0, iconW, iconH, ImageMagick.PixelMapping.RGBA)!;
            Icon = new OpenTK.Windowing.Common.Input.WindowIcon(new OpenTK.Windowing.Common.Input.Image(iconW, iconH, dataA));
            Title = "Resolution";
            AudioManager = new AudioManager();
            PixelSize = new Vector2i(7, 7);
            SetViewport(0, 0, 128 * PixelSize.X, 128 * PixelSize.Y);
            FitWindow();
            events = new EcsWorld();
            world = new EcsWorld();
            Systems = new EcsSystems(world);
            Systems.AddWorld(events, "Events");
            Systems.Add(new ClearActiveLayerSystem());
            Systems.Add(new IntroSystem());
            Systems.Add(new ParticleSystem());
            Systems.Add(new MainMenuSystem());
            Systems.Add(new GameSystem());
            Systems.Add(new EnemySpawnSystem());
            Systems.Add(new Enemy1BehaviorSystem());
            Systems.Add(new Enemy2BehaviorSystem());
            Systems.Add(new Enemy3BehaviorSystem());
            Systems.Add(new ExplosionSystem());
            Systems.Add(new TransformSystem());
            Systems.Add(new LifetimeSystem());
            Systems.Add(new AnimationSystem());
            Systems.Add(new BulletSystem());
            Systems.Add(new PlayerSystem());
            Systems.Add(new ReflectionSystem());
            Systems.Add(new ShopSystem());
            Systems.Add(new CursorSystem());
            Systems.Add(new DisplayTextSystem());
            Systems.Inject(this);
            Systems.Init();

            int entity = events.NewEntity();
            events.GetPool<IntroEvent>().Add(entity).Entering = true;
        }
        EcsWorld world;
        EcsWorld events;
        public EcsSystems Systems;
        public TextRenderer textRenderer = null!;
        float time = 0;
        public float Time => time;
        float deltaTime;
        public float DeltaTime => deltaTime;


        protected override void OnLoad()
        {
            textRenderer = new TextRenderer(this, @"consoletext.png");
        }
        bool side = false;
        float anim = 0;
        public override void Update(float dt)
        {
            anim += dt;
            if (anim > 0.15f)
            {
                anim = 0;
                side = !side;
                if (side)
                {
                    Icon = new OpenTK.Windowing.Common.Input.WindowIcon(new OpenTK.Windowing.Common.Input.Image(iconW, iconH, dataA));
                }
                else
                {
                    Icon = new OpenTK.Windowing.Common.Input.WindowIcon(new OpenTK.Windowing.Common.Input.Image(iconW, iconH, dataB));
                }
            }
            if (KeyboardState.IsKeyPressed(Keys.F11))
            {
                if (WindowState == WindowState.Fullscreen)
                {
                    WindowState = WindowState.Normal;
                    PixelSize = new Vector2i(7, 7);
                    SetViewport(0, 0, 128 * PixelSize.X, 128 * PixelSize.Y);
                    FitWindow();
                }
                else
                {

                    WindowState = WindowState.Fullscreen;
                    int scaleX = ClientSize.X / 128;
                    int scaleY = ClientSize.Y / 128;
                    int scale = Math.Max(Math.Min(scaleX, scaleY), 1);
                    int w = 128 * scale;
                    int h = 128 * scale;
                    int dx = ClientSize.X - w;
                    int dy = ClientSize.Y - h;
                    PixelSize = new Vector2i(scale, scale);
                    SetViewport(dx / 2, dy / 2, 128 * scale, 128 * scale);
                }

            }
            deltaTime = dt;
            time += dt;
            Systems.Run();
        }
        protected override void OnClosed()
        {
            base.OnClosed();
            AudioManager.Dispose();
        }
    }
}
