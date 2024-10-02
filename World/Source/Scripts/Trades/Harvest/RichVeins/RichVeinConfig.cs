using System.Collections.Generic;

namespace Server.Engines.Harvest
{
    public class RichVeinConfig
    {
        public List<Point3D> Candidates { get; set; }
        public int MaxNodes { get; set; }
        public int MinDistance { get; set; }
        public string Region { get; set; }
    }
}