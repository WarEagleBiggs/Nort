using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public GameObject m_AboutPage;
    
    public void Start2PlayerGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void Start1PlayerGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void OnAboutButton()
    {
        m_AboutPage.SetActive(!m_AboutPage.activeSelf);
    }

    public void OnAboutPageExit()
    {
        m_AboutPage.SetActive(false);
    }

}
