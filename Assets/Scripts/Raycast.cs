using UnityEngine;

public class Raycast
{
    private GameObject target;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector2 screenPosition;

    public Raycast(Vector2 screenPosition)
    {
        this.screenPosition = screenPosition;

        Ray raycast = Camera.current.ScreenPointToRay(screenPosition);
        RaycastHit raycastHit;

        if (Physics.Raycast(raycast, out raycastHit))
        {
            target = raycastHit.collider.gameObject;
            targetPosition = raycastHit.point;
            targetRotation = raycastHit.collider.gameObject.transform.rotation;
        }
        else
        {
            target = null;
            targetPosition = new Vector3();
            targetRotation = new Quaternion();
        }

    }

    public GameObject Target
    {
        get
        {
            return target;
        }
    }

    public Vector3 TargetPosition
    {
        get
        {
            return targetPosition;
        }
    }

    public Quaternion TargetRotation
    {
        get { return targetRotation; }
    }

    public Vector2 ScreenPosition
    {
        get
        {
            return screenPosition;
        }
    }
}
