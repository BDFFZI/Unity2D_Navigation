#define 关闭     //调试模式:仅可视化/开启/关闭
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 导航系统 : MonoBehaviour
{
    public static 导航系统 Get;

    private void Awake()
    {
        Get = this;
    }

    public Stack<路径区块> 导航(路径区块 起点, 路径区块[,] 导航数据)
    {
        if (导航数据 == null)
        {
#if 开启
            Debug.LogWarning("导航:无法获取导航数据");
#endif
            return null;
        }

        List<路径区块> 开启列表 = new List<路径区块>();
        List<路径区块> 关闭列表 = new List<路径区块>();
        Stack<路径区块> 路径列表 = new Stack<路径区块>();
        路径区块 当前路径点 = 起点;
        当前路径点.起点距离 = 0;
        开启列表.Add(当前路径点);

        while (开启列表.Count != 0)
        {
            路径区块 临时路径点 = 当前路径点;
            开启列表.Remove(当前路径点);
            关闭列表.Add(当前路径点);

            List<路径区块> 周围路径 = 寻找周围路径(当前路径点, 关闭列表, 导航数据);
            foreach (路径区块 路径点 in 周围路径)
            {
                if (!开启列表.Contains(路径点))
                {
                    路径点.上一个路径点 = 当前路径点;
                    路径点.起点距离 = 当前路径点.起点距离 + 1;
                    路径点.总距离 = 路径点.起点距离 + 路径点.终点距离;
                    开启列表.Add(路径点);
                }
                else if (路径点.起点距离 > 当前路径点.起点距离 + 1)
                {
                    路径点.上一个路径点 = 当前路径点;
                    路径点.起点距离 = 当前路径点.起点距离 + 1;
                    路径点.总距离 = 路径点.起点距离 + 路径点.终点距离;
                }
            }
            当前路径点 = 寻找最小终点距离(开启列表);
            //录入路径
            if (当前路径点.终点距离 == 0)
            {
                do
                {
#if 开启 || 仅可视化
                    导航烘焙.烘焙数据[当前路径点.x, 当前路径点.y].GetComponent<SpriteRenderer>().color = Color.blue;
#endif
                    路径列表.Push(当前路径点);
                    当前路径点 = 当前路径点.上一个路径点;
                } while (当前路径点.上一个路径点 != null);
                break;
            }
        }
        return 路径列表;
    }
    private 路径区块 寻找最小终点距离(List<路径区块> 列表)
    {
        if (列表.Count == 0)
        {
#if 开启
            Debug.LogWarning("寻找最小终点距离:列表为空");
#endif
            return null;
        }
        路径区块 最小距离点 = null;
        float 最小距离 = int.MaxValue;
        foreach (路径区块 i in 列表)
        {
            if (i.总距离 < 最小距离)
            {
                最小距离点 = i;
                最小距离 = i.总距离;
            }
            else if (i.总距离 >= int.MaxValue)
            {
                Debug.Log(i.ToString() + "数据异常");
            }
        }
        return 最小距离点;
    }
    private List<路径区块> 寻找周围路径(路径区块 中心路径点, List<路径区块> 关闭列表, 路径区块[,] 导航数据)
    {
        List<路径区块> 列表 = new List<路径区块>();

        for (int i = 0; i < 9; i++)
        {
            int x = 中心路径点.x - 导航数据[0, 0].x;
            int y = 中心路径点.y - 导航数据[0, 0].y;
            switch (i)
            {
                case 0: break;
                case 1: y++; break;
                case 2: x++; break;
                case 3: y--; break;
                case 4: x--; break;
                case 5: x++; y++; break;
                case 6: x++; y--; break;
                case 7: x--; y--; break;
                case 8: x--; y++; break;
            }

            try
            {
                if (!导航数据[x, y].障碍物 && !关闭列表.Contains(导航数据[x, y]))
                {
#if 开启 || 仅可视化
                    导航烘焙.烘焙数据[x + 导航数据[0, 0].x, y + 导航数据[0, 0].y].GetComponent<SpriteRenderer>().color = Color.green;
#endif
                    导航数据[x, y].起点距离 = Mathf.Sqrt(Mathf.Abs(x - 中心路径点.x) + Mathf.Abs(y - 中心路径点.x));
                    列表.Add(导航数据[x, y]);
                }
            }
            catch { continue; }
        }
#if 开启
        if (列表.Count == 0)
            Debug.Log("寻找周围路径:未发现可走路径" + 导航烘焙.烘焙数据[中心路径点.x, 中心路径点.y].路径数据.ToString());
#endif
        return 列表;
    }
}
