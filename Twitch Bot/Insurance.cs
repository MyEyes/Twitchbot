using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    partial class Connection
    {
        public void Insurance()
        {
            AddCommand(new Command("Credits", UserLevel.Invalid, new string[] { "firzen14" }, Credits));
        }

        public void Credits(string user, string room, string[] parameters)
        {
            Say(room, "Bot written by Firzen14");
        }
    }
}
