using UnityEngine;
using System.Collections;

public class Doors : MonoBehaviour, IInteractable
{
public bool isLocked = false;

public float swingOpenAmount = 90f;
public float giggleAmount = 3f;

public float swingSpeed = 2f;
public float giggleSpeed = 10f;

private HingeJoint hinge;
private Rigidbody rb;
private bool isOpen = false;
private Coroutine giggleCoroutine;

private void Start()
{
    hinge = GetComponent<HingeJoint>();
    rb = GetComponent<Rigidbody>();

    if (hinge == null)
    {
        Debug.LogError("No Hinge Joint found on " + gameObject.name);
        return;
    }

    if (rb == null)
    {
        Debug.LogError("No Rigidbody found on " + gameObject.name);
        return;
    }

    JointLimits limits = hinge.limits;
    limits.min = 0f;
    limits.max = swingOpenAmount;
    hinge.limits = limits;

    hinge.useLimits = true;
}

public void DoSomething()
{
    if (hinge == null)
        return;

    if (!isLocked)
    {
        OpenDoor();
    }
    else
    {
        GiggleDoor();
    }
}

public void HitSomething(RaycastHit hit)
{
    DoSomething();
}

private void OpenDoor()
{
    // Stop any previous giggle
    if (giggleCoroutine != null)
    {
        StopCoroutine(giggleCoroutine);
        giggleCoroutine = null;
    }

    JointMotor motor = hinge.motor;

    motor.force = 1000f;

    if (!isOpen)
    {
        motor.targetVelocity = swingSpeed * 100f;
        isOpen = true;
    }
    else
    {
        motor.targetVelocity = -swingSpeed * 100f;
        isOpen = false;
    }

    hinge.motor = motor;
    hinge.useMotor = true;
}

private void GiggleDoor()
{
    if (giggleCoroutine != null)
        return;

    giggleCoroutine = StartCoroutine(Giggle());
}

private IEnumerator Giggle()
{
    JointMotor motor = hinge.motor;

    motor.force = 500f;
    hinge.useMotor = true;

    // Move right
    motor.targetVelocity = giggleSpeed * 10f;
    hinge.motor = motor;

    yield return new WaitForSeconds(0.12f);

    // Move left
    motor.targetVelocity = -giggleSpeed * 10f;
    hinge.motor = motor;

    yield return new WaitForSeconds(0.12f);

    // Move right
    motor.targetVelocity = giggleSpeed * 10f;
    hinge.motor = motor;

    yield return new WaitForSeconds(0.12f);

    // Turn the motor off
    motor.targetVelocity = 0f;
    hinge.motor = motor;
    hinge.useMotor = false;

    // Kill any leftover physics momentum
    rb.angularVelocity = Vector3.zero;

    // Make absolutely sure the door is closed
    JointSpring spring = hinge.spring;
    spring.spring = 1000f;
    spring.damper = 100f;
    spring.targetPosition = 0f;

    hinge.spring = spring;
    hinge.useSpring = true;

    yield return new WaitForSeconds(0.1f);

    // Stop the spring from continuing to affect the door
    hinge.useSpring = false;

    // Kill any tiny remaining movement
    rb.angularVelocity = Vector3.zero;

    isOpen = false;
    giggleCoroutine = null;
}

public void unlock()
    {
        isLocked = false;
    }

}