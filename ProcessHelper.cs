using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MstscLauncher
{
    public static class ProcessHelper
    {
        public static async Task<(int rc, string result)> ProcessCommandAsync(string exe, string parameters, string workingDirectory, bool throwOnError = false, Action<ProcessStartInfo> cfg = null)
        {
            return await Task.Run(() =>
            {
                ProcessStartInfo info = new ProcessStartInfo(exe, parameters);
                info.UseShellExecute = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.CreateNoWindow = true;
                info.WorkingDirectory = workingDirectory;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;

                cfg?.Invoke(info);

                var process = Process.Start(info);
                process.WaitForExit();

                var rc = process.ExitCode;

                var result = process.StandardOutput.ReadToEnd();

                if (rc != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    if (throwOnError) throw new Exception(string.IsNullOrWhiteSpace(error) ? result : error);
                }

                return (rc, result);
            });
        }
    }
}
