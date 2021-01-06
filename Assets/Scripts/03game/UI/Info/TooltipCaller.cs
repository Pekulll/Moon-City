using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("UI/Tooltip", 90)]
public class TooltipCaller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string title;
    public string info;
    [SerializeField] private bool adaptativeSize;
    [SerializeField] private Vector2 size = new Vector2(200, 125), offset = new Vector2(10, -10);

    const int baseOffset = 10;

    TooltipMotor motor;

    private void Start()
    {
        motor = GameObject.Find("Manager").GetComponent<TooltipMotor>();
    }

    public void CallTooltip()
    {
        motor.CallTooltip(title, info, size, CalculateOffset(), adaptativeSize);
    }

    public void HideTooltip()
    {
        motor.ResetTooltip();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        CallTooltip();
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        HideTooltip();
    }

    private Vector2 CalculateOffset()
    {
        Vector2 position = new Vector2(transform.position.x, transform.position.y);
        Vector2 offset = new Vector2();

        // Le 0, 0 de l'écran est en haut à gauche
        if(position.x < Screen.width / 2) //Partie gauche de l'écran
        {
            offset.x = size.x / 2 + baseOffset;
        }
        else //Partie droite
        {
            offset.x = -(size.x / 2 + baseOffset);
        }

        if (position.y >= Screen.height / 2) //Partie haute de l'écran
        {
            offset.y = -(size.y / 2 + baseOffset);
        }
        else //Partie basse
        {
            offset.y = size.y / 2 + baseOffset;
        }

        return offset;
    }
}
