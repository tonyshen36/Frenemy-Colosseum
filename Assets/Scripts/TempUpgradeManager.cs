using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCharacterAnims;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TempUpgradeManager : MonoBehaviour
{
    //public UnityEngine.InputSystem.PlayerInput input;
    public GameObject buttonObj;
    public EventSystem eventSystem;
    public SceneControl sceneControl;

    public Dictionary<string, float> upgrades = new Dictionary<string, float>() { { "Weapon Size Up", 0.4f }, { "Movement Speed Up", 1.6f }, { "Health Up", 0.3f }, { "Weapon Damage Up", 1.4f }, { "Attack Speed Up", 0.3f }, { "Final Battle: Swap Weapon", 0.0f }, { "Final Battle: Swap Upgrades", 0.0f }, { "Final Battle: Swap Health", 0.0f } };
    private List<string> weapons = new List<string> { "Sword", "Daggers", "Spear", "Axe" };
    private List<string> secondAttack = new List<string> { "Long Attack", "Heavy Attack" };
    private List<string> gameEndOptions = new List<string> { "Exit Game", "Return To Menu" };
    public List<string> chosenAmbush = new List<string>();
    private List<GameObject> curButtons = new List<GameObject>();
    public Text instruction;
    public Text protectionWarning;
    private Vector3 instuctionOgSize;
    private bool gameEnded = false;

    private bool protectionBroken = false;

    private int actionPlayerIndex = 1; //The index of player taking the action command, needs to be set before calling

    // Start is called before the first frame update
    void Start()
    {
        sceneControl = (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>();
        instruction.rectTransform.position = new Vector2((Screen.width / 2.0f), Screen.height / 6.0f * 5.0f);
        instruction.fontSize = (int)(Screen.width / 60.0f);
        instruction.gameObject.SetActive(false);
        instuctionOgSize = instruction.transform.localScale;
    }

    private bool sizeGrowing = true;
    public float pulseEveryXSec = 5.0f;
    public float pulseMaxSizeRatio = 2.0f;
    public float curSizeRatio = 1.0f;
    void FixedUpdate()
    {
        if (instruction.gameObject.activeSelf)
        {
            if (sizeGrowing)
            {
                curSizeRatio += (pulseMaxSizeRatio - 1) / (50 * pulseEveryXSec / 2);
                if (curSizeRatio > pulseMaxSizeRatio)
                {
                    curSizeRatio = pulseMaxSizeRatio;
                    sizeGrowing = false;
                }
            }
            else
            {
                curSizeRatio -= (pulseMaxSizeRatio - 1) / (50 * pulseEveryXSec / 2);
                if (curSizeRatio < 1.0f)
                {
                    curSizeRatio = 1.0f;
                    sizeGrowing = true;
                }
            }
            instruction.transform.localScale = instuctionOgSize * curSizeRatio;
            float c_black = 0.55f * (1 - (pulseMaxSizeRatio - curSizeRatio) / (pulseMaxSizeRatio - 1));
            instruction.color = new Color(c_black, c_black, c_black);
        }

        if (eventSystem.currentSelectedGameObject != null)
        {
            Vector3 tempV3 = eventSystem.currentSelectedGameObject.transform.position;
            TempButton selectedButton = eventSystem.currentSelectedGameObject.GetComponent<TempButton>();
            RectTransform buttonRectTrans = eventSystem.currentSelectedGameObject.GetComponent<RectTransform>();
            protectionWarning.transform.position = new Vector3(Screen.width / 2, Screen.height *.3f);
            protectionWarning.text = selectedButton.friendlyFireProtection || protectionBroken || gameEnded ? "" : "Breaks Friendly Damage Protection";
            protectionWarning.color = selectedButton.friendlyFireProtection ? sceneControl.FriendColor : sceneControl.EnemyColor;
        }
    }

    public bool DisplayUpgradeOptions(GameObject curPlayer, float protectionChance)
    {
        string curInstruction = "Choosing Upgrade For Your Friend";
        return DisplayOptions(curPlayer, protectionChance, curInstruction, 3, 3);
    }

    public bool DisplaySecondAttack(GameObject curPlayer, float protectionChance)
    {
        string curInstruction = "Choosing Special Attack For Your Friend";
        return DisplayOptions(curPlayer, protectionChance, curInstruction, 2, 2);
    }

    public bool DisplayWeaponOptions(GameObject curPlayer, float protectionChance)
    {
        string curInstruction = "Choosing Special Attack For Your Friend";
        return DisplayOptions(curPlayer, protectionChance, curInstruction, 3, 1);
    }

    public bool DisplayEndSceneOptions(GameObject curPlayer, float protectionChance)
    {
        string curInstruction = "Congratulations, You've Survived the Colosseum";
        return DisplayOptions(curPlayer, protectionChance, curInstruction, 2, 4);
    }

    public bool DisplayOptions(GameObject curPlayer, float protectionChance, string curInstruction, int numberOfItems, int chestType)
    {
        PlayerNetworkSync curNetworking = curPlayer.GetComponent<PlayerNetworkSync>();
        if (curNetworking && curNetworking.GetComponent<PlayerNetworkSync>().IsMine())
        {
            CharacterControl playerCC = curPlayer.GetComponent<CharacterControl>();
            playerCC.DisablePlayerInput();
            instruction.gameObject.SetActive(true);
            protectionWarning.gameObject.SetActive(true);
            instruction.text = curInstruction;

            if (playerCC.hostile)
            {
                protectionBroken = true;
                protectionChance = 0;
            }

            List<string> displayedChoice = new List<string>();
            List<string> choiceKeys = new List<string>();
            if (chestType == 1) //Weapon
            {
                choiceKeys = weapons;
            }
            if (chestType == 2) //Attack
            {
                choiceKeys = secondAttack;
            }
            if (chestType == 3) //Upgrade
            {
                choiceKeys = new List<string>(upgrades.Keys);
            }
            if (chestType == 4)
            {
                gameEnded = true;
                choiceKeys = gameEndOptions;
            }

            for (int i = 0; i < numberOfItems; ++i)
            {
                Vector3 tempPos = new Vector3(Screen.width / (numberOfItems+1) * ((i % numberOfItems) + 1), Screen.height / 2, 0);
                GameObject temp = GameObject.Instantiate(buttonObj, transform);

                int j = Random.Range(0, choiceKeys.Count);
                while (displayedChoice.Contains(choiceKeys[j]) || chosenAmbush.Contains(choiceKeys[j]))
                {
                    j = Random.Range(0, choiceKeys.Count);
                }
                displayedChoice.Add(choiceKeys[j]);

                temp.transform.position = tempPos;
                TempButton buttonScript = temp.GetComponent<TempButton>();
                buttonScript.myText.text = choiceKeys[j];
                buttonScript.myText.fontSize = (int)(Screen.width / 60.0f);
                buttonScript.SetFriendlyProtection(Random.Range(0, 100) < protectionChance, gameEnded);
                RectTransform buttonRectTrans = temp.GetComponent<RectTransform>();
                buttonRectTrans.sizeDelta = new Vector2(Screen.height / 6.0f, Screen.height / 6.0f);
                curButtons.Add(temp);
                if (i == 0)
                {
                    eventSystem.SetSelectedGameObject(temp);
                    instruction.transform.position = new Vector3(Screen.width / 2, Screen.height * .7f);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public void ButtonClicked(string s, bool friendlyFireProtection)
    {
        if (s == "Exit Game") Application.Quit();
        if (s == "Return To Menu") sceneControl.ResetGame();

        PlayerNetworkSync playerNetworkSync = sceneControl.GetFriendObject().GetComponent<PlayerNetworkSync>();
        playerNetworkSync.SetFriendlyFire(!friendlyFireProtection);
        sceneControl.GetMyObject().GetComponent<CharacterControl>().EnablePlayerInput();
        sceneControl.GetMyObject().GetComponent<PlayerNetworkSync>().PlayActivateAnim(false);
        instruction.gameObject.SetActive(false);
        protectionWarning.gameObject.SetActive(false);


        if (weapons.Contains(s))
        {
            playerNetworkSync.UpdatePlayerWeapon(s);
            
        }
        else if (upgrades.ContainsKey(s))
        {
            playerNetworkSync.UpdatePlayerStates(s);
            
        }
        else if (secondAttack.Contains(s))
        {
            playerNetworkSync.UpdatePlayerStates(s);
        }
        
        
        ClearButtons();
        //Disable my player input control
    }

    void ClearButtons()
    {
        foreach (GameObject o in curButtons)
        {
            Destroy(o);
        }
    }
}
