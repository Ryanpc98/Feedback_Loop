using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Tank_BotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance, int index)
    {
        botName = "Boss Knight";

        botID = index;

        maxHealth = 700f;
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);

        maxEnergy = 200f;
        energyGainRate = 30;
        curEnergy = energyGainRate;
        energyBar.UpdateEnergyBar(curEnergy, maxEnergy);

        handler = instance;

        isPlayer = isPlayerTeam;

        actionOneDamage = 40;
        actionOneCost = 10;
        actionTwoDamage = 100;
        actionTwoCost = 20;
        actionThreeDamage = 50;
        actionThreeCost = 50;

        actionOneName = "Charge";
        actionTwoName = "Self Heal";
        actionThreeName = "Shatter";

        actionOneType = ActionType.Attack;
        actionTwoType = ActionType.Heal;
        actionThreeType = ActionType.AoE;

        dmgTakenSFX = EnumTypes.SFX.playerDmg;
        deathSFX = EnumTypes.SFX.zombieDmg;
        actionOneSFX = EnumTypes.SFX.punch;
        actionTwoSFX = EnumTypes.SFX.tape;
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

    public override void ActionOneValue(TurnHandler.BotInfo[] bots, int self)
    {
        int lowest = FindLowest(bots);

        Debug.Log("========STARTING AI: " + self.ToString() + " DECISION ALGO FOR ACTION ONE========");

        if (CanDoActionOne())
        {
            for (int i = 0; i < bots.Length; i++)
            {
                if (bots[i].controller.IsAlive())
                {
                    // +1 Value for Alive Target
                    Debug.Log("AI: " + self.ToString() + " detected alive PC: " + i.ToString());
                    decisions.Add(new DecisionInfo { target = i, action = 1 });
                    if (lowest == i)
                    {
                        Debug.Log("AI: " + self.ToString() + " detected lowest PC: " + i.ToString());
                        // +1 Value for Alive target with Lowest HP
                        decisions.Add(new DecisionInfo { target = i, action = 1 });
                    }
                    if (bots[i].controller.GetHealth() <= actionOneDamage)
                    {
                        Debug.Log("AI: " + self.ToString() + " detected killable PC: " + i.ToString());
                        // +1 Value for Alive target that will die to attack
                        decisions.Add(new DecisionInfo { target = i, action = 1 });
                    }
                }
            }
        }
    }

    public override void ActionTwoValue(TurnHandler.BotInfo[] bots, int self)
    {
        Debug.Log("========STARTING AI: " + self.ToString() + "DECISION ALGO FOR ACTION TWO========");

        if (CanDoActionTwo())
        {
            if (GetHealthAsPct() <= 0.75f)
            {
                // +1 Value for Self Below 75% HP
                Debug.Log("AI: " + self.ToString() + " detected self below 0.75 HP");
                decisions.Add(new DecisionInfo { target = self, action = 2 });
                if (GetHealthAsPct() <= 0.20f)
                {
                    Debug.Log("AI: " + self.ToString() + " detected self below 0.20 HP");
                    // +1 Value for Alive target that is below 20% HP
                    decisions.Add(new DecisionInfo { target = self, action = 2 });
                }
            }
        }
    }

    public override void ActionThreeValue(TurnHandler.BotInfo[] bots, int self)
    {
        Debug.Log("========STARTING AI: " + self.ToString() + "DECISION ALGO FOR ACTION THREE========");

        if (CanDoActionThree())
        {
            // +1 Value for Having the Energy to do the attack
            Debug.Log("AI: " + self.ToString() + " has the energy for the AoE");
            decisions.Add(new DecisionInfo { target = 1, action = 3 });
            for (int i = 0; i < bots.Length; i++)
            {
                if (bots[i].controller.IsAlive())
                {
                    if (bots[i].controller.GetHealthAsPct() <= 0.50f)
                    {
                        Debug.Log("AI: " + self.ToString() + " detected low PC: " + i.ToString());
                        // +1 Value for Alive target that is below 50% health
                        decisions.Add(new DecisionInfo { target = i, action = 3 });
                    }
                }
            }
        }
    }

    public override void SaveValue(int self)
    {
        Debug.Log("========STARTING AI: " + self.ToString() + "DECISION ALGO FOR SAVING========");

        if (GetHealthAsPct() >= 0.15f)
        {
            if (!CanDoActionOne())
            {
                Debug.Log("AI: " + self.ToString() + " can't do Action One");
                // +1 Value for Alive target that is below 50% health
                decisions.Add(new DecisionInfo { target = -1, action = -1 });
            }
            if (!CanDoActionTwo())
            {
                Debug.Log("AI: " + self.ToString() + " can't do Action Two");
                // +1 Value for Alive target that is below 50% health
                decisions.Add(new DecisionInfo { target = -1, action = -1 });
            }
            if (!CanDoActionThree())
            {
                Debug.Log("AI: " + self.ToString() + " can't do Action Three");
                // +1 Value for Alive target that is below 50% health
                decisions.Add(new DecisionInfo { target = -1, action = -1 });
            }
            return;
        }
        Debug.Log("AI: " + self.ToString() + " too low, not considering saving");
    }

    public override bool IsValidTarget(int action, BaseBotController target)
    {
        switch (action)
        {
            case (1):
                if (target.IsAlive() && target.IsTargetable())
                {
                    return true;
                }
                return false;
            case (2):
                if (target.GetID() == GetID())
                {
                    if (GetHealthAsPct() < 1f)
                    {
                        return true;
                    }
                }
                return false;
            case (3):
                if (target.IsAlive() && target.IsTargetable())
                {
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    //=====Action One=====//
    public override bool ActionOneCallback(BaseBotController target)
    {
        if (SpendEnergy(actionOneCost) && target.IsAlive())
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
        if (SpendEnergy(actionTwoCost) && target.IsAlive())
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
        handler.PlaySFX(actionTwoSFX);
        curTarget.ApplyHealing(actionTwoDamage);

        state = State.Returning;
    }

    //=====Action Three=====//
    public override bool ActionThreeCallback(BaseBotController target)
    {
        if (SpendEnergy(actionThreeCost) && target.IsAlive())
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
