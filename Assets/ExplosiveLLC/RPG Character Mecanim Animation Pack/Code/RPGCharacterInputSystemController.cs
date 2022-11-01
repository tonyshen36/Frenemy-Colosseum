// To switch your project to using the new InputSystem:
// Edit>Project Settings>Player>Active Input Handling change to "Input System Package (New)".

using UnityEngine;
using UnityEngine.InputSystem;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

namespace RPGCharacterAnims
{
	[HelpURL("https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html")]

	public class RPGCharacterInputSystemController : MonoBehaviour
    {
        RPGCharacterController rpgCharacterController;
		RPGCharacterWeaponController rpgCharacterWeaponController;
		InstructionManager instructionManager;
		CharacterControl characterControl;
		PlayerInfo PI;

		//InputSystem
		//public @RPGInputs rpgInputs;
		public AudioSource audioSource;
		public AudioClip twoHandedSwordClip;
		public AudioClip twoHandedAxeClip;
		public AudioClip twoHandedSpearClip;
		public AudioClip dualSwordClip;

		// Inputs.
		public bool inputJump;
		public bool inputLightHit;
		public bool inputDeath;
		public bool inputAttackL;
		public bool inputAttackR;
		public bool inputCastL;
		public bool inputCastR;
		public bool inputBlock;
		public bool inputRoll;
		public bool inputShield;
		public bool inputRelax;
		public bool inputAim;
		public bool inputKick;
		public bool inputInteract;
		public Vector2 inputMovement;
		public bool inputFace;
		public Vector2 inputFacing;
		public bool inputSwitchUp;
		public bool inputSwitchDown;
		public bool inputSwitchLeft;
		public bool inputSwitchRight;

		// Variables.
		private Vector3 moveInput;
		private Vector3 currentAim;
		private float bowPull;
		[HideInInspector] public bool blockToggle;
		private float inputPauseTimeout = 0;
		private bool inputPaused = false;
		private Animator animator;

		private PhotonView PV;
		private Outline outlineTarget;
		public bool isPlayer = false;
		private void Awake()
        {
            rpgCharacterController = GetComponent<RPGCharacterController>();
			characterControl = GetComponent<CharacterControl>();
			rpgCharacterWeaponController = GetComponent<RPGCharacterWeaponController>();
			instructionManager = GetComponent<InstructionManager>();
			PI = GetComponent<PlayerInfo>();
			PV = GetComponent<PhotonView>();
			// Find the Animator component.
			animator = GetComponentInChildren<Animator>();
			//rpgInputs = new @RPGInputs();
			currentAim = Vector3.zero;
        }

		private void OnEnable()
		{ 
			//rpgInputs.Enable();
		}

		private void OnDisable()
		{ 
			//rpgInputs.Disable();
		}

		public bool HasMoveInput() => moveInput.magnitude > 0.1f;

		public bool HasAimInput() => inputAim;

		public bool HasFacingInput() => inputFacing != Vector2.zero || inputFace;

		public bool HasBlockInput() => inputBlock;

		public bool HasInteractInput() => inputInteract;

		public bool HasKickInput() => inputKick;

		
		private void Update()
		{
			// Pause input for other external input.
			/*if (inputPaused) {
				if (Time.time > inputPauseTimeout) { inputPaused = false; }
				else { return; }
			}*/

			//if (!inputPaused) { Inputs(); }
			
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) GetInputFromPlayerInfo();
			else UpdatePlayerInfoInput();

			if (inputBlock == false) canBlock = true;

			Blocking();
			Moving();
			Jumping();
			Damage();
			SwitchWeapons();
			Interact();
			if (!rpgCharacterController.IsActive("Relax")) 
			{
				Strafing();
				Facing();
				Aiming();
				Rolling();
				Attacking();
			}
		}

        /// <summary>
        /// Pause input for a number of seconds.
        /// </summary>
        /// <param name="timeout">The amount of time in seconds to ignore input.</param>

