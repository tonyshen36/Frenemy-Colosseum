using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobManager : MonoBehaviour
{
    public GameObject chest;
    public List<CharacterControl> mobs = new List<CharacterControl>();
    public int remainingMob;

    private void Start()
    {
        remainingMob = mobs.Count;
    }
    // Update is called once per frame
    void Update()
    {
        if(remainingMob == 0)
        {
            chest.SetActive(true);
            this.enabled = false;
        }
        else
        {
            for (int i = 0; i < mobs.Count; i++)
            {
                if (mobs[i].IsDead())
                {
                    mobs.RemoveAt(i);
                    i--;
                    remainingMob--;
                }
            }
        }
    }
}
