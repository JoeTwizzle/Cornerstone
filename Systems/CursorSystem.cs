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
    [EcsWrite("Canvas")]
    internal class CursorSystem : EcsSystem, IEcsRunSystem
    {
        MyGame game;

        AudioSource bgmSource;
        AudioBuffer bgmBuffer;

        bool canPlayMusic = false;
        public static bool boop = false;
        Vector2i prevCursorPos;

        public CursorSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
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

        public void Run(EcsSystems systems, float elapsed, int threadId)
        {
            var c = Color4.FromHsv(new Vector4(game.Time * 0.2f % 1f, 1, 1, 1));
            var layer = game.ActiveLayer;

            layer.DrawLine(prevCursorPos, game.CursorPos, c);
            layer.DrawPixel(game.CursorPos, c);
            prevCursorPos = game.CursorPos;
        }
    }
}
