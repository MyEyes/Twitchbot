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
using System.Threading;
using System.IO;

namespace Twitch_Bot
{
    partial class Bot
    {
        public string Username = "";
        public string ServerPassword = "";
        //Change this if you run your own
        public const string SuperAdminName = "firzen14";

        Timer sendTimer;

        DateTime[] coolDowns = new DateTime[20];
        DateTime lastSent = DateTime.Now;
        DateTime lastPing = DateTime.Now;

        List<SayContainer> messageQueue = new List<SayContainer>();

        Dictionary<string, Room> currentRooms = new Dictionary<string, Room>();

        List<Command> commands = new List<Command>();
        List<RegularMessages> regulars = new List<RegularMessages>();

        Connection connection;

        bool reconnecting = false;

        int messageOffset = 0;

        public static Bot CurrentBot;

        public Bot()
        {
            connection = new Connection(this);
            CurrentBot = this;
            SetUpCommands();
        }

        public void Connect()
        {
            ReadUser("user.txt");
            connection.Connect();
            Login();
            JoinRooms();
            ReadRegulars();
            ReadReplies();
            Insurance();
            sendTimer = new Timer(Update, null, 1, 2);
        }


        public void Login()
        {
            Message PassMessage = new Message(MessageType.PASS, ServerPassword);
            connection.Send(PassMessage);

            Message NickMessage = new Message(MessageType.NICK, Username);
            connection.Send(NickMessage);

            Message UserMessage = new Message(MessageType.USER, Username, "0", "*", ":FirzenBot");
            connection.Send(UserMessage);
        }

        //Check if any of the regularly sent messages should be sent
        public void CheckRegulars(DateTime now)
        {
            for (int x = 0; x < regulars.Count; x++)
            {
                Room room = GetRoom(regulars[x].Room);
                if (room != null && now - regulars[x].lastIssued > regulars[x].Interval)
                {
                    if (room.Messages - regulars[x].lastMessages > regulars[x].minMessages)
                    {
                        Say(regulars[x].Room, regulars[x].Message);
                        regulars[x].lastIssued = now;
                        regulars[x].lastMessages = room.Messages;
                    }
                    else
                    {
                        regulars[x].lastIssued += new TimeSpan(0, 2, 0);
                    }
                }
            }
        }

        //Sends a message to a room, checking to not go over the message limit
        public void Say(string room, string Message)
        {
            TimeSpan diff = DateTime.Now - coolDowns[messageOffset];
            TimeSpan lastDiff = DateTime.Now - lastSent;
            if (diff.TotalSeconds > 35 && lastDiff.TotalSeconds > 1.5f)
            {
                Message Say = new Message(MessageType.PRIVMSG, room, ":" + Message);
                connection.Send(Say);
                lastSent = DateTime.Now;
                coolDowns[messageOffset] = lastSent;
                messageOffset++;
                messageOffset %= coolDowns.Length;
            }
            else
            {
                SayContainer c = new SayContainer();
                c.Message = Message;
                c.Room = room;
                messageQueue.Add(c);
                if (messageQueue.Count > 200)
                {
                    messageQueue.Clear();
                    Say(room, "Excessive amount of messages in queue; flushing!");
                }
            }
        }


        public void Update(object o)
        {
            if (connection.Connected && (DateTime.Now - lastPing).TotalMinutes<6)
            {
                DateTime now = DateTime.Now;
                TrySay(null);
                CheckRegulars(now);
            }
            else if (!reconnecting)
            {
                Console.WriteLine("Lost connection, attempting to reconnect");
                Reconnect();
            }
        }

        //Callback to try to clear out the message queue
        public void TrySay(object o)
        {
            if (messageQueue.Count > 0)
                TrySay(messageQueue[0]);
        }

        //Routine that tries saying an element in the message queue
        public bool TrySay(SayContainer c)
        {
            TimeSpan diff = DateTime.Now - coolDowns[messageOffset];
            if (diff.TotalSeconds > 35)
            {
                Message Say = new Message(MessageType.PRIVMSG, c.Room, ":" + c.Message);
                connection.Send(Say);
                coolDowns[messageOffset] = DateTime.Now;
                messageOffset++;
                messageOffset %= coolDowns.Length;
                messageQueue.Remove(c);
                return true;
            }
            return false;
        }

        public void Join(string room)
        {
            if (currentRooms.ContainsKey(room))
                return;
            currentRooms.Add(room, new Room(room));
            Message JoinMessage = new Message(MessageType.JOIN, room);
            connection.Send(JoinMessage);
        }

        public void ReJoinRooms()
        {
            foreach (string name in currentRooms.Keys)
            {
                currentRooms[name].Clear();
                Message JoinMessage = new Message(MessageType.JOIN, name);
                connection.Send(JoinMessage);
            }
        }

