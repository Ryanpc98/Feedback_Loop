using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_DPS_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance)
    {
        botName = "Pirate";

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

        actionOneName = "Stab";
        actionTwoName = "Slash";
        actionThreeName = "Bomb";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Attack;
        actionThreeType = ActionType.AoE;

        dmgTakenSFX = EnumTypes.SFX.playerDmg;
        deathSFX = EnumTypes.SFX.zombieDmg;
        actionOneSFX = EnumTypes.SFX.punch_2;
        actionTwoSFX = EnumTypes.SFX.clang;
        actionThreeSFX = EnumTypes.SFX.boom;

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

    public override bool ActionChooser(EnumTypes.GameStateInfo gameState)
    {
        int lowestHPIndex = 0;
        int randomChoice = -1;

        for (int i = 0; i < gameState.pcBots; i++)
        {
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

        // Find Lowest HP
        lowestHPIndex = FindLowest(gameState.pcHPArray);

        if (lowestHPIndex == 4)
        {
            Debug.Log("Cannot Find Alive Target");
            handler.ReleaseAiLock();
            return false;
        }

        //1: We have the energy for the AoE
        if (curEnergy >= actionThreeCost)
        {
            if (ActionThreeCallback(handler.GetTargetFromIndex(true, lowestHPIndex)))
            {
                Debug.Log("AI: " + gameState.selfIndex + " using ActionThree (Opporunistic)");
                return true;
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
                for (int t = 0; t < 3; t++)
                {
                    tgt = (tgt + 1) % 3;
                    if (gameState.pcHPArray[tgt] <= 0f)
                    {
                        break;
                    }
                }
                //4: Attack Random with Two
                if (ActionTwoCallback(handler.GetTargetFromIndex(true, tgt)))
                {
                    Debug.Log("AI: " + gameState.selfIndex + " using ActionTwo (Random) on " + tgt);
                    return true;
                }
                //5: Attack Random with One
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
        handler.PlaySFX(actionOneSFX);
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
        handler.PlaySFX(actionTwoSFX);
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
        handler.PlaySFX(actionThreeSFX);
        handler.DealAoeDamage(isPlayer, actionThreeDamage);

        state = State.Returning;
    }
}
