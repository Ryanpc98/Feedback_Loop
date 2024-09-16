using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Basic_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance)
    {
        botName = "Boss Soldier";

        maxHealth = 40f;
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
        actionTwoDamage = 4;
        actionTwoCost = 2;
        actionThreeDamage = 5;
        actionThreeCost = 3;

        actionOneName = "Melee";
        actionTwoName = "First Aid";
        actionThreeName = "Bash";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Heal;
        actionThreeType = ActionType.Attack;

        dmgTakenSFX = EnumTypes.SFX.playerDmg;
        deathSFX = EnumTypes.SFX.zombieDmg;
        actionOneSFX = EnumTypes.SFX.punch;
        actionTwoSFX = EnumTypes.SFX.tape;
        actionThreeSFX = EnumTypes.SFX.punch_2;

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
        
        // Find Lowest HP
        lowestHPIndex = FindLowest(gameState.pcHPArray);

        if (lowestHPIndex == 4)
        {
            Debug.Log("Cannot Find Alive Target");
            handler.ReleaseAiLock();
            return false;
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
                for (int t = 0; t < 3; t++)
                {
                    tgt = (tgt + 1) % 3;
                    if (gameState.pcHPArray[tgt] <= 0f)
                    {
                        break;
                    }
                }
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
        curTarget.ApplyDamage(actionThreeDamage);

        state = State.Returning;
    }
}
