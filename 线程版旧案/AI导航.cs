using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class AI导航 : MonoBehaviour
{
    public Transform 目标;
    [Range(0f, 0.2f)]
    public float 移动速度;
    public float 刹车距离;
    public Stack<路径区块> 路径;

    private Vector2 记忆坐标;
    private Thread 寻路;

    private void Awake()
    {
        刹车距离 = 刹车距离 * 刹车距离;
        记忆坐标 = transform.position;
    }

    private void Start()
    {
        StartCoroutine("更新目标");
    }

    IEnumerator 更新目标()
    {
        WaitForSeconds s = new WaitForSeconds(0.01f);
        while (true)
        {
            if ((记忆坐标 - (Vector2)目标.position).sqrMagnitude > 刹车距离)
            {
                记忆坐标 = 目标.position;
                路径 = null;
                if (寻路 != null)
                    寻路.Abort();
                寻路 = new Thread(导航系统.Get.导航);
                寻路.Start(new 导航数据(this, 导航烘焙.Get.导航权重绘制(导航烘焙.Get.空间坐标转导航区块(transform.position), 导航烘焙.Get.空间坐标转导航区块(目标.position)), 导航烘焙.Get.空间坐标转导航区块(transform.position).路径数据));
                StopCoroutine("移动");
                StartCoroutine("移动");
                yield return s;
            }
            else
                yield return null;
        }
    }

    IEnumerator 移动()
    {
        WaitForSeconds s = new WaitForSeconds(0.01f);
        while (路径 == null) { yield return null; }
        while (路径.Count > 0)
        {
            路径区块 i = 路径.Pop();
            Vector2 路径点 = 导航烘焙.烘焙数据[i.x, i.y].transform.position;
            while (((Vector2)transform.position - 路径点).sqrMagnitude > 刹车距离)
            {
                transform.position += ((Vector3)路径点 - transform.position).normalized * 移动速度;
                yield return s;
            }
        }
        //else
        //    Debug.LogWarning("导航失败", gameObject);
    }
}
