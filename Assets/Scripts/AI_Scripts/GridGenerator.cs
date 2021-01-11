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

    public Tile[,] m_SelfGrid, m_EnemyGrid;

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
                m_EnemyGrid[i, j].m_EnemyNode.MatrixPosition = new Vector2Int(i, j);
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
                print("a");
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
        m_EnemyGrid[i, j].m_EnemyNode.ArcherValue = 0;
        m_EnemyGrid[i, j].m_EnemyNode.BaseValue = 0;
        m_EnemyGrid[i, j].m_EnemyNode.KingValue = 0;
        m_EnemyGrid[i, j].m_EnemyNode.KnightValue = 0;
        m_EnemyGrid[i, j].m_EnemyNode.VillageValue = 0;
        m_EnemyGrid[i, j].m_EnemyNode.BatValue = 0;
    }

    private void f_CheckWhatIsInTheEnemyTile(int x, int y)
    {
        Collider2D collider = Physics2D.OverlapCircle(m_EnemyGrid[x, y].m_EnemyNode.position, .3f, obstacleLayer);
        if(collider is null)
        {
            return;
        }
        if (collider.gameObject.name.Contains("Dark Bat"))
        {
            m_EnemyGrid[x, y].m_EnemyNode.BatValue = 1;
            f_EnemyMapFlooding(x, y, "Dark Bat");
        }
        else if (collider.gameObject.name.Contains("Dark King"))
        {
            m_EnemyGrid[x, y].m_EnemyNode.KingValue = 1;
            f_EnemyMapFlooding(x, y, "Dark King");
        }
        else if (collider.gameObject.name.Contains("Dark Knight"))
        {
            m_EnemyGrid[x, y].m_EnemyNode.KnightValue = 1;
            f_EnemyMapFlooding(x, y, "Dark Knight");
        }
        else if (collider.gameObject.name.Contains("Dark Archer"))
        {
            m_EnemyGrid[x, y].m_EnemyNode.ArcherValue = 1;
            f_EnemyMapFlooding(x, y, "Dark Archer");
        }
        else if (collider.gameObject.name.Contains("Dark Village"))
        {
            m_EnemyGrid[x, y].m_EnemyNode.VillageValue = 1;
            f_EnemyMapFlooding(x, y, "Dark Village");
        }
        else if (collider.gameObject.name.Contains("Dark Base"))
        {
            m_EnemyGrid[x, y].m_EnemyNode.BaseValue = 1;
            f_EnemyMapFlooding(x, y, "Dark Base");
        }
    }

    private void f_EnemyMapFlooding(int x, int y, string typeOfUnit)
    {
        Collider2D[] collidersOfTile = Physics2D.OverlapCircleAll(m_EnemyGrid[x, y].m_EnemyNode.position, 4.5f, tileLayer);

        foreach (var c in collidersOfTile)
        {
            Tile tile = c.gameObject.GetComponent<Tile>();

            float distance = Vector2.Distance(m_EnemyGrid[x, y].m_EnemyNode.position, m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.position);

            switch (typeOfUnit)
            {
                case "Dark Bat":
                    m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.BatValue += f_ChangeUnitValue(m_EnemyGrid[x, y].m_EnemyNode.BatValue, distance);
                    break;
                case "Dark King":
                    m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.KingValue += f_ChangeUnitValue(m_EnemyGrid[x, y].m_EnemyNode.KingValue, distance);
                    break;
                case "Dark Knight":
                    m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.KnightValue += f_ChangeUnitValue(m_EnemyGrid[x, y].m_EnemyNode.KnightValue, distance);
                    break;
                case "Dark Archer":
                    m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.ArcherValue += f_ChangeUnitValue(m_EnemyGrid[x, y].m_EnemyNode.ArcherValue, distance);
                    break;
                case "Dark Village":
                    m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.VillageValue += f_ChangeUnitValue(m_EnemyGrid[x, y].m_EnemyNode.VillageValue, distance);
                    break;
                case "Dark Base":
                    m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.BaseValue += f_ChangeUnitValue(m_EnemyGrid[x, y].m_EnemyNode.BaseValue, distance);
                    break;
                default:
                    break;
            }
            tile.m_EnemyNode.PopulateArray();
            /* //DEBUG, HAY VALORES MAYORES A 1, HABRIA QUE CAMPLEAR IMAGINO PERO IGUAL TE JODE ALGUNAS COSAS DE LA PROPAGACION
            if (m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.KnightValue >= 1)
            {
                tile.Highlight();
                print(m_EnemyGrid[tile.m_EnemyNode.MatrixPosition.x, tile.m_EnemyNode.MatrixPosition.y].m_EnemyNode.KnightValue);
            } 
            */
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
                m_SelfGrid[i, j].m_SelfNode.MatrixPosition = new Vector2Int(i, j);
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
        m_SelfGrid[i, j].m_SelfNode.ArcherValue = 0;
        m_SelfGrid[i, j].m_SelfNode.BaseValue = 0;
        m_SelfGrid[i, j].m_SelfNode.KingValue = 0;
        m_SelfGrid[i, j].m_SelfNode.KnightValue = 0;
        m_SelfGrid[i, j].m_SelfNode.VillageValue = 0;
        m_SelfGrid[i, j].m_SelfNode.BatValue = 0;
    }

    private void f_CheckWhatIsInTheSelfTile(int x, int y)
    {
        Collider2D collider = Physics2D.OverlapCircle(m_SelfGrid[x, y].m_SelfNode.position, .3f, obstacleLayer);
        if (collider is null)
        {
            return;
        }
        if (collider.gameObject.name.Contains("Blue Bat"))
        {
            m_SelfGrid[x, y].m_SelfNode.BatValue = 1;
            f_SelfMapFlooding(x, y, "Blue Bat");
        }
        else if (collider.gameObject.name.Contains("Blue King"))
        {
            m_SelfGrid[x, y].m_SelfNode.KingValue = 1;
            f_SelfMapFlooding(x, y, "Blue King");
        }
        else if (collider.gameObject.name.Contains("Blue Knight"))
        {
            m_SelfGrid[x, y].m_SelfNode.KnightValue = 1;
            f_SelfMapFlooding(x, y, "Blue Knight");
        }
        else if (collider.gameObject.name.Contains("Blue Archer"))
        {
            m_SelfGrid[x, y].m_SelfNode.ArcherValue = 1;
            f_SelfMapFlooding(x, y, "Blue Archer");
        }
        else if (collider.gameObject.name.Contains("Blue Village"))
        {
            m_SelfGrid[x, y].m_SelfNode.VillageValue = 1;
            f_SelfMapFlooding(x, y, "Blue Village");
        }
        else if (collider.gameObject.name.Contains("Blue Base"))
        {
            m_SelfGrid[x, y].m_SelfNode.BaseValue = 1;
            f_SelfMapFlooding(x, y, "Blue Base");
        }
    }

    private void f_SelfMapFlooding(int x, int y, string typeOfUnit)
    {
        Collider2D[] collidersOfTile = Physics2D.OverlapCircleAll(m_SelfGrid[x, y].m_SelfNode.position, 4.5f, tileLayer);

        foreach (var c in collidersOfTile)
        {
            Tile tile = c.gameObject.GetComponent<Tile>();

            float distance = Vector2.Distance(m_SelfGrid[x, y].m_SelfNode.position, m_SelfGrid[tile.m_SelfNode.MatrixPosition.x, tile.m_SelfNode.MatrixPosition.y].m_SelfNode.position);

            switch (typeOfUnit)
            {
                case "Blue Bat":
                    m_SelfGrid[tile.m_SelfNode.MatrixPosition.x, tile.m_SelfNode.MatrixPosition.y].m_SelfNode.BatValue += f_ChangeUnitValue(m_SelfGrid[x, y].m_SelfNode.BatValue, distance);
                    break;
                case "Blue King":
                    m_SelfGrid[tile.m_SelfNode.MatrixPosition.x, tile.m_SelfNode.MatrixPosition.y].m_SelfNode.KingValue += f_ChangeUnitValue(m_SelfGrid[x, y].m_SelfNode.KingValue, distance);
                    break;
                case "Blue Knight":
                    m_SelfGrid[tile.m_SelfNode.MatrixPosition.x, tile.m_SelfNode.MatrixPosition.y].m_SelfNode.KnightValue += f_ChangeUnitValue(m_SelfGrid[x, y].m_SelfNode.KnightValue, distance);
                    break;
                case "Blue Archer":
                    m_SelfGrid[tile.m_SelfNode.MatrixPosition.x, tile.m_SelfNode.MatrixPosition.y].m_SelfNode.ArcherValue += f_ChangeUnitValue(m_SelfGrid[x, y].m_SelfNode.ArcherValue, distance);
                    break;
                case "Blue Village":
                    m_SelfGrid[tile.m_SelfNode.MatrixPosition.x, tile.m_SelfNode.MatrixPosition.y].m_SelfNode.VillageValue += f_ChangeUnitValue(m_SelfGrid[x, y].m_SelfNode.VillageValue, distance);
                    break;
                case "Blue Base":
                    m_SelfGrid[tile.m_SelfNode.MatrixPosition.x, tile.m_SelfNode.MatrixPosition.y].m_SelfNode.BaseValue += f_ChangeUnitValue(m_SelfGrid[x, y].m_SelfNode.BaseValue, distance);
                    break;
                default:
                    break;
            }
            tile.m_SelfNode.PopulateArray();
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

    private void OnDrawGizmos()
    {
        if(m_EnemyGrid is null)
        {
            return;
        }
        for (int i = 0; i < m_EnemyGrid.GetLength(0); i++)
        {
            for (int j = 0; j < m_EnemyGrid.GetLength(1); j++)
            {
                float x = 0;
                foreach (float f in m_EnemyGrid[i, j].m_EnemyNode.Influence)
                    x += f;

                Gizmos.color = Color.Lerp(Color.black, Color.white, x);
                Gizmos.DrawCube(m_EnemyGrid[i, j].m_EnemyNode.position, Vector2.one);
            }
        }
    }

}
