using UnityEngine;

public class Interactivity : MonoBehaviour
{
    [SerializeField] Camera FPCamera;
    [SerializeField] float range = 10f;

    void Update()
    {
        RaycastHit hit;

        var raycastThing = Physics.Raycast(
            FPCamera.transform.position,
            FPCamera.transform.forward,
            out hit,
            range
        );

        if (raycastThing)
        {
            IInteractable interactable = hit.transform.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                    interactable.DoSomething();

                if (Input.GetMouseButtonDown(0) && GlobalPlayerVars.ArmState != 'A')
                    interactable.HitSomething(hit);
            }
        }
    }
}

public interface IInteractable
{
    public void DoSomething();
    public void HitSomething(RaycastHit hit);
}
