using System;
using Valve.VR;

namespace TrackerConsole
{
   class Rotation
   {
      public float X { get; }
      public float Y { get; }
      public float Z { get; }
      public float W { get; }

      public Rotation(HmdMatrix34_t pose)
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

         this.W = (float)Math.Sqrt(Math.Max(0, 1 + matrix[0, 0] + matrix[1, 1] + matrix[2, 2])) / 2;
         this.X = (float)Math.Sqrt(Math.Max(0, 1 + matrix[0, 0] - matrix[1, 1] - matrix[2, 2])) / 2;
         this.Y = (float)Math.Sqrt(Math.Max(0, 1 - matrix[0, 0] + matrix[1, 1] - matrix[2, 2])) / 2;
         this.Z = (float)Math.Sqrt(Math.Max(0, 1 - matrix[0, 0] - matrix[1, 1] + matrix[2, 2])) / 2;
         this.X = _copysign(this.X, matrix[2, 1] - matrix[1, 2]);
         this.Y = _copysign(this.Y, matrix[0, 2] - matrix[2, 0]);
         this.Z = _copysign(this.Z, matrix[1, 0] - matrix[0, 1]);
      }

      private static float _copysign(float sizeval, float signval)
      {
         return Math.Sign(signval) == 1 ? Math.Abs(sizeval) : -Math.Abs(sizeval);
      }

      public override string ToString()
      {
         return $"X:{X:f2} Y:{Y:f2} Z:{Z:f2} W:{W:f2}";
      }

      public string ToString(string format)
      {
          return $"X:{X.ToString(format)} Y:{Y.ToString(format)} Z:{Z.ToString(format)} W:{W.ToString(format)}";
      }

        public string ToData()
      {
         return $"{X:f2} {Y:f2} {Z:f2} {W:f2}";
      }
   }
}
