using System;
using UnityEngine;

public class UILookAt : MonoBehaviour
{
    public Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    void Update()
    {
        transform.LookAt(camera.transform);
        transform.Rotate(0,180,0);
    }
}
