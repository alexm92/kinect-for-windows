using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestures
{
    public class SwipeLeftSegment1 : IGestureSegment
    {
        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // right hand in front of elbow right
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z)
            {
                // right hand between head and hip
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // right hand right of right shoulder
                    if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ShoulderRight].Position.X)
                    {
                        return GestureSegmentResult.Succeeded;
                    }

                    return GestureSegmentResult.Undetermined;
                }
            }

            return GestureSegmentResult.Failed;
        }
    }

    public class SwipeLeftSegment2 : IGestureSegment
    {
        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // right hand in front of elbow right
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z)
            {
                // right hand between head and hip
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // right hand between right and left shoulder
                    if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ShoulderLeft].Position.X && skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ShoulderRight].Position.X)
                    {
                        return GestureSegmentResult.Succeeded;
                    }

                    return GestureSegmentResult.Undetermined;
                }
            }

            return GestureSegmentResult.Failed;
        }
    }

    public class SwipeLeftSegment3 : IGestureSegment
    {
        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // right hand in front of elbow right
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z)
            {
                // right hand between head and hip
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // right hand left of left shoulder
                    if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
                    {
                        return GestureSegmentResult.Succeeded;
                    }

                    return GestureSegmentResult.Undetermined;
                }
            }

            return GestureSegmentResult.Failed;
        }
    }
}
