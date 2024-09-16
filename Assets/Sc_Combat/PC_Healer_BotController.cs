using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_Healer_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance)
    {
        botName = "Doctor";

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

        actionOneName = "Poke";
        actionTwoName = "Patch Up";
        actionThreeName = "Energize";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Heal;
        actionThreeType = ActionType.Buff;

        dmgTakenSFX = EnumTypes.SFX.playerDmg;
        deathSFX = EnumTypes.SFX.zombieDmg;
        actionOneSFX = EnumTypes.SFX.clang;
        actionTwoSFX = EnumTypes.SFX.tape;
        actionThreeSFX = EnumTypes.SFX.schwoop;

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
        int lowestEnergyIndex = 0;
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
        
        // Find Lowest HP
        lowestHPIndex = FindLowest(gameState.pcHPArray);

        if (lowestHPIndex == 4)
        {
            Debug.Log("Cannot Find Alive Target");
            handler.ReleaseAiLock();
            return false;
        }

        lowestEnergyIndex = FindLowest(gameState.pcEnergyArrayPct);

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
                for (int t = 0; t < 3; t++)
                {
                    tgt = (tgt + 1) % 3;
                    if (gameState.pcHPArray[tgt] <= 0f)
                    {
                        break;
                    }
                }
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
        Color flashColor = Color.magenta;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 2 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 2");
        StartCoroutine(curTarget.FlashColor(flashColor, time));
        handler.PlaySFX(actionTwoSFX);
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
        handler.PlaySFX(actionThreeSFX);
        curTarget.GrantEnergy(actionThreeDamage);

        state = State.Returning;
    }
}
