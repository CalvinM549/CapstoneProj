using Unity.Cinemachine;
using UnityEngine;

namespace PixelPerfectFollow
{
    [DefaultExecutionOrder(-90)] // Call before LateUpdate of Cinemachine
    public class FollowOffsetCalculator : MonoBehaviour
    {
        public int pixelPerUnit = 16;
        public Vector3 targetOffset; // Offset from the target object (in target-local co-ordinates)
        public CinemachineCamera cinemachineCamera;

        CinemachineFollow cinemachineFollow;

        void Start()
        {
            if (cinemachineCamera != null)
                cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
        }

        void LateUpdate()
        {
            if (cinemachineCamera != null && cinemachineFollow != null && cinemachineCamera.Target.TrackingTarget != null)
            {
                var followOffset2D = cinemachineCamera.Target.TrackingTarget.rotation * targetOffset;

                followOffset2D.x = (int)(followOffset2D.x * pixelPerUnit) / (float)pixelPerUnit;
                followOffset2D.y = (int)(followOffset2D.y * pixelPerUnit) / (float)pixelPerUnit;

                cinemachineFollow.FollowOffset = new Vector3(followOffset2D.x, followOffset2D.y, cinemachineFollow.FollowOffset.z);
            }
        }
    }
}
