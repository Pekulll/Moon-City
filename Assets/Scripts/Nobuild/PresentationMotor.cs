using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresentationMotor : MonoBehaviour
{
    public float speed;
    public float delay;
    public string tag;

    private int i;
    private GameObject[] gos;

    private void Start()
    {
        gos = GameObject.FindGameObjectsWithTag(tag);

        foreach(GameObject tr in gos)
        {
            if(tr.transform != transform)
            {
                tr.SetActive(false);
            }
        }

        StartCoroutine(turn());
    }

    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }

    private IEnumerator turn()
    {
        while (i < gos.Length)
        {
            if(i != 0)
            {
                gos[i - 1].SetActive(false);
            }

            gos[i].SetActive(true);
            i += 1;
            yield return new WaitForSeconds(delay);
        }
    }
}
