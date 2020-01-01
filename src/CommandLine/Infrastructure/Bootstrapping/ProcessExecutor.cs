using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace XRepo.CommandLine.Infrastructure.Bootstrapping
{
    public class ProcessExecutor
    {
        private readonly string _workingDirectory;

        public ProcessExecutor() : this(Directory.GetCurrentDirectory())
        {
            
        }
        
        public ProcessExecutor(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public string ExecuteAsString(string processName, params string[] args)
        {
            return ExecuteAs(processName, args, p => p.StandardOutput.ReadToEnd().Trim());
        }

        public IEnumerable<string> ExecuteAsStrings(string processName, params string[] args)
        {
            return ExecuteAs(processName, args, p => p.StandardOutput.ReadLines());
        }

        private T ExecuteAs<T>(string processName, string[] args, Func<Process,T> mapResponse)
        {
            var startInfo = new ProcessStartInfo(processName)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WorkingDirectory = _workingDirectory
            };
            foreach (var arg in args)
            {
                startInfo.ArgumentList.Add(arg);
            }

            using (var process = new Process
            {
                StartInfo = startInfo
            })
            {
                process.Start();

                return mapResponse(process);
            }
        }
    }

    internal static class StreamExtensions
    {
        public static IEnumerable<string> ReadLines(this StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }

        public static void CopyAllLines(this StreamReader reader, TextWriter writer)
        {
            foreach (var line in reader.ReadLines())
            {
                writer.WriteLine(line);
            }
        }
    }

}