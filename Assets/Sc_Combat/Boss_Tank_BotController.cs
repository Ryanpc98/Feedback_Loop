using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Tank_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance)
    {
        botName = "Tank";

        maxHealth = 70f;
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);

        maxEnergy = 20f;
        energyGainRate = 3;
        curEnergy = energyGainRate;
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);

        handler = instance;

        isPlayer = isPlayerTeam;

        actionOneDamage = 4;
        actionOneCost = 1;
        actionTwoDamage = 10;
        actionTwoCost = 2;
        actionThreeDamage = 5;
        actionThreeCost = 5;

        actionOneName = "Charge";
        actionTwoName = "Self Heal";
        actionThreeName = "Shatter";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Heal;
        actionThreeType = ActionType.AoE;

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

    public override bool ActionChooser(EnumTypes.GameStateInfo gameState)
    {
        float lowestHPValue = -1;
        int lowestHPIndex = -1;
        int randomChoice = -1;

        //1: Need to heal
        if (gameState.aiHPArray[gameState.selfIndex] <= 5f)
        {
            if (ActionTwoCallback(handler.GetTargetFromIndex(false, gameState.selfIndex)))
            {
                Debug.Log("AI: " + gameState.selfIndex + " Healing Self");
                return true;
            }
        }
        //2: We have the energy for the AoE
        if (curEnergy >= actionThreeCost)
        {
            if (ActionThreeCallback(handler.GetTargetFromIndex(true, 2)))
            {
                Debug.Log("AI: " + gameState.selfIndex + " using ActionThree (Opporunistic)");
                return true;
            }
        }
        for (int i = 0; i < gameState.pcBots; i++)
        {
            if ((lowestHPIndex == -1 || gameState.pcHPArray[i] < lowestHPValue) && (gameState.pcHPArray[i] > 0f))
            {
                lowestHPIndex = i;
                lowestHPValue = gameState.pcHPArray[i];
                Debug.Log("PC: " + lowestHPIndex + " has the lowest HP with " + lowestHPValue);
            }
        }

        randomChoice = Random.Range(0, 3);

        switch (randomChoice)
        {
            case (0):
                //3: Attack Lowest HP with One
                if (ActionOneCallback(handler.GetTargetFromIndex(true, lowestHPIndex)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionOne (Low) on " + lowestHPIndex);
                    return true;
                }
                break;
            case (1):
                int tgt = Random.Range(0, 3);
                //3: Attack Lowest HP with One
                if (ActionOneCallback(handler.GetTargetFromIndex(true, tgt)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionOne (Random) on " + tgt);
                    return true;
                }
                break;
            case (2):
                //Save
                handler.ReleaseAiLock();
                Debug.Log("AI: " + gameState.selfIndex + " is saving AWP");
                return false;
            default:
                break;
        }

        //No Valid Move Found
        handler.ReleaseAiLock();
        Debug.Log("AI: " + gameState.selfIndex + " found no action to take");
        return false;
    }

    //=====Action One=====//
    public override bool ActionOneCallback(BaseBotController target)
    {
        if (SpendEnergy(actionOneCost))
        {
            curTarget = target;
            StartCoroutine(ActionOneMain());
            state = State.Sliding;
            return true;
        }
        else
        {
            Debug.Log("Only " + curEnergy + "out of " + maxEnergy);
            return false;
        }
    }

    public override IEnumerator ActionOneMain()
    {
        Color flashColor = Color.green;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 1 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 1");
        StartCoroutine(curTarget.FlashColor(flashColor, time));
        curTarget.ApplyDamage(actionOneDamage);

        state = State.Returning;
    }

    //=====Action Two=====//
    public override bool ActionTwoCallback(BaseBotController target)
    {
        if (SpendEnergy(actionTwoCost))
        {
            curTarget = this;
            StartCoroutine(ActionTwoMain());
            state = State.Sliding;
            return true;
        }
        else
        {
            Debug.Log("Only " + curEnergy + "out of " + maxEnergy);
            return false;
        }
    }

    public override IEnumerator ActionTwoMain()
    {
        Color flashColor = Color.magenta;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 2 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 2");
        StartCoroutine(curTarget.FlashColor(flashColor, time));
        curTarget.ApplyHealing(actionTwoDamage);

        state = State.Returning;
    }

    //=====Action Three=====//
    public override bool ActionThreeCallback(BaseBotController target)
    {
        if (SpendEnergy(actionThreeCost))
        {
            curTarget = target;
            StartCoroutine(ActionThreeMain());
            state = State.Sliding;
            return true;
        }
        else
        {
            Debug.Log("Only " + curEnergy + "out of " + maxEnergy);
            return false;
        }
    }  

    public override IEnumerator ActionThreeMain()
    {
        Color flashColor = Color.black;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 3 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 3");
        StartCoroutine(curTarget.FlashColor(flashColor, time));
        handler.DealAoeDamage(isPlayer, actionThreeDamage);

        state = State.Returning;
    }
}
