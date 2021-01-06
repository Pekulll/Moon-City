using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeMotor : MonoBehaviour
{
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private bool fadeOut = true;

    private float o = 0, i = 1;

    private Image fade;

    private void Start()
    {
        fade = GameObject.Find("FadeImg").GetComponent<Image>();
        if (fadeIn) StartCoroutine(FadeIn());
    }

    private IEnumerator FadeO()
    {
        while (o <= 1)
        {
            fade.color = new Color(0, 0, 0, o);
            o += 0.033f;
            yield return new WaitForSeconds(0.03f);
        }
    }

    private IEnumerator FadeIn()
    {
        while (i >= 0)
        {
            fade.color = new Color(0, 0, 0, i);
            i -= 0.033f;
            yield return new WaitForSeconds(0.03f);
        }

        fade.gameObject.SetActive(false);
    }

    public void FadeOut()
    {
        fade.gameObject.SetActive(true);
        StartCoroutine(FadeO());
    }
}
