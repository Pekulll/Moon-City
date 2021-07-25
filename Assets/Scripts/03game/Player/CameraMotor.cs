using UnityEngine;

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

    private MoonManager manager;
    private Camera camera;

    private Vector3 dragStartPosition;
    private Vector3 currentDragPosition;

    private float xRotation, yRotation;

    private void Start()
    {
        manager = FindObjectOfType<MoonManager>();
        camera = GetComponentInChildren<Camera>();

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

        RotateCamera();
        MoveCamera();
    }

    private void RotateCamera()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButtonDown(2))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);

                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

                float entry;

                if (plane.Raycast(ray, out entry))
                {
                    dragStartPosition = ray.GetPoint(entry);
                }
            }

            if (Input.GetMouseButton(2))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
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

        if (Input.anyKey)
        {
            int rotationSpeed = 0;
            
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rotationSpeed = 1;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                rotationSpeed = -1;
            }

            if (rotationSpeed != 0)
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                float entry;
                Vector3 rotateAroundPosition = new Vector3();

                if (plane.Raycast(ray, out entry))
                {
                    rotateAroundPosition = ray.GetPoint(entry);
                }

                transform.RotateAround(rotateAroundPosition, Vector3.up, rotationSpeed * Time.deltaTime * panSpeed);
            }
        }
    }

    private void MoveCamera()
    {
        if (Input.GetMouseButton(2) && !Input.GetKey(KeyCode.LeftControl)) return;

        float speedFactor = Time.unscaledDeltaTime;
        
        if (Input.GetKey(SettingsData.instance.settings.playerInputs[0].inputName)|| (Input.mousePosition.y >= Screen.height - panBorderThickness && edgeToMove))
            transform.Translate(Vector3.forward * panSpeed * speedFactor);

        if (Input.GetKey(SettingsData.instance.settings.playerInputs[1].inputName) || (Input.mousePosition.y <= panBorderThickness && edgeToMove))
            transform.Translate(Vector3.back * panSpeed * speedFactor);

        if (Input.GetKey(SettingsData.instance.settings.playerInputs[2].inputName) || (Input.mousePosition.x <= panBorderThickness && edgeToMove))
            transform.Translate(Vector3.left * panSpeed * speedFactor);

        if (Input.GetKey(SettingsData.instance.settings.playerInputs[3].inputName) || (Input.mousePosition.x >= Screen.width - panBorderThickness && edgeToMove))
            transform.Translate(Vector3.right * panSpeed * speedFactor);

        Vector3 pos = transform.position;

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
