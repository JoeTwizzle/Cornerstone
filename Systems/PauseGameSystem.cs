using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Systems
{
    internal class PauseGameSystem : IEcsRunSystem, IEcsInitSystem
    {
        [EcsInject]
        MyGame game = null!;

        public static AudioSource PauseInSource = null!;
        AudioBuffer PauseInBuffer = null!;
        public static AudioSource PauseOutSource = null!;
        AudioBuffer PauseOutBuffer = null!;
        public void Init(EcsSystems systems)
        {

            PauseInBuffer = new AudioBuffer();
            PauseInSource = new AudioSource();
            PauseInBuffer.Init("SFX/PauseIn.wav");
            PauseInSource.SetBuffer(PauseInBuffer);

            PauseOutBuffer = new AudioBuffer();
            PauseOutSource = new AudioSource();
            PauseOutBuffer.Init("SFX/PauseOut.wav");
            PauseOutSource.SetBuffer(PauseOutBuffer);
        }

        bool state = true;
        public void Run(EcsSystems systems)
        {
            if (game.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                game.ToggleGroup("Game");
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
