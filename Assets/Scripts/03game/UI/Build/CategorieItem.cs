using UnityEngine;
using UnityEngine.UI;

public class CategorieItem : MonoBehaviour
{
    [SerializeField] private Categorie categorie;

    private BuildSystem sys;

    private void Start()
    {
        transform.Find("Image").GetComponent<Image>().sprite = categorie.icon;
        sys = FindObjectOfType<BuildSystem>();
    }

    public void ShowCategory()
    {
        sys.ShowCategory(categorie);
    }
}
