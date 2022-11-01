using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject mobPrefab;
    public List<Transform> locations;
    public GameObject chest;

    private List<GameObject> mobs;

    private void Start()
    {
        mobs = new List<GameObject>();
        SpawnMobs();
    }

    public void SpawnMobs()
    {
        for(int i = 0; i < locations.Count; i++)
        {
            GameObject mob = Instantiate(mobPrefab, locations[i].position, new Quaternion(0, 180, 0, 1));
            mob.GetComponentInChildren<CharacterControl>().SwitchToSword();
            mobs.Add(mob);
        }
    }

    private void Update()
    {
        if (mobs.Any())
        {
            foreach(GameObject mob in mobs)
            {
                if (mob.GetComponentInChildren<CharacterControl>().IsDead())
                {
                    mobs.Remove(mob);
                }
            }
        } 
        else
        {
            chest.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
