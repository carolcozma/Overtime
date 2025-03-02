using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public Transform sourcePosition;
    public float visionAngle;
    private float halfAngle;
    public float visionDistance;
    [Tooltip("Player layer + what can obstruct the enemy's vision")]
    public LayerMask hitLayer;
    void Start()
    {
        halfAngle = visionAngle / 2;
    }

    void Update()
    {
        Debug.DrawRay(sourcePosition.position, sourcePosition.forward * visionDistance, new Color(0, 1, 0));
    }

    // Checks if object is in view by casting a ray towards the object, and checking if: the object is hit by the ray, and,
    // if that is the case, if the angle between the ray and the forward vector is small enough.
    public bool IsInView(GameObject obj)
    {
        Vector3 dir = obj.transform.position - sourcePosition.position;
        if (Physics.Raycast(sourcePosition.position, Vector3.Normalize(dir), out RaycastHit hitInfo, visionDistance, hitLayer))
        {
            //Debug.Log(hitInfo.collider.gameObject.name);
            if (hitInfo.collider.gameObject.Equals(obj))
            {
                if (Mathf.Abs(Vector3.Angle(sourcePosition.forward, Vector3.Normalize(dir))) <= halfAngle)
                {
                    Debug.DrawRay(sourcePosition.position, dir, new Color(0, 0, 1));
                    return true;
                }
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(sourcePosition.position, visionDistance);
    }
}
