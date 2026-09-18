using UnityEngine;

public class Collectables : MonoBehaviour, IInteractable
{
    public ItemsScriptableObj item;
    public bool isEnemy = false;

    private TextBoxes txtBox;
    private bool isLookingAt = false;
    private BasicEnemyControl BEC;
    private RaycastHit currentHit;

    public GameObject bloodSplat;

    private void Awake()
    {
        txtBox = FindFirstObjectByType<TextBoxes>();

        if (txtBox == null)
        {
            Debug.LogError("Collectables: No TextBoxes found in the scene!");
        }
    }

    private void Start()
    {
        BEC = GetComponent<BasicEnemyControl>();

        if (isEnemy && BEC == null)
        {
            Debug.LogError(
                $"{gameObject.name} is marked as an enemy but has no BasicEnemyControl component."
            );
        }
    }

    private void Update()
    {
        // Collect items with E
        if (isLookingAt && !isEnemy && Input.GetKeyDown(KeyCode.E))
        {
            DoSomething();
        }
        isLookingAt = false;
    }

    public void SetLookingAt(RaycastHit hit)
    {
        isLookingAt = true;
        currentHit = hit;
    }

    public void DoSomething()
    {
        if (!isEnemy)
        {
            collected();
        }
    }

    public void collected()
    {
        if (GlobalPlayerVars.inv1 == null)
        {
            GlobalPlayerVars.inv1 = item;
            Destroy(gameObject);
            return;
        }

        if (GlobalPlayerVars.inv2 == null)
        {
            GlobalPlayerVars.inv2 = item;
            Destroy(gameObject);
            return;
        }

        if (GlobalPlayerVars.inv3 == null)
        {
            GlobalPlayerVars.inv3 = item;
            Destroy(gameObject);
            return;
        }

        if (GlobalPlayerVars.inv4 == null)
        {
            GlobalPlayerVars.inv4 = item;
            Destroy(gameObject);
            return;
        }

        if (GlobalPlayerVars.inv5 == null)
        {
            GlobalPlayerVars.inv5 = item;
            Destroy(gameObject);
            return;
        }

        if (GlobalPlayerVars.inv6 == null)
        {
            GlobalPlayerVars.inv6 = item;
            Destroy(gameObject);
            return;
        }

        // Inventory is full
        if (txtBox != null)
        {
            txtBox.StartDio(
                GlobalPlayerVars.BaseChar,
                GlobalPlayerVars.BaseDio,
                true
            );
        }
    }

    // Called by the player/weapon script when a valid attack hits this enemy.
    public void HitSomething(RaycastHit hit)
    {
        if (!isEnemy)
            return;

        if (BEC == null)
        {
            Debug.LogError(
                $"{gameObject.name}: BasicEnemyControl is missing!"
            );
            return;
        }

        GlobalPlayerVars.ArmState = 'A';
        BEC.TakeDamage(GlobalPlayerVars.PlayerDamage);

        if (BEC.Blood != 0 && bloodSplat != null)
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
                Destroy(
                    blood,
                    particles.main.duration +
                    particles.main.startLifetime.constantMax
                );
            }
            else
            {
                Destroy(blood, 2f);
            }
        }
    }
}
