using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Config.Net;
using HandyControl.Controls;
using KeyboardMouseHookLibrary;
using OCRLibrary;
using SQLHelperLibrary;
using TextHookLibrary;

namespace MisakaTranslator_WPF {
    public partial class MainWindow {
        private List<GameInfo> gameInfoList;
        private int gid; //当前选中的顺序，并非游戏ID
        private IntPtr hwnd;

        public static MainWindow Instance { get; set; }

        public MainWindow() {
            // Instance = this;
            Common.mainWin = this;

            var settings = new ConfigurationBuilder<IAppSettings>().UseJsonFile("settings/settings.json").Build();
            InitializeLanguage();
            InitializeComponent();
            Initialize(settings);

            //注册全局OCR热键
            this.SourceInitialized += new EventHandler(MainWindow_SourceInitialized);
        }

        private static void InitializeLanguage() {
            var appResource = Application.Current.Resources.MergedDictionaries;
            Common.appSettings = new ConfigurationBuilder<IAppSettings>().UseIniFile($"{Environment.CurrentDirectory}\\settings\\settings.ini").Build();
            foreach (var item in appResource) {
                if (item.Source.ToString().Contains("lang") && item.Source.ToString() != $@"lang/{Common.appSettings.AppLanguage}.xaml") {
                    appResource.Remove(item);
                    break;
                }
            }
        }

        //按下快捷键时被调用的方法
        public void CallBack() {
            Common.GlobalOCR();
        }

        private void Initialize(IAppSettings settings) {
            this.Resources["Foreground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(settings.ForegroundHex));
            gameInfoList = GameLibraryHelper.GetAllGameLibrary();
            Common.repairSettings = new ConfigurationBuilder<IRepeatRepairSettings>().UseIniFile(Environment.CurrentDirectory + "\\settings\\RepairSettings.ini").Build();
            GameLibraryPanel_Init();
            //先初始化这两个语言，用于全局OCR识别
            Common.UsingDstLang = "zh";
            Common.UsingSrcLang = "jp";
        }

        /// <summary>
        /// 游戏库瀑布流初始化
        /// </summary>
        private void GameLibraryPanel_Init() {
            Random random = new Random();
            var bushLst = new List<SolidColorBrush>
                {
                    System.Windows.Media.Brushes.CornflowerBlue,
                    System.Windows.Media.Brushes.IndianRed,
                    System.Windows.Media.Brushes.Orange,
                    System.Windows.Media.Brushes.ForestGreen
                };
            if (gameInfoList != null) {
                for (var i = 0; i < gameInfoList.Count; i++) {
                    var tb = new TextBlock() {
                        Text = gameInfoList[i].GameName,
                        Foreground = System.Windows.Media.Brushes.White,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(3)
                    };
                    var ico = new System.Windows.Controls.Image() {
                        Source = ImageProcFunc.ImageToBitmapImage(ImageProcFunc.GetAppIcon(gameInfoList[i].FilePath)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 64,
                        Width = 64
                    };
                    var gd = new Grid();
                    gd.Children.Add(ico);
                    gd.Children.Add(tb);
                    var back = new Border() {
                        Name = "game" + i,
                        Width = 150,
                        Child = gd,
                        Margin = new Thickness(5),
                        Background = bushLst[i % 4],
                    };
                    back.MouseEnter += Border_MouseEnter;
                    back.MouseLeave += Border_MouseLeave;
                    back.MouseLeftButtonDown += Back_MouseLeftButtonDown;
                    GameLibraryPanel.RegisterName("game" + i, back);
                    GameLibraryPanel.Children.Add(back);
                }
            }
            var textBlock = new TextBlock() {
                Text = Application.Current.Resources["MainWindow_ScrollViewer_AddNewGame"].ToString(),
                Foreground = System.Windows.Media.Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(3)
            };
            var grid = new Grid();
            grid.Children.Add(textBlock);
            var border = new Border() {
                Name = "AddNewName",
                Width = 150,
                Child = grid,
                Margin = new Thickness(5),
                Background = (SolidColorBrush)this.Resources["Foreground"]
            };
            border.MouseEnter += Border_MouseEnter;
            border.MouseLeave += Border_MouseLeave;
            border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
            GameLibraryPanel.RegisterName("AddNewGame", border);
            GameLibraryPanel.Children.Add(border);
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            AddNewGameDrawer.IsOpen = true;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e) {
            hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
            //注册热键
            Common.GlobalOCRHotKey = new GlobalHotKey();
            if (Common.GlobalOCRHotKey.RegisterHotKeyByStr(Common.appSettings.GlobalOCRHotkey, hwnd, CallBack) == false) {
                Growl.ErrorGlobal(Application.Current.Resources["MainWindow_GlobalOCRError_Hint"].ToString());
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            Common.GlobalOCRHotKey.ProcessHotKey(System.Windows.Forms.Message.Create(hwnd, msg, wParam, lParam));
            return IntPtr.Zero;
        }

        private static SettingsWindow _settingsWindow;

        private void SettingsBtn_Click(object sender, RoutedEventArgs e) {
            if (_settingsWindow == null || _settingsWindow.IsVisible == false) {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Show();
            }
            else {
                _settingsWindow.WindowState = WindowState.Normal;
                _settingsWindow.Activate();
            }
        }

        private void HookGuideBtn_Click(object sender, RoutedEventArgs e) {
            var ggw = new GameGuideWindow(1);
            ggw.Show();
        }

        private void OCRGuideBtn_Click(object sender, RoutedEventArgs e) {
            var ggw = new GameGuideWindow(2);
            ggw.Show();
        }

        private static void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var b = (Border)sender;
            b.BorderThickness = new Thickness(2);
        }

        private static void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var b = (Border)sender;
            b.BorderThickness = new Thickness(0);
        }

        private void Back_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var b = (Border)sender;
            var str = b.Name;
            var temp = str.Remove(0, 4);
            gid = int.Parse(temp);

            GameNameTag.Text = Application.Current.Resources["MainWindow_Drawer_Tag_GameName"].ToString() + gameInfoList[gid].GameName;
            if (gameInfoList[gid].TransMode == 1) {
                TransModeTag.Text = Application.Current.Resources["MainWindow_Drawer_Tag_TransMode"].ToString() + "Hook";
            }
            else {
                TransModeTag.Text = Application.Current.Resources["MainWindow_Drawer_Tag_TransMode"].ToString() + "OCR";
            }

            GameInfoDrawer.IsOpen = true;
        }

