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
    //Different message types
    enum MessageType
    {
        PASS,
        NICK,
        USER,
        JOIN,
        PART,
        PRIVMSG,
        PING,
        PONG,
        NOTICE,
        TMI,
        NUMERIC,
        MODE,
        SPECIALUSER,
        USERCOLOR,
        EMOTESET,
        HISTORYEND,
        INVALID
    }

    //Container for a message
    class Message
    {
        public const string MessageEnd = "\r\n";
        public string[] Parameters;
        public MessageType Type;
        public int NumericType = -1;
        public string Sender="";
        public string SenderName = "";
        public const int MaxMessageSize = 1024;

        static Dictionary<string, MessageType> typeLookup;

        static Message()
        {
            typeLookup = new Dictionary<string, MessageType>();
            for (int x = 0; (MessageType)x != MessageType.INVALID; x++)
                typeLookup.Add(((MessageType)x).ToString(), (MessageType)x);
        }

        public Message(byte[] bytes)
        {
            string message="";
            bool lf = false;
            for (int x = 0; x < bytes.Length; x++)
            {
                char c = (char)bytes[x];
                if (c == MessageEnd[1])
                    lf = true;
                else if (c == '\0')
                    throw new DataMisalignedException();
                message += c;
                if (lf)
                    break;
            }
            string[] messageSplit = message.Split(' ');
            int offset = 0;
            //We have a sender segment
            if (messageSplit[offset][0] == ':')
            {
                Sender = messageSplit[offset].Substring(1);
                SenderName = Sender.Split('!')[0];
                offset++;
            }

            if (int.TryParse(messageSplit[offset], out NumericType))
            {
                Type = MessageType.NUMERIC;
            }
            else
            {
                Type = typeLookup[messageSplit[offset]];
            }
            offset++;
            Parameters = new string[messageSplit.Length - offset];
            for (int x = offset; x < messageSplit.Length; x++)
                Parameters[x - offset] = messageSplit[x];
            Parameters[Parameters.Length - 1] = Parameters[Parameters.Length - 1].Substring(0, Parameters[Parameters.Length - 1].Length - 2);
        }

        public Message(MessageType t, params string[] param)
        {
            Parameters = param;
            Type = t;
        }

        public byte[] ToBytes()
        {
            string TypeString = Type.ToString();
            string ParameterString = "";
            for (int x = 0; x < Parameters.Length; x++)
                ParameterString += " " + Parameters[x];
            string MessageString = TypeString + ParameterString + MessageEnd;
            byte[] bytes = new byte[MessageString.Length];
            for (int x = 0; x < bytes.Length; x++)
                bytes[x] = (byte)MessageString[x];
            return bytes;
        }

        public override string ToString()
        {
            return ((Sender != "") ? "Sender: " + Sender + "\n" : "") +" "+ Type.ToString()+"\n";
        }
    }
}
