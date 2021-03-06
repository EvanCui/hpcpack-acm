﻿namespace Microsoft.HpcAcm.Common.Utilities
{
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncProcess
    {
        public static async Task<ProcessResult> RunAsync(string command, string arguments, object stdin, int timeoutSeconds, CancellationToken token)
        {
            var result = new ProcessResult();

            using (var process = new Process())
            {
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.EnableRaisingEvents = true;

                var outputBuilder = new StringBuilder();
                var outputCloseEvent = new TaskCompletionSource<bool>();

                process.OutputDataReceived += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                    {
                        outputCloseEvent.TrySetResult(true);
                    }
                    else
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                var errorBuilder = new StringBuilder();
                var errorCloseEvent = new TaskCompletionSource<bool>();

                process.ErrorDataReceived += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                    {
                        errorCloseEvent.TrySetResult(true);
                    }
                    else
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                var exitEvent = new TaskCompletionSource<bool>();
                process.Exited += (s, e) => { exitEvent.TrySetResult(true); };

                bool isStarted;

                try
                {
                    isStarted = process.Start();
                    if (stdin != null)
                    {
                        var jsonIn = JsonConvert.SerializeObject(stdin, Formatting.Indented);
                        await process.StandardInput.WriteAsync(jsonIn);
                        await process.StandardInput.FlushAsync();
                        process.StandardInput.Close();
                    }
                }
                catch (Exception error)
                {
                    result.Completed = true;
                    result.ExitCode = -1;
                    result.Output = error.ToString();

                    isStarted = false;
                }

                if (isStarted)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Create task to wait for process exit and closing all output streams
                    var processTask = Task.WhenAll(exitEvent.Task, outputCloseEvent.Task, errorCloseEvent.Task);

                    // Waits process completion and then checks it was not completed by timeout
                    if (await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), token), processTask) == processTask)
                    {
                        result.Completed = true;
                        result.ExitCode = process.ExitCode;
                        result.Output = outputBuilder.ToString();
                        result.Error = errorBuilder.ToString();
                    }
                    else
                    {
                        result.Completed = false;
                        result.ExitCode = -1;
                        result.Error = $"Timeouted after {timeoutSeconds} seconds";
                        outputCloseEvent.TrySetCanceled();
                        errorCloseEvent.TrySetCanceled();
                        exitEvent.TrySetCanceled();
                    }

                    try
                    {
                        // Kill hung process
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Cannot Kill process {0}", ex);
                    }
                }
                else
                {
                    result.Completed = true;
                    result.ExitCode = -1;
                    result.Output = "Process was not started.";
                }
            }

            return result;
        }
    }
}
