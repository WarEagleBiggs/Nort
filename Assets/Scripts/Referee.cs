using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

enum GameState {
    CountDownToStartState = 0,
    PlayingState,
    PausedState,
    EndOfRoundState,
    GameOverState
}


public class Referee : MonoBehaviour
{
    private GameState m_GameState = GameState.CountDownToStartState;

    public Player m_PlayerA;
    public Player m_PlayerB;
    public GameObject m_ReplayButton;
    public GameObject m_PlayerAWinImage;
    public GameObject m_PlayerBWinImage;
    public GameObject m_PlayerAScoredImage;
    public GameObject m_PlayerBScoredImage;
    public GameObject m_ExitMenuButton;
    public GameObject m_ResumeButton;

    public GameObject m_TiedImage;
    public TMPro.TMP_Text m_ScoreTm;
    public TMPro.TMP_Text m_TutorialTm;
    public int m_ScoreGoal = 5;

 

    private float m_InitialTime;
    public const float c_CountDownSec = 1.0f;
    public const float c_ReplayDelaySec = 2.0f;
    public const float c_ImageShowDurationSec = 2.0f;


    private void HideRestartButton()
    {
        if (m_ReplayButton != null) {
            // hide replay button is not active 
            m_ReplayButton.SetActive(false);
        }
    }

    private void HideExitMenuButton()
    {
        if (m_ExitMenuButton != null) {
            // hide exit to menu is not active 
            m_ExitMenuButton.SetActive(false);
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

        if (Mathf.Approximately(durationSec, 0.0f)) {
            // duration is zero, exit
            yield return null;
        } else {
            // wait for duration 
            yield return new WaitForSeconds(durationSec);
            // hide object
            obj.SetActive(false);
        }
    }

    IEnumerator DelayReplay(float durationSec = 2.0f)
    {
        // wait for duration 
        yield return new WaitForSeconds(durationSec);
        // restart
        OnReplay();
    }

    private void Start()
    {
        // ensure players are stopped
        StopGameplay();

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
    }

    private void MonitorGameState()
    {
        switch (m_GameState) {
            case GameState.CountDownToStartState:
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

                    StartCoroutine(ShowSprite(obj: m_TiedImage,
                        durationSec: c_ImageShowDurationSec));
                    m_GameState = GameState.EndOfRoundState;

                } else {
                    // --- one player is still alive ---

                    if (m_PlayerA.IsLiving == false) {
                        // --- player A is dead ---
                        m_PlayerB.m_Score += 1;
                        m_GameState = GameState.EndOfRoundState;
                        if (m_PlayerB.m_Score < m_ScoreGoal) {
                            // player B has not won yet, display round win
                            StartCoroutine(ShowSprite(obj: m_PlayerBScoredImage,
                                durationSec: c_ImageShowDurationSec));
                        }
                    }

                    if (m_PlayerB.IsLiving == false) {
                        // --- player B is dead ---
                        m_PlayerA.m_Score += 1;
                        m_GameState = GameState.EndOfRoundState;
                        if (m_PlayerA.m_Score < m_ScoreGoal) {
                            // player A has not won yet, display round win
                            StartCoroutine(ShowSprite(obj: m_PlayerAScoredImage,
                                durationSec: c_ImageShowDurationSec));
                        }
                    }
                }

                break;

            case GameState.GameOverState:
                // --- handle game over ---

                if (m_ReplayButton != null) {
                    // show replay button
                    m_ReplayButton.SetActive(true);
                }
                if (m_ExitMenuButton != null) {
                    // show exit button
                    m_ExitMenuButton.SetActive(true);
                }

                break;

            case GameState.EndOfRoundState:
                // --- round is over, check game state ---
                
                // ensure players are stopped
                StopGameplay();

                CheckForGameComplete();

                if (m_GameState != GameState.GameOverState) {
                    // start coroutine to delay replay
                    StartCoroutine(DelayReplay(c_ReplayDelaySec));
                }

                break;
            case GameState.PausedState:
                // nothing to do here
                break;
        }
    }

    private void CheckForGameComplete()
    {
        if (m_PlayerA.m_Score >= m_ScoreGoal) {
            // --- player A won!, show and don't hide ---
            StartCoroutine(ShowSprite(m_PlayerAWinImage,
                durationSec: 0.0f));
            m_GameState = GameState.GameOverState;
        }

        if (m_PlayerB.m_Score >= m_ScoreGoal) {
            // --- player B won!, show and don't hide ---
            StartCoroutine(ShowSprite(m_PlayerBWinImage,
                durationSec: 0.0f));
            m_GameState = GameState.GameOverState;
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

    public void OnPause()
    {
        if (m_GameState == GameState.PlayingState) {
            StopGameplay();
            m_ResumeButton.SetActive(true);
            m_ExitMenuButton.SetActive(true);
            m_GameState = GameState.PausedState;
        }
    }

    public void OnResume()
    {
        if (m_GameState == GameState.PausedState) {
            m_ExitMenuButton.SetActive(false);
            m_ResumeButton.SetActive(false);
            StartGameplay();
            m_GameState = GameState.PlayingState;
        }
    }

    public void OnReplay(bool isClearGame = false)
    {
        if (isClearGame) {
            m_PlayerA.m_Score = 0;
            m_PlayerB.m_Score = 0;
            m_PlayerAWinImage.SetActive(false);
            m_PlayerBWinImage.SetActive(false);
        }

        m_GameState = GameState.CountDownToStartState;
        m_InitialTime = Time.time;
        
        HideRestartButton();
        HideExitMenuButton();

        m_PlayerA.Restart();
        m_PlayerB.Restart();
    }
}
