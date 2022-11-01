using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MobNetworkSync : MonoBehaviourPunCallbacks
{
    private PhotonView PV;
    private PlayerInfo PI;
    private MobControl mobControl;
    public bool isMob;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        PI = GetComponent<PlayerInfo>();
        mobControl = GetComponent<MobControl>();
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public void TakeDamage(float i)
    {
        PV.RPC("RPC_PlayerTakeDamage", RpcTarget.AllBufferedViaServer, i);
    }

    [PunRPC]
    void RPC_ReceiveMobKey(Dictionary<string, bool> inputs, Vector2 v)
    {
        PI.SetPlayerInputs(inputs, v);
    }

    [PunRPC]
    void RPC_PlayerTakeDamage(float i)
    {
        mobControl.TakeDamage(i, false, true, Vector3.zero);
    }

    private void LateUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPC_ReceiveMobKey", RpcTarget.Others, PI.inputs, PI.inputDirection);
        }
    }
}
