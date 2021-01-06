using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(GameObject.Find("Player").transform);
    }
}
