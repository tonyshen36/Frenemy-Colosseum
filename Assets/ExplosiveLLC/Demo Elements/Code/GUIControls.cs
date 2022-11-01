using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims
{
    public class GUIControls : MonoBehaviour
    {
        private RPGCharacterController rpgCharacterController;
        private RPGCharacterWeaponController rpgCharacterWeaponController;
        private float charge;
		private float idleStatic;
        private bool useHips;
        private bool useDual;
        private bool useInstant;
        private bool isTalking;
        private bool isAiming;
        private bool hipShooting;
        private bool useNavigation;
        private float swimTimeout;
        private Vector3 jumpInput;
        public GameObject nav;

        private void Start()
        {
            // Get other RPG Character components.
            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgCharacterWeaponController = GetComponent<RPGCharacterWeaponController>();
        }

        private void OnGUI()
        {
			if (!rpgCharacterController.isCasting && rpgCharacterController.maintainingGround) { Navigation(); }

	        // Character is dead or using Navmesh.
	        if (rpgCharacterController.isDead) {
		        Misc();
		        return;
	        }
	        // Character is swimming.
	        if (rpgCharacterController.isSwimming) {
		        Swimming();
		        return;
	        }
	        // Character is not on the ground.
	        if (!rpgCharacterController.maintainingGround) {
		        Jumping();
		        return;
	        }
            // Character is not Casting.
            if (!rpgCharacterController.isCasting) {
	            Idle();
	            Crouching();
	            Sprinting();
	            Charging();
	            Emotes();
	            Attacks();
	            Damage();
	            RollDodgeTurn();
				if (!rpgCharacterController.isBlocking) { WeaponSwitching(); }
            }
			// Character is not doing Special Attack.
			if (!rpgCharacterController.isSpecial) { Blocking(); }

		    Climbing();
            Casting();
			DebugRPGCharacter();
        }

        private void Sprinting()
        {
			// Check if Sprint Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Sprint)) {
				bool useSprint = GUI.Toggle(new Rect(640, 115, 100, 30), rpgCharacterController.isSprinting, "Sprint");
				if (useSprint && rpgCharacterController.CanStartAction(HandlerTypes.Sprint))
				{ rpgCharacterController.StartAction(HandlerTypes.Sprint); }
				else if (!useSprint && rpgCharacterController.CanEndAction(HandlerTypes.Sprint))
				{ rpgCharacterController.EndAction(HandlerTypes.Sprint); }
			}
		}

        private void Charging()
        {
			// Check if the character has a shield.
	        if (!rpgCharacterController.hasShield) { return; }
	        GUI.Button(new Rect(620, 140, 100, 30), "Charge");
	        charge = GUI.HorizontalSlider(new Rect(620, 170, 100, 30), charge, 0.0F, 1f);
	        rpgCharacterController.animator.SetFloat(HandlerTypes.Charge, charge);
        }

		private void Idle()
		{
			GUI.Button(new Rect(540, 140, 60, 30), "Idle");
			idleStatic = GUI.HorizontalSlider(new Rect(540, 170, 60, 30), idleStatic, 0.0F, 1f);
			rpgCharacterController.animator.SetFloat(AnimationParameters.Idle, idleStatic);
		}

		private void Navigation()
        {
			// Check to make sure Navigation Action exists.
            if (!rpgCharacterController.HandlerExists(HandlerTypes.Navigation)) { return; }

            useNavigation = GUI.Toggle(new Rect(550, 105, 100, 30), useNavigation, "Navigation");

            var navChild = nav.transform.GetChild(0);
            if (useNavigation) {

				// Show the navigation pointer.
	            navChild.GetComponent<MeshRenderer>().enabled = true;
	            navChild.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
                    nav.transform.position = hit.point;
                    if (Input.GetMouseButtonDown(0))
					{ rpgCharacterController.StartAction(HandlerTypes.Navigation, hit.point); }
                }
            }
			else {
				// Hide the navigation pointer.
	            if (!rpgCharacterController.CanEndAction(HandlerTypes.Navigation)) { return; }
	            navChild.GetComponent<MeshRenderer>().enabled = false;
	            navChild.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
	            rpgCharacterController.EndAction(HandlerTypes.Navigation);
            }
        }

		private void Attacks()
		{
			// Check if Attack Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Attack)) { return; }

			// End special attack.
			if (rpgCharacterController.CanEndAction(HandlerTypes.Attack) && rpgCharacterController.isSpecial) {
				if (GUI.Button(new Rect(235, 85, 100, 30), "End Special"))
				{ rpgCharacterController.EndAction(HandlerTypes.Attack); }
			}

			if (!rpgCharacterController.CanStartAction(HandlerTypes.Attack)) { return; }

			if (rpgCharacterController.hasLeftWeapon
				|| (rpgCharacterController.leftWeapon == Weapon.Unarmed && rpgCharacterController.rightWeapon == Weapon.Unarmed)) {
				if (GUI.Button(new Rect(25, 85, 100, 30), "Attack L"))
				{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Attack", Side.Left)); }
			}
			if (rpgCharacterController.hasRightWeapon
				|| (rpgCharacterController.rightWeapon == Weapon.Unarmed && rpgCharacterController.leftWeapon == Weapon.Unarmed)) {
				if (GUI.Button(new Rect(130, 85, 100, 30), "Attack R"))
				{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Attack", Side.Right)); }
			}
			if (rpgCharacterController.hasTwoHandedWeapon) {
				if (GUI.Button(new Rect(130, 85, 100, 30), "Attack"))
				{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Attack", Side.None)); }
			}
			if (rpgCharacterController.hasDualWeapons) {

				// Can't Dual Attack with Item weapons or with a Shield.
				if (rpgCharacterController.rightWeapon != Weapon.RightItem && rpgCharacterController.leftWeapon != Weapon.LeftItem
					&& rpgCharacterController.leftWeapon != Weapon.Shield) {
					if (GUI.Button(new Rect(235, 85, 100, 30), "Attack Dual"))
					{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Attack", Side.Dual)); }
				}
			}
			//Special Attack.
			if (rpgCharacterController.hasTwoHandedWeapon && !rpgCharacterController.hasAimedWeapon) {
				if (GUI.Button(new Rect(335, 85, 100, 30), "Special Attack1"))
				{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Special", Side.None)); }
			}
			// Sword + Mace Special Attack.
			if ((rpgCharacterController.leftWeapon == Weapon.LeftSword || rpgCharacterController.leftWeapon == Weapon.LeftMace)
				&& (rpgCharacterController.rightWeapon == Weapon.RightSword || rpgCharacterController.rightWeapon == Weapon.RightMace)) {
				if (GUI.Button(new Rect(335, 85, 100, 30), "Special Attack1"))
				{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Special", Side.Right)); }
			}
			// Kicking.
			if (GUI.Button(new Rect(25, 115, 100, 30), "Left Kick"))
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Left, ( int )KickType.LeftKick1)); }
			if (GUI.Button(new Rect(25, 145, 100, 30), "Left Kick2"))
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Left, ( int )KickType.LeftKick2)); }
			if (GUI.Button(new Rect(130, 115, 100, 30), "Right Kick"))
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Right, ( int )KickType.RightKick1)); }
			if (GUI.Button(new Rect(130, 145, 100, 30), "Right Kick2"))
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext("Kick", Side.Right, ( int )KickType.RightKick2)); }
		}

		private void Damage()
        {
			// Check if Get Hit Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.GetHit)
				&& rpgCharacterController.CanStartAction(HandlerTypes.GetHit)) {
					if (GUI.Button(new Rect(30, 240, 100, 30), "Get Hit"))
					{ rpgCharacterController.StartAction(HandlerTypes.GetHit, new HitContext()); }
			}
			// Check if Knockback Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Knockback)
				&& rpgCharacterController.CanStartAction(HandlerTypes.Knockback)) {
					if (GUI.Button(new Rect(130, 240, 100, 30), "Knockback1"))
					{ rpgCharacterController.StartAction(HandlerTypes.Knockback, new HitContext((int)KnockbackType.Knockback1, Vector3.back)); }
					if (GUI.Button(new Rect(230, 240, 100, 30), "Knockback2"))
					{ rpgCharacterController.StartAction(HandlerTypes.Knockback, new HitContext((int)KnockbackType.Knockback2, Vector3.back)); }
			}
			// Check if Knockdown Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Knockdown)
				&& rpgCharacterController.CanStartAction(HandlerTypes.Knockdown)) {
					if (GUI.Button(new Rect(130, 270, 100, 30), "Knockdown"))
					{ rpgCharacterController.StartAction(HandlerTypes.Knockdown, new HitContext((int)KnockdownType.Knockdown1, Vector3.back)); }
			}
        }

        private void Crouching()
        {
			// Check if Crouch Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Crouch)) {
				bool useCrouch = GUI.Toggle(new Rect(640, 95, 100, 30), rpgCharacterController.isCrouching, "Crouch");
				if (useCrouch && rpgCharacterController.CanStartAction(HandlerTypes.Crouch))
				{ rpgCharacterController.StartAction(HandlerTypes.Crouch); }
				else if (!useCrouch && rpgCharacterController.CanEndAction(HandlerTypes.Crouch))
				{ rpgCharacterController.EndAction(HandlerTypes.Crouch); }

				// Check if Crawl Action exists.
				if (!rpgCharacterController.HandlerExists(HandlerTypes.Crawl)) { return; }

				bool useCrawl = rpgCharacterController.isCrawling;
				if (useCrouch) {
					if (GUI.Button(new Rect(640, 140, 100, 30), "Crawl")) {
						rpgCharacterController.Crawl();
						rpgCharacterController.TryStartAction(HandlerTypes.Crawl);
						rpgCharacterController.TryEndAction(HandlerTypes.Crouch);
					}
				}

				if (!useCrawl) { return; }

				if (GUI.Button(new Rect(640, 140, 100, 30), "Crawl")) {
					rpgCharacterController.StartAction(HandlerTypes.Idle);
					rpgCharacterController.TryStartAction(HandlerTypes.Crouch);
				}
			}
        }

        private void Blocking()
        {
			// Check if Block Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Block)) { return; }

			if (!rpgCharacterController.isCasting
				&& !rpgCharacterController.isSitting
				&& !rpgCharacterController.IsActive(HandlerTypes.Relax)) {
                var blockGui = GUI.Toggle(new Rect(25, 215, 100, 30), rpgCharacterController.isBlocking, "Block");

                if (blockGui && rpgCharacterController.CanStartAction(HandlerTypes.Block))
				{ rpgCharacterController.StartAction(HandlerTypes.Block); }
				else if (!blockGui && rpgCharacterController.CanEndAction(HandlerTypes.Block))
				{ rpgCharacterController.EndAction(HandlerTypes.Block); }

                if (blockGui) {

					// Check if Get Hit Action exists.
					if (!rpgCharacterController.HandlerExists(HandlerTypes.GetHit)) { return; }

					if (GUI.Button(new Rect(30, 240, 100, 30), "Get Hit"))
					{ rpgCharacterController.StartAction(HandlerTypes.GetHit, new HitContext()); }
                }
            }
        }

		private void RollDodgeTurn()
		{
			// Check if Roll Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Block)) {
				if (rpgCharacterController.CanStartAction(HandlerTypes.Roll)) {
					if (GUI.Button(new Rect(25, 15, 100, 30), "Roll Forward"))
					{ rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Forward); }
					if (GUI.Button(new Rect(130, 15, 100, 30), "Roll Backward"))
					{ rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Backward); }
					if (GUI.Button(new Rect(25, 45, 100, 30), "Roll Left"))
					{ rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Left); }
					if (GUI.Button(new Rect(130, 45, 100, 30), "Roll Right"))
					{ rpgCharacterController.StartAction(HandlerTypes.Roll, RollType.Right); }
				}
			}
			// Check if Dodge Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Dodge)) {
				if (rpgCharacterController.CanStartAction(HandlerTypes.Dodge)) {
					if (GUI.Button(new Rect(235, 15, 100, 30), "Dodge Left"))
					{ rpgCharacterController.StartAction(HandlerTypes.Dodge, DodgeType.Left); }
					if (GUI.Button(new Rect(235, 45, 100, 30), "Dodge Right"))
					{ rpgCharacterController.StartAction(HandlerTypes.Dodge, DodgeType.Right); }
				}
			}
			// Check if Turn Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Turn)) {
				if (rpgCharacterController.CanStartAction(HandlerTypes.Turn)) {
					if (GUI.Button(new Rect(340, 15, 100, 30), "Turn Left"))
					{ rpgCharacterController.StartAction(HandlerTypes.Turn, TurnType.Left); }
					if (GUI.Button(new Rect(340, 45, 100, 30), "Turn Right"))
					{ rpgCharacterController.StartAction(HandlerTypes.Turn, TurnType.Right); }
					if (GUI.Button(new Rect(445, 15, 100, 30), "Turn Left 180"))
					{ rpgCharacterController.StartAction(HandlerTypes.Turn, TurnType.Left180); }
					if (GUI.Button(new Rect(445, 45, 100, 30), "Turn Right 180"))
					{ rpgCharacterController.StartAction(HandlerTypes.Turn, TurnType.Right180); }
				}
			}
			// Check if DiveRoll Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.DiveRoll)) {
				if (rpgCharacterController.CanStartAction(HandlerTypes.DiveRoll)) {
					if (GUI.Button(new Rect(445, 75, 100, 30), "Dive Roll"))
					{ rpgCharacterController.StartAction(HandlerTypes.DiveRoll, DiveRollType.DiveRoll1); }
				}
			}
		}

        private void Casting()
        {
			// Check if Cast Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Cast)) { return; }

			if (rpgCharacterController.CanEndAction(HandlerTypes.Cast) && rpgCharacterController.isCasting) {
                if (GUI.Button(new Rect(25, 330, 100, 30), "Stop Casting"))
                { rpgCharacterController.EndAction(HandlerTypes.Cast); }
            }

			if (rpgCharacterController.CanEndAction(HandlerTypes.AttackCast) && rpgCharacterController.isCasting) {
				if (GUI.Button(new Rect(25, 330, 100, 30), "Stop Casting"))
				{ rpgCharacterController.EndAction(HandlerTypes.AttackCast); }
			}

            if (!rpgCharacterController.CanStartAction(HandlerTypes.Cast)) { return; }

            var leftUnarmed = rpgCharacterController.leftWeapon == Weapon.Unarmed;
            var rightUnarmed = rpgCharacterController.rightWeapon == Weapon.Unarmed;
            var wieldStaff = rpgCharacterController.rightWeapon == Weapon.TwoHandStaff;

            if (leftUnarmed && GUI.Button(new Rect(25, 330, 100, 30), "Cast Atk Left"))
			{ rpgCharacterController.StartAction(HandlerTypes.AttackCast, new AttackCastContext(AnimationVariations.AttackCast.TakeRandom(), Side.Left)); }
            if (rightUnarmed && GUI.Button(new Rect(125, 330, 100, 30), "Cast Atk Right"))
			{ rpgCharacterController.StartAction(HandlerTypes.AttackCast, new AttackCastContext(AnimationVariations.AttackCast.TakeRandom(), Side.Right)); }
            if (leftUnarmed && rightUnarmed && GUI.Button(new Rect(80, 365, 100, 30), "Cast Atk Dual"))
			{ rpgCharacterController.StartAction(HandlerTypes.AttackCast, new AttackCastContext(AnimationVariations.AttackCast.TakeRandom(), Side.Dual)); }
            if (!rpgCharacterController.hasDualWeapons
				&& (rpgCharacterController.leftWeapon.IsCastableWeapon() || rpgCharacterController.rightWeapon.IsCastableWeapon())) {
                if (GUI.Button(new Rect(25, 425, 100, 30), "Cast AOE"))
                { rpgCharacterController.StartAction(HandlerTypes.Cast, new CastContext(AnimationVariations.AOE.TakeRandom(), Side.Dual)); }
                if (GUI.Button(new Rect(25, 400, 100, 30), "Cast Buff"))
                { rpgCharacterController.StartAction(HandlerTypes.Cast, new CastContext(AnimationVariations.Buff.TakeRandom(), Side.Dual)); }
                if (GUI.Button(new Rect(25, 450, 100, 30), "Cast Summon"))
                { rpgCharacterController.StartAction(HandlerTypes.Cast, new CastContext(AnimationVariations.Summon.TakeRandom(), Side.Dual)); }
            }
        }

        private void Jumping()
        {
			// Check if Jump Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Jump)) { return; }

			if (rpgCharacterController.CanStartAction(HandlerTypes.Jump)) {
                if (GUI.Button(new Rect(25, 175, 100, 30), "Jump")) {
                    rpgCharacterController.SetJumpInput(Vector3.up);
                    rpgCharacterController.StartAction(HandlerTypes.Jump);
                }
            }
            if (rpgCharacterController.CanStartAction(HandlerTypes.DoubleJump)) {
                if (GUI.Button(new Rect(25, 175, 100, 30), "Jump Flip")) {
                    rpgCharacterController.SetJumpInput(Vector3.up);
                    rpgCharacterController.StartAction(HandlerTypes.DoubleJump);
                }
            }
        }

        private void Emotes()
        {
			// Check if Emote Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Emote)) {
				if (rpgCharacterController.CanStartAction(HandlerTypes.Emote)) {
					var emote = EmoteType.Unknown;
					if (GUI.Button(new Rect(665, 680, 100, 30), "Sleep")) { emote = EmoteType.Laydown; }
					if (GUI.Button(new Rect(770, 680, 100, 30), "Sit")) { emote = EmoteType.Sit; }
					if (GUI.Button(new Rect(770, 650, 100, 30), "Drink")) { emote = EmoteType.Drink; }
					if (GUI.Button(new Rect(665, 650, 100, 30), "Bow")) { emote = EmoteType.Bow1; }
					if (GUI.Button(new Rect(560, 650, 100, 30), "Yes")) { emote = EmoteType.Yes; }
					if (GUI.Button(new Rect(455, 650, 100, 30), "No")) { emote = EmoteType.No; }
					if (GUI.Button(new Rect(130, 175, 100, 30), "Pickup")) { emote = EmoteType.Pickup; }
					if (GUI.Button(new Rect(235, 175, 100, 30), "Activate")) { emote = EmoteType.Activate; }

					if (emote != EmoteType.Unknown) { rpgCharacterController.StartAction(HandlerTypes.Emote, emote); }
				}
				if (rpgCharacterController.CanEndAction(HandlerTypes.Emote)) {
					if (rpgCharacterController.isSitting) {
						if (GUI.Button(new Rect(795, 680, 100, 30), "Stand"))
						{ rpgCharacterController.EndAction(HandlerTypes.Emote); }
					}
				}
			}
			// Check if character can talk.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Talk)) {
				if (rpgCharacterController.CanStartAction(HandlerTypes.Talk))
				{
					if (GUI.Button(new Rect(560, 680, 100, 30), "Start Talking"))
					{ rpgCharacterController.StartAction(HandlerTypes.Talk, AnimationVariations.Conversations.TakeRandom()); }
				}
				if (rpgCharacterController.CanEndAction(HandlerTypes.Talk)) {
					if (rpgCharacterController.isTalking) {
						if (GUI.Button(new Rect(795, 680, 100, 30), "Stop Talking"))
						{ rpgCharacterController.EndAction(HandlerTypes.Talk); }
					}
				}
			}
			// Check if Emote Combat Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.EmoteCombat)) {
				if (rpgCharacterController.CanStartAction(HandlerTypes.EmoteCombat)) {
					var emote = EmoteType.Unknown;
					if (GUI.Button(new Rect(480, 650, 100, 30), "Boost")) { emote = EmoteType.Boost; }

					// Pickup and Activate require a free hand.
					if (!rpgCharacterController.hasDualWeapons) {
						if (GUI.Button(new Rect(130, 175, 100, 30), "Pickup")) { emote = EmoteType.Pickup; }
						if (GUI.Button(new Rect(235, 175, 100, 30), "Activate")) { emote = EmoteType.Activate; }
					}

					if (emote != EmoteType.Unknown) { rpgCharacterController.StartAction(HandlerTypes.EmoteCombat, emote); }
				}
			}
        }

        private void Climbing()
        {
	        // Check if Climb Ladder  Action exists.
	        if (!rpgCharacterController.HandlerExists(HandlerTypes.ClimbLadder)) { return; }

	        if (rpgCharacterController.CanStartAction(HandlerTypes.ClimbLadder)) {
				if (GUI.Button(new Rect(640, 360, 100, 30), "Climb Ladder"))
				{ rpgCharacterController.StartAction(HandlerTypes.ClimbLadder); }
			}
        }

        private void Swimming()
        {
	        if (!rpgCharacterController.isSwimming) { return; }

	        var swimTime = 0.5f;

			// Extract the InputSystem - Requires InputSystem Package.package to remove a
			// warning about this missing component.
			#if ENABLE_INPUT_SYSTEM
			var inputController = rpgCharacterController.GetComponent<RPGCharacterInputSystemController>();
			#else
				var inputController = rpgCharacterController.GetComponent<RPGCharacterInputController>();
			#endif

			if (GUI.Button(new Rect(25, 175, 100, 30), "Swim Up")) {
		        swimTimeout = Time.time + swimTime;
		        jumpInput = Vector3.up;

		        // Override the jump input for a half second to simulate a button press.
		        if (inputController != null) { inputController.PauseInput(swimTime); }
	        }
	        if (GUI.Button(new Rect(25, 225, 100, 30), "Swim Down")) {
		        swimTimeout = Time.time + swimTime;
		        jumpInput = Vector3.down;

		        // Override the jump input for a half second to simulate a button press.
		        if (inputController != null) { inputController.PauseInput(swimTime); }
	        }

	        if (Time.time < swimTimeout) { rpgCharacterController.SetJumpInput(jumpInput); }
        }

        // Death / Debug.
        private void Misc()
        {
            var deathReviveLabel = rpgCharacterController.isDead ? "Revive" : "Death";
            if (!rpgCharacterController.isClimbing && !rpgCharacterController.isCasting
				&& !rpgCharacterController.isSitting && rpgCharacterController.maintainingGround) {

				// Check if Climb Ladder  Action exists.
				if (rpgCharacterController.HandlerExists(HandlerTypes.Death)) {
					if (GUI.Button(new Rect(30, 270, 100, 30), deathReviveLabel)) {
						if (!rpgCharacterController.TryStartAction(HandlerTypes.Death))
						{ rpgCharacterController.TryEndAction(HandlerTypes.Death); }
					}
				}
            }
        }

		private void DebugRPGCharacter()
		{
			// Debug.
			if (GUI.Button(new Rect(600, 20, 120, 30), "Debug Controller"))
			{ rpgCharacterController.DebugController(); }
			if (GUI.Button(new Rect(600, 50, 120, 30), "Debug Animator"))
			{ rpgCharacterController.animator.DebugAnimatorParameters(); }
		}

        private void WeaponSwitching()
		{
			// Check if SwitchWeapon Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.SwitchWeapon)) { return; }

			var doSwitch = false;
			var context = new SwitchWeaponContext();

			// Check if Relax Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Relax)) {
				if (!rpgCharacterController.isRelaxed) {
					if (GUI.Button(new Rect(1115, 240, 100, 30), "Relax")) {
						if (useInstant) { rpgCharacterController.StartAction(HandlerTypes.Relax, true); }
						else { rpgCharacterController.StartAction(HandlerTypes.Relax); }
					}
				}
			}
			if (rpgCharacterController.rightWeapon != Weapon.Unarmed
				|| rpgCharacterController.leftWeapon != Weapon.Unarmed) {
				if (GUI.Button(new Rect(1115, 280, 100, 30), "Unarmed")) {
					doSwitch = true;
					context.type = "Switch";
					context.side = "Dual";
					context.leftWeapon = Weapon.Unarmed;
					context.rightWeapon = Weapon.Unarmed;
				}
			}
			var offset = 310;

			foreach (var weapon in WeaponGroupings.TwoHandedWeapons) {
				if (rpgCharacterController.rightWeapon != weapon) {
					var label = weapon.ToString();
					if (label.StartsWith("TwoHand")) { label = label.Replace("TwoHand", "2H "); }
					if (GUI.Button(new Rect(1115, offset, 100, 30), label)) {
						doSwitch = true;
						context.type = "Switch";
						context.side = "None";
						context.leftWeapon = Weapon.Relax;
						context.rightWeapon = weapon;
					}
				}
				offset += 30;
			}

			offset = 530;

			foreach (var pair in WeaponGroupings.LeftRightWeaponPairs) {
				bool missingOneSide = false;

				// Left weapons.
				if (rpgCharacterController.leftWeapon != pair.Item1) {
					missingOneSide = true;
					if (GUI.Button(new Rect(1065, offset, 100, 30), pair.Item1.ToString())) {
						doSwitch = true;
						context.type = "Switch";
						context.side = "Left";
						context.leftWeapon = pair.Item1;
						context.rightWeapon = Weapon.Relax;
					}
				}
				// Right weapons.
				if (rpgCharacterController.rightWeapon != pair.Item2) {
					missingOneSide = true;
					if (GUI.Button(new Rect(1165, offset, 100, 30), pair.Item2.ToString())) {
						doSwitch = true;
						context.type = "Switch";
						context.side = "Right";
						context.leftWeapon = Weapon.Relax;
						context.rightWeapon = pair.Item2;
					}
				}
				// If at least one side isn't carrying this weapon, show the Dual switch.
				if (missingOneSide) {
					string label = pair.Item1.ToString();
					if (!label.Contains("Shield")) {
						label = label.Replace("Left", "Dual ") + "s";
						if (GUI.Button(new Rect(965, offset, 100, 30), label)) {
							doSwitch = true;
							context.type = "Switch";
							context.side = "Dual";
							context.leftWeapon = pair.Item1;
							context.rightWeapon = pair.Item2;
						}
					}
				}

				offset += 30;
			}
			if (rpgCharacterController.leftWeapon.HasEquippedWeapon()) {
				if (GUI.Button(new Rect(750, offset - 150, 100, 30), "Sheath Left")) {
					doSwitch = true;
					context.type = "Sheath";
					context.side = "Left";
					context.leftWeapon = Weapon.Unarmed;
					context.rightWeapon = Weapon.Relax;
				}
			}
			if (rpgCharacterController.rightWeapon.HasEquippedWeapon()) {
				if (GUI.Button(new Rect(850, offset - 150, 100, 30), "Sheath Right")) {
					doSwitch = true;
					context.type = "Sheath";
					context.side = "Right";
					context.leftWeapon = Weapon.Relax;
					context.rightWeapon = Weapon.Unarmed;
				}
			}

			offset += 30;

			// Check if HipShoot Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.HipShoot)) {
				hipShooting = GUI.Toggle(new Rect(1000, 495, 100, 30), hipShooting, "Hip Shooting");
				if (hipShooting) {
					if (!rpgCharacterController.TryStartAction(HandlerTypes.HipShoot))
					{ rpgCharacterController.TryEndAction(HandlerTypes.HipShoot); }
				}
			}

			// Sheath/Unsheath Hips.
			useHips = GUI.Toggle(new Rect(1000, 260, 100, 30), useHips, "Hips");
			if (useHips) { context.sheathLocation = "Hips"; }
			else { context.sheathLocation = "Back"; }

			// Instant weapon toggle.
			useInstant = GUI.Toggle(new Rect(1000, 310, 100, 30), useInstant, "Instant");
			if (useInstant) { context.type = "Instant"; }

			// Perform the weapon switch.
			if (doSwitch) { 
				rpgCharacterController.TryStartAction(HandlerTypes.SwitchWeapon, context); 
			}
		}
	}
}