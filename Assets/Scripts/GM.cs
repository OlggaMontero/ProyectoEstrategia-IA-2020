using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GM : MonoBehaviour
{
    public Unit selectedUnit;

    public int playerTurn = 1;

    public Transform selectedUnitSquare;


    private Animator camAnim;
    public Image playerIcon; 
    public Sprite playerOneIcon;
    public Sprite playerTwoIcon;

    public GameObject unitInfoPanel;
    public Vector2 unitInfoPanelShift;
    Unit currentInfoUnit;
    public Text heathInfo;
    public Text attackDamageInfo;
    public Text armorInfo;
    public Text defenseDamageInfo;

    public int player1Gold;
    public int player2Gold;
    public int goldIncomePerTurn;

    public Text player1GoldText; 
    public Text player2GoldText;

    public Unit createdUnit;
    public Village createdVillage;

    public GameObject blueVictory;
    public GameObject darkVictory;

	private AudioSource source;

    //IA stuff
    private CharacterCreation _ch_Craeation;

    public Unit _blue_archer;
    public Unit _blue_bat;
    public Unit _blue_knight;
    public Village _blue_village;

    private House _ia_house;
    private House _player_house;
    private List<Unit> _ia_units;
    public List<Village> _ia_villages;

    private void Start()
    {
        _ch_Craeation = GetComponent<CharacterCreation>();
        House[] bases = FindObjectsOfType<House>();
        foreach (House b in bases)
        {
            if (b.playerNumber == 2) _ia_house = b;
            else _player_house = b;
        }
        _ia_units = new List<Unit>();
        source = GetComponent<AudioSource>();
        camAnim = Camera.main.GetComponent<Animator>();
        GetGoldIncome(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("b")) {
            EndTurn();
        }

        if (selectedUnit != null) // moves the white square to the selected unit!
        {
            selectedUnitSquare.gameObject.SetActive(true);
            selectedUnitSquare.position = selectedUnit.transform.position;
        }
        else
        {
            selectedUnitSquare.gameObject.SetActive(false);
        }

    }

    // Sets panel active/inactive and moves it to the correct place
    public void UpdateInfoPanel(Unit unit) {

        if (unit.Equals(currentInfoUnit) == false)
        {
            unitInfoPanel.transform.position = (Vector2)unit.transform.position + unitInfoPanelShift;
            unitInfoPanel.SetActive(true);

            currentInfoUnit = unit;

            UpdateInfoStats();

        } else {
            unitInfoPanel.SetActive(false);
            currentInfoUnit = null;
        }

    }

    // Updates the stats of the infoPanel
    public void UpdateInfoStats() {
        if (currentInfoUnit != null)
        {
            attackDamageInfo.text = currentInfoUnit.attackDamage.ToString();
            defenseDamageInfo.text = currentInfoUnit.defenseDamage.ToString();
            armorInfo.text = currentInfoUnit.armor.ToString();
            heathInfo.text = currentInfoUnit.health.ToString();
        }
    }

    // Moves the udpate panel (if the panel is actived on a unit and that unit moves)
    public void MoveInfoPanel(Unit unit) {
        if (unit.Equals(currentInfoUnit))
        {
            unitInfoPanel.transform.position = (Vector2)unit.transform.position + unitInfoPanelShift;
        }
    }

    // Deactivate info panel (when a unit dies)
    public void RemoveInfoPanel(Unit unit) {
        if (unit.Equals(currentInfoUnit))
        {
            unitInfoPanel.SetActive(false);
			currentInfoUnit = null;
        }
    }

    public void ResetTiles() {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }
    public void EndTurn() {        
        source.Play();
        camAnim.SetTrigger("shake");

        // deselects the selected unit when the turn ends
        if (selectedUnit != null) {
            selectedUnit.ResetWeaponIcons();
            selectedUnit.isSelected = false;
            selectedUnit = null;
        }

        ResetTiles();

        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units) {
            unit.hasAttacked = false;
            unit.hasMoved = false;
            unit.ResetWeaponIcons();
        }

        if (playerTurn == 1) {
            playerIcon.sprite = playerTwoIcon;
            playerTurn = 2;
        } else if (playerTurn == 2) {
            playerIcon.sprite = playerOneIcon;
            playerTurn = 1;
        }

        GetGoldIncome(playerTurn);
        GetComponent<CharacterCreation>().CloseCharacterCreationMenus();
        createdUnit = null;
        createdVillage = null; 

        if (playerTurn == 2)
        {
            f_GetUnits();
            DoIaStuff();
        }
    }

    /// <summary>
    /// Arbol de decisiones que decide cosas que se han de decidir
    /// </summary>
    private void DoIaStuff()
    {

        if (_ia_villages.Count < 3 && player2Gold > 100) f_Buy_Village();
        if (_ia_units.Count < 3 && player2Gold > 40) f_Buy_Unit();


        //IA Functions
        //In Tile: f_Buy_Move()
        //In Unit: f_Select_Unit_attack(); Attack(Unit enemy); AttackBase(House enemyBase); (bool)m_enemyBaseInRange; (list)_tilesReacheable;
        //In CharacterCreation: BuyUnit(Unit Unit); BuyVillage(Village village);
        //createdUnit: _blue_archer; _blue_bat; _blue_knight;
        //createdVillage: _blue_village;
        //(list) _ia_units;
        //(House) _ia_House;
        //player2Gold;
    }

    private void f_GetUnits()
    {
        _ia_units.Clear();
        Unit[] units = FindObjectsOfType<Unit>();
        foreach(Unit unit in units)
        {
            if(unit.playerNumber == 2)
            {
                _ia_units.Add(unit);
            }
        }
    }

    private void f_Buy_Village()
    {
        _ch_Craeation.BuyVillage(_blue_village);
        Tile spawn_tile = null;
        foreach(Tile t in _ch_Craeation._tiles_to_place_things)
        {
            if (spawn_tile != null && Vector2.Distance(_player_house.transform.position, t.transform.position) < Vector2.Distance(_player_house.transform.position, spawn_tile.transform.position)) //Distancia mas proxima a la base enemiga
                spawn_tile = t;
        }
        spawn_tile.f_Buy_Move();
    }

    private void f_Buy_Unit()
    {
        if (player2Gold >= 80) _ch_Craeation.BuyUnit(_blue_bat);
        else if(player2Gold >= 70) _ch_Craeation.BuyUnit(_blue_archer);
        else _ch_Craeation.BuyUnit(_blue_knight);

        Tile spawn_tile = null;
        foreach (Tile t in _ch_Craeation._tiles_to_place_things)
        {
            if (spawn_tile != null && Vector2.Distance(_player_house.transform.position, t.transform.position) < Vector2.Distance(_player_house.transform.position, spawn_tile.transform.position)) //DECIDIR CON MAPA DE INFLUENCIA-----------------------------!*!*!*!*!*!*!*!*!*!*!*!*!*!*!*!
                spawn_tile = t;
        }
        spawn_tile.f_Buy_Move();
    }

    /// <summary>
    /// Añade 1 de oro al jugador en turno por cada pueblo en el tablero
    /// </summary>
    /// <param name="playerTurn"></param>
    void GetGoldIncome(int playerTurn) {
        foreach (Village village in FindObjectsOfType<Village>())
        {
            if (village.playerNumber == playerTurn)
            {
                if (playerTurn == 1)
                {
                    player1Gold += goldIncomePerTurn/2;
                }
                else
                {
                    player2Gold += goldIncomePerTurn/2;
                }
            }
        }
        if (playerTurn == 1)
        {
            player1Gold += goldIncomePerTurn;
        }
        else
        {
            player2Gold += goldIncomePerTurn;
        }
        UpdateGoldText();
    }

    /// <summary>
    /// Actualiza la UI
    /// </summary>
    public void UpdateGoldText()
    {
        player1GoldText.text = player1Gold.ToString();
        player2GoldText.text = player2Gold.ToString();
    }

    // Victory UI
    /// <summary>
    /// Mostrar UI de victoria
    /// </summary>
    /// <param name="playerNumber"></param>
    public void ShowVictoryPanel(int playerNumber) {

        if (playerNumber == 1)
        {
            blueVictory.SetActive(true);
        } else if (playerNumber == 2) {
            darkVictory.SetActive(true);
        }
    }

    /// <summary>
    /// Pues eso
    /// </summary>
    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


}
