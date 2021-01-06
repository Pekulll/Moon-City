using System.Collections.Generic;
using UnityEngine;

public class PhysicalPointMotor : MonoBehaviour
{
    private MoonManager moonManager;
    private Vector3 center, size, realSize;
    private int spacing;

    public void Initialize(int spacing)
    {
        moonManager = GameObject.Find("Manager").GetComponent<MoonManager>();
        this.spacing = spacing;
    }

    public bool isValid(GameObject go, Vector3 position)
    {
        gameObject.name = "{Physical 3} " + go.name;
        transform.position = position;

        size = go.GetComponent<BoxCollider>().size;
        center = go.GetComponent<BoxCollider>().center;

        realSize = size + new Vector3(spacing, 0, spacing);

        AdjustHeight();

        return CheckCollision();
    }

    private void AdjustHeight()
    {
        Vector3 correctedPosition = transform.position;
        //correctedPosition.y = moonManager.GetPointHeight(new Vector2(transform.position.x, transform.position.z));
        transform.position = correctedPosition;
    }

    private bool CheckCollision()
    {
        int mask = ~(1 << 9);
        bool collision = Physics.CheckBox(transform.position + center, realSize / 2, Quaternion.identity, mask);

        //Debug.Log("[INFO:PhysicalPointMotor] In collision: " + collision);
        return collision;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + center, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + center, realSize);
    }
}
