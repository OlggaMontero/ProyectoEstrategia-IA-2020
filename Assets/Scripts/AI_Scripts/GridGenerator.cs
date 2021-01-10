using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject Tile_Container;
    private Tile[] tiles;

    private const int X_SIZE = 19;
    private const int Y_SIZE = 17;

    private Tile[,] grid;

    public LayerMask obstacleLayer;
    public LayerMask tileLayer;

    public static GridGenerator instance;

    private void Awake()
    {
        if(instance is null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }
    private void Start()
    {
        GetTiles();
        f_BuildGrid();
        f_GenerateInfluenceMap();
    }

    private void GetTiles()
    {
        tiles = Tile_Container.GetComponentsInChildren<Tile>();
    }

    private void f_BuildGrid()
    {
        grid = new Tile[Y_SIZE, X_SIZE];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = tiles[i * X_SIZE + j];
                grid[i, j].node.MatrixPosition = new Vector2Int(i, j);
            }
        }
    }

    /// <summary>
    /// Generates the influence map of all units and buildings.
    /// </summary>
    public void f_GenerateInfluenceMap()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                f_ClearNodes(i, j);
            }
        }

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                f_CheckWhatIsInTheTile(i, j);
            }
        }
    }

    private void f_ClearNodes(int i, int j)
    {
        grid[i, j].node.ArcherValue = 0;
        grid[i, j].node.BaseValue = 0;
        grid[i, j].node.KingValue = 0;
        grid[i, j].node.KnightValue = 0;
        grid[i, j].node.VillageValue = 0;
        grid[i, j].node.BatValue = 0;
    }

    private void f_CheckWhatIsInTheTile(int x, int y)
    {
        Collider2D collider = Physics2D.OverlapCircle(grid[x, y].node.position, .3f, obstacleLayer);
        if(collider is null)
        {
            return;
        }
        if (collider.gameObject.name.Contains("Dark Bat"))
        {
            grid[x, y].node.BatValue = 1;
            f_MapFlooding(x, y, "Dark Bat");
        }
        else if (collider.gameObject.name.Contains("Dark King"))
        {
            grid[x, y].node.KingValue = 1;
            f_MapFlooding(x, y, "Dark King");
        }
        else if (collider.gameObject.name.Contains("Dark Knight"))
        {
            grid[x, y].node.KnightValue = 1;
            f_MapFlooding(x, y, "Dark Knight");
        }
        else if (collider.gameObject.name.Contains("Dark Archer"))
        {
            grid[x, y].node.ArcherValue = 1;
            f_MapFlooding(x, y, "Dark Archer");
        }
        else if (collider.gameObject.name.Contains("Dark Village"))
        {
            grid[x, y].node.VillageValue = 1;
            f_MapFlooding(x, y, "Dark Village");
        }
        else if (collider.gameObject.name.Contains("Dark Base"))
        {
            grid[x, y].node.BaseValue = 1;
            f_MapFlooding(x, y, "Dark Base");
        }
    }

    private void f_MapFlooding(int x, int y, string typeOfUnit)
    {
        Collider2D[] collidersOfTile = Physics2D.OverlapCircleAll(grid[x, y].node.position, 4.5f, tileLayer);

        foreach (var c in collidersOfTile)
        {
            Tile tile = c.gameObject.GetComponent<Tile>();

            float distance = Vector2.Distance(grid[x, y].node.position, grid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.position);

            switch (typeOfUnit)
            {
                case "Dark Bat":
                    grid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.BatValue += f_ChangeUnitValue(grid[x, y].node.BatValue, distance);
                    break;
                case "Dark King":
                    grid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.KingValue += f_ChangeUnitValue(grid[x, y].node.KingValue, distance);
                    break;
                case "Dark Knight":
                    grid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.KnightValue += f_ChangeUnitValue(grid[x, y].node.KnightValue, distance);
                    break;
                case "Dark Archer":
                    grid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.ArcherValue += f_ChangeUnitValue(grid[x, y].node.ArcherValue, distance);
                    break;
                case "Dark Village":
                    grid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.VillageValue += f_ChangeUnitValue(grid[x, y].node.VillageValue, distance);
                    break;
                case "Dark Base":
                    grid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.BaseValue += f_ChangeUnitValue(grid[x, y].node.BaseValue, distance);
                    break;
                default:
                    break;
            }
        }
    }

    private float f_ChangeUnitValue(float value, float distance)
    {
        if(distance == 0)
        {
            return 0;
        }
        value /= distance;
        return value;
    }

}
