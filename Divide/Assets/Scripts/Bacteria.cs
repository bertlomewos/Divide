using UnityEngine;
using System.Collections;

public class Bacteria : MonoBehaviour
{
    public Tile currentTile;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    // THIS IS THE NEW METHOD FOR THE PARENT
    // It will make the parent shrink and then regrow.
    public void PerformDivisionShrink()
    {
        StartCoroutine(ShrinkAndRegrowAnimation());
    }

    IEnumerator ShrinkAndRegrowAnimation()
    {
        Vector3 shrunkenScale = originalScale * 0.75f; // How small it gets
        float duration = 0.2f; // How fast it shrinks
        float timer = 0f;

        // Shrink down
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, shrunkenScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = shrunkenScale;

        // Reset timer to grow back
        timer = 0f;

        // Grow back to original size
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(shrunkenScale, originalScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure it's back to the original scale
        transform.localScale = originalScale;
    }


    // This method for the NEW bacterium is still correct. No changes needed here.
    public void MoveToTile(Tile targetTile, Bacteria parent = null)
    {
        currentTile = targetTile;
        StartCoroutine(DivideAndGrowAnimation(targetTile.transform.position, parent));
    }

    IEnumerator DivideAndGrowAnimation(Vector3 targetPosition, Bacteria parent)
    {
        float duration = 0.4f;
        float timer = 0f;
        Vector3 startPosition = (parent != null) ? parent.transform.position : targetPosition;
        transform.position = startPosition;
        transform.localScale = Vector3.zero;

        while (timer < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer / duration);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        transform.localScale = originalScale;
    }
}