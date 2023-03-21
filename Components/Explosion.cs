using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Components
{
    struct Explosion : IComponent
    {
        public Team team;
        public float Size;
        public float Duration;
        public float Time;
    }
}
