using UnityEngine;

public class Object_Blacksmith : Object_NPC,IInteractable
{
    private Animator anim;
    private Inventory_Player inventory;
    private Inventory_Storage storage;

    public override void Interact()
    {
        base.Interact();

        ui.storageUI.SetupStorage(storage);
        ui.craftUI.SetupCraftUI(storage);
        ui.OpenStorageUI(true);
    }

    protected override void Awake()
    {
        base.Awake();
        storage = GetComponent<Inventory_Storage>();
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("isBlacksmith", true);
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);
        inventory = player.GetComponent<Inventory_Player>();
        storage.SetupInventory(inventory);
    }

    protected override void OnTriggerExit2D(Collider2D collider)
    {
        base.OnTriggerExit2D(collider);
        ui.HideAllToolTips();
        ui.OpenStorageUI(false);
    }
}
