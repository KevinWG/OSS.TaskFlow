﻿#region Copyright (C) 2020 Kevin (OSS开源系列) 公众号：OSSCore

/***************************************************************************
*　　	文件功能描述：OSS.EventFlow -  活动基类
*
*　　	创建人： Kevin
*       创建人Email：1985088337@qq.com
*       创建时间： 2020-11-22
*       
*****************************************************************************/

#endregion

using System;
using System.Threading.Tasks;
using OSS.Pipeline.Base;
using OSS.Pipeline.Interface;
using OSS.Pipeline.InterImpls.Watcher;

namespace OSS.Pipeline
{
    /// <summary>
    /// 主动触发执行活动组件基类(不接收上下文)
    /// </summary>
    public abstract class BaseActivity : BaseStraightPipe<EmptyContext, EmptyContext>, IActivity<EmptyContext>
    {
        /// <summary>
        /// 外部Action活动基类
        /// </summary>
        protected BaseActivity() : base(PipeType.Activity)
        {
        }

        /// <summary>
        ///  具体执行扩展方法
        /// </summary>
        /// <returns>
        /// 处理结果
        /// traffic_signal：     
        ///     Green_Pass  - 流体自动流入后续管道
        ///     Yellow_Wait - 暂停执行，既不向后流动，也不触发Block。
        ///     Red_Block - 触发Block，业务流不再向后续管道传递。
        /// </returns>
        protected abstract Task<TrafficSignal> Executing();

        internal override async Task<InterSingleValue> InterHandling(EmptyContext context)
        {
            var res = await Executing();
            await Watch(PipeCode, PipeType, WatchActionType.Executed, context,res);
            if (res == TrafficSignal.Green_Pass)
            {
                return await ToNextThrough(context);
            }
            else if (res == TrafficSignal.Red_Block)
            {
                return new InterSingleValue(TrafficSignal.Red_Block, PipeCode);
            }
            return new InterSingleValue(res, String.Empty);
        }


        #region 流体业务-启动

        /// <summary>
        /// 启动方法
        /// </summary>
        /// <returns></returns>
        public Task<TrafficSignal> Execute()
        {
            return Execute(EmptyContext.Default);
        }

        #endregion
    }

    /// <summary>
    ///  主动触发执行活动组件基类
    ///    接收输入上下文，且此上下文继续传递下一个节点
    /// </summary>
    /// <typeparam name="TInContext">输入输出上下文</typeparam>
    public abstract class BaseActivity<TInContext> : BaseStraightPipe<TInContext, TInContext>, IActivity<TInContext, TInContext>
    {
        /// <summary>
        /// 外部Action活动基类
        /// </summary>
        protected BaseActivity() : base(PipeType.Activity)
        {
        }
        /// <summary>
        ///  具体执行扩展方法
        /// </summary>
        /// <param name="data">当前活动上下文（会继续传递给下一个节点）</param>
        /// <returns>
        /// 处理结果
        /// traffic_signal：     
        ///     Green_Pass  - 流体自动流入后续管道
        ///     Yellow_Wait - 暂停执行，既不向后流动，也不触发Block。
        ///     Red_Block - 触发Block，业务流不再向后续管道传递。
        /// </returns>
        protected abstract Task<TrafficSignal> Executing(TInContext data);

        internal override async Task<InterSingleValue> InterHandling(TInContext context)
        {
            var res = await Executing(context);
            await Watch(PipeCode, PipeType, WatchActionType.Executed, context, res);
           
            if (res == TrafficSignal.Green_Pass)
            {
                return await ToNextThrough(context);
            }
            else if(res == TrafficSignal.Red_Block)
            {
                return new InterSingleValue(TrafficSignal.Red_Block, PipeCode);
            }
            return new InterSingleValue(res,String.Empty);
        }
    }
}
