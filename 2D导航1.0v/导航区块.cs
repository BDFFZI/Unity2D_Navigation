using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 导航区块 : MonoBehaviour
{
    public 路径区块 路径数据 = new 路径区块();

    public void 封路()
    {
        路径数据.障碍物 = true;
        GetComponent<SpriteRenderer>().color = Color.black;
    }
    
    private void Start()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.5f * transform.lossyScale.x, 1 << LayerMask.NameToLayer("障碍物")))      //请将路障设为"障碍物"层
            封路();
        gameObject.SetActive(false);      //调试时请关闭
    }
}

[System.Serializable]
public class 路径区块
{
    //在原始烘焙数据的数组中的坐标
    public int x;
    public int y;

    public float 起点距离 = float.MaxValue;
    public float 终点距离 = float.MaxValue;
    public float 总距离 = float.MaxValue;
    public bool 障碍物;
    public 路径区块 上一个路径点;     //便于输出路径

    public 路径区块(路径区块 路径数据)
    {
        x = 路径数据.x;
        y = 路径数据.y;
        障碍物 = 路径数据.障碍物;
    }

    public 路径区块()
    {
    }

    public void 权重初始化(float 起点距离, float 终点距离)
    {
        this.起点距离 = 起点距离;
        this.终点距离 = 终点距离;
        总距离 = 起点距离 + 终点距离;
    }

    public void 初始化(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return ("坐标:(" + x + "," + y + ")");
    }

    public static float operator-(路径区块 a, 路径区块 b)
    {
        float x = Mathf.Abs(a.x - b.x);
        float y = Mathf.Abs(a.y - b.y);
        return Mathf.Sqrt(x * x + y * y);
    }
}
