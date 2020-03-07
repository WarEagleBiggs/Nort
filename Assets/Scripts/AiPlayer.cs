using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    public Player m_OpponentPlayer;

    private Bounds m_Bb = new Bounds();
    
    void Update()
    {
        // compute opponent's full extents 
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
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        float radius = m_Bb.extents.magnitude;
        Gizmos.DrawWireCube(m_Bb.center, m_Bb.size);
    }
}