        private void GetInputFromPlayerInfo()
        {
			inputAttackL = PI.GetPlayerInput("inputAttackL");
			inputAttackR = PI.GetPlayerInput("inputAttackR");
			inputRoll = PI.GetPlayerInput("inputRoll");
			inputBlock = PI.GetPlayerInput("inputBlock");
			inputFace = PI.GetPlayerInput("inputFace");
			inputAim = PI.GetPlayerInput("inputAim");
			inputKick = PI.GetPlayerInput("inputKick");
			inputInteract = PI.GetPlayerInput("inputInteract");
			doStrafe = PI.GetPlayerInput("doStrafe");
			inputMovement = PI.GetPlayerInputDirection();
			
		}

		private void UpdatePlayerInfoInput()
        {
			PI.UpdatePlayerInputs("inputAttackL", inputAttackL);
			PI.UpdatePlayerInputs("inputAttackR", inputAttackR);
			PI.UpdatePlayerInputs("inputRoll", inputRoll);
			PI.UpdatePlayerInputs("inputBlock", inputBlock);
			PI.UpdatePlayerInputs("inputFace", inputFace);
			PI.UpdatePlayerInputs("inputAim", inputAim);
			PI.UpdatePlayerInputs("inputKick", inputKick);
			PI.UpdatePlayerInputs("inputInteract", inputInteract);
			PI.UpdatePlayerInputs("doStrafe", doStrafe);
			PI.UpdatePlayerInputs(inputMovement);
        }

		public void PauseInput(float timeout)
		{
			inputPaused = true;
			inputPauseTimeout = Time.time + timeout;
		}

		public void OnAttackL(InputValue value)
        {
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputAttackL = value.isPressed;
		}

		public void OnAttackR(InputValue value)
		{
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputAttackR = value.isPressed;
		}

		public void OnBlock(InputValue value)
		{
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputBlock = value.isPressed;
		}

		public void OnInteract(InputValue value)
        {
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputInteract = value.isPressed;
		}

		/*public void OnCastL(InputValue value)
		{
			inputCastL = value.isPressed;
		}*/

		/*public void OnCastR(InputValue value)
		{
			inputCastR = value.isPressed;
		}*/

		/*public void OnDeath(InputValue value)
		{
			inputDeath = value.isPressed;
		}*/

		public void OnFace(InputValue value)
		{
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputFace = value.isPressed;
		}

		/*public void OnFacing(InputValue value)
		{
			inputFacing = value.Get<Vector2>();
		}*/

		/*public void OnJump(InputValue value)
		{
			inputJump = value.isPressed;
		}*/

		/*public void OnLightHit(InputValue value)
		{
			inputLightHit = value.isPressed;
		}*/

		public void OnMove(InputValue value)
		{
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputMovement = value.Get<Vector2>();
		}

		/*public void OnRelax(InputValue value)
		{
			inputRelax = value.isPressed;
		}*/

		public void OnRoll(InputValue value)
		{
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputRoll = value.isPressed;
		}

		/*public void OnShield(InputValue value)
		{
			inputShield = value.isPressed;
		}*/

		public void OnAim(InputValue value)
		{
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return; // Will need some work here, but not now
			inputAim = value.isPressed;
		}

		public void OnKick(InputValue value)
        {
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			inputKick = value.isPressed;
		}

		/*public void OnWeaponDown(InputValue value)
		{
			inputSwitchDown = value.isPressed;
		}*/

		/*public void OnWeaponLeft(InputValue value)
		{
			inputSwitchLeft = value.isPressed;
		}*/

		/*public void OnWeaponRight(InputValue value)
		{
			inputSwitchRight = value.isPressed;
		}*/

		/*public void OnWeaponUp(InputValue value)
		{
			inputSwitchUp = value.isPressed;
		}*/

