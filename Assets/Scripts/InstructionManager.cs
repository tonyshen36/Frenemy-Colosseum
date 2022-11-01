using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionManager : MonoBehaviour
{
    public InteractControl interactionControl;
    [HideInInspector] public string nextTo = "";
    [HideInInspector] public SceneControl sceneControl;
    private RPGCharacterInputSystemController rpgCharacterInputSystemController;
    private RPGCharacterController rpgCharacterController;
    private CharacterControl characterControl;
    public float interactDistance = 3.0f;
    public GameObject firstChest;
    private List<Transform> objectsNextTo;

    private string movementInstruction = "Press WASD to Move";
    private string rollInstruction = "Press Space to Roll";
    private string firstAttackInstruction = "Press J to Attack";
    private string secondAttackInstruction = "Press K to Special Attack";
    private string kickInstruction = "Press L to Kick";
    private string blockInstruction = "Press Shift to Block";
    private string lockInstruction = "Press R to toggle Lock-On";
    private string interactInstruction = "Approach To Chest";
    private string cancelInstruction = "Press F to Skip Tutorial";

    [HideInInspector] public bool displayNextInstruction = true;
    private bool instructionOff = false;
    private bool canCancelInstruction = true;

    public enum instructionState
    {
        PreFirstChest,
        PreFirstGate,
        PreSecondChest,
        PreSecondGate,
        None
    };

    private int PreFirstChestInstructions = 3;
    private int PreFirstGateInstructions = 2;
    private int PreSecondChestInstructions = 2;
    private int PreSecondGateInstructions = 1;

    [HideInInspector] public string curInstruction = "";
    [HideInInspector] public int curInstructionNum = 0;
    [HideInInspector] public List<string> instructionSet = new List<string>();

    public instructionState curInstructionState = instructionState.PreFirstChest;

    private void Start()
    {
        sceneControl = (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>();
        rpgCharacterInputSystemController = GetComponent<RPGCharacterInputSystemController>();
        rpgCharacterController = GetComponent<RPGCharacterController>();
        characterControl = GetComponent<CharacterControl>();
        objectsNextTo = new List<Transform>();
        instructionSet.Add(movementInstruction);
        instructionSet.Add(rollInstruction);
        instructionSet.Add(interactInstruction);
        instructionSet.Add(firstAttackInstruction);
        instructionSet.Add(kickInstruction);
        instructionSet.Add(lockInstruction);
        instructionSet.Add(blockInstruction);
        instructionSet.Add(secondAttackInstruction);
        interactionControl.ShowInstruction2(cancelInstruction, Color.black, 5f);
    }

    doorScript closestDoorScript = null;
    private int archwayDisplayNumber = -1;
    private void FixedUpdate()
    {
        float closestDist = Mathf.Infinity;
        Transform closestObject = null;
        List<Transform> objectsOutOfRange = new List<Transform>();

        foreach (Transform objectNextTo in objectsNextTo)
        {
            if (objectNextTo == null) continue;
            float curDist = Vector3.Distance(transform.position, objectNextTo.position);
            if (curDist > interactDistance || !objectNextTo.gameObject.activeSelf || (objectNextTo.CompareTag("Player") && !objectNextTo.GetComponent<EntityControl>().IsDead()))
            {
                objectsOutOfRange.Add(objectNextTo);
                continue;
            }
            if (curDist < closestDist)
            {
                closestDist = curDist;
                closestObject = objectNextTo;
            }
        }
        if (objectsOutOfRange.Count > 0)
        {
            foreach (Transform objectOutOfRange in objectsOutOfRange)
            {
                objectsNextTo.Remove(objectOutOfRange);
            }
        }

        if (closestObject != null)
        {
            if (closestObject.CompareTag("Player") && nextTo != "Player")
            {
                EntityControl objectEntityControl = closestObject.GetComponent<EntityControl>();
                if (objectEntityControl.IsDead() && objectEntityControl.sceneControl.pveStage)
                {
                    nextTo = "Player";
                    interactionControl.DisplayRevive();
                }
            }
            if (closestObject.CompareTag("Chest") && nextTo != "Chest")
            {
                nextTo = "Chest";
                interactionControl.DisplayChest();
            }
            if (closestObject.CompareTag("Archway") && nextTo != "Archway")
            {
                nextTo = "Archway";
                closestDoorScript = closestObject.GetComponent<doorScript>();
                GameObject friendGameObject = sceneControl.GetFriendObject();

                if (friendGameObject.GetComponent<CharacterControl>().IsDead())
                {
                    archwayDisplayNumber = -1;
                }
                else
                {
                    if (friendGameObject.GetComponent<InstructionManager>().nextTo == "Archway")
                        archwayDisplayNumber = 2;
                    else
                        archwayDisplayNumber = 1;
                }
                interactionControl.DisplayArchway(archwayDisplayNumber);
            }
        }
        else
        {
            if (!instructionSet.Contains(interactionControl.instruction1.text))
            {
                nextTo = "";
                interactionControl.HideInstruction1();
            }
        }

        //Display Instructions
        if (instructionSet.Count > 0 && displayNextInstruction && !instructionOff)
        {
            if (curInstructionState == instructionState.PreFirstChest)
            {
                if (!interactionControl.instruction1.enabled && PreFirstChestInstructions > 0)
                {
                    curInstruction = instructionSet[curInstructionNum];
                    interactionControl.ShowInstruction1(instructionSet[curInstructionNum]);
                    PreFirstChestInstructions--;
                    curInstructionNum++;
                    displayNextInstruction = false;
                }
            }
            else if (curInstructionState == instructionState.PreFirstGate)
            {
                if (!interactionControl.instruction1.enabled && PreFirstGateInstructions > 0)
                {
                    curInstruction = instructionSet[curInstructionNum];
                    interactionControl.ShowInstruction1(instructionSet[curInstructionNum]);
                    PreFirstGateInstructions--;
                    curInstructionNum++;
                    displayNextInstruction = false;
                }
            }
            else if (curInstructionState == instructionState.PreSecondChest)
            {
                if (!interactionControl.instruction1.enabled && PreSecondChestInstructions > 0)
                {
                    curInstruction = instructionSet[curInstructionNum];
                    interactionControl.ShowInstruction1(instructionSet[curInstructionNum]);
                    PreSecondChestInstructions--;
                    curInstructionNum++;
                    displayNextInstruction = false;
                }
            }
            else if (curInstructionState == instructionState.PreSecondGate)
            {
                if (!interactionControl.instruction1.enabled && PreSecondGateInstructions > 0)
                {
                    curInstruction = instructionSet[curInstructionNum];
                    interactionControl.ShowInstruction1(instructionSet[curInstructionNum]);
                    PreSecondGateInstructions--;
                    curInstructionNum++;
                    displayNextInstruction = false;
                }
            }
        }
    }

    private void Update()
    {
        if (!instructionOff)
        {
            if (rpgCharacterInputSystemController.inputMovement != Vector2.zero && curInstruction == movementInstruction)
            {
                interactionControl.HideInstruction1();
                displayNextInstruction = true;
            }
            if (rpgCharacterInputSystemController.inputRoll && curInstruction == rollInstruction)
            {
                interactionControl.HideInstruction1();
                displayNextInstruction = true;
                if (rpgCharacterInputSystemController.transform.GetComponent<CharacterControl>().PV.IsMine)
                    firstChest.SetActive(true);
            }
            if (rpgCharacterInputSystemController.inputAttackL && curInstruction == firstAttackInstruction)
            {
                interactionControl.HideInstruction1();
                /*
                if (rpgCharacterInputSystemController.transform.GetComponent<CharacterControl>().PV.IsMine)
                    firstChest.SetActive(true);
                */
                displayNextInstruction = true;
            }
            if (rpgCharacterInputSystemController.inputAttackR && curInstruction == secondAttackInstruction)
            {
                interactionControl.HideInstruction1();
                displayNextInstruction = true;
                curInstructionState = instructionState.None;
            }
            if (rpgCharacterInputSystemController.inputKick && curInstruction == kickInstruction)
            {
                interactionControl.HideInstruction1();
                if (rpgCharacterInputSystemController.transform.GetComponent<CharacterControl>().PV.IsMine)
                    firstChest.SetActive(true);
                displayNextInstruction = true;
            }
            if (rpgCharacterInputSystemController.inputBlock && curInstruction == blockInstruction)
            {
                interactionControl.HideInstruction1();
                displayNextInstruction = true;
            }
            if (rpgCharacterInputSystemController.inputAim && curInstruction == lockInstruction)
            {
                interactionControl.HideInstruction1();
                displayNextInstruction = true;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F) && interactionControl.instruction2.text == "Press F to Skip Tutorial" && interactionControl.instruction2.enabled)
        {
            if (instructionSet.Contains(curInstruction))
            {
                if (instructionSet.Contains(curInstruction))
                    interactionControl.HideInstruction1();
                if (interactionControl.instruction2.text == cancelInstruction)
                    interactionControl.ForceHideInstruction2();
                displayNextInstruction = false;
            }
            instructionOff = true;
            firstChest.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == null || objectsNextTo.Contains(other.transform)) return;
        if (other.CompareTag("Chest") && other.gameObject.activeSelf && nextTo != "Chest")
        {
            objectsNextTo.Add(other.transform);
        }
        if (other.CompareTag("Archway") && nextTo != "Archway")
        {
            objectsNextTo.Add(other.transform);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == null || objectsNextTo.Contains(collision.transform)) return;
        if (collision.transform.CompareTag("Player") && nextTo != "Player")
        {
            objectsNextTo.Add(collision.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!objectsNextTo.Contains(other.transform) || other.gameObject.activeSelf) return;
        objectsNextTo.Remove(other.transform);
    }

    public void DoInteract()
    {
        List<Transform> markedToDstroy = new List<Transform>();
        foreach(Transform objectNextTo in objectsNextTo)
        {
            if (objectNextTo == null) continue;
            if (objectNextTo.CompareTag(nextTo))
            {
                switch (nextTo)
                {
                    case "Chest":
                        print("enabling chest");
                        if (!objectNextTo.GetComponent<ChestScript>().DoInteract(gameObject))
                        {
                            interactionControl.ShowInstruction2("Waiting for P2 to Join", Color.black);
                        }
                        else
                        {
                            if (interactionControl.instruction1.text == "Press F To Open")
                            {
                                interactionControl.HideInstruction1();
                            }
                            characterControl.playerNetworking.PlayActivateAnim(true);
                            objectNextTo.GetComponent<ChestScript>().enabled = false;
                            markedToDstroy.Add(objectNextTo);
                            nextTo = "";
                        }
                        break;
                    case "Archway":
                        print("using archway");
                        if (!objectNextTo.GetComponent<doorScript>().DoInteract())
                        {
                            //if (sceneControl.GetFriendObject().GetComponent<CharacterControl>().IsDead())
                                //interactionControl.ShowInstruction2("Revive Your Teammate To Pass");
                        }
                        break;
                    case "Player":
                        //If next to dead player

                        CharacterControl friendCC = sceneControl.GetFriendObject().GetComponent<CharacterControl>();

                        if (friendCC.IsDead() && friendCC.sceneControl.pveStage)
                        {
                            rpgCharacterController.StartAction(HandlerTypes.EmoteCombat, EmoteType.Activate);

                            characterControl.RemoveRandomUpgrade();
                            characterControl.playerNetworking.PlayActivateAnim(true);
                            friendCC.RPC_Revive();
                            //Remove one random upgrade!
                            //interacting = true;
                        }

                        break;
                    default:
                        break;
                }
            } 
        }

        foreach (Transform t in markedToDstroy)
        {
            if (objectsNextTo.Contains(t))
                objectsNextTo.Remove(t);
            GameObject.Destroy(t.gameObject);
        }
    }
}
