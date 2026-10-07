using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例模式改造器
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingleMomoBase<T> : MonoBehaviour where T : SingleMomoBase<T>
{
    public static T INSTANCE;

    protected virtual void Awake()
    {
        if (INSTANCE != null) 
        {
            Debug.LogError(this + "不符合单例模式");
        }
        INSTANCE = (T)this;
    }

    protected virtual void OnDestroy()
    {
        Destroy();         
    }

    /// <summary>
    /// 清除子类单例
    /// </summary>
    public void Destroy()
    {
        INSTANCE = null;
    }
}
