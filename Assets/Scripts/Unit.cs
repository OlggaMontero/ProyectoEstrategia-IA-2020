using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public bool isSelected;
    public bool hasMoved;

    public int tileSpeed;
    public float moveSpeed;

    private GM gm;

    public int attackRadius;
    public int viewRadius;
    public bool hasAttacked;
    public List<Unit> enemiesInRange = new List<Unit>();

    public int playerNumber;

    public GameObject weaponIcon;


    // Attack Stats
    public int health;
    public int attackDamage;
    public int defenseDamage;
    public int armor;
    public bool m_enemyBaseInRange;

    [HideInInspector] public GameObject enemyBase;

    public DamageIcon damageIcon;

    public int cost;

	public GameObject deathEffect;

	private Animator camAnim;

    public bool isKing;

	private AudioSource source;

    public Text displayedText;

    public Collider2D fieldOfView;

    private int enemigosQueMeVen = 0;

    private HashSet<GameObject> objetosVistos = new HashSet<GameObject>();
    private HashSet<GameObject> tilesVistos = new HashSet<GameObject>();



    private void Start()
    {
		source = GetComponent<AudioSource>();
		camAnim = Camera.main.GetComponent<Animator>();
        gm = FindObjectOfType<GM>();
        UpdateHealthDisplay();

        //enemy base
        if (playerNumber == 1)
        {
            enemyBase = GameObject.Find("Blue House");
        }
        else
        {
            enemyBase = GameObject.Find("Dark House");
        }
    }

    private void UpdateHealthDisplay ()
    {
        if (isKing)
        {
            displayedText.text = health.ToString();
        }
    }

    #region Mouse control

    
    private void OnMouseDown() // select character or deselect if already selected
    {
        
        ResetWeaponIcons();

        if (isSelected == true)
        {
            
            isSelected = false;
            gm.selectedUnit = null;
            gm.ResetTiles();

        }
        else { //select character
            if (playerNumber == gm.playerTurn) { // select unit only if it's his turn
                if (gm.selectedUnit != null)
                { // deselect the unit that is currently selected, so there's only one isSelected unit at a time
                    gm.selectedUnit.isSelected = false;
                }
                gm.ResetTiles();

                gm.selectedUnit = this;

                isSelected = true;
				if(source != null){
					source.Play();
				}
				
                GetWalkableTiles();
                GetEnemies();                
            }

        }



        Collider2D col = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.15f);
        if (col != null)
        {            
            Unit unit = col.GetComponent<Unit>(); // double check that what we clicked on is a unit
            if (unit != null && gm.selectedUnit != null)
            {
                if (gm.selectedUnit.enemiesInRange.Contains(unit) && !gm.selectedUnit.hasAttacked)
                { // does the currently selected unit have in his list the enemy we just clicked on
                    gm.selectedUnit.Attack(unit);
                }
            }
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            gm.UpdateInfoPanel(this);
        }
    }

    #endregion

    #region Get info

    void GetWalkableTiles() { // Looks for the tiles the unit can walk on
        if (hasMoved == true) {
            return;
        }

        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles) {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
            { // how far he can move
                if (tile.isClear() == true)
                { // is the tile clear from any obstacles
                    tile.Highlight();
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



    /// <summary>
    /// Coger lista de enemigos en rango
    /// </summary>
    void GetEnemies() {
    
        enemiesInRange.Clear();

        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= attackRadius) // check is the enemy is near enough to attack
            {
                if (enemy.playerNumber != gm.playerTurn && !hasAttacked) { // make sure you don't attack your allies
                    enemiesInRange.Add(enemy);
                    enemy.weaponIcon.SetActive(true);
                }

            }
        }

        //coger tambien la base enemiga        
        if (EnemyBaseInRange() && !hasAttacked) enemyBase.GetComponent<House>().weaponIcon.SetActive(true);
    }

    public void GetVisibleEnemies()
    {
        enemiesInRange.Clear();

        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            if (enemy.playerNumber != gm.playerTurn)
            {
                if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= viewRadius) // check is the enemy is near enough to attack
                {
                    if (!objetosVistos.Contains(enemy.gameObject))
                    {
                        enemy.VistoPorEnemigo();
                        objetosVistos.Add(enemy.gameObject);
                    }
                }
                else
                {
                    if (objetosVistos.Contains(enemy.gameObject))
                    {
                        enemy.DesvistoPorEnemigo();
                        objetosVistos.Remove(enemy.gameObject);
                    }
                }
            
            }
        }
        VisibleStructures();
    }

    public void VisibleStructures()
    {
        Village[] villages = FindObjectsOfType<Village>();
        foreach (Village village in villages)
        {
            if (village.playerNumber != gm.playerTurn)
            {
                if (Mathf.Abs(transform.position.x - village.transform.position.x) + Mathf.Abs(transform.position.y - village.transform.position.y) <= viewRadius) // check is the enemy is near enough to attack
                {
                    if (!objetosVistos.Contains(village.gameObject))
                    {
                        village.VistoPorEnemigo();
                        objetosVistos.Add(village.gameObject);
                    }
                }
                else
                {
                    if (objetosVistos.Contains(village.gameObject))
                    {
                        village.DesvistoPorEnemigo();
                        objetosVistos.Remove(village.gameObject);
                    }
                }

            }
        }

        House[] houses = FindObjectsOfType<House>();
        foreach (House house in houses)
        {
            if (house.playerNumber != gm.playerTurn)
            {
                if (Mathf.Abs(transform.position.x - house.transform.position.x) + Mathf.Abs(transform.position.y - house.transform.position.y) <= viewRadius) // check is the enemy is near enough to attack
                {
                    if (!objetosVistos.Contains(house.gameObject))
                    {
                        house.VistoPorEnemigo();
                        objetosVistos.Add(house.gameObject);
                    }
                }
                else
                {
                    if (objetosVistos.Contains(house.gameObject))
                    {
                        house.DesvistoPorEnemigo();
                        objetosVistos.Remove(house.gameObject);
                    }
                }

            }
        }

    }


    private bool EnemyBaseInRange()
    {
        return false;
        m_enemyBaseInRange = (Mathf.Abs(transform.position.x - enemyBase.transform.position.x) + Mathf.Abs(transform.position.y - enemyBase.transform.position.y) <= attackRadius);
        return m_enemyBaseInRange;
    }


    #endregion

    #region Actions

    public void Move(Transform movePos)
    {
        gm.ResetTiles();
        StartCoroutine(StartMovement(movePos));
    }

    public void AttackBase(House enemyBase)
    {
        hasAttacked = true;

        int enemyDamage = attackDamage;// - enemyBase.armor;
        //int unitDamage = enemyBase.defenseDamage - armor;

        /*if (unitDamage >= 1)
        {
            health -= unitDamage;
            UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
            d.Setup(unitDamage);
        }
        */
        if (enemyDamage >= 1)
        {
            enemyBase.health -= enemyDamage;
            if (enemyBase.health <= 0)
            {
                enemyBase.health = 0;
                camAnim.SetTrigger("shake");

                gm.ShowVictoryPanel(enemyBase.playerNumber);

                GetWalkableTiles(); // check for new walkable tiles (if enemy has died we can now walk on his tile)            
                Destroy(enemyBase.gameObject);
            }            
            enemyBase.UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, enemyBase.transform.position, Quaternion.identity);
            d.Setup(enemyDamage);
        }


        /*if (health <= 0)
        {

            if (deathEffect != null)
            {
                Instantiate(deathEffect, enemyBase.transform.position, Quaternion.identity);
                camAnim.SetTrigger("shake");
            }

            if (isKing)
            {
                gm.ShowVictoryPanel(playerNumber);
            }

            gm.ResetTiles(); // reset tiles when we die
            gm.RemoveInfoPanel(this);
            Destroy(gameObject);
        }*/
    }

    void Attack(Unit enemy) {
        hasAttacked = true;

        int enemyDamege = attackDamage - enemy.armor;
        int unitDamage = enemy.defenseDamage - armor;

        if (enemyDamege >= 1)
        {
            enemy.health -= enemyDamege;
            enemy.UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, enemy.transform.position, Quaternion.identity);
            d.Setup(enemyDamege);
        }

        if (transform.tag == "Archer" && enemy.tag != "Archer")
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= 1) // check is the enemy is near enough to attack
            {
                if (unitDamage >= 1)
                {
                    health -= unitDamage;
                    UpdateHealthDisplay();
                    DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                    d.Setup(unitDamage);
                }
            }
        } else {
            if (unitDamage >= 1)
            {
                health -= unitDamage;
                UpdateHealthDisplay();
                DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                d.Setup(unitDamage);
            }
        }

        if (enemy.health <= 0)
        {
         
            if (deathEffect != null){
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

            if (enemy.isKing)
            {
                gm.ShowVictoryPanel(enemy.playerNumber);
            }

            GetWalkableTiles(); // check for new walkable tiles (if enemy has died we can now walk on his tile)
            gm.RemoveInfoPanel(enemy);
            Destroy(enemy.gameObject);
        }

        if (health <= 0)
        {

            if (deathEffect != null)
			{
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

			if (isKing)
            {
                gm.ShowVictoryPanel(playerNumber);
            }

            gm.ResetTiles(); // reset tiles when we die
            gm.RemoveInfoPanel(this);
            Destroy(gameObject);
        }

        gm.UpdateInfoStats();
  

    }

    #endregion

    public void ResetWeaponIcons() {
        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            enemy.weaponIcon.SetActive(false);
        }
        //resetear icono de las bases
        House[] bases = FindObjectsOfType<House>();
        foreach (House h in bases)
        {
            h.weaponIcon.SetActive(false);            
        }
    }

    IEnumerator StartMovement(Transform movePos) { // Moves the character to his new position.

        while (transform.position.x != movePos.position.x) { // first aligns him with the new tile's x pos
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(movePos.position.x, transform.position.y), moveSpeed * Time.deltaTime);
            GetVisibleEnemies();
            VisibleTiles();
            yield return null;
        }
        while (transform.position.y != movePos.position.y) // then y
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, movePos.position.y), moveSpeed * Time.deltaTime);
            GetVisibleEnemies();
            VisibleTiles();
            yield return null;
        }

        hasMoved = true;
        ResetWeaponIcons();
        GetEnemies();
        gm.MoveInfoPanel(this);
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
        objetosVistos.Clear();
        tilesVistos.Clear();
    }



}
