using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerJoinController : MonoBehaviour
{
    public Transform[] locations;
    public GameObject camera;
    public GameObject p1CheckMark;
    public GameObject p2CheckMark;
    public Text p1Check;
    public Text p2Check;
    public GameObject backgroundImage;
    public GameObject P1;
    public GameObject P2;

    public int playerCount = 0;


    private void OnPlayerJoined(UnityEngine.InputSystem.PlayerInput playerInput)
    {
        playerInput.transform.position = locations[playerCount].position;
        if (playerCount == 0)
        {
            p1CheckMark.SetActive(true);
            p1Check.text = "P1 Ready";
        }
        else
        {
            p2CheckMark.SetActive(true);
            p2Check.text = "P2 Ready";
            backgroundImage.SetActive(false);
            P1.SetActive(false);
            P2.SetActive(false);
        }
        playerCount++;
        camera.SetActive(false);
        Debug.Log("joined");
    }
}
