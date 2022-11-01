using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractControl : MonoBehaviour
{
    public bool isEnabled = false;
    public Text instruction1;
    public Text instruction2;
    public float offset = 40.0f;
    public float textSpeed = 0.6f;
    public float autoHideTimer = 2.0f;

    public float pulseEveryXSec = 5.0f;
    public float pulseMaxSizeRatio = 2.0f;
    public float curSizeRatio = 1.0f;
    private bool textPulsing = false;
    private bool doNotRegrow = false;
    public bool sizeGrowing = true;
    public Vector3 instuction1OgSize;

    private bool raiseInstruction1 = false;
    private bool lowerInstruction1 = false;
    private bool raiseInstruction2 = false;
    private bool lowerInstruction2 = false;
    public CharacterControl characterControl;
    // Start is called before the first frame update
    void Start()
    {
        if (!enabled) return;
        instuction1OgSize = instruction1.transform.localScale;
    }

   

    private float text2RelativeY = 0;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (raiseInstruction1 || lowerInstruction1)
        {
            Color c = instruction1.color;
            c.a = instruction1.transform.localPosition.y / offset;
            

            if (raiseInstruction1)
            {
                lowerInstruction1 = false;
                if (instruction1.transform.localPosition.y <= offset)
                {
                    instruction1.transform.localPosition += new Vector3(0, textSpeed, 0);
                }
                else
                {
                    instruction1.transform.localPosition = new Vector3(0, offset, 0);
                    raiseInstruction1 = false;
                    StartTextPulsing();
                    c.a = 1;
                }
            }

            else if (lowerInstruction1)
            {
                raiseInstruction1 = false;
                if (instruction1.transform.localPosition.y >= 0)
                {
                    instruction1.transform.localPosition -= new Vector3(0, textSpeed, 0);
                    EndTextPulsing();
                }
                else
                {
                    instruction1.transform.localPosition = new Vector3(0, 0, 0);
                    instruction1.enabled = false;
                    lowerInstruction1 = false;
                    c.a = 0;
                }
            }
            instruction1.color = c;
        }

        if (textPulsing)
        {
            if (sizeGrowing)
            {
                curSizeRatio += (pulseMaxSizeRatio - 1) / (50 * pulseEveryXSec / 2);
                if (curSizeRatio > pulseMaxSizeRatio)
                {
                    curSizeRatio = pulseMaxSizeRatio;
                    sizeGrowing = false;
                }
            }
            else
            {
                curSizeRatio -= (pulseMaxSizeRatio - 1) / (50 * pulseEveryXSec / 2);
                if (curSizeRatio < 1.0f)
                {
                    curSizeRatio = 1.0f;
                    if (doNotRegrow) textPulsing = false;
                    else sizeGrowing = true;
                }
            }
            instruction1.transform.localScale = instuction1OgSize * curSizeRatio;
            float c_black = 0.6f * (1 - (pulseMaxSizeRatio - curSizeRatio) / (pulseMaxSizeRatio - 1));
            instruction1.color = new Color(c_black, c_black, c_black);
        }

        float offset2 = instruction1.transform.localPosition.y + offset;
        float textSpeed2 = instruction1.transform.localPosition.y <= 0.01f ? textSpeed : textSpeed * 2.0f;

        Color c2 = instruction2.color;

        if (c2.a == 1 && instruction2.transform.localPosition.y - instruction1.transform.localPosition.y < offset && !lowerInstruction2)
        {
            instruction2.transform.localPosition += new Vector3(0, textSpeed2, 0);
        }
        else if (raiseInstruction2 || lowerInstruction2)
        {
            
            c2.a = instruction2.transform.localPosition.y / offset;


            if (raiseInstruction2)
            {
                lowerInstruction2 = false;
                if (instruction2.transform.localPosition.y <= offset2)
                {
                    instruction2.transform.localPosition += new Vector3(0, textSpeed2, 0);
                }
                else
                {
                    instruction2.transform.localPosition = new Vector3(0, offset2, 0);
                    raiseInstruction2 = false;
                    c2.a = 1;
                }
            }

            else if (lowerInstruction2)
            {
                raiseInstruction2 = false;
                if (instruction2.transform.localPosition.y >= 0)
                {
                    instruction2.transform.localPosition -= new Vector3(0, textSpeed2, 0);
                }
                else
                {
                    instruction2.transform.localPosition = new Vector3(0, 0, 0);
                    instruction2.enabled = false;
                    lowerInstruction2 = false;
                    instruction2.color = Color.black;
                    c2.a = 0;
                }
            }
            instruction2.color = c2;
        }

        if (false)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                ShowInstruction1();
            }
            if (Input.GetKey(KeyCode.X))
            {
                HideInstruction1();
            }
        }
    }

    private void StartTextPulsing()
    {
        textPulsing = true;
        sizeGrowing = true;
        doNotRegrow = false;
    }

    private void EndTextPulsing()
    {
        if (!doNotRegrow) doNotRegrow = true;
    }

    public void DisplayArchway(int playerCount)
    {
        ShowInstruction1();
        if (playerCount == -1)
            instruction1.text = "Revive Your Teammate to Pass";
        else if (playerCount < 2)
            instruction1.text = "Need Two Players to Pass";
        else if (playerCount == 2)
            instruction1.text = "Press F To Go To Next Level"; 

    }

    public void DisplayChest()
    {
        ShowInstruction1();
        instruction1.text = "Press F To Open";
    }

    public void DisplayRevive()
    {
        ShowInstruction1();
        instruction1.text = "Press F To Revive Teammate";
    }

    public void DisplayPVP(List<string> l)
    {

        //if (l.Count == 0) ShowInstruction2("Bugged out", Color.black, 3.0f);
        string result = "Swapped " + l[0];
        if (l.Count == 3)
        {
            result += " " + l[1] + " and " + l[2];
        }
        else if (l.Count == 2)
        {
            result += " and " + l[1];
        }
        ShowInstruction2(result, characterControl.sceneControl.EnemyColor, 5.0f);
        
        //instruction2.color = Color.red;
    }

    private Coroutine cur_coroutine = null;
    public void ShowInstruction2(string txt, Color c, float timer = -1)
    {
        if (!characterControl.PV.IsMine) return;
        raiseInstruction2 = true;
        instruction2.transform.localPosition = Vector3.zero;
        instruction2.enabled = true;
        instruction2.text = txt;
        instruction2.color = c;
        if (cur_coroutine != null)
            StopCoroutine(cur_coroutine);
        if (timer == -1)
            timer = autoHideTimer;
        if (timer == -2)
            cur_coroutine = StartCoroutine(HideInstruction2(autoHideTimer, true));
        else
            cur_coroutine = StartCoroutine(HideInstruction2(timer));
    }

    IEnumerator HideInstruction2(float waitTime, bool brokenProtection = false)
    {
        yield return new WaitForSeconds(waitTime);
        if (brokenProtection)
            ShowInstruction2("Friendly Damage Protection Broken", characterControl.sceneControl.EnemyColor, 4.0f);
        else
            lowerInstruction2 = true;
    }

    public void ForceHideInstruction2()
    {
        if (cur_coroutine != null)
            StopCoroutine(cur_coroutine);
        lowerInstruction2 = true;
    }

    public void ShowInstruction1(string txt = null)
    {
        if (!characterControl.PV.IsMine) return;
        raiseInstruction1 = true;
        instruction1.transform.localPosition = Vector3.zero;
        instruction1.enabled = true;
        if (txt != null) instruction1.text = txt;
    }

    public void HideInstruction1()
    {
        lowerInstruction1 = true;
    }
}