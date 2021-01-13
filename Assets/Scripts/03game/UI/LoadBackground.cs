using UnityEngine;
using UnityEngine.UI;

public class LoadBackground : MonoBehaviour
{
    private void Awake()
    {
        Texture2D background = Resources.Load<Texture2D>("loading_background");
        Sprite backgroundSprite = Sprite.Create(background, new Rect(0, 0, background.width, background.height), new Vector2(.5f, .5f));
        GetComponent<Image>().sprite = backgroundSprite;
    }
}
