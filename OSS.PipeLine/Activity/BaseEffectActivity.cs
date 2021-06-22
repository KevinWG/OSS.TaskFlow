﻿using System.Threading.Tasks;
using OSS.Pipeline.Base;
using OSS.Pipeline.Interface;

namespace OSS.Pipeline
{
    /// <summary>
    ///  主动触发执行活动组件基类
    ///       不接收上下文，自身返回处理结果，且结果作为上下文传递给下一个节点
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class BaseEffectActivity<TResult> : BaseStraightPipe<Empty, TResult>, IActivity<TResult>
    {
        /// <summary>
        /// 外部Action活动基类
        /// </summary>
        protected BaseEffectActivity() : base(PipeType.EffectActivity)
        {
        }

        #region 业务扩展方法

        /// <summary>
        ///  具体执行扩展方法
        /// </summary>
        /// <returns>
        /// (bool traffic_signal,TResult result)-（活动是否处理成功，业务结果）
        /// traffic_signal：
        /// traffic_signal：     
        ///     Green_Pass  - 流体自动流入后续管道
        ///     Yellow_Wait - 暂停执行，既不向后流动，也不触发Block。
        ///     Red_Block - 触发Block，业务流不再向后续管道传递。
        /// </returns>
        protected abstract Task<TrafficSignal<TResult>> Executing();


        #endregion

        #region 流体内部业务处理

        /// <inheritdoc />
        internal override async Task<TrafficResult<TResult, TResult>> InterHandlePack(Empty context)
        {
            var trafficRes = await Executing();
            return new TrafficResult<TResult, TResult>(trafficRes,
                trafficRes.signal == SignalFlag.Red_Block ? PipeCode : string.Empty, trafficRes.result);
        }


        #endregion
        #region 流体业务-启动

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public Task<TrafficResult> Execute()
        {
            return Execute(Empty.Default);
        }

        #endregion

    }

    /// <summary>
    ///  主动触发执行活动组件基类
    ///       接收上下文，自身返回处理结果，且结果作为上下文传递给下一个节点
    /// </summary>
    /// <typeparam name="TInContext"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public abstract class BaseEffectActivity<TInContext, TResult> : BaseStraightPipe<TInContext, TResult>, IActivity<TInContext, TResult>
    {
        /// <summary>
        /// 外部Action活动基类
        /// </summary>
        protected BaseEffectActivity() : base(PipeType.EffectActivity)
        {
        }

        #region 业务扩展方法

        /// <summary>
        ///  具体执行扩展方法
        /// </summary>
        /// <param name="para">当前活动上下文信息</param>
        /// <returns>
        /// (bool traffic_signal,TResult result)-（活动是否处理成功，业务结果）
        /// traffic_signal：     
        ///     Green_Pass  - 流体自动流入后续管道
        ///     Yellow_Wait - 暂停执行，既不向后流动，也不触发Block。
        ///     Red_Block - 触发Block，业务流不再向后续管道传递。
        /// </returns>
        protected abstract Task<TrafficSignal<TResult>> Executing(TInContext para);

        #endregion


        #region 流体内部业务处理

        /// <inheritdoc />
        internal override async Task<TrafficResult<TResult, TResult>> InterHandlePack(TInContext context)
        {
            var trafficRes = await Executing(context);
            return new TrafficResult<TResult, TResult>(trafficRes,
                trafficRes.signal == SignalFlag.Red_Block ? PipeCode : string.Empty, trafficRes.result);
        }

        #endregion
    }

}