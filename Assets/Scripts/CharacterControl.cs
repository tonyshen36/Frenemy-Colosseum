using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;
using Photon.Pun;

public class CharacterControl : EntityControl
{
	[Header("Player Exclusive")]
	public List<string> upgrades = new List<string>();
	public float lockDistance = 200f;

	protected UnityEngine.InputSystem.PlayerInput playerInput;

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
	[HideInInspector] public int skinIndex = -1;

	// Start is called before the first frame update 
	void Start()
	{
		m_transform = transform;
		timer = attackDelay;
		base.BaseStart();
		if (isPlayer)
		{
			sceneControl.AddPlayer(this);
			playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
			if (PV.IsMine)
			{
				m_camera.gameObject.SetActive(true);
				m_camera.tag = "MainCamera";
			}
			else
			{
				m_camera.gameObject.SetActive(false);
				m_camera.tag = "Untagged";
			}
		}
		else
		{
			sceneControl.enemies.Add(this);
			if (sword) SwitchToSword();
			else if (dualSword) SwitchToDualSword();
			else if (axe) SwitchToAxe();
			else if (spear) SwitchToSpear();
		}

		if (isPlayer) instructionManager.firstChest = sceneControl.firstChest;

		if (!PV.IsMine)
		{
			SetHpBarColor(!isPlayer || hostile);
		}
		else
        {
			if (!isPlayer) SetHpBarColor(!isPlayer);
        }

	}

