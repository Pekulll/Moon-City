using UnityEngine;
using UnityEngine.UI;

public class OutputPopupotor : MonoBehaviour
{
    [SerializeField] private Text popupTxt;
    [SerializeField] private Image popupImg;

    public void Init(float gain, Sprite icon)
    {
        transform.SetParent(GameObject.Find("PopupParent").transform);
        
        if (gain >= 0)
        {
            popupTxt.text = "+ " + gain;
            popupTxt.color = new Color(.5f, 1, .5f, 1);
            popupImg.color = new Color(.5f, 1, .5f, 1);
        }
        else
        {
            popupTxt.text = "- " + (-gain);
            popupTxt.color = new Color(1, .5f, .5f, 1);
            popupImg.color = new Color(1, .5f, .5f, 1);
        }

        popupImg.sprite = icon;

        Destroy(gameObject, 1.5f);
    }

    private void Update()
    {
        transform.LookAt(GameObject.Find("Player").transform);

        Quaternion rotation = transform.rotation;
        rotation.z = 0;
        transform.rotation = rotation;

        transform.Translate(Vector3.up * 1f * Time.deltaTime);
    }
}
