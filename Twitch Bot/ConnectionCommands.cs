using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    //Setting up commands for the bot
    partial class Connection
    {

        public void SetUpCommands()
        {
            AddCommand(new Command("test", UserLevel.Mod, new string[] { }, Test));
            AddCommand(new Command("join", UserLevel.Admin, new string[] { }, Join));
            AddCommand(new Command("part", UserLevel.Mod, new string[] { }, Part));
            AddCommand(new Command("Klappa", UserLevel.Normal, new string[] { }, Klappa));
            AddCommand(new Command("Slap", UserLevel.Normal, new string[] { }, Slap));
            AddCommand(new Command("Insult", UserLevel.Normal, new string[] { }, Insult));
            AddCommand(new Command("Count", UserLevel.Mod, new string[] { }, MessageCount));
            AddCommand(new Command("Version", UserLevel.Normal, new string[] { }, Version));
        }

        public void Version(string user, string room, string[] parameters)
        {
            Say(room, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
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
