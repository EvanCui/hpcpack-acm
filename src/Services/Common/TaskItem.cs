﻿namespace Microsoft.HpcAcm.Services.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class TaskItem : IDisposable
    {
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }

        public virtual T GetMessage<T>() => default(T);
        public virtual Task FinishAsync(CancellationToken token) => Task.CompletedTask;
        public virtual Task ReturnAsync(CancellationToken token) => Task.CompletedTask;
    }
}
