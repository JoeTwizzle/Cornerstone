global using Leopotam.EcsLite.ExtendedSystems;
global using Leopotam.EcsLite.Di;
global using Leopotam.EcsLite;
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
        public AudioManager AudioManager = null!;
        //Debug
        byte[] dataA = null!;
        byte[] dataB = null!;
        int iconW = 128;
        int iconH => iconW;
        public string GPUVendor = null!;
        public string GPUVersion = null!;
        //ECS
        EcsFilter statesFilter = null!;
        EcsPool<EcsGroupSystemState> groupPool = null!;
        public const string EventWorldName = "Events";
        List<(string, bool)> groupEvents = new List<(string, bool)>();
        EcsWorld world = null!;
        EcsWorld events = null!;
        public EcsSystems Systems = null!;
        //Game
        public ulong Score = 0;
        public TextRenderer TextRenderer = null!;
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
            world = new EcsWorld();
            Systems = new EcsSystems(world);
            Systems.AddWorld(events, "Events");
            Systems.Add(new ClearActiveLayerSystem());
            Systems.AddGroupInject("Intro", true, EventWorldName, this,
                new IntroSystem());
            Systems.AddGroupInject("Menu", false, EventWorldName, this,
                new ParticleSystem(),
                new MainMenuSystem());
            Systems.AddGroupInject("GameCore", false, EventWorldName, this,
                new DrawBGSystem());
            Systems.AddGroupInject("Game", false, EventWorldName, this,
                new EnemySpawnSystem(),
                new Enemy1BehaviorSystem(),
                new Enemy2BehaviorSystem(),
                new Enemy3BehaviorSystem(),
                new ExplosionSystem(),
                new TransformSystem(),
                new LifetimeSystem(),
                new AnimationSystem(),
                new BulletSystem(),
                new PlayerSystem());
            Systems.AddGroupInject("GameCore", false, EventWorldName, this,
                new ReflectionSystem());
            Systems.AddGroupInject("Shop", false, EventWorldName, this,
                new ShopSystem());
            Systems.AddGroupInject("Pauseable", false, EventWorldName, this,
                new PauseGameSystem(),
                new DrawHudSystem());
            Systems.Add(new CursorSystem());
            Systems.Add(new DisplayTextSystem());
            Systems.Inject(this);
            Systems.Init();
            int entity = events.NewEntity();
            events.GetPool<IntroEvent>().Add(entity).Entering = true;
            groupPool = events.GetPool<EcsGroupSystemState>();
            statesFilter = events.Filter<EcsGroupSystemState>().End();
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
            for (int i = 0; i < groupEvents.Count; i++)
            {
                foreach (var item in statesFilter)
                {
                    ref var group = ref groupPool.Get(item);
                    if (group.Name == groupEvents[i].Item1)
                    {
                        group.State = groupEvents[i].Item2;
                    }
                }
            }
            groupEvents.Clear();
            Systems.Run();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            AudioManager.Dispose();
        }

        public void EnableGroup(string name)
        {
            foreach (var ent in statesFilter)
            {
                ref var group = ref groupPool.Get(ent);
                if (group.Name == name)
                {
                    group.State = true;
                }
            }
        }

        public void DisableGroup(string name)
        {
            foreach (var item in statesFilter)
            {
                ref var group = ref groupPool.Get(item);
                if (group.Name == name)
                {
                    group.State = false;
                }
            }
        }

        public void ToggleGroup(string name)
        {
            foreach (var item in statesFilter)
            {
                ref var group = ref groupPool.Get(item);
                if (group.Name == name)
                {
                    group.State = !group.State;
                }
            }
        }

        public void EnableGroupNextFrame(string name)
        {
            groupEvents.Add((name, true));
        }

        public void DisableGroupNextFrame(string name)
        {
            groupEvents.Add((name, false));
        }

        public void ToggleGroupNextFrame(string name)
        {
            bool state = false;
            foreach (var item in statesFilter)
            {
                ref var group = ref groupPool.Get(item);
                if (group.Name == name)
                {
                    state = group.State;
                }
            }
            groupEvents.Add((name, !state));
        }
    }
}
