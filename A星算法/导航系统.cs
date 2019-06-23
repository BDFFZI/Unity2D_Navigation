using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 导航系统 : MonoBehaviour
{
    public int 起点x;
    public int 起点y;
    public int 终点x;
    public int 终点y;

    private List<导航区块> 开启列表 = new List<导航区块>();
    private List<导航区块> 关闭列表 = new List<导航区块>();
    导航区块 最小路径点 = null;

    public static 导航系统 寻路;
    private void Awake()
    {
        寻路 = this;
    }

    public void Play()
    {
        StartCoroutine("开始寻路");
    }

    IEnumerator 开始寻路()
    {
        WaitForSeconds s = new WaitForSeconds(0.03f);
        最小路径点 = 导航烘焙.数据[起点x, 起点y].GetComponent<导航区块>();
        开启列表.Add(最小路径点);
        while (开启列表.Count != 0)
        {
            导航区块 临时路径点 = 最小路径点;
            开启列表.Remove(最小路径点);
            关闭列表.Add(最小路径点);

            List<导航区块> 周围路径 = 寻找周围路径(最小路径点);
            foreach (导航区块 路径点 in 周围路径)
            {
                if (!开启列表.Contains(路径点))
                {
                    路径点.上一个路径点 = 最小路径点;
                    路径点.起点距离 = 最小路径点.起点距离+1;
                    路径点.总距离 = 路径点.起点距离 + 路径点.终点距离;
                    开启列表.Add(路径点);
                }
                else if (路径点.起点距离 > 最小路径点.起点距离 + 1)
                {
                    路径点.上一个路径点 = 最小路径点;
                    路径点.起点距离 = 最小路径点.起点距离 + 1;
                    路径点.总距离 = 路径点.起点距离 + 路径点.终点距离;
                }
            }
            最小路径点 = 寻找最小终点距离(开启列表);
            导航烘焙.数据[临时路径点.x, 临时路径点.y].GetComponent<SpriteRenderer>().color = Color.yellow;
            yield return s;
            导航烘焙.数据[临时路径点.x, 临时路径点.y].GetComponent<SpriteRenderer>().color = Color.white;
            if (最小路径点.终点距离 == 0)
            {
                do
                {
                    导航烘焙.数据[最小路径点.x, 最小路径点.y].GetComponent<SpriteRenderer>().color = Color.blue;
                    最小路径点 = 最小路径点.上一个路径点;
                    yield return s;
                } while (最小路径点.上一个路径点 != null);
                break;
            }
        }

        导航烘焙.数据[寻路.起点x, 寻路.起点y].GetComponent<SpriteRenderer>().color = Color.red;
        导航烘焙.数据[寻路.终点x, 寻路.终点y].GetComponent<SpriteRenderer>().color = Color.green;
    }

    private 导航区块 寻找最小终点距离(List<导航区块> 列表)
    {
        导航区块 最小距离点 = null;
        int 最小距离 = int.MaxValue;
        foreach (导航区块 i in 列表)
        {
            if (i.总距离 < 最小距离)
            {
                最小距离点 = i;
                最小距离 = i.总距离;
            }
        }
        return 最小距离点;
    }

    private List<导航区块> 寻找周围路径(导航区块 路径点)
    {
        List<导航区块> 列表 = new List<导航区块>();

        for (int i = 0; i < 4; i++)
        {
            导航区块 临时路径点 = null;
            int x = 路径点.x;
            int y = 路径点.y;
            switch (i)
            {
                case 0: y++; break;
                case 1: x++; break;
                case 2: y--; break;
                case 3: x--; break;
            }

            try
            {
                临时路径点 = 导航烘焙.数据[x, y].GetComponent<导航区块>();
                if (!临时路径点.墙壁 && !关闭列表.Contains(临时路径点))
                    列表.Add(临时路径点);
            }
            catch { continue; }

        }
        return 列表;
    }
}
