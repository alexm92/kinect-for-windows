using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestures
{
    public class WaveRightSegment1 : IGestureSegment
    {

        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // hand above elbow
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // hand right of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    return GestureSegmentResult.Succeeded;
                }

                return GestureSegmentResult.Undetermined;
            }

            // hand dropped
            return GestureSegmentResult.Failed;
        }
    }

    public class WaveRightSegment2 : IGestureSegment
    {

        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // hand above elbow
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // hand left of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X)
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
