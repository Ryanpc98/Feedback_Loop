using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBotController : MonoBehaviour
{
    [SerializeField] protected Sprite d_sprite;
    [SerializeField] protected Sprite l_sprite;
    [SerializeField] protected Sprite r_sprite;
    [SerializeField] protected Sprite u_sprite;

    [SerializeField] protected Sprite e_d_sprite;
    [SerializeField] protected Sprite e_l_sprite;
    [SerializeField] protected Sprite e_r_sprite;
    [SerializeField] protected Sprite e_u_sprite;

    /*======Variables======*/
    //Sprite renderer
    protected SpriteRenderer sprite;

    //Tracks Bot state
    protected State state;
    protected bool isPlayer = false;
    protected string botName = "DEV NAME";
    protected int botID;

    //Position info and target info
    protected Vector3 startingPosition;
    protected BaseBotController curTarget;

    //Health and Energy Bars
    protected FloatingHealthBar healthBar;
    protected FloatingEnergyBar energyBar;

    //Reference to the scene turn handler
    protected TurnHandler handler;

    //Animation Movement Values
    protected float slideSpeed = 4.5f;
    protected float reachedDistance = .1f;

    //Health Values
    protected float maxHealth = 10f;
    protected float curHealth;

    //Energy Values
    protected float maxEnergy = 10f;
    protected float curEnergy;
    protected float energyGainRate;

    //Action Information
    //Damage can refer to healing or damage
    protected float actionOneDamage = 0;
    protected float actionOneCost = 0;
    protected float actionTwoDamage = 0;
    protected float actionTwoCost = 0;
    protected float actionThreeDamage = 0;
    protected float actionThreeCost = 0;

    protected string actionOneName = "DEV_NAME_ONE";
    protected string actionTwoName = "DEV_NAME_TWO";
    protected string actionThreeName = "DEV_NAME_THREE";

    protected ActionType actionOneType = ActionType.Attack;
    protected ActionType actionTwoType = ActionType.Attack;
    protected ActionType actionThreeType = ActionType.Attack;

    //Sounds Associated With the Bot
    protected EnumTypes.SFX dmgTakenSFX = EnumTypes.SFX.playerDmg;
    protected EnumTypes.SFX deathSFX = EnumTypes.SFX.zombieDmg;
    protected EnumTypes.SFX actionOneSFX = EnumTypes.SFX.clang;
    protected EnumTypes.SFX actionTwoSFX = EnumTypes.SFX.punch;
    protected EnumTypes.SFX actionThreeSFX = EnumTypes.SFX.hitSound;

    protected bool taunting = false;
    protected bool targetable = true;

    protected Color curColor;

    // Stores information for AI decision algorithm
    public struct DecisionInfo
    {
        public int target;
        public int action;
    }

    public List<DecisionInfo> decisions = new List<DecisionInfo>();

    
    //Possible Bot States
    protected enum State
    {
        Idle,
        Sliding,
        Attacking,
        Returning,
        Busy,
        Dead
    }

    public enum ActionType
    {
        Attack,
        AoE,
        Heal,
        Buff,
        Debuff
    }

    /*======Functions======*/
        /*======Utilities======*/

    //Initialization function to pass in data from the handler   
    public virtual void Setup(bool isPlayerTeam, TurnHandler instance, int index)
    {
        botID = index;

        maxHealth = 20f;
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);

        maxEnergy = 10f;
        energyGainRate = 2;
        curEnergy = energyGainRate;
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Attack;
        actionThreeType = ActionType.Attack;

        dmgTakenSFX = EnumTypes.SFX.playerDmg;
        deathSFX = EnumTypes.SFX.zombieDmg;
        actionOneSFX = EnumTypes.SFX.clang;
        actionTwoSFX = EnumTypes.SFX.punch;
        actionThreeSFX = EnumTypes.SFX.hitSound;

        handler = instance;
        if (isPlayerTeam)
        {
            curColor = Color.white;
            ChangeColor(curColor);
            sprite.sprite = d_sprite;
        }
        else
        {
            curColor = Color.white;
            ChangeColor(curColor);
            sprite.sprite = e_d_sprite;
        }
    }

    //Get current color
    public Color GetColor()
    {
        return sprite.color;
    }

    //Set color
    public void ChangeColor(Color color)
    {
        sprite.color = color;
    }

    // Returns bot name
    public string GetName()
    {
        return botName;
    }

    public int GetID()
    {
        return botID;
    }

    public float GetActionCost(int action)
    {
        switch (action)
        {
            case (1):
                return actionOneCost;
            case (2):
                return actionTwoCost;
            case (3):
                return actionThreeCost;
            default:
                return -1f;
        }
    }

    //ASYNC //changes color for the time specified
    public IEnumerator FlashColor(Color color, float time)
    {
        ChangeColor(color);
        yield return new WaitForSeconds(time);
        ChangeColor(curColor);
    }

    //Returns current position
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    //Moves closer to target, called in update
    private void SlideToPosition(Vector3 attackTargetPosition)
    {
        transform.position += (attackTargetPosition - GetPosition()) * slideSpeed * Time.deltaTime;
    }

    //for TurnHandler to check status
    public bool IsAlive()
    {
        if (GetHealthAsPct() > 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //For other Bots to make decisions
    // Return Health Value
    public float GetHealth()
    {
        return curHealth;
    }

    // Return Health as a percentage of total
    public float GetHealthAsPct()
    {
        return curHealth / maxHealth;
    }

    // Return Current Energy
    public float GetEnergy()
    {
        return curEnergy;
    }

    // Return Energy as a percentage of total
    public float GetEnergyAsPct()
    {
        return curEnergy / maxEnergy;
    }

    // Get Action Type
    public ActionType GetActionType(int action)
    {
        switch (action)
        {
            case (1):
                return actionOneType;
            case (2):
                return actionTwoType;
            case (3):
                return actionThreeType;
            default:
                return ActionType.Attack;
        }
    }

    //Returns if attack is possible
    public bool SpendEnergy(float cost)
    {
        Debug.Log("Attemping to Spend " + cost.ToString() + " energy out of " + curEnergy.ToString());
        if (curEnergy >= cost && IsAlive() && state == State.Idle)
        {
            curEnergy -= cost;
            energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
            return true;
        }
        else
        {
            return false; 
        }
    }

    //Returns if attack is possible
    public bool CheckEnergy(float cost)
    {
        Debug.Log("Checking on spending " + cost.ToString() + " energy out of " + curEnergy.ToString());
        if (curEnergy >= cost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Give Energy
    public bool GrantEnergy(float energy)
    {
        if ((curEnergy + energy) <= maxEnergy)
        {
            curEnergy += energy;
            energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
            return true;
        }
        else
        {
            curEnergy = maxEnergy;
            energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
            return true;
        }
    }

    // Remove Energy
    public bool SapEnergy(float cost)
    {
        if ((curEnergy - cost) >= 0)
        {
            curEnergy -= cost;
            energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
            return true;
        }
        else
        {
            curEnergy = 0;
            energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
            return true;
        }
    }

    //Taunt Handlers
    public bool IsTaunting()
    {
        return taunting;
    }

    public void SetTaunting()
    {
        taunting = true;
        targetable = true;
        handler.HandleTaunt(!isPlayer, true);
    }

    public void ClearTaunting()
    {
        taunting = false;
        handler.HandleTaunt(!isPlayer, false);
    }

    //Targetable Handlers
    public bool IsTargetable()
    {
        return targetable;
    }

    public void SetTargetable(bool enable)
    {
        Color tmp;
        tmp = curColor;
        if (enable)
        {
            tmp.a = 1f;
        }
        else
        {
            tmp.a = 0.6f;
        }
        curColor = tmp;
        ChangeColor(curColor);
        targetable = enable;
    }

    //Called at the start of each turn
    public void AddTurnEnergy()
    {
        curEnergy += energyGainRate;
        if (curEnergy >= maxEnergy)
        {
            curEnergy = maxEnergy;
        }
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
    }

    // Find Lowest in Array
    // Used for ActionChooser
    protected int FindLowest(float[] arr)
    {
        int index = 4;
        float val = 999f;

        for (int i = 0; i < arr.Length; i++)
        {
            if ((arr[i] < val) && (arr[i] > 0f))
            {
                index = i;
                val = arr[i];
                Debug.Log("PC: " + index + " has the lowest HP with " + val);
            }
        }

        return index;
    }

    protected int FindLowest(TurnHandler.BotInfo[] arr)
    {
        int index = 4;
        float val = 999f;

        for (int i = 0; i < arr.Length; i++)
        {
            if ((arr[i].controller.GetHealth() < val) && (arr[i].controller.GetHealth() > 0f))
            {
                index = i;
                val = arr[i].controller.GetHealth();
                Debug.Log("PC: " + index + " has the lowest HP with " + val);
            }
        }

        return index;
    }

    protected int FindHighestDamage(TurnHandler.BotInfo[] arr)
    {
        int index = 4;
        float val = -1;

        for (int i = 0; i < arr.Length; i++)
        {
            if ((arr[i].controller.actionOneDamage > val) && (arr[i].controller.IsAlive()) && (GetActionType(1) == ActionType.Attack))
            {
                index = i;
                val = actionOneDamage;
                Debug.Log("AI: " + index + " has the highest damage with " + val);
            }
        }

        return index;
    }

    /*======Gameplay======*/
    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        healthBar = GetComponent<FloatingHealthBar>();
        energyBar = GetComponent<FloatingEnergyBar>();
        startingPosition = GetPosition();
        state = State.Idle;
    }

    // Handles sprite movement
    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.Busy:
                break;
            case State.Sliding:
                if (isPlayer)
                {
                    sprite.sprite = r_sprite;
                }
                else
                {
                    sprite.sprite = e_l_sprite;
                }
                SlideToPosition(curTarget.GetPosition());
                if (Vector3.Distance(GetPosition(), curTarget.GetPosition()) < reachedDistance)
                {
                    transform.position = curTarget.GetPosition();
                    state = State.Attacking;
                }
                break;
            case State.Returning:
                if (isPlayer)
                {
                    sprite.sprite = l_sprite;
                }
                else
                {
                    sprite.sprite = e_r_sprite;
                }
                SlideToPosition(startingPosition);
                if (Vector3.Distance(GetPosition(), startingPosition) < reachedDistance)
                {
                    transform.position = startingPosition;
                    if (isPlayer)
                    {
                        sprite.sprite = d_sprite;
                    }
                    else
                    {
                        sprite.sprite = e_d_sprite;
                    }
                    state = State.Idle;
                    handler.ReleaseAiLock();
                }
                break;
        }
    }

    //Applies damage and checks for death
    public void ApplyDamage(float damage)
    {
        handler.PlaySFX(dmgTakenSFX);
        curHealth -= damage;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log("Owie, I have " + curHealth + " health");
        if (curHealth <= 0)
        {
            handler.PlaySFX(deathSFX);
            curEnergy = 0f;
            energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
            Debug.Log("Bruh I'm dead");
            state = State.Dead;
            handler.setDeadCheckFlag();
        }
    }

    // Heal target
    public void ApplyHealing(float health)
    {
        curHealth += health;
        if (curHealth > maxHealth)
        {
            Debug.Log("No overheals man");
            curHealth = maxHealth;
        }
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log("Pog, I have " + curHealth + " health");
    }

    public void Revive(float health)
    {
        curHealth = health;
        if (curHealth > maxHealth)
        {
            Debug.Log("No overheals man");
            curHealth = maxHealth;
        }
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log("Pog, I have " + curHealth + " health");
        state = State.Idle;
        handler.setDeadCheckFlag();
    }

    /*======Stat Modifiers======*/
    // Changes the energy gain rate by val
    public void ChangeEnergyGainRate(int val)
    {
        // Stop energy gain rate from going negative
        if ((energyGainRate + val) <= 1)
        {
            energyGainRate = 1;
        }
        else
        {
            energyGainRate += val;
        }
        if (curEnergy >= maxEnergy)
        {
            curEnergy = maxEnergy;
        }
        else
        {
            curEnergy = energyGainRate;
        }
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
    }

    // Changes the max energy by val
    public void ChangeMaxEnergy(int val)
    {
        // Stop energy gain rate from going negative
        if ((maxEnergy + val) <= 1)
        {
            maxEnergy = 1;
        }
        else
        {
            maxEnergy += val;
        }
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
    }

    // Changes damage/healing/whatever for action by val%
    public void ChangeDamageMult(int action, float val)
    {
        switch (action)
        {
            case (1):
                actionOneDamage += (actionOneDamage * val);
                break;
            case (2):
                actionTwoDamage += (actionTwoDamage * val);
                break;
            case (3):
                actionThreeDamage += (actionThreeDamage * val);
                break;
        }
    }

    // Changes damage/healing/whatever for action by val%
    public void ChangeDamageAdd(int action, float val)
    {
        switch (action)
        {
            case (1):
                actionOneDamage += val;
                break;
            case (2):
                actionTwoDamage += actionTwoDamage * val;
                break;
            case (3):
                actionThreeDamage += actionThreeDamage * val;
                break;
        }
    }

    // Changes max health by val%
    public void ChangeMaxHealth(float val)
    {
        maxHealth += (float)Math.Round((maxHealth * val), 0, MidpointRounding.AwayFromZero);
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
    }

    // Changes the cost of action by val
    public void ChangeActionCost(int action, int val)
    {
        switch (action)
        {
            case (1):
                if ((actionOneCost + val) <= 1)
                {
                    actionOneCost = 1;
                }
                else
                {
                    actionOneCost += val;
                }
                Debug.Log("Action One Cost is Now: " + actionOneCost);
                break;
            case (2):
                if ((actionTwoCost + val) <= 1)
                {
                    actionTwoCost = 1;
                }
                else
                {
                    actionTwoCost += val;
                }
                Debug.Log("Action Two Cost is Now: " + actionTwoCost);
                break;
            case (3):
                if ((actionThreeCost + val) <= 1)
                {
                    actionThreeCost = 1;
                }
                else
                {
                    actionThreeCost += val;
                }
                Debug.Log("Action Three Cost is Now: " + actionThreeCost);
                break;
        }
    }

    // Changes the cost of all actions by val
    public void ChangeAllActionCosts(int val)
    {
        for(int i = 4; i <= 3; i++)
        {
            ChangeActionCost(i, val);
        }
    }

    // Overloaded for new AI algorithm
    public bool ActionChooser(TurnHandler.BotInfo[] AIBots, TurnHandler.BotInfo[] PCBots, int self)
    {
        decisions = new List<DecisionInfo>();

        // Pass Action One the correct array
        if (ActionOneSide())
        {
            ActionOneValue(AIBots, self);
        }
        else
        {
            ActionOneValue(PCBots, self);
        }

        // Pass Action Two the correct array
        if (ActionTwoSide())
        {
            ActionTwoValue(AIBots, self);
        }
        else
        {
            ActionTwoValue(PCBots, self);
        }

        // Pass Action Three the correct array
        if (ActionThreeSide())
        {
            ActionThreeValue(AIBots, self);
        }
        else
        {
            ActionThreeValue(PCBots, self);
        }

        SaveValue(self);

        int numChoices = decisions.Count;

        if (numChoices == 0)
        {
            //No Valid Move Found
            Debug.Log("AI: " + self + " found no action to take");
            return false;
        }
        else
        {
            int choice = UnityEngine.Random.Range(0, numChoices);

            for (int i = 0; i < decisions.Count; i++)
            {
                Debug.Log("---AI: " + self.ToString() + " HAS OPTION " + decisions[i].action.ToString() + " ON TARGET " + decisions[i].target.ToString() + "---");
            }

            Debug.Log("#########-AI: " + self.ToString() + " DECIDED TO USE ACTION " + decisions[choice].action.ToString() + " ON TARGET " + decisions[choice].target.ToString() + "-#########");
            switch (decisions[choice].action)
            {
                case -1:
                    return false;
                case 1:
                    return ActionOneCallback(handler.GetTargetFromIndex(!ActionOneSide(), decisions[choice].target));
                case 2:
                    return ActionTwoCallback(handler.GetTargetFromIndex(!ActionTwoSide(), decisions[choice].target));
                case 3:
                    return ActionThreeCallback(handler.GetTargetFromIndex(!ActionThreeSide(), decisions[choice].target));
                default:
                    return false;
            }
        }
    }


    /*======Actions======*/
    //UNIMPLEMENTED //Generic functions for children to overrride

    public virtual bool IsValidTarget(int action, BaseBotController target)
    {
        Debug.Log("DetermineValidTarget VIRTUAL FUNCTION");
        switch (action)
        {
            case (1):
                return false;
            case (2):
                return false;
            case (3):
                return false;
            default:
                return false;
        }
    }

    public virtual void SaveValue(int self)
    {
        Debug.Log("SAVE VALUE VIRTUAL FUNCTION");
    }

    //=====Action One=====//
    public virtual bool ActionOneCallback(BaseBotController target)
    {
        Debug.Log("ACTION ONE CALLBACK VIRTUAL FUNCTION");
        return false;
    }
    public virtual IEnumerator ActionOneMain()
    {
        Debug.Log("ACTION ONE MAIN VIRTUAL FUNCTION");
        yield return new WaitForSeconds(0);
    }
    public string ActionOneName()
    {
        return actionOneName + " - (" + actionOneCost.ToString() + ")";
    }
    public bool ActionOneSide()
    {
        switch (actionOneType)
        {
            case (ActionType.Attack):
            case (ActionType.AoE):
            case (ActionType.Debuff):
                return false;
            case (ActionType.Buff):
            case (ActionType.Heal):
                return true;
            default:
                return false;
        }
    }
    public bool CanDoActionOne()
    {
        if (curEnergy >= actionOneCost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public virtual void ActionOneValue(TurnHandler.BotInfo[] bots, int self)
    {
        Debug.Log("ACTION ONE VALUE VIRTUAL FUNCTION");
    }

    //=====Action Two=====//
    public virtual bool ActionTwoCallback(BaseBotController target)
    {
        Debug.Log("ACTION TWO CALLBACK VIRTUAL FUNCTION");
        return false;
    }
    public virtual IEnumerator ActionTwoMain()
    {
        Debug.Log("ACTION TWO MAIN VIRTUAL FUNCTION");
        yield return new WaitForSeconds(0);
    }
    public string ActionTwoName()
    {
        return actionTwoName + " - (" + actionTwoCost.ToString() + ")";
    }
    public bool ActionTwoSide()
    {
        switch (actionTwoType)
        {
            case (ActionType.Attack):
            case (ActionType.AoE):
            case (ActionType.Debuff):
                return false;
            case (ActionType.Buff):
            case (ActionType.Heal):
                return true;
            default:
                return false;
        }
    }
    public bool CanDoActionTwo()
    {
        if (curEnergy >= actionTwoCost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public virtual void ActionTwoValue(TurnHandler.BotInfo[] bots, int self)
    {
        Debug.Log("ACTION TWO VALUE VIRTUAL FUNCTION");
    }

    //=====Action Three=====//
    public virtual bool ActionThreeCallback(BaseBotController target)
    {
        Debug.Log("ACTION THREE CALLBACK VIRTUAL FUNCTION");
        return false;
    }
    public virtual IEnumerator ActionThreeMain()
    {
        Debug.Log("ACTION THREE MAIN VIRTUAL FUNCTION");
        yield return new WaitForSeconds(0);
    }
    public string ActionThreeName()
    {
        return actionThreeName + " - (" + actionThreeCost.ToString() + ")";
    }
    public bool ActionThreeSide()
    {
        switch (actionThreeType)
        {
            case (ActionType.Attack):
            case (ActionType.AoE):
            case (ActionType.Debuff):
                return false;
            case (ActionType.Buff):
            case (ActionType.Heal):
                return true;
            default:
                return false;
        }
    }
    public bool CanDoActionThree()
    {
        if (curEnergy >= actionThreeCost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public virtual void ActionThreeValue(TurnHandler.BotInfo[] bots, int self)
    {
        Debug.Log("ACTION THREE VALUE VIRTUAL FUNCTION");
    }
}
