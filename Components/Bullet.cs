using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Components
{
    public enum Team : byte
    {
        Player,
        Enemy
    }

    [Flags]
    public enum BulletType : byte
    {
        Normal = 1,
        Explosive = 2,
        Penetrating = 4
    }

    public struct Bullet
    {
        public Vector2 Position;
        public Vector2 PrevPosition;
        public Vector2 Velocity;
        public float LifeTime;
        public bool HasGravity;
        public float Damage;
        public BulletType BulletType;
        public Team Team;
        public Color4 Color;
    }
}
