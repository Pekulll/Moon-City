using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DiscordPresence;

public class LoadingScript : MonoBehaviour {

    [SerializeField] private string sceneName, detail;

    [SerializeField] private float delay;
    [SerializeField] private int fontSize;
    private float o = 0, i = 1;

    [SerializeField] private bool fadeIn = true;
    [SerializeField] private bool fadeOut = true;

    private Text loadingIndicator;
    private Image fade;

    private void Start()
    {
        loadingIndicator = GameObject.Find("Loading%").GetComponent<Text>();
        fade = GameObject.Find("FadeOut").GetComponent<Image>();

        if (fadeIn) StartCoroutine(FadeIn());

        StartCoroutine(LoadGameScene());
        StartCoroutine(TextAnimation());
    }

    IEnumerator LoadGameScene()
    {
        AsyncOperation result = SceneManager.LoadSceneAsync(sceneName);
        result.allowSceneActivation = false;

        while (!result.isDone)
        {
            
            float progress = Mathf.Clamp01(result.progress / 0.9f);
            if(result.progress != 0.9f)
            { 
                /*if (GameObject.Find("Presence Manager") != null)
                {
                    PresenceManager.UpdatePresence(detail: "Loading.", state: "", largeKey: "earth", largeText: "In development...", smallKey: "", smallText: "");
                }*/

                yield return null;
            }
            else if(delay >= 0)
            {
                if(delay != 1)
                {
                    yield return new WaitForSeconds(1);
                    delay--;
                }
                else
                {
                    if(fadeOut) Invoke("StartWithDelay", 0.5f);
                    yield return new WaitForSeconds(1);
                    delay--;
                }
            }
            else if(delay <= 0 && result.progress == 0.9f)
            {
                result.allowSceneActivation = true;
                yield return null;
            }
            

        }
    }

    IEnumerator TextAnimation()
    {
        while(delay != 0)
        {
            if (loadingIndicator.text == "<size=" + (fontSize + 20) + ">.</size>..")
            {
                loadingIndicator.text = ".<size=" + (fontSize + 20) + ">.</size>.";
                yield return new WaitForSeconds(0.5f);
            }
            else if (loadingIndicator.text == ".<size=" + (fontSize + 20) + ">.</size>.")
            {
                loadingIndicator.text = "..<size=" + (fontSize + 20) + ">.</size>";
                yield return new WaitForSeconds(0.5f);
            }
            else if (loadingIndicator.text == "..<size=" + (fontSize + 20) + ">.</size>")
            {
                loadingIndicator.text = "<size=" + (fontSize + 20) + ">.</size>..";
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                loadingIndicator.text = ".<size=" + (fontSize + 20) + ">.</size>.";
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    IEnumerator FadeOut()
    {
        while (o <= 1)
        {
            fade.color = new Color(0, 0, 0, o);
            o += 0.033f;
            yield return new WaitForSeconds(0.03f);
        }
    }

    IEnumerator FadeIn()
    {
        while (i >= 0)
        {
            fade.color = new Color(0, 0, 0, i);
            i -= 0.033f;
            yield return new WaitForSeconds(0.03f);
        }
    }

    private void StartWithDelay()
    {
        StartCoroutine(FadeOut());
    }
}
