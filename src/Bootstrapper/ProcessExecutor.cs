using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XRepo.Bootstrapper
{
    public class ProcessExecutor
    {
        private readonly string _workingDirectory;

        public ProcessExecutor(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public IEnumerable<string> Exec(string processName, params string[] args)
        {
            var processStartInfo = new ProcessStartInfo(processName, string.Join(" ", args))
            {
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true
            };
            var process = Process.Start(processStartInfo);
            process.WaitForExit();
            if(process.ExitCode != 0)
                throw new ProcessErrorException(process);

            var line = process.StandardOutput.ReadLine();
            while (line != null)
            {
                yield return line;
                line = process.StandardOutput.ReadLine();
            }
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
