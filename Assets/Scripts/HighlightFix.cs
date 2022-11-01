using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Selectable))]
public class HighlightFix : MonoBehaviour, IPointerEnterHandler
{
    public EventSystem eventSystem;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EventSystem.current.alreadySelecting)
            //eventSystem.currentSelectedGameObject
            eventSystem.SetSelectedGameObject(this.gameObject);
    }
}