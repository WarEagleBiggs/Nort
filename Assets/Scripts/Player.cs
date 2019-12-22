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

    private bool m_IsBeginTrail;

    public Color m_TrailColor = Color.yellow;
    public float m_TrailWidth = 0.01f;
    private PolygonCollider2D m_TrailCollider;

    private Mesh m_TrailMesh;

    public bool IsLiving => m_IsLiving;

    private void Start()
    {
        m_IsBeginTrail = true;
    }

    Vector3 PlayerForwardDirection() { return transform.rotation * m_ForwardVec; }
    Vector3 PlayerRightDirection() { return transform.rotation * m_RightVec; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name.Contains(name)) {
            Debug.Log("hitting trail");
        } else { 
            Debug.Log(name + " is Dead");
            m_IsLiving = false;
        }
    }

    private void AddTrail()
    {
        GameObject trail = new GameObject();
        trail.transform.parent = transform.root;
        trail.name = m_PlayerName + "Trail";

        MeshRenderer renderer = trail.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        renderer.material = mat;
        mat.SetColor("_Color", m_TrailColor);
    
        // 4 vertices for two triangles
        Vector3[] triVerts = new Vector3[4];
        // two triangles with 3 vertices each
        int[] triIndices = new int[6];
        
        // position the vertices
        Vector3 startPos = transform.position;
        startPos.z = -5f;
        
        triVerts[0] = startPos - PlayerRightDirection() * m_TrailWidth;
        triVerts[1] = startPos + PlayerRightDirection() * m_TrailWidth;

        triVerts[2] = startPos - PlayerRightDirection() * m_TrailWidth;
        triVerts[3] = startPos + PlayerRightDirection() * m_TrailWidth;

        triIndices[0] = 0;
        triIndices[1] = 1;
        triIndices[2] = 2;
        triIndices[3] = 2;
        triIndices[4] = 1;
        triIndices[5] = 3;

        m_TrailMesh = new Mesh();
        m_TrailMesh.Clear();
        m_TrailMesh.vertices = triVerts;
        m_TrailMesh.triangles = triIndices;

        MeshFilter mfilter = trail.AddComponent<MeshFilter>();
        mfilter.mesh = m_TrailMesh;

        // add collider to match the trail geometry
        m_TrailCollider = trail.AddComponent<PolygonCollider2D>();
        m_TrailCollider.isTrigger = true;
    }

    private void UpdateTrail(bool isFinalize = false)
    {
        // --- update trail mesh ---
        if (m_TrailMesh != null) {
            Vector3[] triVerts = m_TrailMesh.vertices;

            Vector3 center = transform.position;

            triVerts[2] = center - PlayerRightDirection() * m_TrailWidth;
            triVerts[3] = center + PlayerRightDirection() * m_TrailWidth;

            m_TrailMesh.vertices = triVerts;
        }
        
        // --- update trail box collider ---
        if (m_TrailCollider != null) {
            
            Vector3[] triVerts = m_TrailMesh.vertices;

            Vector2[] collidePoints = new Vector2[4];
            collidePoints[0].x = triVerts[0].x;
            collidePoints[0].y = triVerts[0].y;

            collidePoints[1].x = triVerts[1].x;
            collidePoints[1].y = triVerts[1].y;

            collidePoints[2].x = triVerts[2].x;
            collidePoints[2].y = triVerts[2].y;

            collidePoints[3].x = triVerts[3].x;
            collidePoints[3].y = triVerts[3].y;

            m_TrailCollider.points = collidePoints;
        }

    }

    private void RotatePlayer(Vector3 eulerDeg)
    {
        transform.rotation = Quaternion.Euler(eulerDeg.x, eulerDeg.y, eulerDeg.z);
    }
    
    private void HandleInput()
    {  
        if (!m_IsLeftDown && Input.GetKey(m_LeftTurnKey)) {
            m_IsLeftDown = true;
            
            Vector3 euler = transform.rotation.eulerAngles;
            euler.z += 90f;

            RotatePlayer(euler);

            // flag to create a new trail
            m_IsBeginTrail = true;

            if (m_TrailCollider != null) {
                // rename trail collider 
                m_TrailCollider.name = "Trail";
            }
            
        } else if (!Input.GetKey(m_LeftTurnKey)) {
            // clear flag
            m_IsLeftDown = false;
        }
        if (!m_IsRightDown && Input.GetKey(m_RightTurnKey)) {
            m_IsRightDown = true;
            
            Vector3 euler = transform.rotation.eulerAngles;
            euler.z -= 90f;

            RotatePlayer(euler);

            // flag to create a new trail
            m_IsBeginTrail = true;

            if (m_TrailCollider != null) {
                // rename trail collider 
                m_TrailCollider.name = "Trail";
            }


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

        if (m_IsBeginTrail) {
            // --- start a new trail in this frame ---
            m_IsBeginTrail = false;

            // back up a bit
            transform.position += PlayerForwardDirection() * (m_TrailWidth);

            // generate mesh for trail
            AddTrail();

            // restore position
            transform.position -= PlayerForwardDirection() * (m_TrailWidth);

        }

        // update trail vertices based on player position
        UpdateTrail();

        if (m_IsLiving) {
            // --- play is alive, update position ---
            float speed = m_PlayerSpeedPerSec * Time.deltaTime;
            transform.position += -PlayerForwardDirection() * speed;
        }

    }
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

