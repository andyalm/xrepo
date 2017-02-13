using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;

namespace XRepo.CommandLine.Infrastructure
{
    static class CommandLineApplicationExtensions
    {
        public static void OnExecuteWithHelp(this CommandLineApplication app, Func<int> action)
        {
            var help = app.HelpOption("-h|--help");
            app.OnExecute(() =>
            {
                if (help.HasValue())
                {
                    app.Out.WriteLine(app.GetHelpText());
                }

                return action();
            });
        }

        public static void ShowHelpOnEmptyExecute(this CommandLineApplication app)
        {
            app.OnExecute(() =>
            {
                app.Out.WriteLine(app.GetHelpText());

                return 0;
            });
            
        }
    }
}
