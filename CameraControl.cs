using UnityEngine;

namespace CwtD
{
    public enum CameraMode
    {
        Original,
        Left,
        Right,
        Down,
        Devil,
        FiendLeft,
        FiendRight,
        RoundIndicator,
        PlayedCard,
        Cutscene,
        RecoverCard,
        CheeseEatingCompetition
    }

    public class CameraControl : MonoBehaviour
    {
        private float currentMovementSpeed;
        [SerializeField] private float movementSpeed = 2f;
        [SerializeField] private float rotationSpeed = 5f;
        //[SerializeField] private float cutsceneLerpSpeed = 70f;
        [SerializeField] private float cutsceneArrivalThreshold = 0.1f;

        [SerializeField] private GameObject originalCamera;
        [SerializeField] private GameObject leftCamera;
        [SerializeField] private GameObject rightCamera;
        [SerializeField] private GameObject downCamera;
        [SerializeField] private GameObject devilCamera;
        [SerializeField] private GameObject fiendCameraLeft;
        [SerializeField] private GameObject fiendCameraRight;
        [SerializeField] private GameObject roundIndicatorCamera;
        [SerializeField] private GameObject cutsceneCamera;
        [SerializeField] private GameObject playedCardCamera;
        [SerializeField] private GameObject recoverCardCamera;

        private Camera mainCamera;
        public CameraMode currentMode { private set; get; }
        public bool hasReachedTargetPosition { private set; get; }

        void Start()
        {
            mainCamera = Camera.main;
            SetCameraMode(CameraMode.Cutscene);
            mainCamera.transform.SetPositionAndRotation(cutsceneCamera.transform.position, cutsceneCamera.transform.rotation);
        }
        
        void Update()
        {
            MoveCamera();
        }

        void MoveCamera()
        {
            Vector3 targetPosition = originalCamera.transform.position;
            Quaternion targetRotation = originalCamera.transform.rotation;

            switch (currentMode)
            {
                case CameraMode.Left:
                    targetPosition = leftCamera.transform.position;
                    targetRotation = leftCamera.transform.rotation;
                    break;
                case CameraMode.Right:
                    targetPosition = rightCamera.transform.position;
                    targetRotation = rightCamera.transform.rotation;
                    break;
                case CameraMode.Down:
                    targetPosition = downCamera.transform.position;
                    targetRotation = Quaternion.Euler(90, 0, 0); // Rotate 90 degrees when looking down
                    break;
                case CameraMode.PlayedCard:
                    targetPosition = playedCardCamera.transform.position;
                    targetRotation = Quaternion.Euler(90, 0, 0);
                    break;
                case CameraMode.Devil:
                    targetPosition = devilCamera.transform.position;
                    targetRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case CameraMode.FiendLeft:
                    targetPosition = fiendCameraLeft.transform.position;
                    targetRotation = Quaternion.Euler(0, -35, 0);
                    break;
                case CameraMode.FiendRight:
                    targetPosition = fiendCameraRight.transform.position;
                    targetRotation = Quaternion.Euler(0, 35, 0);
                    break;
                case CameraMode.RoundIndicator:
                    targetPosition = roundIndicatorCamera.transform.position;
                    targetRotation = Quaternion.Euler(0, 55, 0);
                    break;
                case CameraMode.Cutscene:
                    targetPosition = cutsceneCamera.transform.position;
                    targetRotation = cutsceneCamera.transform.rotation;
                    break;
                case CameraMode.RecoverCard:
                    targetPosition = recoverCardCamera.transform.position;
                    targetRotation = recoverCardCamera.transform.rotation;
                    break;
                case CameraMode.CheeseEatingCompetition:
                    targetRotation = Quaternion.Euler(40, 0, 0);
                    break;
            }

            if (!hasReachedTargetPosition)
            {
                // Check if the camera is close enough to the position
                if (Vector3.Distance(mainCamera.transform.position, targetPosition) <= cutsceneArrivalThreshold)
                {
                    hasReachedTargetPosition = true;
                }
            }

            if (hasReachedTargetPosition && currentMode == CameraMode.Cutscene)
            {
                mainCamera.transform.position = targetPosition;
                mainCamera.transform.rotation = targetRotation;
            }
            else
            {
                // Smoothly interpolate to the target position and rotation
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, movementSpeed * Time.deltaTime);
                mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // Method to set camera mode from other scripts
        public void SetCameraMode(CameraMode mode)
        {
            currentMode = mode;
            hasReachedTargetPosition = false;
        }
    }
}
