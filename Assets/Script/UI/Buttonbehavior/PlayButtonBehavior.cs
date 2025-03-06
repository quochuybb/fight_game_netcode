using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClickStartButton : MonoBehaviour
{
    [SerializeField] private Button myButton;
    [SerializeField] private AudioSource clickSound; // For sound effects
    [SerializeField] private Animator buttonAnimator; // For animations

    void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        Debug.Log("Play Clicked!");
        if (clickSound) clickSound.Play();  // Play sound
        if (buttonAnimator) buttonAnimator.SetTrigger("Click");  // Play animation
    }
}

