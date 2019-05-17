﻿using System;
using System.Threading.Tasks;
using OSS.Common.ComModels;
using OSS.Common.ComModels.Enums;
using OSS.Common.Extention;
using OSS.Common.Plugs.LogPlug;
using OSS.EventTask.Interfaces;
using OSS.EventTask.Mos;
using OSS.EventTask.Util;

namespace OSS.EventTask
{
    public abstract partial class BaseTask<TTData, TTRes>
    {
        #region 任务进入入口

        public Task<TaskResponse<TTRes>> Run(TTData req)=> Run(req, 0);
        
        public async Task<TaskResponse<TTRes>> Run(TTData req, int triedTimes)
        {
            var taskResp = new TaskResponse<TTRes>
            {
                task_cond = new RunCondition(){tried_times = triedTimes}
            };

            await TryRun(req, taskResp);
            return taskResp;
        }

        async Task<TaskResponse<ResultMo>> IBaseTask<TTData>.Run(TTData req, int triedTimes)
        {
            var taskResp = await Run(req, triedTimes);
            return new TaskResponse<ResultMo>()
            {
                run_status = taskResp.run_status,resp = taskResp.resp,task_cond = taskResp.task_cond
            };
        }

        #endregion

        #region 生命周期扩展方法

        /// <summary>
        /// 任务开始方法
        /// </summary>
        /// <param name="req"></param>
        /// <param name="loopTimes">循环执行次数，当次运行过程中的循环执行次数，默认是1</param>
        /// <param name="triedTimes">重新重试次数，默认是0</param>
        /// <returns></returns>
        protected virtual Task<ResultMo> RunStartCheck(TTData req, int loopTimes, int triedTimes)
        {
            return Task.FromResult(new ResultMo());
        }

        /// <summary>
        /// 任务结束方法
        /// </summary>
        /// <param name="req"></param>
        /// <param name="context">请求的上下文</param>
        /// <returns></returns>
        protected virtual Task RunEnd(TTData req, TaskResponse<TTRes> context)
        {
            return Task.CompletedTask;
        }
        
        #endregion

        #region 扩展方法（实现，回退，失败）  扩展方法

        /// <summary>
        ///     任务的具体执行
        /// </summary>
        /// <param name="req"></param>
        /// <param name="loopTimes">内部循环执行次数</param>
        /// <param name="triedTimes">重试运行次数</param>
        /// <returns> 
        ///  runStatus = TaskRunStatus.RunFailed 系统会字段判断是否满足重试条件执行重试
        /// </returns>
        protected abstract Task<DoResponse<TTRes>> Do(TTData req, int loopTimes, int triedTimes);

        /// <summary>
        ///  执行失败回退操作
        ///   如果设置了重试配置，调用后重试
        /// </summary>
        /// <param name="req"></param>
        /// <param name="triedTimes">重试运行次数</param>
        public virtual Task<bool> Revert(TTData req, int triedTimes)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        ///  最终失败执行方法
        /// </summary>
        /// <param name="req"></param>
        /// <param name="taskResp"></param>
        protected virtual Task FinallyFailed(TTData req, TaskResponse<TTRes> taskResp)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region 辅助方法

        // 运行
        private async Task TryRun(TTData req, TaskResponse<TTRes> taskResp)
        {
            string errorMsg;
            try
            {
                await Recurs(req, taskResp);
                return;
            }
            catch (ResultException e)
            {
                errorMsg = e.ToString();
                if (taskResp.resp == null)
                    taskResp.resp = e.ConvertToReultInherit<TTRes>(); //.ConvertToReult<TTRes>();
            }
            catch (Exception e)
            {
                errorMsg = e.ToString();
                if (taskResp.resp == null)
                    taskResp.resp = new TTRes().WithResult(SysResultTypes.ApplicationError,
                        "Error occurred during task [Run]!");
            }

            taskResp.run_status = TaskRunStatus.RunFailed;
            var resp = taskResp.resp;
            LogUtil.Error($"sys_ret:{resp.sys_ret}, ret:{resp.ret},msg:{resp.msg}, Detail:{errorMsg}",
                TaskMeta.task_id, ModuleName);
            await TrySaveTaskContext(req, taskResp);
        }

