using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehaviour : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;

    [SerializeField]
    private Animator playerAnimator;

    [SerializeField]
    private MoveBehaviour playerMoveBehaviour;

    private Item currentItem;

    public void PickupItem(Item item)
    {
        if (inventory.IsFull())
        {
            Debug.Log("Inventory is full");
            return;
        }

        currentItem = item;

        playerAnimator.SetTrigger("Pickup");

        playerMoveBehaviour.canMove = false;
    }

    public void AddItemToInventory()
    {
        inventory.AddItem(currentItem.itemData);
        Destroy(currentItem.gameObject);

        currentItem = null;
    }

    public void ReactivatePlayerMovement()
    {
        playerMoveBehaviour.canMove = true;
    }
}
