using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Twitch_Bot
{
    partial class Connection
    {
        public string Username = "";
        public string ServerPassword = "";
        public string ServerAddress = "irc.twitch.tv";
        public int ServerPort = 6667;
        //Change this if you run your on
        public string SuperAdminName = "firzen14";

        Socket Socket;
        Thread receiveThread;
        Timer sendTimer;

        byte[] buffer = new byte[2 * Message.MaxMessageSize];
        
        int messageOffset = 0;

        DateTime[] coolDowns = new DateTime[20];
        DateTime lastSent = DateTime.Now;

        List<SayContainer> messageQueue = new List<SayContainer>();

        Dictionary<string, Room> currentRooms = new Dictionary<string,Room>();

        List<Command> commands = new List<Command>();
        List<RegularMessages> regulars = new List<RegularMessages>();

        bool reconnecting = false;

        public static Connection CurrentConnection;

        public Connection()
        {
            CurrentConnection = this;
        }

        public void ReadUser(string file)
        {
            if (File.Exists(file))
            {
                StreamReader reader = new StreamReader(file);
                Username = reader.ReadLine();
                ServerPassword = reader.ReadLine();
                reader.Close();
            }
            else
            {
                Console.WriteLine("ERROR: No user file, can not join server");
                Exit();
            }
        }

        public void Connect()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect(ServerAddress, ServerPort);

            receiveThread = new Thread(new ThreadStart(ReceiveLoop));

            sendTimer = new Timer(new TimerCallback(Update), null, 1000, 2000);

            SetUpCommands();
            Insurance();

            while (!Socket.Connected) ;

            receiveThread.Start();

            Message PassMessage = new Message(MessageType.PASS, ServerPassword);
            Send(PassMessage);

            Message NickMessage = new Message(MessageType.NICK, Username);
            Send(NickMessage);

            Message UserMessage = new Message(MessageType.USER, Username, "0", "*", ":Firzen Bot");
            Send(UserMessage);

            JoinRooms();

            ReadRegulars();

            ReadReplies();
        }

        public void JoinRooms()
        {
            if (File.Exists("rooms.txt"))
            {
                StreamReader reader = new StreamReader("rooms.txt");
                while (!reader.EndOfStream)
                    Join(reader.ReadLine());
                reader.Close();
            }
            else
            {
                Console.WriteLine("ERROR: No rooms to join, read the readme file!");
                Exit();
            }
        }

        public void ReadRegulars()
        {
            if (File.Exists("regulars.txt"))
            {
                StreamReader reader = new StreamReader("regulars.txt");
                while (!reader.EndOfStream)
                {
                    string room = reader.ReadLine();
                    string message = reader.ReadLine();
                    string sminutes = reader.ReadLine();
                    string smessages = reader.ReadLine();
                    int minutes = 0;
                    int messages = 0;
                    int.TryParse(sminutes, out minutes);
                    int.TryParse(smessages, out messages);
                    regulars.Add(new RegularMessages(room, message, new TimeSpan(0, minutes, 00), messages));
                }
                reader.Close();
            }
            else
            {
                Console.WriteLine("Warning: No file for regular messages!");
            }
        }

        public void ReadReplies()
        {
            if (File.Exists("replies.txt"))
            {
                StreamReader reader = new StreamReader("replies.txt");
                while (!reader.EndOfStream)
                {
                    string room = reader.ReadLine();
                    string name = reader.ReadLine();
                    string message = reader.ReadLine();
                    commands.Add(new ReplyCommand(name, room, message));
                }
                reader.Close();
            }
            else
            {
                Console.WriteLine("Warning: No file for replies!");
            }
        }

        public void WriteReplies()
        {
            StreamWriter writer = null;
            for(int x=0; x<commands.Count; x++)
            {
                ReplyCommand reply = commands[x] as ReplyCommand;
                if(reply!=null)
                {
                    if (writer == null)
                        writer = new StreamWriter("replies.txt");
                    writer.WriteLine(reply.room);
                    writer.WriteLine(reply.Name);
                    writer.WriteLine(reply.message);
                }
            }
            if (writer != null)
                writer.Close();
            else if (File.Exists("replies.txt"))
                File.Delete("replies.txt");
        }

        public void Reconnect()
        {
            reconnecting = true;
            try
            {
                receiveThread.Abort();
                Socket.Connect(ServerAddress, ServerPort);

                while (!Socket.Connected) ;

                receiveThread.Start();

                Message PassMessage = new Message(MessageType.PASS, ServerPassword);
                Send(PassMessage);

                Message NickMessage = new Message(MessageType.NICK, Username);
                Send(NickMessage);

                Message UserMessage = new Message(MessageType.USER, Username, "0", "*", ":Firzen Bot");
                Send(UserMessage);

                JoinRooms();
            }
            catch (SocketException e)
            {
                Console.WriteLine("While trying to reconnect:");
                Console.WriteLine(e.ToString());
            }
            finally
            {
                reconnecting = false;
            }
        }

        public void Update(object o)
        {
            if (Socket.Connected)
            {
                DateTime now = DateTime.Now;
                TrySay(null);
                CheckRegulars(now);
            }
            else if (!reconnecting)
                Reconnect();
        }

        //Check if any of the regularly sent messages should be sent
        public void CheckRegulars(DateTime now)
        {
            for (int x = 0; x < regulars.Count; x++)
            {
                Room room = GetRoom(regulars[x].Room);
                if (room!=null && now - regulars[x].lastIssued > regulars[x].Interval)
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
            TimeSpan diff = DateTime.Now-coolDowns[messageOffset];
            TimeSpan lastDiff = DateTime.Now - lastSent;
            if (diff.TotalSeconds > 35 && lastDiff.TotalSeconds>1.5f)
            {
                Message Say = new Message(MessageType.PRIVMSG, room, ":" + Message);
                Send(Say);
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
                Send(Say);
                coolDowns[messageOffset] = DateTime.Now;
                messageOffset++;
                messageOffset %= coolDowns.Length;
                messageQueue.Remove(c);
                return true;
            }
            return false;
        }

        public void Send(Message m)
        {
            Socket.Send(m.ToBytes());
        }

        public void Join(string room)
        {
            if (currentRooms.ContainsKey(room))
                return;
            currentRooms.Add(room, new Room(room));
            Message JoinMessage = new Message(MessageType.JOIN, room);
            Send(JoinMessage);
        }

        public void Part(string room)
        {
            if (currentRooms.Count > 1)
            {
                currentRooms.Remove(room);
                Message PartMessage = new Message(MessageType.PART, room);
                Send(PartMessage);
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
            string[] cParams = new string[m.Parameters.Length-2];
            for(int x=0; x<cParams.Length; x++)
            {
                cParams[x]=m.Parameters[x+2];
            }

            for (int x = 0; x < commands.Count; x++)
            {
                if (m.Parameters[1] == commands[x].Name)
                    if (commands[x].Execute(user, Room, cParams))
                        return true;
            }
            return false;
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
                        Console.Write(m.Parameters[x]+" ");
                    Console.WriteLine();
                    break;

                case MessageType.PRIVMSG:

                    if (m.Parameters.Length > 0)
                    {
                        Room Room;
                        if (currentRooms.TryGetValue(m.Parameters[0], out Room))
                            Room.Messages++;
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
                    Send(pong);
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
                    if (m.SenderName == "jtv" && m.Parameters.Length==3)
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

        //Receive thread
        //We don't know if a message is completely inside the read buffer
        //So if we get an incomplete message we expect to receive the rest of
        //it in the next read operation
        //So there is some bufffer magic in this
        public void ReceiveLoop()
        {

            int bufferWriteOffset = 0;
            while (Socket.Connected)
            {
                int offset = 0;

                int bytes = Socket.Receive(buffer, bufferWriteOffset, Message.MaxMessageSize, SocketFlags.None);
                int totalBytes = bufferWriteOffset + bytes;
                if (bytes > 0)
                {
                    offset = 0;
                    bufferWriteOffset = 0;
                    //Check if there is a next byte and if we can fit it into the buffer
                    while (offset < totalBytes && buffer[offset] != 0)
                    {
                        byte[] messageBuffer = new byte[Message.MaxMessageSize];
                        int writeOffset = 0;
                        bool found = false;
                        while (writeOffset < messageBuffer.Length && offset<totalBytes)
                        {
                            messageBuffer[writeOffset] = buffer[offset];
                            //If we read the end character for an IRC package, we found a whole message
                            if (buffer[offset] == '\n')
                            {
                                writeOffset++;
                                offset++;
                                found = true;
                                break;
                            }
                            writeOffset++;
                            offset++;
                        }
                        if (found)
                        {
                            Message m = new Message(messageBuffer);
                            Handle(m);
                        }
                            //If we don't find a message copy the remaining unprocessed bytes to the beginning of the buffer
                        else
                        {
                            for (int x = 0; x < writeOffset; x++)
                            {
                                buffer[x] = messageBuffer[x];
                            }
                            for (int x = writeOffset; x < buffer.Length; x++)
                                buffer[x] = 0;
                            bufferWriteOffset = writeOffset;
                            break;
                        }
                    }

                }
            }
            Console.WriteLine("Connection Lost");
        }

        public void Exit()
        {
            receiveThread.Abort();
            Socket.Close();
            WriteReplies();
        }
    }
}
