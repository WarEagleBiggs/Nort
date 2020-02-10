using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

enum GameState {
    CountDownState = 0,
    PlayingState,
    PausedState,
    TiedState,
    ScoredState,
    GameOverState
}


public class Referee : MonoBehaviour
{
    private GameState m_GameState = GameState.CountDownState;

    public Player m_PlayerA;
    public Player m_PlayerB;
    public GameObject m_ReplayButton;
    public GameObject m_PlayerAWinImage;
    public GameObject m_PlayerBWinImage;
    public GameObject m_PlayerAScoredImage;
    public GameObject m_PlayerBScoredImage;
    public GameObject m_TiedImage;
    public TMPro.TMP_Text m_ScoreTm;
    public TMPro.TMP_Text m_TutorialTm;
    public int m_ScoreGoal = 5;
 

    private float m_InitialTime;
    public const float c_CountDownSec = 2.0f;


    private void HideRestartButton()
    {
        if (m_ReplayButton != null) {
            // initially ensure replay button is not active 
            m_ReplayButton.SetActive(false);
        }
    }

    IEnumerator ShowTutorial()
    {
        m_TutorialTm.text = "First To " + m_ScoreGoal + " Wins";
        yield return new WaitForSeconds(2.0f);
        m_TutorialTm.gameObject.SetActive(false);
    }

    IEnumerator ShowSprite(GameObject obj, float durationSec = 2.0f)
    {
        // show object
        obj.SetActive(true);
        // wait for duration 
        yield return new WaitForSeconds(durationSec);
        // hide object
        obj.SetActive(false);
    }
    
    private void Start()
    {
        HideRestartButton();
        m_InitialTime = Time.time;

        // initial score text
        m_ScoreTm.text = "0-0";

        StartCoroutine(ShowTutorial());

    }

    // Update is called once per frame
    void Update()
    {
        MonitorGameState();

        UpdateScoreDisplay();

    }

    private void UpdateScoreDisplay()
    {
        string scoreStr = "";

        scoreStr += m_PlayerA.m_Score.ToString();
        scoreStr += "-";
        scoreStr += m_PlayerB.m_Score.ToString();

        m_ScoreTm.text = scoreStr;

        if (m_PlayerA.m_Score >= m_ScoreGoal || m_PlayerB.m_Score >= m_ScoreGoal) {
            SceneController.GotoMenu();

        }
        //int numGames = m_PlayerA.m_Score + m_PlayerB.m_Score;

        //if (numGames >= 10) {
        //    SceneController.GotoMenu();
        //}

    }

    private void MonitorGameState()
    {
        switch (m_GameState) {
            case GameState.CountDownState:
                if (Time.time > (m_InitialTime + c_CountDownSec)) {
                    // transition to playing state
                    m_GameState = GameState.PlayingState;
                    // start players
                    StartGameplay();
                }
                break;
            case GameState.PlayingState:

                if (m_PlayerA.IsLiving == false && m_PlayerB.IsLiving == false) {
                    // --- both players just died! ---

                    StartCoroutine(ShowSprite(obj: m_TiedImage));
                    m_GameState = GameState.TiedState;

                } else {
                    // --- one player is still alive ---

                    if (m_PlayerA.IsLiving == false) {
                        // --- player A is dead ---
                        m_PlayerB.m_Score += 1;
                        m_GameState = GameState.ScoredState;
                        StartCoroutine(ShowSprite(obj: m_PlayerBScoredImage));
                    }

                    if (m_PlayerB.IsLiving == false) {
                        // --- player B is dead ---
                        m_PlayerA.m_Score += 1;
                        m_GameState = GameState.ScoredState;
                        StartCoroutine(ShowSprite(obj: m_PlayerAScoredImage));
                    }
                }

                break;

            case GameState.GameOverState:
                // --- handle game over ---

                if (m_ReplayButton != null) {
                    // show replay button
                    m_ReplayButton.SetActive(true);
                }

                break;

            case GameState.TiedState:
                // TODO display tied
                m_GameState = GameState.GameOverState;

                break;
            case GameState.ScoredState:
                // ensure players are stopped
                StopGameplay();

                // TODO display who scored


                break;
            case GameState.PausedState:
                // TODO
                break;
        }
    }

        private void StartGameplay()
    {
        // --- tell players to start ---

        m_PlayerA.m_IsPlaying = true;
        m_PlayerB.m_IsPlaying = true;

    }

    private void StopGameplay()
    {
        // --- tell players to stop ---

        m_PlayerA.m_IsPlaying = false;
        m_PlayerB.m_IsPlaying = false;
    }

    public void OnRestart()
    {
        m_GameState = GameState.CountDownState;
        m_InitialTime = Time.time;
        
        HideRestartButton();

        m_PlayerA.Restart();
        m_PlayerB.Restart();
        
    }

}
