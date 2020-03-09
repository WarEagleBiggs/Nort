using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    public Player m_OpponentPlayer;
    public float m_AiResponseDelaySec = 0.7f;
    public float m_AiMinForwardDistance = 1.0f;
    
    private Player m_PlayerToControl;

    private float m_LastTurnTime = 0.0f;

    private RaycastHit2D[] m_AiRayHitList;
    
    private enum AiState
    {
        MoveTowards = 0,
        MoveToOpening
    }

    private AiState m_AiState = AiState.MoveTowards;
    
    private Dictionary<string, Vector3> m_RayDirectionMap;
    
    private Dictionary<string, float> m_RayResponseRangeMap;
    
    private List<Vector3> m_DebugHitList;

    private void Start()
    {
        m_PlayerToControl = GetComponent<Player>();
    }

    private void UpdateAiState()
    {
        switch (m_AiState) {
            case AiState.MoveTowards:
                
                /*  
                    if range is small in forward direction, do a turn!
                        which range is great, left or right and turn in that direction                        
                */

                if (m_RayResponseRangeMap != null) {
                    if (m_RayResponseRangeMap["forward"] < m_AiMinForwardDistance) {
                        // change state to move toward largest opening
                        m_AiState = AiState.MoveToOpening;
                    } else {

                        // --- continuously point toward opponent ---

                        Vector3 dirV = (m_OpponentPlayer.transform.localPosition -
                                        transform.localPosition)
                            .normalized;
                        float yawDestDeg = Mathf.Rad2Deg * Mathf.Atan2(dirV.y, dirV.x) + 90.0f;

                        Player.CardinalDirection destinationDir =
                            Player.NearestCardinalDirectionFromYaw(yawDestDeg);

                        if (destinationDir != m_PlayerToControl.direction) {
                            if (Time.realtimeSinceStartup - m_LastTurnTime > m_AiResponseDelaySec) {
                                m_LastTurnTime = Time.realtimeSinceStartup;
                                float playerYawDeg = m_PlayerToControl.transform.eulerAngles.z;
                                float diffDeg = Player.DiffAngleDeg(playerYawDeg, yawDestDeg);
                                if (diffDeg > 0.0) {
                                    m_PlayerToControl.OnTurnRight();
                                } else {
                                    m_PlayerToControl.OnTurnLeft();
                                }
                            }
                        }
                    }
                }
                break;
            case AiState.MoveToOpening:
                if (m_RayResponseRangeMap != null) {
                    if (m_RayResponseRangeMap["forward"] < 1.0f) {
                        if (m_RayResponseRangeMap["right"] > m_RayResponseRangeMap["left"]) {
                            m_PlayerToControl.OnTurnRight();
                        } else {
                            m_PlayerToControl.OnTurnLeft();
                        }
                    }
                }

                break;
        }
    }
    
    private void FireRays()
    {
        if (m_RayDirectionMap == null) {
            m_RayDirectionMap = new Dictionary<string, Vector3>();
            m_RayDirectionMap["forward"] = -Player.s_ForwardVec;
            m_RayDirectionMap["left"] = Player.s_RightVec;
            m_RayDirectionMap["right"] = -Player.s_RightVec;

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

            if (m_AiRayHitList == null) {
                m_AiRayHitList = new RaycastHit2D[5];
            }

            int numHits = Physics2D.RaycastNonAlloc(
                pos,worldDir, m_AiRayHitList, 100.0f);

            for (int i = 0; i < numHits; ++i) {

                RaycastHit2D hit = m_AiRayHitList[i];
                
                if (hit.collider != null && hit.collider.name != m_PlayerToControl.name) {
                    m_DebugHitList.Add(hit.point);
                    m_RayResponseRangeMap[entry.Key] = hit.distance;
                    // found first intersection, quit looking  
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
    
//    void OnDrawGizmos()
//    {
//        // --- debug drawing ---
//
//        Gizmos.color = Color.magenta;
//        Gizmos.DrawLine(transform.localPosition, m_OpponentPlayer.transform.localPosition);
//        
//        if (m_DebugHitList != null) {
//            Gizmos.color = Color.yellow;
//            foreach (Vector3 point in m_DebugHitList) {
//                Gizmos.DrawLine(transform.localPosition, point);
//            }
//
//            m_DebugHitList.Clear();
//        }
//    }
}
