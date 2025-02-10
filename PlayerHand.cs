using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CwtD
{
    public abstract class HandFunction : MonoBehaviour
    {

        public Transform playedCardPos;
        public float yOffset = 0f; // Y offset for positioning
        //public float zOffset;
        public float spacing; // Spacing between objects
        public float spacingLerpSpeed = 7;
        public float smoothness;
        public Collider handCollider;
        public Collider boingBlocker;
        [SerializeField] private DiscardedCards discard;
        public GameObject cheeseHandPos;
        public CheeseInput cheeseInput;
        public GameplayManager gp;
        public bool recoveryInProgress = false;
        public bool specialRecoveryInProgress = false;
        public bool devilTurnBottleNeck;
        private float cardToHandThreshold = 0.01f;

        public void PlayCard(CardGameobject card)
        {
            Vector3 position = Vector3.zero;
            Vector3 rotation = Vector3.zero;
            card.GetComponent<Collider>().enabled = true;
            card.gameObject.transform.SetParent(playedCardPos.transform);
            card.MoveTo(position, rotation);
            card.IsInteractable = false;
        }
        public void DiscardCard(CardGameobject card)
        {
            discard.PutCardToSide(card);
            card.gameObject.GetComponent<Collider>().enabled = true;
        }

        public void RecoverCard(CardGameobject card)
        {
            recoveryInProgress = true;
            Vector3 position = Vector3.zero;
            Vector3 rotation = Vector3.zero;
            card.MoveTo(position, rotation);
            if (recoveryInProgress)
            {
                StartCoroutine(LetTheCardSlide(card)); 
            }
            card.gameObject.transform.SetParent(gameObject.transform);
            card.IsInteractable = true;
        }

        IEnumerator LetTheCardSlide(CardGameobject card)
        {
            yield return new WaitUntil(() => Vector3.Distance(card.transform.position, this.transform.position) <= cardToHandThreshold);
            recoveryInProgress = false;
        }

        public void UpdateGridLayout()
        {
            int numChildren = transform.childCount;

            // Calculate the total length of the row
            float totalLength = spacing * (numChildren - 1);

            // Calculate the starting position
            Vector3 startPosition = -Vector3.right * totalLength / 2f;

            // Position the child objects in a straight row
            for (int i = 0; i < numChildren; i++)
            {
                Vector3 newPosition = startPosition + Vector3.right * i * spacing;
                Transform child = transform.GetChild(i);
                Lexankorttitempupu lexankorttitempupu = child.GetComponent<Lexankorttitempupu>();

                // Adjust position smoothly if the object is highlighted
                if (lexankorttitempupu.IsHighlighted)
                {
                    // Gradually move towards the highlighted position
                    newPosition += Vector3.up * Mathf.Lerp(child.localPosition.y, yOffset, Time.deltaTime * smoothness);
                }
                else
                {
                    // Gradually move towards the default position
                    newPosition -= Vector3.up * Mathf.Lerp(child.localPosition.y, 0, Time.deltaTime * smoothness);
                }

                // Update the position
                child.localPosition = newPosition;
            }
        }

        /*public void UpdateSpecialGridLayout()
        {
            int numChildren = transform.childCount;

            // Calculate the total length of the row
            float totalLength = spacing * (numChildren - 1);

            // Calculate the starting position
            Vector3 startPosition = -Vector3.right * totalLength / 2f;

            // Position the child objects in a straight row
            for (int i = 0; i < numChildren; i++)
            {
                Vector3 newPosition = startPosition + Vector3.right * i * spacing;
                Transform child = transform.GetChild(i);
                Lexankorttitempupu lexankorttitempupu = child.GetComponent<Lexankorttitempupu>();

                // Adjust position smoothly if the object is highlighted
                if (lexankorttitempupu.IsHighlighted)
                {
                    // Gradually move towards the highlighted position
                    newPosition += Vector3.forward * Mathf.Lerp(child.localPosition.z, zOffset, Time.deltaTime * smoothness);
                }
                else
                {
                    // Gradually move towards the default position
                    newPosition -= Vector3.forward * Mathf.Lerp(child.localPosition.z, 0, Time.deltaTime * smoothness);
                }

                // Update the position
                child.localPosition = newPosition;
            }
        }*/

        public void HandColliderManager()
        {
            if (spacing < 0.18f)
            {
                handCollider.enabled = true;
                boingBlocker.enabled = false;

                foreach (Transform child in gameObject.transform)
                {
                    Collider childCollider = child.GetComponent<Collider>();

                    if (childCollider != null)
                    {
                        childCollider.enabled = false;
                    }
                }
            }
            else
            {
                handCollider.enabled = false;
                boingBlocker.enabled = true;

                foreach (Transform child in gameObject.transform)
                {
                    Collider childCollider = child.GetComponent<Collider>();

                    if (childCollider != null)
                    {
                        childCollider.enabled = true;
                    }
                }
            }

            if (gameObject.transform.childCount == 0 || gp.PhaseGame == GameplayManager.GamePhase.Blocked)
            {
                handCollider.enabled = false;
                boingBlocker.enabled = false;
            }
        }
    }

    public class PlayerHand : HandFunction, IInteractable
    {
        public Vector3 Point { get; set; }
        public UnityEvent OnClickEvent;
        public UnityEvent OnHoverEnterEvent;
        public UnityEvent OnHoverExitEvent;
        private GameObject currentCard;
        private GameObject previousCard;
        private CameraControl cam;

        [SerializeField] private float movementSpeed = 1f;
        [SerializeField] private float rotationSpeed = 4f;
        [SerializeField] private GameObject defaultHandPos;
        [SerializeField] private GameObject tableHandPos;
        [SerializeField] private GameObject playInProgressPos;
        [SerializeField] private GameObject cameraHub;

        public void Start()
        {
            cam = cameraHub.GetComponent<CameraControl>();
            UpdateGridLayout();
        }
        
        public void Update()
        {
            // MoveCamera();
            if (!recoveryInProgress)
            {
                UpdateGridLayout();
            }
            MoveHand();
            HandColliderManager();
        }

        void MoveHand()
        {
            Quaternion handRotation = defaultHandPos.transform.rotation;
            Vector3 handPosition = defaultHandPos.transform.position;
            float targetSpacing = 0f;

            if (cam.currentMode == CameraMode.Down)
            {
                targetSpacing = 0f;
                spacing = Mathf.Lerp(spacing, targetSpacing, spacingLerpSpeed * Time.deltaTime);
                if (spacing <= 0.0002f)
                {
                    spacing = 0f;
                }
                handPosition = tableHandPos.transform.position;
                handRotation = tableHandPos.transform.rotation;
            }
            else if (cam.currentMode == CameraMode.Cutscene)
            {
                handPosition = playedCardPos.transform.position;
                handRotation = playedCardPos.transform.rotation;
            }
            else if (cheeseInput.CheeseAlert())
            {
                targetSpacing = 0f;
                spacing = Mathf.Lerp(spacing, targetSpacing, spacingLerpSpeed * Time.deltaTime);
                if (spacing <= 0.0002f)
                {
                    spacing = 0f;
                }
                handPosition = cheeseHandPos.transform.position;
                handRotation = cheeseHandPos.transform.rotation;
            }
            else if (gp.PhaseGame == GameplayManager.GamePhase.Blocked || cam.currentMode == CameraMode.PlayedCard)
            {
                targetSpacing = 0f;
                spacing = Mathf.Lerp(spacing, targetSpacing, spacingLerpSpeed * Time.deltaTime);
                if (spacing <= 0.0002f)
                {
                    spacing = 0f;
                }
                handPosition = playInProgressPos.transform.position;
                handRotation = playInProgressPos.transform.rotation;
            }
            else
            {
                targetSpacing = 0.18f;
                spacing = Mathf.Lerp(spacing, targetSpacing, spacingLerpSpeed * Time.deltaTime);
                if (spacing >= 0.179f)
                {
                    spacing = 0.18f;
                }
            }

            // Smoothly interpolate to the target position
            transform.position = Vector3.Lerp(transform.position, handPosition, movementSpeed * Time.deltaTime);
            
            // Smoothly interpolate to the target rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, handRotation, rotationSpeed * Time.deltaTime);
        }

        public void NormalHandActive()
        {
            cam.SetCameraMode(CameraMode.Original);
        }

        /*public void PlayCard(CardGameobject card, GameObject cardPos)
        {
            Transform newPos = cardPos.transform;
            Vector3 position = newPos.position;
            Vector3 rotation = newPos.eulerAngles;
            RemoveChildFromGrid(card.gameObject);
            UpdateGridLayout();
            card.MoveTo(position, rotation);
            cam.SetCameraMode(CameraMode.PlayedCard);
        }*/

        void MoveCamera()
        {
            // Check for left arrow key press and change camera mode
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Set the camera mode to Left
                cam.SetCameraMode(CameraMode.RecoverCard);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Set the camera mode Right
                cam.SetCameraMode(CameraMode.Right);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Set the camera mode Down
                cam.SetCameraMode(CameraMode.Down);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                cam.SetCameraMode(CameraMode.Devil);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                cam.SetCameraMode(CameraMode.FiendLeft);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                cam.SetCameraMode(CameraMode.FiendRight);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                cam.SetCameraMode(CameraMode.RoundIndicator);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                cam.SetCameraMode(CameraMode.Cutscene);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                cam.SetCameraMode(CameraMode.Original);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                cam.SetCameraMode(CameraMode.PlayedCard);
            }
        }

        public void OnHoverEnter()
        {
            if (GameManager.InGameState == InGameState.CardGameIn && cam.currentMode == CameraMode.Down)
            {
                OnHoverEnterEvent.Invoke();
            }
        }

        public void OnHoverExit()
        {
            if (GameManager.InGameState == InGameState.CardGameIn)
            {
                OnHoverExitEvent.Invoke();
            }
        }

        public void OnClickStart()
        {
            if (GameManager.InGameState == InGameState.CardGameIn)
            {
                OnClickEvent.Invoke();
            }
        }

        public void OnClickRelease()
        {

        }
    }
}
