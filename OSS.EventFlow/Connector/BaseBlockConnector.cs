﻿#region Copyright (C) 2020 Kevin (OSS开源系列) 公众号：OSSCore

/***************************************************************************
*　　	文件功能描述：OSS.EventFlow -  异步阻塞连接基类
*
*　　	创建人： Kevin
*       创建人Email：1985088337@qq.com
*       创建时间： 2020-11-22
*       
*****************************************************************************/

#endregion

using System.Threading.Tasks;
using OSS.EventFlow.Interface;
using OSS.EventFlow.Mos;

namespace OSS.EventFlow.Connector
{
    public abstract class BaseBlockConnector<InContext, OutContext> : BaseConnector<InContext, OutContext>,
        IBlockTunnel<InContext>
        where InContext : FlowContext
        where OutContext : FlowContext
    {
        public abstract Task Push(InContext data);

        public Task Pop(InContext data)
        {
            var outContext = Convert(data);
            return NextPipe.Through(outContext);
        }

        internal override Task Through(InContext context)
        {
            return Push(context);
        }
    }


    public abstract class BaseBlockConnector<TContext> : BaseBlockConnector<TContext, TContext>,
        IBlockTunnel<TContext>
        where TContext : FlowContext
    {
        protected override TContext Convert(TContext inContextData)
        {
            return inContextData;
        }
    }
}
