﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OSS.Pipeline.Interface;
using OSS.Tools.Log;

namespace OSS.Pipeline.Tests.FlowItems
{
    /// <summary>
    /// 空活动
    /// </summary>
    public class EndGateway : BaseAggregateGateway<Empty>
    {
        protected override Task<TrafficSignal> Switch(Empty context, string prePipeCode, IList<IPipe> allPrePipes)
        {
            LogHelper.Info($"通过 {prePipeCode} 管道进入结束网关！");
            return Task.FromResult(TrafficSignal.GreenSignal);
        }
    }
}