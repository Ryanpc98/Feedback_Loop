using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TurnHandler : MonoBehaviour
{
    /*======Variables======*/
    //Reference to this handler for Bots to use
    private static TurnHandler instance;

    //Character Prefabs
    [SerializeField] private Transform BaseDevBot;
    [SerializeField] private Transform TestDevBot;
    [SerializeField] private Transform PC_BasicBot;
    [SerializeField] private Transform PC_DPSBot;
    [SerializeField] private Transform PC_TankBot;
    [SerializeField] private Transform PC_HealerBot;
    [SerializeField] private Transform Boss_TankBot;

    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI turnCounterText;
    [SerializeField] private BattleButtonHandler mainUI;

    public static List<string> adaptations = new List<string>();

    //Spawn Points
    private static Vector3 playerPos1 = new Vector3(-5, 3.5f);
    private static Vector3 playerPos2 = new Vector3(-5, 1.5f);
    private static Vector3 playerPos3 = new Vector3(-5, -0.5f);

    private static Vector3 AiPos1 = new Vector3(+5, 3.5f);
    private static Vector3 AiPos2 = new Vector3(+5, 1.5f);
    private static Vector3 AiPos3 = new Vector3(+5, -0.5f);

    private static Vector3[] playerPosArray = { playerPos1, playerPos2, playerPos3 };
    private static Vector3[] AiPosArray = { AiPos1, AiPos2, AiPos3 };

    //Max Bots Per Team
    private const int playerBotMax = 3;
    private const int aiBotMax = 3;

    //List of Bots Per Team
    public BotInfo[] playerBotArray = new BotInfo[playerBotMax];
    public BotInfo[] aiBotArray = new BotInfo[aiBotMax];

    //Tracks game state
    private State state;
    private int turnCounter = 0;
    private int turnPointer = 0;
    private bool AiLock = false;
    private bool deadCheckFlag = false;

    //Save Data Variables
    private SaveData data = new SaveData();
    private List<CharType> s_aiRoster = new List<CharType>();

    //Current Player Bot Selection
    private int actor;
    private int target;
    private bool targetIsPlayer;
    private int action;

    //Holds relevant info for each bot
    public struct BotInfo
    {
        public BaseBotController controller;
        public CharType type;
        public bool isPlayerTeam;
        public bool isAlive;
    }

    //Possible Game States
    public enum State
    {
        BusyAll,
        WaitingPlayer,
        BusyPlayer,
        WaitingAI,
        BusyAI
    }

    //Enumerated Bot Types
    public enum CharType
    {
        Base,
        Test,
        Basic,
        DPS,
        Healer,
        Tank,
        B_Tank
    }

    /*======Utilities======*/

        //Returns turn handler instance
    public static TurnHandler GetInstance()
    {
        return instance;
    }

    private SaveData PopulateSaveData()
    {
        SaveData sd = new SaveData();

        //Adaptations
        sd.d_adaptations.increasedRAM = data.d_adaptations.increasedRAM;
        sd.d_adaptations.biggerHardDrives = data.d_adaptations.biggerHardDrives;
        sd.d_adaptations.hotSpares = data.d_adaptations.hotSpares;
        sd.d_adaptations.upgradedFirewall = data.d_adaptations.upgradedFirewall;
        sd.d_adaptations.efficientRouting = data.d_adaptations.efficientRouting;
        
        //Player Bots
        sd.d_roster = data.d_roster;

        //Zone Info
        sd.d_zone_index = data.d_zone_index;

        //Level Info
        sd.d_zone_one_ld = data.d_zone_one_ld;

        //Level Name
        sd.d_level_name = data.d_level_name;

        return sd;
    }

    private void LoadFromSaveData(SaveData sd)
    {
        //Adaptations
        data.d_adaptations.increasedRAM = sd.d_adaptations.increasedRAM;
        data.d_adaptations.biggerHardDrives = sd.d_adaptations.biggerHardDrives;
        data.d_adaptations.hotSpares = sd.d_adaptations.hotSpares;
        data.d_adaptations.upgradedFirewall = sd.d_adaptations.upgradedFirewall;
        data.d_adaptations.efficientRouting = sd.d_adaptations.efficientRouting;

        Debug.Log("======ADAPT RAM: " + data.d_adaptations.increasedRAM);
        Debug.Log("======ADAPT HDD: " + data.d_adaptations.biggerHardDrives);
        Debug.Log("======ADAPT HSP: " + data.d_adaptations.hotSpares);
        Debug.Log("======ADAPT FIR: " + data.d_adaptations.upgradedFirewall);
        Debug.Log("======ADAPT ROU: " + data.d_adaptations.efficientRouting);


        //Player Bots
        data.d_roster = sd.d_roster;

        // Zone Info
        data.d_zone_index = sd.d_zone_index;

        //Level Info
        data.d_zone_one_ld = sd.d_zone_one_ld;

        //Level Name
        data.d_level_name = sd.d_level_name;
    }

    //Initialize variables here
    private void Awake()
    {
        instance = this;
        state = State.BusyAll;
        LoadFromSaveData(SaveManager.LoadJsonData());

        switch (data.d_zone_index)
        {
            case (1):
                s_aiRoster = ZoneOneNodeMapController.GetEnemyBotArrayZoneOne(data.d_level_name);
                break;
            default:
                s_aiRoster = NodeMapController.GetEnemyBotArray(data.d_level_name);
                break;
        }

        turnCounter = 0;
        levelNameText.text = data.d_level_name;
        turnCounterText.text = "Turn: 0";

        StartBattle();
    }

    // Let us know that someone died and we need to update data
    public void setDeadCheckFlag()
    {
        deadCheckFlag = true;
    }

    // Make sure action and target side information are correct
    public void HandleAttack(int attack)
    {
        switch (attack)
        {
            case 1:
                action = 1;
                targetIsPlayer = playerBotArray[actor].controller.ActionOneSide();
                break;
            case 2:
                action = 2;
                targetIsPlayer = playerBotArray[actor].controller.ActionTwoSide();
                break;
            case 3:
                action = 3;
                targetIsPlayer = playerBotArray[actor].controller.ActionThreeSide();
                break;
            default:
                Debug.Log("You Should Not Be Here Bro");
                break;
        }
    }

    // Execute the selected action
    public void ExecuteAction()
    {
        BaseBotController targetController;
        if (targetIsPlayer)
        {
            targetController = playerBotArray[target].controller;
        }
        else
        {
            targetController = aiBotArray[target].controller;
        }
        switch (action)
        {
            case 1:
                playerBotArray[actor].controller.ActionOneCallback(targetController);
                break;
            case 2:
                playerBotArray[actor].controller.ActionTwoCallback(targetController);
                break;
            case 3:
                playerBotArray[actor].controller.ActionThreeCallback(targetController);
                break;
            default:
                Debug.Log("You Should Not Be Here Bro");
                break;
        }
        mainUI.ActionButtonUpdate();
    }

    //AoE Damage Helper
    public bool DealAoeDamage(bool isPlayer, float damage)
    {
        if (isPlayer)
        {
            for (int i = 0; i < 3; i++)
            {
                aiBotArray[i].controller.ApplyDamage(damage);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                playerBotArray[i].controller.ApplyDamage(damage);
            }
        }
        return false;
    }

    /*======Button Handlers======*/  
    //Set actor from button press
    public void HandleActorBotSelection(int i)
    {
        actor = i;
    }
    
    //Set target from button NOTE: could be enemy or friendly
    public void HandleEnemyBotSelection(int i)
    {
        target = i;
    }

    /*======Turn Transition Handlers======*/
    //Various end turn processing
    public void HandleEndPlayerTurn()
    {
        Debug.Log("===END PC TURN===");
        if (state == State.WaitingPlayer)
        {
            for (int i = 0; i < 3; i++)
            {
                playerBotArray[i].controller.AddTurnEnergy();
            }
            state = State.WaitingAI;
        }
    }

    public void HandleEndAiTurn()
    {
        Debug.Log("===END AI TURN===");
        for (int i = 0; i < 3; i++)
        {
            aiBotArray[i].controller.AddTurnEnergy();
        }
        turnPointer = 0;
        state = State.WaitingPlayer;
        turnCounter += 1;
        turnCounterText.text = "Turn: " + turnCounter.ToString();
    }

    /*======Round Transition Handlers======*/
    // Main Handler
    public void HandleEndRound(bool playerLoss)
    {
        if (playerLoss) {
            Debug.Log("Ending Round for Player Loss");
            data.d_zone_one_ld.level_status_arr[data.d_zone_one_ld.level_index] = 3;

            //Save Game State
            SaveManager.SaveJsonData(PopulateSaveData());

            SceneManager.LoadScene("ZoneOne");
        }
        else
        {
            findAdaptations();
            data.d_zone_one_ld.level_status_arr[data.d_zone_one_ld.level_index] = 4;
            Debug.Log("Ending Round for AI Loss");

            //Save Game State
            SaveManager.SaveJsonData(PopulateSaveData());

            SceneManager.LoadScene("AdaptationSelector");
        }
    }

    // Determine Adaptations
    public void findAdaptations()
    {
        adaptations.Clear();

        if (IncreaseRam())
        {
            Debug.Log("RAM Avail");
            adaptations.Add("Increase RAM");
        }
        if (BiggerHardDrives())
        {
            Debug.Log("HDD Avail");
            adaptations.Add("Bigger Hard Drives");
        }
        if (HotSpares())
        {
            Debug.Log("HSP Avail");
            adaptations.Add("Hot Spares");
        }
        if (UpgradedFirewall())
        {
            Debug.Log("FIR Avail");
            adaptations.Add("Upgraded Firewall");
        }
        if (EfficientRouting())
        {
            Debug.Log("ROU Avail");
            adaptations.Add("Efficient Routing");
        }

    }

    // Various Adaptation Checks

    // Positive
    //Increase RAM
    public bool IncreaseRam()
    {
        if (turnCounter <= 6)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Bigger Hard Drives
    public bool BiggerHardDrives()
    {
        float cost;
        switch (action)
        {
            case 1:
                cost = playerBotArray[actor].controller.getActionCost(1);
                break;
            case 2:
                cost = playerBotArray[actor].controller.getActionCost(2);
                break;
            case 3:
                cost = playerBotArray[actor].controller.getActionCost(3);
                break;
            default:
                cost = 0;
                Debug.Log("No Action Found for Bigger Hard Drives");
                break;
        }
        if (cost >= 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    //Hot Spares
    public bool HotSpares()
    {
        Debug.Log("Entering HotSpares()");
        for (int i = 0; i < playerBotMax; i++)
        {
            Debug.Log("HotSpares Iter: " + i.ToString());
            Debug.Log("HotSpares Comparing: " + playerBotArray[i].controller.GetHealthAsPct().ToString());
            if (playerBotArray[i].controller.GetHealthAsPct() == 1)
            {
                return true;
            }
        }
        return false;
    }

    // Negative
    // Upgraded Firewall
    public bool UpgradedFirewall()
    {
        for (int i = 0; i < playerBotMax; i++)
        {
            if ((playerBotArray[i].controller.IsAlive()) && (playerBotArray[i].controller.GetHealthAsPct() <= .2f))
            {
                return true;
            }
        }
        return false;
    }

    // Efficient Routing
    public bool EfficientRouting()
    {
        if (turnCounter >= 5)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Allows buttons to poll for updates
    //Probably inefficient, might need to look at other solutions
    //Should buttons announce themselves to the handler? Call some function in here that registers them
    public string[] UpdateActionButtons()
    {
        string[] names = { "!!!ERROR!!!", "!!!ERROR!!!", "!!!ERROR!!!" };
        names[0] = playerBotArray[actor].controller.ActionOneName();
        names[1] = playerBotArray[actor].controller.ActionTwoName();
        names[2] = playerBotArray[actor].controller.ActionThreeName();

        return names;
    }

    public string[] UpdateActorButtons()
    {
        string[] names = { "!!!ERROR!!!", "!!!ERROR!!!", "!!!ERROR!!!" };
        names[0] = playerBotArray[0].controller.GetName();
        names[1] = playerBotArray[1].controller.GetName();
        names[2] = playerBotArray[2].controller.GetName();

        return names;
    }

    public string[] UpdateEnemyButtons()
    {
        string[] names = { "!!!ERROR!!!", "!!!ERROR!!!", "!!!ERROR!!!" };
        names[0] = aiBotArray[0].controller.GetName();
        names[1] = aiBotArray[1].controller.GetName();
        names[2] = aiBotArray[2].controller.GetName();

        return names;
    }

    public bool[] UpdateActionButtonEnablement()
    {
        bool[] en = { true, true, true };
        en[0] = playerBotArray[actor].controller.CanDoActionOne();
        en[1] = playerBotArray[actor].controller.CanDoActionTwo();
        en[2] = playerBotArray[actor].controller.CanDoActionThree();

        return en;
    }

    public void UpdateActorButtonEnablement()
    {
        bool[] en = { true, true, true };
        en[0] = playerBotArray[0].controller.IsAlive();
        en[1] = playerBotArray[1].controller.IsAlive();
        en[2] = playerBotArray[2].controller.IsAlive();

        mainUI.ActorButtonUpdate(en);
    }

    public void UpdateEnemyButtonEnablement()
    {
        bool[] en = { true, true, true };
        en[0] = aiBotArray[0].controller.IsAlive();
        en[1] = aiBotArray[1].controller.IsAlive();
        en[2] = aiBotArray[2].controller.IsAlive();

        mainUI.EnemyButtonUpdate(en);
    }

    /*======Spawning Logic======*/
    private void StartBattle()
    {

        for (int i = 0; i < 3; i++)
        {
            playerBotArray[i].type = data.d_roster[i];
            playerBotArray[i].controller = SpawnCharacter(true, playerBotArray[i].type, playerPosArray[i]);
            playerBotArray[i].isPlayerTeam = true;
            playerBotArray[i].isAlive = true;
            Debug.Log("Spawning PC " + i);
        }

        for (int i = 0; i < 3; i++)
        {
            aiBotArray[i].type = s_aiRoster[i];
            aiBotArray[i].controller = SpawnCharacter(false, aiBotArray[i].type, AiPosArray[i]);
            aiBotArray[i].isPlayerTeam = false;
            aiBotArray[i].isAlive = true;
            Debug.Log("Spawning AI " + i);
        }
        HandleAdaptations();
        mainUI.InitializeButtons();
        state = State.WaitingPlayer;
    }

    private BaseBotController SpawnCharacter(bool isPlayerTeam, CharType charToSpawn, Vector3 position)
    {
        Transform characterTransform;
        switch (charToSpawn)
        {
            case (CharType.Test):
                characterTransform = Instantiate(TestDevBot, position, Quaternion.identity);
                break;
            case (CharType.Base):
                characterTransform = Instantiate(BaseDevBot, position, Quaternion.identity);
                break;
            case (CharType.Basic):
                characterTransform = Instantiate(PC_BasicBot, position, Quaternion.identity);
                break;
            case (CharType.DPS):
                characterTransform = Instantiate(PC_DPSBot, position, Quaternion.identity);
                break;
            case (CharType.Healer):
                characterTransform = Instantiate(PC_HealerBot, position, Quaternion.identity);
                break;
            case (CharType.Tank):
                characterTransform = Instantiate(PC_TankBot, position, Quaternion.identity);
                break;
            case (CharType.B_Tank):
                characterTransform = Instantiate(Boss_TankBot, position, Quaternion.identity);
                break;
            default:
                characterTransform = Instantiate(BaseDevBot, position, Quaternion.identity);
                Debug.Log("WTF Am I Supposed to Spawn with This?");
                break;
        }
        BaseBotController spawned = characterTransform.GetComponent<BaseBotController>();
        spawned.Setup(isPlayerTeam, instance);

        return spawned;
    }

    //Loop through all bots and apply adaptations
    private void HandleAdaptations()
    {
        for (int j = 0; j < playerBotMax; j++)
        {
            Debug.Log("Adapting Bot: " + j);

            AdaptIncreasedRam(data.d_adaptations.increasedRAM, playerBotArray[j].controller);
            AdaptBiggerHardDrives(data.d_adaptations.biggerHardDrives, playerBotArray[j].controller);
            AdaptHotSpares(data.d_adaptations.hotSpares, playerBotArray[j].controller);
            AdaptUpgradedFirewall(data.d_adaptations.upgradedFirewall, playerBotArray[j].controller);
            AdaptEfficientRouting(data.d_adaptations.efficientRouting, playerBotArray[j].controller);
        }
    }

    private void AdaptIncreasedRam(int loops, BaseBotController bot)
    {
        Debug.Log("AdaptIncreasedRam Loops: " + loops);
        for(int i = 0; i < loops; i++)
        {
            bot.ChangeEnergyGainRate(2);
        }
    }

    private void AdaptBiggerHardDrives(int loops, BaseBotController bot)
    {
        Debug.Log("AdaptBiggerHardDrives Loops: " + loops);
        for (int i = 0; i < loops; i++)
        {
            bot.ChangeMaxEnergy(5);
        }
    }

    private void AdaptHotSpares(int loops, BaseBotController bot)
    {
        Debug.Log("AdaptHotSpares Loops: " + loops);
        for (int i = 0; i < loops; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                if (bot.GetActionType(j) == BaseBotController.ActionType.Heal)
                {
                    bot.ChangeDamage(j, 0.20f);
                }
            }

        }
    }

    private void AdaptUpgradedFirewall(int loops, BaseBotController bot)
    {
        Debug.Log("AdaptUpgradedFirewall Loops: " + loops);
        for (int i = 0; i < loops; i++)
        {
            bot.ChangeMaxHealth(0.25f);
        }
    }

    private void AdaptEfficientRouting(int loops, BaseBotController bot)
    {
        Debug.Log("AdaptEfficientRouting Loops: " + loops);
        for (int i = 0; i < loops; i++)
        {
            bot.ChangeAllActionCosts(-1);
        }
    }

    /*======AI Controller======*/
    private void Update()
    {
        if (turnPointer >= aiBotMax)
        {
            HandleEndAiTurn();
        }
        if (state == State.WaitingAI)
        {
            if (!GetAiLock())
            {
                state = State.BusyAI;
                SetAiLock();
                BasicAITurn();
            }
        }
        if (deadCheckFlag)
        {
            int deadCounter = 0;
            // Update AI dead tags and check if all AI are dead
            for (int i = 0; i < aiBotMax; i++)
            {
                aiBotArray[i].isAlive = aiBotArray[i].controller.IsAlive();
                if (!aiBotArray[i].isAlive)
                {
                    deadCounter += 1;
                }
            }
            if (deadCounter == 3)
            {
                HandleEndRound(false);
            }

            // Update Player dead tags and check if all Players are dead
            deadCounter = 0;
            for (int j = 0; j < playerBotMax; j++)
            {
                playerBotArray[j].isAlive = playerBotArray[j].controller.IsAlive();
            }
            if (deadCounter == 3)
            {
                HandleEndRound(true);
            }

            UpdateActorButtonEnablement();
            UpdateEnemyButtonEnablement();

            deadCheckFlag = false;
        }
    }

    private EnumTypes.GameStateInfo GetGameState(int self)
    {
        EnumTypes.GameStateInfo gameState;
        gameState.turnCounter = turnCounter;
        gameState.selfIndex = self;
        gameState.aiBots = aiBotMax;
        gameState.pcBots = playerBotMax;
        gameState.aiHPArray = new float[aiBotMax];
        gameState.pcHPArray = new float[playerBotMax];
        gameState.aiHPArrayPct = new float[aiBotMax];
        gameState.pcHPArrayPct = new float[playerBotMax];
        gameState.aiEnergyArrayPct = new float[aiBotMax];
        gameState.pcEnergyArrayPct = new float[playerBotMax];

        for (int i = 0; i < aiBotMax; i++)
        {
            gameState.aiHPArray[i] = aiBotArray[i].controller.GetHealth();
            gameState.aiHPArrayPct[i] = aiBotArray[i].controller.GetHealthAsPct();
            gameState.aiEnergyArrayPct[i] = aiBotArray[i].controller.GetEnergyAsPct();
        }

        for (int j = 0; j < playerBotMax; j++)
        {
            gameState.pcHPArray[j] = playerBotArray[j].controller.GetHealth();
            gameState.pcHPArrayPct[j] = playerBotArray[j].controller.GetHealthAsPct();
            gameState.pcEnergyArrayPct[j] = playerBotArray[j].controller.GetEnergyAsPct();
        }

        return gameState;
    }

    public BaseBotController GetTargetFromIndex(bool isPlayer, int i)
    {
        if (isPlayer)
        {
            return playerBotArray[i].controller;
        }
        else
        {
            return aiBotArray[i].controller;
        }
    }

    public void SetAiLock()
    {
        AiLock = true;
    }

    public void ReleaseAiLock()
    {
        AiLock = false;
    }

    public bool GetAiLock()
    {
        return AiLock;
    }

    private void BasicAITurn()
    {
        if (aiBotArray[turnPointer].controller.IsAlive())
        {
            if (!aiBotArray[turnPointer].controller.ActionChooser(GetGameState(turnPointer)))
            {
                turnPointer += 1;
            }
        }
        else
        {
            turnPointer += 1;
            ReleaseAiLock();
        }
        state = State.WaitingAI;
    }
}
