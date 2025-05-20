using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AnimationChest : NetworkBehaviour
{
    private Animator animator;
    private readonly int isOpen = Animator.StringToHash("isOpen");
    private ChestController chestController;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        chestController = GetComponent<ChestController>();
    }

    private void Start()
    {
        chestController.onOpenChest.AddListener(OnOpenChest);
    }

    private void OnOpenChest(Transform transform)
    {
        animator.SetBool(isOpen, true); 
    }

    private void OnCloseChest()
    {
        animator.SetBool(isOpen, false);
    }
}
