using UnityEngine;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;

public class HandsUI : MonoBehaviour
{
    private RawImage image;
    private char curAS;
    public Texture restingSprite;
    public Texture blockingSprite;
    public Texture attackingSprite;
    public Texture zoomingSprite;
    void Awake()
    {
        image = GetComponent<RawImage>();
    }
    void Start()
    {
        curAS = GlobalPlayerVars.ArmState;
        if (curAS == 'R')
        {
            image.texture = restingSprite;
        }
        else if (curAS == 'B')
        {
            image.texture = blockingSprite;
        }
        else if (curAS == 'A')
        {
            image.texture = attackingSprite;
        }
        else if (curAS == 'Z')
        {
            image.texture = zoomingSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        curAS = GlobalPlayerVars.ArmState;
        if (curAS == 'R')
        {
            image.texture = restingSprite;
        }
        else if (curAS == 'B')
        {
            image.texture = blockingSprite;
        }
        else if (curAS == 'A')
        {
            image.texture = attackingSprite;
        }
        else if (curAS == 'Z')
        {
            image.texture = zoomingSprite;
        }
    }
}
