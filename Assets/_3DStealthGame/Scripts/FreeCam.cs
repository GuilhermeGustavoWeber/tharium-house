using UnityEngine;

public class FreeCam : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float boostMultiplier = 3f;

    [Header("Mouse")]
    public float sensitivity = 2f;
    public bool invertY = false;

    private float yaw = 0f;
    private float pitch = 0f;

    private bool cursorLocked = true;

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        HandleCursorToggle();

        if (!cursorLocked)
            return;

        HandleRotation();
        HandleMovement();
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            LockCursor(false);

        if (Input.GetMouseButtonDown(0))
            LockCursor(true);
    }

    void LockCursor(bool state)
    {
        cursorLocked = state;
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * (invertY ? 1 : -1);

        yaw += mouseX;
        pitch += mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.Q)) move -= transform.up;
        if (Input.GetKey(KeyCode.E)) move += transform.up;

        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= boostMultiplier;

        transform.position += move.normalized * speed * Time.deltaTime;
    }
}
