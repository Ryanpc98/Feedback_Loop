using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_Healer_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance)
    {
        botName = "Healer";

        maxHealth = 15f;
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);

        maxEnergy = 10f;
        energyGainRate = 2;
        curEnergy = energyGainRate;
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);

        handler = instance;

        isPlayer = isPlayerTeam;

        actionOneDamage = 2;
        actionOneCost = 1;
        actionTwoDamage = 6;
        actionTwoCost = 2;
        actionThreeDamage = 4;
        actionThreeCost = 3;

        actionOneName = "Slash";
        actionTwoName = "Heal";
        actionThreeName = "Energize";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Heal;
        actionThreeType = ActionType.Buff;

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
        float lowestEnergyValue = -1;
        int lowestEnergyIndex = -1;
        int randomChoice = -1;

        //1: Need to heal self
        if (gameState.aiHPArray[gameState.selfIndex] <= 5f)
        {
            if (ActionTwoCallback(handler.GetTargetFromIndex(false, gameState.selfIndex)))
            {
                Debug.Log("AI: " + gameState.selfIndex + " Healing Self");
                return true;
            }
        }
        for (int i = 0; i < gameState.aiBots; i++)
        {
            if ((lowestHPIndex == -1 || gameState.aiHPArray[i] < lowestHPValue) && (gameState.aiHPArray[i] > 0f))
            {
                lowestHPIndex = i;
                lowestHPValue = gameState.aiHPArray[i];
                Debug.Log("PC: " + lowestHPIndex + " has the lowest HP with " + lowestHPValue);
            }
            if ((lowestEnergyIndex == -1 || gameState.aiEnergyArrayPct[i] < lowestEnergyValue) && (gameState.aiHPArray[i] > 0f))
            {
                lowestEnergyIndex = i;
                lowestEnergyValue = gameState.aiEnergyArrayPct[i];
                Debug.Log("PC: " + lowestHPIndex + " has the lowest Energy with " + lowestHPValue);
            }
        }
        //2: Heal Lowest HP with Two
        if (ActionTwoCallback(handler.GetTargetFromIndex(false, lowestHPIndex)))
        {
            Debug.Log("AI: " + gameState.selfIndex + " using ActionTwo (Low) on " + lowestHPIndex);
            return true;
        }
        //3: Energize Lowest Energy with Three
        if (ActionThreeCallback(handler.GetTargetFromIndex(false, lowestEnergyIndex)))
        {
            Debug.Log("AI: " + gameState.selfIndex + " using ActionThree (Low) on " + lowestHPIndex);
            return true;
        }

        randomChoice = Random.Range(0, 3);

        switch (randomChoice)
        {
            case (0):
                //4: Attack Lowest HP with One
                if (ActionOneCallback(handler.GetTargetFromIndex(true, lowestHPIndex)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionOne (Low) on " + lowestHPIndex);
                    return true;
                }
                break;
            case (1):
                int tgt = Random.Range(0, 3);
                //5: Attack random enemy
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

        //TODO: Add Logic for Energize
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
        curTarget.GrantEnergy(actionThreeDamage);

        state = State.Returning;
    }
}
