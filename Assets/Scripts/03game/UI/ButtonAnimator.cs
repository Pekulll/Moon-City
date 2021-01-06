using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Color hoverColor;
    [SerializeField] private UnityEvent onEnter, onExit, onClick;

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        ChangeColor();
        onEnter.Invoke();
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        ResetColor();
        onExit.Invoke();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        onClick.Invoke();
    }

    private void ChangeColor()
    {
        Image img = GetComponent<Image>();

        if(img != null)
        {
            float r = hoverColor.r + img.color.r;
            float g = hoverColor.g + img.color.g;
            float b = hoverColor.b + img.color.b;
            float a = hoverColor.a + img.color.a;

            img.color = new Color(r / 2f, g / 2f, b / 2f, a / 2f);
        }
    }

    private void ResetColor()
    {
        Image img = GetComponent<Image>();

        if (img != null)
        {
            float r = img.color.r * 2f - hoverColor.r;
            float g = img.color.g * 2f - hoverColor.g;
            float b = img.color.b * 2f - hoverColor.b;
            float a = img.color.a * 2f - hoverColor.a;

            img.color = new Color(r, g, b, a);
        }
    }
}
