using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twitch_Bot
{
    class RegularMessages
    {
        public string Room;
        public string Message;
        public TimeSpan Interval;
        public DateTime lastIssued;
        public int minMessages;
        public int lastMessages;

        public RegularMessages(string Room, string Message, TimeSpan interval, int minMessages)
        {
            this.Room = Room;
            this.Message = Message;
            this.Interval = interval;
            this.minMessages = minMessages;
            this.lastIssued = DateTime.Now;
        }
    }
}