		/// <summary>
		/// Input abstraction for easier asset updates using outside control schemes.
		/// </summary>
		/*private void Inputs()
        {
            try {
				inputAttackL = rpgInputs.RPGCharacter.AttackL.WasPressedThisFrame();
				inputAttackR = rpgInputs.RPGCharacter.AttackR.WasPressedThisFrame();
				inputBlock = rpgInputs.RPGCharacter.Block.IsPressed();
				inputCastL = rpgInputs.RPGCharacter.CastL.WasPressedThisFrame();
				inputCastR = rpgInputs.RPGCharacter.CastR.WasPressedThisFrame();
				inputDeath = rpgInputs.RPGCharacter.Death.WasPressedThisFrame();
				inputFace = rpgInputs.RPGCharacter.Face.IsPressed();
				inputFacing = rpgInputs.RPGCharacter.Facing.ReadValue<Vector2>();
				inputJump = rpgInputs.RPGCharacter.Jump.IsPressed();
				inputLightHit = rpgInputs.RPGCharacter.LightHit.WasPressedThisFrame();
				inputMovement = rpgInputs.RPGCharacter.Move.ReadValue<Vector2>();
				inputRelax = rpgInputs.RPGCharacter.Relax.WasPressedThisFrame();
				inputRoll = rpgInputs.RPGCharacter.Roll.WasPressedThisFrame();
				inputShield = rpgInputs.RPGCharacter.Shield.WasPressedThisFrame();
				inputAim = rpgInputs.RPGCharacter.Aim.IsPressed();
				inputSwitchDown = rpgInputs.RPGCharacter.WeaponDown.WasPressedThisFrame();
				inputSwitchLeft = rpgInputs.RPGCharacter.WeaponLeft.WasPressedThisFrame();
				inputSwitchRight = rpgInputs.RPGCharacter.WeaponRight.WasPressedThisFrame();
				inputSwitchUp = rpgInputs.RPGCharacter.WeaponUp.WasPressedThisFrame();

				// Injury toggle.
				if (Keyboard.current.iKey.wasPressedThisFrame) {
                    if (rpgCharacterController.CanStartAction("Injure"))
					{ rpgCharacterController.StartAction("Injure"); }
					else if (rpgCharacterController.CanEndAction("Injure"))
					{ rpgCharacterController.EndAction("Injure"); }
                }
                // Headlook toggle.
                if (Keyboard.current.lKey.wasPressedThisFrame)
				{ rpgCharacterController.ToggleHeadlook(); }

                // Slow time toggle.
                if (Keyboard.current.tKey.wasPressedThisFrame) {
                    if (rpgCharacterController.CanStartAction("SlowTime"))
					{ rpgCharacterController.StartAction("SlowTime", 0.125f); }
					else if (rpgCharacterController.CanEndAction("SlowTime"))
					{ rpgCharacterController.EndAction("SlowTime"); }
                }
                // Pause toggle.
                if (Keyboard.current.pKey.wasPressedThisFrame) {
                    if (rpgCharacterController.CanStartAction("SlowTime"))
					{ rpgCharacterController.StartAction("SlowTime", 0f); }
					else if (rpgCharacterController.CanEndAction("SlowTime"))
					{ rpgCharacterController.EndAction("SlowTime"); }
                }
            }
			catch (System.Exception) { Debug.LogError("Inputs not found!  Character must have Player Input component."); }
        }*/
		bool interacting = false;
		
		public void Interact()
        {
			if ((isPlayer && !PV.IsMine) || (!isPlayer && !PhotonNetwork.IsMasterClient)) return;
			if (inputInteract && !interacting && !rpgCharacterController.isAttacking && !rpgCharacterController.isKnockback && !rpgCharacterController.isKnockdown && !rpgCharacterController.isSpecial && !rpgCharacterController.isRolling && !rpgCharacterController.isDead)
            {
				inputInteract = false;
				instructionManager.DoInteract();
			}
        }

