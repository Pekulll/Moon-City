using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private GameObject groundTargetEffect;

    public void GroundTargetEffect(Vector3 position, Color color)
    {
        GameObject g = Instantiate(groundTargetEffect, position, Quaternion.identity) as GameObject;
        g.transform.eulerAngles = new Vector3(90, 0, 0);
        g.GetComponent<SpriteRenderer>().color = color;
        Destroy(g, 1f);
    }
}
