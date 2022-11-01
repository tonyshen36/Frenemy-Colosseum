using System.Collections;
using RPGCharacterAnims;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MobControl : EntityControl
{
    private MobCharacterInputSystemController characterInputSystemController;

    [Header("Mob Exclusive")]
    public int placeHolder = 0;

    public float minDistanceSquare = 80f;
    public float attackRangeSquare = 5f;
    public float attackDelay = 2f;

    private GameObject player;
    private float distance = 0f;
    private Vector2 movement;

    private bool attacked = false;
    private float timer;

    [Header("Test")]
    public bool sword = false;
    public bool dualSword = false;
    public bool axe = false;
    public bool spear = false;

    private bool isAiming = false;

    // Start is called before the first frame update
    void Start()
    {
        characterInputSystemController = GetComponent<MobCharacterInputSystemController>();
        rpgCharacterController = GetComponent<RPGCharacterController>();
        timer = attackDelay;
        m_transform = transform;
        base.isPlayer = false;
        base.BaseStart();
        //sceneControl.enemies.Add(this);
        if (sword) SwitchToSword();
        else if (dualSword) SwitchToDualSword();
        else if (axe) SwitchToAxe();
        else if (spear) SwitchToSpear();
        if (!PhotonNetwork.IsMasterClient)
        {
            //this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            player = GetClosestEnemy();
            if (player != null)
            {
                rpgCharacterController.target = player.transform;
                if (distance < minDistanceSquare && distance > attackRangeSquare)
                {
                    if (isAiming)
                    {
                        isAiming = false;
                        ToggleAim(isAiming, player);
                    }
                    Move();
                }
                else if (distance <= attackRangeSquare && !attacked)
                {
                    if (!isAiming)
                    {
                        isAiming = true;
                        ToggleAim(isAiming, player);
                    }
                    attacked = true;
                    Attack();
                }
                else
                {
                    if (isAiming)
                    {
                        isAiming = false;
                        ToggleAim(isAiming, player);
                    }
                    Stop();
                }
                if (attacked)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        timer = attackDelay;
                        attacked = false;
                    }
                }
            }
            else
            {
                if (isAiming)
                {
                    isAiming = false;
                    ToggleAim(isAiming, player);
                }
                Stop();
            }
        }
        base.BaseUpdate();
    }

    /*private IEnumerator Attack()
    {
        characterInputSystemController.inputAttackL = true;
        yield return null;
        characterInputSystemController.inputAttackL = false;
    }*/

    private void ToggleAim(bool aim, GameObject player)
    {
        characterInputSystemController.inputAim = aim;
        if (aim && player != null)
        {
            rpgCharacterController.SetAimInput(player.transform.position);
        }
    }

    private void Attack()
    {
        characterInputSystemController.inputAttackL = true;
        Invoke("StopAttack", 1f);
    }

    private void StopAttack()
    {
        characterInputSystemController.inputAttackL = false;
    }

    private void Move()
    {
        movement = new Vector2(player.transform.position.x - this.transform.position.x, player.transform.position.z - this.transform.position.z);
        characterInputSystemController.inputMovement = movement;
    }

    private void Stop()
    {
        movement = new Vector2(0, 0);
        characterInputSystemController.inputMovement = movement;
    }

    GameObject GetClosestEnemy()
    {
        GameObject bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        for (int i = 0; i < sceneControl.players.Count; i++)
        {
            if (sceneControl.players[i] == null) continue;
            if (sceneControl.players[i].GetComponent<CharacterControl>().IsDead()) continue;
            Vector3 directionToTarget = sceneControl.players[i].transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = sceneControl.players[i].gameObject;
                distance = dSqrToTarget;
            }
        }

        return bestTarget;
    }
}
