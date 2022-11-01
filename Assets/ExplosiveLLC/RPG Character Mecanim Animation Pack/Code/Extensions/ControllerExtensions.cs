using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims.Extensions
{
    public static class ControllerExtensions
    {
        public static void DebugController(this RPGCharacterController controller)
        {
            Debug.Log("CONTROLLER SETTINGS---------------------------");
            Debug.Log("AnimationSpeed: " + controller.animationSpeed);
            Debug.Log("headLook: " + controller.headLook);
            Debug.Log("isHeadlook: " + controller.isHeadlook);
            Debug.Log("ladder: " + controller.ladder);
            Debug.Log("cliff: " + controller.cliff);
            Debug.Log("canAction: " + controller.canAction);
            Debug.Log("canFace: " + controller.canFace);
            Debug.Log("canMove: " + controller.canMove);
            Debug.Log("canStrafe: " + controller.canStrafe);
            Debug.Log("acquiringGround: " + controller.acquiringGround);
            Debug.Log("maintainingGround: " + controller.maintainingGround);
            Debug.Log("isAiming: " + controller.isAiming);
			Debug.Log("isAttacking: " + controller.isAttacking);
			Debug.Log("isBlocking: " + controller.isBlocking);
            Debug.Log("isCasting: " + controller.isCasting);
            Debug.Log("isClimbing: " + controller.isClimbing);
            Debug.Log("isCrouching: " + controller.isCrouching);
			Debug.Log("isCrawling: " + controller.isCrawling);
			Debug.Log("isDead: " + controller.isDead);
            Debug.Log("isFacing: " + controller.isFacing);
            Debug.Log("isFalling: " + controller.isFalling);
            Debug.Log("isHipShooting: " + controller.isHipShooting);
            Debug.Log("isIdle: " + controller.isIdle);
            Debug.Log("isInjured: " + controller.isInjured);
			Debug.Log("Aiming: " + controller.isAiming);
            Debug.Log("isMoving: " + controller.isMoving);
            Debug.Log("isNavigating: " + controller.isNavigating);
            Debug.Log("isNearCliff: " + controller.isNearCliff);
            Debug.Log("isNearLadder: " + controller.isNearLadder);
            Debug.Log("isRelaxed: " + controller.isRelaxed);
            Debug.Log("isRolling: " + controller.isRolling);
            Debug.Log("isKnockback: " + controller.isKnockback);
            Debug.Log("isKnockdown: " + controller.isKnockdown);
            Debug.Log("isSitting: " + controller.isSitting);
            Debug.Log("isSpecial: " + controller.isSpecial);
            Debug.Log("isSprinting: " + controller.isSprinting);
            Debug.Log("isStrafing: " + controller.isStrafing);
            Debug.Log("isSwimming: " + controller.isSwimming);
            Debug.Log("isTalking: " + controller.isTalking);
            Debug.Log("moveInput: " + controller.moveInput);
            Debug.Log("aimInput: " + controller.aimInput);
            Debug.Log("jumpInput: " + controller.jumpInput);
            Debug.Log("cameraRelativeInput: " + controller.cameraRelativeInput);
            Debug.Log("_bowPull: " + controller.bowPull);
            Debug.Log("rightWeapon: " + controller.rightWeapon);
            Debug.Log("leftWeapon: " + controller.leftWeapon);
            Debug.Log("hasRightWeapon: " + controller.hasRightWeapon);
            Debug.Log("hasLeftWeapon: " + controller.hasLeftWeapon);
            Debug.Log("hasDualWeapons: " + controller.hasDualWeapons);
            Debug.Log("hasTwoHandedWeapon: " + controller.hasTwoHandedWeapon);
            Debug.Log("hasShield: " + controller.hasShield);
        }
    }
}