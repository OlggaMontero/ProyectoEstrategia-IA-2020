using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private void Start()
    {
        source = GetComponent<AudioSource>();
        camAnim = Camera.main.GetComponent<Animator>();
        GetGoldIncome(1);
        FogTiles();
        Unit[] units = FindObjectsOfType<Unit>();
        House[] houses = FindObjectsOfType<House>();
        Village[] villages = FindObjectsOfType<Village>();
        MostrarUnidadesJugador(units, villages, houses);
        CompruebaVisionUnidades(units, villages, houses);
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
        Village[] villages = FindObjectsOfType<Village>();
        House[] houses = FindObjectsOfType<House>();

        if (playerTurn == 1) {
            playerIcon.sprite = playerTwoIcon;
            playerTurn = 2;
        } else if (playerTurn == 2) {
            playerIcon.sprite = playerOneIcon;
            playerTurn = 1;
        }

        FogTiles();
        MostrarUnidadesJugador(units, villages, houses);
        CompruebaVisionUnidades(units, villages, houses);
        GetGoldIncome(playerTurn);
        GetComponent<CharacterCreation>().CloseCharacterCreationMenus();
        createdUnit = null;
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
                    player1Gold += goldIncomePerTurn / 2;
                }
                else
                {
                    player2Gold += goldIncomePerTurn / 2;
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


    public void MostrarUnidadesJugador(Unit[] unidades, Village[] villages, House[] houses)
    {
        foreach (Unit unit in unidades)
        {
            if (playerTurn == unit.playerNumber)
            {
                unit.Mostrar();
            }
            else
            {
                unit.Esconder();
            }
            unit.ResetVision();
            
        }

        foreach (Village village in villages)
        {
            if (playerTurn == village.playerNumber)
            {
                village.Mostrar();
            }
            else
            {
                village.Esconder();
            }
            village.ResetVision();
        }

        foreach (House house in houses)
        {
            if (playerTurn == house.playerNumber)
            {
                house.Mostrar();
            }
            else
            {
                house.Esconder();
            }
            house.ResetVision();
        }
    }

    public void CompruebaVisionUnidades(Unit[] unidades, Village[] villages, House[] houses)
    {
        foreach(Unit unit in unidades)
        {
            if (playerTurn == unit.playerNumber)
            {
                unit.GetVisibleEnemies();
                unit.VisibleTiles();
            }
        }

        foreach (House house in houses)
        {
            if (playerTurn == house.playerNumber)
            {
                house.GetVisibleEnemies();
                house.VisibleTiles();
            }
        }

        foreach (Village village in villages)
        {
            if (playerTurn == village.playerNumber)
            {
                village.GetVisibleEnemies();
                village.VisibleTiles();
            }
        }
    }

    public void FogTiles()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile.ResetVision();
            tile.Esconder();
        }
    }
}
