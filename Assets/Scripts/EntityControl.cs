using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EntityControl : MonoBehaviour
{
	protected RPGCharacterController rpgCharacterController = null;
	protected RPGCharacterWeaponController rpgWeaponController = null;
	protected RPGCharacterInputSystemController rpgCharacterInputSystemController = null;
	protected InstructionManager instructionManager = null;
	protected RPGCharacterMovementController movementController = null;
	[HideInInspector] public PlayerNetworkSync playerNetworking = null;
	public PhotonView PV = null;
	public bool isPlayer = false;
	public bool hostile = false;

	[HideInInspector] public Transform m_transform = null;
	[HideInInspector] public TempUpgradeManager upgradeManager;
	[HideInInspector] public SceneControl sceneControl;

	protected float damage = 1.0f;
	protected float hitpoints = 100;
	protected float maxHitpoints = 100;
	protected float attackSpeed = 1.0f;
	protected bool dead = false;

	[Header("Base Stats")]
	public float knockBackDamage = 25.0f;
	public float knockBackRecoveryTime = 3.0f;
	public float baseHitPoint = 100.0f;
	public float runSpeed = 1.0f;
	public float runAccel = 30.0f;
	public float baseDamage = 1.0f;
	public float baseWalkSpeed = 0.65f;
	public float baseWalkAccel = 21.0f;
	public float baseWeaponSize = 1.0f;
	public float baseAttackSpeed = 1.0f;
	public float staminaRegenerateTime = 2.0f;

	[Header("Attachments")]
	public Camera m_camera;
	public ProgressBar hpIndicator;
	public float HpBarOffset = 3.0f;
	[HideInInspector] public ProgressBarCircle staminaIndicator;

	//Weapon Info
	[HideInInspector] public Weapon curWeaponType = Weapon.Relax;
	[HideInInspector] public WeaponTrigger curWeaponTrigger = null;
	[HideInInspector] public WeaponTrigger curOffhandWeaponTrigger = null;
	public int specialAttack = 0;
	[HideInInspector] public int baseAttackAnimNum = -1;
	[HideInInspector] public int secondBaseAttackAnimNum = -1;
	public int specialAttackAnimNum = -1;
	public float baseAttackStartFrame = 0;
	public float baseAttackEndFrame = 0;
	public float secondBaseAttackStartFrame = 0;
	public float secondBaseAttackEndFrame = 0;
	public float specialAttackStartFrame = 0;
	public float specialAttackEndFrame = 0;
	//Hi?
	protected bool isAttacking = false;

	protected float knockBackTimer;
	protected float knockBackDamageCounter = 0;

	//Health
	protected Vector2 ogHPBarSize = new Vector2();

	//Stamina
	protected float stamina = 100f;
	protected float maxStamina = 100f;
	public bool isRegeneratingStamina = false;
	public float staminaTimer = 0;

	public Outline mobOutline;

	[Header("Audio Effects")]
	public AudioSource drawSword;
	public AudioSource drawDualSword;
	public AudioSource drawSpear;
	public AudioSource drawAxe;

	protected void BaseStart()
	{
		upgradeManager = (GameObject.FindObjectsOfType<TempUpgradeManager>())[0].GetComponent<TempUpgradeManager>();
		sceneControl = (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>();
		instructionManager = m_transform.GetComponent<InstructionManager>();
		PV = GetComponent<PhotonView>();
		rpgCharacterController = m_transform.GetComponent<RPGCharacterController>();
		rpgWeaponController = m_transform.GetComponent<RPGCharacterWeaponController>();
		rpgCharacterInputSystemController = m_transform.GetComponent<RPGCharacterInputSystemController>();
		movementController = m_transform.GetComponent<RPGCharacterMovementController>();
		playerNetworking = m_transform.GetComponent<PlayerNetworkSync>();
		baseAttackSpeed = rpgCharacterController.animationSpeed;
		staminaIndicator = hpIndicator.GetComponent<ProgressBarCircle>();

		hpIndicator.Bar1Value = 100;
		hpIndicator.transform.parent = GameObject.Find("Canvas").transform;
		ogHPBarSize = hpIndicator.GetComponent<RectTransform>().sizeDelta;
		SetMaxHealth(baseHitPoint);
		
		//InvokeRepeating(nameof(RegenerateStamina), 0f, 1f);
	}

	protected void BaseFixedUpdate()
    {
        if (!IsDead() || true)
        {
			Vector3 barPos = Camera.main.WorldToScreenPoint(m_transform.position + new Vector3(0, HpBarOffset, 0));
			if(hpIndicator!=null)
				hpIndicator.transform.position = barPos;
        }
	}

    protected void BaseUpdate()
    {
		if (!isRegeneratingStamina && stamina < maxStamina)
        {
			staminaTimer += Time.deltaTime;
			if (staminaTimer >= staminaRegenerateTime)
            {
				isRegeneratingStamina = true;
				staminaTimer = 0;
			}
		}

		RegenerateStamina();
		UpdateHealthBar();

		
		if (PV.IsMine && false)
        {
			if (Input.GetKeyDown(KeyCode.Minus))
			{
				playerNetworking.UpdatePlayerStates("Long Attack");
				print("set special to 1");
			}
			if (Input.GetKeyDown(KeyCode.Equals))
			{
				playerNetworking.UpdatePlayerStates("Heavy Attack");
				print("set special to 2");
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
            {
				playerNetworking.UpdatePlayerStates("Weapon Size Up");
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
            {
				playerNetworking.UpdatePlayerStates("Health Up");
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				playerNetworking.UpdatePlayerStates("Attack Speed Up");
			}
			if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				playerNetworking.UpdatePlayerStates("Movement Speed Up");
			}

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				playerNetworking.UpdatePlayerWeapon("Sword");
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				playerNetworking.UpdatePlayerWeapon("Axe");
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				playerNetworking.UpdatePlayerWeapon("Spear");
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				playerNetworking.UpdatePlayerWeapon("Daggers");
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				playerNetworking.UpdatePlayerStates("Final Battle: Swap Health");
			}
			if (Input.GetKeyDown(KeyCode.I))
            {
				SetCurHealth(GetMaxHealth() * 0.25f);
            }
			if (Input.GetKeyDown(KeyCode.O))
            {
				TryTakeDamage(GetMaxHealth() / 2.0f, false, true, Vector3.zero);
            }
			if (Input.GetKeyDown(KeyCode.L))
			{
				RotateTowardTarget(new Vector3(0, transform.position.y, 0));
			}
		}
	}

	public void RotateTowardTarget(Vector3 target)
    {
		movementController.SetRotateTowardsTarget(target);
    }

    public void TryTakeDamage(float i, bool kick, bool doTakeDamage, Vector3 damageSoursePosition)
	{
		//TakeDamage(i);
		//return;
		if (PV.IsMine)
        {
			i *= damage;
			i = hitpoints < i ? hitpoints : i;
			playerNetworking.TakeDamage(i, kick, doTakeDamage, damageSoursePosition);
		}
	}

	public void TakeDamage(float i, bool kick, bool doTakeDamage, Vector3 damageSoursePosition)
    {

		//reset recovery time;
		staminaTimer = 0;

		if (rpgCharacterController.isBlocking)
		{
			if (kick)
			{
				TryReduceStamina(30f);
				RotateTowardTarget(damageSoursePosition);
				rpgCharacterController.EndAction("Block");
				rpgCharacterInputSystemController.blockToggle = false;
				rpgCharacterInputSystemController.canBlock = false;
				rpgCharacterController.StartAction(HandlerTypes.Knockback, new HitContext((int)KnockbackType.Knockback1, Vector3.back));
				i *= 3;
			}
			else
			{
				TryReduceStamina(i /** (curWeaponType == Weapon.RightMace ? 0.6f : 0.8f)*/);
				if (GetCurStamina() > i)
					i = 0;
				else
					i -= GetCurStamina();
			}
			staminaIndicator.TakeStaminaDamageAnimation();
		}

		//Will not take damage from teammate if friendly damage is off
		float newHealth = GetCurHealth() - (doTakeDamage ? i : 0);

		if (newHealth <= 0)
		{
			rpgCharacterController.TryStartAction(HandlerTypes.Death);
			StartCoroutine(DieAgain());
			dead = true;
			if (isPlayer)
			{
                if(sceneControl.PlayersAlive() == 0){
					sceneControl.GameOver();
                }
			}
			if (!isPlayer)
            {
				Destroy(hpIndicator.gameObject);
                if (mobOutline.enabled)
                {
					mobOutline.enabled = false;
                }
				if (curWeaponTrigger.gameObject.TryGetComponent(out Outline outline1))
				{
					outline1.enabled = false;
				}
				else
				{
					var outline2 = curWeaponTrigger.gameObject.transform.parent.gameObject.GetComponent<Outline>();
					outline2.OutlineColor = new Color(0.9803922f, 0, 0, 1);
				}
				if (curOffhandWeaponTrigger != null)
				{
					var outline3 = curOffhandWeaponTrigger.gameObject.GetComponent<Outline>();
					outline3.OutlineColor = new Color(0.9803922f, 0, 0, 1);
				}
			}
			if (isPlayer && sceneControl.pveStage == false)
            {
				print("Enable Chest!");
				sceneControl.lastChest.SetActive(true);
            }
		}
        else
        {
			if (kick)
			{
				print("knock back");
				RotateTowardTarget(damageSoursePosition);
				rpgCharacterController.StartAction(HandlerTypes.Knockback, new HitContext((int)KnockbackType.Knockback1, Vector3.back));
			}
			else
			{
				print("get hit");
				HitContext hitContext = new HitContext();
				if (Vector3.Dot(damageSoursePosition - transform.position, transform.forward) > 0)
                {
					if (Random.value < 0.5)
						hitContext.number = (int)HitType.Forward1;
					else
						hitContext.number = (int)HitType.Forward2;
				}
				else
                {
					hitContext.number = (int)HitType.Back1;
                }
				rpgCharacterController.StartAction("GetHit", hitContext);
			}
		}

		if (i > 0 && doTakeDamage && !IsDead()) hpIndicator.TakeDamageAnimation();
		SetCurHealth(newHealth);
	}

	IEnumerator DieAgain()
    {
		yield return new WaitForSeconds(1.0f);
		rpgCharacterController.TryStartAction(HandlerTypes.Death);
		if (!rpgCharacterController.isDead && IsDead())
			StartCoroutine(DieAgain());
		
	}

	public void TryReduceStamina(float i, GameObject enemy = null, bool attacked = false)
	{
		if (PV.IsMine)
		{
			//EntityControl enemyScript = enemy.GetComponent<EntityControl>();
			if (isPlayer) playerNetworking.ReduceStamina(i, attacked);
			//ReduceStamina(i);
		}
	}

	//WIP
	public void ReduceStamina(float i, bool attacked = false)
	{
		if (stamina > 0)
		{
			stamina -= i;
			isRegeneratingStamina = false;
			staminaTimer = 0;
			if (stamina <= 0)
            {
				staminaIndicator.ThrowWarning();
			}
            if (attacked)
            {
				staminaIndicator.TakeStaminaDamageAnimation();
			}
		}
	}

	//Regenerate Stamina depends on boolean
	protected void RegenerateStamina()
	{
		if (isRegeneratingStamina)
		{
			stamina += 20f * Time.deltaTime;
			if (stamina >= 100)
			{
				stamina = 100;
				isRegeneratingStamina = false;
				staminaTimer = 0;
			}
		}
	}

	public void UpdateHealthBar()
	{
		hpIndicator.Bar1Value = hitpoints / maxHitpoints * 100.0f;
		if (isPlayer) staminaIndicator.SetBarValue(stamina / maxStamina * 100.0f);
	}

	public void DealDamage(WeaponTrigger m_weaponTrigger, float i, GameObject enemy)
	{
		/*if (!m_weaponTrigger.IsTargetDamagedByThisWeapon(enemy) && rpgCharacterController.isAttacking)
        {
			enemy.GetComponent<EntityControl>().TryTakeDamage(i, enemy);
			m_weaponTrigger.SetTargetDamagedByThisWeapon(enemy);
		}
		return;*/
		//if (isPlayer && !PV.IsMine) return;
		if (enemy.CompareTag("Enemy") && m_transform.CompareTag("Enemy")) return;
		//if (!sceneControl.friendlyFire && enemy.CompareTag("Player") && m_transform.CompareTag("Player")) return;
		if (!m_weaponTrigger.IsTargetDamagedByThisWeapon(enemy) && rpgCharacterController.isAttacking)
		{
			EntityControl enemyScript = enemy.GetComponent<EntityControl>();
			bool doTakeDamage = !isPlayer || !enemyScript.isPlayer || hostile;
			if (!enemyScript.rpgCharacterController.isRolling)
			{
				enemyScript.TryTakeDamage(i, m_weaponTrigger.isKick, doTakeDamage, transform.position);
				enemyScript.TryReduceStamina(m_weaponTrigger.GetEnergyDamage());

				m_weaponTrigger.SetTargetDamagedByThisWeapon(enemy);
			}
		}
	}

	public void PlayActivateAnim()
    {
		rpgCharacterController.StartAction(HandlerTypes.EmoteCombat, EmoteType.Activate);
	}

	public void EndActivateAnim()
	{
		rpgCharacterController.EndAction(HandlerTypes.EmoteCombat);
	}

	[PunRPC]
	public void SwitchToSword(bool doHUD = true)
	{
		print("Sword!");
		//if(isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Two Hand Sword", Color.black);
		if(isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Friendly Damage Protection", sceneControl.FriendColor, 4);
		var context = new SwitchWeaponContext();

		context.type = "Switch";
		context.side = "None";
		context.leftWeapon = Weapon.Relax;
		context.rightWeapon = Weapon.TwoHandSword;

		StartCoroutine(WaitAndPlayAudio(0.3f, drawSword));
		m_transform.GetComponent<RPGCharacterController>().TryStartAction(HandlerTypes.SwitchWeapon, context);

		curWeaponType = Weapon.TwoHandSword;
		curWeaponTrigger = rpgWeaponController.GetWeaponTrigger(curWeaponType);
		if(!PV.IsMine && isPlayer)
        {
			var outline = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.2225347f, 0.7382407f, 0.8867924f, 1);
        }
        else if(!isPlayer)
        {
			var outline = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.9803922f, 0, 0, 1);
		}
		curOffhandWeaponTrigger = null;
	}

	[PunRPC]
	public void SwitchToAxe(bool doHUD = true)
	{
		print("Axe!");
		//if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Two Hand Axe", Color.black);
		if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Friendly Damage Protection", sceneControl.FriendColor, 4);
		var context = new SwitchWeaponContext();

		context.type = "Switch";
		context.side = "None";
		context.leftWeapon = Weapon.Relax;
		context.rightWeapon = Weapon.TwoHandAxe;

		StartCoroutine(WaitAndPlayAudio(0.3f, drawAxe));
		m_transform.GetComponent<RPGCharacterController>().TryStartAction(HandlerTypes.SwitchWeapon, context);

		curWeaponType = Weapon.TwoHandAxe;
		curWeaponTrigger = rpgWeaponController.GetWeaponTrigger(curWeaponType);
		if (!PV.IsMine && isPlayer)
        {
			var outline = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.2225347f, 0.7382407f, 0.8867924f, 1);
		}
		else if (!isPlayer)
		{
			var outline = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.9803922f, 0, 0, 1);
		}
		curOffhandWeaponTrigger = null;
	}

	[PunRPC]
	public void SwitchToSpear(bool doHUD = true)
	{
		print("Spear!");
		//if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Two Hand Spear", Color.black);
		if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Friendly Damage Protection", sceneControl.FriendColor, 4);
		var context = new SwitchWeaponContext();

		context.type = "Switch";
		context.side = "None";
		context.leftWeapon = Weapon.Relax;
		context.rightWeapon = Weapon.TwoHandSpear;

		StartCoroutine(WaitAndPlayAudio(0.3f, drawSpear));
		m_transform.GetComponent<RPGCharacterController>().TryStartAction(HandlerTypes.SwitchWeapon, context);

		curWeaponType = Weapon.TwoHandSpear;
		curWeaponTrigger = rpgWeaponController.GetWeaponTrigger(curWeaponType);
		var outline = curWeaponTrigger.gameObject.transform.parent.gameObject.AddComponent<Outline>();
		if (!PV.IsMine && isPlayer)
		{
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.2225347f, 0.7382407f, 0.8867924f, 1);
		}
		else if (!isPlayer)
		{
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.9803922f, 0, 0, 1);
		}
		curOffhandWeaponTrigger = null;
	}

	[PunRPC]
	public void SwitchToDualSword(bool doHUD = true)
	{
		print("Dual Sword!");
		//if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Daggers", Color.black);
		if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Friendly Damage Protection", sceneControl.FriendColor, 4);
		var context = new SwitchWeaponContext();

		context.type = "Switch";
		context.side = "Dual";
		context.rightWeapon = Weapon.RightSword;
		context.leftWeapon = Weapon.LeftSword;

		StartCoroutine(WaitAndPlayAudio(0.3f, drawDualSword));
		m_transform.GetComponent<RPGCharacterController>().TryStartAction(HandlerTypes.SwitchWeapon, context);

		curWeaponType = Weapon.LeftSword;
		curWeaponTrigger = rpgWeaponController.GetWeaponTrigger(Weapon.LeftSword);
		curOffhandWeaponTrigger = rpgWeaponController.GetWeaponTrigger(Weapon.RightSword);
		if (!PV.IsMine && isPlayer)
		{
			var outline1 = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline1.OutlineMode = Outline.Mode.OutlineVisible;
			outline1.OutlineWidth = 10f;
			outline1.OutlineColor = new Color(0.2225347f, 0.7382407f, 0.8867924f, 1);
			var outline2 = curOffhandWeaponTrigger.gameObject.AddComponent<Outline>();
			outline2.OutlineMode = Outline.Mode.OutlineVisible;
			outline2.OutlineWidth = 10f;
			outline2.OutlineColor = new Color(0.2225347f, 0.7382407f, 0.8867924f, 1);
		}
		else if (!isPlayer)
		{
			var outline = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.9803922f, 0, 0, 1);
			var outline2 = curOffhandWeaponTrigger.gameObject.AddComponent<Outline>();
			outline2.OutlineMode = Outline.Mode.OutlineVisible;
			outline2.OutlineWidth = 10f;
			outline2.OutlineColor = new Color(0.9803922f, 0, 0, 1);
		}
	}

	[PunRPC]
	public void SwitchToMaceShield(bool doHUD = true)
	{
		print("Mace Shield!");
		//if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Shield N Mace", Color.black);
		if (isPlayer && doHUD) instructionManager.interactionControl.ShowInstruction2("Friendly Damage Protection", sceneControl.FriendColor, 4);
		var context = new SwitchWeaponContext();
		
		context.type = "Switch";
		context.side = "Right";
		context.rightWeapon = Weapon.RightMace;
		context.leftWeapon = Weapon.Relax;

		m_transform.GetComponent<RPGCharacterController>().TryStartAction(HandlerTypes.SwitchWeapon, context);
		StartCoroutine(EquipShield(0.8f));

		curWeaponType = Weapon.RightMace;
		curWeaponTrigger = rpgWeaponController.GetWeaponTrigger(curWeaponType);
		curOffhandWeaponTrigger = rpgWeaponController.GetWeaponTrigger(Weapon.Shield);
		if (!PV.IsMine && isPlayer)
		{
			var outline1 = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline1.OutlineMode = Outline.Mode.OutlineVisible;
			outline1.OutlineWidth = 10f;
			outline1.OutlineColor = new Color(0.2225347f, 0.7382407f, 0.8867924f, 1);
			var outline2 = curOffhandWeaponTrigger.gameObject.AddComponent<Outline>();
			outline2.OutlineMode = Outline.Mode.OutlineVisible;
			outline2.OutlineWidth = 10f;
			outline2.OutlineColor = new Color(0.2225347f, 0.7382407f, 0.8867924f, 1);
		}
		else if (!isPlayer)
		{
			var outline = curWeaponTrigger.gameObject.AddComponent<Outline>();
			outline.OutlineMode = Outline.Mode.OutlineVisible;
			outline.OutlineWidth = 10f;
			outline.OutlineColor = new Color(0.9803922f, 0, 0, 1);
			var outline2 = curOffhandWeaponTrigger.gameObject.AddComponent<Outline>();
			outline2.OutlineMode = Outline.Mode.OutlineVisible;
			outline2.OutlineWidth = 10f;
			outline2.OutlineColor = new Color(0.9803922f, 0, 0, 1);
		}
	}

	private IEnumerator WaitAndPlayAudio(float waitTime, AudioSource drawEffect)
	{
		yield return new WaitForSeconds(waitTime);
		drawEffect.Play();
	}

	private IEnumerator EquipShield(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		var context = new SwitchWeaponContext();
		context.type = "Switch";
		context.side = "Left";
		context.rightWeapon = Weapon.Relax;
		context.leftWeapon = Weapon.Shield;
		StartCoroutine(WaitAndPlayAudio(0.3f, drawDualSword));
		m_transform.GetComponent<RPGCharacterController>().StartAction(HandlerTypes.SwitchWeapon, context);
	}

	public float GetDamage() { return damage; }
	public float GetCurHealth() {return hitpoints;}
	public float GetMaxHealth() { return maxHitpoints; }
	public bool IsDead() { return dead; }
	public void SetDamage(float f) { damage = f; }
	public void _SetCurHealth(float f) {
		//print("new health is: " + f);
		if (f > maxHitpoints) f = maxHitpoints;
		if (f < 0) f = 0;
		hitpoints = f;
		UpdateHealthBar();
	}

	public void SetCurHealth(float f)
	{
		playerNetworking.SetHealth(f);
	}

	public void SetCurHealthLocally(float f)
	{
		_SetCurHealth(f);
	}

	public void SetMaxHealth(float f) {

		float prevMaxHitpoints = maxHitpoints;
		maxHitpoints = f;
		float rectSizeRatio = maxHitpoints / 100.0f;
		//print("The sive ratio is: " + rectSizeRatio);

		if (prevMaxHitpoints == f) return;

		float tempHP = f - (prevMaxHitpoints - GetCurHealth());
		if (prevMaxHitpoints > f) tempHP = GetCurHealth();

		tempHP = Mathf.Min(maxHitpoints, tempHP);
		tempHP = Mathf.Max(0, tempHP);
		SetCurHealthLocally(tempHP);

		hpIndicator.barRT.transform.localScale = new Vector3(hpIndicator.ogBarSize.x * rectSizeRatio, hpIndicator.ogBarSize.y * (rectSizeRatio + 1) / 2.0f, hpIndicator.ogBarSize.z);
		hpIndicator.barBackgroundRT.localScale = new Vector3(hpIndicator.ogBarBackgroundSize.x * rectSizeRatio, hpIndicator.ogBarBackgroundSize.y * (rectSizeRatio + 1) / 2.0f, hpIndicator.ogBarBackgroundSize.z);
	}
	public float GetCurStamina() { return stamina; }
	public void SetStamina(float f) { stamina = f; }
	public float GetMaxStamina() { return maxStamina; }
	public int GetAttackNumber(Weapon weapon, int attackType)
	{
		if (weapon == Weapon.TwoHandAxe)
		{
			if (attackType == 0) return 1;
			if (attackType == 1) return 4;
			if (attackType == 2) return 5;
		}
		else if (weapon == Weapon.TwoHandSword)
		{
			if (attackType == 0) return 5;
			if (attackType == 1) return 6;
			if (attackType == 2) return 11;
		}
		else if (weapon == Weapon.TwoHandSpear)
		{
			if (attackType == 0) return 9;
			if (attackType == 1) return 5;
			if (attackType == 2) return 10;
		}
		else if (weapon == Weapon.LeftSword)
		{
			if (attackType == 0) return 4;
			if (attackType == 1) return 1;
			if (attackType == 2) return 2;
		}
		else if (weapon == Weapon.RightSword)
		{
			if (attackType == 0) return 11;
			if (attackType == 1) return 1;
			if (attackType == 2) return 2;
		}
		else if (weapon == Weapon.Shield)
		{
			if (attackType == 0) return 1;
			if (attackType == 1) return 1;
			if (attackType == 2) return 1;
		}
		else if (weapon == Weapon.RightMace)
		{
			if (attackType == 0) return 6;
			if (attackType == 1) return 1;
			if (attackType == 2) return 1;
		}

		return -1;
	}

	public void SetAttackFrames()
    {
		specialAttackAnimNum = GetAttackNumber(curWeaponType, specialAttack);
		if (curWeaponType == Weapon.TwoHandAxe)
		{
			baseAttackStartFrame = 0.72f;
			baseAttackEndFrame = 0.87f;
			specialAttackStartFrame = specialAttack == 1 ? 0.62f : 0.78f;
			specialAttackEndFrame = specialAttack == 1 ? 0.96f : 0.93f;
		}
		else if (curWeaponType == Weapon.TwoHandSword)
		{
			baseAttackStartFrame = 0.45f;
			baseAttackEndFrame = 0.55f;
			specialAttackStartFrame = specialAttack == 1 ? 0.48f : 0.69f;
			specialAttackEndFrame = specialAttack == 1 ? 0.88f : 0.8f;
		}
		else if (curWeaponType == Weapon.TwoHandSpear)
		{
			baseAttackStartFrame = 0.35f;
			baseAttackEndFrame = 0.85f;
			specialAttackStartFrame = specialAttack == 1 ? 0.54f : 0.55f;
			specialAttackEndFrame = specialAttack == 1 ? 0.7f : 0.7f;
		}
		else if (curWeaponType == Weapon.RightSword)
		{
			baseAttackStartFrame = 0.29f;
			baseAttackEndFrame = 0.37f;
			secondBaseAttackStartFrame = 0.21f;
			secondBaseAttackEndFrame = 0.29f;
			specialAttackStartFrame = specialAttack == 1 ? 0.23f : 0.37f;
			specialAttackEndFrame = specialAttack == 1 ? 0.68f : 0.65f;
		}
		else if (curWeaponType == Weapon.RightMace)
		{
			baseAttackStartFrame = 0.26f;
			baseAttackEndFrame = 0.35f;
			specialAttackStartFrame = specialAttack == 1 ? 0.28f : 0;
			specialAttackEndFrame = specialAttack == 1 ? 0.5f : 0;
		}

		if (specialAttack == 0)
        {
			specialAttackStartFrame = baseAttackStartFrame;
			specialAttackEndFrame = baseAttackEndFrame;

		}
	}

    private void OnDisable()
    {
		if(hpIndicator != null)
        {
			Destroy(hpIndicator.gameObject);
		}
    }
}