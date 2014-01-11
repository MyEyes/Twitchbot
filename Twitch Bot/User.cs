using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    enum UserLevel
    {
        Normal=0,
        Mod=1,
        Streamer=2,
        Admin=3,
        Staff=4,
        Invalid
    }

    class User
    {
        public string Name;
        public UserLevel Level;
    }
}
