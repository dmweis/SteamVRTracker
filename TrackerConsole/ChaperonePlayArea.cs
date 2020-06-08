using System.Collections.Generic;
using System.Numerics;

namespace TrackerConsole
{
    class ChaperonePlayArea
    {
        public List<Vector3> Corners { get; }

        public ChaperonePlayArea(List<Vector3> corners)
        {
            Corners = corners;
        }
    }
}
