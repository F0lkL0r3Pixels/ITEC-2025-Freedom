using UnityEngine;

public class GodMode : MonoBehaviour
{
    public GameObject fpCamera;
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;

    private bool godModeOn;
    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            fpCamera.SetActive(!fpCamera.activeSelf);
            godModeOn = !godModeOn;

            if (!godModeOn)
            {
                PlayerManager.Instance.transform.position = transform.position;
            }
        }

        if (godModeOn)
        {
            HandleFreeMovement();
            HandleFreeLook();
        }
    }

    private void HandleFreeMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // A, D
        float moveZ = Input.GetAxis("Vertical");   // W, S
        float moveY = 0f;

        if (Input.GetKey(KeyCode.E)) moveY += 1f; // up
        if (Input.GetKey(KeyCode.Q)) moveY -= 1f; // down

        Vector3 move = new Vector3(moveX, moveY, moveZ);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.Self);
    }

    private void HandleFreeLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        rotationY += mouseX;

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}
