using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    class Room
    {
        public string Name;
        public List<User> Users = new List<User>();
        public int Messages = 0;

        public Room(string name)
        {
            Name = name;
        }

        public User GetUser(string name)
        {
            for (int x = 0; x < Users.Count; x++)
                if (Users[x].Name == name)
                    return Users[x];
            return null;
        }

        public void Part(string name)
        {
            for (int x = 0; x < Users.Count; x++)
                if (Users[x].Name == name)
                {
                    Users.RemoveAt(x);
                    return;
                }
        }

        public void Join(string name)
        {
            for (int x = 0; x < Users.Count; x++)
                if (Users[x].Name == name)
                {
                    return;
                }
            User user = new User();
            user.Level = UserLevel.Normal;
            user.Name = name;
            Users.Add(user);
        }
    }
}
