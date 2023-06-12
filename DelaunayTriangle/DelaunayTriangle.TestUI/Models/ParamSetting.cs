//using MathNet.Spatial.Triangulation;
using Prism.Mvvm;
using System;
using System.Windows.Media;
namespace DelaunayTriangle.TestUI.Models
{
    public class ParamSetting : BindableBase
    {
        public string Name { get; set; }
        bool isChecked;
        public bool IsChecked
        {
            get => isChecked; set
            {
                if (value != isChecked)
                {
                    if (value)
                        Checked?.Invoke(this, EventArgs.Empty);
                    else
                        UnChecked?.Invoke(this, EventArgs.Empty);
                }
                SetProperty(ref isChecked, value);
            }
        }
        public Color Color { get; set; }

        public event EventHandler Checked;
        public event EventHandler UnChecked;
    }


}
