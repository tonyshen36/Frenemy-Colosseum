using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkin : MonoBehaviour
{
    public List<GameObject> skins = new List<GameObject>();
    SceneControl sceneControl;

    public bool randomSkin = true;
    public int skinIndex = 0;
    // Start is called before the first frame update
    void Awake()
    {
        /*
        //sceneControl = (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>();
        if (randomSkin)
        {
            
            skinIndex = Random.Range(0, skins.Count);
            while (false && sceneControl.selectedSkin.Contains(skinIndex))
            {
                if (sceneControl.selectedSkin.Count >= skins.Count)
                {
                    break;
                }
                skinIndex = Random.Range(0, skins.Count - 1);
            }
            for (int i = 0; i < skins.Count; ++i)
            {
                if (i == skinIndex) skins[i].SetActive(true);
                else skins[i].SetActive(false);
                //if (sceneControl) sceneControl.selectedSkin.Add(i);
            }
        }
        */
    }

    public void SetSkin()
    {
        for (int i = 0; i < skins.Count; ++i)
        {
            if (i == skinIndex) skins[i].SetActive(true);
            else skins[i].SetActive(false);
            //if (sceneControl) sceneControl.selectedSkin.Add(i);
        }
    }

    public int GetRandomSkin(int exclude = -1)
    {
        if (exclude == -1) return Random.Range(0, skins.Count);
        int curSkin = Random.Range(0, skins.Count);
        while (curSkin == exclude)
        {
            curSkin = Random.Range(0, skins.Count);
        }
        return curSkin;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
