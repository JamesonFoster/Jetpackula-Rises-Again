using UnityEngine;

public class KeyInsertSpot : MonoBehaviour, IInteractable
{
    public GameObject visualUnlock;
    public string keyCode;
    public Doors targetDoor;
    public InventController iC;

    public void DoSomething()
    {
        iC.OpenInventory();
        GlobalPlayerVars.inventKeyCode = keyCode;
        GlobalPlayerVars.inventInteraction = this;
    }

    public void HitSomething(RaycastHit hit)
    {
        DoSomething();
    }

    public void KeyInserted()
    {
        visualUnlock.SetActive(true);
        targetDoor.unlock();

        Destroy(this);
    }
}
