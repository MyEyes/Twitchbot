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
            this.silent = silent;
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
