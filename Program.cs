using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace MstscLauncher
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                var mstsc = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\mstsc.exe");

                if (File.Exists(mstsc) == false)
                {
                    MessageBox.Show($"{mstsc} was not found", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

	            if (args.Length == 0)
	            {
		            SetupProtocolHandelingInRegistry();
		            return;
	            }

                Uri uri = new Uri(args[0]);

                if (uri.Scheme.Equals("mstsc", StringComparison.CurrentCultureIgnoreCase))
                {
                    var host = $"/v:{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}";

                    if (String.IsNullOrEmpty(uri.Query) == false)
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

                        Execute(mstsc, arguments);
                    }
                    else
                    {
                        Execute(mstsc, host);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mstsc Launcher - Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Execute(string exe, params string[] args)
        {
            Execute(exe, args.AsEnumerable());
        }

        static void Execute(string exe, IEnumerable<string> args)
        {
            var path = new FileInfo(exe);

	        Process p = new Process
	        {
		        StartInfo =
		        {
			        UseShellExecute = false,
			        FileName = path.Name,
			        WorkingDirectory = path.Directory.FullName,
			        Arguments = String.Join(" ", args.Where(s => !String.IsNullOrEmpty(s)))
		        }
	        };
	        p.Start();
        }

        static void SetupProtocolHandelingInRegistry()
        {
				var mstscLauncherPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

			var hkcr = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);

            try
            {
	            RegisterProtocol(hkcr, mstscLauncherPath);

	            MessageBox.Show("URL handler registered", "Mstsc Launcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Could not register as URL handler, please run again as administrator", "Mstsc Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

	    private static void RegisterProtocol(RegistryKey hkcr, string mstscLauncherPath)
	    {
		    var mstsc = hkcr.CreateSubKey("mstsc");

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
