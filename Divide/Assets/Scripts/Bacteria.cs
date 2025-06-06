using UnityEngine;

public class Bacteria : MonoBehaviour
{
    public Tile currentTile;

    public void MoveToTile(Tile targetTile)
    {
        transform.position = targetTile.transform.position;
        currentTile = targetTile;
    }
}
