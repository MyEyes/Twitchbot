using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    //Contains Commands
    class Command
    {
        public string Name;
        UserLevel AccessLevel;
        string[] AccessUsers;
        Action<string,string,string[]> _command;
        bool silent = false;

        public Command(string name, UserLevel level, string[] users, Action<string, string, string[]> command, bool silent=false)
        {
            Name = name;
            AccessLevel = level;
            AccessUsers = users;
            _command = command;
        }

        public bool Execute(User user, Room room, string[] parameters)
        {
            if (user.Level >= AccessLevel)
            {
                if (!silent)
                    Console.WriteLine("User " + user.Name + " executed " + Name + " in " + room.Name + "!");
                _command(user.Name, room.Name, parameters);
                return true;
            }
            else
            {
                for (int x = 0; x < AccessUsers.Length; x++)
                    if (user.Name == AccessUsers[x])
                    {
                        if (!silent)
                            Console.WriteLine("User " + user.Name + " executed " + Name + " in " + room.Name + "!");
                        _command(user.Name, room.Name, parameters);
                        return true;
                    }
            }
            if (!silent)
                Console.WriteLine("User " + user.Name + " does not have access to " + Name + " in " + room.Name + "!");
            return false;
        }
    }
}
