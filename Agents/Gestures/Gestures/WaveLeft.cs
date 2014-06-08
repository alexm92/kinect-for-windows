using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestures
{
    public class WaveLeftSegment1 : IGestureSegment
    {

        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // hand above elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
            {
                // hand Left of elbow
                if (skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X)
                {
                    return GestureSegmentResult.Succeeded;
                }

                return GestureSegmentResult.Undetermined;
            }

            // hand dropped
            return GestureSegmentResult.Failed;
        }
    }

    public class WaveLeftSegment2 : IGestureSegment
    {

        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // hand above elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
            {
                // hand left of elbow
                if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X)
                {
                    return GestureSegmentResult.Succeeded;
                }

                return GestureSegmentResult.Undetermined;
            }

            // hand dropped
            return GestureSegmentResult.Failed;
        }
    }
}
