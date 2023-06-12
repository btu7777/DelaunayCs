using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utils.Models;

namespace DelaunayTriangle.TestUI.Views
{
    /// <summary>
    /// DataView.xaml 的交互逻辑
    /// </summary>
    public partial class DataView : Window
    {
        public DataView(List<TIN_Point> points)
        {
            this.Closing += DataView_Closing;
            InitializeComponent();
            Points = points;
        }

        private void DataView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
            this.Closing -= DataView_Closing;
        }

        public List<TIN_Point> Points { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TIN_Point point = (((Button)sender).Tag) as TIN_Point;
            if (point != null && Points.Contains(point))
            {
                Points.Remove(point);
            }
        }
    }
}
