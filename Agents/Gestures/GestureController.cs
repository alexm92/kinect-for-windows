using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestures
{
    public class GestureController
    {
        List<Gesture> _gestures;
        public event EventHandler<GestureEventArgs> GestureRecognized;

        public GestureController()
        {
            _gestures = new List<Gesture>();
        }

        public GestureController(GestureType[] typeList)
        {
            _gestures = new List<Gesture>();

            foreach (var type in typeList)
            {
                AddGesture(type);
            }
        }

        private void AddGesture(GestureType type)
        {
            IGestureSegment[] segments = null;

            switch (type)
            {
                case GestureType.WaveRight:
                    WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
                    WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();

                    segments = new IGestureSegment[] {
                        waveRightSegment1, waveRightSegment2,
                        waveRightSegment1, waveRightSegment2,
                        waveRightSegment1, waveRightSegment2
                    };
                    break;

                case GestureType.WaveLeft:
                    WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
                    WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();

                    segments = new IGestureSegment[] {
                        waveLeftSegment1, waveLeftSegment2,
                        waveLeftSegment1, waveLeftSegment2,
                        waveLeftSegment1, waveLeftSegment2
                    };
                    break;
                
                case GestureType.SwipeLeft:
                    segments = new IGestureSegment[] {
                        new SwipeLeftSegment1(),
                        new SwipeLeftSegment2(),
                        new SwipeLeftSegment3()
                    };
                    break;

                case GestureType.SwipeRight:
                    segments = new IGestureSegment[] {
                        new SwipeRightSegment1(),
                        new SwipeRightSegment2(),
                        new SwipeRightSegment3(),
                    };
                    break;
            }

            var gesture = new Gesture(type, segments);
            gesture.GestureRecognized += OnGestureRecognized;
            _gestures.Add(gesture);
        }

        public void Update(Skeleton skeleton) {
            foreach (var gesture in _gestures) {
                gesture.Update(skeleton);
            }
        }

        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            if (GestureRecognized != null)
            {
                GestureRecognized(this, e);
            }

            foreach (var gesture in _gestures)
            {
                gesture.Reset();
            }
        }
    }
}
