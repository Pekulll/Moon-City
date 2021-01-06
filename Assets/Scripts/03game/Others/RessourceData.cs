using UnityEngine;
using UnityEngine.UI;

public class RessourceData : MonoBehaviour
{
    [Header("Sprite - Entity")]
    public Sprite circleLight;
    public Sprite circleHeavy;
    public Sprite entityMarker;

    [Header("Sprites - Buildings")]
    public Sprite riot;
    public Sprite epidemy;
    public Sprite pause;

    [Header("Sprites - Infos")]
    public Sprite baseIcon;
    [Space(5)]
    public Sprite searchAndDestroy;
    public Sprite defend;
    public Sprite passif;
    [Space(5)]
    public Sprite square;
    public Sprite triangle;
    public Sprite line;

    [Header("Sprites - Output")]
    public Sprite money;
    public Sprite energy;
    public Sprite regolith;
    public Sprite bioplastic;
    public Sprite food;
    public Sprite health;
    [Space(5)]
    public GameObject popup;

    [Header("Prefabs")]
    public GameObject tunelPrefab;
    public Image questPrefab;

    [Header("Sounds")]
    public AudioClip previewSound;
}
