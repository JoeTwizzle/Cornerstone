using Cornerstone.Components;
using Cornerstone.Events;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Cornerstone.Systems
{
    enum LayoutDirection
    {
        RTL,
        LTR
    }
    internal class CursorSystem : IEcsRunSystem, IEcsInitSystem
    {
        [EcsInject]
        MyGame game = null!;

        [EcsWorld("Events")]
        EcsWorld Events = null!;

        [EcsFilter("Events", typeof(StartEvent))]
        EcsFilter StartEventFilter = null!;

        [EcsPool]
        EcsPool<Player> Players = null!;

        [EcsPool("Events")]
        EcsPool<StartEvent> StartEvents = null!;

        [EcsFilter(typeof(Player))]
        EcsFilter PlayerFilter = null!;
        AudioSource bgmSource = null!;
        AudioBuffer bgmBuffer = null!;
        
        bool canPlayMusic = false;
        public void Init(EcsSystems systems)
        {
            bgmBuffer = new AudioBuffer();
            bgmSource = new AudioSource();
            try
            {
                bgmBuffer.Init("SFX/Kubbi.wav");
                canPlayMusic = true;
            }
            catch
            {
                canPlayMusic = false;
            }
            if (canPlayMusic)
            {
                bgmSource.SetBuffer(bgmBuffer);
                bgmSource.SetLoop(true);
            }
            game.CursorVisible = false;
            if (canPlayMusic)
            {
                bgmSource.Play();
            }
        }
        public static bool boop = false;
        bool once = true;
        bool state = false;
        Vector2i prevCursorPos;
        public void Run(EcsSystems systems)
        {
            var c = Color4.FromHsv(new Vector4(game.Time * 0.2f % 1f, 1, 1, 1));
            var layer = game.ActiveLayer;

            layer.DrawLine(prevCursorPos, game.CursorPos, c);
            layer.DrawPixel(game.CursorPos, c);
            prevCursorPos = game.CursorPos;
        }
    }
}