        public void Part(string room)
        {
            if (currentRooms.Count > 1)
            {
                currentRooms.Remove(room);
                Message PartMessage = new Message(MessageType.PART, room);
                connection.Send(PartMessage);
            }
            else
            {
                Say(room, "Bot should be in at least one room, will not leave");
            }
        }

        public void AddCommand(Command c)
        {
            commands.Add(c);
        }


        Room GetRoom(string name)
        {
            Room Room;
            if (currentRooms.TryGetValue(name, out Room))
                return Room;
            return null;
        }

        //When we receive a list of user, handle adding them to the correct room
        public void HandleUserList(Message m)
        {
            string room = m.Parameters[2];
            Room Room = GetRoom(room);
            if (Room != null)
            {
                for (int x = 3; x < m.Parameters.Length; x++)
                {
                    Room.Join(x > 3 ? m.Parameters[x] : m.Parameters[x].Substring(1));
                }
            }
            Console.WriteLine("Added " + (m.Parameters.Length - 3).ToString() + " users to " + room);
        }

        //Callback to handle a message
        public void Handle(Message m)
        {
            lastPing = DateTime.Now;
            switch (m.Type)
            {
                case MessageType.NUMERIC:
                    if (m.NumericType == 353)
                    {
                        HandleUserList(m);
                        return;
                    }
                    Console.Write(m.NumericType);
                    Console.Write(": ");
                    for (int x = 0; x < m.Parameters.Length; x++)
                        Console.Write(m.Parameters[x] + " ");
                    Console.WriteLine();
                    break;

                case MessageType.PRIVMSG:

                    if (m.Parameters.Length > 0)
                    {
                        Room Room;
                        if (currentRooms.TryGetValue(m.Parameters[0], out Room))
                            Room.Messages++;
                        if (Room == null)
                            return;
                        if (m.Parameters.Length > 1 && m.Parameters[1].Length > 1 && m.Parameters[1][1] == '!')
                            if (HandleCommand(m)) break;

                    }
                    /*
                    if (m.Parameters[0][0] == '#')
                    {
                        Console.WriteLine(m.Parameters[0]);
                        Console.Write("\t");
                    }
                    Console.Write(m.SenderName + ": ");
                    for (int x = 1; x < m.Parameters.Length; x++)
                        Console.Write(m.Parameters[x]+" ");
                    Console.WriteLine();
                     */
                    break;

                case MessageType.PING:
                    Message pong = new Message(MessageType.PONG, m.Parameters);
                    connection.Send(pong);
                    break;

                case MessageType.JOIN:
                    if (m.SenderName != "jtv")
                    {
                        Room room = GetRoom(m.Parameters[0]);
                        if (room != null)
                            room.Join(m.SenderName);
                    }
                    break;
                case MessageType.PART:
                    if (m.SenderName != "jtv")
                    {
                        Room room = GetRoom(m.Parameters[0]);
                        if (room != null)
                            room.Part(m.SenderName);
                    }
                    break;

                case MessageType.MODE:
                    if (m.SenderName == "jtv" && m.Parameters.Length == 3)
                    {
                        Room room = GetRoom(m.Parameters[0]);
                        UserLevel level = UserLevel.Normal;
                        if (m.Parameters[1] == "+o")
                            level = UserLevel.Mod;
                        if (room != null)
                        {
                            User user = room.GetUser(m.Parameters[2]);
                            if (user != null)
                                user.Level = level;
                        }
                    }
                    break;


                default:
                    try
                    {
                        Console.WriteLine(m.ToString());
                    }
                    catch (DataMisalignedException)
                    {
                    }
                    break;
            }
        }

        public void Exit()
        {
            WriteReplies();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public void Reconnect()
        {
            reconnecting = true;
            try
            {
                connection.Reconnect();
                Login();
                ReJoinRooms();
                lastPing = DateTime.Now;
            }
            finally
            {
                reconnecting = false;
            }
        }

        //If a message starts with ! try parsing it as a command
        public bool HandleCommand(Message m)
        {
            string room = m.Parameters[0];

            //Get Room and User objects
            Room Room;
            if (!currentRooms.TryGetValue(room, out Room))
                return false;
            User user = Room.GetUser(m.SenderName);
            if (user == null)
                return false;
            m.Parameters[1] = m.Parameters[1].Substring(2);
            string[] cParams = new string[m.Parameters.Length - 2];
            for (int x = 0; x < cParams.Length; x++)
            {
                cParams[x] = m.Parameters[x + 2];
            }

            for (int x = 0; x < commands.Count; x++)
            {
                if (m.Parameters[1] == commands[x].Name)
                    if (commands[x].Execute(user, Room, cParams))
                        return true;
            }
            return false;
        }
    }
}
