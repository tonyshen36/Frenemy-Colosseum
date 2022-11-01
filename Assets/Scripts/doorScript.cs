using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorScript : MonoBehaviour
{
    public List<Transform> players = new List<Transform>();
    public GameObject nextEntrance;
    public GameObject PVPEntrance1;
    public GameObject PVPEntrance2;
    public bool setFriendlyFire = false;

    private Vector3 targetPosition = new Vector3();
    private BoxCollider boxCollider;
    public bool raiseGate = false;

    public GameObject MobControl = null;
    public bool disableMobControl = false;
    public GameObject nextMobControl = null;
    public bool activateNextMobControl = false;

    public GameObject portalOpenVFX;
    public GameObject portalVFX;
    public RewindParticle rewindParticle;

    public AudioClip doorRisedClip;
    private AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position + Vector3.up * 10;
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false;
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (raiseGate)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.025f);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                portalOpenVFX.SetActive(true);
                portalVFX.SetActive(true);
                raiseGate = false;
                boxCollider.enabled = true;
            }
        }
        /*if (players.Count >= 1)
        {
            Vector3 prevPos = players[0].position;
            players[0].position = nextEntrance.transform.position;
            players[0].position += nextEntrance.transform.forward * 3.0f;
            players[0].position -= new Vector3((transform.position - prevPos).x, 0, 0);
            this.enabled = false;
        }*/        
    }

    public void RaiseGate()
    {
        raiseGate = true;
        audio.PlayOneShot(doorRisedClip);
    }

    public bool DoInteract()
    {
        if (players.Count < 2) return false;
        Vector3 p1Position = players[0].position;
        Vector3 p2Position = players[1].position;
        if (nextEntrance != null)
        {
            p1Position = nextEntrance.transform.position;
            p1Position += nextEntrance.transform.forward * 3.0f;
            p1Position -= Vector3.Dot(transform.position - players[0].position, transform.right) * transform.right;
            p2Position = nextEntrance.transform.position;
            p2Position += nextEntrance.transform.forward * 3.0f;
            p2Position -= Vector3.Dot(transform.position - players[1].position, transform.right) * transform.right;
        }
        else
        {
            p1Position = PVPEntrance1.transform.position;
            p1Position += PVPEntrance1.transform.forward * 3.0f;
            p1Position -= Vector3.Dot(transform.position - players[0].position, transform.right) * transform.right;
            p2Position = PVPEntrance2.transform.position;
            p2Position += PVPEntrance2.transform.forward * 3.0f;
            p2Position -= Vector3.Dot(transform.position - players[1].position, transform.right) * transform.right;
        }
        
        /*
        if (Vector3.Distance(p1Position, p2Position) < 1)
        {
            p1Position += nextEntrance.transform.forward * 2.0f;
        }
        */
        if (setFriendlyFire)
        {
            (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>().EnterPVPArea(MobControl.name, disableMobControl, nextMobControl.name, activateNextMobControl);
        }
        if (!setFriendlyFire)
        {
            (GameObject.FindObjectsOfType<SceneControl>())[0].GetComponent<SceneControl>().ExitPVPArea(MobControl.name, disableMobControl, nextMobControl.name, activateNextMobControl);
        }
        players[0].GetComponent<CharacterControl>().SetPosition(p1Position);
        players[1].GetComponent<CharacterControl>().SetPosition(p2Position);

        
        return true;
        //this.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("added player");
            players.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (players.Contains(other.transform))
            {
                players.Remove(other.transform);
            }
        }
    }

    private void disableVFX()
    {
        rewindParticle.enabled = false;
        portalOpenVFX.SetActive(false);
        portalVFX.SetActive(false);
    }
}
