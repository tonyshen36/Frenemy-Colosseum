using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestEffect : MonoBehaviour
{
    private Outline outline;
    public float duration = 1f;
    private bool fadedIn = false;
    private bool fadedOut = false;
    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        outline = GetComponent<Outline>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!fadedIn)
        {
            timer += Time.deltaTime / duration;
            outline.OutlineColor = new Color(outline.OutlineColor.r, outline.OutlineColor.g, outline.OutlineColor.b, Mathf.Lerp(0, 1, timer));
            if (timer >= 1)
            {
                fadedIn = true;
                fadedOut = false;
                timer = 0;
            }
        }
        else if (!fadedOut)
        {
            timer += Time.deltaTime / duration;
            outline.OutlineColor = new Color(outline.OutlineColor.r, outline.OutlineColor.g, outline.OutlineColor.b, Mathf.Lerp(1, 0, timer));
            if (timer >= 1)
            {
                fadedOut = true;
                fadedIn = false;
                timer = 0;
            }
        }
    }
}
