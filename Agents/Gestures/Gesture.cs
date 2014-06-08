using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gestures
{
    public class Gesture
    {
        const int MAX_FRAME_COUNT = 50;

        public event EventHandler<GestureEventArgs> GestureRecognized;
        GestureType _type;

        IGestureSegment[] _segments;
        int _current_segment;

        int _frameCount;

        public Gesture(GestureType type, IGestureSegment[] segments)
        {
            _type = type;
            _segments = segments;
        }

        public void Update(Skeleton skeleton)
        {
            GestureSegmentResult result = _segments[_current_segment].Update(skeleton);
            if (result == GestureSegmentResult.Succeeded)
            {
                if (_current_segment + 1 < _segments.Length)
                {
                    _current_segment++;
                    _frameCount = 0;
                }
                else
                {
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new GestureEventArgs(_type, skeleton.TrackingId));
                        Reset();
                    }
                }
            }
            else if (result == GestureSegmentResult.Failed || _frameCount == MAX_FRAME_COUNT)
            {
                Reset();
            }
            else
            {
                _frameCount++;
            }
        }

        public void Reset()
        {
            _current_segment = 0;
            _frameCount = 0;
        }
    }
}
