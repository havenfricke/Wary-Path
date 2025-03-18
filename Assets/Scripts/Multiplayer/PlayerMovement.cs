using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using ExitGames.Client.Photon;

public class PlayerMovement : MonoBehaviourPun, IPunObservable, IInRoomCallbacks
{
    // Components and input
    private CharacterController cc;
    private PlayerControls playerControls;
    private PlayerAnimatorManager playerAnimatorManager;
    private InventoryManager inventoryManager;
    private PlayerResourceManager playerResourceManager;
    private PlayerUIManager playerUIManager;

    // Movement & locking
    private Vector3 velocity; // Used for vertical/gravity movement
    private bool isGrounded;
    private bool isLockedOn = false;
    private List<LockOnOrigin> lockOnOrigins;
    private Vector3 closestLockOnOrigin;
    private LockOnOrigin lockedOnOrigin;
    public bool canLockOn = false;
    private float lockOnRadius = 15f;

    // Input Actions for switching targets
    public InputAction NextTarget;
    public InputAction PreviousTarget;

    private Camera playerCamera;
    private FixCameraToPlayer playerCameraScript;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    public float rotationSpeed = 10f;

    public bool menuInput;

    // Networked input parameters (for animation, etc.)
    private Vector2 _moveInput;
    public bool sprintInput;
    public bool evadeInput;
    private bool _lockOnInput;
    public bool attackInput;
    public bool dashInput;

    // PhotonTransformView component reference (added via Inspector)
    private PhotonTransformView photonTransformView;



    private void Awake()
    {
        // Set higher send/serialization rates if desired
        PhotonNetwork.SendRate = 100;
        PhotonNetwork.SerializationRate = 100;

        playerControls = new PlayerControls();
        lockOnOrigins = new List<LockOnOrigin>();
        closestLockOnOrigin = Vector3.zero;

        playerAnimatorManager = GetComponent<PlayerAnimatorManager>();

        photonTransformView = GetComponent<PhotonTransformView>();

        inventoryManager = GetComponent<InventoryManager>();

        playerResourceManager = GetComponent<PlayerResourceManager>();

        playerUIManager = GetComponentInChildren<PlayerUIManager>();

    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        if (photonView.IsMine)
        {
            AlignWithGround();
        }
        else
        {
            // disable other players cc so network can sync
            cc.enabled = false;
        }

        playerCamera = Camera.main;
        if (playerCamera != null)
            playerCameraScript = playerCamera.GetComponent<FixCameraToPlayer>();

    }


    private void OnEnable()
    {
        playerControls.Enable();
        PhotonNetwork.AddCallbackTarget(this);

        // Subscribe to target switching events here to avoid duplicate subscriptions
        playerControls.Player.Next.performed += OnNextTarget;
        playerControls.Player.Previous.performed += OnPreviousTarget;
    }

    private void OnDisable()
    {
        playerControls.Disable();
        PhotonNetwork.RemoveCallbackTarget(this);

        // Unsubscribe from target switching events
        playerControls.Player.Next.performed -= OnNextTarget;
        playerControls.Player.Previous.performed -= OnPreviousTarget;
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
                playerCameraScript = playerCamera.GetComponent<FixCameraToPlayer>();
        }

        HandleLockOnVicinity();

        if (photonView.IsMine)
        {
            MovePlayer();
        }
        // Remote players’ transforms will be updated via PhotonTransformView

