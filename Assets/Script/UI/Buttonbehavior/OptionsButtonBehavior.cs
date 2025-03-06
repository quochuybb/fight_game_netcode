using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsButtonBehavior : MonoBehaviour
{
    [SerializeField] private Button myButton;
    [SerializeField] private AudioSource clickSound;
    [SerializeField] private Animator buttonAnimator;

    void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        Debug.Log("Quit Clicked!");
        if (clickSound) clickSound.Play();  // Play sound
        if (buttonAnimator) buttonAnimator.SetTrigger("Click");  // Play animation
    }
}
