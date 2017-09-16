using System.Numerics;
using Valve.VR;

namespace TrackerConsole
{
    class DeviceTrackingData
    {
        public int DeviceIndex { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public DeviceTrackingData(int deviceIndex, HmdMatrix34_t hmdMatrix)
        {
            DeviceIndex = deviceIndex;
            Position = hmdMatrix.ToPositionVector();
            Rotation = hmdMatrix.ToRotationQuaternion();
        }

        public DeviceTrackingData(int deviceIndex, Vector3 position, Quaternion rotation)
        {
            DeviceIndex = deviceIndex;
            Position = position;
            Rotation = rotation;
        }
    }
}