        if (playerCameraScript != null)
            playerCameraScript.HandleLockOnValues(closestLockOnOrigin, isLockedOn);
    }

    private void MovePlayer()
    {
        // character controller is disabled on death events
        // if dead we don't want to be able to move.
        if(cc.enabled)
        {
            // Only the local player should process movement.
            if (!photonView.IsMine || cc == null)
                return;

            // Read menu input
            menuInput = playerControls.Player.OpenMenu.triggered;

            // Handle input and return if menu open
            // If the menu is open, cancel movement.
            if (playerUIManager.isMenuOpen)
            {
                _moveInput = Vector2.zero;
                velocity = Vector3.zero;
                playerAnimatorManager.ReadInputValues(0f, 0f, false, false, false, false, false);
                playerUIManager.HandleInputForMenu(
                    playerControls.Player.Move.ReadValue<Vector2>(),
                    playerControls.Player.Dash.triggered
                    );
                return;
            }

            // Read movement and action inputs
            Vector2 moveInput = playerControls.Player.Move.ReadValue<Vector2>();
            bool sprintInput = playerControls.Player.Sprint.triggered;
            bool evadeInput = playerControls.Player.Evade.triggered;
            bool lockOnInput = playerControls.Player.LockOn.triggered;
            bool attackInput = playerControls.Player.Attack.IsPressed();
            bool dashInput = playerControls.Player.Dash.triggered;
            bool interactInput = playerControls.Player.Interact.triggered;

            // prevent actions if stamina low
            if (sprintInput && playerResourceManager.playerStamina < playerResourceManager.sprintStaminaCost)
            {
                sprintInput = false;
            };
            if (evadeInput && playerResourceManager.playerStamina < playerResourceManager.evadeStaminaCost) 
            { 
                evadeInput = false;
            };
            if (dashInput && playerResourceManager.playerStamina < playerResourceManager.dashStaminaCost)
            {
                dashInput = false;
            };
            if (attackInput && playerResourceManager.playerStamina < playerResourceManager.attackStaminaCost)
            {
                attackInput = false;
            };

            _moveInput = moveInput;
            this.sprintInput = sprintInput;
            this.evadeInput = evadeInput;
            _lockOnInput = lockOnInput;
            this.attackInput = attackInput;
            this.dashInput = dashInput;

            // Lock-on logic
            if (lockOnInput && !isLockedOn && canLockOn)
            {
                isLockedOn = true;
                lockedOnOrigin = FindClosestLockOnOrigin();
                closestLockOnOrigin = lockedOnOrigin ? lockedOnOrigin.transform.position : Vector3.zero;
            }
            else if (lockOnInput && isLockedOn)
            {
                isLockedOn = false;
                lockedOnOrigin = null;
            }

            // Update animator with input values
            playerAnimatorManager.ReadInputValues(
                moveInput.x,
                moveInput.y,
                sprintInput,
                evadeInput,
                isLockedOn,
                attackInput,
                dashInput
            );

            if(interactInput)
            {
                inventoryManager.AddLootableToInventory();
            }

            // Determine move direction based on camera orientation
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector3 moveDirection = right * moveInput.x + forward * moveInput.y;

            // Rotate the player toward the movement direction
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Apply gravity and vertical velocity
            isGrounded = cc.isGrounded;
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;
            if (!isGrounded)
                velocity.y += gravity * Time.deltaTime;

            // Calculate final movement velocity (horizontal + vertical)
            Vector3 currentVelocity = moveDirection * moveSpeed + new Vector3(0, velocity.y, 0);

            // Move the player via the CharacterController
            cc.Move(currentVelocity * Time.deltaTime);
        }
    }

    private void HandleLockOnVicinity()
    {
        lockOnOrigins.Clear();
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRadius);
        LockOnOrigin closestLockOn = null;
        float closestDistance = float.MaxValue;

        // Cache our own LockOnOrigin component (which should be on the dedicated child)
        LockOnOrigin myLockOnOrigin = GetComponentInChildren<LockOnOrigin>();

        foreach (Collider collider in colliders)
        {
            // Ignore our own colliders (comparing root objects)
            if (collider.transform.root == transform.root)
                continue;

            // Get the LockOnOrigin component from the target player's prefab (child object)
            LockOnOrigin potentialTarget = collider.transform.root.GetComponentInChildren<LockOnOrigin>();
            if (potentialTarget != null)
            {
                // Avoid picking up our own lock-on component if it somehow gets detected
                if (potentialTarget == myLockOnOrigin)
                    continue;

                // Skip targets that belong to the local player
                PhotonView targetPV = potentialTarget.GetComponentInParent<PhotonView>();
                if (targetPV != null && targetPV.IsMine)
                    continue;

                // Add the potential target for lock-on
                lockOnOrigins.Add(potentialTarget);
                float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLockOn = potentialTarget;
                }
            }
        }

        canLockOn = lockOnOrigins.Count > 0;
        if (isLockedOn && lockedOnOrigin != null)
        {
            // Continuously update the lock-on target's position while locked on.
            closestLockOnOrigin = lockedOnOrigin.transform.position;
        }
        else if (!isLockedOn && closestLockOn != null)
        {
            closestLockOnOrigin = closestLockOn.transform.position;
        }
    }


    private LockOnOrigin FindClosestLockOnOrigin()
    {
        LockOnOrigin closestTarget = null;
        float minDistance = float.MaxValue;
        foreach (LockOnOrigin target in lockOnOrigins)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestTarget = target;
            }
        }
        return closestTarget;
    }

    private void OnNextTarget(InputAction.CallbackContext context)
    {
        if (lockOnOrigins.Count == 0 || lockedOnOrigin == null)
            return;

        int currentIndex = lockOnOrigins.IndexOf(lockedOnOrigin);
        if (currentIndex == -1)
            return;

        int nextIndex = (currentIndex + 1) % lockOnOrigins.Count;
        lockedOnOrigin = lockOnOrigins[nextIndex];
        closestLockOnOrigin = lockedOnOrigin.transform.position;
    }

    private void OnPreviousTarget(InputAction.CallbackContext context)
    {
        if (lockOnOrigins.Count == 0 || lockedOnOrigin == null)
            return;

        int currentIndex = lockOnOrigins.IndexOf(lockedOnOrigin);
        if (currentIndex == -1)
            return;

        int prevIndex = (currentIndex - 1 + lockOnOrigins.Count) % lockOnOrigins.Count;
        lockedOnOrigin = lockOnOrigins[prevIndex];
        closestLockOnOrigin = lockedOnOrigin.transform.position;
    }

    /// <summary>
    /// Adjusts the local player's position to align with the ground.
    /// </summary>
    private void AlignWithGround()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 10f))
        {
            float bottomOffset = cc.center.y - cc.height * 0.5f;
            Vector3 newPos = transform.position;
            newPos.y = hit.point.y - bottomOffset;
            transform.position = newPos;
        }
    }

    // INITIAL TRANSFORM SYNC USING RPC TO NEW PLAYERS

    /// <summary>
    /// When a new player joins the room, send the current transform to that player.
    /// </summary>
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            // Send the current transform to the new player.
            photonView.RPC("RPC_SyncPositionToPlayer", newPlayer, transform.position, transform.rotation);
        }
    }

    [PunRPC]
    private void RPC_SyncPositionToPlayer(Vector3 pos, Quaternion rot, PhotonMessageInfo info)
    {
        // This RPC is sent to a new player so they can update the transform of an already instantiated player.
        transform.position = pos;
        transform.rotation = rot;
    }

    // IPunObservable - Not used here because PhotonTransformView handles syncing.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // The PhotonTransformView component automatically handles serialization.
    }

    // IInRoomCallbacks required methods
    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnJoinedRoom() { }
    public void OnLeftRoom() { }
    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }
}
