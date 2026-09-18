using UnityEngine;

public class MenuItem : MonoBehaviour
{
    public ItemsScriptableObj item;
    public Transform player;
    public TextBoxes txtBo;
    public int currentSlot;
    public Canvas canvas;

    public void UseItem()
    {
        item.Use();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );
        localPoint.y += 99999f;
        transform.localPosition = localPoint;
        if ((!item.keyItem) || (item.keyItem && GlobalPlayerVars.inventKeyCode == item.stringLock))
        {
        if (item.keyItem)
        {
            GlobalPlayerVars.inventInteraction.KeyInserted();
        }
        switch (currentSlot)
        {
            case 0:
                GlobalPlayerVars.inv1 = null;
                return;
            case 1:
                GlobalPlayerVars.inv2 = null;
                return;
            case 2:
                GlobalPlayerVars.inv3 = null;
                return;
            case 3:
                GlobalPlayerVars.inv4 = null;
                return;
            case 4:
                GlobalPlayerVars.inv5 = null;
                return;
            case 5:
                GlobalPlayerVars.inv6 = null;
                return;
        }
        }
    }

    public void InspectItem()
    {
        item.Inspect(txtBo, !txtBo.isDioRunning);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );
        localPoint.y += 99999f;
        transform.localPosition = localPoint;
    }

    public void DiscardItem()
    {
        item.Discard(player);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );
        localPoint.y += 99999f;
        transform.localPosition = localPoint;
        switch (currentSlot)
        {
            case 0:
                GlobalPlayerVars.inv1 = null;
                return;
            case 1:
                GlobalPlayerVars.inv2 = null;
                return;
            case 2:
                GlobalPlayerVars.inv3 = null;
                return;
            case 3:
                GlobalPlayerVars.inv4 = null;
                return;
            case 4:
                GlobalPlayerVars.inv5 = null;
                return;
            case 5:
                GlobalPlayerVars.inv6 = null;
                return;
        }
    }
}
