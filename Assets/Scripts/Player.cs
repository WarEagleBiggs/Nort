using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    public Color m_WallColor = Color.yellow;
    public float m_WallWidth = 3f;

    private GameObject m_CurrentWall;
    private Mesh m_WallMesh;

    public bool IsLiving
    {
        get => m_IsLiving;
    }

    private void Start()
    {
        AddWall();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(name + " is Dead");
        m_IsLiving = false;
    }

    private void AddWall()
    {
        m_CurrentWall = new GameObject();
        MeshRenderer renderer = m_CurrentWall.AddComponent<MeshRenderer>();
        MeshFilter mfilter = m_CurrentWall.AddComponent<MeshFilter>();
        m_WallMesh = mfilter.mesh;
        m_WallMesh = new Mesh();
        
        // 4 vertices for two triangles
        Vector3[] triVerts = new Vector3[4];
        // two triangles with 3 vertices each
        int[] triIndices = new int[6];
        
        // position the vertices
        //triVerts[0] = transform.position + transform 
        
        m_WallMesh.Clear();
        m_WallMesh.vertices = triVerts;
        m_WallMesh.triangles = triIndices;
    }

    private void UpdateWall()
    {
        
    }

    private float HandleInput(float turn)
    {  
        if (!m_IsLeftDown && Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = true;
            turn += 90.0f;
            AddWall();
        } else if (!Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = false;
            AddWall();
        }
        if (!m_IsRightDown && Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = true;
            turn -= 90.0f;
            AddWall();
        } else if (!Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = false;
            AddWall();
        }

        return turn;
    }

    Vector3 GetPlayerDirection()
    {
        // returns player direction unit vector
        // assumes forward direction is Y axis (green)
        return transform.rotation * new Vector3(0f, 1f, 0f); 
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
            
            transform.position += -GetPlayerDirection() * speed;

            UpdateWall();
        }
    }
}
