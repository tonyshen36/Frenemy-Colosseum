using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempButton : MonoBehaviour
{
    public Text myText;
    private TempUpgradeManager upgradeManager;
    Button btn;
    public bool friendlyFireProtection = false;

    void Start()
    {
        btn = gameObject.GetComponent<Button>();
        upgradeManager = transform.parent.gameObject.GetComponent<TempUpgradeManager>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        upgradeManager.ButtonClicked(myText.text, friendlyFireProtection);
        btn.interactable = false;
    }

    public void SetFriendlyProtection(bool flag, bool gameEnded)
    {
        
        friendlyFireProtection = flag;
        if (gameEnded)
            myText.color = Color.black;
        else
        {
            SceneControl sceneControl = (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>();
            myText.color = friendlyFireProtection ? sceneControl.FriendColor : sceneControl.EnemyColor;
        }
    }
}
