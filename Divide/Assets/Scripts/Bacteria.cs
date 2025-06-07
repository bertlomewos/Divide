using UnityEngine;
using System.Collections;

public class Bacteria : MonoBehaviour
{
    public Tile currentTile;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void AnimateSpawn(Tile targetTile, Vector3 startPosition, float duration)
    {
        currentTile = targetTile;
        StartCoroutine(MoveAndGrowCoroutine(targetTile.transform.position, startPosition, duration));
    }

    private IEnumerator MoveAndGrowCoroutine(Vector3 targetPosition, Vector3 startPosition, float duration)
    {
        float elapsedTime = 0;
        transform.position = startPosition;
        transform.localScale = Vector3.zero; 

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);

            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = originalScale;
    }

   
    public void AnimateDivisionShrink(float duration)
    {
        StartCoroutine(ShrinkCoroutine(originalScale * 0.75f, duration));
    }

    private IEnumerator ShrinkCoroutine(Vector3 targetScale, float duration)
    {
        float elapsedTime = 0;
        Vector3 startingScale = transform.localScale;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(startingScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}