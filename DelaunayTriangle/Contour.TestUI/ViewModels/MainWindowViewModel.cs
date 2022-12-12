using Prism.Commands;
using Prism.Mvvm;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contour.TestUI.ViewModels
{
    internal class MainWindowViewModel : BindableBase
    {

        public string Info { get; set; }
        public string FilePath { get; set; }
        public string PythonExePath { get; set; }
        public DelegateCommand GenTrisCmd { get; set; }
        public DelegateCommand GenContourCmd { get; set; }
        public DelegateCommand InitCmd { get; set; }
        public MainWindowViewModel()
        {
            GenTrisCmd = new DelegateCommand(GenTris);
            InitCmd = new DelegateCommand(Init);

        }

        private void Init()
        {

        }

        string pythonDir = Environment.GetEnvironmentVariable("PYTHONPATH");
        private void GenTris()
        {
            if (PythonExePath != null)
            {
                if (!string.IsNullOrEmpty(pythonDir))
                {
                    PythonExePath = pythonDir;
                }
                else return;
            }
            if (string.IsNullOrEmpty(FilePath) || File.Exists(FilePath))
                return;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = pythonExe;
            startInfo.Arguments = pythonFile;
            // Start the Python process
            using (Process process = Process.Start(startInfo))
            {
                // Wait for the process to exit
                process.WaitForExit();

                // Check the exit code of the process
                if (process.ExitCode != 0)
                {
                    // The Python process exited with an error
                    Console.WriteLine("An error occurred while running the Python file.");
                }
            }
        }
    }
}
