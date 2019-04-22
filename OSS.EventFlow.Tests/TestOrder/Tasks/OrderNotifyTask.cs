﻿using System.Threading.Tasks;
using OSS.Common.Plugs.LogPlug;
using OSS.EventFlow.Tasks;
using OSS.EventFlow.Tasks.Mos;

namespace OSS.EventFlow.Tests.TestOrder.Tasks
{
    public class OrderNotifyTask : BaseTask<OrderInfo,TaskResultMo>
    {
        protected override Task<TaskResultMo> Do(TaskContext<OrderInfo> context)
        {
            LogUtil.Info("执行通知！");
            // 手动表示执行出错！
            return Task.FromResult(new TaskResultMo((int) TaskResultType.Failed));
        }
    }
    
    public class NotifyMsg
    {
        public string name { get; set; }

        public string msg { get; set; }

        public string num { get; set; }
    }
}
