using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // So CPU doesn't turn into a George Foreman Grill
        Application.targetFrameRate = 60;
    }
}
