using System.Text;
using System.Windows;
using System.IO.Ports;
using SanicaSNP42X;
using Microsoft.Extensions.Configuration;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.ComponentModel;
using System.Resources;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;

namespace SanicaSNP42XDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private IProcLogger logger;
        private RS485Cytel cytel1;
        private const int POLLING_INTERVAL = 1000;   // 每 1 秒發送一次
        private const int BLINK_INTERVAL = 500;   // 每 0.7 秒閃爍一次

        private System.Timers.Timer _timer;
        private LoopSensorStatus _preLoopSensorStatus;
        private LockPlateStatus _preLockPlateStatus;
        private SensorStatus _preSensorStatus;

        private BitmapSource _loopSensorSource;                
        private static BitmapImage IMG_LS_ON;
        private static BitmapImage IMG_LS_OFF;
        private static BitmapImage IMG_LS_FORCE_ON;
        private static BitmapImage IMG_LS_FORCE_OFF;
        private static BitmapImage IMG_LS_NA;
        private static BitmapImage IMG_LS_ERROR;
        private static System.Windows.Media.Brush LIGHT_BLUE = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 51, 139, 228));
        private static System.Windows.Media.Brush DEEP_BLUE = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 8, 255));
        private static System.Windows.Media.Brush GRAY = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 100, 100));
        private static System.Windows.Media.Brush DEEP_GRAY = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 73, 73, 73));
        private static System.Windows.Media.Brush THIN_RED = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 228, 51, 84));
        private static BitmapImage IMG_ARROW_UP;
        private static BitmapImage IMG_ARROW_DOWN;
        private static BitmapImage IMG_PLATE_UP;
        private static BitmapImage IMG_PLATE_DOWN;

        private System.Timers.Timer _blinkTimer;

        private int _pollingCounter = 0;

        public MainWindow()
        {
            InitializeComponent();
            logger = new RunningLogger();

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            try
            {
                Settings settings = config.GetRequiredSection("Settings").Get<Settings>();

                StopBits stopBits;
                if (settings.Comport.StopBits == 1)
                    stopBits = StopBits.One;
                else if (settings.Comport.StopBits == 2)
                    stopBits = StopBits.Two;
                else stopBits = StopBits.None;

                Manager manager = Manager.getInstance(
                    settings.Comport.PortName, 
                    settings.Comport.BaudRate,
                    (Parity)Enum.Parse(typeof(Parity), settings.Comport.Parity),
                    settings.Comport.DataBits,
                    stopBits);

                manager.SetLogger(logger);

                cytel1 = manager.CreateRS485Cytel(0x01);
                cytel1.ParkingPlateStatusResponse += PSRespReceivied;
                cytel1.ParkingControlResponse += PCRespReceivied;
                cytel1.NoResponse += NoRespReceivied;

                _timer = new System.Timers.Timer(POLLING_INTERVAL);
                _timer.Elapsed += OnTimedEvent;
                _timer.AutoReset = true;
                _timer.Enabled = false;

                _blinkTimer = new System.Timers.Timer(BLINK_INTERVAL);
                _blinkTimer.Elapsed += OnBlinkTimedEvent;
                _blinkTimer.AutoReset = true;
                _blinkTimer.Enabled = false;
            }
            catch (Exception ex)
            {
                logger.Error("Fail to get settings from appsettings.json.", ex);
            }

            Bitmap bitmap = Properties.Resources.LS_on;
            IMG_LS_ON = ConvertBitmapToBitmapImage(bitmap);
            bitmap = Properties.Resources.LS_off;
            IMG_LS_OFF = ConvertBitmapToBitmapImage(bitmap);
            bitmap = Properties.Resources.LS_force_on;
            IMG_LS_FORCE_ON = ConvertBitmapToBitmapImage(bitmap);
            bitmap = Properties.Resources.LS_force_off;
            IMG_LS_FORCE_OFF = ConvertBitmapToBitmapImage(bitmap);
            bitmap = Properties.Resources.LS_NA;
            IMG_LS_NA = ConvertBitmapToBitmapImage(bitmap);
            bitmap = Properties.Resources.LS_error;
            IMG_LS_ERROR = ConvertBitmapToBitmapImage(bitmap);

            bitmap = Properties.Resources.arrow_up;
            IMG_ARROW_UP = ConvertBitmapToBitmapImage(bitmap);
            bitmap = Properties.Resources.arrow_down;
            IMG_ARROW_DOWN = ConvertBitmapToBitmapImage(bitmap);

            bitmap = Properties.Resources.plate_up;
            IMG_PLATE_UP = ConvertBitmapToBitmapImage(bitmap);
            bitmap = Properties.Resources.plate_down;
            IMG_PLATE_DOWN = ConvertBitmapToBitmapImage(bitmap);

        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        public BitmapSource LoopSensorSource
        {
            get { return _loopSensorSource; }
            set
            {
                _loopSensorSource = value;
                NotifyPropertyChanged("LoopSensorSource");
            }
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            cytel1.Polling();
            _pollingCounter++;

            Dispatcher.Invoke(() =>
            {
                PollingCntLB.Content = _pollingCounter;
            });
        }
        private void OnBlinkTimedEvent(Object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ArrowIMG.Visibility = (ArrowIMG.Visibility == Visibility.Collapsed)? Visibility.Visible : Visibility.Collapsed;
            });
        }
        private void PSRespReceivied(ParkingStatus ps)
        {
            logger.Info($"Receivied: Addr {ps.Address}");
            logger.Info($"\tLoop Sensor Status: {ps.LoopSensorStatus}");
            logger.Info($"\tMat Switch Status: {ps.MatSwitchStatus}");
            logger.Info($"\tLock Plate Status: {ps.LockPlateStatus}");
            logger.Info($"\tSensor Status: {ps.SensorStatus}");
            logger.Info($"\tLoop count: {ps.LoopCount}");
            logger.Info($"\tBase count: ON: {ps.OnBaseCount}, OFF: {ps.OffBaseCount}");
            logger.Info($"\tLevel count: ON: {ps.OnLevelCount}, OFF: {ps.OffLevelCount}");

            Dispatcher.Invoke(() =>
            {
                LoopSensorLB.Content = ps.LoopSensorStatus.ToString();
                MatSwitchLB.Content = ps.MatSwitchStatus.ToString();
                LockPlateLB.Content = ps.LockPlateStatus.ToString();
                SensorLB.Content = ps.SensorStatus.ToString();
                LoopCountLB.Content = ps.LoopCount.ToString();
                BaseCountOnLB.Content = ps.OnBaseCount.ToString();
                BaseCountOffLB.Content = ps.OffBaseCount.ToString();
                LevelCountOnLB.Content = ps.OnLevelCount.ToString();
                LevelCountOffLB.Content= ps.OffLevelCount.ToString();

                if (ps.LoopSensorStatus != _preLoopSensorStatus)
                {
                    LoopSensorStatusLB.Content = ps.LoopSensorStatus.ToString();
                    try
                    {
                        switch (ps.LoopSensorStatus)
                        {
                            case LoopSensorStatus.ON:
                                viewModel.SetLoopSensorImg(IMG_LS_ON);
                                LoopSensorStatusLB.Foreground = LIGHT_BLUE;
                                break;
                            case LoopSensorStatus.OFF:
                                viewModel.SetLoopSensorImg(IMG_LS_OFF);
                                LoopSensorStatusLB.Foreground = GRAY;
                                break;
                            case LoopSensorStatus.ForcedON:
                                viewModel.SetLoopSensorImg(IMG_LS_FORCE_ON);
                                LoopSensorStatusLB.Foreground = DEEP_BLUE;
                                break;
                            case LoopSensorStatus.ForcedOFF:
                                viewModel.SetLoopSensorImg(IMG_LS_FORCE_OFF);
                                LoopSensorStatusLB.Foreground = DEEP_GRAY;
                                break;
                            case LoopSensorStatus.Error:
                                viewModel.SetLoopSensorImg(IMG_LS_ERROR);
                                LoopSensorStatusLB.Foreground = THIN_RED;
                                break;
                            case LoopSensorStatus.NA:
                                viewModel.SetLoopSensorImg(IMG_LS_NA);
                                LoopSensorStatusLB.Foreground = GRAY;
                                break;
                        }
                    }
                    catch(Exception exp)
                    {
                        logger.Error("fail to show image of loop sensor status.", exp);
                    }
                    
                    _preLoopSensorStatus = ps.LoopSensorStatus;
                }

                if (ps.LockPlateStatus != _preLockPlateStatus)
                {
                    _preLockPlateStatus = ps.LockPlateStatus;
                    if (ps.LockPlateStatus == LockPlateStatus.Inclining ||
                        ps.LockPlateStatus == LockPlateStatus.ForcedIncline)
                    {
                        viewModel.SetArrowImg(IMG_ARROW_UP);
                        _blinkTimer.Start();
                    }
                    else if (ps.LockPlateStatus == LockPlateStatus.Declining ||
                        ps.LockPlateStatus == LockPlateStatus.ForcedDecline)
                    {
                        viewModel.SetArrowImg(IMG_ARROW_DOWN);
                        _blinkTimer.Start();
                    }
                    else ArrowIMG.Visibility = Visibility.Collapsed;
                }
                else if(ps.LockPlateStatus == LockPlateStatus.StandBy)
                {
                    if(_blinkTimer.Enabled == true)_blinkTimer.Stop();

                    if(ArrowIMG.Visibility == Visibility.Visible)
                        ArrowIMG.Visibility = Visibility.Collapsed;
                }

                if (ps.SensorStatus != _preSensorStatus) 
                {
                    if (ps.SensorStatus == SensorStatus.TopEnd)
                    {
                        viewModel.SetPlateImg(IMG_PLATE_UP);
                    }
                    else if (ps.SensorStatus == SensorStatus.BottomEnd)
                    {
                        viewModel.SetPlateImg(IMG_PLATE_DOWN);
                    }

                    _preSensorStatus = ps.SensorStatus;
                }
            });
        }
        private void PCRespReceivied(PC_Resp pcResp)
        {
            logger.Info($"Parking Plate Control Result: {pcResp}");
        }
        private void NoRespReceivied(RS485Cytel sender)
        {
            logger.Warn($"no response from #{sender.Address} plate.");
        }
        private void SendPollingCmdBTN_Click(object sender, RoutedEventArgs e)
        {
            _timer.Start();  // 開始送出 polling cmd
        }

        private void GetPlateStatusBTN_Click(object sender, RoutedEventArgs e)
        {
            logger.Info($"#{cytel1.Address}: Send GetPlateStatus command.");
            cytel1.GetLockingPlateStatus();
        }

        private void LoopSensorOnBTN_Click(object sender, RoutedEventArgs e)
        {
            logger.Info($"#{cytel1.Address}: Send TurnLoopOn command.");
            cytel1.TurnLoopOn();
        }

        private void LoopSensorOffBTN_Click(object sender, RoutedEventArgs e)
        {
            logger.Info($"#{cytel1.Address}: Send TurnLoopOff command.");
            cytel1.TurnLoopOff();
        }

        private void LockingCtrlUpBTN_Click(object sender, RoutedEventArgs e)
        {
            logger.Info($"#{cytel1.Address}: Send TurnLockUp command.");
            cytel1.TurnLockUp();
        }

        private void LockingCtrlDownBTN_Click(object sender, RoutedEventArgs e)
        {
            logger.Info($"#{cytel1.Address}: Send TurnLockDown command.");
            cytel1.TurnLockDown();
        }

        private void StopPollingCmdBTN_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();  // 停止送出 polling cmd
        }
        private BitmapImage InitialBitmapImage(string uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(uri);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            return image;
        }
        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage image = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }
            return image;
        }
    }
}