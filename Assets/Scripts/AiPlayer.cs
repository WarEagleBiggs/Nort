﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    public Player m_OpponentPlayer;
    public float m_AiResponseDelaySec = 0.7f;
    
    private Player m_PlayerToControl;

    private float m_LastTurnTime = 0.0f;
    
    private enum AiState
    {
        MoveTowards = 0,
        MoveCw,
        MoveCcw
    }

    private AiState m_AiState = AiState.MoveTowards;
    
    private Dictionary<string, Vector3> m_RayDirectionMap;
    
    private Dictionary<string, float> m_RayResponseRangeMap;
    
    private List<Vector3> m_DebugHitList;

    private void Start()
    {
        m_PlayerToControl = GetComponent<Player>();
    }
//
//    private Bounds ComputeObjectBounds(GameObject obj)
//    {
//        Bounds bb = new Bounds();
//        
//        bb.size = Vector3.negativeInfinity;
//        bb.center = Vector3.negativeInfinity;
//            
//        Transform[] allChildren = GetComponentsInChildren<Transform>();
//        foreach (Transform t in allChildren) {
//            BoxCollider2D c = t.GetComponent<BoxCollider2D>();
//            if (c != null) {
//                Bounds b = c.bounds;
//                bb.Encapsulate(b.min);
//                bb.Encapsulate(b.max);
//            }
//        }
//
//        return bb;
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

                    if (Time.realtimeSinceStartup - m_LastTurnTime > m_AiResponseDelaySec) {
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
    
    private void FireRays()
    {
        if (m_RayDirectionMap == null) {
            m_RayDirectionMap = new Dictionary<string, Vector3>();
            m_RayDirectionMap["forward"] = -Player.s_ForwardVec;
            m_RayDirectionMap["right"] = Player.s_RightVec;
            m_RayDirectionMap["left"] = -Player.s_RightVec;

            m_RayResponseRangeMap = new Dictionary<string, float>();
            m_RayResponseRangeMap["forward"] = Single.PositiveInfinity;
            m_RayResponseRangeMap["right"] = Single.PositiveInfinity;
            m_RayResponseRangeMap["left"] = Single.PositiveInfinity;
        }

        m_DebugHitList = new List<Vector3>();
        
        Vector3 pos = transform.localPosition;
        
        foreach (var entry in m_RayDirectionMap) {
            Vector3 localDirV = entry.Value;
            Vector3 worldDir = transform.TransformDirection(localDirV);
            
            RaycastHit2D[] hitList = Physics2D.RaycastAll(
                pos,worldDir, 100.0f);

            foreach (var hit in hitList) {
                if (hit.collider != null && hit.collider.name != m_PlayerToControl.name) {
                    m_DebugHitList.Add(hit.point);
                    m_RayResponseRangeMap[entry.Key] = hit.distance;
                    break;
                } else {
                    m_RayResponseRangeMap[entry.Key] = Single.PositiveInfinity;
                }
            }
        }
    }

    void Update()
    {
        // fire rays into the scene
        FireRays();

        // update AI state
        UpdateAiState();
    }
    
    void OnDrawGizmos()
    {
        // --- debug drawing ---

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
