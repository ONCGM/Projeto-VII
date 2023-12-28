using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageCanvas : MonoBehaviour {
    public int damageValue = 1;
    
    private void Start() {
        var camera = GameObject.FindObjectOfType<Camera>();
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = camera;

        camera.transform.LookAt(camera.transform);

        var text = GetComponentInChildren<TMP_Text>();
        text.text = damageValue.ToString();
        DOTween.To(()=> text.alpha, x=> text.alpha = x, 0f, 2f);

        transform.DOMoveY(5f, 2f).onComplete += () =>  Destroy(gameObject);
        
    }
}
