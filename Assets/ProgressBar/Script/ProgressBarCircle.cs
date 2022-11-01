using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]

public class ProgressBarCircle : MonoBehaviour
{
    [Header("Bar Setting")]
    public Color BarColor;
    public Color BarBackGroundColor;
    public Sprite BarBackGroundSprite;
    [Range(1f, 100f)]
    public int Alert = 20;
    public Color BarAlertColor;

    [Header("Sound Alert")]
    public AudioClip sound;
    public bool repeat = false;
    public float RepearRate = 1f;

    public Image bar;
    public Image barBackground;
    public Image Mask;
    private float nextPlay;
    private AudioSource audiosource;
    private Text txtTitle;
    private float barValue;

    public void SetBarValue(float val)
    {
        val = Mathf.Clamp(val, 0, 100);
        barValue = val;
        UpdateValue(barValue);
    }

    bool DoFlash = false;
    bool flashing = false;
    bool colorUp = false;
    float deltaG = 0;
    float deltaB = 0;
    public float flashFrames = 12;
    private void FixedUpdate()
    {
        if (DoFlash)
        {
            colorUp = true;
            deltaG = barBackground.color.g / flashFrames;
            deltaB = barBackground.color.b / flashFrames;
            DoFlash = false;
            flashing = true;
            barBackground.color = Color.white;
            print("aha!");
        }
        if (flashing)
        {
            Color c = barBackground.color;
            if (colorUp)
            {
                print("Coloring Up");
                c.g -= deltaG;
                c.b -= deltaB;
                if (c.g < 0)
                {
                    c.g = 0;
                    c.b = 0;
                    colorUp = false;
                }
            }
            else
            {
                print("Coloring Down");
                c.g += deltaG;
                c.b += deltaB;
                if (c.g > .75f)
                {
                    c.g = 1;
                    c.b = 1;
                    flashing = false;
                }
            }
            barBackground.color = c;
        }
    }

    void UpdateValue(float val)
    {
        
        bar.fillAmount = val / 100.0f;

        if (val <= 0) return;

        
        if (Alert >= val)
        {
            barBackground.color = new Color(1, val/Alert, val / Alert);
        }
        /*
        else
        {
            barBackground.color = BarBackGroundColor;
        }
        */
    }

    public void ThrowWarning()
    {
        StartCoroutine(Flash(0.15f, true));
    }

    IEnumerator Flash(float flashInterval, bool isOn)
    {
        yield return new WaitForSeconds(flashInterval);
        if (isOn)
        {
            barBackground.color = new Color(1, 0, 0);
        }
        else
        {
            barBackground.color = new Color(1, 1, 1);
        }

        if (barValue <= 0)
        {
            isOn = !isOn;
            StartCoroutine(Flash(flashInterval, isOn));
        }
    }

    public void TakeStaminaDamageAnimation()
    {
        DoFlash = true;
    }
}
