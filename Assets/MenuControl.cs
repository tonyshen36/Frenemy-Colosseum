using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    public GameObject menuUI;
    public GameObject gameOverUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameOverUI.activeSelf)
            {
                if (menuUI.activeSelf)
                {
                    menuUI.SetActive(false);
                }
                else
                {
                    menuUI.SetActive(true);
                }
            }
        }
    }
}
