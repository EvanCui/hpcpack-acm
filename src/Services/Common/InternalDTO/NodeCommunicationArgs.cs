﻿namespace Microsoft.HpcAcm.Services.Common
{
    using Microsoft.HpcAcm.Common.Dto;
    using System;
    using System.Collections.Generic;
    using System.Text;

    #region CallBack Method Arguments

    /// <summary>
    /// Base class for arguments to node communicator methods.
    /// These methods are used by the scheduler
    /// to perform operations on the compute nodes.
    /// These arguments are also used in the callback at the end of the the operation.
    /// The callback at the end of the operation informs the scheduler that the
    /// operation on the compute node has completed.
    /// </summary>
    public class NodeCommunicatorCallBackArg
    {
        private readonly IEnumerable<int> resIds;

        public NodeCommunicatorCallBackArg(IEnumerable<int> resIds)
        {
            this.resIds = resIds;
        }

        public IEnumerable<int> ResIds
        {
            get { return resIds; }
        }


    }

    //delegate for callbacks methods used by the node communicator
    // to report completion of operations initiated by the scheduler
    public delegate void NodeCommunicatorCallBack<in ArgT>(string nodeName, ArgT arg, Exception exception) where ArgT : NodeCommunicatorCallBackArg;

    /// <summary>
    /// Argument for the StartJob operation issued by the scheduler
    /// </summary>
    public class StartJobArg : NodeCommunicatorCallBackArg
    {
        private readonly int jobId;

        public StartJobArg(IEnumerable<int> resIds, int jobId)
            : base(resIds)
        {
            this.jobId = jobId;
        }

        public int JobId
        {
            get { return jobId; }
        }

        public override string ToString()
        {
            return "Start job " + jobId;
        }
    }

    /// <summary>
    /// Argument for the EndJob operation issued by the scheduler
    /// </summary>
    public class EndJobArg : NodeCommunicatorCallBackArg
    {
        public EndJobArg(IEnumerable<int> resIds, int jobId)
            : base(resIds)
        {
            this.JobId = jobId;
        }

        public int JobId { get; }

        public ComputeClusterJobInformation JobInfo { get; set; }

        public override string ToString() => $"End job {this.JobId}";
    }

    /// <summary>
    /// Argument for the StartTask operation issued by the scheduler
    /// </summary>
    public class StartTaskArg : NodeCommunicatorCallBackArg
    {
        public StartTaskArg(IEnumerable<int> resIds, int jobId, int taskId)
            : base(resIds)
        {
            this.JobId = jobId;
            this.TaskId = taskId;
        }

        public int JobId { get; private set; }

        public int TaskId { get; private set; }

        public override string ToString()
        {
            return "Start task " + this.JobId + "." + this.TaskId;
        }
    }

    /// <summary>
    /// Argument for the EndTask operation issued by the scheduler
    /// It also includes the taskinfo object that is used by the callback
    /// to report the task's state on the compute node at the completion of the
    /// EndTask operation
    /// </summary>
    public class EndTaskArg : NodeCommunicatorCallBackArg
    {
        public EndTaskArg(IEnumerable<int> resIds, int jobId, int taskId)
            : base(resIds)
        {
            this.JobId = jobId;
            this.TaskId = taskId;
            this.TaskCancelGracePeriod = 0;
        }

        public EndTaskArg(IEnumerable<int> resIds, int jobId, int taskId, int gracePeriod)
            : base(resIds)
        {
            this.JobId = jobId;
            this.TaskId = taskId;
            this.TaskCancelGracePeriod = gracePeriod;
        }

        public int JobId { get; }

        public int TaskId { get; }

        public int TaskCancelGracePeriod { get; }

        public ComputeClusterTaskInformation TaskInfo { get; set; }

        public override string ToString()
        {
            return "End task " + this.JobId + "." + this.TaskId;
        }
    }

    /// <summary>
    /// /// <summary>
    /// Argument for the StartJobAndTask operation issued by the scheduler
    /// </summary>
    /// </summary>
    public class StartJobAndTaskArg : NodeCommunicatorCallBackArg
    {
        public StartJobAndTaskArg(IEnumerable<int> resIds, int jobId, int taskId)
            : base(resIds)
        {
            this.JobId = jobId;
            this.TaskId = taskId;
        }

        public int JobId { get; set; }
        public int TaskId { get; set; }
    }

    public class PeekTaskOutputArg : NodeCommunicatorCallBackArg
    {
        public PeekTaskOutputArg(int jobId, int taskId) : base(null)
        {
            this.JobId = jobId;
            this.TaskId = taskId;
        }

        public int JobId { get; }

        public int TaskId { get; }

        public string Output
        {
            get;
            set;
        }

        public override string ToString()
        {
            return "Peek output of task " + JobId + "." + TaskId;
        }
    }
    #endregion
}
