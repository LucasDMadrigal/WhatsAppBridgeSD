using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhatsAppBridgeSD.src.Core.Models
{
    public class Message
    {
        public long Id { get; set; }
        public string FromPhone { get; set; }
        public string ToPhone { get; set; }
        public string WhatsAppMessageId { get; set; }
  //      public TypeMessage Type { get; set; }   // "text", "image", ...
        public string Type { get; set; }
        public string Body { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }
//        public StatusMessage Status { get; set; } // "received", "acknowledged", "processed"

        public string Status { get; set; } // "received", "acknowledged", "processed"
    }
}
