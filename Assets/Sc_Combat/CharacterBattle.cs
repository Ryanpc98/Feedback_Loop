using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattle : MonoBehaviour
{
    SpriteRenderer sprite;
    protected State state;
    protected Vector3 attackTargetPosition;
    protected Vector3 startingPosition;
    protected CharacterBattle curTargetCharacterBattle;

    protected FloatingHealthBar healthBar;

    protected BattleHandler handler;
    protected int index;

    protected float slideSpeed = 3f;
    protected float reachedDistance = .1f;

    protected float maxHealth = 10f;
    protected float curHealth;

    protected enum State
    {
        Idle,
        Sliding,
        Attacking,
        Returning,
        Busy,
        Dead
    }

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        healthBar = GetComponent<FloatingHealthBar>();
        startingPosition = GetPosition();
        curHealth = maxHealth;
        index = -2; //Invalid Index, throw error if recieved
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        state = State.Idle; 
    }

    public void Setup(bool isPlayerTeam, BattleHandler instance, int turn)
    {
        handler = instance;
        index = turn;
        if (isPlayerTeam)
        {
            ChangeColor(Color.cyan);
        } else
        {
            ChangeColor(Color.red);
        }
    }

    public Color GetColor()
    {
        return sprite.color;
    }

    public void ChangeColor(Color color)
    {
        sprite.color = color;
    }

    public IEnumerator FlashColor(Color color, float time)
    {
        Debug.Log("Flashing color");
        Color curColor = GetColor();
        ChangeColor(color);
        yield return new WaitForSeconds(time);
        Debug.Log("Returning to color");
        ChangeColor(curColor);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void Update()
    {

        switch (state)
        {
            case State.Idle:
                break;
            case State.Busy:
                break;
            case State.Sliding:
                SlideToPosition(attackTargetPosition);
                if (Vector3.Distance(GetPosition(), attackTargetPosition) < reachedDistance)
                {
                    transform.position = attackTargetPosition;
                    state = State.Attacking;
                }
                break;
            case State.Returning:
                SlideToPosition(startingPosition);
                if (Vector3.Distance(GetPosition(), startingPosition) < reachedDistance)
                {
                    transform.position = startingPosition;
                    state = State.Idle;
                    handler.FinishedTurn();
                }
                break;
        }
    }
    
    private void SlideToPosition(Vector3 attackTargetPosition)
    {
        transform.position += (attackTargetPosition - GetPosition()) * slideSpeed * Time.deltaTime;
    }

    public void DevAttackCallback(CharacterBattle targetCharacterBattle)
    {
        attackTargetPosition = targetCharacterBattle.GetPosition();
        curTargetCharacterBattle = targetCharacterBattle;
        StartCoroutine(DevAttackMain());
        state = State.Sliding;
    }

    private IEnumerator DevAttackMain()
    {
        Color flashColor = Color.white;
        Color curColor = GetColor();
        float time = 0.1f;

        Debug.Log("Waiting to Attack");
        yield return new WaitUntil(() => state == State.Attacking);

        Debug.Log("Executing Attack");
        StartCoroutine(curTargetCharacterBattle.FlashColor(flashColor, time));
        curTargetCharacterBattle.ApplyDamage(2f);

        state = State.Returning;
    }

    public void ApplyDamage(float damage)
    {
        curHealth -= damage;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log("Owie, I have " + curHealth + " health");
        if (curHealth <= 0)
        {
            Debug.Log("Bruh I'm dead");
            state = State.Dead;
        }
    }

    public bool IsAlive()
    {
        if (state == State.Dead)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public float GetHealth()
    {
        return curHealth;
    }

    public virtual void ActionOneCallback(CharacterBattle targetCharacterBattle)
    {
        Debug.Log("ACTION ONE CALLBACK VIRTUAL FUNCTION");
        return;
    }
    public virtual IEnumerator ActionOneMain() {
        Debug.Log("ACTION ONE MAIN VIRTUAL FUNCTION");
        yield return new WaitForSeconds(0);
    }
    public virtual void ActionTwoCallback(CharacterBattle targetCharacterBattle) {
        Debug.Log("ACTION TWO CALLBACK VIRTUAL FUNCTION");
        return;
    }
    public virtual IEnumerator ActionTwoMain() {
        Debug.Log("ACTION TWO MAIN VIRTUAL FUNCTION");
        yield return new WaitForSeconds(0);
    }
    public virtual void ActionThreeCallback(CharacterBattle targetCharacterBattle) {
        Debug.Log("ACTION THREE CALLBACK VIRTUAL FUNCTION");
        return;
    }
    public virtual IEnumerator ActionThreeMain() {
        Debug.Log("ACTION THREE MAIN VIRTUAL FUNCTION");
        yield return new WaitForSeconds(0);
    }
}
