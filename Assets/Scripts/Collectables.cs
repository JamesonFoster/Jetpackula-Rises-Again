using UnityEngine;

public class Collectables : MonoBehaviour, IInteractable
{
    // Update is called once per frame
    public string itemName = "none";
    public bool isEnemy = false;
    private bool isLookingAt = false;     // Whether the player is looking at this button this frame
    private BasicEnemyControl BEC;
    public GameObject bloodSplat;
    private void Start()
    {
      BEC = GetComponent<BasicEnemyControl>(); 
    }
    void Update()
    {
        if (isLookingAt && Input.GetKeyDown(KeyCode.E))
        {
            DoSomething();
        }
        isLookingAt = false;
    }
    public void DoSomething()
    {
        collected();
    }

    public void collected()
    {
        switch (itemName)
        {
            case "none":
                return;
            case "BloodBag":
                BloodBag();
                return;
            case "NailFile":
                return;
            case "KeyOB1":
                return;
            case "KeyOB2":
                return;
            case "KeyOB3":
                return;
            case "KeyCard1":
                return;
            case "Coin":
                return;
            case "Nade":
                return;
        }
    }
    public void HitSomething(RaycastHit hit)
    {
        if (isEnemy)
        {
            BEC.TakeDamage(GlobalPlayerVars.PlayerDamage);

            if (BEC.Blood != 0)
            {
            GameObject blood = Instantiate(
                bloodSplat,
                hit.point,
                Quaternion.LookRotation(hit.normal),
                hit.transform
            );

            ParticleSystem particles = blood.GetComponent<ParticleSystem>();

            if (particles != null)
            {
                Destroy(blood, particles.main.duration + particles.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(blood, 2f);
            }
            }
        }
    }

    private void BloodBag()
    {
        GlobalPlayerVars.BloodCount += 20;
        Destroy(gameObject);
    }

}
