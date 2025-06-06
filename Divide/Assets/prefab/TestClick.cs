using UnityEngine;
using UnityEngine.EventSystems;

public class TestClick : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Called when a click is detected
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Sprite Clicked: " + gameObject.name);
        // You can add any click logic here, like dealing damage, opening a menu, etc.
    }

    // Called when the pointer enters the sprite's collider
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow; // Change color on hover
        }
    }

    // Called when the pointer exits the sprite's collider
    public void OnPointerExit(PointerEventData eventData)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // Revert to original color
        }
    }
}
