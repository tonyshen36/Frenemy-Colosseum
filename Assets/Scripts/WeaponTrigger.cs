using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

public class WeaponTrigger : MonoBehaviour
{
    public EntityControl entityControl;
    private List<GameObject> targetsDamagedThisAttack = new List<GameObject>();
    public bool triggerEnabled = false;
    public Side side = Side.None;
    public bool dualSwing = false;
    public bool isKick = false;

    [Header("Base Attack")]
    public int baseAttackAnim = -1;
    public float baseAttackDamage = 10.0f;
    public float baseAttackEnergyCost = 10.0f;
    public float baseAttackStart = 0;
    public float baseAttackEnd = 1.5f;

    [Header("Special Attack 1")]
    public int specialAttack1Anim = -1;
    public float specialAttack1Damage = 10.0f;
    public float specialAttack1EnergyCost = 10.0f;
    public float specialAttack1Start = 0;
    public float specialAttack1End = 1.5f;

    [Header("Special Attack 2")]
    public int specialAttack2Anim = -1;
    public float specialAttack2Damage = 10.0f;
    public float specialAttack2EnergyCost = 10.0f;
    public float energyDamage = 10.0f;
    public float specialAttack2Start = 0;
    public float specialAttack2End = 1.5f;
    public bool knockBack = false;
    protected Side ogSide = Side.None;


    private void Start()
    {
        ogSide = side;
    }

    public enum attackType
    {
        baseAttack = 0,
        specialAttack1 = 1,
        specialAttack2 = 2,
    }

    attackType curSpecialAttack = attackType.baseAttack;
    attackType curAttackType = attackType.baseAttack;

    public void SetAttackType(attackType _attackType)
    {
        curAttackType = _attackType;
        if (dualSwing && _attackType != attackType.baseAttack) side = Side.Dual;
        else side = ogSide;
    }

    public int GetAnimNumber()
    {
        switch (curAttackType)
        {
            case attackType.baseAttack: return baseAttackAnim;
            case attackType.specialAttack1: return specialAttack1Anim;
            case attackType.specialAttack2: return specialAttack2Anim;
            default: return baseAttackAnim;
        }
    }

    public float GetStartFrame()
    {
        switch (curAttackType)
        {
            case attackType.baseAttack: return baseAttackStart;
            case attackType.specialAttack1: return specialAttack1Start;
            case attackType.specialAttack2: return specialAttack2Start;
            default: return baseAttackStart;
        }
    }

    public float GetEndFrame()
    {
        switch (curAttackType)
        {
            case attackType.baseAttack: return baseAttackEnd;
            case attackType.specialAttack1: return specialAttack1End;
            case attackType.specialAttack2: return specialAttack2End;
            default: return baseAttackEnd;
        }
    }

    public float GetDamage()
    {
        switch (curAttackType)
        {
            case attackType.baseAttack: return baseAttackDamage;
            case attackType.specialAttack1: return specialAttack1Damage;
            case attackType.specialAttack2: return specialAttack2Damage;
            default: return baseAttackDamage;
        }
    }

    public float GetEnergyCost()
    {
        switch (curAttackType)
        {
            case attackType.baseAttack: return baseAttackEnergyCost;
            case attackType.specialAttack1: return specialAttack1EnergyCost;
            case attackType.specialAttack2: return specialAttack2EnergyCost;
            default: return baseAttackEnergyCost;
        }
    }

    public float GetEnergyDamage()
    {
        if (curAttackType != attackType.specialAttack2) return 0;
        return energyDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy") || other.CompareTag("PlayerPart"))
        {
            if (!triggerEnabled) return;
            entityControl.DealDamage(this, GetDamage(), other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy") || other.CompareTag("PlayerPart"))
        {
            if (!triggerEnabled) return;
            entityControl.DealDamage(this, GetDamage(), other.gameObject);
        }
    }

    public void SetTargetDamagedByThisWeapon(GameObject target)
    {
        targetsDamagedThisAttack.Add(target);
    }

    public bool IsTargetDamagedByThisWeapon(GameObject target)
    {
        return targetsDamagedThisAttack.Contains(target);
    }

    public void ClearTargetDamagedThisAttack()
    {
        targetsDamagedThisAttack.Clear();
    }
}
