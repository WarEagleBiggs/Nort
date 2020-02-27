using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public GameObject m_AboutPage;
    
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
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
