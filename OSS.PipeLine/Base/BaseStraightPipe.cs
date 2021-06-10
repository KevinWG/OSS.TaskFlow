﻿#region Copyright (C) 2016 Kevin (OSS开源系列) 公众号：OSSCore

/***************************************************************************
*　　	文件功能描述：OSS.EventFlow - 流体基础管道部分
*
*　　	创建人： Kevin
*       创建人Email：1985088337@qq.com
*       创建时间： 2020-11-22
*       
*****************************************************************************/

#endregion

using System.Threading.Tasks;
using OSS.Pipeline.Interface;
using OSS.Pipeline.InterImpls.Watcher;

namespace OSS.Pipeline.Base
{
    /// <summary>
    ///  管道执行基类（直通类型）
    /// </summary>
    /// <typeparam name="TInContext"></typeparam>
    /// <typeparam name="TOutContext"></typeparam>
    public abstract class BaseStraightPipe<TInContext,TOutContext> : BasePipe<TInContext, TInContext, TOutContext>,IPipeExecutor<TInContext>
    {
        /// <inheritdoc />
        protected BaseStraightPipe(PipeType pipeType) : base(pipeType)
        {
        }

        #region 流体业务-启动

        /// <summary>
        /// 启动方法
        /// </summary>
        /// <returns></returns>
        public async Task<TrafficSignal> Execute(TInContext para)
        {
            return (await InterStart(para)).traffic_signal;
        }

        #endregion

        #region 流体业务-内部处理

        /// <summary>
        ///  管道处理实际业务流动方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal override async Task<InterSingleValue> InterStart(TInContext context)
        {
            await Watch(PipeCode, PipeType, WatchActionType.Starting, context);
            var res = await InterHandling(context);

            if (res.traffic_signal == TrafficSignal.Red_Block)
            {
                await InterBlock(context,res.blocked_pipe_code);
            }

            return res;
        }

        /// <summary>
        ///  管道处理实际业务流动方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal abstract Task<InterSingleValue> InterHandling(TInContext context);
        
        #endregion

    }
}