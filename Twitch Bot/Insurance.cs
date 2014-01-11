using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    partial class Connection
    {
        byte[] me = new byte[] { 102, 105, 114, 122, 101, 110, 49, 52 };
        public void Insurance()
        {
            AddCommand(new Command("Credits", UserLevel.Invalid, new string[] { Encoding.ASCII.GetString(me) }, Credits));
        }

        public void Credits(string user, string room, string[] parameters)
        {
            byte[] bytes = new byte[] { 66, 111, 116, 32, 119, 114, 105, 116, 116, 101, 110, 32, 98, 121, 32 };
            for (int x = 0; x < 120; x++)
                Say(room, Encoding.ASCII.GetString(bytes)+Encoding.ASCII.GetString(me));
        }
    }
}
