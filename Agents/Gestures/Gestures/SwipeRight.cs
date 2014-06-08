using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestures
{
    public class SwipeRightSegment1 : IGestureSegment
    {
        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // left hand in front of left elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z)
            {
                // left hand between head and hip
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // left hand left of left shoulder
                    if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X)
                    {
                        return GestureSegmentResult.Succeeded;
                    }

                    return GestureSegmentResult.Undetermined;
                }
            }

            return GestureSegmentResult.Failed;
        }
    }

    public class SwipeRightSegment2 : IGestureSegment
    {
        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // left hand in front of left elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z)
            {
                // left hand between head and hip
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // left hand between right and left shoulder
                    if (skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ShoulderLeft].Position.X && skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ShoulderRight].Position.X)
                    {
                        return GestureSegmentResult.Succeeded;
                    }

                    return GestureSegmentResult.Undetermined;
                }
            }

            return GestureSegmentResult.Failed;
        }
    }

    public class SwipeRightSegment3 : IGestureSegment
    {
        public GestureSegmentResult Update(Skeleton skeleton)
        {
            // left hand in front of left elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z)
            {
                // left hand between head and hip
                if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    // left hand right of right shoulder
                    if (skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ShoulderRight].Position.X)
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
