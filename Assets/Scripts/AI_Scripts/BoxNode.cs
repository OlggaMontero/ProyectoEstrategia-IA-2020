using UnityEngine;

public class BoxNode 
{
    public float ArcherValue;
    public float BatValue;
    public float KingValue;
    public float VillageValue;
    public float KnightValue;
    public float BaseValue;

    public Vector2 position;
    public Vector2Int MatrixPosition;

    public BoxNode(float archerValue, float batValue, float kingValue, float villageValue, float knightValue, float baseValue, Vector2 position, Vector2Int matrixPosition)
    {
        ArcherValue = archerValue;
        BatValue = batValue;
        KingValue = kingValue;
        VillageValue = villageValue;
        KnightValue = knightValue;
        BaseValue = baseValue;

        MatrixPosition = matrixPosition;
        this.position = position;
    }
}
