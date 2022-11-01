using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    BoxCollider boxCollider;
    MeshRenderer meshRenderer;
    TempUpgradeManager upgradeManager;
    SceneControl sceneControl;
    public GameObject lid;
    public bool weaponBox = false;
    public bool attackBox = false;
    public BoxCollider chestTrigger;

    public AudioClip spawnAudio;
    public AudioClip openAudio;
    public AudioSource audioSource;
    public doorScript door1;
    public doorScript door2;

    public int islandNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider.enabled = false;
        meshRenderer.enabled = false;
        lid.SetActive(false);
        upgradeManager = (GameObject.FindObjectsOfType<TempUpgradeManager>())[0].GetComponent<TempUpgradeManager>();
        sceneControl = (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>();
        ShowTreasureChest();
        
        /*
        door1.RaiseGate();
        if (door2 != null)
        {
            door2.RaiseGate();
        }
        */
    }

    private void OnEnable()
    {
        audioSource.PlayOneShot(spawnAudio, 0.7f);
    }

    public void ShowTreasureChest()
    {
        lid.SetActive(true);
        boxCollider.enabled = true;
        meshRenderer.enabled = true;
    }

    public bool DoInteract(GameObject m_player)
    {
        
        if (sceneControl.pveStage && sceneControl.players.Count < 2) return false;

        audioSource.PlayOneShot(openAudio, 0.7f);
        int chance = GetChanceOfFriendlyFireProtection();

        if (islandNum == 5 && upgradeManager.DisplayEndSceneOptions(m_player, chance))
        {
            return true;
        }
        else if ((weaponBox && upgradeManager.DisplayWeaponOptions(m_player, chance)) || (attackBox && upgradeManager.DisplaySecondAttack(m_player, chance)) || (!weaponBox && upgradeManager.DisplayUpgradeOptions(m_player, chance)))
        {
            door1.RaiseGate();
            if (door2 != null)
            {
                door2.RaiseGate();
            }
            return true;
        }
        return true;
    }

    public int GetChanceOfFriendlyFireProtection()
    {
        switch (islandNum)
        {
            case 0:
                return 100;
            case 1:
                return 92;
            case 2:
                return 82;
            case 3:
                return 65;
            case 4:
                return 0;
            case 5:
                return 0;
            default:
                return 0;
        }
    }
}
