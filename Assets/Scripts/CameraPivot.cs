using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    public Transform m_player;
    public float camOffset = 2.0f;
    public float heightOffset = 1.0f;
    public CharacterControl characterControl = null;

    // Start is called before the first frame update
    void Start()
    {
        if (!characterControl.PV.IsMine) enabled = false;
        transform.localPosition = new Vector3(0, heightOffset, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player == null) return;
        Vector3 player_position = new Vector3(m_player.position.x, m_player.position.y + heightOffset, m_player.position.z);
        Vector3 pivot_position = new Vector3(transform.position.x, m_player.position.y + heightOffset, transform.position.z);

        if (Vector3.Distance(player_position, pivot_position) > camOffset)
        {
            Vector3 positionOffset = (pivot_position - player_position).normalized * camOffset;
            transform.position = player_position + positionOffset;
        }
    }
}
