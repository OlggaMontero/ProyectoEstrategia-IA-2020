using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    private List<BoxNode> grid = new List<BoxNode>();
    private Transform[] Tiles;

    private void Start()
    {
        Tiles = GameObject.Find("Tiles").GetComponentsInChildren<Transform>();
        grid = new List<BoxNode>();

        f_BuildGrid();
    }

    private void f_BuildGrid()
    {
        int i = 0;
        foreach (var tile in Tiles)
        {
            BoxNode node = new BoxNode();

            node.position = new Vector2(tile.position.x, tile.position.y);
            Collider2D[] colliders = Physics2D.OverlapBoxAll(node.position, new Vector2(.5f, .5f), 0f);
            foreach (var c in colliders)
            {
                if (c.gameObject.name.Contains("Dark Bat"))
                {
                    //Hacer lo del murcielago
                    node.BatValue = 1;
                    node.ArcherValue = 0;
                    node.KingValue = 0;
                    node.KnightValue = 0;
                    node.VillageValue = 0;
                    node.nodeIndex = i;
                    break;
                }
                else if (c.gameObject.name.Contains("Dark King"))
                {
                    //Hacer lo del rey
                    node.BatValue = 0;
                    node.ArcherValue = 0;
                    node.KingValue = 1;
                    node.KnightValue = 0;
                    node.VillageValue = 0;
                    node.nodeIndex = i;
                    break;
                }
                else if (c.gameObject.name.Contains("Dark Knight"))
                {
                    //Hacer lo del caballero
                    node.BatValue = 0;
                    node.ArcherValue = 0;
                    node.KingValue = 0;
                    node.KnightValue = 1;
                    node.VillageValue = 0;
                    node.nodeIndex = i;
                    break;
                }
                else if (c.gameObject.name.Contains("Dark Village"))
                {
                    //Hacer lo de la casa
                    node.BatValue = 0;
                    node.ArcherValue = 0;
                    node.KingValue = 0;
                    node.KnightValue = 0;
                    node.VillageValue = 1;
                    node.nodeIndex = i;
                    break;
                }
                else if (c.gameObject.name.Contains("Dark Archer"))
                {
                    //Hacer lo del arquero
                    node.BatValue = 0;
                    node.ArcherValue = 1;
                    node.KingValue = 0;
                    node.KnightValue = 0;
                    node.VillageValue = 0;
                    node.nodeIndex = i;
                    break;
                }
                grid.Add(node);
            }
            i++;
        }
        foreach (var item in grid)
        {
            print(item.position);
        }
    }


    private void OnDrawGizmos()
    {
        foreach (var item in grid)
        {
            if(item.KingValue != 0) Gizmos.color = Color.red;
            else if(item.ArcherValue != 0) Gizmos.color = Color.yellow;
            else if (item.VillageValue != 0) Gizmos.color = Color.black;
            else if (item.KnightValue != 0) Gizmos.color = Color.blue;
            else if (item.BatValue != 0) Gizmos.color = Color.green;
            else Gizmos.color = Color.white;
            Gizmos.DrawCube(item.position, Vector2.one);


        }
    }
}
