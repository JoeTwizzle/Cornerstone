using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Components
{
    struct Enemy : IComponent, IEcsInit<Enemy>
    {
        public Box2i HitBox;
        public int HP;
        public float TimeToNextShot;

        public static void OnInit(ref Enemy c)
        {
            c.HP = 1;
            c.HitBox = new Box2i(-3, -3, 3, 3);
            c.TimeToNextShot = 1f / 3f;
        }
    }
}
