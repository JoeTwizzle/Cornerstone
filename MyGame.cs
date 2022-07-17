global using TGELayerDraw;
global using ALAudio;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Cornerstone.Systems;
using Cornerstone.Events;
using Cornerstone.UI;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using Cornerstone.Components;

namespace Cornerstone
{
    class MyGame : Game
    {
        public MyGame(Vector2i size) : base(size)
        {

        }

        public MyGame(int sizeX, int sizeY) : base(sizeX, sizeY)
        {

        }
        //Audio
        public AudioManager AudioManager;
        //Debug
        byte[] dataA;
        byte[] dataB;
        int iconW = 128;
        int iconH => iconW;
        public string GPUVendor;
        public string GPUVersion;
        //ECS
        public const string EventWorldName = "Events";
        EcsWorld world;
        EcsWorld events;
        public EcsSystems Systems;
        //Game
        public ulong Score = 0;
        public TextRenderer TextRenderer;
        float time = 0;
        public float Time => time;
        float deltaTime;
        public float DeltaTime => deltaTime;


        protected override void OnLoad()
        {
            TextRenderer = new TextRenderer(this, @"consoletext.png");
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
            events.AllowPool<IntroEvent>();
            events.AllowPool<MainMenuEvent>();
            events.AllowPool<ResetGameEvent>();
            events.AllowPool<ShopEvent>();
            events.AllowPool<StartEvent>();

            world = new EcsWorld();
            world.AllowPool<Player>();
            world.AllowPool<Transform>();
            world.AllowPool<Bullet>();
            world.AllowPool<Enemy>();
            world.AllowPool<Enemy1>();
            world.AllowPool<Enemy2>();
            world.AllowPool<Enemy3>();
            world.AllowPool<Explosion>();
            world.AllowPool<Lifetime>();
            world.AllowPool<SpriteAnimation>();
            var builder = new EcsSystemsBuilder(world);
            builder.AddWorld(events, "Events");
            builder.ClearGroup();
            builder.InjectSingleton(this); 
            builder.Add<ClearActiveLayerSystem>();
            builder.SetGroup("Intro");
            builder.Add<IntroSystem>();
            builder.SetGroup("Menu", false);
            builder.Add<ParticleSystem>();
            builder.Add<MainMenuSystem>();
            builder.SetGroup("GameCore", false);
            builder.Add<DrawBGSystem>();
            builder.SetGroup("Game", false);
            builder.Add<EnemySpawnSystem>();
            builder.Add<Enemy1BehaviorSystem>();
            builder.Add<Enemy2BehaviorSystem>();
            builder.Add<Enemy3BehaviorSystem>();
            builder.Add<ExplosionSystem>();
            builder.Add<TransformSystem>();
            builder.Add<LifetimeSystem>();
            builder.Add<AnimationSystem>();
            builder.Add<BulletSystem>();
            builder.Add<PlayerSystem>();
            builder.SetGroup("GameCore", false);
            builder.Add<ReflectionSystem>();
            builder.SetGroup("Shop", false);
            builder.Add<ShopSystem>();
            builder.SetGroup("Pauseable", false);
            builder.Add<PauseGameSystem>();
            builder.Add<DrawHudSystem>();
            builder.ClearGroup();
            builder.Add<CursorSystem>();
            //builder.Add<DisplayTextSystem>();
            Systems = builder.Finish(2);
            Systems.Init();
            int entity = events.NewEntity();
            events.GetPool<IntroEvent>().Add(entity).Entering = true;
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
            Systems.Run(dt);
            ActiveLayer.UpdateVisual();
            DisplaySprite(ActiveLayer);
            //TextRenderer.DrawText(Vector2.Zero, "" + dt * 1000 + "ms", Color4.White);
            TextRenderer.Display();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            AudioManager.Dispose();
        }

        public void EnableGroupNextFrame(string name)
        {
            Systems.EnableGroupNextFrame(name);
        }

        public void DisableGroupNextFrame(string name)
        {
            Systems.DisableGroupNextFrame(name);
        }

        public void ToggleGroupNextFrame(string name)
        {
            Systems.ToggleGroupNextFrame(name);
        }
    }
}
