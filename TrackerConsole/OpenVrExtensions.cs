using System;
using System.Collections.Generic;
using System.Numerics;
using Valve.VR;

namespace TrackerConsole
{
    static class OpenVrExtensions
    {
        public static Vector3 ToPositionVector(this HmdMatrix34_t hmdMatrix)
        {
            float x = hmdMatrix.m3;
            float y = hmdMatrix.m7;
            float z = -hmdMatrix.m11;
            return new Vector3(x, y, z);
        }

        public static Quaternion ToRotationQuaternion(this HmdMatrix34_t hmdMatrix)
        {
            float[,] matrix = new float[4, 4];

            matrix[0, 0] = hmdMatrix.m0;
            matrix[0, 1] = hmdMatrix.m1;
            matrix[0, 2] = -hmdMatrix.m2;
            matrix[0, 3] = hmdMatrix.m3;

            matrix[1, 0] = hmdMatrix.m4;
            matrix[1, 1] = hmdMatrix.m5;
            matrix[1, 2] = -hmdMatrix.m6;
            matrix[1, 3] = hmdMatrix.m7;

            matrix[2, 0] = -hmdMatrix.m8;
            matrix[2, 1] = -hmdMatrix.m9;
            matrix[2, 2] = hmdMatrix.m10;
            matrix[2, 3] = -hmdMatrix.m11;

            float w = (float)Math.Sqrt(Math.Max(0, 1 + matrix[0, 0] + matrix[1, 1] + matrix[2, 2])) / 2;
            float x = (float)Math.Sqrt(Math.Max(0, 1 + matrix[0, 0] - matrix[1, 1] - matrix[2, 2])) / 2;
            float y = (float)Math.Sqrt(Math.Max(0, 1 - matrix[0, 0] + matrix[1, 1] - matrix[2, 2])) / 2;
            float z = (float)Math.Sqrt(Math.Max(0, 1 - matrix[0, 0] - matrix[1, 1] + matrix[2, 2])) / 2;
            x = CopySign(x, matrix[2, 1] - matrix[1, 2]);
            y = CopySign(y, matrix[0, 2] - matrix[2, 0]);
            z = CopySign(z, matrix[1, 0] - matrix[0, 1]);
            return new Quaternion(x, y, z, w);
        }

        public static Vector3 ToVelocityVector(this HmdVector3_t hmdVector)
        {
            return new Vector3(hmdVector.v0, hmdVector.v1, hmdVector.v2);
        }

        public static ChaperonePlayArea ToPlayArea(this HmdQuad_t hmdQuad)
        {
            var points = new List<Vector3>();
            points.Add(hmdQuad.vCorners0.ToVelocityVector());
            points.Add(hmdQuad.vCorners1.ToVelocityVector());
            points.Add(hmdQuad.vCorners2.ToVelocityVector());
            points.Add(hmdQuad.vCorners3.ToVelocityVector());
            return new ChaperonePlayArea(points);
        }

        private static float CopySign(float sizeval, float signval)
        {
            return Math.Sign(signval) == 1 ? Math.Abs(sizeval) : -Math.Abs(sizeval);
        }
    }
}
