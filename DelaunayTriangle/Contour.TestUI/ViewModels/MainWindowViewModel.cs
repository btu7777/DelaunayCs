using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Utils;
using System.Threading.Tasks;

namespace Contour.TestUI.ViewModels
{
    internal class MainWindowViewModel : BindableBase
    {
        static string tempFile = Path.Combine(Path.GetTempPath(), "triData.csv");
        public string Info { get; set; }
        public string FilePath { get; set; }
        public string PythonExePath { get; set; }
        public string PythonFilePath { get; set; }
        public double Heigtht { get; set; } = 50;
        public DelegateCommand GenTris2Cmd { get; set; }
        public DelegateCommand GenTrisCmd { get; set; }
        public DelegateCommand GenContourCmd { get; set; }
        public DelegateCommand InitCmd { get; set; }
        public DelegateCommand ClearCmd { get; set; }
        public DelegateCommand LoadDataCmd { get; set; }
        public DelegateCommand SetPythonExePathCmd { get; set; }
        public DelegateCommand SetPythonFilePathCmd { get; set; }
        public MainWindowViewModel()
        {
            GenContourCmd = new DelegateCommand(GenContour);
            GenTrisCmd = new DelegateCommand(GenTris);
            GenTris2Cmd = new DelegateCommand(GenTris2);
            InitCmd = new DelegateCommand(Init);
            ClearCmd = new DelegateCommand(Clear);
            LoadDataCmd = new DelegateCommand(LoadData);
            SetPythonExePathCmd = new DelegateCommand(() => new Action<string>((m) => PythonExePath = m).MyOpenFileDialog("选择python.exe路径", "exe"));
            SetPythonFilePathCmd = new DelegateCommand(() => new Action<string>((m) => PythonFilePath = m).MyOpenFileDialog("选择py文件", "py"));

        }

        private void GenTris2()
        {

        }

        private void LoadData()
        {
            new Action<string>((m) => FilePath = m).MyOpenFileDialog("选择py文件", "csv", "txt");
        }

        private void GenContour()
        {
        }

        private void Clear()
        {
            PythonFilePath = string.Empty;
            Info = string.Empty;
            FilePath = string.Empty;
            PythonExePath = string.Empty;
        }

        private void Init()
        {
            PythonFilePath = "F:\\Document\\GitHub\\1-Python\\practice-2\\setup.py";
            //PythonFilePath = @"D:\document\GitHub\1-python\1-practice\setup.py";
            FilePath = @".\点数据2000.txt";
            AutoSetPythonExe();
        }
        bool AutoSetPythonExe()
        {
            var paths = Environment.GetEnvironmentVariable("Path").Split(';').Where(x => x.Contains("Python"));
            foreach (var path in paths)
            {
                string exeFile = Path.Combine(path, "python.exe");
                if (File.Exists(exeFile))
                {
                    PythonExePath = exeFile;
                    return true;
                }
            }
            return false;
        }
        private void GenTris()
        {
            // Check PythonExePath
            if (string.IsNullOrEmpty(PythonExePath))
            {
                if (!AutoSetPythonExe()) return;
            }
            // Check FilePath
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
                return;
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = PythonExePath,
                Arguments = $"{PythonFilePath} --param1=={FilePath}",
            };
            startInfo.FileName = PythonExePath;
            //startInfo.Arguments = pythonFile;
            // Start the Python process
            using (Process process = Process.Start(startInfo))
            {
                // Wait for the process to exit
                process.WaitForExit();

                // Check the exit code of the process
                if (process.ExitCode != 0)
                {
                    // The Python process exited with an error
                    Info = "生成失败!";
                }
                else
                {
                    // The Python process exited normally
                    Info = "三角形生成成功!";
                    File.ReadAllText(tempFile);
                }
            }
        }
        void ReadCsvData(string content)
        {

        }
    }
}