		/*
		private IEnumerator WaitToStopCasting(float time)
		{
			yield return new WaitForSeconds(time);
			print("Stoped");
			interacting = false;
			rpgCharacterController.EndAction(HandlerTypes.Cast);
		}
		*/
		public bool canBlock = true;
		public void Blocking()
        {
            bool blocking = HasBlockInput();
            if (blocking && rpgCharacterController.CanStartAction("Block") && canBlock && !rpgCharacterController.isKnockback && !rpgCharacterController.isKnockdown && !rpgCharacterController.isSpecial && !rpgCharacterController.isRolling && !rpgCharacterController.isDead && characterControl.GetCurStamina() > 0) {
                rpgCharacterController.StartAction("Block");
				characterControl.staminaTimer = 0;
				blockToggle = true;
            }
			else if (((!blocking && blockToggle) || characterControl.GetCurStamina() <= 0) && rpgCharacterController.CanEndAction("Block")) {
                rpgCharacterController.EndAction("Block");
				blockToggle = false;
            }
        }

        public void Moving()
		{
			moveInput = new Vector3(inputMovement.x, inputMovement.y, 0f);

			// Filter the 0.1 threshold of HasMoveInput.
			if (HasMoveInput() && !rpgCharacterController.idleThisFrame) { rpgCharacterController.SetMoveInput(moveInput); }
			else { rpgCharacterController.SetMoveInput(Vector3.zero); }
		}

		private void Jumping()
		{
			// Set the input on the jump axis every frame.
			Vector3 jumpInput = inputJump ? Vector3.up : Vector3.zero;
			rpgCharacterController.SetJumpInput(jumpInput);

			// If we pressed jump button this frame, jump.
			if (inputJump && rpgCharacterController.CanStartAction("Jump")) { rpgCharacterController.StartAction("Jump"); } else if (inputJump && rpgCharacterController.CanStartAction("DoubleJump")) { rpgCharacterController.StartAction("DoubleJump"); }
		}

