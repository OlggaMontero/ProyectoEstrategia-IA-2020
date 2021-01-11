using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer rend;
    public Color highlightedColor;
    public Color creatableColor;
    public Color fogColor;

    public LayerMask obstacles;

    public bool isWalkable;
    public bool isCreatable;
    public bool isNearToBase;
    public bool isNearToVillage;
    public bool isNearToBaseVillage;

    [HideInInspector] public int m_nearToBaseIndex = 0;

    private GM gm;

    public float amount;
    private bool sizeIncrease;

	private AudioSource source;

    public BoxNode node;

    private void Start()
    {
		source = GetComponent<AudioSource>();
        gm = FindObjectOfType<GM>();
        rend = GetComponent<SpriteRenderer>();

        //calcular si es una tile cercana a una base o no
        House[] bases = FindObjectsOfType<House>();
        isNearToBase = false;
        m_nearToBaseIndex = 0;
        f_InitializeNode();
        foreach (House b in bases)
        {
            if (Vector2.Distance(transform.position, b.transform.position) < FindObjectOfType<CharacterCreation>().m_distanceToAllowSpawnForVillage)
            {
                isNearToBaseVillage = true;                
                m_nearToBaseIndex = b.playerNumber;                
            }
            if (Vector2.Distance(transform.position, b.transform.position) < FindObjectOfType<CharacterCreation>().m_distanceToAllowSpawn)
            {
                isNearToBase = true;
                m_nearToBaseIndex = b.playerNumber;
                break;
            }
        }
        
    }

    private void f_InitializeNode()
    {
        node = new BoxNode(0, 0, 0, 0, 0, 0, new Vector2(transform.position.x, transform.position.y), Vector2Int.zero);
    }

    public void checkTilesNearVillages()
    {
        Village[] villages = FindObjectsOfType<Village>();
        foreach (Village v in villages)
        {            
            if (Vector2.Distance(transform.position, v.transform.position) < FindObjectOfType<CharacterCreation>().m_distanceToAllowSpawn)
            {
                isNearToVillage = true;
                m_nearToBaseIndex = v.playerNumber;
                break;
            }
        }
    }

    public bool isClear() // does this tile have an obstacle on it. Yes or No?
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 0.2f, obstacles);
        if (col == null)
        {
            return true;
        }
        else {
            return false;
        }
    }


    public bool isPreparedForSpawn(bool village)
    {
        if(village)
            return (isNearToBaseVillage || isNearToVillage) && isClear();
        else return (isNearToBase || isNearToVillage) && isClear();
    }

    public void Highlight() {
		
        rend.color = highlightedColor;
        isWalkable = true;
    }

    public void Reset()
    {
        if(unidadesViendome >= 1)
        {
            rend.color = Color.white;
        }
        isWalkable = false;
        isCreatable = false;
    }

    public void SetCreatable() {
        rend.color = creatableColor;
        isCreatable = true;
    }

    
    private void OnMouseDown()
    {
        if (isWalkable == true) {
            gm.selectedUnit.Move(this.transform);
        } else if (isCreatable == true && gm.createdUnit != null) {
            Unit unit = Instantiate(gm.createdUnit, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            unit.hasMoved = true;
            unit.hasAttacked = true;
            gm.ResetTiles();
            gm.createdUnit = null;
        } else if (isCreatable == true && gm.createdVillage != null) {
            Instantiate(gm.createdVillage, new Vector3(transform.position.x, transform.position.y, 0) , Quaternion.identity);
            gm.ResetTiles();
            gm.createdVillage = null;
        }
    }


    private void OnMouseEnter()
    {
        if (isClear() == true) {
			source.Play();
			sizeIncrease = true;
            transform.localScale += new Vector3(amount, amount, amount);
        }
        
    }

    private void OnMouseExit()
    {
        if (isClear() == true)
        {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }

        if (isClear() == false && sizeIncrease == true) {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }
    }


    public void VistoPorUnidad()
    {
        unidadesViendome += 1;
        if (unidadesViendome >= 1)
        {
            Mostrar();
        }
        if (unidadesViendome < 0)
        {
            Debug.Log("bug -> tile.cs, no pueden haber negativos enemigos viendote");
        }
    }

    public void DesvistoPorUnidad()
    {
        unidadesViendome -= 1;
        if (unidadesViendome == 0)
        {
            Esconder();
        }
        if (unidadesViendome < 0)
        {
            Debug.Log("bug -> tile.cs, no pueden haber negativos enemigos viendote");
        }
    }

    public void Mostrar()
    {
        if (rend == null)
        {
            rend = GetComponent<SpriteRenderer>();
        }
        rend.color = Color.white;
    }

    public void Esconder()
    {
        if (rend == null)
        {
            rend = GetComponent<SpriteRenderer>();
        }
        rend.color = fogColor;
    }

    public void ResetVision()
    {
        unidadesViendome = 0;
    }

}
