using System.Numerics;
using Valve.VR;

namespace TrackerConsole
{
    class DeviceTrackingData
    {
        public int DeviceIndex { get; }
        public string DeviceClass { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public DeviceTrackingData(int deviceIndex, string deviceClass, HmdMatrix34_t hmdMatrix)
        {
            DeviceIndex = deviceIndex;
            DeviceClass = deviceClass;
            Position = hmdMatrix.ToPositionVector();
            Rotation = hmdMatrix.ToRotationQuaternion();
        }

        public DeviceTrackingData(int deviceIndex, string deviceClass, Vector3 position, Quaternion rotation)
        {
            DeviceIndex = deviceIndex;
            DeviceClass = deviceClass;
            Position = position;
            Rotation = rotation;
        }
    }
}
