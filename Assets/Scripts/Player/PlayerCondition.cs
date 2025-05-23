using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakePhysicalDamage(int damage);
}

public class PlayerCondition : MonoBehaviour ,IDamageable
{
    public UICondition uiCondition;
    
    Condition health { get { return uiCondition.health; } }
    Condition stamina { get { return uiCondition.stamina; } }

    public event Action onTakeDamage;
    void Update()
    {
        stamina.Add(stamina.passiveValue * Time.deltaTime);
        health.Add(health.passiveValue * Time.deltaTime);

        if (health.curValue == 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("die");
    }

    public void TakePhysicalDamage(int damage)
    {
        health.Subtract(damage);
        onTakeDamage?.Invoke();
    }

    public bool UseStamina(float amount)
    {
        if (stamina.curValue - amount < 0f)
        {
            return false;
        }
        
        stamina.Subtract(amount);
        return true;
    }
}
