using System;
using UnityEngine;

public class UILookAt : MonoBehaviour
{
    public Camera mcamera;

    private void Awake()
    {
        mcamera = Camera.main;
    }

    void Update()
    {
        transform.LookAt(mcamera.transform);
        transform.Rotate(0,180,0);
    }
}
