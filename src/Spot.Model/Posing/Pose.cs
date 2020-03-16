using System.Collections.Generic;

namespace Spot.Model.Posing
{
    public class Pose
    {
        public string Name { get; set; }
        public double Duration { get; set; }
        public IList<double?> Values { get; set; }
    }
}
