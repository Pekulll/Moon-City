using UnityEngine;
using UnityEngine.EventSystems;

public class HoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool isHoverUI;

    private MoonManager manager;

    private void Start()
    {
        manager = FindObjectOfType<MoonManager>();
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
