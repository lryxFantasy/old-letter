using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public GameObject inventoryPanel;
    public InventoryManager inventoryManager;
    public Button inventoryButton;
    public PauseMenu pauseMenu;
    public Button inventoryCloseButton;
    private float previousTimeScale;

    void Start()
    {
        inventoryPanel.SetActive(false);
        previousTimeScale = Time.timeScale;

        if (inventoryButton != null)
        {
            inventoryButton.onClick.AddListener(ToggleInventory);
        }

        if (inventoryCloseButton != null)
        {
            inventoryCloseButton.onClick.AddListener(ToggleInventory);
        }
    }

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryManager.letterDetailPanel.activeSelf)
            {
                inventoryManager.letterDetailPanel.SetActive(false);
            }
            else if (inventoryPanel.activeSelf)
            {
                CloseInventory();
            }
        }
    }

    public void ToggleInventory()
    {
        if (PauseMenu.IsPaused)
            return;

        bool isActive = inventoryPanel.activeSelf;
        if (!isActive)
        {
            inventoryPanel.SetActive(true);
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            CloseInventory();
        }
    }

    void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        if (inventoryManager.letterDetailPanel.activeSelf)
        {
            inventoryManager.letterDetailPanel.SetActive(false);
        }
        Time.timeScale = previousTimeScale;
    }
}