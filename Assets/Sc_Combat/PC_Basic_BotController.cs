using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_Basic_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance)
    {
        botName = "Basic Bot";

        maxHealth = 20f;
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);

        maxEnergy = 10f;
        energyGainRate = 2;
        curEnergy = energyGainRate;
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);

        handler = instance;

        isPlayer = isPlayerTeam;

        actionOneDamage = 3;
        actionOneCost = 1;
        actionTwoDamage = 3;
        actionTwoCost = 2;
        actionThreeDamage = 10;
        actionThreeCost = 6;

        actionOneName = "Punch";
        actionTwoName = "Heal";
        actionThreeName = "Kick";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Heal;
        actionThreeType = ActionType.Attack;

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
        if (GetHealthAsPct() <= 0.2f)
        {
            if (ActionTwoCallback(handler.GetTargetFromIndex(false, gameState.selfIndex)))
            {
                Debug.Log("AI: " + gameState.selfIndex + " Healing Self");
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
            //2: Can kill with Three
            if (gameState.pcHPArray[i] <= actionThreeDamage && gameState.pcHPArray[i] > 0f)
            {
                if (ActionThreeCallback(handler.GetTargetFromIndex(true, i)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionThree (Kill) on " + i);
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
        for (int j = 0; j < gameState.aiBots; j++)
        {
            //4: Need to Heal Teammate
            if (gameState.aiHPArray[j] <= 5f && gameState.aiHPArray[j] > 0f)
            {
                if (ActionTwoCallback(handler.GetTargetFromIndex(false, j)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionTwo on " + j);
                    return true;
                }
            }
        }
        randomChoice = Random.Range(0, 3);

        switch (randomChoice)
        {
            case (0):
                //5: Attack Lowest HP with Three
                if (ActionThreeCallback(handler.GetTargetFromIndex(true, lowestHPIndex)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionThree (Low) on " + lowestHPIndex);
                    return true;
                }
                //6: Attack Lowest HP with One
                else if (ActionOneCallback(handler.GetTargetFromIndex(true, lowestHPIndex)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionOne (Low) on " + lowestHPIndex);
                    return true;
                }
                break;
            case (1):
                int tgt = Random.Range(0, 3);
                //5: Attack Random with Three
                if (ActionThreeCallback(handler.GetTargetFromIndex(true, tgt)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionThree (Random) on " + tgt);
                    return true;
                }
                //6: Attack Random with One
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
        curTarget.ApplyDamage(actionThreeDamage);

        state = State.Returning;
    }
}
