using UnityEngine;
using UnityEngine.UI;

public class TabPanel : MonoBehaviour
{
    [SerializeField] private Tab[] tabs;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;
    
    void Start()
    {
        if (tabs == null)
        {
            Destroy(this);
            return;
        }
        
        HideTabs();
    }

    private void HideTabs()
    {
        foreach (Tab tab in tabs)
        {
            tab.button.color = normalColor;
            tab.panel.SetActive(false);
        }
    }

    private GameObject GetTab(Image button)
    {
        foreach (Tab tab in tabs)
        {
            if (tab.button == button)
            {
                return tab.panel;
            }
        }

        return null;
    }

    public void OnTabSelected(Image tab)
    {
        GameObject panel = GetTab(tab);
        bool active = !panel.activeSelf;
        
        HideTabs();
        
        if (panel == null)
            return;

        panel.SetActive(active);
        tab.color = (active) ? selectedColor : normalColor;
    }

    [System.Serializable]
    private struct Tab
    {
        public Image button;
        public GameObject panel;
    }
}
