using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkSync : MonoBehaviour
{
    private PhotonView PV;
    private PlayerInfo PI;
    private CharacterControl characterControl;
    public CharacterSkin characterSkin; //Needs to be moved elsewhere later
    private SceneControl sceneControl = null;
    public bool isPlayer = false;

    private int selectedSkin = -1;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        PI = GetComponent<PlayerInfo>();
        sceneControl = (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>();
        characterControl = GetComponent<CharacterControl>();
        if (isPlayer)
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

            if (PV.IsMine && characterSkin != null)
            {
                characterSkin.skinIndex = characterSkin.GetRandomSkin(selectedSkin); 
                print("Selected skin is: " + selectedSkin);
                PV.RPC("RPC_SetSkinIndex", RpcTarget.AllBufferedViaServer, characterSkin.skinIndex);
            }

            if (_cameraWork != null)
            {
                if (PV.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else if (characterSkin != null && _cameraWork == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }
        }
    }
    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }
    public bool IsMine()
    {
        return PV.IsMine;
    }
    [PunRPC]
    void RPC_ReceiveKey(Dictionary<string, bool> inputs, Vector2 v)
    {
        PI.SetPlayerInputs(inputs, v);
    }

    [PunRPC]
    void RPC_SetSkinIndex(int index)
    {
        print("aha! " + selectedSkin);
        characterSkin.skinIndex = index;
        selectedSkin = index;
        characterSkin.SetSkin();
        var outline = this.gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = new Color(1, 0, 0, 1);
        outline.OutlineWidth = 3f;
        outline.enabled = false;
    }

    private void LateUpdate()
    {
        if (isPlayer && PV.IsMine)
        {
            PV.RPC("RPC_ReceiveKey", RpcTarget.AllViaServer, PI.inputs, PI.inputDirection); 
        }
        if (!isPlayer && PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPC_ReceiveKey", RpcTarget.AllViaServer, PI.inputs, PI.inputDirection);
        }
    }

    public void TakeDamage(float i, bool kick, bool dotakeDamage, Vector3 damageSoursePosition)
    {
        PV.RPC("RPC_PlayerTakeDamage", RpcTarget.AllBufferedViaServer, i, kick, dotakeDamage, damageSoursePosition);
    }

    public void UpdatePlayerStates(string upgrade = "", bool add = true) //if add do UpgradePlayerStates(upgrade) if subtract do UpgradePlayerStates(upgrade, false)
    {
        PV.RPC("RPC_UpgradePlayerStates", RpcTarget.AllBufferedViaServer, upgrade, add, true);
    }

    public void SetFriendlyFire(bool flag = true) //if add do UpgradePlayerStates(upgrade) if subtract do UpgradePlayerStates(upgrade, false)
    {
        PV.RPC("RPC_SetFriendlyFire", RpcTarget.AllBufferedViaServer, flag);
    }

    public void OnEnterPVPArea(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
        PV.RPC("RPC_OnEnterPVPArea", RpcTarget.AllBufferedViaServer, mobControl, disableMobControl, nextMobControl, activateNextMobControl);
    }

    public void OnEnterPVEArea(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
        PV.RPC("RPC_OnEnterPVEArea", RpcTarget.AllBufferedViaServer, mobControl, disableMobControl, nextMobControl, activateNextMobControl);
    }

    public void RevivePlayer()
    {
        PV.RPC("RPC_RevivePlayer", RpcTarget.AllBufferedViaServer);
    }

    public void ReplacePlayerStates(List<string> s)
    {
        print(s.Count);
        PV.RPC("RPC_ClearUpgrades", RpcTarget.AllBufferedViaServer);
        foreach (string upgrade in s) PV.RPC("RPC_UpgradePlayerStates", RpcTarget.AllBufferedViaServer, upgrade, true, false);
    }

    public void SetPlayerPosition(Vector3 pos)
    {
        PV.RPC("RPC_SetPlayerPosition", RpcTarget.AllViaServer, pos);
    }

    public void SetHealth(float f, bool fullHealth = false)
    {
        PV.RPC("RPC_SetHealth", RpcTarget.AllBufferedViaServer, f, fullHealth);
    }

    public void PlayActivateAnim(bool doPlay)
    {
        PV.RPC("RPC_PlayActivateAnim", RpcTarget.AllBufferedViaServer, doPlay);
    }

    [PunRPC]
    void RPC_UpgradePlayerStates(string upgrade = "", bool add = true, bool doHUD = true)
    {
        if (upgrade.Length > 0)
        {
            if (add)
            {
                GetComponent<CharacterControl>().AddUpgrade(upgrade, doHUD);
            }
            else
            {
                GetComponent<CharacterControl>().RemoveUpgrade(upgrade);
            }
        }
        GetComponent<CharacterControl>().UpdateStats();
    }

    [PunRPC]
    void RPC_SetFriendlyFire(bool flag)
    {
        GetComponent<CharacterControl>().SetFriendlyFire(flag);
    }

    [PunRPC]
    void RPC_ClearUpgrades()
    {
        GetComponent<CharacterControl>().upgrades.Clear();
        GetComponent<CharacterControl>().UpdateStats();
    }

    [PunRPC]
    void RPC_RevivePlayer()
    {
        GetComponent<CharacterControl>().Revive();
    }

    public void UpdatePlayerWeapon(string weapon = "", bool doHUD = true) 
    {
        PV.RPC("RPC_UpgradePlayerWeapon", RpcTarget.AllBufferedViaServer, weapon, doHUD);
        //RPC_UpgradePlayerWeapon(weapon);
    }

    [PunRPC]
    void RPC_UpgradePlayerWeapon(string weapon = "", bool doHUD = true)
    {
        switch (weapon)
        {
            case "Sword":
                GetComponent<CharacterControl>().SwitchToSword(doHUD);
                break;
            case "Daggers":
                GetComponent<CharacterControl>().SwitchToDualSword(doHUD);
                break;
            case "Spear":
                GetComponent<CharacterControl>().SwitchToSpear(doHUD);
                break;
            case "Axe":
                GetComponent<CharacterControl>().SwitchToAxe(doHUD);
                break;
            case "Mace":
                GetComponent<CharacterControl>().SwitchToMaceShield(doHUD);
                break;
            default:
                print("No Matching Weapon Found");
                break;
        }
        
        InstructionManager im = GetComponent<InstructionManager>();
        if (im.curInstructionState != InstructionManager.instructionState.None)
        {
            im.curInstructionState = InstructionManager.instructionState.PreFirstGate; 
            im.displayNextInstruction = true;
            if (im.curInstructionNum < 3)
            {
                if (im.firstChest != null)
                    im.firstChest.SetActive(true);
            }
            im.curInstructionNum = 3;
            if (im.instructionSet.Contains(im.curInstruction))
            {
                im.interactionControl.HideInstruction1();

                /*
                if (im.firstChest != null)
                {
                    ChestScript cs = im.firstChest.GetComponent<ChestScript>();

                    cs.door1.RaiseGate();
                    if (cs.door2 != null)
                    {
                        cs.door2.RaiseGate();
                    }
                }
                */
            }   
        }
    }

    [PunRPC]
    void RPC_SetPlayerPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    [PunRPC]
    void RPC_OnEnterPVPArea(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
        characterControl.OnPVPAreaEnter(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
    }

    [PunRPC]
    void RPC_OnEnterPVEArea(string mobControl, bool disableMobControl, string nextMobControl, bool activateNextMobControl)
    {
        characterControl.OnPVEAreaEnter(mobControl, disableMobControl, nextMobControl, activateNextMobControl);
    }

    [PunRPC]
    void RPC_PlayerTakeDamage(float i, bool kick, bool doTakeDamage, Vector3 damageSoursePosition)
    {
        characterControl.TakeDamage(i, kick, doTakeDamage, damageSoursePosition);
    }

    [PunRPC]
    void RPC_SetHealth(float f, bool fullHealth = false)
    {
        if (fullHealth) characterControl._SetCurHealth(characterControl.GetMaxHealth());
        else characterControl._SetCurHealth(f);
    }

    public void ReduceStamina(float i, bool attacked = false)
    {
        PV.RPC("RPC_PlayerReduceStamina", RpcTarget.AllBufferedViaServer, i, attacked);
    }

    [PunRPC]
    void RPC_PlayerReduceStamina(float i, bool attacked = false)
    {
        characterControl.ReduceStamina(i, attacked);
    }

    [PunRPC]
    void RPC_PlayActivateAnim(bool doPlay)
    {
        if (doPlay)
            characterControl.PlayActivateAnim();
        else
            characterControl.EndActivateAnim();
    }
}
