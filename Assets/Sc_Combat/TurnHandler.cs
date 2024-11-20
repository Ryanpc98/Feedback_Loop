using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Transform PC_NinjaBot;
    [SerializeField] private Transform PC_GladiatorBot;
    [SerializeField] private Transform PC_ShamanBot;
    [SerializeField] private Transform PC_CowboyBot;
    [SerializeField] private Transform PC_VikingBot;
    [SerializeField] private Transform PC_ClericBot;

    [SerializeField] private Transform Boss_TankBot;
    [SerializeField] private Transform Boss_DPSBot;
    [SerializeField] private Transform Boss_BasicBot;

    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI turnCounterText;
    [SerializeField] private BattleButtonHandler mainUI;

    [SerializeField] private Sprite z1bg;
    [SerializeField] private Sprite z2bg;
    [SerializeField] private Sprite z3bg;

    [SerializeField] private Image bg;

    public AudioClip boom;
    public AudioClip clang;
    public AudioClip hitSound;
    public AudioClip playerDmg;
    public AudioClip punch;
    public AudioClip punch_2;
    public AudioClip punch_3;
    public AudioClip schwoop;
    public AudioClip skeletonDmg;
    public AudioClip tape;
    public AudioClip zombieDmg;

    public static List<string> adaptations = new List<string>();

    //Spawn Points
    private static Vector3 playerPos1 = new Vector3(-5.2f, 3f, 1);
    private static Vector3 playerPos2 = new Vector3(-3.4f, 1f, 1);
    private static Vector3 playerPos3 = new Vector3(-5.2f, -1f, 1);

    private static Vector3 AiPos1 = new Vector3(+5.2f, 3f, 1);
    private static Vector3 AiPos2 = new Vector3(+3.4f, 1f, 1);
    private static Vector3 AiPos3 = new Vector3(+5.2f, -1f, 1);

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
    private float s_volume;

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
        Ninja,
        Gladiator,
        Shaman,
        Cowboy,
        Viking,
        Cleric,
        B_Tank,
        B_DPS,
        B_Basic
    }

    /*======Utilities======*/

        //Returns turn handler instance
    public static TurnHandler GetInstance()
    {
        return instance;
    }

    public SaveData PopulateSaveData()
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
        sd.d_zone_two_ld = data.d_zone_two_ld;
        sd.d_zone_three_ld = data.d_zone_three_ld;

        //Level Name
        sd.d_level_name = data.d_level_name;

        return sd;
    }

    public void LoadFromSaveData(SaveData sd)
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
        data.d_zone_two_ld = sd.d_zone_two_ld;
        data.d_zone_three_ld = sd.d_zone_three_ld;

        //Level Name
        data.d_level_name = sd.d_level_name;
    }

    //Initialize variables here
    private void Awake()
    {
        instance = this;
        state = State.BusyAll;
        LoadFromSaveData(SaveManager.LoadJsonData());
        s_volume = PlayerPrefs.GetFloat(SaveManager.m_VolumeKey, 0.75f);

        switch (data.d_zone_index)
        {
            case (1):
                bg.sprite = z1bg;
                s_aiRoster = ZoneOneNodeMapController.GetEnemyBotArray(data.d_level_name);
                break;
            case (2):
                bg.sprite = z2bg;
                s_aiRoster = ZoneTwoNodeMapController.GetEnemyBotArray(data.d_level_name);
                break;
            case (3):
                bg.sprite = z3bg;
                s_aiRoster = ZoneThreeNodeMapController.GetEnemyBotArray(data.d_level_name);
                break;
            default:
                bg.color = new Color(1, 1, 1, 1);
                s_aiRoster = new List<CharType>() {CharType.Basic, CharType.Basic, CharType.Basic};
                break;
        }

        turnCounter = 0;
        levelNameText.text = data.d_level_name;
        turnCounterText.text = "Turn: 0";

        StartBattle();
    }

    // This feel like one of those functions that gets made fun of in a youtube video
    public void PlaySFX(EnumTypes.SFX sfx)
    {
        AudioClip tmp;

        switch (sfx)
        {
            case EnumTypes.SFX.boom:
                tmp = boom;
                break;
            case EnumTypes.SFX.clang:
                tmp = clang;
                break;
            case EnumTypes.SFX.hitSound:
                tmp = hitSound;
                break;
            case EnumTypes.SFX.playerDmg:
                tmp = playerDmg;
                break;
            case EnumTypes.SFX.punch:
                tmp = punch;
                break;
            case EnumTypes.SFX.punch_2:
                tmp = punch_2;
                break;
            case EnumTypes.SFX.punch_3:
                tmp = punch_3;
                break;
            case EnumTypes.SFX.schwoop:
                tmp = schwoop;
                break;
            case EnumTypes.SFX.skeletonDmg:
                tmp = skeletonDmg;
                break;
            case EnumTypes.SFX.tape:
                tmp = tape;
                break;
            case EnumTypes.SFX.zombieDmg:
                tmp = zombieDmg;
                break;
            default:
                tmp = clang;
                break;
        }

        SoundEffectsManager.instance.PlaySoundFXClip(tmp, transform, s_volume);
    }

    // Let us know that someone died and we need to update data
    public void setDeadCheckFlag()
    {
        deadCheckFlag = true;
    }

    // Make sure action and target side information are correct
    public bool HandleAttack(int attack)
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
        UpdateTargetButtonEnablement();
        return targetIsPlayer;
    }

    // Execute the selected action
    public void ExecuteAction()
    {
        Debug.Log("Executing Action");
        BaseBotController targetController;
        if (targetIsPlayer)
        {
            Debug.Log("Target is Player");
            targetController = playerBotArray[target].controller;
        }
        else
        {
            Debug.Log("Target is NOT Player");
            targetController = aiBotArray[target].controller;
        }
        Debug.Log("Actor " + actor.ToString() + " attemping to execute action: " + action.ToString() + " on target " + target.ToString());
        if (playerBotArray[actor].controller.IsValidTarget(action, targetController))
        {
            Debug.Log("executed");
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
            Debug.Log("executed");
            mainUI.ActionButtonUpdate();
        }
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

    public bool DealAoeHealing(bool isPlayer, float hp)
    {
        if (isPlayer)
        {
            for (int i = 0; i < 3; i++)
            {
                aiBotArray[i].controller.ApplyHealing(hp);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                playerBotArray[i].controller.ApplyHealing(hp);
            }
        }
        return false;
    }

    public void HandleTaunt(bool isPlayer, bool flag)
    {
        {
            if (isPlayer)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!aiBotArray[i].controller.IsTaunting())
                    {
                        aiBotArray[i].controller.SetTargetable(!flag);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!playerBotArray[i].controller.IsTaunting())
                    {
                        playerBotArray[i].controller.SetTargetable(!flag);
                    }
                }
            }
        }
    }

    /*======Button Handlers======*/
    //Set actor from button press
    public void HandleActorBotSelection(int i)
    {
        actor = i;

        switch (action)
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
        UpdateTargetButtonEnablement();
    }

    //Set target from button NOTE: could be enemy or friendly
    public void HandleTargetBotSelection(int i)
    {
        if (targetIsPlayer)
        {
            if (playerBotArray[actor].controller.IsValidTarget(action, playerBotArray[i].controller))
            {
                target = i;
            }
        }
        else
        {
            if (playerBotArray[actor].controller.IsValidTarget(action, aiBotArray[i].controller))
            {
                target = i;
            }
        }

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
                if (playerBotArray[i].controller.IsAlive())
                {
                    playerBotArray[i].controller.AddTurnEnergy();
                }
            }
            mainUI.DisableUI();
            state = State.WaitingAI;
        }
    }

    public void HandleEndAiTurn()
    {
        Debug.Log("===END AI TURN===");
        for (int i = 0; i < 3; i++)
        {
            if (aiBotArray[i].controller.IsAlive())
            {
                aiBotArray[i].controller.AddTurnEnergy();
            }
        }
        turnPointer = 0;
        UpdateTargetButtonEnablement();
        mainUI.RefreshUI();
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

            //Save Game State
            SaveManager.SaveJsonData(PopulateSaveData());

            switch (data.d_zone_index)
            {
                case 1:
                    data.d_zone_one_ld.level_status_arr[data.d_zone_one_ld.level_index] = 3;
                    SceneManager.LoadScene("ZoneOne");
                    break;
                case 2:
                    data.d_zone_two_ld.level_status_arr[data.d_zone_two_ld.level_index] = 3;
                    SceneManager.LoadScene("ZoneTwo");
                    break;
                case 3:
                    data.d_zone_three_ld.level_status_arr[data.d_zone_three_ld.level_index] = 3;
                    SceneManager.LoadScene("ZoneThree");
                    break;
                default:
                    break;
            }
            //loseScreen.EnableMenu(data.d_zone_index);
        }
        else
        {
            findAdaptations();

            switch (data.d_zone_index)
            {
                case 1:
                    data.d_zone_one_ld.level_status_arr[data.d_zone_one_ld.level_index] = 4;
                    break;
                case 2:
                    data.d_zone_two_ld.level_status_arr[data.d_zone_two_ld.level_index] = 4;
                    break;
                case 3:
                    data.d_zone_three_ld.level_status_arr[data.d_zone_three_ld.level_index] = 4;
                    break;
                default:
                    break;
            }

            Debug.Log("Ending Round for AI Loss");

            //Save Game State
            SaveManager.SaveJsonData(PopulateSaveData());

            //winScreen.EnableMenu();
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
                cost = playerBotArray[actor].controller.GetActionCost(1);
                break;
            case 2:
                cost = playerBotArray[actor].controller.GetActionCost(2);
                break;
            case 3:
                cost = playerBotArray[actor].controller.GetActionCost(3);
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

    public void UpdateTargetButtonEnablement()
    {
        bool[] en = { true, true, true };

        if (targetIsPlayer)
        {
            for (int i = 0; i < playerBotMax; i++)
            {
                en[i] = playerBotArray[actor].controller.IsValidTarget(action, playerBotArray[i].controller);
            }
        }
        else
        {
            for (int i = 0; i < aiBotMax; i++)
            {
                en[i] = playerBotArray[actor].controller.IsValidTarget(action, aiBotArray[i].controller);
            }
        }


        mainUI.TargetButtonUpdate(en);
    }

    /*======Spawning Logic======*/
    private void StartBattle()
    {

        for (int i = 0; i < 3; i++)
        {
            playerBotArray[i].type = data.d_roster[i];
            playerBotArray[i].controller = SpawnCharacter(true, playerBotArray[i].type, playerPosArray[i], i);
            playerBotArray[i].isPlayerTeam = true;
            playerBotArray[i].isAlive = true;
            Debug.Log("Spawning PC " + i);
        }

        for (int i = 0; i < 3; i++)
        {
            aiBotArray[i].type = s_aiRoster[i];
            aiBotArray[i].controller = SpawnCharacter(false, aiBotArray[i].type, AiPosArray[i], i + 3);
            aiBotArray[i].isPlayerTeam = false;
            aiBotArray[i].isAlive = true;
            Debug.Log("Spawning AI " + i);
        }
        HandleAdaptations(true);
        HandleAdaptations(false);
        mainUI.InitializeButtons();
        state = State.WaitingPlayer;
    }

    private BaseBotController SpawnCharacter(bool isPlayerTeam, CharType charToSpawn, Vector3 position, int index)
    {
        Transform characterTransform;
        // Yeah, this is a big switch statement. What about it?
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
            case (CharType.Ninja):
                characterTransform = Instantiate(PC_NinjaBot, position, Quaternion.identity);
                break;
            case (CharType.Gladiator):
                characterTransform = Instantiate(PC_GladiatorBot, position, Quaternion.identity);
                break;
            case (CharType.Shaman):
                characterTransform = Instantiate(PC_ShamanBot, position, Quaternion.identity);
                break;
            case (CharType.Cowboy):
                characterTransform = Instantiate(PC_CowboyBot, position, Quaternion.identity);
                break;
            case (CharType.Viking):
                characterTransform = Instantiate(PC_VikingBot, position, Quaternion.identity);
                break;
            case (CharType.Cleric):
                characterTransform = Instantiate(PC_ClericBot, position, Quaternion.identity);
                break;
            case (CharType.B_Tank):
                characterTransform = Instantiate(Boss_TankBot, position, Quaternion.identity);
                break;
            case (CharType.B_DPS):
                characterTransform = Instantiate(Boss_DPSBot, position, Quaternion.identity);
                break;
            case (CharType.B_Basic):
                characterTransform = Instantiate(Boss_BasicBot, position, Quaternion.identity);
                break;
            default:
                characterTransform = Instantiate(BaseDevBot, position, Quaternion.identity);
                Debug.Log("WTF Am I Supposed to Spawn with This?");
                break;
        }
        BaseBotController spawned = characterTransform.GetComponent<BaseBotController>();
        spawned.Setup(isPlayerTeam, instance, index);

        return spawned;
    }

    //Loop through all bots and apply adaptations
    private void HandleAdaptations(bool player)
    {
        if (player)
        {
            for (int j = 0; j < playerBotMax; j++)
            {
                Debug.Log("Adapting Player Bot: " + j);

                AdaptIncreasedRam(data.d_adaptations.increasedRAM, playerBotArray[j].controller);
                AdaptBiggerHardDrives(data.d_adaptations.biggerHardDrives, playerBotArray[j].controller);
                AdaptHotSpares(data.d_adaptations.hotSpares, playerBotArray[j].controller);
                AdaptUpgradedFirewall(data.d_adaptations.upgradedFirewall, playerBotArray[j].controller);
                AdaptEfficientRouting(data.d_adaptations.efficientRouting, playerBotArray[j].controller);
            }
        }
        else
        {
            int ram = 0;
            int hdd = 0;
            int hsp = 0;
            int fir = 0;
            int rou = 0;

            switch (data.d_zone_index)
            {
                case 1:
                    ram = 0;
                    hdd = 0;
                    hsp = 0;
                    fir = 0;
                    rou = 0;
                    break;
                case 2:
                    ram = 2;
                    hdd = 2;
                    hsp = 0;
                    fir = 1;
                    rou = 0;
                    break;
                case 3:
                    ram = 3;
                    hdd = 3;
                    hsp = 0;
                    fir = 2;
                    rou = 1;
                    break;
            }
            for (int i = 0; i < playerBotMax; i++)
            {
                Debug.Log("Adapting AI Bot: " + i);

                AdaptIncreasedRam(ram, aiBotArray[i].controller);
                AdaptBiggerHardDrives(hdd, aiBotArray[i].controller);
                AdaptHotSpares(hsp, aiBotArray[i].controller);
                AdaptUpgradedFirewall(fir, aiBotArray[i].controller);
                AdaptEfficientRouting(rou, aiBotArray[i].controller);
            }
        }
    }

    private void AdaptIncreasedRam(int loops, BaseBotController bot)
    {
        Debug.Log("AdaptIncreasedRam Loops: " + loops);
        for(int i = 0; i < loops; i++)
        {
            bot.ChangeEnergyGainRate(10);
        }
    }

    private void AdaptBiggerHardDrives(int loops, BaseBotController bot)
    {
        Debug.Log("AdaptBiggerHardDrives Loops: " + loops);
        for (int i = 0; i < loops; i++)
        {
            bot.ChangeMaxEnergy(50);
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
                    bot.ChangeDamageMult(j, 0.20f);
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
            bot.ChangeAllActionCosts(-10);
        }
    }

    /*======AI Controller======*/
    private void Update()
    {
        if (turnPointer >= (aiBotMax * 2))
        {
            HandleEndAiTurn();
        }
        else if (state == State.WaitingAI)
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
                    if (aiBotArray[i].controller.IsTaunting())
                    {
                        aiBotArray[i].controller.ClearTaunting();
                    }
                    deadCounter += 1;
                }
            }
            Debug.Log("New Dead, Dead Counter (AI) is at: " + deadCounter);
            if (deadCounter == 3)
            {
                HandleEndRound(false);
            }

            // Update Player dead tags and check if all Players are dead
            deadCounter = 0;
            for (int j = 0; j < playerBotMax; j++)
            {
                playerBotArray[j].isAlive = playerBotArray[j].controller.IsAlive();
                if (!playerBotArray[j].isAlive)
                {
                    if (playerBotArray[j].controller.IsTaunting())
                    {
                        playerBotArray[j].controller.ClearTaunting();
                    }
                    deadCounter += 1;
                }                
            }
            Debug.Log("New Dead, Dead Counter (PC) is at: " + deadCounter);
            if (deadCounter == 3)
            {
                HandleEndRound(true);
            }

            UpdateActorButtonEnablement();
            UpdateTargetButtonEnablement();

            deadCheckFlag = false;
        }
    }

    public BaseBotController GetTargetFromIndex(bool isPlayer, int i)
    {
        Debug.Log("Getting Target Info on: " + i.ToString());
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
        Debug.Log("Turn For AI: " + (turnPointer % aiBotMax).ToString() + " (" + turnPointer.ToString() + ")");
        if (aiBotArray[turnPointer % aiBotMax].controller.IsAlive())
        {
            if (!aiBotArray[turnPointer % aiBotMax].controller.ActionChooser(aiBotArray, playerBotArray, turnPointer % aiBotMax))
            {
                ReleaseAiLock();
                turnPointer += 1;
            }
        }
        else
        {
            ReleaseAiLock();
            turnPointer += 1;
        }
        state = State.WaitingAI;
    }
}
