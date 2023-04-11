
using System.Collections.Generic;
using System.Diagnostics;

namespace Ex
{
    public static class ExCmd
    {
        public static void Cmd(List<string> inputCommand = null, DataReceivedEventHandler outputDataReceived = null, DataReceivedEventHandler errorDataReceived = null)
        {
            Cmd("cmd.exe", ".", false, true, true, true, true, inputCommand, outputDataReceived, errorDataReceived);
        }

        public static void Cmd(string fileName = "cmd.exe", List<string> inputCommand = null, DataReceivedEventHandler outputDataReceived = null,
            DataReceivedEventHandler errorDataReceived = null)
        {
            Cmd(fileName, ".", false, true, true, true, true, inputCommand, outputDataReceived, errorDataReceived);
        }
        public static void Cmd(string fileName = "cmd.exe", string workingDirectory = ".", bool useShellExecute = false, bool redirectStandardInput = true,
            bool redirectStandardOutput = true, bool createNoWindow = true, bool redirectStandardError = true, List<string> inputCommand = null, 
            DataReceivedEventHandler outputDataReceived = null, 
            DataReceivedEventHandler errorDataReceived = null)
        {
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.UseShellExecute = useShellExecute;
            process.StartInfo.RedirectStandardInput = redirectStandardInput;
            process.StartInfo.RedirectStandardOutput = redirectStandardOutput;
            process.StartInfo.CreateNoWindow = createNoWindow;
            process.StartInfo.RedirectStandardError = redirectStandardError;
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += ErrorDataHandler;
            if (outputDataReceived != null)
                process.OutputDataReceived += outputDataReceived;
            if (errorDataReceived != null)
                process.ErrorDataReceived += errorDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (inputCommand != null)
                for (int i = 0; i < inputCommand.Count; i++)
                {
                    process.StandardInput.WriteLine(inputCommand[i]);
                }

            process.WaitForExit();
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                UnityEngine.Debug.Log(outLine.Data);
            }
        }

        private static void ErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                UnityEngine.Debug.LogError(outLine.Data);
            }
        }
    }
}