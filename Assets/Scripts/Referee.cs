using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Referee : MonoBehaviour
{
    private bool m_IsGameOver = false;
    public Player m_PlayerA;
    public Player m_PlayerB;

    // Update is called once per frame
    void Update()
    {
        if (m_IsGameOver == false) {

            if (m_PlayerA.IsLiving == false) {
                Debug.Log(m_PlayerA.name + " Lost");
                m_IsGameOver = true;
            } 
            
            if (m_PlayerB.IsLiving == false) {
                Debug.Log(m_PlayerB.name + " Lost");
                m_IsGameOver = true;
            }
        }
    }
}
