using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool isHoverUI;

    private MoonManager manager;

    private void Start()
    {
        manager = FindObjectOfType<MoonManager>();

        if(GetComponent<Image>() != null)
        {
            GetComponent<Image>().raycastTarget = true;
        }
    }

    public void OnPointerEnter(PointerEventData ped)
    {
        manager.isOverUI = isHoverUI;
    }

    public void OnPointerExit(PointerEventData ped)
    {
        manager.isOverUI = !isHoverUI;
    }
}
