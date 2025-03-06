using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClickStartButton : MonoBehaviour
{
    [SerializeField] private Button myButton;
    [SerializeField] private Animator buttonAnimator; // For animations
    AudioManager audioManager;
    void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);
        audioManager =GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void OnButtonClick()
    {
        Debug.Log("Play Clicked!");
        audioManager.PlaySFX(audioManager.clickbutton);
        //if (buttonAnimator) buttonAnimator.SetTrigger("Click");  // Play animation
    }
}

