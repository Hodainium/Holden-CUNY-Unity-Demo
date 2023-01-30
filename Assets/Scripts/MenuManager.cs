using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject _playerObjects;
    
    [SerializeField] GameObject _mainMenuElements;
    [SerializeField] GameObject _creditMenuElements;
    [SerializeField] GameObject _replayMenuElements;
    [SerializeField] GameObject _gameHudElements;

    [SerializeField] GameObject _mainMenuCamera;
    [SerializeField] GameObject _playerCamera;

    public void OnClickPlayButton() //Can't have it just be re-enabled should always be on
    {
        _playerObjects.SetActive(true);
        _gameHudElements.SetActive(true);
        _mainMenuCamera.SetActive(false);
        _mainMenuElements.SetActive(false);
    }

    public void OnClickCreditsButton()
    {
        _mainMenuElements.SetActive(false);
        _creditMenuElements.SetActive(true);
    }

    public void OnClickExitCreditsButton()
    {
        _creditMenuElements.SetActive(false);
        _mainMenuElements.SetActive(true);
    }

    public void OnQuitButtonClick()
    {
        Debug.Log("Game quit");
        Application.Quit();
    }

    public void OnEnterReplayMenu()
    {
        _replayMenuElements.SetActive(true);
        _gameHudElements.SetActive(false);
        //_playerObjects.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void OnClickReplayButton()
    {
        //Reload the scene
        SceneManager.LoadScene(0);
    }

    
}
