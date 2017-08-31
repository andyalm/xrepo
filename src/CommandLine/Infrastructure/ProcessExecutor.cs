using System;
using System.Diagnostics;

namespace XRepo.CommandLine.Infrastructure
{
    public class ProcessExecutor
    {
        private readonly string _workingDirectory;

        public ProcessExecutor(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public void Exec(string processName, params string[] args)
        {
            var processStartInfo = new ProcessStartInfo(processName, string.Join(" ", args))
            {
                WorkingDirectory = _workingDirectory
            };
            var process = Process.Start(processStartInfo);
            process.WaitForExit();
            if(process.ExitCode != 0)
                throw new ProcessErrorException(process);
        }
    }

    public class ProcessErrorException : Exception
    {
        public Process Process { get; }

        public ProcessErrorException(Process process)
        {
            Process = process;
        }

        public override string Message => $"Command {Process.ProcessName} {Process.StartInfo.Arguments} exited with code {Process.ExitCode}";
    }
}