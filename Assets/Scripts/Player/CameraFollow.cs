using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }
    [SerializeField] private float smoothSpeed = 10f;

    private Transform _target;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetTarget(Transform followTarget)
    {
        _target = followTarget;
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        transform.SetLocalPositionAndRotation(_target.position, _target.rotation);
    }

             
}
