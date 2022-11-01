using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]

public class ProgressBar : MonoBehaviour
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
    public float RepeatRate = 1f;

    public Image bar, barBackground;
    [HideInInspector] public Vector3 ogBarSize, ogBar2Size, ogBarBackgroundSize;
    [HideInInspector] public Transform barRT, bar2RT, barBackgroundRT;
    private float nextPlay;
    private AudioSource audiosource;
    private float bar1Value;

    public float flashFrames = 20;

    public RectTransform barBackgroundTrans;
    public float Bar1Value
    {
        get { return bar1Value; }

        set
        {
            value = Mathf.Clamp(value, 0, 100);
            bar1Value = value;
            UpdateBar1Value(bar1Value);
        }
    }

    bool DoFlash = false;
    bool flashing = false;
    bool colorUp = false;
    float deltaG = 0;
    float deltaB = 0;
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
        }
        if (flashing)
        {
            Color c = barBackground.color;
            if (colorUp)
            {
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

    void UpdateBar1Value(float val)
    {
        if (bar == null) return;
        bar.fillAmount = val / 100;

        if (Alert >= val)
        {
            bar.color = BarAlertColor;
        }
        else
        {
            bar.color = BarColor;
        }
    }

    public void TakeDamageAnimation()
    {
        DoFlash = true;
    }
}
