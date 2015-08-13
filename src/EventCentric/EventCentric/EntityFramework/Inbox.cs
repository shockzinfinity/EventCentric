using System;
using System.Collections.Generic;

namespace EventCentric.EntityFramework
{
    public partial class Inbox
    {
        public long InboxId { get; set; }
        public System.Guid EventId { get; set; }
        public string StreamType { get; set; }
        public System.Guid StreamId { get; set; }
        public int Version { get; set; }
        public string EventType { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string Payload { get; set; }
    }
}
