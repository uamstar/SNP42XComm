using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Drawing;

namespace SanicaSNP42XDemo
{
    public class ViewModel: INotifyPropertyChanged
    {
        private ImageSource? _loopSensorImg;
        public ImageSource? LoopSensorImgSrc
        {
            get => _loopSensorImg;
            set
            {
                _loopSensorImg = value;
                NotifyPropertyChanged(nameof(LoopSensorImgSrc));
            }
        }
        private ImageSource? _plateImg;
        public ImageSource? PlateImgSrc
        {
            get => _plateImg;
            set
            {
                _plateImg = value;
                NotifyPropertyChanged(nameof(PlateImgSrc));
            }
        }
        private ImageSource? _arrowImg;
        public ImageSource? ArrowImgSrc
        {
            get => _arrowImg;
            set
            {
                _arrowImg = value;
                NotifyPropertyChanged(nameof(ArrowImgSrc));
            }
        }                
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler? CanExecuteChanged;
        public void NotifyPropertyChanged([CallerMemberName] string info = "") =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        public void SetLoopSensorImg(BitmapImage img)
        {
            LoopSensorImgSrc = img;
        }
        public void SetPlateImg(BitmapImage img)
        {
            PlateImgSrc = img;
        }
        public void SetArrowImg(BitmapImage img)
        {
            ArrowImgSrc = img;
        }
    }
}
