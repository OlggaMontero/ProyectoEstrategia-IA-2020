using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour
{
    private GM gm;

    public GameObject weaponIcon;   

    public int playerNumber;
    bool isActive;

    public int health;
    public int armor;
    public int defenseDamage;

    public Text displayedText;

    private void Start()
    {
        gm = FindObjectOfType<GM>();
        UpdateHealthDisplay();
    }
  
    private void OnMouseDown() // attack
    {
        if (gm.selectedUnit == null) return;

        if (playerNumber == gm.playerTurn)
        {
            //nada de momento?
        }
        else
        {
            gm.selectedUnit.ResetWeaponIcons();
            if (gm.selectedUnit.m_enemyBaseInRange && !gm.selectedUnit.hasAttacked && gm.selectedUnit.enemyBase == this.gameObject)
            {
                gm.selectedUnit.AttackBase(this);
            }
        }
    }

    public void UpdateHealthDisplay()
    {
        displayedText.text = health.ToString();
    }
}
