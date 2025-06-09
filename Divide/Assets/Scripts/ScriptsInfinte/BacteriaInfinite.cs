using UnityEngine;
using System.Collections;

public class BacteriaInfinite : MonoBehaviour
{
    public TileInfinite currentTile;
    private Vector3 originalScale;
    private int generation = 0;
    private const float scaleReductionPerGen = 0.95f; // Scale factor per generation

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetGeneration(int gen)
    {
        generation = gen;
        float scaleFactor = Mathf.Pow(scaleReductionPerGen, generation);
        transform.localScale = originalScale * scaleFactor;
    }

    public void PerformDivisionShrink()
    {
        StartCoroutine(ShrinkAndRegrowAnimation());
    }

    IEnumerator ShrinkAndRegrowAnimation()
    {
        Vector3 currentScale = transform.localScale;
        Vector3 shrunkenScale = currentScale * 0.75f;
        float duration = 0.2f;
        float timer = 0f;

        // Shrink
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(currentScale, shrunkenScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = shrunkenScale;

        // Grow back
        timer = 0f;
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(shrunkenScale, currentScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = currentScale;
    }

    public void MoveToTile(TileInfinite targetTile, BacteriaInfinite parent = null)
    {
        currentTile = targetTile;

        if (parent != null)
        {
            generation = parent.generation + 1;
        }
        SetGeneration(generation);

        StartCoroutine(DivideAndGrowAnimation(targetTile.transform.position, parent));
    }

    IEnumerator DivideAndGrowAnimation(Vector3 targetPosition, BacteriaInfinite parent)
    {
        float duration = 0.4f;
        float timer = 0f;
        Vector3 startPosition = parent != null ? parent.transform.position : targetPosition;
        transform.position = startPosition;
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;

        while (timer < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer / duration);
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        transform.localScale = targetScale;
    }
}
