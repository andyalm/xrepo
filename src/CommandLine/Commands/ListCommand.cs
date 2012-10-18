using System;
using System.Collections.Generic;

using FubuCore.CommandLine;

using XPack.Core;

using System.Linq;

namespace CommandLine.Commands
{
    [CommandDescription("Lists xpack things", Name="list")]
    public class ListCommand : FubuCommand<ListInputArgs>
    {
        private string _thing;
        private bool _match;
        private List<string> _validThings = new List<string>(); 
        
        public override bool Execute(ListInputArgs input)
        {
            _thing = input.Thing;
            var environment = XPackEnvironment.ForCurrentUser();
            
            ListThingsIfMatch("pins", () => environment.PinRegistry.GetPinnedAssemblies().Select(a => a.AssemblyName));
            
            if(!_match)
            {
                var validThingList = String.Join(", ", _validThings.ToArray());
                throw new CommandFailureException("I don't know how to list '" + input.Thing + "'. Please try listing one of these: " + validThingList);
            }

            return true;
        }

        private void ListThingsIfMatch(string thingName, Func<IEnumerable<string>> retrieveThings)
        {
            _validThings.Add(thingName);
            if(thingName.Equals(_thing, StringComparison.OrdinalIgnoreCase))
            {
                _match = true;
                foreach (var thing in retrieveThings())
                {
                    Console.WriteLine(thing);
                }
            }
        }
    }

    public class ListInputArgs
    {
        [RequiredUsage("default")]
        public string Thing { get; set; }
    }
}