        private void StartTranslateByGid(int gid) {
            var ps = Process.GetProcesses();
            var pidList = new List<Process>();

            foreach (var process in ps) {
                string filepath;
                try {
                    filepath = process.MainModule.FileName;
                }
                catch (Exception) {
                    continue;
                    //这个地方直接跳过，是因为32位程序确实会读到64位的系统进程，而系统进程是不能被访问的
                    //throw ex;
                }

                if (gameInfoList[gid].FilePath == filepath) {
                    pidList.Add(process);
                }
            }

            if (pidList.Count == 0) {
                HandyControl.Controls.MessageBox.Show(Application.Current.Resources["MainWindow_StartError_Hint"].ToString(), Application.Current.Resources["MessageBox_Hint"].ToString());
                return;
            }
            else {
                var pid = pidList[0].Id;
                pidList.Clear();
                pidList = ProcessHelper.FindSameNameProcess(pid);
            }

            Common.GameID = gameInfoList[gid].GameID;
            Common.transMode = 1;
            Common.UsingDstLang = gameInfoList[gid].DstLang;
            Common.UsingSrcLang = gameInfoList[gid].SrcLang;
            Common.UsingRepairFunc = gameInfoList[gid].RepairFunc;

            switch (Common.UsingRepairFunc) {
                case "RepairFun_RemoveSingleWordRepeat":
                    Common.repairSettings.SingleWordRepeatTimes = gameInfoList[gid].RepairParamA;
                    break;
                case "RepairFun_RemoveSentenceRepeat":
                    Common.repairSettings.SentenceRepeatFindCharNum = gameInfoList[gid].RepairParamA;
                    break;
                case "RepairFun_RegexReplace":
                    Common.repairSettings.Regex = gameInfoList[gid].RepairParamA;
                    Common.repairSettings.Regex_Replace = gameInfoList[gid].RepairParamB;
                    break;
                default:
                    break;
            }

            Common.RepairFuncInit();

            Common.textHooker = pidList.Count == 1 ? new TextHookHandle(pidList[0].Id) : new TextHookHandle(pidList);

            Common.textHooker.Init(!gameInfoList[gid].Isx64);
            Common.textHooker.HookCodeList.Add(gameInfoList[gid].Hookcode);
            Common.textHooker.HookCode_Custom = gameInfoList[gid].HookCodeCustom;

            if (gameInfoList[gid].IsMultiHook) {
                var ggw = new GameGuideWindow(3);
                ggw.Show();
            }
            else {
                //无重复码。直接进游戏
                Common.textHooker.MisakaCodeList = null;
                //2020-06-08 大部分情况无重复码的游戏不会hook到很多，不进行去多余hook
                //Common.textHooker.DetachUnrelatedHookWhenDataRecv = Convert.ToBoolean(Common.appSettings.AutoDetach);
                Common.textHooker.StartHook(Convert.ToBoolean(Common.appSettings.AutoHook));
                var task1 = Task.Run(async delegate
                {
                    await Task.Delay(3000);
                    Common.textHooker.Auto_AddHookToGame();
                });

                var tw = new TranslateWindow();
                tw.Show();
            }
        }

        private void CloseDrawerBtn_Click(object sender, RoutedEventArgs e) {
            GameInfoDrawer.IsOpen = false;
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e) {
            var res = Process.Start(gameInfoList[gid].FilePath);
            res?.WaitForInputIdle(5000);
            GameInfoDrawer.IsOpen = false;
            await Task.Delay(2000);
            StartTranslateByGid(gid);
        }

