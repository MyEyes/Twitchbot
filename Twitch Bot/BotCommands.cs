/********************************************************************************
* The MIT License (MIT)
*
* Copyright (c) 2013 Nils Ole Timm
*
* Permission is hereby granted, free of charge, to any person obtaining a copy of
* this software and associated documentation files (the "Software"), to deal in
* the Software without restriction, including without limitation the rights to
* use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
* the Software, and to permit persons to whom the Software is furnished to do so,
* subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
* FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
* COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
* IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
* CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*********************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    //Setting up commands for the bot
    partial class Bot
    {

        public void SetUpCommands()
        {
            AddCommand(new Command("test", UserLevel.Mod, new string[] { }, Test));
            AddCommand(new Command("join", UserLevel.Admin, new string[] { SuperAdminName }, Join));
            AddCommand(new Command("part", UserLevel.Mod, new string[] { SuperAdminName }, Part));
            AddCommand(new Command("Klappa", UserLevel.Normal, new string[] { }, Klappa));
            AddCommand(new Command("Slap", UserLevel.Normal, new string[] { }, Slap));
            AddCommand(new Command("Insult", UserLevel.Normal, new string[] { }, Insult));
            AddCommand(new Command("Count", UserLevel.Mod, new string[] { }, MessageCount));
            AddCommand(new Command("Info", UserLevel.Normal, new string[] { }, Version));
            AddCommand(new Command("AddReply", UserLevel.Mod, new string[] { SuperAdminName }, AddReply));
            AddCommand(new Command("DelReply", UserLevel.Mod, new string[] { SuperAdminName }, DelReply));
            AddCommand(new Command("Shutdown", UserLevel.Invalid, new string[] { SuperAdminName }, delegate(string a, string b, string[] c) { Exit(); }));
            AddCommand(new Command("Exit", UserLevel.Invalid, new string[] { SuperAdminName }, delegate(string a, string b, string[] c) { Exit(); }));
            AddCommand(new Command("Mirror", UserLevel.Invalid, new string[] { SuperAdminName }, Mirror));
        }

        string mirrorRoom = "";

        public void Mirror(string user, string room, string[] parameters)
        {
            mirrorRoom = room;
        }

        public void AddReply(string user, string room, string[] parameters)
        {
            if (parameters.Length < 2)
                return;
            string name = parameters[0];
            string message = "";
            for (int x = 1; x < parameters.Length; x++)
            {
                message += parameters[x];
                if (x != parameters.Length - 1)
                    message += " ";
            }


            for (int x = 0; x < commands.Count; x++)
            {
                ReplyCommand testcmd = commands[x] as ReplyCommand;
                if (testcmd != null)
                {
                    if (testcmd.Name == name && (user == SuperAdminName || room == testcmd.room))
                    {
                        Say(room, "Command already exists!");
                        return;
                    }
                }
            }

            ReplyCommand cmd = null;
            if (user == SuperAdminName)
                cmd = new ReplyCommand(name, "global", message);
            else
                cmd = new ReplyCommand(name, room, message);
            AddCommand(cmd);
            Say(room, "Added command !" + cmd.Name + " to " + cmd.room);
        }

        public void DelReply(string user, string room, string[] parameters)
        {
            if (parameters.Length < 1)
                return;
            string name = parameters[0];

            for (int x = 0; x < commands.Count; x++)
            {
                ReplyCommand cmd = commands[x] as ReplyCommand;
                if (cmd != null)
                {
                    if (cmd.Name == name && (user == SuperAdminName || room == cmd.room))
                    {
                        Say(room, "Removed command !" + cmd.Name + " from " + cmd.room);
                        commands.RemoveAt(x);
                        return;
                    }
                }
            }
            Say(room, "Command " + name + " does not exist!");
        }

        public void Version(string user, string room, string[] parameters)
        {
            Say(room, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " by Firzen14 running on " + System.Environment.MachineName);
        }

        public void MessageCount(string user, string room, string[] parameters)
        {
            Room Room = GetRoom(room);
            if (Room != null)
                Say(room, "Since I joined there have been " + Room.Messages.ToString() + " messages in this room.");
        }

        public void Slap(string user, string room, string[] parameters)
        {
            if (parameters.Length < 1)
                return;
            Say(room, "In the name of " + user + " TAKE THIS!");
            Say(room, "/me slaps " + parameters[0]);
        }

        public void Insult(string user, string room, string[] parameters)
        {
            string[] Insults = new string[] { "chinchilla", "bread", "<insert insult here>", "flux capacitor(tm)", "not nice person", "RejectedLink", "purse", "point of mass", "wonderful chap", "person that visits countries in south-east asia for sex", "sexless panda", "monkey dick", "ship", "penguin", 
            "celebratory christmas poop", "morally bankrupt sellout", "TriHard", "lady who looks like this OpieOP", "penis", "pinata", "rotationally invariant 3d object", "cheese", "CHEESE", "worry for the government, because you are so thick you might collapse into a black hole",
             "horror to behold", "sad excuse for a human being", "shoo in to die a virgin", "trite little so-and-so", "nasty little tart", "truly unsavoury sort"};
            if (parameters.Length < 1)
                return;
            Random random = new Random();
            Say(room, "Hey " + parameters[0] + ", you are a " + Insults[random.Next(Insults.Length)] + "!");
        }

        public void Klappa(string user, string room, string[] parameters)
        {
            Say(room, "Kappa //");
        }

        public void Join(string user, string room, string[] parameters)
        {
            if (parameters.Length < 1)
                return;
            Join(parameters[0]);
        }

        public void Part(string user, string room, string[] parameters)
        {
            Part(room);
        }

        public void Test(string user, string room, string[] parameters)
        {
            Say(room, "PACHOW!");
        }
    }
}
