using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float m_PlayerSpeedPerSec = 0.2f;

    public KeyCode m_LeftTurnKey;
    private bool m_IsLeftDown = false;

    public KeyCode m_RightTurnKey;
    private bool m_IsRightDown = false;
    
    // true if player still alive
    private bool m_IsLiving = true;

    public bool IsLiving
    {
        get => m_IsLiving;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(name + " is Dead");
        m_IsLiving = false;
        
    }

    private float HandleInput(float turn)
    {  
        if (!m_IsLeftDown && Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = true;
            turn += 90.0f;
        } else if (!Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = false;
        }

        if (!m_IsRightDown && Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = true;
            turn -= 90.0f;
        } else if (!Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = false;
        }

        return turn;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        float turn = euler.z;

        // handle input keys
        turn = HandleInput(turn);

        if (m_IsLiving) {
            transform.rotation = Quaternion.Euler(euler.x, euler.y, turn);
            float speed = m_PlayerSpeedPerSec * Time.deltaTime;
            transform.position += -transform.up * speed;
        }
    }
}
