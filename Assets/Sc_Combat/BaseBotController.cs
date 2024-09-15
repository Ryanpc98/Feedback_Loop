using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBotController : MonoBehaviour
{
    /*======Variables======*/
    //Sprite renderer
    protected SpriteRenderer sprite;

    //Tracks Bot state
    protected State state;
    protected bool isPlayer = false;
    protected string botName = "DEV NAME";

    //Position info and target info
    protected Vector3 startingPosition;
    protected BaseBotController curTarget;

    //Health and Energy Bars
    protected FloatingHealthBar healthBar;
    protected FloatingEnergyBar energyBar;

    //Reference to the scene turn handler
    protected TurnHandler handler;

    //Animation Movement Values
    protected float slideSpeed = 3f;
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

    protected Color curColor;

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
    public virtual void Setup(bool isPlayerTeam, TurnHandler instance)
    {
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

        handler = instance;
        if (isPlayerTeam)
        {
            curColor = Color.cyan;
            ChangeColor(curColor);
        }
        else
        {
            curColor = Color.red;
            ChangeColor(curColor);
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

    public float getActionCost(int action)
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
        if (state == State.Dead)
        {
            return false;
        }
        else
        {
            return true;
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
            curEnergy = maxEnergy;
            energyBar.UpdateEnergyBar(curEnergy, maxEnergy);
            return true;
        }
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
                SlideToPosition(curTarget.GetPosition());
                if (Vector3.Distance(GetPosition(), curTarget.GetPosition()) < reachedDistance)
                {
                    transform.position = curTarget.GetPosition();
                    state = State.Attacking;
                }
                break;
            case State.Returning:
                SlideToPosition(startingPosition);
                if (Vector3.Distance(GetPosition(), startingPosition) < reachedDistance)
                {
                    transform.position = startingPosition;
                    state = State.Idle;
                    handler.ReleaseAiLock();
                }
                break;
        }
    }

    //Applies damage and checks for death
    public void ApplyDamage(float damage)
    {
        curHealth -= damage;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log("Owie, I have " + curHealth + " health");
        if (curHealth <= 0)
        {
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
        curEnergy = energyGainRate;
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
    public void ChangeDamage(int action, float val)
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

    // Changes max health by val%
    public void ChangeMaxHealth(float val)
    {
        maxHealth += (float)Math.Round((maxHealth * val), 1, MidpointRounding.AwayFromZero);
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
    }

    // Changes the cost of action by val
    public void ChangeActionCost(int action, int val)
    {
        switch (action)
        {
            case (1):
                if (actionOneCost + val <= 1)
                {
                    actionOneCost = 1;
                }
                else
                {
                    actionOneCost += val;
                }
                break;
            case (2):
                if (actionTwoCost + val <= 1)
                {
                    actionTwoCost = 1;
                }
                else
                {
                    actionTwoCost += val;
                }
                break;
            case (3):
                if (actionThreeCost + val <= 1)
                {
                    actionThreeCost = 1;
                }
                else
                {
                    actionThreeCost += val;
                }
                break;
        }
    }

    // Changes the cost of all actions by val
    public void ChangeAllActionCosts(int val)
    {
        // Action One
        if (actionOneCost + val <= 1)
        {
            actionOneCost += val;
        }
        else
        {
            actionOneCost = 1;
        }

        //Action Two
        if (actionTwoCost + val <= 1)
        {
            actionTwoCost += val;
        }
        else
        {
            actionTwoCost = 1;
        }

        //Action Three
        if (actionThreeCost + val <= 1)
        {
            actionThreeCost += val;
        }
        else
        {
            actionThreeCost = 1;
        }
    }


    /*======Actions======*/
    //UNIMPLEMENTED //Generic functions for children to overrride

    public virtual bool ActionChooser(EnumTypes.GameStateInfo gameState)
    {
        Debug.Log("ACTION CHOOSER CALLBACK VIRTUAL FUNCTION");
        handler.ReleaseAiLock();
        return false;
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
}
