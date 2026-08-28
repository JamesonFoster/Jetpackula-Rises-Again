using UnityEngine;

public class HandsUI : MonoBehaviour
{
    private RawImage image;
    private char curAS;
    public Sprite restingSprite;
    public Sprite blockingSprite;
    public Sprite attackingSprite;
    public Sprite zoomingSprite;
    void Awake()
    {
        image = GetComponent<RawImage>();
    }
    void Start()
    {
        curAS = GlobalPlayerVars.ArmState;
        if (curAS = 'R')
        {
            image.sprite = restingSprite
        }
    }

    // Update is called once per frame
    void Update()
    {
        curAS = GlobalPlayerVars.ArmState;
        if (curAS = 'R')
        {
            image.sprite = restingSprite
        }
    }
}
