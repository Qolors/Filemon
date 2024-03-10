using System.Diagnostics;
using System.IO.Pipes;

namespace Filemon
{
    public static class ProcessManager
    {
        public static void Status()
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream("FilemonPipe"))
                {
                    pipeClient.Connect(1000);

                    if (pipeClient.IsConnected)
                    {
                        string message = "status";

                        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                        pipeClient.Write(messageBytes, 0, messageBytes.Length);

                        byte[] buffer = new byte[1024];
                        int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);

                        string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        Console.WriteLine("Filemon Status: " + response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Filemon Status: Not Running");
            }
        }

        public static void Kill()
        {
            Console.WriteLine("Attemping to shut down Filemon...");

            try
            {
               using (var pipeClient = new NamedPipeClientStream("FilemonPipe"))
                {
                    pipeClient.Connect(1000);

                    if (pipeClient.IsConnected)
                    {
                        string message = "kill";

                        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                        pipeClient.Write(messageBytes, 0, messageBytes.Length);

                        byte[] buffer = new byte[1024];
                        int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);

                        string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        Console.WriteLine(response);
                    }
                    else if (!pipeClient.IsConnected)
                    {
                        Console.WriteLine("Filemon is Not Running");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during kill operation: " + ex.Message);
            }
        }

        public static void Start()
        {
            try
            {
               using (var pipeClient = new NamedPipeClientStream("FilemonPipe"))
                {
                    pipeClient.Connect(1000);

                    if (pipeClient.IsConnected)
                    {
                        Console.WriteLine("Filemon is already running");
                    }

                }
            }
            catch (TimeoutException ex)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "Filemon.OverWatch.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        Console.WriteLine("Filemon Started Successfully");
                    }
                    else
                    {
                        Console.WriteLine("Filemon Failed to Start");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during start operation: " + ex.Message);
            }
        }

        public static void Log()
        {

        }
    }
}
