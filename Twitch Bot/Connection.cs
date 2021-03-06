﻿/********************************************************************************
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Twitch_Bot
{
    class Connection
    {
        public string ServerAddress = "199.9.250.229";
        public int ServerPort = 6667;

        Socket Socket;
        Thread receiveThread;

        byte[] buffer = new byte[2 * Message.MaxMessageSize];

        Bot owner;

        public Connection(Bot owner)
        {
            this.owner = owner;
        }        

        public void Connect()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            while (!Socket.Connected)
            {
                try
                {
                    Socket.Connect(ServerAddress, ServerPort);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("Error connecting to server, retrying...");
                    Console.WriteLine(se);
                    SpinWait.SpinUntil(delegate { return false; }, 1000);
                }
            }

            receiveThread = new Thread(new ThreadStart(ReceiveLoop));



            receiveThread.Start();
        }

        public void Reconnect()
        {
            try
            {
                receiveThread.Abort();
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Dispose();
                Socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                while (!Socket.Connected)
                {
                    try
                    {
                        Socket.Connect(ServerAddress, ServerPort);
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("Error {0} while connecting to server, retrying...",se.ErrorCode);
                        Console.WriteLine(se);
                        SpinWait.SpinUntil(delegate { return false; }, 1000);
                    }
                }

                while (!Socket.Connected) ;
                receiveThread = new Thread(ReceiveLoop);
                receiveThread.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine("While trying to reconnect:");
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(Message m)
        {
            if (Socket != null && Socket.Connected)
                Socket.Send(m.ToBytes());
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

                int bytes = 0;
                try
                {
                    bytes = Socket.Receive(buffer, bufferWriteOffset, Message.MaxMessageSize, SocketFlags.None);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("Exception occurred while trying to receive:" + se.ToString());
                }
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
                            owner.Handle(m);
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

        public bool Connected
        {
            get { return Socket.Connected; }
        }
    }
}
