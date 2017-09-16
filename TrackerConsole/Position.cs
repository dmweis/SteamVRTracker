using Valve.VR;

namespace TrackerConsole
{
   class Position
   {
      public float X { get; }
      public float Y { get; }
      public float Z { get; }

      public Position(HmdMatrix34_t pose)
      {
         float[,] matrix = new float[4, 4];
         matrix[0, 0] = pose.m0;
         matrix[0, 1] = pose.m1;
         matrix[0, 2] = -pose.m2;
         matrix[0, 3] = pose.m3;

         matrix[1, 0] = pose.m4;
         matrix[1, 1] = pose.m5;
         matrix[1, 2] = -pose.m6;
         matrix[1, 3] = pose.m7;

         matrix[2, 0] = -pose.m8;
         matrix[2, 1] = -pose.m9;
         matrix[2, 2] = pose.m10;
         matrix[2, 3] = -pose.m11;

         X = matrix[0, 3];
         Y = matrix[1, 3];
         Z = matrix[2, 3];
      }

      public override string ToString()
      {
         return $"X:{X:f2} Y:{Y:f2} Z:{Z:f2}";
      }
      public string ToData()
      {
         return $"{X:f2} {Y:f2} {Z:f2}";
      }
   }
}
