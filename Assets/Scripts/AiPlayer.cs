using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    private enum CoordFrame
    {
        LocalFrame = 0,
        WorldFrame
    }
    
    private AiState m_AiState = AiState.MoveTowards;
    
    private Dictionary<string, Tuple<CoordFrame, Vector3>> m_RayDirectionMap;
    
    private Dictionary<string, float> m_RayResponseRangeMap;

    private Dictionary<string, string> m_RayColliderName;
    
    private List<Vector3> m_DebugHitList;

    private void Start()
    {
        m_PlayerToControl = GetComponent<Player>();
    }

    private float FindMinForwardDistance()
    {
        return Mathf.Min(m_RayResponseRangeMap["forwardRight"],
            Mathf.Min(m_RayResponseRangeMap["forward"], 
                m_RayResponseRangeMap["forwardLeft"]));
    }

    private void TurnRight()
    {
        m_LastTurnTime = Time.realtimeSinceStartup;
        m_PlayerToControl.OnTurnRight();
    }

    private void TurnLeft()
    {
        m_LastTurnTime = Time.realtimeSinceStartup;
        m_PlayerToControl.OnTurnLeft();
    }

    private void MoveTowardOpponent()
    {
        Vector3 dirV = (m_OpponentPlayer.transform.localPosition -
                        transform.localPosition).normalized;
        float yawDestDeg = Mathf.Rad2Deg * Mathf.Atan2(dirV.y, dirV.x) + 90.0f;

        Player.CardinalDirection destinationDir =
            Player.NearestCardinalDirectionFromYaw(yawDestDeg);

        if (FindMinForwardDistance() < m_AiMinForwardDistance) {
            m_AiState = AiState.MoveToOpening;
            MoveToOpening();
        } else {

            if (destinationDir != m_PlayerToControl.direction) {
                if ((Time.realtimeSinceStartup - m_LastTurnTime) > m_AiResponseDelaySec) {
                    float playerYawDeg = m_PlayerToControl.transform.eulerAngles.z;
                    float diffDeg = Player.DiffAngleDeg(playerYawDeg, yawDestDeg);
                    if (diffDeg > 0.0) {
                        if (m_RayResponseRangeMap["right"] > 2.0f) {
                            TurnRight();
                        }
                    } else { 
                        if (m_RayResponseRangeMap["left"] > 2.0f) {
                            TurnLeft();
                        }
                    }
                }
            }
        }
    }

    private void MoveToOpening()
    {
        if (m_RayResponseRangeMap != null) {
            if ((Time.realtimeSinceStartup - m_LastTurnTime) > m_AiResponseDelaySec) {
                if (FindMinForwardDistance() < m_AiMinForwardDistance) {
                    if (m_RayResponseRangeMap["right"] > m_RayResponseRangeMap["left"]) {
                        if (m_RayResponseRangeMap["right"] > 4.0f) {
                            TurnRight();
                        } else {
                            TurnLeft();
                        }
                    } else if (m_RayResponseRangeMap["left"] > 4.0f) {
                        TurnLeft();
                    } else {
                        TurnRight();
                    }
                }
            }
        }
    }

    private void UpdateAiState()
    {
        if (m_RayResponseRangeMap != null) {
            if (m_AiState == AiState.MoveTowards) {
                if (FindMinForwardDistance() < m_AiMinForwardDistance) {
                    // change state to move toward largest opening
                    m_AiState = AiState.MoveToOpening;
                }
            } else if (m_AiState == AiState.MoveToOpening) {
                // check LOS with opponent
                if (m_RayColliderName != null) {
                    string colliderName = m_RayColliderName["towardOpponent"];

                    if (colliderName == m_OpponentPlayer.name) {
                        m_AiState = AiState.MoveTowards;
                    }
                }
            }
        }
    }
    
    Vector3 VectorFromAngle(float angleDeg)
    {
        float angleRad = Mathf.Deg2Rad * angleDeg;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0.0f).normalized;
    }

    private void FireRays()
    {
        if (m_RayDirectionMap == null) {
            m_RayDirectionMap = new Dictionary<string, Tuple<CoordFrame, Vector3>>();
            m_RayDirectionMap["forward"] = new Tuple<CoordFrame, Vector3>(
                CoordFrame.LocalFrame, -Player.s_ForwardVec);
            m_RayDirectionMap["left"] = new Tuple<CoordFrame, Vector3>(
                CoordFrame.LocalFrame, Player.s_RightVec);
            m_RayDirectionMap["right"] = new Tuple<CoordFrame, Vector3>(
                CoordFrame.LocalFrame, -Player.s_RightVec);
            m_RayDirectionMap["forwardLeft"] = new Tuple<CoordFrame, Vector3>(
                CoordFrame.LocalFrame, VectorFromAngle(-75.0f));
            m_RayDirectionMap["forwardRight"] = new Tuple<CoordFrame, Vector3>(
                CoordFrame.LocalFrame, VectorFromAngle(-105.0f));
            m_RayDirectionMap["towardOpponent"] = new Tuple<CoordFrame, Vector3>(
                CoordFrame.LocalFrame, Vector3.zero);

            m_RayResponseRangeMap = new Dictionary<string, float>();
            m_RayResponseRangeMap["forward"] = Single.PositiveInfinity;
            m_RayResponseRangeMap["right"] = Single.PositiveInfinity;
            m_RayResponseRangeMap["left"] = Single.PositiveInfinity;
            m_RayResponseRangeMap["forwardLeft"] = Single.PositiveInfinity;
            m_RayResponseRangeMap["forwardRight"] = Single.PositiveInfinity;
            m_RayResponseRangeMap["towardOpponent"] = Single.PositiveInfinity;
            
            m_RayColliderName = new Dictionary<string, string>();
            m_RayColliderName["forward"] = "";
            m_RayColliderName["right"] = "";
            m_RayColliderName["left"] = "";
            m_RayColliderName["forwardLeft"] = "";
            m_RayColliderName["forwardRight"] = "";
            m_RayColliderName["towardOpponent"] = "";
        }

        m_DebugHitList = new List<Vector3>();
        
        Vector3 pos = transform.localPosition;
        
        foreach (var entry in m_RayDirectionMap) {
            CoordFrame frame = entry.Value.Item1;
            Vector3 dirV = entry.Value.Item2;
            
            if (frame == CoordFrame.LocalFrame) {
                dirV = transform.TransformDirection(dirV);
            }

            if (m_AiRayHitList == null) {
                const int enoughIntersects = 5;
                m_AiRayHitList = new RaycastHit2D[enoughIntersects];
            }

            int numHits = Physics2D.RaycastNonAlloc(
                pos,dirV, m_AiRayHitList, 100.0f);

            for (int i = 0; i < numHits; ++i) {

                RaycastHit2D hit = m_AiRayHitList[i];
                
                if (hit.collider != null && hit.collider.name != m_PlayerToControl.name) {
                    m_DebugHitList.Add(hit.point);
                    m_RayResponseRangeMap[entry.Key] = hit.distance;
                    m_RayColliderName[entry.Key] = hit.collider.name;
                    // found first intersection, quit looking  
                    break;
                } else {
                    m_RayResponseRangeMap[entry.Key] = Single.PositiveInfinity;
                    m_RayColliderName[entry.Key] = "";
                }
            }
        }
    }

    void Update()
    {
        if (m_RayDirectionMap != null) {
            // direction vector to opponent
            Vector3 towardV = (m_OpponentPlayer.transform.localPosition -
                               transform.localPosition).normalized;
            m_RayDirectionMap["towardOpponent"] = new Tuple<CoordFrame, Vector3>(
                CoordFrame.WorldFrame, towardV);
        }
        
        // fire rays into the scene
        FireRays();

        // update AI state
        UpdateAiState();

        if (m_PlayerToControl.IsLiving) {
            if (m_AiState == AiState.MoveTowards) {
                // continuously point toward opponent
                MoveTowardOpponent();
            } else if (m_AiState == AiState.MoveToOpening) {
                // move to large opening
                MoveToOpening();
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // --- debug drawing ---
        if (m_DebugHitList != null) {
            Gizmos.color = Color.yellow;
            foreach (Vector3 point in m_DebugHitList) {
                Gizmos.DrawLine(transform.localPosition, point);
            }

            m_DebugHitList.Clear();
        }
    }
}
