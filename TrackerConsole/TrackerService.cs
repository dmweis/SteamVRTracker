using System;
using System.Linq;
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
        //private readonly ControllerButtonState[] _previousButtonState = new ControllerButtonState[OpenVR.k_unMaxTrackedDeviceCount];
        private readonly ControllerButtonState[] _previousButtonState = 
            Enumerable
            .Range(1, (int)OpenVR.k_unMaxTrackedDeviceCount)
            .Select(num => new ControllerButtonState())
            .ToArray();

        private volatile bool _keepReading = true;

        public event EventHandler<DeviceTrackingData> NewPoseUpdate;

        private ChaperonePlayArea _playArea;
        public ChaperonePlayArea PlayArea
        {
            get { return _playArea; }
        }


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
                var chaperone = OpenVR.Chaperone;
                var quadArea = new HmdQuad_t();
                chaperone.GetPlayAreaRect(ref quadArea);
                _playArea = quadArea.ToPlayArea();
                if (initError != EVRInitError.None)
                {
                    throw new InvalidOperationException($"EVR init erro: {initError}");
                }
                while (_keepReading)
                {
                    TrackedDevicePose_t[] trackedDevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                    cvrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, trackedDevicePoses);
                    for (uint trackedDeviceIndex = 0; trackedDeviceIndex < OpenVR.k_unMaxTrackedDeviceCount; trackedDeviceIndex++)
                    {
                        if (cvrSystem.IsTrackedDeviceConnected(trackedDeviceIndex))
                        {
                            ETrackedDeviceClass deviceClass = cvrSystem.GetTrackedDeviceClass(trackedDeviceIndex);
                            if (true)
                            {
                                VRControllerState_t controllerState = new VRControllerState_t();
                                cvrSystem.GetControllerState(1, ref controllerState, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t)));
                                ETrackingResult trackingResult = trackedDevicePoses[trackedDeviceIndex].eTrackingResult;

                                bool trigger = controllerState.rAxis1.x > 0.9f;
                                bool menuButton = (controllerState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu)) != 0;
                                bool gripButton = (controllerState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_Grip)) != 0;

                                if (trackingResult == ETrackingResult.Running_OK)
                                {
                                    
                                    HmdMatrix34_t trackingMatrix = trackedDevicePoses[trackedDeviceIndex].mDeviceToAbsoluteTracking;

                                    Vector3 speedVector = trackedDevicePoses[trackedDeviceIndex].vVelocity.ToVelocityVector();
                                    Vector3 position = trackingMatrix.ToPositionVector();
                                    DeviceTrackingData trackingUpdate = new DeviceTrackingData((int)trackedDeviceIndex, deviceClass.ToString(), position, trackingMatrix.ToRotationQuaternion());
                                    NewPoseUpdate?.Invoke(this, trackingUpdate);
                                    
                                }

                            }
                        }
                    }
                    Thread.Sleep(UpdatedInterval);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
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
