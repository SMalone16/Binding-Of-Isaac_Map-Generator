using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public static CameraFollow instance;

    void Awake()
    {
        instance = this;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}
