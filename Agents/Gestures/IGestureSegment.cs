using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gestures
{
    public interface IGestureSegment
    {
        GestureSegmentResult Update(Skeleton skeleton);
    }
}
