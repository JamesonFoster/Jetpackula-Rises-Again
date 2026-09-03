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


    private float atkTimer = 0f;
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
        if (Input.GetKey(KeyCode.Mouse1))
        {
            GlobalPlayerVars.ArmState = 'B';
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && GlobalPlayerVars.ArmState != 'A')
        {
            basicAtkStart();
        }
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

        if (atkTimer >= -0.01f)
        {
            basicAtkHandler();
        }
    }

    public void basicAtkStart()
    {
        atkTimer = 0.4f;
        GlobalPlayerVars.ArmState = 'A';
    }

    public void basicAtkHandler()
    {
        if (atkTimer >= 0f)
        {
            atkTimer -= Time.deltaTime;
            GlobalPlayerVars.ArmState = 'A';
        }
        else
        {
            GlobalPlayerVars.ArmState = 'R';
        }
    }
}
