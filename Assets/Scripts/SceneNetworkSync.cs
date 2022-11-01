using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SceneNetworkSync : MonoBehaviour
{
    private SceneControl sceneControl = null;
    private PhotonView PV = null;
    // Start is called before the first frame update
    void Start()
    {
        sceneControl = GetComponent<SceneControl>();
        PV = GetComponent<PhotonView>();
    }

    public void ResetScene()
    {
        PV.RPC("RPC_ResetScene", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void RPC_ResetScene()
    {
        sceneControl.ResetScene();
    }
}
