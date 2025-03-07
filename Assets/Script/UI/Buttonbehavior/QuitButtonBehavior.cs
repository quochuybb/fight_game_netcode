using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuitButtonBehavior : MonoBehaviour
{
    [SerializeField] private Button myButton;
    [SerializeField] private Animator buttonAnimator;
    AudioManager audioManager;
    void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);
        audioManager =GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void OnButtonClick()
    {
        Debug.Log("Quit Clicked!");
        audioManager.PlaySFX(audioManager.clickbutton);
        if (buttonAnimator) buttonAnimator.SetTrigger("Click");  // Play animation
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop Play Mode in Editor
        #else
        Application.Quit(); // Quit the actual game
        #endif
    }
}
