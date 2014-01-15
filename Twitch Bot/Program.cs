using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot b = new Bot();
            b.Connect();
            string command = "";
            while ((command = Console.ReadLine()) != "exit")
            {
                string[] split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Message m = new Message(MessageType.PRIVMSG, split);
                m.SenderName = "firzen14";
                b.Handle(m);
            }
            b.Exit();
        }
    }
}