        /// <summary> 
        ///   任务的具体执行
        /// </summary>
        /// <param name="req"></param>
        /// <param name="taskResp"></param>
        /// <returns>  </returns>
        private async Task Recurs(TTData req, TaskResponse<TTRes> taskResp)
        {
            var runCondition = taskResp.task_cond;
            do
            {
                taskResp.run_status = TaskRunStatus.WaitToRun;

                await RunLife(req, taskResp);
                runCondition.loop_times++;
            }
            while (taskResp.run_status.IsFailed() && runCondition.loop_times <= TaskMeta.loop_times);
            runCondition.run_timestamp = DateTime.Now.ToUtcSeconds();

            // 判断是否间隔执行,生成重试信息
            if (taskResp.run_status.IsFailed()
                && runCondition.tried_times < TaskMeta.retry_times)
            {
                runCondition.tried_times++;
                runCondition.next_timestamp = runCondition.run_timestamp + TaskMeta.retry_seconds;

                taskResp.run_status = TaskRunStatus.RunPaused;  
                await TrySaveTaskContext(req, taskResp);
            }

            //  最终失败，执行失败方法
            if (taskResp.run_status.IsFailed())
                await FinallyFailed(req, taskResp);
        }

        //  一个完整执行经历的方法
        private async Task RunLife(TTData req, TaskResponse<TTRes> taskResp)
        {
            // 【1】 执行起始方法 附加校验
            var checkRes = await RunCheck(req, taskResp);
            if (!checkRes)
                return;

            //  直接执行
            var condition = taskResp.task_cond;
            var doResp = await TryDo(req, condition.loop_times, condition.tried_times);
            doResp.SetToTaskResp(taskResp);

            // 判断是否失败回退
            if (doResp.run_status.IsFailed())
                await Revert(req, condition.tried_times);
           
            // 【3】 执行结束方法
            await RunEnd(req, taskResp);
        }


        private async Task<bool> RunCheck(TTData req, TaskResponse<TTRes> taskResp)
        {
            if (string.IsNullOrEmpty(TaskMeta?.task_id))
            {
                taskResp.run_status = TaskRunStatus.RunFailed;
                taskResp.resp = new TTRes().WithResult(SysResultTypes.ApplicationError, "Task metainfo is null!");
                return false;
            }

            var condition = taskResp.task_cond;
            var res = await RunStartCheck(req, condition.loop_times, condition.tried_times);

            if (!res.IsSuccess())
            {
                taskResp.run_status = TaskRunStatus.RunFailed;
                taskResp.resp = res.ConvertToResultInherit<TTRes>();
                return false;
            }

            return true;
        }
        
        //  保证外部异常不会对框架内部运转造成影响
        //  如果失败返回 RunFailed 保证系统后续重试处理
        private async Task<DoResponse<TTRes>> TryDo(TTData req, int loopTimes, int triedTimes)
        {
            var doRes = default(DoResponse<TTRes>);
            try
            {
                doRes = await Do(req, loopTimes, triedTimes);
                if (doRes.resp == null)
                {
                    doRes.resp = new TTRes().WithResult(SysResultTypes.NoResponse,"Have no response during task [Do]!");
                }
            }
            catch (Exception e)
            {
                if (doRes == null)
                    doRes = new DoResponse<TTRes>();

                doRes.run_status = TaskRunStatus.RunFailed;
                doRes.resp =
                    new TTRes().WithResult(SysResultTypes.ApplicationError, "Error occurred during task [Do]!");

                LogUtil.Error(
                    $"sys_ret:{doRes.resp.sys_ret}, ret:{doRes.resp.ret},msg:{doRes.resp.msg}, Detail:{e}"
                    , TaskMeta.task_id, ModuleName);

            }

            return doRes;
        }

        #endregion
    }
}