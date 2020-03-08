using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    public Player m_OpponentPlayer;
    protected Player m_PlayerToControl;

    //private Bounds m_Bb = new Bounds();

    private float m_LastTurnTime = 0.0f;

    private Bounds m_SelfBounds;

    private enum AiState
    {
        MoveTowards = 0,
        MoveCw,
        MoveCcw
    }

    private AiState m_AiState = AiState.MoveTowards;
    
    private void Start()
    {
        m_PlayerToControl = GetComponent<Player>();
        m_SelfBounds = ComputeObjectBounds(m_PlayerToControl.gameObject);
    }

    private Bounds ComputeObjectBounds(GameObject obj)
    {
        Bounds bb = new Bounds();
        
        bb.size = Vector3.negativeInfinity;
        bb.center = Vector3.negativeInfinity;
            
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform t in allChildren) {
            BoxCollider2D c = t.GetComponent<BoxCollider2D>();
            if (c != null) {
                Bounds b = c.bounds;
                bb.Encapsulate(b.min);
                bb.Encapsulate(b.max);
            }
        }

        return bb;
    }
    
//
//    private void ComputeOpponentRectangle()
//    {
//        // --- figure opponent's extents box ---
//        
//        if (m_OpponentPlayer.trailObjects != null) {
//
//            m_Bb.size = Vector3.negativeInfinity;
//            m_Bb.center = Vector3.negativeInfinity;
//            
//            foreach (GameObject trailObj in m_OpponentPlayer.trailObjects) {
//                Collider2D c = trailObj.GetComponent<Collider2D>();
//                if (c != null) {
//                    Bounds b = c.bounds;
//                    m_Bb.Encapsulate(b.min);
//                    m_Bb.Encapsulate(b.max);
//                }
//            }
//        }
//    }


    private void UpdateAiState()
    {
        switch (m_AiState) {
            case AiState.MoveTowards:
                // point toward opponent

                Vector3 dirV = (m_OpponentPlayer.transform.localPosition - transform.localPosition)
                    .normalized;
                float yawDestDeg = Mathf.Rad2Deg * Mathf.Atan2(dirV.y, dirV.x) + 90.0f;
                
                Player.CardinalDirection destinationDir = 
                    Player.NearestCardinalDirectionFromYaw(yawDestDeg);

                if (destinationDir != m_PlayerToControl.direction) {

                    if (Time.realtimeSinceStartup - m_LastTurnTime > 0.7f) {
                        m_LastTurnTime = Time.realtimeSinceStartup;

                        float playerYawDeg = m_PlayerToControl.transform.eulerAngles.z;
                        float diffDeg = Player.DiffAngleDeg(playerYawDeg,yawDestDeg);
                        if (diffDeg > 0.0) {
                            m_PlayerToControl.OnTurnRight();
                        } else {
                            m_PlayerToControl.OnTurnLeft();
                        }
                    }
                }

                break;
            case AiState.MoveCw:
                break;
            case AiState.MoveCcw:
                break;
        }
    }

    private List<Vector3> m_RayDirectionList;

    private List<Vector3> m_DebugHitList;
    
    
    private void FireRays()
    {
        if (m_RayDirectionList == null) {
            m_RayDirectionList = new List<Vector3>();
            m_RayDirectionList.Add(-Player.s_ForwardVec);
            m_RayDirectionList.Add(Player.s_RightVec);
            m_RayDirectionList.Add(-Player.s_RightVec);
        }

        m_DebugHitList = new List<Vector3>();
        
        Vector3 pos = transform.localPosition;
        float ownRadius = m_SelfBounds.size.magnitude / 2.0f;
        
        foreach (Vector3 dirV in m_RayDirectionList) {
            Vector3 worldDir = transform.TransformDirection(dirV);
            
            RaycastHit2D hit = Physics2D.Raycast(
                pos + (worldDir * ownRadius),
                worldDir, 100.0f);
            if (hit.collider != null) {
                m_DebugHitList.Add(hit.point);
            }
        }
    }

    void Update()
    {
//        // compute opponent's full extents
//        ComputeOpponentRectangle();

        // fire rays into the scene
        FireRays();

        // update AI statee
        UpdateAiState();
    }
    
    void OnDrawGizmos()
    {
        // --- debug drawing ---
        
//        Gizmos.color = Color.white;
//        float radius = m_Bb.extents.magnitude;
//        Gizmos.DrawWireCube(m_Bb.center, m_Bb.size);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.localPosition, m_OpponentPlayer.transform.localPosition);
        
        if (m_DebugHitList != null) {
            Gizmos.color = Color.yellow;
            foreach (Vector3 point in m_DebugHitList) {
                Gizmos.DrawLine(transform.localPosition, point);
            }

            m_DebugHitList.Clear();
        }
    }
}
