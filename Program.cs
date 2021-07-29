using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace MstscLauncher
{
    static class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    SetupProtocolHandlingInRegistry();
                    return;
                }

                Uri uri = new Uri(args[0]);

                if (uri.Scheme.Equals("mstsc", StringComparison.InvariantCultureIgnoreCase) || uri.Scheme.Equals("rdp", StringComparison.InvariantCultureIgnoreCase))
                {
                    await LaunchMstsc(uri);
                }
                else if (uri.Scheme.Equals("radmin", StringComparison.InvariantCultureIgnoreCase))
                {
                    await LaunchRAdmin(uri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mstsc Launcher - Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        static async Task LaunchMstsc(Uri uri)
        {
            var mstsc = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\mstsc.exe");

            if (!File.Exists(mstsc))
            {
                MessageBox.Show($"{mstsc} was not found", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var host = $"/v:{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}";

            if (string.IsNullOrEmpty(uri.Query) == false)
            {
                var arguments = new List<string> { host };
                var query = HttpUtility.ParseQueryString(uri.Query);

                foreach (var key in query.AllKeys)
                {
                    switch (key)
                    {
                        case "admin":
                        case "f":
                        case "public":
                        case "span":
                        case "multimon":
                        case "restrictedAdmin":
                        case "remoteGuard":
                        case "prompt":
                        case "control":
                        case "noConsentPrompt":
                            arguments.Add($"/{key}");
                            break;

                        case "w":
                        case "h":
                        case "shadow":
                            arguments.Add($"/{key}:{query[key]}");
                            break;
                    }
                }

                await ProcessHelper.ProcessCommandAsync(mstsc, string.Join(" ", arguments), Path.GetDirectoryName(mstsc));
            }
            else
            {
                await ProcessHelper.ProcessCommandAsync(mstsc, host, Path.GetDirectoryName(mstsc));
            }
        }

        static async Task LaunchRAdmin(Uri uri)
        {
            var radmin = @"C:\Program Files (x86)\Radmin Viewer 3\Radmin.exe";

            if (!File.Exists(radmin))
            {
                MessageBox.Show($"{radmin} was not found", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var host = $"/connect:{uri.Host}:{(uri.IsDefaultPort ? 4899 : uri.Port)}";

            await ProcessHelper.ProcessCommandAsync(radmin, host, Path.GetDirectoryName(radmin));
        }

        static void SetupProtocolHandlingInRegistry()
        {
            var mstscLauncherPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var hkcr = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);

            try
            {
                RegisterProtocol(hkcr, mstscLauncherPath, "mstsc");
                RegisterProtocol(hkcr, mstscLauncherPath, "rdp");
                RegisterProtocol(hkcr, mstscLauncherPath, "radmin");

                MessageBox.Show("URL handler registered", "Mstsc Launcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Could not register as URL handler, please run again as administrator", "Mstsc Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void RegisterProtocol(RegistryKey hkcr, string mstscLauncherPath, string protocolName)
        {
            var mstsc = hkcr.CreateSubKey(protocolName);

            var DefaultIcon = mstsc.CreateSubKey("DefaultIcon");
            var Shell = mstsc.CreateSubKey("Shell");
            var Open = Shell.CreateSubKey("Open");
            var Command = Open.CreateSubKey("Command");

            mstsc.SetValue("", "URL:Remote Desktop Client Launcher");
            mstsc.SetValue("URL Protocol", "");
            DefaultIcon.SetValue("", $"\"{mstscLauncherPath}\",1");
            Command.SetValue("", $"\"{mstscLauncherPath}\" \"%1\"");
        }
    }
}
