﻿using UnityEngine;
using System.Collections;

public class GameRoot : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnGUI()
    {
        //设置按钮中文字的颜色  
        GUI.color = Color.green;
        //设置按钮的背景色  
        GUI.backgroundColor = Color.red;

        if (GUI.Button(new Rect(100, 100, 300, 40), "你已经在游戏场景了"))
        {
            //开始游戏按钮被按下
        }
    }
}