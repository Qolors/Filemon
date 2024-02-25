using System;

namespace Filemon
{
    public static class CommandHandler
    {
        public static void HandleCommand(string[] args)
        {
            if (args.Length >= 1)
            {
                switch (args[0])
                {
                    case "monitor":
                        if (args.Length == 2)
                        {
                            FileOperations.Monitor(args[1]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: Filemon monitor <path>");
                        }
                        break;
                    case "status":
                        ProcessManager.Status();
                        break;
                    case "start":
                        ProcessManager.Start();
                        break;
                    case "return":
                        ProcessManager.Kill();
                        break;
                    default:
                        Console.WriteLine("Invalid command. Available commands: monitor, status, start, return");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Usage: Filemon <command> [options]");
            }
        }
    }
}

