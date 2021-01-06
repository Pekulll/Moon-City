using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingMotor : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool useWarning;
    [SerializeField] private bool fadeIn;
    [SerializeField] private bool fadeOut;

    private AsyncOperation result;

    private GameObject warning;
    private Animator fade;
    private Text percentage;
    private Image bar;

    private void Start()
    {
        percentage = GameObject.Find("T_Percentage").GetComponent<Text>();
        bar = GameObject.Find("I_LoadingBar").GetComponent<Image>();
        fade = GameObject.Find("I_Fade").GetComponent<Animator>();

        if (fadeIn)
        {
            fade.Play("In");
        }

        if (useWarning)
        {
            warning = GameObject.Find("E_Warning");
            warning.SetActive(false);
        }

        StartCoroutine(LoadGameScene());        
    }

    IEnumerator LoadGameScene()
    {
        result = SceneManager.LoadSceneAsync(sceneName);
        result.allowSceneActivation = false;

        while (!result.isDone)
        {

            float progress = Mathf.Clamp01(result.progress / 0.9f);
            bar.fillAmount = progress;
            percentage.text = (int)(progress * 100) + "%";

            if (result.progress != 0.9f)
            {
                /*if (GameObject.Find("Presence Manager") != null)
                {
                    PresenceManager.UpdatePresence(detail: "Loading.", state: "", largeKey: "earth", largeText: "In development...", smallKey: "", smallText: "");
                }*/

                yield return null;
            }
            else if (result.progress == 0.9f)
            {
                if (useWarning)
                {
                    warning.SetActive(true);
                }
                else
                {
                    StartFadeOut();
                }
                
                yield return null;
            }
        }
    }

    public void Btn_Warning()
    {
        StartFadeOut();
    }


    private void StartFadeOut()
    {
        if (fadeOut)
            fade.Play("Out");
        else
            ActiveScene();
    }

    public void ActiveScene()
    {
        result.allowSceneActivation = true;
    }
}
