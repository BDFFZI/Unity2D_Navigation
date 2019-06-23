using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 导航区块 : MonoBehaviour
{
    public int 总距离;
    public int x;
    public int y;
    public int 起点距离;
    public int 终点距离;
    public bool 墙壁;

    public 导航区块 上一个路径点;

    public void 初始化(int x, int y, int 起点距离, int 终点距离)
    {
        this.x = x;
        this.y = y;
        this.起点距离 = 起点距离;
        this.终点距离 = 终点距离;
        总距离 = 起点距离 + 终点距离;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        封路();
    }

    public void 封路()
    {
        墙壁 = true;
        GetComponent<SpriteRenderer>().color = Color.black;
    }
}
