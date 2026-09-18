using UnityEngine;

public class Interactivity : MonoBehaviour
{
    [SerializeField] Camera FPCamera;
    [SerializeField] float range = 10f;

    public GameObject eyeE;
    public GameObject Slash;

    void Update()
    {
        RaycastHit hit;

        bool raycastThing = Physics.Raycast(
            FPCamera.transform.position,
            FPCamera.transform.forward,
            out hit,
            range
        );

        // Nothing in front of the player
        if (!raycastThing)
        {
            eyeE.SetActive(false);
            Slash.SetActive(false);
            return;
        }

        IInteractable interactable = hit.transform.GetComponent<IInteractable>();

        // Nothing interactable
        if (interactable == null)
        {
            eyeE.SetActive(false);
            Slash.SetActive(false);
            return;
        }

        bool isEnemy = hit.transform.CompareTag("Enemy");

        // Show interaction icons
        eyeE.SetActive(!isEnemy);
        Slash.SetActive(isEnemy);

        if (!isEnemy && Input.GetKeyDown(KeyCode.E))
        {
            interactable.DoSomething();
        }

        if (isEnemy &&
            Input.GetMouseButtonDown(0) &&
            GlobalPlayerVars.ArmState == 'R')
        {
            interactable.HitSomething(hit);
        }
    }
}

public interface IInteractable
{
    void DoSomething();
    void HitSomething(RaycastHit hit);
}
