using UnityEngine;
using UnityEngine.UI;

public class TooltipMotor : MonoBehaviour
{
    private RectTransform toolTip, toolTipTitle, aTooltip;
    private Image background;
    private Text titleTxt, tttTitleTxt, aTitleTxt;
    [HideInInspector] public Text infoTxt, aInfoTxt;
    private MoonManager manager;

    private void Start()
    {
        aTooltip = GameObject.Find("Tooltip_Adaptatif").GetComponent<RectTransform>();
        //aTitleTxt = GameObject.Find("aTooltipTitle").GetComponent<Text>();
        aInfoTxt = GameObject.Find("aTooltipInfo").GetComponent<Text>();

        toolTip = GameObject.Find("Tooltip").GetComponent<RectTransform>();
        titleTxt = GameObject.Find("TooltipTitle").GetComponent<Text>();
        infoTxt = GameObject.Find("TooltipInfo").GetComponent<Text>();

        toolTipTitle = GameObject.Find("Tooltip_Title").GetComponent<RectTransform>();
        tttTitleTxt = GameObject.Find("TooltipTitle_Title").GetComponent<Text>();

        manager = GetComponent<MoonManager>();

        toolTip.gameObject.SetActive(false);
        toolTipTitle.gameObject.SetActive(false);
        aTooltip.gameObject.SetActive(false);
    }

    public void CallTooltip(string title, string info, Vector2 size, Vector2 offset, bool adaptative = false)
    {
        if(size != new Vector2()) toolTip.sizeDelta = size;
        Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        if(info != "")
        {
            if (!adaptative)
            {
                toolTip.position = mousePos + offset;
                titleTxt.text = manager.Traduce(title);
                infoTxt.text = manager.Traduce(info);

                if (size != new Vector2()) toolTip.sizeDelta = size;

                toolTip.gameObject.SetActive(true);
            }
            else
            {
                //aTitleTxt.text = manager.Traduce(title);
                aInfoTxt.text = manager.Traduce(info);

                Vector2 newOffset = new Vector2(toolTip.sizeDelta.x / 2 + 60, 0 );
                newOffset = VerifyOffset(newOffset);
                aTooltip.position = mousePos + newOffset;

                aTooltip.gameObject.SetActive(true);
            }
        }
        else
        {
            toolTipTitle.position = mousePos + offset;
            tttTitleTxt.text = manager.Traduce(title);

            if (size != new Vector2()) toolTipTitle.sizeDelta = size;

            toolTipTitle.gameObject.SetActive(true);
        }
    }

    private Vector2 VerifyOffset(Vector2 offset)
    {
        if(Input.mousePosition.x >= Screen.width / 2)
        {
            offset.x *= -1;
        }

        return offset;
    }

    public void ResetTooltip()
    {
        toolTip.gameObject.SetActive(false);
        toolTipTitle.gameObject.SetActive(false);
        aTooltip.gameObject.SetActive(false);
    }
}
