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

    public GameObject m_WallPrefab;

    private GameObject m_CurrentWall = null;

    private Vector3 m_WallStartPos;

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

            m_CurrentWall = null;

        } else if (!Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = false;
        }

        if (!m_IsRightDown && Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = true;
            turn -= 90.0f;

            m_CurrentWall = null;

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

        if (m_CurrentWall == null) {
            // --- current wall does not exist, create it ---

            // store wall starting position
            m_WallStartPos = transform.position;

            // create the wall
            m_CurrentWall = (GameObject)Instantiate(
                m_WallPrefab, m_WallStartPos, Quaternion.identity);

            m_CurrentWall.transform.position = m_WallStartPos;

        } else {
            // --- current wall exists, update transform ---

            // TODO

            // update position

            // update scale

            Vector3 deltaPos = transform.position - m_WallStartPos;

            Debug.Log(deltaPos);

            Vector3 scale = m_CurrentWall.transform.localScale;

            m_CurrentWall.transform.localScale = new Vector3(deltaPos.x*10, 1, 1);

        }

    }
}
