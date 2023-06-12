using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class FileDialogOperation
    {
        public static void MyOpenFileDialog(this Action<string> func, string title, params string[] filters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = title,
                //Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv",
                Filter = string.Join("|", filters.Select(x => $"{x} File (*.{x})|*.{x}")),
                Multiselect = false,

            };
            if (dialog.ShowDialog() == true)
            {
                //action(dialog.FileName);
                func.Invoke(dialog.FileName);

            }
        }
    }
}