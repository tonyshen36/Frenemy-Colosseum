using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;

public class SceneControl : MonoBehaviour
{
    //public UnityEngine.InputSystem.PlayerInput input;
    public List<UnityEngine.InputSystem.PlayerInput> inputs = new List<UnityEngine.InputSystem.PlayerInput>();
    public List<CharacterControl> players = new List<CharacterControl>();
    public SceneNetworkSync networking = null;
    public List<CharacterControl> enemies = new List<CharacterControl>();
    public GameObject friendObject = null;
    public GameObject myObject = null;
    public GameObject firstChest = null;
    public GameObject lastChest = null;
    public bool pveStage = true;
    public Color FriendColor;
    public Color EnemyColor;
    public Color FriendLowHPColor;
    public Color EnemyLowHPColor;
    public GameObject[] mobStages;
    public List<string> selectedAmbush = new List<string>();
    public GameObject PVPBGM;
    public GameObject PVEBGM;
    public GameObject gameOverUI;
    public GameObject menuUI;
    [HideInInspector] public int selectedSkin = -1;

    private void Start()
    {
        networking = GetComponent<SceneNetworkSync>();
    }

    private void Update()
    {
        /*
        // Temporary move it to delete key
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            ResetGame();
        }
        */
    }

    public void ResetGame()
    {
        networking.ResetScene();
    }
    public void AddPlayer(CharacterControl p)
    {
        players.Add(p);
    }

    public void ResetScene()
    {
        PhotonNetwork.LeaveRoom();
        //Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene("Loading");
    }

    public void AddAmbush(string s)
    {
        if (selectedAmbush.Contains(s))
        {
            //selectedAmbush.Remove(s);
            print("Removed Ambush: " + s);
            print(selectedAmbush.Count);
        }
        else
        {
            selectedAmbush.Add(s);
            print("Added Ambush: " + s);
            print(selectedAmbush.Count);
        }
       
    }

    public int PlayersAlive()
    {
        int alivePlayers = 0;
        foreach (CharacterControl player in players)
        {
            if (!player.IsDead()) alivePlayers += 1;
        }
        return alivePlayers;
    }

    public GameObject GetFriendObject()
    {
        if (friendObject != null)
        {
            return friendObject;
        }
        else
        {
            if (players.Count > 1)
            {
                friendObject = players[0].GetComponent<PlayerNetworkSync>().IsMine() ? players[1].gameObject : players[0].gameObject;
                return friendObject;
            }
            else
            {
                return null;
            }
        }
    }

    public GameObject GetMyObject()
    {
        if (myObject != null)
        {
            return myObject;
        }
        else
        {
            myObject = players[0].GetComponent<PlayerNetworkSync>().IsMine() ? players[0].gameObject : players[1].gameObject;
            return myObject;
        }
    }

    public void EnterPVPArea(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
        PVPBGM.SetActive(true);
        PVEBGM.SetActive(false);
        players[0].RPC_OnPVPAreaEnter(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
        players[1].RPC_OnPVPAreaEnter(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
    }

    public void ExitPVPArea(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
        PVPBGM.SetActive(false);
        PVEBGM.SetActive(true);
        players[0].RPC_OnPVEAreaEnter(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
        if (players.Count > 1)
            players[1].RPC_OnPVEAreaEnter(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
    }

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        menuUI.SetActive(false);
    }
}
