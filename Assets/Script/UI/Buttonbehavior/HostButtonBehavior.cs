using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HostButtonBehavior : MonoBehaviour
{
    [SerializeField] private Button myButton;
    //[SerializeField] private Animator buttonAnimator;
    AudioManager audioManager;
    void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);
        audioManager =GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void OnButtonClick()
    {
        Debug.Log("Host Clicked!");
        audioManager.PlaySFX(audioManager.clickbutton);
    }
}
