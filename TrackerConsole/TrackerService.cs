using System;
using System.Numerics;
using System.Threading;
using Valve.VR;

namespace TrackerConsole
{
    class TrackerService : IDisposable
    {
        private readonly Thread _openVrThread;
        private const int UpdatedInterval = 20;
        
        private readonly Vector3[] _relativeAnchors = new Vector3[OpenVR.k_unMaxTrackedDeviceCount];
        private readonly bool[] _previousButtonState = new bool[OpenVR.k_unMaxTrackedDeviceCount];

        private bool _keepReading = true;

        public event EventHandler<DeviceTrackingData> NewPoseUpdate;

        public TrackerService()
        {
            _openVrThread = new Thread(TrackingLoop)
            {
                Name = "OpenVrTrackingThread",
                IsBackground = true
            };
            _openVrThread.Start();
        }

        private void TrackingLoop()
        {
            try
            {
                EVRInitError initError = EVRInitError.None;
                CVRSystem cvrSystem = OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Utility);
                if (initError != EVRInitError.None)
                {
                    throw new InvalidOperationException($"EVR init erro: {initError}");
                }
                while (_keepReading)
                {
                    TrackedDevicePose_t[] trackedDevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                    cvrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0f, trackedDevicePoses);
                    for (uint trackedDeviceIndex = 0; trackedDeviceIndex < OpenVR.k_unMaxTrackedDeviceCount; trackedDeviceIndex++)
                    {
                        if (cvrSystem.IsTrackedDeviceConnected(trackedDeviceIndex))
                        {
                            ETrackedDeviceClass deviceClass = cvrSystem.GetTrackedDeviceClass(trackedDeviceIndex);
                            if (deviceClass == ETrackedDeviceClass.Controller)
                            {
                                VRControllerState_t controllerState = new VRControllerState_t();
                                cvrSystem.GetControllerState(1, ref controllerState, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t)));
                                ETrackingResult trackingResult = trackedDevicePoses[trackedDeviceIndex].eTrackingResult;

                                bool trigger = controllerState.rAxis1.x > 0.9f;
                                bool menuButton = (controllerState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu)) != 0;

                                if (trackingResult == ETrackingResult.Running_OK)
                                {
                                    HmdMatrix34_t trackingMatrix = trackedDevicePoses[trackedDeviceIndex].mDeviceToAbsoluteTracking;
                                    if (menuButton && !_previousButtonState[trackedDeviceIndex])
                                    {
                                        _relativeAnchors[trackedDeviceIndex] = trackingMatrix.ToPositionVector();
                                    }
                                    _previousButtonState[trackedDeviceIndex] = menuButton;

                                    Vector3 speedVector = trackedDevicePoses[trackedDeviceIndex].vVelocity.ToVelocityVector();
                                    Vector3 position = trackingMatrix.ToPositionVector() - _relativeAnchors[(int) trackedDeviceIndex];
                                    DeviceTrackingData trackingUpdate = new DeviceTrackingData((int)trackedDeviceIndex, position * 100, trackingMatrix.ToRotationQuaternion());
                                    if (trigger)
                                    {
                                        Console.WriteLine($"position {trackingUpdate.Position}");
                                        NewPoseUpdate?.Invoke(this, trackingUpdate);
                                    }
                                }

                            }
                        }
                    }
                    Thread.Sleep(UpdatedInterval);
                }
            }
            finally 
            {
                OpenVR.Shutdown();
            }
        }

        public void SignalStop()
        {
            _keepReading = false;
        }

        private void ReleaseUnmanagedResources()
        {
            _openVrThread.Abort();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            _openVrThread.Join();
            GC.SuppressFinalize(this);
        }

        ~TrackerService()
        {
            ReleaseUnmanagedResources();
        }
    }
}
