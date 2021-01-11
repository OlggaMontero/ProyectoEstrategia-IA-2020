using UnityEngine;

public class BoxNode 
{
    public float ArcherValue;
    public float BatValue;
    public float KingValue;
    public float VillageValue;
    public float KnightValue;
    public float BaseValue;
    public float[] Influence;

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

        Influence = new float[4];

        MatrixPosition = matrixPosition;
        this.position = position;
    }

    public void PopulateArray()
    {
        Influence[0] = ArcherValue;
        Influence[1] = BatValue;
        Influence[2] = KingValue;
        Influence[3] = KnightValue;
    }
}
