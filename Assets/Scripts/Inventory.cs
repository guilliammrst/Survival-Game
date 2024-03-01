using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Panel References")]

    [SerializeField]
    private List<ItemData> items = new List<ItemData>();

    [SerializeField]
    private GameObject inventoryPanel;

    [SerializeField]
    private Transform inventorySlotsParent;

    [SerializeField]
    private Transform dropPoint;

    const int InventorySize = 24;

    [Header("Action Panel References")]

    [SerializeField]
    private GameObject actionPanel;

    [SerializeField]
    private GameObject useItemButton;

    [SerializeField]
    private GameObject equipItemButton;

    [SerializeField]
    private GameObject dropItemButton;

    [SerializeField]
    private GameObject destroyItemButton;

    private ItemData itemCurrentlySelected;
    
    [SerializeField]
    private Sprite emptySlotVisual;

    [Header("Equipment Panel References")]

    [SerializeField]
    private EquipmentLibrary equipmentLibrary;

    [SerializeField]
    private Image headSlotImage;

    [SerializeField]
    private Image chestSlotImage;

    [SerializeField]
    private Image handsSlotImage;

    [SerializeField]
    private Image legsSlotImage;

    [SerializeField]
    private Image feetSlotImage;

    private ItemData equipedHeadItem;
    private ItemData equipedChestItem;
    private ItemData equipedHandsItem;
    private ItemData equipedLegsItem;
    private ItemData equipedFeetItem;

    [SerializeField]
    private Button headSlotDesequipButton;

    [SerializeField]
    private Button chestSlotDesequipButton;

    [SerializeField]
    private Button handsSlotDesequipButton;

    [SerializeField]
    private Button legsSlotDesequipButton;

    [SerializeField]
    private Button feetSlotDesequipButton;

    private bool isOpen = false;

    public static Inventory instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CloseInventory();
        RefreshContent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isOpen)
            {
                CloseInventory();
                isOpen = false;
            }
            else 
            {
                OpenInventory();
                isOpen = true;
            }
        }
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        RefreshContent();
    }

    private void OpenInventory()
    {
        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        actionPanel.SetActive(false);
        TooltipSystem.instance.Hide();
    }

    private void RefreshContent()
    {
        // Vider visuel inventaire
        for(int i = 0; i < inventorySlotsParent.childCount; i++)
        {
            Slot currentSlot = inventorySlotsParent.GetChild(i).GetComponent<Slot>();
            currentSlot.item = null;
            currentSlot.itemVisual.sprite = emptySlotVisual;
        }

        // Repeupler l'inventaire
        for (int i = 0; i < items.Count; i++)
        {
            Slot currentSlot = inventorySlotsParent.GetChild(i).GetComponent<Slot>();
            currentSlot.item = items[i];
            currentSlot.itemVisual.sprite = items[i].visual;
        }

        UpdateEquipmentsDesequipButtons();
    } 

    public bool IsFull()
    {
        return InventorySize == items.Count;
    }

    public void OpenActionPanel(ItemData item, Vector3 slotPosition)
    {
        itemCurrentlySelected = item;

        if (item == null)
        {
            actionPanel.SetActive(false);
            return;
        }

        switch (item.itemType)
        {
            case ItemType.Resource:
                useItemButton.SetActive(false);
                equipItemButton.SetActive(false);
                break;
            case ItemType.Equipment:
                useItemButton.SetActive(false);
                equipItemButton.SetActive(true);
                break;
            case ItemType.Consumable:
                useItemButton.SetActive(true);
                equipItemButton.SetActive(false);
                break;
        }

        actionPanel.transform.position = slotPosition;
        actionPanel.SetActive(true);
    }

    public void CloseActionPanel()
    {
        actionPanel.SetActive(false);
        itemCurrentlySelected = null;
    }

    public void UseActionButton() 
    {
        print("Using item : " + itemCurrentlySelected.name);
        CloseActionPanel();
    }

    public void EquipActionButton() 
    {
        EquipmentLibraryItem equipmentLibraryItem = equipmentLibrary.content.Where(elem => elem.itemData == itemCurrentlySelected).First();

        if (equipmentLibraryItem != null)
        {
            switch (itemCurrentlySelected.equipmentType)
            {
                case EquipmentType.Head:
                    DisablePreviousEquipedItem(equipedHeadItem);
                    headSlotImage.sprite = itemCurrentlySelected.visual;
                    equipedHeadItem = itemCurrentlySelected;
                    break;
                case EquipmentType.Chest:
                    DisablePreviousEquipedItem(equipedChestItem);
                    chestSlotImage.sprite = itemCurrentlySelected.visual;
                    equipedChestItem = itemCurrentlySelected;
                    break;
                case EquipmentType.Hands:
                    DisablePreviousEquipedItem(equipedHandsItem);
                    handsSlotImage.sprite = itemCurrentlySelected.visual;
                    equipedHandsItem = itemCurrentlySelected;
                    break;
                case EquipmentType.Legs:
                    DisablePreviousEquipedItem(equipedLegsItem);
                    legsSlotImage.sprite = itemCurrentlySelected.visual;
                    equipedLegsItem = itemCurrentlySelected;
                    break;
                case EquipmentType.Feet:
                    DisablePreviousEquipedItem(equipedFeetItem);
                    feetSlotImage.sprite = itemCurrentlySelected.visual;
                    equipedFeetItem = itemCurrentlySelected;
                    break;
            }

            for (int i = 0; i < equipmentLibraryItem.elementsToDisable.Length; i++)
            {
                equipmentLibraryItem.elementsToDisable[i].SetActive(false);  
            }

            equipmentLibraryItem.itemPrefab.SetActive(true);
            
            items.Remove(itemCurrentlySelected);
            RefreshContent();
        }
        else
        {
            Debug.LogError("Item non existant : " + itemCurrentlySelected.name);
        }

        CloseActionPanel();
    }

    public void DropActionButton() 
    {
        GameObject instantiatedItem = Instantiate(itemCurrentlySelected.prefab);
        instantiatedItem.transform.position = dropPoint.position;

        items.Remove(itemCurrentlySelected);
        RefreshContent();
        CloseActionPanel();
    }

    public void DestroyActionButton() 
    {
        items.Remove(itemCurrentlySelected);
        RefreshContent();
        CloseActionPanel();
    }

    private void UpdateEquipmentsDesequipButtons()
    {
        headSlotDesequipButton.onClick.RemoveAllListeners();
        headSlotDesequipButton.onClick.AddListener(delegate { DesequipEquipment(EquipmentType.Head); });
        headSlotDesequipButton.gameObject.SetActive(equipedHeadItem);

        chestSlotDesequipButton.onClick.RemoveAllListeners();
        chestSlotDesequipButton.onClick.AddListener(delegate { DesequipEquipment(EquipmentType.Chest); });
        chestSlotDesequipButton.gameObject.SetActive(equipedChestItem);
        
        handsSlotDesequipButton.onClick.RemoveAllListeners();
        handsSlotDesequipButton.onClick.AddListener(delegate { DesequipEquipment(EquipmentType.Hands); });
        handsSlotDesequipButton.gameObject.SetActive(equipedHandsItem);
        
        legsSlotDesequipButton.onClick.RemoveAllListeners();
        legsSlotDesequipButton.onClick.AddListener(delegate { DesequipEquipment(EquipmentType.Legs); });
        legsSlotDesequipButton.gameObject.SetActive(equipedLegsItem);
        
        feetSlotDesequipButton.onClick.RemoveAllListeners();
        feetSlotDesequipButton.onClick.AddListener(delegate { DesequipEquipment(EquipmentType.Feet); });
        feetSlotDesequipButton.gameObject.SetActive(equipedFeetItem);
    }

    public void DesequipEquipment(EquipmentType equipmentType)
    {
        if (IsFull())
        {
            Debug.Log("Inventaire plein");
            return;
        }

        ItemData currentItem = null;

        switch (equipmentType)
        {
            case EquipmentType.Head:
                currentItem = equipedHeadItem;
                headSlotImage.sprite = emptySlotVisual;
                equipedHeadItem = null;
                break;
            case EquipmentType.Chest:
                currentItem = equipedChestItem;
                chestSlotImage.sprite = emptySlotVisual;
                equipedChestItem = null;
                break;
            case EquipmentType.Hands:
                currentItem = equipedHandsItem;
                handsSlotImage.sprite = emptySlotVisual;
                equipedHandsItem = null;
                break;
            case EquipmentType.Legs:
                currentItem = equipedLegsItem;
                legsSlotImage.sprite = emptySlotVisual;
                equipedLegsItem = null;
                break;
            case EquipmentType.Feet:
                currentItem = equipedFeetItem;
                feetSlotImage.sprite = emptySlotVisual;
                equipedFeetItem = null;
                break;
        }

        EquipmentLibraryItem equipmentLibraryItem = equipmentLibrary.content.Where(elem => elem.itemData == currentItem).First();

        if (equipmentLibraryItem != null)
        {
            for (int i = 0; i < equipmentLibraryItem.elementsToDisable.Length; i++)
            {
                equipmentLibraryItem.elementsToDisable[i].SetActive(true);  
            }

            equipmentLibraryItem.itemPrefab.SetActive(false);
        }
        
        AddItem(currentItem);
        RefreshContent();
    }

    private void DisablePreviousEquipedItem(ItemData itemToDisable) 
    {
        if (itemToDisable == null)
        {
            return;
        }

        EquipmentLibraryItem equipmentLibraryItem = equipmentLibrary.content.Where(elem => elem.itemData == itemToDisable).First();

        if (equipmentLibraryItem != null)
        {
            for (int i = 0; i < equipmentLibraryItem.elementsToDisable.Length; i++)
            {
                equipmentLibraryItem.elementsToDisable[i].SetActive(true);  
            }

            equipmentLibraryItem.itemPrefab.SetActive(false);
        }

        AddItem(itemToDisable);
    }
}
