﻿using System.Threading.Tasks;
using OSS.EventFlow.Activity;
using OSS.Tools.Log;

namespace OSS.TaskFlow.Tests.FlowItems
{
    public class ApplyActivity : BaseActivity<ApplyContext>
    {
        protected override Task<bool> Executing(ApplyContext data)
        {
            LogHelper.Info("这里刚才发生了一个采购申请"); 
            return Task.FromResult(true);
        }
    }

    public class ApplyContext : TestContext<string>
    {
    }
}