using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoadInput : MonoBehaviour
{
    public InputActionAsset actions;
    // Start is called before the first frame update
    void Awake()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        string json = JsonUtility.ToJson(actions["Roll"]);
    }
}