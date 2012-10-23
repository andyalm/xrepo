using System;
using System.Collections;
using System.Collections.Generic;

using FubuCore.CommandLine;

using XRepo.Core;

using System.Linq;

namespace CommandLine.Commands
{
    [CommandDescription("Lists xrepo things", Name="list")]
    public class ListCommand : FubuCommand<ListInputArgs>
    {
        public override bool Execute(ListInputArgs input)
        {
            var environment = XRepoEnvironment.ForCurrentUser();

            switch (input.Subject)
            {
                case ListSubject.pins:
                    WriteList("pinned repos", environment.PinRegistry.GetPinnedRepos().Select(r => r.RepoName));
                    Console.WriteLine();
                    Console.WriteLine();
                    WriteList("pinned assemblies", environment.PinRegistry.GetPinnedAssemblies().Select(a => a.AssemblyName));
                    break;
                case ListSubject.repos:
                    WriteRepos("repos", environment.RepoRegistry.GetRepos());
                    break;
                case ListSubject.assemblies:
                    WriteAssemblies("assemblies", environment.AssemblyRegistry.GetAssemblies());
                    break;
            }

            return true;
        }

        private void WriteRepos(string title, IEnumerable<RegisteredRepo> repos)
        {
            Write(title, repos, r => Console.WriteLine("{0} - {1}", r.Name, r.Path));
        }

        private void WriteAssemblies(string title, IEnumerable<RegisteredAssembly> assemblies)
        {
            Write(title, assemblies, a =>
            {
                Console.WriteLine(a.Name);
                foreach (var project in a.Projects)
                {
                    Console.WriteLine("\t{0}", project.AssemblyPath);
                }
                Console.WriteLine("----------------------");
            });
        }

        private void Write<T>(string title, IEnumerable<T> items, Action<T> write)
        {
            Console.WriteLine(title);
            Console.WriteLine("-------------------");
            foreach (var item in items)
            {
                write(item);
            }
        }

        private void WriteList(string title, IEnumerable<string> items)
        {
            Write(title, items, Console.WriteLine);
        }
    }

    public class ListInputArgs
    {
        [RequiredUsage("default")]
        public ListSubject Subject { get; set; }
    }

    public enum ListSubject
    {
        pins,
        repos,
        assemblies
    }
}