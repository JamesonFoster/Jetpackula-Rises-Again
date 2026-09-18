using UnityEngine;

public class InventController : MonoBehaviour
{
    public GameObject pointer;
    public GameObject palm;
    public GameObject hands;
    public GameObject menu1;

    public Canvas canvas;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (GlobalPlayerVars.playerMode == 0)
            {
                OpenInventory();
            }
            else if (GlobalPlayerVars.playerMode == 1)
            {
                CloseInventory();
            }
        }
    }

    public void OpenInventory()
    {
        if (GlobalPlayerVars.playerMode == 1)
            return;

        pointer.SetActive(true);
        palm.SetActive(true);
        hands.SetActive(false);

        GlobalPlayerVars.playerMode = 1;

        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseInventory()
    {
        if (GlobalPlayerVars.playerMode == 0)
            return;

        pointer.SetActive(false);
        palm.SetActive(false);
        hands.SetActive(true);

        GlobalPlayerVars.inventKeyCode = "none";

        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera,
            out localPoint
        );

        localPoint.y += 99999f;

        menu1.GetComponent<RectTransform>().localPosition = localPoint;

        GlobalPlayerVars.playerMode = 0;

        Cursor.lockState = CursorLockMode.Locked;
    }
}
