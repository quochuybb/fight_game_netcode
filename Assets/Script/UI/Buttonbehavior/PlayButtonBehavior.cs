using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClickStartButton : MonoBehaviour
{
    [SerializeField] private Button myButton;
    //[SerializeField] private Animator buttonAnimator; // For animations
    private UIManager uiManager;
    AudioManager audioManager;
    void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);
        uiManager = FindObjectOfType<UIManager>();
        audioManager =GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void OnButtonClick()
    {
  
        audioManager.PlaySFX(audioManager.clickbutton);
        //if (buttonAnimator) buttonAnimator.SetTrigger("Click");  // Play animation
        uiManager.showplaymenu();
        Debug.Log("Play Clicked!");
    }
}

