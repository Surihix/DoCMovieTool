﻿using System;
using System.IO;
using static DoCMovieTool.SupportClasses.ToolEnums;
using static DoCMovieTool.SupportClasses.ToolHelpers;

namespace DoCMovieTool
{
    internal class Core
    {
        static void Main(string[] args)
        {
            var exampleMsgArray = new string[]
            {
                "Examples:",
                "To unpack a movie archive: DoCMovieTool.exe -u \"23CFDD41.F7\"",
                "To repack a movie archive: DoCMovieTool.exe -r \"_23CFDD41.F7\""
            };

            if (args.Length < 2)
            {
                ExitType.Error.ExitProgram($"Enough arguments not specified\n\n{string.Join("\n", exampleMsgArray)}");
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

            try
            {
                switch (toolActionSwitch)
                {
                    case ActionSwitches.u:
                        if (!File.Exists(args[1]))
                        {
                            ExitType.Error.ExitProgram("Specified file does not exist");
                        }
                        MovieUnpack.UnpackProcess(args[1]);
                        break;

                    case ActionSwitches.r:
                        if (!Directory.Exists(args[1]))
                        {
                            ExitType.Error.ExitProgram("Specified folder does not exist");
                        }
                        MovieRepack.RepackProcess(args[1]);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExitType.Error.ExitProgram($"An Exception has occured\n{ex}");
            }
        }

        enum ActionSwitches
        {
            u,
            r
        }
    }
}