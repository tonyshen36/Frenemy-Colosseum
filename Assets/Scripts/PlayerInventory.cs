using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int cash = 0;

    public int GetCash()
    {
        return cash;
    }

    public void SetCash(int cash)
    {
        this.cash = cash;
    }

    public void AddCash(int cash)
    {
        this.cash += cash;
    }
}
