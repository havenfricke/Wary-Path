using Photon.Pun;
using UnityEngine;

public class FixCameraToPlayer : MonoBehaviour
{
    private Transform playerTransform;
    private PlayerControls playerControls;

    [Header("Camera Settings")]
    [SerializeField]
    private float distanceFromPlayer = 5f;
    [SerializeField]
    private float rotateSpeed = 1f;       // Rotation speed multiplier
    [SerializeField]
    private float smoothSpeed = 10f;      // Smoothing speed (higher = faster interpolation)
    [SerializeField]
    private float height = 2f;            // Height offset above the player

    private float currentYaw;
    private float currentPitch;

    private bool isLockOnActive = false;
    private Vector3 lockOnOrigin = Vector3.zero;

    private Quaternion currentRotation;
    private Quaternion targetRotation;

    private void OnEnable()
    {

        playerControls = new PlayerControls();
        playerControls.Enable();

        currentRotation = transform.rotation;

        // Try to find a local player immediately (if it exists)
        if (playerTransform == null)
        {
            FindLocalPlayer();
        }
    }

    private void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.Disable();
        }

    }

    private void Update()
    {
        // Fallback: if we lost our player reference, try to find one.
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            FindLocalPlayer();
        }

        Look();
    }

    private void FindLocalPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                playerTransform = p.transform;
                break;
            }
        }
    }

    private void HandleLocalPlayerSpawned(GameObject newPlayer)
    {
        // When a new local player is spawned, update the reference.
        PhotonView pv = newPlayer.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            playerTransform = newPlayer.transform;
        }
    }

    private void Look()
    {
        if (isLockOnActive)
        {
            LockOnLook();
        }
        else
        {
            Vector2 inputLook = playerControls.Player.Look.ReadValue<Vector2>();

            currentYaw += inputLook.x * rotateSpeed;
            currentPitch -= inputLook.y * rotateSpeed;
            currentPitch = Mathf.Clamp(currentPitch, -60f, 60f);

            targetRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            // Smoothly interpolate rotation.
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
        }

        RotateCamera();
        FollowPlayer();
    }

    private void LockOnLook()
    {
        Vector3 directionToLockOn = lockOnOrigin - transform.position;
        targetRotation = Quaternion.LookRotation(directionToLockOn);
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
    }

    private void RotateCamera()
    {
        transform.rotation = currentRotation;
    }

    private void FollowPlayer()
    {
        if (playerTransform == null)
            return;

        // Calculate desired position relative to the player's position.
        Vector3 targetPosition = playerTransform.position + Vector3.up * height - transform.forward * distanceFromPlayer;
        // Smoothly move the camera toward the target position.
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        Debug.DrawLine(playerTransform.position, targetPosition, Color.red);
    }

    public void HandleLockOnValues(Vector3 closestLockOnOrigin, bool isLockedOn)
    {
        isLockOnActive = isLockedOn;
        lockOnOrigin = closestLockOnOrigin;
    }
}