		public void Rolling()
		{
			if (!inputRoll) { return; }
			if (!rpgCharacterController.CanStartAction("DiveRoll")) { return; }
			if (characterControl.GetCurStamina() <= 0) { return; }
			if(isPlayer) characterControl.TryReduceStamina(12.0f);
			//rpgCharacterController.StartAction("DiveRoll", 1);
			if (rpgCharacterController.isStrafing)
            {
				rpgCharacterController.EndAction("Strafe");
				switch (rollDirection)
                {
					case 0:
						rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Forward);
						break;
					case 1:
						rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Backward);
						break;
					case 2:
						rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Left);
						break;
					case 3:
						rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Right);
						break;
					default:
						rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Forward);
						break;
                }
				/*
				if (doEndStrafe)
				{
					CancelStrafing();
				}
				*/
			}
            else
            {
				rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Forward);
			}
			

		}

		public void CancelStrafing()
        {
			if (isPlayer && PV.IsMine)
			{
				if (outlineTarget != null)
				{
					outlineTarget.enabled = false;
				}
			}
			doStrafe = false;
		}

		private void Aiming()
		{
			if (rpgCharacterController.hasAimedWeapon) {
				if (rpgCharacterController.HandlerExists(HandlerTypes.Aim)) {
					if (HasAimInput()) { rpgCharacterController.TryStartAction(HandlerTypes.Aim); }
					else { rpgCharacterController.TryEndAction(HandlerTypes.Aim); }
				}
				if (rpgCharacterController.rightWeapon == Weapon.TwoHandBow) {

					// If using the bow, we want to pull back slowly on the bow string while the
					// Left Mouse button is down, and shoot when it is released.
					if (Mouse.current.leftButton.isPressed) { bowPull += 0.05f; }
					else if (Mouse.current.leftButton.wasReleasedThisFrame) {
						if (rpgCharacterController.HandlerExists(HandlerTypes.Shoot))
						{ rpgCharacterController.TryStartAction(HandlerTypes.Shoot); }
					}
					else { bowPull = 0f; }
					bowPull = Mathf.Clamp(bowPull, 0f, 1f);
				}
				else {
					// If using a gun or a crossbow, we want to fire when the left mouse button is pressed.
					if (rpgCharacterController.HandlerExists(HandlerTypes.Shoot)) {
						if (Mouse.current.leftButton.isPressed) { rpgCharacterController.TryStartAction(HandlerTypes.Shoot); }
					}
				}
				// Reload.
				if (rpgCharacterController.HandlerExists(HandlerTypes.Reload)) {
					if (Mouse.current.rightButton.isPressed) { rpgCharacterController.TryStartAction(HandlerTypes.Reload); }
				}
				// Finally, set aim location and bow pull.
				rpgCharacterController.SetAimInput(rpgCharacterController.target.position);
				rpgCharacterController.SetBowPull(bowPull);
			}
			else { Strafing(); }
		}

		private int rollDirection = 0;
		private bool doStrafe = false;
		//private bool doEndStrafe = false;

		bool prevInputAim = false;
		private void Strafing()
		{
			if (rpgCharacterController.canStrafe && !rpgCharacterController.hasAimedWeapon) {

				if (inputAim && !prevInputAim)
				{
					doStrafe = !doStrafe;
					//print("Strafe!!");
				}
				prevInputAim = inputAim;
				if (doStrafe)
				{
                    if (isPlayer)
                    {
						outlineTarget = characterControl.GetAimTargetPosition();
					}
					else if (!isPlayer)
                    {
						characterControl.GetClosestPlayer();
					}
					if (rpgCharacterController.aimInput.y < 0)
                    {
						doStrafe = false;
						return;
					}

					if (rpgCharacterController.CanStartAction("Strafe")) 
					{ 
						rpgCharacterController.StartAction("Strafe");
						if (isPlayer && PV.IsMine) outlineTarget.enabled = true;
					}

					Vector3 aimInput = (rpgCharacterController.aimInput - transform.position).normalized;
					float inputAngleDiff = Vector2.Angle(new Vector2(aimInput.x, aimInput.z), new Vector2(moveInput.x, moveInput.y));
					float zDirection = Vector3.Cross(new Vector3(aimInput.x, aimInput.z, moveInput.z), moveInput).normalized.z;

					if (inputAngleDiff < 50) rollDirection = 0; // forward
					else if (inputAngleDiff > 130) rollDirection = 1; // back
					else rollDirection = zDirection > 0 ? 2 : 3; // left or right
					//doEndStrafe = inputAngleDiff >= 100 ? true : false;
				}
				else
				{
					if (rpgCharacterController.CanEndAction("Strafe")) 
					{ 
						rpgCharacterController.EndAction("Strafe");
						if (isPlayer && PV.IsMine) outlineTarget.enabled = false;
					}
				}
			}
            else
            {
				doStrafe = false;
            }	
		}

		private void Facing()
		{
			if (rpgCharacterController.canFace) {
				if (HasFacingInput()) {
					if (inputFace && !inputAttackL && !inputAttackR) {

						// Get world position from mouse position on screen and convert to direction from character.
						Plane playerPlane = new Plane(Vector3.up, transform.position);
						Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
						float hitdist = 0.0f;
						if (playerPlane.Raycast(ray, out hitdist)) {
							Vector3 targetPoint = ray.GetPoint(hitdist);
							Vector3 lookTarget = new Vector3(targetPoint.x - transform.position.x, transform.position.z - targetPoint.z, 0);
							rpgCharacterController.SetFaceInput(lookTarget);
						}
					}
					else { rpgCharacterController.SetFaceInput(new Vector3(inputFacing.x, inputFacing.y, 0)); }

					if (rpgCharacterController.CanStartAction("Face")) { rpgCharacterController.StartAction("Face"); }
				}
				else {
					if (rpgCharacterController.CanEndAction("Face")) { rpgCharacterController.EndAction("Face"); }
				}
			}
		}

		public int attackNumberTest = 1;
		private bool rightAttack = false;
		private float reduceStamina;
		private bool isKickMove = false;
		private void Attacking()
		{
			// Check to make sure Attack and Cast Actions exist.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Attack)
				&& rpgCharacterController.HandlerExists(HandlerTypes.AttackCast)) { return; }

			// If already casting, stop casting.
			if ((inputCastL || inputCastR) && rpgCharacterController.IsActive(HandlerTypes.AttackCast)) {
				rpgCharacterController.EndAction(HandlerTypes.AttackCast);
				return;
			}
			//rpgCharacterController.animationSpeed = 2.0f;
			// Check to make character can Attack.
			if (!rpgCharacterController.CanStartAction(HandlerTypes.Attack)) { return; }
			if (isPlayer && characterControl.GetCurStamina() <= 0) { return; }

			rpgCharacterWeaponController.SetAllWeaponToUnattacked();
			WeaponTrigger mainWeapon = characterControl.curWeaponTrigger;
			WeaponTrigger offhandWeapon = characterControl.curOffhandWeaponTrigger;
			//rpgCharacterWeaponController.SetAllWeaponToUnattacked();
			if (inputAttackL)
            {
				//Set Base Attack
				mainWeapon.SetAttackType(WeaponTrigger.attackType.baseAttack);
				reduceStamina = mainWeapon.GetEnergyCost();
				//characterControl.TryReduceStamina(mainWeapon.GetEnergyCost());
				if (characterControl.curWeaponType != Weapon.LeftSword || !rightAttack) //If nor sword or attacking with main hand
                {
					rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, mainWeapon.side, mainWeapon.GetAnimNumber()));
					SetAttackStartAndEndFrame(mainWeapon.GetStartFrame(), mainWeapon.GetEndFrame(), mainWeapon);
				}
				else //Offhand dagger
                {
					//Set Base Attack
					offhandWeapon.SetAttackType(WeaponTrigger.attackType.baseAttack);
					rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, offhandWeapon.side, offhandWeapon.GetAnimNumber()));
					SetAttackStartAndEndFrame(offhandWeapon.GetStartFrame(), offhandWeapon.GetEndFrame(), offhandWeapon);
				}
				rightAttack = !rightAttack;
            } 
			else if (inputAttackR)
            {
				if (offhandWeapon == null) offhandWeapon = mainWeapon;
				offhandWeapon.SetAttackType((WeaponTrigger.attackType)characterControl.specialAttack);
				reduceStamina = offhandWeapon.GetEnergyCost();
				//characterControl.TryReduceStamina(offhandWeapon.GetEnergyCost());
				rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, offhandWeapon.side, offhandWeapon.GetAnimNumber()));
				SetAttackStartAndEndFrame(offhandWeapon.GetStartFrame(), offhandWeapon.GetEndFrame(), offhandWeapon, offhandWeapon.side == Side.Dual ? mainWeapon : null);
			}
			else if (inputCastL)
			{ rpgCharacterController.StartAction(HandlerTypes.AttackCast, new AttackCastContext(AnimationVariations.AttackCast.TakeRandom(), Side.Left)); }
			else if (inputCastR)
			{ rpgCharacterController.StartAction(HandlerTypes.AttackCast, new AttackCastContext(AnimationVariations.AttackCast.TakeRandom(), Side.Right)); }
			else if (inputKick)
			{
				//In the future, kick at the enemy side
				WeaponTrigger leftFootTrigger = rpgCharacterWeaponController.GetWeaponTrigger(Weapon.LeftFoot);
				reduceStamina = leftFootTrigger.GetEnergyCost();
				WeaponTrigger rightFootTrigger = rpgCharacterWeaponController.GetWeaponTrigger(Weapon.RightFoot);
				reduceStamina = rightFootTrigger.GetEnergyCost();
				isKickMove = true;
				Outline targetOutline = characterControl.GetAimTargetPosition();
				if (outlineTarget == null)
                {
					if (Random.value > 0.5f)
                    {
						rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Left, (int)KickType.RightKick1));
						SetAttackStartAndEndFrame(rightFootTrigger.GetStartFrame(), rightFootTrigger.GetEndFrame(), rightFootTrigger);
					}
						
					else
                    {
						rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Left, (int)KickType.LeftKick1));
						SetAttackStartAndEndFrame(leftFootTrigger.GetStartFrame(), leftFootTrigger.GetEndFrame(), leftFootTrigger);
					}
						
				}
                else
                {
					if (Vector3.Dot(transform.right, transform.position - characterControl.GetAimTargetPosition().transform.position) > 0)
                    {
						rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Left, (int)KickType.LeftKick1));
						SetAttackStartAndEndFrame(leftFootTrigger.GetStartFrame(), leftFootTrigger.GetEndFrame(), leftFootTrigger);
					}
						
					else
                    {
						rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Left, (int)KickType.RightKick1));
						SetAttackStartAndEndFrame(rightFootTrigger.GetStartFrame(), rightFootTrigger.GetEndFrame(), rightFootTrigger);
					}
				}
			}
			//rpgCharacterController.animationSpeed = 1.0f;
		}

		private void SetAttackStartAndEndFrame(float start, float end, WeaponTrigger weaponA, WeaponTrigger weaponB = null)
        {
			StartCoroutine(EnableWeaponTrigger(start, weaponA, weaponB));
			StartCoroutine(DisableWeaponTrigger(end, weaponA, weaponB));
		}

		private IEnumerator EnableWeaponTrigger(float time, WeaponTrigger weaponA, WeaponTrigger weaponB = null)
		{
			yield return new WaitForSeconds(time);
            if (!isKickMove)
            {
				if (characterControl.curWeaponType == Weapon.TwoHandSword)
				{
					audioSource.PlayOneShot(twoHandedSwordClip, 0.7f);
				}
				else if (characterControl.curWeaponType == Weapon.TwoHandAxe)
				{
					audioSource.PlayOneShot(twoHandedAxeClip, 0.7f);
				}
				else if (characterControl.curWeaponType == Weapon.TwoHandSpear)
				{
					audioSource.PlayOneShot(twoHandedSpearClip, 0.7f);
				}
				else if (characterControl.curWeaponType == Weapon.LeftSword || characterControl.curWeaponType == Weapon.RightSword)
				{
					audioSource.PlayOneShot(dualSwordClip, 0.7f);
				}
			}
            else
            {
				isKickMove = false;
            }
			weaponA.triggerEnabled = true;
			if(isPlayer) characterControl.TryReduceStamina(reduceStamina);
			if (weaponB) weaponB.triggerEnabled = true;
		}

		private IEnumerator DisableWeaponTrigger(float time, WeaponTrigger weaponA, WeaponTrigger weaponB = null)
		{
			yield return new WaitForSeconds(time);
			weaponA.triggerEnabled = false;
			if (weaponB) weaponB.triggerEnabled = false;
		}

		private void Damage()
		{
			// Hit.
			if (inputLightHit) { 
				if (rpgCharacterController.CanStartAction("GetHit"))
                {
					rpgCharacterController.StartAction("GetHit", new HitContext());
				}
			}

			// Death.
			if (inputDeath) {
				if (rpgCharacterController.CanStartAction("Death")) { rpgCharacterController.StartAction("Death"); }
				else if (rpgCharacterController.CanEndAction("Death")) { rpgCharacterController.EndAction("Death"); }
			}
		}

		/// <summary>
		/// Cycle weapons using directional pad input. Up and Down cycle forward and backward through
		/// the list of two handed weapons. Left cycles through the left hand weapons. Right cycles through
		/// the right hand weapons.
		/// </summary>
		private void SwitchWeapons()
		{
			// Check to make sure SwitchWeapon Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.SwitchWeapon)) { return; }

			// Bail out if we can't switch weapons.
			if (!rpgCharacterController.CanStartAction(HandlerTypes.SwitchWeapon)) { return; }

			// Switch to Relaxed.
			if (inputRelax) {
				rpgCharacterController.StartAction(HandlerTypes.Relax);
				return;
			}

			var doSwitch = false;
			var context = new SwitchWeaponContext();
			var weaponNumber = Weapon.Unarmed;

			// Switch to Shield.
			if (inputShield) {
				doSwitch = true;
				context.side = "Left";
				context.type = "Switch";
				context.leftWeapon = Weapon.Shield;
				context.rightWeapon = weaponNumber;
				rpgCharacterController.StartAction(HandlerTypes.SwitchWeapon, context);
				return;
			}

			// Cycle through 2Handed weapons if any input happens on the up-down axis.
			if (inputSwitchUp || inputSwitchDown) {
				var twoHandedWeapons = new Weapon[] {
					Weapon.TwoHandSword,
					 Weapon.TwoHandSpear,
					 Weapon.TwoHandAxe,
					 Weapon.TwoHandBow,
					 Weapon.TwoHandCrossbow,
					 Weapon.TwoHandStaff,
					 Weapon.Rifle,
				};
				// If we're not wielding 2Handed weapon already, just switch to the first one in the list.
				if (System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon) == -1)
				{ weaponNumber = twoHandedWeapons[0]; }

				// Otherwise, we should loop through them.
				else {
					var index = System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon);
					if (inputSwitchUp) { index = (index - 1 + twoHandedWeapons.Length) % twoHandedWeapons.Length; }
					else if (inputSwitchDown) { index = (index + 1) % twoHandedWeapons.Length; }
					weaponNumber = twoHandedWeapons[index];
				}
				// Set up the context and flag that we actually want to perform the switch.
				doSwitch = true;
				context.type = HandlerTypes.Switch;
				context.side = "None";
				context.leftWeapon = Weapon.Relax;
				context.rightWeapon = weaponNumber;
			}

			// Cycle through 1Handed weapons if any input happens on the left-right axis.
			if (inputSwitchLeft  || inputSwitchRight) {
				doSwitch = true;
				context.type = HandlerTypes.Switch;

				// Left-handed weapons.
				if (inputSwitchLeft) {
					var leftWeaponType = rpgCharacterController.leftWeapon;

					// If we are not wielding a left-handed weapon, switch to Left Sword.
					if (System.Array.IndexOf(WeaponGroupings.LeftHandedWeapons, leftWeaponType) == -1)
					{ weaponNumber = Weapon.LeftSword; }

					// Otherwise, cycle through the list.
					else {
						var currentIndex = System.Array.IndexOf(WeaponGroupings.LeftHandedWeapons, leftWeaponType);
						weaponNumber = WeaponGroupings.LeftHandedWeapons[(currentIndex + 1) % WeaponGroupings.LeftHandedWeapons.Length];
					}

					context.side = "Left";
					context.leftWeapon = weaponNumber;
					context.rightWeapon = Weapon.Relax;
				}
				// Right-handed weapons.
				else if (inputSwitchRight) {
					var rightWeaponType = rpgCharacterController.rightWeapon;

					// If we are not wielding a right-handed weapon, switch to Unarmed.
					if (System.Array.IndexOf(WeaponGroupings.RightHandedWeapons, rightWeaponType) == -1)
					{ weaponNumber = Weapon.Unarmed; }

					// Otherwise, cycle through the list.
					else {
						var currentIndex = System.Array.IndexOf(WeaponGroupings.RightHandedWeapons, rightWeaponType);
						weaponNumber = WeaponGroupings.RightHandedWeapons[(currentIndex + 1) % WeaponGroupings.RightHandedWeapons.Length];
					}
					context.side = "Right";
					context.leftWeapon = Weapon.Relax;
					context.rightWeapon = weaponNumber;
				}
			}
			// If we've received input, then "doSwitch" is true, and the context is filled out,
			// so start the SwitchWeapon action.
			if (doSwitch) { rpgCharacterController.StartAction(HandlerTypes.SwitchWeapon, context); }
		}
	}

	/// <summary>
	/// Extension Method to allow checking InputSystem without Action Callbacks.
	/// </summary>
	public static class InputActionExtensions
	{
		public static bool IsPressed(this InputAction inputAction) => inputAction.ReadValue<float>() > 0f;

		public static bool WasPressedThisFrame(this InputAction inputAction) => inputAction.triggered && inputAction.ReadValue<float>() > 0f;

		public static bool WasReleasedThisFrame(this InputAction inputAction) => inputAction.triggered && inputAction.ReadValue<float>() == 0f;
	}
}