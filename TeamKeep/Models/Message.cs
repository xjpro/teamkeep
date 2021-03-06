﻿using System.Collections.Generic;
using Teamkeep.Models.DataModels;

namespace Teamkeep.Models
{
    public class Message : MessageData
    {
        public Message()
        {
        }

        public Message(MessageData data)
        {
            Id = data.Id;
            TeamId = data.TeamId;
            Date = data.Date;
            To = data.To;
            Subject = data.Subject;
            Content = data.Content;
        }

        public new string To { get; set; }
        public string From { get; set; }
        public IEnumerable<int> RecipientPlayerIds { get; set; }
        public string TeamName { get; set; }
        public string DateTime { get { return Date.ToString("MMM d, yyyy, h:mm tt"); } }

        public bool RequestAvailability { get; set; }
        public int AvailabilityEventId { get; set; }
        public Game AvailabilityEvent { get; set; }
    }
}