using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvPWarningTrigger : MonoBehaviour
{
    public GameObject text;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            text.SetActive(true);
        }
    }
}
