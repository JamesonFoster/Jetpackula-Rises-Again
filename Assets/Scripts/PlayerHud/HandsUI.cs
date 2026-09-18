using UnityEngine;
using UnityEngine.UI;

public class HandsUI : MonoBehaviour
{
    private RawImage image;

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
        UpdateHandImage();
    }

    void Update()
    {
        // Blocking
        if (Input.GetMouseButton(1))
        {
            GlobalPlayerVars.ArmState = 'B';
        }
        // Attacking
        else if (Input.GetMouseButtonDown(0) && GlobalPlayerVars.ArmState != 'A')
        {
            basicAtkStart();
        }

        // Handle attack timer
        if (atkTimer > 0f)
        {
            basicAtkHandler();
        }

        UpdateHandImage();
    }

    public void basicAtkStart()
    {
        atkTimer = 0.4f;
    }

    public void basicAtkHandler()
    {
        atkTimer -= Time.deltaTime;

        if (atkTimer <= 0f)
        {
            atkTimer = 0f;
            GlobalPlayerVars.ArmState = 'R';
        }
    }

    private void UpdateHandImage()
    {
        switch (GlobalPlayerVars.ArmState)
        {
            case 'R':
                image.texture = restingSprite;
                break;

            case 'B':
                image.texture = blockingSprite;
                break;

            case 'A':
                image.texture = attackingSprite;
                break;

            case 'Z':
                image.texture = zoomingSprite;
                break;
        }
    }
}
