using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dev_CharacterBattle : CharacterBattle
{
    public override void ActionOneCallback(CharacterBattle targetCharacterBattle) {
        attackTargetPosition = targetCharacterBattle.GetPosition();
        curTargetCharacterBattle = targetCharacterBattle;
        StartCoroutine(ActionOneMain());
        state = State.Sliding;
    }

    public override IEnumerator ActionOneMain()
    {
        Color flashColor = Color.green;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 1 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 1");
        StartCoroutine(curTargetCharacterBattle.FlashColor(flashColor, time));
        curTargetCharacterBattle.ApplyDamage(1f);

        state = State.Returning;
    }

    public override void ActionTwoCallback(CharacterBattle targetCharacterBattle) {
        attackTargetPosition = targetCharacterBattle.GetPosition();
        curTargetCharacterBattle = targetCharacterBattle;
        StartCoroutine(ActionTwoMain());
        state = State.Sliding;
    }

    public override IEnumerator ActionTwoMain()
    {
        Color flashColor = Color.magenta;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 2 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 2");
        StartCoroutine(curTargetCharacterBattle.FlashColor(flashColor, time));
        curTargetCharacterBattle.ApplyDamage(3f);

        state = State.Returning;
    }

    public override void ActionThreeCallback(CharacterBattle targetCharacterBattle) {
        attackTargetPosition = targetCharacterBattle.GetPosition();
        curTargetCharacterBattle = targetCharacterBattle;
        StartCoroutine(ActionThreeMain());
        state = State.Sliding;
    }

    public override IEnumerator ActionThreeMain()
    {
        Color flashColor = Color.black;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Action 3 Waiting");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Action 3");
        StartCoroutine(curTargetCharacterBattle.FlashColor(flashColor, time));
        curTargetCharacterBattle.ApplyDamage(5f);

        state = State.Returning;
    }
}
