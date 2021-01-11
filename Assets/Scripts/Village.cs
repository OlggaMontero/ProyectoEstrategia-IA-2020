using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    public int goldPerTurn;
    public int playerNumber;
    public int cost;

    private int enemigosQueMeVen = 0;
    private HashSet<GameObject> enemigosVistos = new HashSet<GameObject>();
    private HashSet<GameObject> tilesVistos = new HashSet<GameObject>();
    public int viewRadius = 2;


    public void GetVisibleEnemies()
    {
        GM gm = FindObjectOfType<GM>();

        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            if (enemy.playerNumber != gm.playerTurn)
            {
                if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= viewRadius) // check is the enemy is near enough to attack
                {
                    if (!enemigosVistos.Contains(enemy.gameObject))
                    {
                        enemy.VistoPorEnemigo();
                        enemigosVistos.Add(enemy.gameObject);
                    }
                }
                else
                {
                    if (enemigosVistos.Contains(enemy.gameObject))
                    {
                        enemy.DesvistoPorEnemigo();
                        enemigosVistos.Remove(enemy.gameObject);
                    }
                }

            }
        }
    }

    public void VisibleTiles()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= viewRadius)
            {
                if (!tilesVistos.Contains(tile.gameObject))
                {
                    tile.VistoPorUnidad();
                    tilesVistos.Add(tile.gameObject);
                }
            }
            else
            {
                if (tilesVistos.Contains(tile.gameObject))
                {
                    tile.DesvistoPorUnidad();
                    tilesVistos.Remove(tile.gameObject);
                }
            }
        }
    }
    public void VistoPorEnemigo()
    {
        enemigosQueMeVen += 1;
        if (enemigosQueMeVen >= 1)
        {
            Mostrar();
        }
        if (enemigosQueMeVen < 0)
        {
            Debug.Log("bug -> unit.cs, no pueden haber negativos enemigos viendote");
        }
    }

    public void DesvistoPorEnemigo()
    {
        enemigosQueMeVen -= 1;
        if (enemigosQueMeVen == 0)
        {
            Esconder();
        }
        if (enemigosQueMeVen < 0)
        {
            Debug.Log("bug -> unit.cs, no pueden haber negativos enemigos viendote");
        }
    }

    public void Mostrar()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1.0f);
        }
    }

    public void Esconder()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.0f);
        }
    }

    public void ResetVision()
    {
        enemigosQueMeVen = 0;
        enemigosVistos.Clear();
        tilesVistos.Clear();
    }
}
