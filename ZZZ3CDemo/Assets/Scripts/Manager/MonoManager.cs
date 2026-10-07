using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoManager : SingleMomoBase<MonoManager>
{
    //update任务集合
    public Action updatAction;
    //fixedUpdate任务集合
    public Action fixedUpdatAction;
    //lateUpdate任务集合
    public Action lateUpdatAction;

    /// <summary>
    /// 添加update任务
    /// </summary>
    /// <param name="task">任务</param>
    public void AddUpdateAction(Action task)
    {
        updatAction += task;
    }

    /// <summary>
    /// 移除update任务
    /// </summary>
    /// <param name="task">任务</param>
    public void RemoveUpdateAction(Action task)
    {
        updatAction -= task;
    }

    /// <summary>
    /// 添加fixedUpdate任务
    /// </summary>
    /// <param name="task">任务</param>
    public void AddFixedUpdateAction(Action task)
    {
        fixedUpdatAction += task;
    }

    /// <summary>
    /// 移除fixedUpdate任务
    /// </summary>
    /// <param name="task">任务</param>
    public void RemoveFixedUpdateAction(Action task)
    {
        fixedUpdatAction -= task;
    }

    /// <summary>
    /// 添加lateUpdate任务
    /// </summary>
    /// <param name="task">任务</param>
    public void AddLateUpdateAction(Action task)
    {
        lateUpdatAction += task;
    }

    /// <summary>
    /// 移除lateUpdate任务
    /// </summary>
    /// <param name="task">任务</param>
    public void RemoveLateUpdateAction(Action task)
    {
        lateUpdatAction -= task;
    }

    void Update()
    {
        updatAction?.Invoke();
    }

    void FixedUpdate()
    {
        fixedUpdatAction?.Invoke();
    }
     void LateUpdate()
    {
        lateUpdatAction?.Invoke();
    }
}
