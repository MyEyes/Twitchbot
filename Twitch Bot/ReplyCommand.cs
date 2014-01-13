using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    class ReplyCommand:Command
    {
        public string room = "";
        public string message = "";
        public ReplyCommand(string name,string room, string message)
            : base(name, UserLevel.Normal, new string[] { }, delegate(string a, string b, string[] c) { if(b==room || room=="global") Connection.CurrentConnection.Say(b, message); }, true)
        {
            this.room = room;
            this.message = message;
        }
    }
}
