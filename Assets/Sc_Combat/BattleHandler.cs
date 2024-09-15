using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    [SerializeField] private Transform GenericCharacter;
    [SerializeField] private Transform TestPlayerCharacer;

    private static BattleHandler instance;

    //private CharacterBattle playerCharacterBattle;
    //private CharacterBattle enemyCharacterBattle;

    private State state;
    private charType characterType;

    private bool turnInProgress = false;
    private int turnPointer = 0;
    private int turnMax = 6;

    private static Vector3 playerPos1 = new Vector3(-5, 6);
    private static Vector3 playerPos2 = new Vector3(-5, 2);
    private static Vector3 playerPos3 = new Vector3(-5, 0);

    private static Vector3 AiPos1 = new Vector3(+5, 6);
    private static Vector3 AiPos2 = new Vector3(+5, 2);
    private static Vector3 AiPos3 = new Vector3(+5, 0);

    private static Vector3[] playerPosArray = { playerPos1, playerPos2, playerPos3 };
    private static Vector3[] AiPosArray = { AiPos1, AiPos2, AiPos3 };

    private static int playerBotMax = 3;
    private static int aiBotMax = 3;

    public BotInfo[] playerBotArray = new BotInfo[playerBotMax];
    public BotInfo[] enemyBotArray = new BotInfo[aiBotMax];

    public struct BotInfo
    {
        public charType type;
        public CharacterBattle characterBattle;
        public int turnOrder;
        public bool isPlayerTeam;
    }

    public enum State
    {
        WaitingPlayer,
        BusyPlayer,
        WaitingAI,
        BusyAI
    }

    public enum charType
    {
        Base,
        Dev_AI,
        Dev_PC
    }

    public static BattleHandler GetInstance()
    {
        return instance;
    }

    public void FinishedTurn()
    {
        bool aliveFound = false;
        while (!aliveFound)
        {
            turnPointer = (turnPointer + 1) % turnMax;
            if (turnPointer < playerBotMax)
            {
                if (playerBotArray[turnPointer].characterBattle.IsAlive())
                {
                    aliveFound = true;
                }
            }
            else
            {
                if (enemyBotArray[turnPointer % playerBotMax].characterBattle.IsAlive())
                {
                    aliveFound = true;
                }

            }
        }
        switch (turnPointer)
        {
            case 0:
                state = State.WaitingPlayer;
                break;
            case 1:
                state = State.WaitingPlayer;
                break;
            case 2:
                state = State.WaitingPlayer;
                break;
            case 3:
                state = State.WaitingAI;
                break;
            case 4:
                state = State.WaitingAI;
                break;
            case 5:
                state = State.WaitingAI;
                break;
            default:
                Debug.Log("Invalid Turn Pointer");
                break;
        }
        turnInProgress = false;
    }

    private BotInfo GetBotInfo(int i)
    {
        if (turnPointer < playerBotMax)
        {
            return playerBotArray[i];
        }
        else
        {
            return enemyBotArray[i % playerBotMax];
        }
    }

    private void Awake()
    {
        instance = this;
        turnInProgress = false;
    }

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            playerBotArray[i].type = charType.Dev_PC;
            playerBotArray[i].characterBattle = SpawnCharacter(true, charType.Dev_PC, playerPosArray[i], i);
            playerBotArray[i].turnOrder = i;
            playerBotArray[i].isPlayerTeam = true;
            Debug.Log("Spawning PC " + i);
        }

        for (int i = 0; i < 3; i++)
        {
            enemyBotArray[i].type = charType.Dev_PC;
            enemyBotArray[i].characterBattle = SpawnCharacter(false, charType.Dev_AI, AiPosArray[i], i);
            enemyBotArray[i].turnOrder = i;
            enemyBotArray[i].isPlayerTeam = false;
            Debug.Log("Spawning AI " + i);
        }
        state = State.WaitingPlayer;
    }

    private CharacterBattle SpawnCharacter(bool isPlayerTeam, charType charToSpawn, Vector3 position, int turn)
    {
        Transform characterTransform;
        if (isPlayerTeam)
        {
            characterTransform = Instantiate(TestPlayerCharacer, position, Quaternion.identity);
        }
        else
        {
            characterTransform = Instantiate(GenericCharacter, position, Quaternion.identity);
        }
        CharacterBattle characterBattle = characterTransform.GetComponent<CharacterBattle>();
        characterBattle.Setup(isPlayerTeam, instance, turn);

        return characterBattle;
    }

    private void Update()
    {
        if (turnInProgress == false)
        {
            if (state == State.WaitingPlayer)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    turnInProgress = true;
                    state = State.BusyPlayer;
                    Debug.Log("TP: " + turnPointer);
                    playerBotArray[turnPointer % playerBotMax].characterBattle.DevAttackCallback(enemyBotArray[turnPointer % playerBotMax].characterBattle);
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    turnInProgress = true;
                    state = State.BusyPlayer;
                    Debug.Log("TP: " + turnPointer);
                    playerBotArray[turnPointer % playerBotMax].characterBattle.ActionOneCallback(enemyBotArray[turnPointer % playerBotMax].characterBattle);
                }
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    turnInProgress = true;
                    state = State.BusyPlayer;
                    Debug.Log("TP: " + turnPointer);
                    playerBotArray[turnPointer % playerBotMax].characterBattle.ActionTwoCallback(enemyBotArray[turnPointer % playerBotMax].characterBattle);
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    turnInProgress = true;
                    state = State.BusyPlayer;
                    Debug.Log("TP: " + turnPointer);
                    playerBotArray[turnPointer % playerBotMax].characterBattle.ActionThreeCallback(enemyBotArray[turnPointer % playerBotMax].characterBattle);
                }
            }
            else if (state == State.WaitingAI)
            {
                turnInProgress = true;
                state = State.BusyAI;
                Debug.Log("TP % playerBotMax: " + turnPointer % playerBotMax);
                enemyBotArray[turnPointer % playerBotMax].characterBattle.DevAttackCallback(playerBotArray[turnPointer % playerBotMax].characterBattle);
            }
        }
    }
}