	public int strafingTargetIndex = -1;
	public bool isInjured = false;
	// Update is called once per frame
	void Update()
	{
        if (!isPlayer)
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
							ToggleAim();
						}
						Move();
					}
					else if (distance <= attackRangeSquare && !attacked)
					{
						attacked = true;
						Attack();
					}
					else if(distance <= attackRangeSquare && attacked)
					{
						Stop();
					}
					else
                    {
						/*if (isAiming)
						{
							isAiming = false;
							ToggleAim();
						}*/
						Stop();
					}
					if (attacked)
					{
						timer -= Time.deltaTime;
						if (timer <= attackDelay - (1.5/rpgCharacterController.animationSpeed))
                        {
							if (distance <= attackRangeSquare)
                            {
								if (!isAiming)
								{
									isAiming = true;
									ToggleAim();
								}
							}
						}
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
						ToggleAim();
					}
					Stop();
				}
			}
		}
		if (!rpgCharacterController.isStrafing)
        {
			strafingTargetIndex = -1;
        }
		base.BaseUpdate();
        if (isPlayer)
        {
			if (stamina > 0 && rpgCharacterController.isInjured)
			{
				if (rpgCharacterController.CanEndAction(HandlerTypes.Injure))
				{
					rpgCharacterController.EndAction(HandlerTypes.Injure);
					StartCoroutine(IdleFrame(.02f));
				}
			}

			if (stamina <= 0 && !rpgCharacterController.isInjured)
			{
				if (rpgCharacterController.CanStartAction(HandlerTypes.Injure))
				{
					rpgCharacterController.StartAction(HandlerTypes.Injure);
					rpgCharacterInputSystemController.CancelStrafing();
				}
			}
		}
	}

    private void FixedUpdate()
    {
		base.BaseFixedUpdate();
	}

    IEnumerator IdleFrame(float waitTime)
	{
		rpgCharacterController.idleThisFrame = true;
		yield return new WaitForSeconds(waitTime);
		rpgCharacterController.idleThisFrame = false;
	}


	public Outline GetAimTargetPosition()
	{
		Dictionary<float, GameObject> enemyPositions = new Dictionary<float, GameObject>();
		foreach (CharacterControl enemy in sceneControl.enemies)
		{
			if (enemy.IsDead()) continue;
			Vector3 directionToTarget = enemy.transform.position - m_transform.position;
			float dSqrToTarget = directionToTarget.sqrMagnitude;
			if (!enemy.IsDead() && dSqrToTarget <= lockDistance) enemyPositions.Add(Vector3.Distance(enemy.transform.position, m_transform.position), enemy.gameObject);
		}

		if (hostile)
		{
			foreach (CharacterControl player in sceneControl.players)
			{
				if (player.transform.position == m_transform.position || player.IsDead()) continue;
				Vector3 directionToTarget = player.transform.position - m_transform.position;
				float dSqrToTarget = directionToTarget.sqrMagnitude;
				if(dSqrToTarget <= lockDistance)
                {
					enemyPositions.Add(Vector3.Distance(player.transform.position, m_transform.position), player.gameObject);
                }
			}
		}

		if (enemyPositions.Count == 0)
        {
			rpgCharacterController.SetAimInput(new Vector3(-1, -1, -1));
			return null;
		}
		List<float> distant = new List<float>(enemyPositions.Keys);
		distant.Sort();

		if (strafingTargetIndex >= distant.Count) strafingTargetIndex = 0;
		rpgCharacterController.SetAimInput(enemyPositions[distant[0]].transform.position);

		return enemyPositions[distant[0]].GetComponent<Outline>();
	}
	public void GetClosestPlayer()
	{
		Dictionary<float, GameObject> playerPositions = new Dictionary<float, GameObject>();
		foreach (CharacterControl player in sceneControl.players)
		{
			if (player.IsDead()) continue;
			Vector3 directionToTarget = player.transform.position - m_transform.position;
			float dSqrToTarget = directionToTarget.sqrMagnitude;
			if (!player.IsDead() && dSqrToTarget <= lockDistance) playerPositions.Add(Vector3.Distance(player.transform.position, m_transform.position), player.gameObject);
		}

		if (playerPositions.Count == 0)
		{
			rpgCharacterController.SetAimInput(new Vector3(-1, -1, -1));
			return;
		}
		List<float> distant = new List<float>(playerPositions.Keys);
		distant.Sort();
		rpgCharacterController.SetAimInput(playerPositions[distant[0]].transform.position);
	}

	public void UpdateStats()
	{
		float healthMultiplier = 1;
		float speedMultiplier = 1;
		float damageMultiplier = 1;
		float newWeaponSize = baseWeaponSize;
		float newAttackSpeed = baseAttackSpeed;

		if (instructionManager.curInstructionState != InstructionManager.instructionState.None)
		{
			instructionManager.curInstructionState = InstructionManager.instructionState.PreSecondGate;
			instructionManager.displayNextInstruction = true;
			instructionManager.curInstructionNum = 7;
			if (instructionManager.instructionSet.Contains(instructionManager.curInstruction))
			{
				instructionManager.interactionControl.HideInstruction1();
			}
		}

		foreach (string upgrade in upgrades)
		{
			switch (upgrade)
			{
				case "Weapon Size Up":
					newWeaponSize += upgradeManager.upgrades[upgrade];
					break;
				case "Movement Speed Up":
					speedMultiplier *= upgradeManager.upgrades[upgrade];
					break;
				case "Health Up":
					healthMultiplier += upgradeManager.upgrades[upgrade];
					break;
				case "Weapon Damage Up":
					damageMultiplier *= upgradeManager.upgrades[upgrade];
					break;
				case "Attack Speed Up":
					newAttackSpeed += upgradeManager.upgrades[upgrade];
					break;
				case "Long Attack":
					specialAttack = 1;
					break;
				case "Heavy Attack":
					specialAttack = 2;
					break;
				default:
					print("No Matching Upgrade Found For " + upgrade);
					break;
			}
		}

		SetAttackFrames();
		UpdateHealthStats(healthMultiplier);
		UpdateSpeedStats(speedMultiplier);
		UpdateDamageStates(damageMultiplier);
		UpdateWeaponsSizeStates(newWeaponSize);
		UpdateAttackSpeedStates(newAttackSpeed);
	}

	public void RPC_Revive()
    {
		playerNetworking.RevivePlayer();
    }

	public void Revive()
    {
		SetCurHealth(maxHitpoints);
		dead = false;
		rpgCharacterController.TryEndAction(HandlerTypes.Death);
	}

	public void OnPVPAreaEnter(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
		sceneControl.pveStage = false;
		if (!PV.IsMine) return;

		List<string> swapped = new List<string>();

		CharacterControl friendCC = sceneControl.GetFriendObject().GetComponent<CharacterControl>();
		if (sceneControl.selectedAmbush.Contains("Final Battle: Swap Weapon"))
        {
			swapped.Add("weapon");
			if (PhotonNetwork.IsMasterClient)
            {
				print("Sweap Weapon");
				string my_weapon = "";
				string opponent_weapon = "";
				if (friendCC.curWeaponType == Weapon.TwoHandSword) my_weapon = "Sword";
				if (friendCC.curWeaponType == Weapon.TwoHandAxe) my_weapon = "Axe";
				if (friendCC.curWeaponType == Weapon.TwoHandSpear) my_weapon = "Spear";
				if (friendCC.curWeaponType == Weapon.LeftSword) my_weapon = "Daggers";
				if (curWeaponType == Weapon.TwoHandSword) opponent_weapon = "Sword";
				if (curWeaponType == Weapon.TwoHandAxe) opponent_weapon = "Axe";
				if (curWeaponType == Weapon.TwoHandSpear) opponent_weapon = "Spear";
				if (curWeaponType == Weapon.LeftSword) opponent_weapon = "Daggers";
				playerNetworking.UpdatePlayerWeapon(my_weapon, false);
				friendCC.playerNetworking.UpdatePlayerWeapon(opponent_weapon, false);
			}
		}
		if (sceneControl.selectedAmbush.Contains("Final Battle: Swap Upgrades"))
		{
			swapped.Add("upgrade");
			if (PhotonNetwork.IsMasterClient)
			{
				print("Sweap Upgrade");
				List<string> my_upgrades = friendCC.upgrades;
				List<string> opponent_upgrades = upgrades;

				playerNetworking.ReplacePlayerStates(my_upgrades);
				friendCC.playerNetworking.ReplacePlayerStates(opponent_upgrades);
			}
		}
		if (sceneControl.selectedAmbush.Contains("Final Battle: Swap Health"))
		{
			swapped.Add("health");
			if (PhotonNetwork.IsMasterClient)
			{
				print("Sweap Health");
				upgrades.Remove("Final Battle: Swap Health");
				float my_hitpoints = friendCC.hitpoints;
				float opponent_hitpoints = hitpoints;
				playerNetworking.SetHealth(my_hitpoints);
				friendCC.playerNetworking.SetHealth(opponent_hitpoints);
			}
		}
		if (activateNextMobControl)
		{
			for (int i = 0; i < sceneControl.mobStages.Length; ++i)
            {
				if (sceneControl.mobStages[i].name == nextMobControl)
                {
					sceneControl.mobStages[i].SetActive(true);
                }
            }
		}
		if (disableMobControl)
		{
			for (int i = 0; i < sceneControl.mobStages.Length; ++i)
			{
				if (sceneControl.mobStages[i].name == mobControl)
				{
					sceneControl.mobStages[i].SetActive(false);
				}
			}
		}
		instructionManager.interactionControl.DisplayPVP(swapped);
	}

	public void OnPVEAreaEnter(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
		sceneControl.pveStage = true;
		if (instructionManager.curInstructionState != InstructionManager.instructionState.None)
        {
			instructionManager.curInstructionState = InstructionManager.instructionState.PreSecondChest;
			if (instructionManager.curInstructionState == InstructionManager.instructionState.PreSecondGate)
				instructionManager.curInstructionState = InstructionManager.instructionState.None;
			if (instructionManager.instructionSet.Contains(instructionManager.curInstruction))
				instructionManager.interactionControl.HideInstruction1();
			instructionManager.curInstructionNum = 5;
			instructionManager.displayNextInstruction = true;
		}
		if (activateNextMobControl)
		{
			for (int i = 0; i < sceneControl.mobStages.Length; ++i)
			{
				if (sceneControl.mobStages[i].name == nextMobControl)
				{
					sceneControl.mobStages[i].SetActive(true);
				}
			}
		}
		if (disableMobControl)
		{
			for (int i = 0; i < sceneControl.mobStages.Length; ++i)
			{
				if (sceneControl.mobStages[i].name == mobControl)
				{
					sceneControl.mobStages[i].SetActive(false);
				}
			}
		}
	}

	public void RPC_OnPVPAreaEnter(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
		playerNetworking.OnEnterPVPArea(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
	}

	public void RPC_OnPVEAreaEnter(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
	{
		playerNetworking.OnEnterPVEArea(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
	}

	private bool protectionBroken = false;
	public void AddUpgrade(string s, bool doHUG = true)
	{

		Color textColor = hostile ? sceneControl.EnemyColor : sceneControl.FriendColor;

		if (s == "Final Battle: Swap Weapon" || s == "Final Battle: Swap Upgrades" || s == "Final Battle: Swap Health")
        {
			if (sceneControl.selectedAmbush.Contains(s)) sceneControl.selectedAmbush.Remove(s);
			else sceneControl.selectedAmbush.Add(s);
			upgradeManager.chosenAmbush.Add(s);

			if (doHUG)
			{
				if (hostile && !protectionBroken)
                {
					instructionManager.interactionControl.ShowInstruction2("? ? ?", textColor, -2);
				}
				else
                {
					instructionManager.interactionControl.ShowInstruction2("? ? ?", textColor);
				}
				
			}
		}
		else
        {
			upgrades.Add(s);
			if (doHUG)
			{
				if (hostile && !protectionBroken)
				{
					instructionManager.interactionControl.ShowInstruction2(s, textColor, -2);
				}
				else
				{
					instructionManager.interactionControl.ShowInstruction2(s, textColor);
				}
			}
		}

		if (hostile) protectionBroken = true;
    }

	public void RemoveRandomUpgrade()
    {
		if (PV.IsMine)
        {
			List<string> actualUpgrades = new List<string>();
			foreach (string upgrade in upgrades)
            {
				if (!upgrade.Contains("Final Battle")) actualUpgrades.Add(upgrade);
            }
			if (actualUpgrades.Count <= 0)
			{
				if (isPlayer) instructionManager.interactionControl.ShowInstruction2("No Upgrades To Lose", Color.black, 3.0f);
				return;
			}

			string s = actualUpgrades[Random.Range(0, actualUpgrades.Count - 1)];
			playerNetworking.UpdatePlayerStates(s, false);
			if (isPlayer) instructionManager.interactionControl.ShowInstruction2("Lost Upgrade: " + s, Color.black, 3.0f);
		}
	}

	public void RemoveUpgrade(string s)
    {
		if (upgrades.Contains(s))
		{
			upgrades.Remove(s);
			if (s == "Long Attack" || s == "Heavy Attack") specialAttack = 0;
		}
	}

	protected void UpdateHealthStats(float healthMultiplier)
	{
		SetMaxHealth( baseHitPoint * healthMultiplier);
	}

	private void UpdateSpeedStats(float speedMultiplier)
	{
		//Comment!
		movementController.runSpeed = runSpeed * speedMultiplier;
		movementController.runAccel = runAccel * speedMultiplier;
		movementController.walkSpeed = baseWalkSpeed * speedMultiplier;
		movementController.walkAccel = baseWalkAccel * speedMultiplier;
	}

	private void UpdateDamageStates(float damageMultiplier)
	{
		damage = baseDamage * damageMultiplier;
	}

	private void UpdateWeaponsSizeStates(float newWeaponSize)
	{
		rpgWeaponController.SetAllWeaponSize(Vector3.one * newWeaponSize);
	}

	private void UpdateAttackSpeedStates(float newAttackSpeed)
    {
		attackSpeed = newAttackSpeed;
		rpgCharacterController.attackAnimationSpeed = attackSpeed;
	}

	public void SetPosition(Vector3 pos)
    {
		playerNetworking.SetPlayerPosition(pos);
    }

	public void SetFriendlyFire(bool flag)
    {
		if (hostile) return;
		hostile = flag;
		if (PV.IsMine)
		{
			sceneControl.GetFriendObject().GetComponent<CharacterControl>().SetHpBarColor(hostile);
		}
        else
        {
			SetWeaponOutline(flag);
		}
	}

	public void SetWeaponOutline(bool hostileColor)
    {
		if (hostileColor)
		{
			var otherPlayer = sceneControl.GetFriendObject().GetComponent<CharacterControl>();
			if (otherPlayer.curWeaponTrigger.gameObject.TryGetComponent(out Outline outline))
			{
				outline.OutlineColor = new Color(0.9803922f, 0, 0, 1);
			}
			else
			{
				var outline1 = otherPlayer.curWeaponTrigger.gameObject.transform.parent.gameObject.GetComponent<Outline>();
				outline1.OutlineColor = new Color(0.9803922f, 0, 0, 1);
			}
			if (otherPlayer.curOffhandWeaponTrigger != null)
			{
				var outline2 = otherPlayer.curOffhandWeaponTrigger.gameObject.GetComponent<Outline>();
				outline2.OutlineColor = new Color(0.9803922f, 0, 0, 1);
			}
		}
	}

	public void SetHpBarColor(bool enemy)
    {
		hpIndicator.BarColor = enemy ? sceneControl.EnemyColor : sceneControl.FriendColor;
		if (enemy)
			hpIndicator.BarAlertColor = sceneControl.EnemyLowHPColor;
        else
        {
			if (PV.IsMine) hpIndicator.BarAlertColor = sceneControl.FriendLowHPColor;
			else hpIndicator.BarAlertColor = sceneControl.FriendColor;
		}
		
    }

	public void DisablePlayerInput()
    {
		playerInput.DeactivateInput();
    }

	public void EnablePlayerInput()
    {
		playerInput.ActivateInput();
    }

	private void ToggleAim()
	{
		rpgCharacterInputSystemController.inputAim = true;
		Invoke("StopInputAim", 1f);
	}

	private void StopInputAim()
	{
		rpgCharacterInputSystemController.inputAim = false;
	}

	private void Attack()
	{
		if (isAiming)
		{
			isAiming = false;
			ToggleAim();
		}
		rpgCharacterInputSystemController.inputAttackL = true;
		Invoke("StopAttack", 1f);
	}

	private void StopAttack()
	{
		rpgCharacterInputSystemController.inputAttackL = false;
	}

	private void Move()
	{
		movement = new Vector2(player.transform.position.x - this.transform.position.x, player.transform.position.z - this.transform.position.z);
		rpgCharacterInputSystemController.inputMovement = movement;
	}

	private void Stop()
	{
		movement = new Vector2(0, 0);
		rpgCharacterInputSystemController.inputMovement = movement;
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
