using System;
using System.IO;
using static DoCMovieTool.SupportClasses.ToolEnums;

namespace DoCMovieTool.SupportClasses
{
    internal static class ToolHelpers
    {
        public static void ExitProgram(this ExitType exitType, string exitMsg)
        {
            var exitMsgType = "";

            switch (exitType)
            {
                case ExitType.Success:
                    exitMsgType = "Success: ";
                    break;

                case ExitType.Error:
                    exitMsgType = "Error: ";
                    break;
            }

            Console.WriteLine("");
            Console.WriteLine($"{exitMsgType}{exitMsg}");
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static void ExCopyTo(this Stream inStream, Stream outStream, long size)
        {
            int bufferSize = 81920;
            long amountRemaining = size;

            while (amountRemaining > 0)
            {
                long arraySize = Math.Min(bufferSize, amountRemaining);
                byte[] copyArray = new byte[arraySize];

                _ = inStream.Read(copyArray, 0, (int)arraySize);
                outStream.Write(copyArray, 0, (int)arraySize);

                amountRemaining -= arraySize;
            }
        }

        public static void IfFileExistsDel(this string fileToDelete)
        {
            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
            }
        }
    }
}