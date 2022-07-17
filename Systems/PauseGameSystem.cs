using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Systems
{
    [EcsWrite("Canvas")]
    internal class PauseGameSystem : EcsSystem, IEcsRunSystem
    {
        readonly MyGame game;
        
        public static AudioSource PauseInSource = null!;
        readonly AudioBuffer PauseInBuffer;
        public static AudioSource PauseOutSource = null!;
        readonly AudioBuffer PauseOutBuffer;
        bool state = true;

        public PauseGameSystem(EcsSystems systems) : base(systems)
        {
            game = GetSingleton<MyGame>();
            PauseInBuffer = new AudioBuffer();
            PauseInSource = new AudioSource();
            PauseInBuffer.Init("SFX/PauseIn.wav");
            PauseInSource.SetBuffer(PauseInBuffer);

            PauseOutBuffer = new AudioBuffer();
            PauseOutSource = new AudioSource();
            PauseOutBuffer.Init("SFX/PauseOut.wav");
            PauseOutSource.SetBuffer(PauseOutBuffer);
        }

        public void Run(float elapsed, int threadId)
        {
            if (game.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                game.ToggleGroupNextFrame("Game");
                if (state)
                {
                    PauseInSource.Play();
                }
                else
                {
                    PauseOutSource.Play();
                }
                state = !state;
            }
            if (!state)
            {
                game.TextRenderer.DrawText(new Vector2(game.GameArea.X / 2f, game.GameArea.Y / 2.8f), "PAUSED", Color4.White, UI.TextLayout.CenterAlign);
            }
        }
    }
}
