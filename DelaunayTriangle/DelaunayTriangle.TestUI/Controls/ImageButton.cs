using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DelaunayTriangle.TestUI.Controls
{
    public class ImageButton : Button
    {
        public static readonly DependencyProperty DefaultImgProperty = DependencyProperty.Register("DefaultImg", typeof(ImageSource), typeof(ImageButton));

        public ImageSource DefaultImg
        {
            get { return (ImageSource)GetValue(DefaultImgProperty); }
            set { SetValue(DefaultImgProperty, value); }
        }
    }
}
