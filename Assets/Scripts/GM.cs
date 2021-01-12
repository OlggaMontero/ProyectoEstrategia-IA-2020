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
    private string _state; //"Normal", "Attack", "Defend"

    private float timeToPassToNextUnit = 0;
    private bool timer = false;

    [Header ("Change the AI State to Defend")]
    [SerializeField] private float minimumDistanceOfEnemiesToBase;
    [SerializeField] private int numberOfEnemiesNearToBase;
    [Space (20)]

    public Unit _blue_archer;
    public Unit _blue_bat;
    public Unit _blue_knight;
    public Village _blue_village;

    private House _ia_house;
    private House _player_house;
    private List<Unit> _ia_units;
    private Unit[] _ia_units_array;
    public List<Village> _ia_villages;



    private void Start()
    {
        _state = "Normal";
        _ch_Craeation = GetComponent<CharacterCreation>();

        //FogTiles();
        Unit[] units = FindObjectsOfType<Unit>();
        House[] bases = FindObjectsOfType<House>();
        foreach (House b in bases)
        {
            if (b.playerNumber == 2) _ia_house = b;
            else _player_house = b;
        }
        House[] houses = FindObjectsOfType<House>();
        Village[] villages = FindObjectsOfType<Village>();
        //MostrarUnidadesJugador(units, villages, houses);
        //CompruebaVisionUnidades(units, villages, houses);

        _ia_units = new List<Unit>();
        source = GetComponent<AudioSource>();
        camAnim = Camera.main.GetComponent<Animator>();
        GetGoldIncome(1);
    }

    private void Update()
    {
        if (/*playerTurn == 1 &&*/ (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("b"))) {
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

        if (timer)
        {
            timeToPassToNextUnit += Time.deltaTime;
        }
        else
        {
            timeToPassToNextUnit = 0;
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

        //Village[] villages = FindObjectsOfType<Village>();
        //House[] houses = FindObjectsOfType<House>();

        if (playerTurn == 1) {
            playerIcon.sprite = playerTwoIcon;
            playerTurn = 2;
        } else if (playerTurn == 2) {
            playerIcon.sprite = playerOneIcon;
            playerTurn = 1;
        }

        //FogTiles();
        //MostrarUnidadesJugador(units, villages, houses);
        //CompruebaVisionUnidades(units, villages, houses);

        GetGoldIncome(playerTurn);
        GetComponent<CharacterCreation>().CloseCharacterCreationMenus();
        createdUnit = null;
        createdVillage = null;

        GridGenerator.instance.f_ClearEnemyNodes();
        GridGenerator.instance.f_GenerateEnemyInfluenceMap();

        if (playerTurn == 2)
        {
            f_GetUnits();
            DoIaStuff();
        }
    }

    //IA Functions
    //In Tile: f_Buy_Move()
    //In Unit: f_Select_Unit_attack(); Attack(Unit enemy); AttackBase(House enemyBase); (bool)m_enemyBaseInRange; (list)_tilesReacheable;
    //In CharacterCreation: BuyUnit(Unit Unit); BuyVillage(Village village);
    //createdUnit: _blue_archer; _blue_bat; _blue_knight;
    //createdVillage: _blue_village;
    //(list) _ia_units;
    //(House) _ia_House;
    //player2Gold;

    /// <summary>
    /// Arbol de decisiones que decide cosas que se han de decidir
    /// </summary>
    private void DoIaStuff()
    {
        //Compruebo el cambio de estado
        int enemiesNearBase = 0;
        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
            if (enemy.playerNumber != playerTurn && Vector2.Distance(_ia_house.transform.position, enemy.transform.position) < minimumDistanceOfEnemiesToBase) // check is the enemy is near enough to base
                enemiesNearBase++;

        if (enemiesNearBase >= numberOfEnemiesNearToBase)
            _state = "Defend";
        else
            _state = "Attack";

        //print(_state);

        //Fase de compra
        f_Buy_Stage();


        //Units behaviour
        Unit aux = null;

        for (int i = 0; i < _ia_units_array.Length; i++)
        {
            _ia_units_array[i].f_Select_Unit_attack();
            if (_ia_units_array[i].enemiesInRange.Count <= 0 || !_ia_units_array[i].m_enemyBaseInRange)
            {
                if (i > 0)
                    StartCoroutine(WaitToExecute(aux, _ia_units_array[i], "Move")); //COMO COÑO PUEDE DAR ESTO OUT OF RANGE???? ME LO EXPLICAIS?????
                else
                    f_ia_Move(_ia_units_array[i]);
            }
            else
            {
                if(i > 0)
                    StartCoroutine(WaitToExecute(aux, _ia_units_array[i], "Attack"));
                else
                    f_ia_Attack(_ia_units_array[i]);
            }
            aux = _ia_units_array[i];
        }
        //Acaba el turno cuando haya terminado su accion la ultima unidad
        StartCoroutine(WaitToEndTurn(aux, () => EndTurn()));
    }

    IEnumerator WaitToEndTurn(Unit iaUnit, Action FirstAction) //La siguiente Funcion se ejecuta cuando haya acabado la anterior la anterior unidad su accion
    {
        if(iaUnit != null)
        {
            yield return new WaitUntil(() => iaUnit.hasMoved || iaUnit.hasAttacked);
            yield return new WaitForSeconds(1f);
        }
        FirstAction();
    }

    IEnumerator WaitToExecute(Unit iaUnitToWait, Unit iaUnit, string action) //La siguiente Funcion se ejecuta cuando haya acabado la anterior la anterior unidad su accion
    {
        if (iaUnitToWait != null)
        {
            yield return new WaitUntil(() => iaUnitToWait.hasMoved || iaUnitToWait.hasAttacked);
            yield return new WaitForSeconds(0.5f);
        }
        if (action.Equals("Move"))
            f_ia_Move(iaUnit);
        else
            f_ia_Attack(iaUnit);
    }

    private void f_Buy_Stage()
    {
        if (_ia_villages.Count < 3 && player2Gold >= 100) f_Buy_Village();
        if (_ia_units.Count < 3 && player2Gold >= 40) f_Buy_Unit(false);
        if (player2Gold >= 500) while (player2Gold > 200) { f_Buy_Unit(true); }
        else if (player2Gold >= 400) { f_Buy_Unit(true); }
        else if (player2Gold >= 270 && _ia_units.Count < 6) f_Buy_Unit(true);
        else if (player2Gold >= 250 && _ia_villages.Count < 5) f_Buy_Village();
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
        _ia_units_array = _ia_units.ToArray();
    }

    private void f_Buy_Village()
    {
        _ch_Craeation.BuyVillage(_blue_village);
        Tile spawn_tile = null;
        foreach(Tile t in _ch_Craeation._tiles_to_place_things)
        {
            if (spawn_tile == null || Vector2.Distance(_player_house.transform.position, t.transform.position) < Vector2.Distance(_player_house.transform.position, spawn_tile.transform.position)) //Distancia mas proxima a la base enemiga
                spawn_tile = t;
        }
        if(spawn_tile != null) spawn_tile.f_Buy_Move();
    }

    private void f_Buy_Unit(bool random)
    {
        if (random)
        {
            int r = UnityEngine.Random.Range(0, 3);
            switch (r)
            {
                case 0:
                    _ch_Craeation.BuyUnit(_blue_knight);
                    break;
                case 1:
                    _ch_Craeation.BuyUnit(_blue_archer);
                    break;
                case 2:
                    _ch_Craeation.BuyUnit(_blue_bat);
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (player2Gold < 100)
            {
                if (player2Gold >= 80) _ch_Craeation.BuyUnit(_blue_bat);
                else if (player2Gold >= 70) _ch_Craeation.BuyUnit(_blue_archer);
                else _ch_Craeation.BuyUnit(_blue_knight);
            }
            else
            {
                if (player2Gold >= 400) _ch_Craeation.BuyUnit(_blue_bat);
                else if (player2Gold >= 300) _ch_Craeation.BuyUnit(_blue_archer);
                else if (player2Gold >= 200) _ch_Craeation.BuyUnit(_blue_knight);
            }
        }
       
        Tile spawn_tile = null;
        if (_state.Equals("Normal")) //Spawn unit in a random tile
        {
            Tile[] tiles = _ch_Craeation._tiles_to_place_things.ToArray();
            spawn_tile = tiles[UnityEngine.Random.Range(0, tiles.Length)];
        }
        else
        {
            foreach (Tile t in _ch_Craeation._tiles_to_place_things)
            {
                if (_state.Equals("Attack")) //Spawn units near the enemy base
                {
                    if (spawn_tile == null || Vector2.Distance(_player_house.transform.position, t.transform.position) < Vector2.Distance(_player_house.transform.position, spawn_tile.transform.position)) //DECIDIR CON MAPA DE INFLUENCIA-----------------------------!*!*!*!*!*!*!*!*!*!*!*!*!*!*!*!
                        spawn_tile = t;
                }
                else if (_state.Equals("Defend")) //Spawn units near your base
                {
                    if (spawn_tile == null || Vector2.Distance(_player_house.transform.position, t.transform.position) > Vector2.Distance(_player_house.transform.position, spawn_tile.transform.position)) //DECIDIR CON MAPA DE INFLUENCIA-----------------------------!*!*!*!*!*!*!*!*!*!*!*!*!*!*!*!
                        spawn_tile = t;
                }
            }
        }
        if (spawn_tile != null) spawn_tile.f_Buy_Move();
    }

    private void f_ia_Attack(Unit iaUnit)
    {
        if (iaUnit.hasAttacked) return;
        if (!iaUnit.isSelected) iaUnit.f_Select_Unit_attack();
        if (iaUnit.m_enemyBaseInRange)
        {
            iaUnit.AttackBase(_player_house);
            return;
        }
        if (iaUnit.enemiesInRange.Count <= 0) return;
        Unit _unit_aux = null;
        foreach(Unit player_unit in iaUnit.enemiesInRange)
        {
            if (_unit_aux is null || player_unit.health < _unit_aux.health) _unit_aux = player_unit; //focus al que menos vida tenga
        }
        if(_unit_aux != null) iaUnit.Attack(_unit_aux);
        iaUnit.hasAttacked = true;
    }

    private void f_ia_Move(Unit iaUnit)
    {
        if (iaUnit.hasMoved) return;
        if (!iaUnit.isSelected) iaUnit.f_Select_Unit_attack();
        //List<Tile> aux_Destinies = new List<Tile>(); //prueba borrable
        Tile t_destiny = null;
        Tile aux_base = null;
        bool near_Influence = false;
        foreach (Tile t in iaUnit._tilesReacheable) //Elijo hacia que unidad me muevo
        {
            if (aux_base is null || aux_base.m_EnemyNode.BaseValue < t.m_EnemyNode.BaseValue) 
                aux_base = t;
            if (t_destiny is null) 
                t_destiny = t;
            else //influencia de enemigo mas alta
            {
                bool enemyInMyBack = false;
                float aux_t = 0;
                float aux_td = 0;
                foreach (float iAux_value in t.m_EnemyNode.Influence) if (aux_t < iAux_value) aux_t = iAux_value;
                foreach (float iAux_value in t_destiny.m_EnemyNode.Influence) if (aux_td < iAux_value) aux_td = iAux_value;
                foreach (Unit enemy in FindObjectsOfType<Unit>())
                {
                    if (enemy.playerNumber != playerTurn && Vector2.Distance(_ia_house.transform.position, enemy.transform.position) < Vector2.Distance(_ia_house.transform.position, iaUnit.transform.position))
                        enemyInMyBack = true;
                }
                if (_state.Equals("Defend") && enemyInMyBack) //me muevo hacia la unidad con mas influencia y cercana a mi base
                {
                    if (aux_td < aux_t && 
                        Vector2.Distance(_ia_house.transform.position, t.transform.position) < Vector2.Distance(_ia_house.transform.position, t_destiny.transform.position))
                    {
                        t_destiny = t;
                        //aux_Destinies.Add(t_destiny); //prueba borrable
                    }
                }
                else //Me muevo hacia la unidad con mas influencia
                {
                    if (aux_td < aux_t) 
                        t_destiny = t;
                }
            }
        }
        /*if (_state.Equals("Defend")) //prueba borrable
            foreach (Tile t_aux in aux_Destinies)
                if (Vector2.Distance(_player_house.transform.position, t_aux.transform.position) > Vector2.Distance(_player_house.transform.position, t_destiny.transform.position))
                    t_destiny = t_aux;*/

        //Compruebo si ha podido encontrar un destino mediante los mapas de influencia
        if (aux_base != null && (aux_base.m_EnemyNode.BaseValue > 0.4 || (aux_base.m_EnemyNode.BaseValue > 0 && Vector2.Distance(_player_house.transform.position, iaUnit.transform.position) <= iaUnit.tileSpeed + iaUnit.attackRadius)))
            t_destiny = aux_base;
        else if (t_destiny != null)
            foreach (float iAux_value in t_destiny.m_EnemyNode.Influence)
            {
                if (iAux_value > 0)
                    near_Influence = true;
            }
        if (!near_Influence) //Si no llega el mapa de influencia me muevo hacia la base enemiga, si estoy defendiendo, hacia la mia
        {
            foreach (Tile t in iaUnit._tilesReacheable)
            {
                if (_state.Equals("Defend"))
                {
                    if (t_destiny is null || Vector2.Distance(_player_house.transform.position, t.transform.position) > Vector2.Distance(_player_house.transform.position, t_destiny.transform.position)) 
                        t_destiny = t;
                }
                else
                {
                    if (t_destiny is null || Vector2.Distance(_player_house.transform.position, t.transform.position) < Vector2.Distance(_player_house.transform.position, t_destiny.transform.position)) 
                        t_destiny = t;
                }
            }
        }
        if (t_destiny is null /*|| Vector2.Distance(t_destiny.transform.position, iaUnit.transform.position) < 0.8*/) //Quiero evitar que se mueva a la casilla sobre la que ya esta pero en principio esa ni esta en la lista
        {
            //print("No ha encontrado destino");
            ResetTiles();
            iaUnit.FinishgMovement();
            iaUnit.hasAttacked = true;
            iaUnit.hasMoved = true;
        }
        else
        {
            //t_destiny.rend.color = Color.red;
            t_destiny.f_Buy_Move(); //Creo que a veces nunca se mueve, pero llama a moverse y como no completa el movimiento no pone a true iaUnit.hasmoved, Casi 100% seguro que entra aqui en una especie de bucle infinito
        }
        StartCoroutine(WaitForMoveToEnd(iaUnit));
    }
    IEnumerator WaitForMoveToEnd(Unit iaUnit)
    {
        timer = true;
        yield return new WaitUntil(() => iaUnit.hasMoved || timeToPassToNextUnit > 5f); //iaUnit.tileSpeed/iaUnit.moveSpeed + 0.2f
        ResetTiles();
        yield return new WaitForSeconds(0.5f);
        iaUnit.FinishgMovement();//para segurarse
        //GridGenerator.instance.f_GenerateSelfInfluenceMap();
        f_ia_Attack(iaUnit);
        timer = false;
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
        foreach (Unit unit in unidades)
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
