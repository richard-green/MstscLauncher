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
                    MessageBox.Show(String.Format("{0} was not found", mstsc), "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SetupRegistry(mstsc);

                if (args.Length == 0)
                {
                    return;
                }

                Uri uri = new Uri(args[0]);

                if (uri.Scheme.Equals("mstsc", StringComparison.CurrentCultureIgnoreCase))
                {
                    var host = String.Format("/v:{0}{1}", uri.Host, uri.IsDefaultPort ? "" : String.Format(":{0}", uri.Port));

                    if (String.IsNullOrEmpty(uri.Query) == false)
                    {
                        var arguments = new List<string>() { host };
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
                                    arguments.Add(String.Format("/{0}", key));
                                    break;

                                case "w":
                                case "h":
                                case "shadow":
                                    arguments.Add(String.Format("/{0}:{1}", key, query[key]));
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

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = path.Name;
            p.StartInfo.WorkingDirectory = path.Directory.FullName;
            p.StartInfo.Arguments = String.Join(" ", args.Where(s => String.IsNullOrEmpty(s) == false));
            p.Start();
        }

        static void SetupRegistry(string exe)
        {
            var launcher = Path.Combine(Environment.CurrentDirectory, "MstscLauncher.exe");

            var hkcr = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);

            var mstsc = hkcr.OpenSubKey("mstsc");

            if (mstsc == null)
            {
                try
                {
                    mstsc = hkcr.CreateSubKey("mstsc");

                    var DefaultIcon = mstsc.CreateSubKey("DefaultIcon");
                    var Shell = mstsc.CreateSubKey("Shell");
                    var Open = Shell.CreateSubKey("Open");
                    var Command = Open.CreateSubKey("Command");

                    mstsc.SetValue("", "URL:Remote Desktop Client Launcher");
                    mstsc.SetValue("URL Protocol", "");
                    DefaultIcon.SetValue("", String.Format("\"{0}\",1", launcher));
                    Command.SetValue("", String.Format("\"{0}\" \"%1\"", launcher));

                    MessageBox.Show("URL handler registered", "Mstsc Launcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Could not register as URL handler, please run again as administrator", "Mstsc Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
