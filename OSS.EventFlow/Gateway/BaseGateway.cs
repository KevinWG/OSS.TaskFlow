﻿#region Copyright (C) 2020 Kevin (OSS开源系列) 公众号：OSSCore

/***************************************************************************
*　　	文件功能描述：OSS.EventFlow -  网关基类
*
*　　	创建人： Kevin
*       创建人Email：1985088337@qq.com
*       创建时间： 2020-11-22
*       
*****************************************************************************/

#endregion

using OSS.EventFlow.Mos;

namespace OSS.EventFlow.Gateway
{
    public abstract class BaseGateway<TContext> : BasePipe<TContext>
        where TContext : FlowContext
    {
        protected BaseGateway() : base(PipeType.Gateway)
        {
        }


        //  todo Gateway 多入多出



    }

    //    public abstract class BaseGateway
    //    {
    //        public GatewayType GatewayType { get; internal set; }

    //        protected BaseGateway(GatewayType gatewayType)
    //        {
    //            GatewayType = gatewayType;
    //        }

    //        /// <summary>
    //        ///  聚合控制检查是否满足条件
    //        /// </summary>
    //        /// <param name="preData"></param>
    //        /// <returns>true - 满足条件，false- 不能满足条件</returns>
    //        protected internal virtual Task<bool> AggregateCheck()
    //        {
    //            return Task.FromResult(true);
    //        }

    //        /// <summary>
    //        ///  聚合控制释放，不再检查
    //        /// </summary>
    //        /// <param name="preData"></param>
    //        /// <returns></returns>
    //        protected internal virtual Task<bool> AggregateRelease()
    //        {
    //            return Task.FromResult(true);
    //        }

    //        internal async Task MoveNext()
    //        {
    //            var aCheck = await AggregateCheck();
    //            if (!aCheck)
    //            {
    //                var release = await AggregateRelease();
    //                if (release)
    //                {
    //                    return ;
    //                }
    //            }
    //            await MoveSubNext();
    //        }
    //        internal abstract Task MoveSubNext( );


    //        /// <summary>
    //        ///   多agent前进 
    //        /// </summary>
    //        /// <param name="preData"></param>
    //        /// <returns></returns>
    //        internal static async Task MoveMulitAgents( BaseMsgTunnel[] agents)
    //        {
    //            foreach (var ag in agents)
    //            {
    //                await ag.MoveIn();
    //            }
    //        }

    //        /// <summary>
    //        /// 单一agent前进
    //        /// </summary>
    //        /// <param name="preData"></param>
    //        /// <returns></returns>
    //        internal static  Task MoveSingleAgents( BaseMsgTunnel agent)
    //        {
    //            return agent.MoveIn();
    //        }

    //    }
}
