using UnityEngine;

public class BounceMarkerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Pitch Movement Limits")]
    [SerializeField] private float minX = -0.45f;
    [SerializeField] private float maxX = 0.45f;
    [SerializeField] private float minZ = -1.8f;
    [SerializeField] private float maxZ = 1.8f;

    private void Update()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            verticalInput -= 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            verticalInput += 1f;
        }

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedZ = Mathf.Clamp(transform.position.z, minZ, maxZ);

        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }
}