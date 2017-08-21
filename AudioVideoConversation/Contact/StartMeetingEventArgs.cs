using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioVideoConversation
{
    public class StartMeetingEventArgs : EventArgs
    {
        public Appointment Appointment { get; set; }
    }
}
