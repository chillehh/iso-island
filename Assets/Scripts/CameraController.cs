using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 10.0f;
    [SerializeField] private float mouseSensitivity = 0.01f;

    private Vector2 lastPos;
    private MouseInput mouseInput;
    private bool mouseDown = false;
    [SerializeField] private InputAction mouseClick;
    

    private Camera mainCam;

    void Awake()
    {
        mouseInput = new();
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        mouseInput.Enable();
        mouseClick.Enable();
        mouseClick.performed += mousePressed;
    }

    private void OnDisable()
    {
        mouseInput.Disable();
        mouseClick.performed -= mousePressed;
        mouseClick.Disable();
    }

    private void mousePressed(InputAction.CallbackContext ctx)
    {
        if (mouseClick.ReadValue<float>() != 0)
        {
            lastPos = mouseInput.Mouse.mousePosition.ReadValue<Vector2>();
            mouseDown = true;
        } else
        {
            mouseDown = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float scrollValue = mouseInput.Mouse.mouseScroll.ReadValue<float>();
        float scrollFactor = 0f;
        if (scrollValue > 0)
        {
            scrollFactor = 0.1f;
        } else if (scrollValue < 0)
        {
            scrollFactor = -0.1f;
        }
        float orthoSize = scrollFactor * scrollSpeed;
        mainCam.orthographicSize = mainCam.orthographicSize - orthoSize;

        if (mainCam.orthographicSize <= 1)
        {
            mainCam.orthographicSize = 2;
        } else if (mainCam.orthographicSize >= 50)
        {
            mainCam.orthographicSize = 50;
        }

        if (mouseDown)
        {
            Vector2 delta = mouseInput.Mouse.mousePosition.ReadValue<Vector2>() - lastPos;
            transform.Translate((-delta.x * mouseSensitivity) * (mainCam.orthographicSize * 0.15f), (-delta.y * mouseSensitivity) * (mainCam.orthographicSize * 0.15f), 0);
            lastPos = mouseInput.Mouse.mousePosition.ReadValue<Vector2>();
        }
    }
}
