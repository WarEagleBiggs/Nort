using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string m_PlayerName = "name";
    public float m_PlayerSpeedPerSec = 0.2f;

    public KeyCode m_LeftTurnKey;
    private bool m_IsLeftDown = false;

    public KeyCode m_RightTurnKey;
    private bool m_IsRightDown = false;

    readonly Vector3 m_ForwardVec = new Vector3(0f, 1f, 0f);
    readonly Vector3 m_RightVec = new Vector3(1f, 0f, 0f);

    // true if player still alive
    private bool m_IsLiving = true;

    private bool m_IsRebuildWall;

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
        m_IsRebuildWall = true;
    }

    Vector3 PlayerForwardDirection() { return transform.rotation * m_ForwardVec; }
    Vector3 PlayerRightDirection() { return transform.rotation * m_RightVec; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(name + " is Dead");
        m_IsLiving = false;
    }

    private void AddWall()
    {
        m_CurrentWall = new GameObject();
        m_CurrentWall.transform.parent = transform.root;
        m_CurrentWall.name = m_PlayerName + "Wall";
        
        MeshRenderer renderer = m_CurrentWall.AddComponent<MeshRenderer>();

        Shader shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        renderer.material = mat;
        mat.SetColor("_Color", m_WallColor);
        
        // 4 vertices for two triangles
        Vector3[] triVerts = new Vector3[4];
        // two triangles with 3 vertices each
        int[] triIndices = new int[6];
        
        // position the vertices
        Vector3 startPos = transform.position;
        startPos.z = -5f;
        
        triVerts[0] = startPos - PlayerRightDirection() * m_WallWidth;
        triVerts[1] = startPos + PlayerRightDirection() * m_WallWidth;

        triVerts[2] = startPos - PlayerRightDirection() * m_WallWidth;
        triVerts[3] = startPos + PlayerRightDirection() * m_WallWidth;

        triIndices[0] = 0;
        triIndices[1] = 1;
        triIndices[2] = 2;
        triIndices[3] = 2;
        triIndices[4] = 1;
        triIndices[5] = 3;

        m_WallMesh = new Mesh();
        m_WallMesh.Clear();
        m_WallMesh.vertices = triVerts;
        m_WallMesh.triangles = triIndices;

        MeshFilter mfilter = m_CurrentWall.AddComponent<MeshFilter>();
        mfilter.mesh = m_WallMesh;

    }

    private void UpdateWall()
    {
        if (m_WallMesh != null) {
            Vector3[] triVerts = m_WallMesh.vertices;
            triVerts[2] = transform.position - PlayerRightDirection() * m_WallWidth;
            triVerts[3] = transform.position + PlayerRightDirection() * m_WallWidth;

            m_WallMesh.vertices = triVerts;
        }
    }

    private float HandleInput(float turn)
    {  
        if (!m_IsLeftDown && Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = true;
            turn += 90.0f;
            // XXX TODO end wall here!
            m_IsRebuildWall = true;
        } else if (!Input.GetKey(m_LeftTurnKey)) {
            // clear flag
            m_IsLeftDown = false;
        }
        if (!m_IsRightDown && Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = true;
            turn -= 90.0f;
            // XXX TODO end wall here!
            m_IsRebuildWall = true;
        } else if (!Input.GetKey(m_RightTurnKey)) {
            // clear flag
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
            
            transform.position += -PlayerForwardDirection() * speed;
        }

        if (m_IsRebuildWall) {
            m_IsRebuildWall = false;
            AddWall();
        }
        
        
        UpdateWall();

    }
}
