using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SimpleButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    private Image buttonImage;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.sprite = normalSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.sprite = normalSprite;
    }
}