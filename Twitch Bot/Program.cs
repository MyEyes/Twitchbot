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
            Connection c = new Connection();
            c.ReadUser("user.txt");
            c.Connect();
            string command = "";
            while ((command = Console.ReadLine()) != "exit")
            {
                string[] split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Message m = new Message(MessageType.PRIVMSG, split);
                m.SenderName = "firzen14";
                c.Handle(m);
            }
            c.Exit();
        }
    }
}
