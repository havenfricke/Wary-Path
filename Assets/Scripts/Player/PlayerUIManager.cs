using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    private PlayerResourceManager resourceManager;
    private PhotonView photonView;
    private PlayerMovement playerMovement;
    private EquippedItemsAccessor equippedItemsAccessor;

    public Slider healthUIBar;
    public Slider staminaUIBar;
    public float smoothingFactor = 5f;

    public bool isMenuOpen = false;

    // For UI navigation.
    private int selectedIndex = 0;
    private float inputCooldown = 0.3f;
    private float currentInputDelay = 0f;

    // Flags to track input state.
    private bool axisHeld = false;
    private bool selectHeld = false;

    InventoryItemIdReference selectedItemIdRef;

    void Awake()
    {
        // Get the PhotonView from the parent.
        photonView = GetComponentInParent<PhotonView>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        equippedItemsAccessor = GetComponentInParent<EquippedItemsAccessor>();

        // Only enable UI for the local player.
        if (photonView != null && !photonView.IsMine)
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        resourceManager = GetComponentInParent<PlayerResourceManager>();
    }

    void Update()
    {
        // Update health and stamina bars.
        if (resourceManager != null)
        {
            UpdateUIBars();
        }

        // Toggle the menu when requested.
        if (playerMovement.menuInput)
        {
            HandleMenuToggle();
        }

        // Decrease the input delay timer.
        if (currentInputDelay > 0)
        {
            currentInputDelay -= Time.deltaTime;
        }
    }

    private void UpdateUIBars()
    {
        if (healthUIBar != null)
        {
            float targetHealth = resourceManager.PlayerHealth / resourceManager.playerMaxHealth;
            healthUIBar.value = Mathf.Lerp(healthUIBar.value, targetHealth, smoothingFactor * Time.deltaTime);
        }
        if (staminaUIBar != null)
        {
            float targetStamina = resourceManager.PlayerStamina / resourceManager.playerMaxStamina;
            staminaUIBar.value = Mathf.Lerp(staminaUIBar.value, targetStamina, smoothingFactor * Time.deltaTime);
        }
    }

    private void HandleMenuToggle()
    {
        // Toggle the menu state.
        isMenuOpen = !isMenuOpen;
        menu.SetActive(isMenuOpen);
        Debug.Log("Menu toggled. isMenuOpen: " + isMenuOpen);

        if (isMenuOpen)
        {
            selectedIndex = 0;
            Button[] menuButtons = menu.GetComponentsInChildren<Button>();
            if (menuButtons.Length > 0)
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(menuButtons[selectedIndex].gameObject);
                    Debug.Log("Selected button: " + menuButtons[selectedIndex].name);
                }
                else
                {
                    Debug.LogWarning("EventSystem.current is null! Ensure you have an EventSystem in your scene.");
                }
            }
            else
            {
                Debug.LogWarning("No buttons found in the menu.");
            }
        }
        else
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void HandleInputForMenu(Vector2 input, bool select)
    {
        // Only process input if the menu is open.
        if (!isMenuOpen)
            return;

        // If still in cooldown, do not process new input.
        if (currentInputDelay > 0)
            return;

        // Process the select input on its rising edge.
        if (select)
        {
            if (!selectHeld)
            {
                Button[] menuButtons = menu.GetComponentsInChildren<Button>();
                if (menuButtons.Length == 0)
                    return;

                menuButtons[selectedIndex].onClick.Invoke();

                // Instead of only invoking onClick, directly call EquipItem with the selected ID.
                if (menuButtons[selectedIndex].TryGetComponent<InventoryItemIdReference>(out selectedItemIdRef))
                {
                    Debug.Log("Selected item ID: " + selectedItemIdRef.itemIdReference + " [PLAYERUIMANAGER]");
                    equippedItemsAccessor.EquipItem(selectedItemIdRef.itemIdReference);
                }
                else
                {
                    Debug.LogWarning("No InventoryItemIdReference found on the selected button.");
                }

                selectHeld = true;
                currentInputDelay = inputCooldown;
                return;
            }
        }
        else
        {
            // Reset the selectHeld flag once the select input is released.
            selectHeld = false;
        }

        // Determine the dominant axis for navigation.
        float absVertical = Mathf.Abs(input.y);
        float absHorizontal = Mathf.Abs(input.x);
        bool navigationPressed = absVertical > 0.5f || absHorizontal > 0.5f;

        if (navigationPressed && !axisHeld)
        {
            Button[] menuButtons = menu.GetComponentsInChildren<Button>();
            if (menuButtons.Length == 0)
                return;

            if (absVertical >= absHorizontal)
            {
                // Vertical navigation.
                if (input.y < 0)
                    selectedIndex = (selectedIndex + 1) % menuButtons.Length;
                else if (input.y > 0)
                    selectedIndex = (selectedIndex - 1 + menuButtons.Length) % menuButtons.Length;
            }
            else
            {
                // Horizontal navigation.
                if (input.x < 0)
                    selectedIndex = (selectedIndex + 1) % menuButtons.Length;
                else if (input.x > 0)
                    selectedIndex = (selectedIndex - 1 + menuButtons.Length) % menuButtons.Length;
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(menuButtons[selectedIndex].gameObject);
                Debug.Log("New selected button: " + menuButtons[selectedIndex].name);
            }

            if (menuButtons[selectedIndex].TryGetComponent<InventoryItemIdReference>(out selectedItemIdRef))
            {
                // Update the equip system on hover.
                equippedItemsAccessor.SetSelectedItemId(selectedItemIdRef.itemIdReference);
                Debug.Log("Hovered over item ID: " + selectedItemIdRef.itemIdReference + " [PLAYERUIMANAGER]");
            }
            else
            {
                equippedItemsAccessor.SetSelectedItemId(null);
            }

            axisHeld = true;
            currentInputDelay = inputCooldown;
        }
        else if (!navigationPressed)
        {
            axisHeld = false;
        }
    }
}
