using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MenuTransition : MonoBehaviour
{
    [SerializeField] private RectTransform Online;

    public void OnlineButtonPressed()
    {
        Online.DOAnchorPos(new Vector2(0,1000), 1, false);
    }
}
