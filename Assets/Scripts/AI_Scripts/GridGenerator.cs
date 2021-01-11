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

    private Tile[,] m_SelfGrid, m_EnemyGrid;

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

        f_BuildGridSelf();
        f_BuildEnemyGrid();

        f_GenerateSelfInfluenceMap();
        f_GenerateEnemyInfluenceMap();
    }

    private void GetTiles()
    {
        tiles = Tile_Container.GetComponentsInChildren<Tile>();
    }

    #region ENEMY INFLUENCE MAP AI

    private void f_BuildEnemyGrid()
    {
        m_EnemyGrid = new Tile[Y_SIZE, X_SIZE];

        for (int i = 0; i < m_EnemyGrid.GetLength(0); i++)
        {
            for (int j = 0; j < m_EnemyGrid.GetLength(1); j++)
            {
                m_EnemyGrid[i, j] = tiles[i * X_SIZE + j];
                m_EnemyGrid[i, j].node.MatrixPosition = new Vector2Int(i, j);
            }
        }
    }

    /// <summary>
    /// Generates the influence map of all units and buildings.
    /// </summary>
    public void f_GenerateEnemyInfluenceMap()
    {
        for (int i = 0; i < m_EnemyGrid.GetLength(0); i++)
        {
            for (int j = 0; j < m_EnemyGrid.GetLength(1); j++)
            {
                f_ClearEnemyNodes(i, j);
            }
        }

        for (int i = 0; i < m_EnemyGrid.GetLength(0); i++)
        {
            for (int j = 0; j < m_EnemyGrid.GetLength(1); j++)
            {
                f_CheckWhatIsInTheEnemyTile(i, j);
            }
        }
    }

    private void f_ClearEnemyNodes(int i, int j)
    {
        m_EnemyGrid[i, j].node.ArcherValue = 0;
        m_EnemyGrid[i, j].node.BaseValue = 0;
        m_EnemyGrid[i, j].node.KingValue = 0;
        m_EnemyGrid[i, j].node.KnightValue = 0;
        m_EnemyGrid[i, j].node.VillageValue = 0;
        m_EnemyGrid[i, j].node.BatValue = 0;
    }

    private void f_CheckWhatIsInTheEnemyTile(int x, int y)
    {
        Collider2D collider = Physics2D.OverlapCircle(m_EnemyGrid[x, y].node.position, .3f, obstacleLayer);
        if(collider is null)
        {
            return;
        }
        if (collider.gameObject.name.Contains("Blue Bat"))
        {
            m_EnemyGrid[x, y].node.BatValue = 1;
            f_EnemyMapFlooding(x, y, "Blue Bat");
        }
        else if (collider.gameObject.name.Contains("Blue King"))
        {
            m_EnemyGrid[x, y].node.KingValue = 1;
            f_EnemyMapFlooding(x, y, "Blue King");
        }
        else if (collider.gameObject.name.Contains("Blue Knight"))
        {
            m_EnemyGrid[x, y].node.KnightValue = 1;
            f_EnemyMapFlooding(x, y, "Blue Knight");
        }
        else if (collider.gameObject.name.Contains("Blue Archer"))
        {
            m_EnemyGrid[x, y].node.ArcherValue = 1;
            f_EnemyMapFlooding(x, y, "Blue Archer");
        }
        else if (collider.gameObject.name.Contains("Blue Village"))
        {
            m_EnemyGrid[x, y].node.VillageValue = 1;
            f_EnemyMapFlooding(x, y, "Blue Village");
        }
        else if (collider.gameObject.name.Contains("Blue Base"))
        {
            m_EnemyGrid[x, y].node.BaseValue = 1;
            f_EnemyMapFlooding(x, y, "Blue Base");
        }
    }

    private void f_EnemyMapFlooding(int x, int y, string typeOfUnit)
    {
        Collider2D[] collidersOfTile = Physics2D.OverlapCircleAll(m_EnemyGrid[x, y].node.position, 4.5f, tileLayer);

        foreach (var c in collidersOfTile)
        {
            Tile tile = c.gameObject.GetComponent<Tile>();

            float distance = Vector2.Distance(m_EnemyGrid[x, y].node.position, m_EnemyGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.position);

            switch (typeOfUnit)
            {
                case "Blue Bat":
                    m_EnemyGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.BatValue += f_ChangeUnitValue(m_EnemyGrid[x, y].node.BatValue, distance);
                    break;
                case "Blue King":
                    m_EnemyGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.KingValue += f_ChangeUnitValue(m_EnemyGrid[x, y].node.KingValue, distance);
                    break;
                case "Blue Knight":
                    m_EnemyGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.KnightValue += f_ChangeUnitValue(m_EnemyGrid[x, y].node.KnightValue, distance);
                    break;
                case "Blue Archer":
                    m_EnemyGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.ArcherValue += f_ChangeUnitValue(m_EnemyGrid[x, y].node.ArcherValue, distance);
                    break;
                case "Blue Village":
                    m_EnemyGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.VillageValue += f_ChangeUnitValue(m_EnemyGrid[x, y].node.VillageValue, distance);
                    break;
                case "Blue Base":
                    m_EnemyGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.BaseValue += f_ChangeUnitValue(m_EnemyGrid[x, y].node.BaseValue, distance);
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

    #region SELF INFLUENCE MAP AI

    private void f_BuildGridSelf()
    {
        m_SelfGrid = new Tile[Y_SIZE, X_SIZE];

        for (int i = 0; i < m_SelfGrid.GetLength(0); i++)
        {
            for (int j = 0; j < m_SelfGrid.GetLength(1); j++)
            {
                m_SelfGrid[i, j] = tiles[i * X_SIZE + j];
                m_SelfGrid[i, j].node.MatrixPosition = new Vector2Int(i, j);
            }
        }
    }

    /// <summary>
    /// Generates the influence map of all units and buildings.
    /// </summary>
    public void f_GenerateSelfInfluenceMap()
    {
        for (int i = 0; i < m_SelfGrid.GetLength(0); i++)
        {
            for (int j = 0; j < m_SelfGrid.GetLength(1); j++)
            {
                f_ClearSelfNodes(i, j);
            }
        }

        for (int i = 0; i < m_SelfGrid.GetLength(0); i++)
        {
            for (int j = 0; j < m_SelfGrid.GetLength(1); j++)
            {
                f_CheckWhatIsInTheSelfTile(i, j);
            }
        }
    }

    private void f_ClearSelfNodes(int i, int j)
    {
        m_SelfGrid[i, j].node.ArcherValue = 0;
        m_SelfGrid[i, j].node.BaseValue = 0;
        m_SelfGrid[i, j].node.KingValue = 0;
        m_SelfGrid[i, j].node.KnightValue = 0;
        m_SelfGrid[i, j].node.VillageValue = 0;
        m_SelfGrid[i, j].node.BatValue = 0;
    }

    private void f_CheckWhatIsInTheSelfTile(int x, int y)
    {
        Collider2D collider = Physics2D.OverlapCircle(m_SelfGrid[x, y].node.position, .3f, obstacleLayer);
        if (collider is null)
        {
            return;
        }
        if (collider.gameObject.name.Contains("Dark Bat"))
        {
            m_SelfGrid[x, y].node.BatValue = 1;
            f_SelfMapFlooding(x, y, "Dark Bat");
        }
        else if (collider.gameObject.name.Contains("Dark King"))
        {
            m_SelfGrid[x, y].node.KingValue = 1;
            f_SelfMapFlooding(x, y, "Dark King");
        }
        else if (collider.gameObject.name.Contains("Dark Knight"))
        {
            m_SelfGrid[x, y].node.KnightValue = 1;
            f_SelfMapFlooding(x, y, "Dark Knight");
        }
        else if (collider.gameObject.name.Contains("Dark Archer"))
        {
            m_SelfGrid[x, y].node.ArcherValue = 1;
            f_SelfMapFlooding(x, y, "Dark Archer");
        }
        else if (collider.gameObject.name.Contains("Dark Village"))
        {
            m_SelfGrid[x, y].node.VillageValue = 1;
            f_SelfMapFlooding(x, y, "Dark Village");
        }
        else if (collider.gameObject.name.Contains("Dark Base"))
        {
            m_SelfGrid[x, y].node.BaseValue = 1;
            f_SelfMapFlooding(x, y, "Dark Base");
        }
    }

    private void f_SelfMapFlooding(int x, int y, string typeOfUnit)
    {
        Collider2D[] collidersOfTile = Physics2D.OverlapCircleAll(m_SelfGrid[x, y].node.position, 4.5f, tileLayer);

        foreach (var c in collidersOfTile)
        {
            Tile tile = c.gameObject.GetComponent<Tile>();

            float distance = Vector2.Distance(m_SelfGrid[x, y].node.position, m_SelfGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.position);

            switch (typeOfUnit)
            {
                case "Dark Bat":
                    m_SelfGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.BatValue += f_ChangeUnitValue(m_SelfGrid[x, y].node.BatValue, distance);
                    break;
                case "Dark King":
                    m_SelfGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.KingValue += f_ChangeUnitValue(m_SelfGrid[x, y].node.KingValue, distance);
                    break;
                case "Dark Knight":
                    m_SelfGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.KnightValue += f_ChangeUnitValue(m_SelfGrid[x, y].node.KnightValue, distance);
                    break;
                case "Dark Archer":
                    m_SelfGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.ArcherValue += f_ChangeUnitValue(m_SelfGrid[x, y].node.ArcherValue, distance);
                    break;
                case "Dark Village":
                    m_SelfGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.VillageValue += f_ChangeUnitValue(m_SelfGrid[x, y].node.VillageValue, distance);
                    break;
                case "Dark Base":
                    m_SelfGrid[tile.node.MatrixPosition.x, tile.node.MatrixPosition.y].node.BaseValue += f_ChangeUnitValue(m_SelfGrid[x, y].node.BaseValue, distance);
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

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
