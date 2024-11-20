using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevBotController : BaseBotController
{
    public override void Setup(bool isPlayerTeam, TurnHandler instance, int index)
    {
        botName = "DevBot";

        botID = index;

        maxHealth = 20f;
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
        actionTwoDamage = 3;
        actionTwoCost = 2;
        actionThreeDamage = 10;
        actionThreeCost = 6;

        actionOneName = "DEV_ATTACK_ONE";
        actionTwoName = "DEV_HEAL_TWO";
        actionThreeName = "DEV_ATTACK_THREE";

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
        int lowest = FindLowest(bots);

        Debug.Log("========STARTING AI: " + self.ToString() + "DECISION ALGO FOR ACTION TWO========");

        if (CanDoActionTwo())
        {
            for (int i = 0; i < bots.Length; i++)
            {
                if (bots[i].controller.IsAlive())
                {
                    if (bots[i].controller.GetHealthAsPct() <= 0.75f)
                    {
                        // +1 Value for Alive Target Below 75% HP
                        Debug.Log("AI: " + self.ToString() + " detected AI below 0.75 HP: " + i.ToString());
                        decisions.Add(new DecisionInfo { target = i, action = 2 });
                        if (lowest == i)
                        {
                            Debug.Log("AI: " + self.ToString() + " detected lowest AI: " + i.ToString());
                            // +1 Value for Alive target with Lowest HP
                            decisions.Add(new DecisionInfo { target = i, action = 2 });
                        }
                        if (bots[i].controller.GetHealthAsPct() <= 0.20f)
                        {
                            Debug.Log("AI: " + self.ToString() + " detected AI below 0.20 HP: " + i.ToString());
                            // +1 Value for Alive target that is below 20% HP
                            decisions.Add(new DecisionInfo { target = i, action = 2 });
                            if (i == self)
                            {
                                Debug.Log("AI: " + self.ToString() + " detected self below 0.20 HP");
                                // +1 Value for self being below 20% HP
                                decisions.Add(new DecisionInfo { target = i, action = 2 });
                            }
                        }
                    }
                }
            }
        }
    }

    public override void ActionThreeValue(TurnHandler.BotInfo[] bots, int self)
    {
        int lowest = FindLowest(bots);

        Debug.Log("========STARTING AI: " + self.ToString() + " DECISION ALGO FOR ACTION THREE========");

        if (CanDoActionThree())
        {
            for (int i = 0; i < bots.Length; i++)
            {
                if (bots[i].controller.IsAlive())
                {
                    // +1 Value for Alive Target
                    Debug.Log("AI: " + self.ToString() + " detected alive PC: " + i.ToString());
                    decisions.Add(new DecisionInfo { target = i, action = 3 });
                    if (lowest == i)
                    {
                        Debug.Log("AI: " + self.ToString() + " detected lowest PC: " + i.ToString());
                        // +1 Value for Alive target with Lowest HP
                        decisions.Add(new DecisionInfo { target = i, action = 3 });
                    }
                    if (bots[i].controller.GetHealth() <= actionThreeDamage)
                    {
                        Debug.Log("AI: " + self.ToString() + " detected killable PC: " + i.ToString());
                        // +1 Value for Alive target that will die to attack
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