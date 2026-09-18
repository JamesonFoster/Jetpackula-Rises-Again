using UnityEngine;

public class HandPointMove : MonoBehaviour
{
    public Canvas canvas;

    // Update is called once per frame
    void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );
        transform.localPosition = localPoint;
    }
}
