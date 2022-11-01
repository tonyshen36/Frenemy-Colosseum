using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public Dictionary<string, bool> inputs = new Dictionary<string, bool>();
    public Vector2 inputDirection = new Vector2();

    private void Start()
    {

        if(!inputs.ContainsKey("inputAttackL")) inputs.Add("inputAttackL", false);
        if (!inputs.ContainsKey("inputAttackR")) inputs.Add("inputAttackR", false);
        if (!inputs.ContainsKey("inputRoll")) inputs.Add("inputRoll", false);
        if (!inputs.ContainsKey("inputBlock")) inputs.Add("inputBlock", false);
        if (!inputs.ContainsKey("inputFace")) inputs.Add("inputFace", false);
        if (!inputs.ContainsKey("inputAim")) inputs.Add("inputAim", false);
        if (!inputs.ContainsKey("inputKick")) inputs.Add("inputKick", false);
        if (!inputs.ContainsKey("doStrafe")) inputs.Add("doStrafe", false);
        if (!inputs.ContainsKey("inputInteract")) inputs.Add("inputInteract", false);
    }

    public void UpdatePlayerInputs(string s, bool b)
    {
        inputs[s] = b;
    }

    public void UpdatePlayerInputs(Vector2 v)
    {
        inputDirection = v;
    }

    public bool GetPlayerInput(string s)
    {
        return inputs[s];
    }

    public Vector2 GetPlayerInputDirection()
    {
        return inputDirection;
    }

    public void SetPlayerInputs(Dictionary<string, bool> _inputs, Vector2 v)
    {
        inputs = _inputs;
        inputDirection = v;
    }
}