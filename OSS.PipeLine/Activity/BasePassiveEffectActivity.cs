﻿#region Copyright (C) 2020 Kevin (OSS开源系列) 公众号：OSSCore

/***************************************************************************
*　　	文件功能描述：OSS.EventFlow -  外部动作活动
*
*　　	创建人： Kevin
*       创建人Email：1985088337@qq.com
*       创建时间： 2020-11-22
*       
*****************************************************************************/

#endregion

using System.Threading.Tasks;
using OSS.Pipeline.Interface;
using OSS.Pipeline.Activity.Base;

namespace OSS.Pipeline
{
    /// <summary>
    ///  被动触发执行活动组件基类
    ///      传入TPassivePara类型参数，自身返回处理结果，且结果作为上下文传递给下一个节点
    /// </summary>
    /// <typeparam name="TPassivePara"></typeparam>
    /// <typeparam name="TPassiveResult"></typeparam>
    public abstract class BasePassiveEffectActivity<TPassivePara, TPassiveResult> :
        BaseThreeWayPassiveActivity<TPassivePara, TPassiveResult, TPassiveResult>, IPassiveEffectActivity<TPassivePara, TPassiveResult>
    {
        /// <summary>
        /// 外部Action活动基类
        /// </summary>
        protected BasePassiveEffectActivity(string pipeCode = null) : base(pipeCode, PipeType.Activity,true,true)
        {
        }
        
        internal override async Task<TrafficResult<TPassiveResult, TPassiveResult>> InterProcessPackage(TPassivePara context, string prePipeCode)
        {
            var tSignal = await Executing(context);
            return new TrafficResult<TPassiveResult, TPassiveResult>(tSignal,
                tSignal.signal == SignalFlag.Red_Block ? PipeCode : string.Empty, tSignal.result);
        }
    }
}