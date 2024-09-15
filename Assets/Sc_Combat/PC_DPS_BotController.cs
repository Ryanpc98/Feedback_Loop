using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_DPS_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance)
    {
        botName = "DPS";

        maxHealth = 25f;
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);

        maxEnergy = 10f;
        energyGainRate = 2;
        curEnergy = energyGainRate;
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);

        handler = instance;

        isPlayer = isPlayerTeam;

        actionOneDamage = 4;
        actionOneCost = 1;
        actionTwoDamage = 9;
        actionTwoCost = 2;
        actionThreeDamage = 3;
        actionThreeCost = 3;

        actionOneName = "Punch";
        actionTwoName = "Jab";
        actionThreeName = "Bomb";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Attack;
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

        //1: We have the energy for the AoE
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
            //2: Can kill with Two
            if (gameState.pcHPArray[i] <= actionTwoDamage && gameState.pcHPArray[i] > 0f)
            {
                if (ActionTwoCallback(handler.GetTargetFromIndex(true, i)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionTwo (Kill) on " + i);
                    return true;
                }
            }
            //3: Can kill with One
            else if (gameState.pcHPArray[i] <= actionOneDamage && gameState.pcHPArray[i] > 0f)
            {
                if (ActionOneCallback(handler.GetTargetFromIndex(true, i)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionOne (Kill) on " + i);
                    return true;
                }
            }
        }

        randomChoice = Random.Range(0, 3);

        switch (randomChoice)
        {
            case (0):
                //4: Attack Lowest HP with Two
                if (ActionTwoCallback(handler.GetTargetFromIndex(true, lowestHPIndex)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionTwo (Low) on " + lowestHPIndex);
                    return true;
                }
                //5: Attack Lowest HP with One
                else if (ActionOneCallback(handler.GetTargetFromIndex(true, lowestHPIndex)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionOne (Low) on " + lowestHPIndex);
                    return true;
                }
                break;
            case (1):
                int tgt = Random.Range(0, 3);
                //4: Attack Lowest HP with Two
                if (ActionTwoCallback(handler.GetTargetFromIndex(true, tgt)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionTwo (Random) on " + tgt);
                    return true;
                }
                //5: Attack Lowest HP with One
                else if (ActionOneCallback(handler.GetTargetFromIndex(true, tgt)))
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
            curTarget = target;
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
        Color flashColor = Color.green;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 1 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 1");
        StartCoroutine(curTarget.FlashColor(flashColor, time));
        curTarget.ApplyDamage(actionTwoDamage);

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
