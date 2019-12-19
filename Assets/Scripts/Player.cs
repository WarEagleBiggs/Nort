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

    // true if player is still alive
    private bool m_IsLiving = true;

    private bool m_IsBeginWall;

    public Color m_WallColor = Color.yellow;
    public float m_WallWidth = 3f;

    private Mesh m_WallMesh;

    public bool IsLiving
    {
        get => m_IsLiving;
    }

    private void Start()
    {
        m_IsBeginWall = true;
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
        GameObject wall = new GameObject();
        wall.transform.parent = transform.root;
        wall.name = m_PlayerName + "Wall";

        MeshRenderer renderer = wall.AddComponent<MeshRenderer>();
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

        MeshFilter mfilter = wall.AddComponent<MeshFilter>();
        mfilter.mesh = m_WallMesh;
    }

//    
//    public static Bounds FindExtents(GameObject obj)
//    {
//        Bounds ret = new Bounds();
//        ret.max = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);
//        ret.min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
//
//        foreach (Renderer rend in obj.GetComponentsInChildren(typeof(Renderer), includeInactive: true)) {
//            ret.Encapsulate(rend.bounds);
//
//            Vector3 scale = rend.gameObject.transform.localScale;
//            ret.SetMinMax(
//                Vector3.Scale(ret.min, scale),
//                Vector3.Scale(ret.max, scale));
//            
//        }
//
//        return ret;
//    }


    private void UpdateWall(bool isFinalize = false)
    {
        if (m_WallMesh != null) {
            Vector3[] triVerts = m_WallMesh.vertices;

            Vector3 center = transform.position;

            triVerts[2] = center - PlayerRightDirection() * m_WallWidth;
            triVerts[3] = center + PlayerRightDirection() * m_WallWidth;

            m_WallMesh.vertices = triVerts;
        }
    }

    private void RotatePlayer(Vector3 eulerDeg)
    {
        transform.rotation = Quaternion.Euler(eulerDeg.x, eulerDeg.y, eulerDeg.z);
        // back up a bit
        transform.position += PlayerForwardDirection() * (m_WallWidth);
    }
    

    private void HandleInput()
    {  
        if (!m_IsLeftDown && Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = true;
            
            Vector3 euler = transform.rotation.eulerAngles;
            euler.z += 90f;

            RotatePlayer(euler);

            // flag to create a new wall
            m_IsBeginWall = true;
            
        } else if (!Input.GetKey(m_LeftTurnKey)) {
            // clear flag
            m_IsLeftDown = false;
        }
        if (!m_IsRightDown && Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = true;
            
            Vector3 euler = transform.rotation.eulerAngles;
            euler.z -= 90f;

            RotatePlayer(euler);

            // flag to create a new wall
            m_IsBeginWall = true;
        } else if (!Input.GetKey(m_RightTurnKey)) {
            // clear flag
            m_IsRightDown = false;
        }
    }

    void Update()
    {
        if (m_IsLiving) {
            // handle input keys
            HandleInput();
        }

        if (m_IsBeginWall) {
            // --- start a new wall in this frame ---
            m_IsBeginWall = false;
            // generate mesh for wall
            AddWall();
        }

        // update wall vertices based on player position
        UpdateWall();

        if (m_IsLiving) {
            // --- play is alive, update position ---
            float speed = m_PlayerSpeedPerSec * Time.deltaTime;
            transform.position += -PlayerForwardDirection() * speed;
        }

    }
}