        /// <summary>
        /// 删除游戏按钮事件
        /// </summary>
        private void DeleteGameBtn_Click(object sender, RoutedEventArgs e) {
            if (HandyControl.Controls.MessageBox.Show(Application.Current.Resources["MainWindow_Drawer_DeleteGameConfirmBox"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                GameLibraryHelper.DeleteGameByID(gameInfoList[gid].GameID);
                var b = GameLibraryPanel.FindName($"game{gid}") as Border;
                GameLibraryPanel.Children.Remove(b);
                GameInfoDrawer.IsOpen = false;
            }

        }

        private void UpdateNameBtn_Click(object sender, RoutedEventArgs e) {
            Dialog.Show(new GameNameDialog(gameInfoList, gid));
        }

        /*
         * 插眼
         */
        private async void LEStartBtn_Click(object sender, RoutedEventArgs e) {
            var filepath = gameInfoList[gid].FilePath;
            var p = new ProcessStartInfo();
            var lePath = Common.appSettings.LEPath;
            p.FileName = lePath + "\\LEProc.exe";
            // 记住加上引号，否则可能会因为路径带空格而无法启动
            p.Arguments = $"-run \"{filepath}\"";
            p.UseShellExecute = false;
            p.WorkingDirectory = lePath;
            var res = Process.Start(p);
            res?.WaitForInputIdle(5000);
            GameInfoDrawer.IsOpen = false;
            await Task.Delay(2000);
            StartTranslateByGid(gid);
        }

        private void BlurWindow_Closing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            switch (Common.appSettings.OnClickCloseButton) {
                case "Minimization":
                    Visibility = Visibility.Collapsed;
                    break;
                case "Exit":
                    Common.GlobalOCRHotKey.UnRegisterGlobalHotKey(hwnd, CallBack);
                    CloseNotifyIcon();
                    Application.Current.Shutdown();
                    break;
            }
        }

        public void CloseNotifyIcon() {
            // Instance.NotifyIconContextContent.Visibility = Visibility.Collapsed;
        }

        // private void ButtonPush_OnClick(object sender, RoutedEventArgs e) => NotifyIconContextContent.CloseContextControl();

        /// <summary>
        /// 切换语言通用事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Language_MenuItem_Click(object sender, RoutedEventArgs e) {
            if (sender is MenuItem menuItem) {
                switch (menuItem.Tag) {
                    case "zh-cn":
                        Common.appSettings.AppLanguage = "zh-CN";
                        HandyControl.Controls.MessageBox.Show("语言配置已修改！重启软件后生效！", "提示");
                        break;
                    case "en-us":
                        Common.appSettings.AppLanguage = "en-US";
                        HandyControl.Controls.MessageBox.Show("Language configuration has been modified! It will take effect after restarting MisakaTranslator!", "Hint");
                        break;
                }
            }
        }

        private void AutoStart_BtnClick(object sender, RoutedEventArgs e) {
            var res = GetGameListHasProcessGame_PID_ID();
            if (res == -1) {
                Growl.ErrorGlobal(Application.Current.Resources["MainWindow_AutoStartError_Hint"].ToString());
            }
            else {
                StartTranslateByGid(res);
            }
        }

        /// <summary>
        /// 寻找任何正在运行中的之前已保存过的游戏
        /// </summary>
        /// <returns>数组索引（非GameID），-1代表未找到</returns>
        private int GetGameListHasProcessGame_PID_ID() {
            var processes = Process.GetProcesses();
            var ret = new List<int>();
            gameInfoList = GameLibraryHelper.GetAllGameLibrary();
            if (gameInfoList != null) {
                foreach (var process in processes) {
                    for (int j = 0; j < gameInfoList.Count; j++) {
                        string filepath;
                        try {
                            filepath = process.MainModule.FileName;
                        }
                        catch (Win32Exception ex) {
                            continue;
                        }

                        if (filepath == gameInfoList[j].FilePath) {
                            return j;
                        }
                    }
                }

                return -1;
            }
            return -1;
        }

        private void ClipboardGuideBtn_Click(object sender, RoutedEventArgs e) {
            Common.textHooker = new TextHookHandle();
            Common.GameID = 0;
            Common.transMode = 1;
            Common.textHooker.AddClipBoardThread();

            //剪贴板方式读取的特殊码和misakacode
            Common.textHooker.HookCodeList.Add("HB0@0");
            Common.textHooker.MisakaCodeList.Add("【0:-1:-1】");

            var ggw = new GameGuideWindow(4);
            ggw.Show();
        }


        private void BlurWindow_ContentRendered(object sender, EventArgs e) {
            List<string> res = Common.CheckUpdate();
            if (res != null) {
                MessageBoxResult dr = HandyControl.Controls.MessageBox.Show(res[0] + "\n" + Application.Current.Resources["MainWindow_AutoUpdateCheck"].ToString(), "AutoUpdateCheck", MessageBoxButton.OKCancel);

                if (dr == MessageBoxResult.OK) {
                    System.Diagnostics.Process.Start(res[1]);
                }

            }
        }

        private void ComicTransBtn_Click(object sender, RoutedEventArgs e) {
            var ctmw = new ComicTranslator.ComicTransMainWindow();
            ctmw.Show();
        }
    }
}
