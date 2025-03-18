using UnityEngine;
using Photon.Pun;
using System;

public class PlayerResourceManager : MonoBehaviourPunCallbacks
{
    public float playerHealth = 1000f;
    public float playerStamina = 1000f;

    public float playerMaxHealth = 1000;
    public float healthRegenRate = 10f;

    public float playerMaxStamina = 1000;
    private float staminaRegenRate = 2f;

    public float attackStaminaCost = 70f;
    public float sprintStaminaCost = 60f;
    public float dashStaminaCost = 140f;
    public float evadeStaminaCost = 210f;


    // Declare an event that subscribers can listen to.
    // Observer pattern in Unity
    public event Action<float> OnHealthChanged;
    public event Action<float> OnStaminaChanged;

    public float PlayerHealth
    {
        get => playerHealth;
        set
        {
            if (!Mathf.Approximately(playerHealth, value))
            {
                playerHealth = value;
                OnHealthChanged?.Invoke(playerHealth);
            }
        }
    }
    public float PlayerStamina
    {
        get => playerStamina;
        set
        {
            if (!Mathf.Approximately(playerStamina, value))
            {
                playerStamina = value;
                OnStaminaChanged?.Invoke(playerStamina);
            }
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            RegenerateResources();
        }
    }

    private void RegenerateResources()
    {
        float delta = Time.deltaTime;
        bool updated = false;

        if (playerHealth < playerMaxHealth)
        {
            playerHealth += healthRegenRate * delta;
            playerHealth = Mathf.Min(playerHealth, playerMaxHealth);
            updated = true;
        }

        if (playerStamina < playerMaxStamina)
        {
            playerStamina += staminaRegenRate;
            playerStamina = Mathf.Min(playerStamina, playerMaxStamina);
            updated = true;
        }

        if (updated)
        {
            photonView.RPC("RPC_UpdateResources", RpcTarget.Others, playerHealth, playerStamina);
        }
    }

    // Animator events
    public void UseStamina_Attack()
    {
        playerStamina -= attackStaminaCost;
    }

    public void UseStamina_Dash()
    {
        playerStamina -= dashStaminaCost;
    }

    public void UseStamina_Evade()
    {
        playerStamina -= evadeStaminaCost;
    }
    public void UseStamina_Sprint()
    {
        playerStamina -= sprintStaminaCost;
    }


    [PunRPC]
    void RPC_TakeDamage(float damageAmount)
    {
        playerHealth -= damageAmount;
        playerHealth = Mathf.Max(playerHealth, 0);
    }

    [PunRPC]
    void RPC_UpdateResources(float newHealth, float newStamina)
    {
        playerHealth = newHealth;
        playerStamina = newStamina;
    }
}
