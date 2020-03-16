using System.Collections.Generic;

namespace Spot.Model.Posing
{
    public class PoseSettings
    {
        public double JointRefreshFrequency { get; set; }
        public IList<Pose> Poses { get; set; }
    }
}