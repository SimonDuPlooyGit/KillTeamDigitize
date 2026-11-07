using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialStepAction
{
    public string stepName = "New Step";

    [Header("Panels to Enable/Disable")]
    public List<GameObject> panelsToEnable = new List<GameObject>();
    public List<GameObject> panelsToDisable = new List<GameObject>();
    [Header("Optional Scene Transition")]
    public string sceneToLoad = "";
}
