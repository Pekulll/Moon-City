﻿using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float scrollSpeed = 20f;
    [SerializeField] private int panBorderThickness = 10;
    [SerializeField] private Vector2 panLimit;
    [SerializeField] private Vector2 heightLimit;
    [Space(5)]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool edgeToMove = true;

    [Header("KeyCode")]
    [SerializeField] private string forKey = "w";
    [SerializeField] private string backKey = "s";
    [SerializeField] private string leftKey = "a";
    [SerializeField] private string rightKey = "d";
    [SerializeField] private string rotateLeft = "a";
    [SerializeField] private string rotateRight = "e";

    private MoonManager manager;

    private Vector3 dragStartPosition;
    private Vector3 currentDragPosition;

    private float xRotation, yRotation;

    private void Start()
    {
        manager = FindObjectOfType<MoonManager>();

        forKey = PlayerPrefs.GetString("MoveForward");
        backKey = PlayerPrefs.GetString("MoveBackward");
        leftKey = PlayerPrefs.GetString("StrafeLeft");
        rightKey = PlayerPrefs.GetString("StrafeRight");

        if (!PlayerPrefs.HasKey("MoveForward")) { forKey = "w"; }
        if (!PlayerPrefs.HasKey("MoveBackward")) { forKey = "s"; }
        if (!PlayerPrefs.HasKey("StrafeLeft")) { forKey = "a"; }
        if (!PlayerPrefs.HasKey("StrafeRight")) { forKey = "d"; }

#if UNITY_EDITOR
        edgeToMove = false;
#else
        edgeToMove = true;
#endif

        panSpeed = (int)transform.position.y;
    }

    void Update()
    {
        if (!canMove) return;

        MouseInput();
        KeyboardInput();
    }

    private void MouseInput()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButtonDown(2))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float entry;

                if (plane.Raycast(ray, out entry))
                {
                    dragStartPosition = ray.GetPoint(entry);
                }
            }

            if (Input.GetMouseButton(2))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 pos;

                float entry;

                if (plane.Raycast(ray, out entry))
                {
                    currentDragPosition = ray.GetPoint(entry);

                    pos = transform.position + dragStartPosition - currentDragPosition;

                    pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
                    pos.y = Mathf.Clamp(pos.y, heightLimit.x, heightLimit.y);
                    pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

                    transform.position = pos;
                }
            }
        }
        else if(Input.GetMouseButton(2))
        {
            Cursor.lockState = CursorLockMode.Locked;

            xRotation += -2f * Input.GetAxis("Mouse Y");
            yRotation += 2f * Input.GetAxis("Mouse X");

            xRotation = Mathf.Clamp(xRotation, -45, 45);
            yRotation %= 360;

            transform.eulerAngles = new Vector3(xRotation, yRotation, 0);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void KeyboardInput()
    {
        if (Input.GetMouseButton(2) && !Input.GetKey(KeyCode.LeftControl)) return;

        Vector3 pos = transform.position;
        float speedFactor = (Time.timeScale != 0) ? Time.deltaTime / Time.timeScale : Time.fixedDeltaTime / 2;

        if (Input.GetKey(forKey) || Input.GetKey(KeyCode.UpArrow) || (Input.mousePosition.y >= Screen.height - panBorderThickness && edgeToMove))
            pos.z += panSpeed * speedFactor;

        if (Input.GetKey(backKey) || Input.GetKey(KeyCode.DownArrow) || (Input.mousePosition.y <= panBorderThickness && edgeToMove))
            pos.z -= panSpeed * speedFactor;

        if (Input.GetKey(leftKey) || Input.GetKey(KeyCode.LeftArrow) || (Input.mousePosition.x <= panBorderThickness && edgeToMove))
            pos.x -= panSpeed * speedFactor;

        if (Input.GetKey(rightKey) || Input.GetKey(KeyCode.RightArrow) || (Input.mousePosition.x >= Screen.width - panBorderThickness && edgeToMove))
            pos.x += panSpeed * speedFactor;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (!manager.isOverUI) pos.y -= scroll * scrollSpeed * 100f * 1.5f * speedFactor;

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, heightLimit.x, heightLimit.y);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        panSpeed = (int)pos.y;
        transform.position = pos;
    }

    public void ResetCamera()
    {
        xRotation = 0;
        yRotation = 0;

        transform.eulerAngles = new Vector3();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(new Vector3(), new Vector3(panLimit.x * 2, 50, panLimit.y * 2));
    }
}
