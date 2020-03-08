using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    public Player m_OpponentPlayer;
    protected Player m_PlayerToControl;

    private Bounds m_Bb = new Bounds();

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
    }

    private void ComputeOpponentRectangle()
    {
        // --- figure opponent's extents box ---
        
        if (m_OpponentPlayer.trailObjects != null) {

            m_Bb.size = Vector3.negativeInfinity;
            m_Bb.center = Vector3.negativeInfinity;
            
            foreach (GameObject trailObj in m_OpponentPlayer.trailObjects) {
                Collider2D collider = trailObj.GetComponent<Collider2D>();
                if (collider != null) {
                    Bounds b = collider.bounds;
                    m_Bb.Encapsulate(b.min);
                    m_Bb.Encapsulate(b.max);
                }
            }
        }
    }

    private float m_LastTurnTime = 0.0f;
    
    void Update()
    {
        // compute opponent's full extents
        ComputeOpponentRectangle();

        switch (m_AiState) {
            case AiState.MoveTowards:
                // point toward opponent

                Vector3 dirV = (m_OpponentPlayer.transform.localPosition - transform.localPosition)
                    .normalized;
                float yawDestDeg = Mathf.Rad2Deg * Mathf.Atan2(dirV.y, dirV.x) + 90.0f;
                
                Player.CardinalDirection destinationDir = Player.NearestCardinalDirectionFromYaw(yawDestDeg);

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
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        float radius = m_Bb.extents.magnitude;
        Gizmos.DrawWireCube(m_Bb.center, m_Bb.size);


        Gizmos.DrawLine(transform.localPosition, m_OpponentPlayer.transform.localPosition);
    }
}
