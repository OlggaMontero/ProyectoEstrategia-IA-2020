using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour
{
    private GM gm;

    public GameObject weaponIcon;   

    public int playerNumber;
    bool isActive;

    public int health;
    public int armor;
    public int defenseDamage;

    public Text displayedText;

    private int enemigosQueMeVen = 0;
    private HashSet<GameObject> enemigosVistos = new HashSet<GameObject>();
    private HashSet<GameObject> tilesVistos = new HashSet<GameObject>();
    public int viewRadius = 2;



    private void Start()
    {
        gm = FindObjectOfType<GM>();
        UpdateHealthDisplay();
    }
  
    private void OnMouseDown() // attack
    {
        if(gm.playerTurn == 1)
        {
            if (gm.selectedUnit == null) return;

            if (playerNumber == gm.playerTurn)
            {
                //nada de momento?
            }
            else
            {
                gm.selectedUnit.ResetWeaponIcons();
                if (gm.selectedUnit.m_enemyBaseInRange && !gm.selectedUnit.hasAttacked && gm.selectedUnit.enemyBase == this.gameObject)
                {
                    gm.selectedUnit.AttackBase(this);
                }
            }
        } 
    }

    public void UpdateHealthDisplay()
    {
        displayedText.text = health.ToString();
    }

    public void GetVisibleEnemies()
    {
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
