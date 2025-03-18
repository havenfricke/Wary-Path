using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerAnimatorManager : MonoBehaviourPun, IPunObservable
{
    private Animator animator;
    private CharacterController characterController;
    private PlayerResourceManager resourceManager;
    private LootSpawner spawner;
    

    const float resetEvadeTime = 0.20f;

    // for death and respawn events
    public bool enableRagdoll = false;  
    public bool disableRagdoll = false; 

    [Header("Player States")]
    public bool isMoving = false;
    public bool isWalking = false;
    public bool isRunning = false;
    public bool isSprinting = false;
    public bool isEvading = false;
    public bool canEvade = true;
    public bool isAttacking = false;
    public bool isDashing = false;
    public bool isLockedOn = false;

    private void Awake()
    {
        PhotonNetwork.SendRate = 100;
        PhotonNetwork.SerializationRate = 100;

        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        resourceManager = GetComponent<PlayerResourceManager>();
        spawner = GetComponent<LootSpawner>();

    }

    private void Update()
    {
        CommenceDeath(); // it could happen at any time
    }

    /// <summary>
    /// Called by the local player to update animation states based on input.
    /// This method also sends an RPC to synchronize states on remote clients.
    /// </summary>
    public void ReadInputValues(float x, float y, bool sprintInput, bool evadeInput, bool isLockedOnActive, bool attackInput, bool dashInput)
    {
        float snappedHorizontal = 0f;
        float snappedVertical = 0f;

        isMoving = (Mathf.Abs(x) > 0 || Mathf.Abs(y) > 0);
        isAttacking = attackInput;
        isDashing = dashInput;

        // Horizontal movement logic.
        if (x > 0 && x <= 0.5f)
        {
            isWalking = true;
            isRunning = false;
            snappedHorizontal = 0.5f;
        }
        else if (x > 0.5f && x <= 1)
        {
            isRunning = true;
            isWalking = false;
            snappedHorizontal = 1f;
        }
        else if (x < 0 && x >= -0.5f)
        {
            isWalking = true;
            isRunning = false;
            snappedHorizontal = -0.5f;
        }
        else if (x < -0.5f && x >= -1)
        {
            isRunning = true;
            isWalking = false;
            snappedHorizontal = -1f;
        }
        else
        {
            isMoving = false;
            snappedHorizontal = 0f;
        }

        // Vertical movement logic.
        if (y > 0 && y <= 0.5f)
        {
            isWalking = true;
            snappedVertical = 0.5f;
        }
        else if (y > 0.5f && y <= 1)
        {
            isRunning = true;
            isWalking = false;
            snappedVertical = 1f;
        }
        else if (y < 0 && y >= -0.5f)
        {
            isWalking = true;
            snappedVertical = -0.5f;
        }
        else if (y < -0.5f && y >= -1)
        {
            isRunning = true;
            isWalking = false;
            snappedVertical = -1f;
        }
        else
        {
            isMoving = false;
            snappedVertical = 0f;
        }

        // Process evading.
        if (evadeInput && canEvade)
        {
            isEvading = true;
            canEvade = false;
            StartCoroutine(ResetEvade(resetEvadeTime));
        }

        // Toggle sprinting.
        if (sprintInput && isSprinting)
        {
            isSprinting = false;
        }
        else if (sprintInput && !isSprinting)
        {
            isSprinting = true;
        }
        else if (x == 0 || y == 0)
        {
            isSprinting = false;
        }
        else if (resourceManager.playerStamina < resourceManager.sprintStaminaCost)
        {
            isSprinting = false;
        }

        // Update lock-on state.
        isLockedOn = isLockedOnActive;

        // If no movement, reset movement states.
        if (x == 0 || y == 0)
        {
            isMoving = false;
            isWalking = false;
            isRunning = false;
            isSprinting = false;
        }

        // Synchronize animation state via RPC to remote clients.
        if (photonView.IsMine)
        {
            photonView.RPC("SyncAnimationStates", RpcTarget.Others, isMoving, isWalking, isRunning, isSprinting, isEvading, isAttacking, isDashing, isLockedOn, snappedHorizontal, snappedVertical);
        }

        // Update local animator parameters.
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isSprinting", isSprinting);
        animator.SetBool("isEvading", isEvading);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isDashing", isDashing);
        animator.SetFloat("xInput", snappedHorizontal);
        animator.SetFloat("yInput", snappedVertical);
    }

    /// <summary>
    /// Called by the movement script on remote clients to update animation parameters.
    /// </summary>
    public void UpdateRemoteAnimation(float remoteMoveX, float remoteMoveY, bool remoteSprint, bool remoteEvade, bool remoteAttack, bool remoteDash)
    {
        // Determine snapped values and movement states based on remote inputs.
        float snappedHorizontal = 0f;
        float snappedVertical = 0f;
        bool remoteWalking = false;
        bool remoteRunning = false;
        bool remoteMoving = (Mathf.Abs(remoteMoveX) > 0 || Mathf.Abs(remoteMoveY) > 0);

        if (remoteMoveX > 0 && remoteMoveX <= 0.5f)
        {
            remoteWalking = true;
            snappedHorizontal = 0.5f;
        }
        else if (remoteMoveX > 0.5f && remoteMoveX <= 1f)
        {
            remoteRunning = true;
            snappedHorizontal = 1f;
        }
        else if (remoteMoveX < 0 && remoteMoveX >= -0.5f)
        {
            remoteWalking = true;
            snappedHorizontal = -0.5f;
        }
        else if (remoteMoveX < -0.5f && remoteMoveX >= -1f)
        {
            remoteRunning = true;
            snappedHorizontal = -1f;
        }
        else
        {
            snappedHorizontal = 0f;
        }

        if (remoteMoveY > 0 && remoteMoveY <= 0.5f)
        {
            remoteWalking = true;
            snappedVertical = 0.5f;
        }
        else if (remoteMoveY > 0.5f && remoteMoveY <= 1f)
        {
            remoteRunning = true;
            snappedVertical = 1f;
        }
        else if (remoteMoveY < 0 && remoteMoveY >= -0.5f)
        {
            remoteWalking = true;
            snappedVertical = -0.5f;
        }
        else if (remoteMoveY < -0.5f && remoteMoveY >= -1f)
        {
            remoteRunning = true;
            snappedVertical = -1f;
        }
        else
        {
            snappedVertical = 0f;
        }

        // Update the animator with remote values.
        animator.SetBool("isMoving", remoteMoving);
        animator.SetBool("isWalking", remoteWalking);
        animator.SetBool("isRunning", remoteRunning);
        animator.SetBool("isSprinting", remoteSprint);
        animator.SetBool("isEvading", remoteEvade);
        animator.SetBool("isAttacking", remoteAttack);
        animator.SetBool("isDashing", remoteDash);
        animator.SetFloat("xInput", snappedHorizontal);
        animator.SetFloat("yInput", snappedVertical);
    }

    private IEnumerator ResetEvade(float time)
    {
        yield return new WaitForSeconds(time);
        isEvading = false;
        StartCoroutine(ResetEvadeAbility(0.7f));
    }

    private IEnumerator ResetEvadeAbility(float time)
    {
        yield return new WaitForSeconds(time);
        canEvade = true;
    }

    public void CommenceDeath()
    {
        if (resourceManager.playerHealth < 1)
        {
            // Only execute respawn logic on the owning client.
            if (photonView.IsMine)
            {
                resourceManager.healthRegenRate = 0;
                resourceManager.playerHealth = 0;

                if (!enableRagdoll)
                {
                    enableRagdoll = true;
                    photonView.RPC("RPC_SetRagdoll", RpcTarget.All, true);

                    // Start respawn routine only on the local player's instance.
                    StartCoroutine(Respawn());
                    spawner.debug = true;
                }
            }
        }
    }


    public void EnableRagdoll()
    {
        characterController.enabled = false;
        animator.enabled = false;
    }

    public void DisableRagdoll()
    {
        characterController.enabled = true;
        animator.enabled = true;
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(10);
        // Call the respawn method in the PlayerSpawner.
        PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null)
        {
            spawner.RespawnPlayer();
        }
        else
        {
            Debug.LogError("PlayerSpawner not found in scene!");
        }

        // Reset resource values for this client (if applicable).
        resourceManager.playerHealth = resourceManager.playerMaxHealth;
        resourceManager.healthRegenRate = 10;
        disableRagdoll = true;
    }


    /// <summary>
    /// RPC to toggle ragdoll state across the network.
    /// </summary>
    [PunRPC]
    private void RPC_SetRagdoll(bool enable)
    {
        if (enable)
        {
            EnableRagdoll();
        }
        else
        {
            DisableRagdoll();
        }
    }

    // RPC to synchronize animation states on remote clients.
    [PunRPC]
    void SyncAnimationStates(bool moving, bool walking, bool running, bool sprinting, bool evading, bool attacking, bool dashing, bool lockedOn, float xInput, float yInput)
    {
        isMoving = moving;
        isWalking = walking;
        isRunning = running;
        isSprinting = sprinting;
        isEvading = evading;
        isAttacking = attacking;
        isDashing = dashing;
        isLockedOn = lockedOn;
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isSprinting", isSprinting);
        animator.SetBool("isEvading", isEvading);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isDashing", isDashing);
        animator.SetFloat("xInput", xInput);
        animator.SetFloat("yInput", yInput);
    }

    // (Optional) Photon stream serialization if further precision is needed.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isMoving);
            stream.SendNext(isWalking);
            stream.SendNext(isRunning);
            stream.SendNext(isSprinting);
            stream.SendNext(isEvading);
            stream.SendNext(canEvade);
            stream.SendNext(isAttacking);
            stream.SendNext(isDashing);
        }
        else
        {
            isMoving = (bool)stream.ReceiveNext();
            isWalking = (bool)stream.ReceiveNext();
            isRunning = (bool)stream.ReceiveNext();
            isSprinting = (bool)stream.ReceiveNext();
            isEvading = (bool)stream.ReceiveNext();
            canEvade = (bool)stream.ReceiveNext();
            isAttacking = (bool)stream.ReceiveNext();
            isDashing = (bool)stream.ReceiveNext();
        }
    }
}
