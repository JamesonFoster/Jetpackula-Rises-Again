using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotHandler : MonoBehaviour
{
    public Texture empty;
    public RawImage raw;
    public ItemsScriptableObj itemInSlot;
    public int slotNumb;
    public Transform invenButtons;
    public MenuItem meit;
    public Canvas canvas;
    
    void Update()
    {
        if (itemInSlot == null)
        {
            raw.texture = empty;
        }
        else
        {
            raw.texture = itemInSlot.itemImage;
        }
        switch (slotNumb)
        {
            case 0:
                itemInSlot = GlobalPlayerVars.inv1;
                return;
            case 1:
                itemInSlot = GlobalPlayerVars.inv2;
                return;
            case 2:
                itemInSlot = GlobalPlayerVars.inv3;
                return;
            case 3:
                itemInSlot = GlobalPlayerVars.inv4;
                return;
            case 4:
                itemInSlot = GlobalPlayerVars.inv5;
                return;
            case 5:
                itemInSlot = GlobalPlayerVars.inv6;
                return;
        }
    }

    public void click()
    {
        if (itemInSlot != null)
        {
        meit.item = itemInSlot;
        meit.currentSlot = slotNumb;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );
        invenButtons.localPosition = localPoint;
        }
    }
}
