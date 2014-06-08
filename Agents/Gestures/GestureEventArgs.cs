using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gestures
{
    public class GestureEventArgs
    {
        public GestureType Type { get; set; }
        public int TrackingId { get; set; }

        public GestureEventArgs(GestureType type, int trackingId)
        {
            Type = type;
            TrackingId = trackingId;
        }
    }
}
