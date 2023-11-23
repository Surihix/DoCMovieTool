using System;
using static DoCMovieTool.SupportClasses.ToolHelpers;

namespace DoCMovieTool
{
    internal class Core
    {
        static void Main(string[] args)
        {
            if (args.Length > 2)
            {
                var exampleMsg = "Examples:\nDoCMovieTool.exe -u \"7570F45E.F7\"\nDoCMovieTool.exe -r \"_7570F45E.F7\"";
                ExitType.Error.ExitProgram($"Enough arguments not specified\n{exampleMsg}");
            }

            var toolActionSwitch = new ActionSwitches();
            if (Enum.TryParse(args[0].Replace("-", ""), false, out ActionSwitches convertedActionSwitch))
            {
                toolActionSwitch = convertedActionSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram("Invalid action switch specified. Must be \"-u\" or \"-r\"");
            }

            var inFileOrDir = args[1];

            switch (toolActionSwitch)
            {
                case ActionSwitches.u:
                    MovieUnpack.UnpackProcess(inFileOrDir);
                    break;

                case ActionSwitches.r:
                    ExitType.Error.ExitProgram("Unimplemented");
                    break;
            }
        }

        enum ActionSwitches
        {
            u,
            r
        }
    }
}