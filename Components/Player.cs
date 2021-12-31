using Leopotam.EcsLite;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Components
{
    public enum PlayerState
    {
        Idle,
        Normal,
        Stunned,
        Crouching
    }

    public struct Player : IEcsAutoReset<Player>
    {
        public int Armor;
        public int ShootLevel;
        public int MoveLevel;
        public int JumpLevel;
        public Vector2 Position;
        public Vector2 Velocity;
        public int HP;
        public float InvincibleTimer;
        public float TimeSinceLastShot;
        public float FireRate;
        public PlayerState PlayerState;

        public void AutoReset(ref Player c, EcsPool<Player> pool, int entity)
        {
            c.Armor = 0;
            c.ShootLevel = 0;
            c.MoveLevel = 0;
            c.JumpLevel = 0;
            c.HP = 10;
            c.InvincibleTimer = 0.5f;
            c.FireRate = 1f / 3f;
            c.TimeSinceLastShot = c.FireRate;
            c.Position = new Vector2(128 / 2);
            c.Velocity = Vector2.Zero;
            c.PlayerState = PlayerState.Idle;
        }
    }
}
