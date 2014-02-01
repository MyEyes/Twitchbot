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
using System.IO;

namespace Twitch_Bot
{
    partial class Bot
    {
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
            for (int x = 0; x < commands.Count; x++)
            {
                ReplyCommand reply = commands[x] as ReplyCommand;
                if (reply != null)
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
    }
}
