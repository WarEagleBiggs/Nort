using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum CardinalDirection
    {
        North = 0,
        South,
        East,
        West
    }

    private Vector3 m_InitialPosition;
    private Quaternion m_InitialRotation;
    
    public string m_PlayerName = "name";
    public float m_PlayerSpeedPerSec = 0.2f;

    public KeyCode m_LeftTurnKey;
    private bool m_IsLeftDown = false;

    public KeyCode m_RightTurnKey;
    private bool m_IsRightDown = false;

    readonly Vector3 m_ForwardVec = new Vector3(0f, 1f, 0f);
    readonly Vector3 m_RightVec = new Vector3(1f, 0f, 0f);

    // current player direction
    private CardinalDirection m_Direction = CardinalDirection.North;
    
    // true if player is still alive
    private bool m_IsLiving = true;

    public int m_Score;
    
    private List<GameObject> m_TrailObjectList;
    
    private bool m_IsStartTrail;
    public Animator m_Animator;

    public float m_TrailWidth = 0.01f;
    private PolygonCollider2D m_TrailCollider;

    private List<Tuple<Vector3, PolygonCollider2D>> m_TrailsToEnable;

    private Mesh m_TrailMesh;
    public Material m_TrailMaterial;
    public float m_TrailZ;
    
    public float m_MinGapBetweenTrails = 0.15f;
    
    public bool m_IsPlaying;

    private AiPlayer m_AiPlayer;
    
    // --- accessor properties ---
    public bool IsLiving => m_IsLiving;

    public List<GameObject> trailObjects => m_TrailObjectList;

    public CardinalDirection direction => m_Direction;
    private Vector3 PlayerForwardDirection() { return transform.rotation * m_ForwardVec; }
    private Vector3 PlayerRightDirection() { return transform.rotation * m_RightVec; }

    private void Start()
    {
        m_IsStartTrail = true;

        m_AiPlayer = gameObject.GetComponent<AiPlayer>();
        
        m_TrailsToEnable = new List<Tuple<Vector3, PolygonCollider2D>>();

        // ensure starting direction is Cardinal
        InitStartingDirection();

        m_InitialPosition = transform.position;
        m_InitialRotation = transform.rotation;
    }

    public void Restart()
    {
        if (m_TrailObjectList != null) {
            foreach (var obj in m_TrailObjectList) {
                DestroyImmediate(obj);
            }
            m_TrailObjectList.Clear();
        }

        m_TrailsToEnable = new List<Tuple<Vector3, PolygonCollider2D>>();

        m_TrailCollider = null;
        m_TrailMesh = null;
        
        m_IsStartTrail = true;

        m_IsLiving = true;
        
        transform.position = m_InitialPosition;
        transform.rotation = m_InitialRotation;
               
        // ensure starting direction is Cardinal
        InitStartingDirection();
    }

    public static float DiffAngleDeg(float xDeg, float yDeg)
    {
        return Mathf.Rad2Deg * Mathf.Atan2(
            Mathf.Sin(Mathf.Deg2Rad * (xDeg - yDeg)), 
            Mathf.Cos(Mathf.Deg2Rad * (xDeg - yDeg)));
    }

    public static bool DiffAngleIsSame(float xDeg, float yDeg, float epsilonDeg = 1.0f)
    {
        bool ret = false;
        
        float diffDeg = DiffAngleDeg(xDeg, yDeg);
        
        if (Mathf.Abs(diffDeg) <= epsilonDeg) {
            // difference magnitude is within tolerance
            ret = true;
        }

        return ret;
    }

    public static CardinalDirection NearestCardinalDirectionFromYaw(float yawDeg)
    {
        CardinalDirection dir;
        
        const float thresholdDeg = 45.0f;
        
        if (DiffAngleIsSame(yawDeg, 180.0f, thresholdDeg)) {
            dir = CardinalDirection.North;
        } else if (DiffAngleIsSame(yawDeg, 90.0f, thresholdDeg)) {
            dir = CardinalDirection.East;
        } else if (DiffAngleIsSame(yawDeg, 0.0f, thresholdDeg)) {
            dir = CardinalDirection.South;
        } else if (DiffAngleIsSame(yawDeg, -90.0f, thresholdDeg)) { 
            dir = CardinalDirection.West;
        } else {
            Debug.Log("Warning: ambiguous cardinal direction: " + yawDeg);
            dir = CardinalDirection.North;
        }

        return dir;
    }

    private void InitStartingDirection()
    {
        Vector3 eulerDeg = transform.rotation.eulerAngles;

        m_Direction = NearestCardinalDirectionFromYaw(eulerDeg.z);

        // ensure Euler angles are set along Cardinal direction 
        SetRotationFromCardinalDirection(m_Direction);
    }

    public void SetRotationFromCardinalDirection(CardinalDirection dir)
    {
        Vector3 eulerDeg = transform.rotation.eulerAngles;
        
        switch (dir) {
            case CardinalDirection.North:
                eulerDeg.z = 180.0f;
                break;
            case CardinalDirection.South:
                eulerDeg.z = 0.0f;
                break;
            case CardinalDirection.East:
                eulerDeg.z = 90.0f;
                break;
            case CardinalDirection.West:
                eulerDeg.z = -90.0f;
                break;
        }
        transform.rotation = Quaternion.Euler(eulerDeg.x, eulerDeg.y, eulerDeg.z);
    }

    public void OnTurnLeft()
    {
        switch (m_Direction) {
            case CardinalDirection.North:
                m_Direction = CardinalDirection.West;
                break;
            case CardinalDirection.South:
                m_Direction = CardinalDirection.East;
                break;
            case CardinalDirection.East:
                m_Direction = CardinalDirection.North;
                break;
            case CardinalDirection.West:
                m_Direction = CardinalDirection.South;
                break;
        }

        SetRotationFromCardinalDirection(m_Direction);
        
        // flag to create a new trail
        m_IsStartTrail = true;

        // mark trail as collidable
        EnableTrailAsCollidable();
    }

    public void OnTurnRight()
    {
        switch (m_Direction) {
            case CardinalDirection.North:
                m_Direction = CardinalDirection.East;
                break;
            case CardinalDirection.South:
                m_Direction = CardinalDirection.West;
                break;
            case CardinalDirection.East:
                m_Direction = CardinalDirection.South;
                break;
            case CardinalDirection.West:
                m_Direction = CardinalDirection.North;
                break;
        }

        SetRotationFromCardinalDirection(m_Direction);
        
        // flag to create a new trail
        m_IsStartTrail = true;

        // mark trail as collidable
        EnableTrailAsCollidable();
    }

    private void EnableOldTrails()
    {
        if (m_TrailsToEnable.Count > 0) {

            List<Tuple<Vector3, PolygonCollider2D>> toRemoveList =
                new List<Tuple<Vector3, PolygonCollider2D>>();

            foreach (var trail in m_TrailsToEnable) {
                Vector3 pos = trail.Item1;

                if ((transform.localPosition - pos).magnitude > 0.5f) {
                    PolygonCollider2D poly = trail.Item2;
                    poly.name = "Trail";
                    toRemoveList.Add(trail);
                }
            }

            foreach (var trail in toRemoveList) {
                m_TrailsToEnable.Remove(trail);
            }
        }
    }

    private void EnableTrailAsCollidable()
    {
        m_TrailsToEnable.Add(
            new Tuple<Vector3, PolygonCollider2D>(transform.localPosition, m_TrailCollider));
    }

    private void EvaluateCollisionWith(Collider2D other)
    {
        // --- detect collision ---

        if (!m_IsDisbleCollision) {

            if (!other.name.Contains(name)) {
                // not hitting self

                m_IsLiving = false;
                if (m_Animator != null) {
                    m_Animator.SetTrigger("ImpactTrigger");
                    m_Animator.SetTrigger("BackToBase");
                }
            }
        }
    }

    // TODO remove this soon!!! (testing only)
    public bool m_IsDisbleCollision = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        EvaluateCollisionWith(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        EvaluateCollisionWith(other);
    }

    private void AddTrail()
    {
        GameObject trail = new GameObject();
        trail.transform.parent = transform.root;
        trail.name = m_PlayerName + "Trail";

        if (m_TrailObjectList == null) {
            m_TrailObjectList = new List<GameObject>();
        }
        
        m_TrailObjectList.Add(trail);

        MeshRenderer renderer = trail.AddComponent<MeshRenderer>();
        renderer.material = m_TrailMaterial;

        // 4 vertices for two triangles
        Vector3[] triVerts = new Vector3[4];
        // two triangles with 3 vertices each
        int[] triIndices = new int[6];
        
        // position the vertices
        Vector3 startPos = transform.position;
        startPos.z = m_TrailZ;

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
            center.z = m_TrailZ;

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

    private void HandleInput()
    {
        if (!m_IsLeftDown && Input.GetKeyDown(m_LeftTurnKey)) {

            m_IsLeftDown = true;

            // turn player
            OnTurnLeft();

        } else if (!Input.GetKeyDown(m_LeftTurnKey)) {
            // clear flag
            m_IsLeftDown = false;
        }
        
        if (!m_IsRightDown && Input.GetKeyDown(m_RightTurnKey)) {
            m_IsRightDown = true;

            // turn player
            OnTurnRight();

        } else if (!Input.GetKeyDown(m_RightTurnKey)) {
            // clear flag
            m_IsRightDown = false;
        }
    }

    void Update()
    {
        bool isAiPlayer = m_AiPlayer != null && m_AiPlayer.isActiveAndEnabled;
        
        if (m_IsPlaying) {
            
            if (m_IsLiving && !isAiPlayer) {
                // handle input keys
                HandleInput();
            }

            if (m_IsStartTrail) {
                // --- start a new trail in this frame ---

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

                if (m_IsStartTrail) {
                    // ensure gap between trails
                    transform.position -= PlayerForwardDirection() * m_MinGapBetweenTrails;
                }
            }
            
            m_IsStartTrail = false;

            EnableOldTrails();

        }
    }
}
