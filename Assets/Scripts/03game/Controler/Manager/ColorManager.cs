using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    #region Colors

    [Header("Main Theme")]
    public Color icon;
    public Color background;
    public Color backgroundBorder;
    public Color forground;
    public Color text;
    public Color textWarning;

    [Header("Main Theme/Inverse")]
    public Color inversedIcon;
    public Color inversedBackground;
    public Color inversedBackgroundBorder;
    public Color inversedForground;
    public Color inversedText;

    [Header("Main Theme/Transparent")]
    public Color backgroundTransparent;
    public Color foregroundTransparent;

    [Header("Research")]
    public Color available;
    public Color inProgress;
    public Color unavailable;
    public Color finished;

    [Header("Other Theme")]
    public Color importantColor;
    public Color veryImportantColor;

    private void Awake()
    {
        AssignColors();
    }

    public void AssignColors()
    {
        AssignIconColor();
        AssignBackgroundColor();
        AssignForegroundColor();
        AssignTextColor();

        AssignInversedBackgroundColor();
        AssignInversedForegroundColor();
        AssignInversedIconColor();

        AssignTransparentBackgroundColor();
        AssignTransparentForegroundColor();

        AssignImportantColor();
        AssignVeryImportantColor();
    }

    #region Main theme

    private void AssignIconColor()
    {
        GameObject[] icons = GameObject.FindGameObjectsWithTag("Color/Icon");

        foreach (GameObject g in icons)
        {
            Image img = g.GetComponent<Image>();

            if (img != null)
                img.color = icon;
        }

        //Debug.Log("[INFO:InterfaceManager] " + icons.Length + " icons' color changed.");
    }

    private void AssignBackgroundColor()
    {
        GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Color/Background");

        foreach (GameObject g in backgrounds)
        {
            Image img = g.GetComponent<Image>();
            Outline outl = g.GetComponent<Outline>();

            if (img != null)
                img.color = background;

            if (outl != null)
                outl.effectColor = backgroundBorder;
        }

        //Debug.Log("[INFO:InterfaceManager] " + backgrounds.Length + " backgrounds' color changed.");
    }

    private void AssignForegroundColor()
    {
        GameObject[] forgrounds = GameObject.FindGameObjectsWithTag("Color/Forground");

        foreach (GameObject g in forgrounds)
        {
            Image img = g.GetComponent<Image>();

            if (img != null)
                img.color = forground;
        }

        //Debug.Log("[INFO:InterfaceManager] " + forgrounds.Length + " forgrounds' color changed.");
    }

    private void AssignTextColor()
    {
        GameObject[] texts = GameObject.FindGameObjectsWithTag("Color/Text");

        foreach (GameObject g in texts)
        {
            Text txt = g.GetComponent<Text>();

            if (txt != null)
                txt.color = text;
        }

        //Debug.Log("[INFO:InterfaceManager] " + texts.Length + " texts' color changed.");
    }

    #endregion

    #region Inversed

    private void AssignInversedBackgroundColor()
    {
        GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Color/Background (inversed)");

        foreach (GameObject g in backgrounds)
        {
            Image img = g.GetComponent<Image>();
            Outline outl = g.GetComponent<Outline>();

            if (img != null)
                img.color = inversedBackground;

            if (outl != null)
                outl.effectColor = inversedBackgroundBorder;
        }

        //Debug.Log("[INFO:InterfaceManager] " + backgrounds.Length + " inversed backgrounds' color changed.");
    }

    private void AssignInversedForegroundColor()
    {
        GameObject[] forgrounds = GameObject.FindGameObjectsWithTag("Color/Forground (inversed)");

        foreach (GameObject g in forgrounds)
        {
            Image img = g.GetComponent<Image>();

            if (img != null)
                img.color = inversedForground;
        }

        //Debug.Log("[INFO:InterfaceManager] " + forgrounds.Length + " inversed forgrounds' color changed.");
    }

    private void AssignInversedIconColor()
    {
        GameObject[] icons = GameObject.FindGameObjectsWithTag("Color/Icon (inversed)");

        foreach (GameObject g in icons)
        {
            Image img = g.GetComponent<Image>();

            if (img != null)
                img.color = inversedIcon;
        }

        //Debug.Log("[INFO:InterfaceManager] " + icons.Length + " inversed icons' color changed.");
    }

    #endregion

    #region Important

    private void AssignImportantColor()
    {
        GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Color/Important");

        foreach (GameObject g in backgrounds)
        {
            Image img = g.GetComponent<Image>();
            Outline outl = g.GetComponent<Outline>();
            Text txt = g.GetComponent<Text>();

            if (outl != null)
            {
                outl.effectColor = importantColor;

                if (img != null)
                    img.color = background;
            }
            else if (img != null)
                img.color = importantColor;

            if (txt != null)
                txt.color = importantColor;
        }

        //Debug.Log("[INFO:InterfaceManager] " + backgrounds.Length + " important color changed.");
    }

    private void AssignVeryImportantColor()
    {
        GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Color/VeryImportant");

        foreach (GameObject g in backgrounds)
        {
            Image img = g.GetComponent<Image>();
            Outline outl = g.GetComponent<Outline>();
            Text txt = g.GetComponent<Text>();

            if (outl != null)
            {
                outl.effectColor = veryImportantColor;

                if (img != null)
                    img.color = background;
            }
            else if (img != null)
                img.color = veryImportantColor;

            if (txt != null)
                txt.color = veryImportantColor;
        }

        //Debug.Log("[INFO:InterfaceManager] " + backgrounds.Length + " very important color changed.");
    }

    #endregion

    #region Transparent

    private void AssignTransparentBackgroundColor()
    {
        GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Color/Background (transparent)");

        foreach (GameObject g in backgrounds)
        {
            Image img = g.GetComponent<Image>();
            Outline outl = g.GetComponent<Outline>();

            if (img != null)
                img.color = backgroundTransparent;

            if (outl != null)
                outl.effectColor = foregroundTransparent;
        }

        //Debug.Log("[INFO:InterfaceManager] " + backgrounds.Length + " transparent backgrounds' color changed.");
    }

    private void AssignTransparentForegroundColor()
    {
        GameObject[] forgrounds = GameObject.FindGameObjectsWithTag("Color/Foreground (transparent)");

        foreach (GameObject g in forgrounds)
        {
            Image img = g.GetComponent<Image>();

            if (img != null)
                img.color = foregroundTransparent;
        }

        //Debug.Log("[INFO:InterfaceManager] " + forgrounds.Length + " transparent forgrounds' color changed.");
    }

    #endregion

    #endregion
}
