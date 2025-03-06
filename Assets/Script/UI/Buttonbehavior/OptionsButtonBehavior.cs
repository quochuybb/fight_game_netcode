using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsButtonBehavior : MonoBehaviour
{
    [SerializeField] private Button myButton;
    [SerializeField] private Animator buttonAnimator;
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
        Debug.Log("Option Clicked!");
        audioManager.PlaySFX(audioManager.clickbutton);
        //if (buttonAnimator) buttonAnimator.SetTrigger("Click");  // Play animation
        uiManager.showoptionsmenu();
    }
}
