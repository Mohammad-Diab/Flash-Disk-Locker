using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flash_disk_locker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<ListItem> data_binding = new ObservableCollection<ListItem>();

        TabItem selector = TabItem.Locker;
        TabItem Selector
        {
            get { return selector; }
            set
            {
                selector = value;
                switch (value)
                {
                    case TabItem.Locker:

                        //locker_tab.FontWeight = FontWeights.Bold;
                        //logo_tab.FontWeight = info_tab.FontWeight = FontWeights.Normal;

                        ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(10, 37, 10, 5), new Duration(TimeSpan.FromMilliseconds(250)));
                        main_grid.BeginAnimation(MarginProperty, ta1);

                        ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(0), new Duration(TimeSpan.FromMilliseconds(100)));
                        line.BeginAnimation(MarginProperty, ta2);
                        line.Width = locker_tab.ActualWidth;

                        locker_tab.Cursor = Cursors.Arrow;
                        log_tab.Cursor = info_tab.Cursor = Cursors.Hand;

                        locker_tab.Background = Brushes.White;
                        locker_tab.Foreground = Brushes.Black;
                        log_tab.Foreground = info_tab.Foreground = Brushes.White;
                        log_tab.Background = info_tab.Background = Brushes.Black;

                        btm_img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/refresh.png"));
                        main_btm.ToolTip = "Refresh list";

                        info_btm.Visibility = Visibility.Visible;

                        break;
                    case TabItem.Log:

                        //logo_tab.FontWeight = FontWeights.Bold;
                        //locker_tab.FontWeight = info_tab.FontWeight = FontWeights.Normal;

                        ThicknessAnimation ta3 = new ThicknessAnimation(new Thickness(-565, 37, 10, 5), new Duration(TimeSpan.FromMilliseconds(250)));
                        main_grid.BeginAnimation(MarginProperty, ta3);

                        ThicknessAnimation ta4 = new ThicknessAnimation(new Thickness(locker_tab.ActualWidth + 10, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(100)));
                        line.BeginAnimation(MarginProperty, ta4);
                        line.Width = log_tab.ActualWidth;

                        log_tab.Cursor = Cursors.Arrow;
                        locker_tab.Cursor = info_tab.Cursor = Cursors.Hand;

                        log_tab.Background = Brushes.White;
                        log_tab.Foreground = Brushes.Black;
                        locker_tab.Foreground = info_tab.Foreground = Brushes.White;
                        locker_tab.Background = info_tab.Background = Brushes.Black;

                        btm_img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/clear.png"));
                        main_btm.ToolTip = "Clear log";

                        info_btm.Visibility = Visibility.Collapsed;

                        break;
                    case TabItem.Info:

                        //info_tab.FontWeight = FontWeights.Bold;
                        //locker_tab.FontWeight = logo_tab.FontWeight = FontWeights.Normal;

                        ThicknessAnimation ta5 = new ThicknessAnimation(new Thickness(-1140, 37, 10, 5), new Duration(TimeSpan.FromMilliseconds(250)));
                        main_grid.BeginAnimation(MarginProperty, ta5);

                        ThicknessAnimation ta6 = new ThicknessAnimation(new Thickness(locker_tab.ActualWidth + log_tab.ActualWidth + +20, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(100)));
                        line.BeginAnimation(MarginProperty, ta6);
                        line.Width = info_tab.ActualWidth;

                        info_tab.Cursor = Cursors.Arrow;
                        log_tab.Cursor = locker_tab.Cursor = Cursors.Hand;

                        info_tab.Background = Brushes.White;
                        info_tab.Foreground = Brushes.Black;
                        log_tab.Foreground = locker_tab.Foreground = Brushes.White;
                        log_tab.Background = locker_tab.Background = Brushes.Black;

                        btm_img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/help.png"));
                        main_btm.ToolTip = "View help";

                        info_btm.Visibility = Visibility.Collapsed;

                        break;
                }
            }
        }

        StringBuilder log = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
            BackgroundWorker log_viewer = new BackgroundWorker();
            log_viewer.WorkerReportsProgress = true;

            log_viewer.ProgressChanged += (object send, ProgressChangedEventArgs args) => 
            {
                log_txtbx.Text = log.ToString();
            };

            log_viewer.DoWork += (object send, DoWorkEventArgs args) =>
            {
                while(true)
                {
                    log_viewer.ReportProgress(1);
                    Thread.Sleep(500);
                }
            };

            log_viewer.RunWorkerAsync();
        }

        private void MainBtm_Click(object sender, RoutedEventArgs e)
        {
            switch (Selector)
            {
                case TabItem.Locker:
                    GetDisk(false);
                    break;
                case TabItem.Log:
                    log.Clear();
                    break;
                case TabItem.Info:
                    MessageBox.Show("Coming Soon...", "Disk locker", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        void GetDisk(bool startup)
        {
            //IsEnabled = false;
            data_binding.Clear();
            if (!startup)
            {
                log.AppendLine("**********************************************");
                log.AppendLine();
            }
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += (object sender, DoWorkEventArgs args) =>
            {
                try
                {
                    Process process = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardInput = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    process.StartInfo = psi;
                    process.Start();
                    process.StandardInput.WriteLine("diskpart");
                    process.StandardInput.WriteLine("list disk");
                    process.StandardInput.WriteLine("exit");
                    bw.ReportProgress(0);
                    List<string> tmp = new List<string>();
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string x = process.StandardOutput.ReadLine();
                        log.AppendLine(x);
                        if (x.Length > 9 && (x.ToLower().Contains("online") || x.ToLower().Contains("offline")))
                        {
                            tmp.Add(x);
                            //bw.ReportProgress(1, x);
                        }
                        if (x.ToUpper().Equals("LEAVING DISKPART..."))
                            break;

                    }

                    process.StandardInput.WriteLine("diskpart");
                    foreach (var i in tmp)
                    {
                        process.StandardInput.WriteLine("select " + i.Substring(1, 8));
                        process.StandardInput.WriteLine("detail disk");
                    }
                    process.StandardInput.WriteLine("exit");
                    int ind = -1;
                    string _line = "";
                    string output = "";

                    while (!(((_line = process.StandardOutput.ReadLine()).Equals("Leaving DiskPart...")) || process.StandardOutput.EndOfStream))
                        output += _line + "\n";

                    var disks = output.Split(new[] { "DISKPART> " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 2; i < disks.Length; i += 2)
                    {
                        if (disks[i].Length > 35)
                        {
                            disks[i].Replace("\nLeaving DiskPart...", "");
                            ind++;
                            bw.ReportProgress(1, tmp[ind] + "\n" + disks[i]);
                        }
                    }
                    process.StandardInput.WriteLine("exit");
                }
                catch (Exception ex) { MessageBox.Show("Error\n" + ex.Message, "Disk locker", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
            };

            bw.ProgressChanged += (object sender, ProgressChangedEventArgs args) =>
            {
                if (args.ProgressPercentage == 0)
                    data_binding.Clear();
                else
                {
                    var item = MakeListItem(args.UserState.ToString());
                    if (item != null)
                        data_binding.Add(item);
                }
                    
            };

            bw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs args) =>
              {
                  IsEnabled = true;
              };

            bw.RunWorkerAsync();
        }

        public bool IsRunningAsAdmin()
        {
            WindowsIdentity cur = WindowsIdentity.GetCurrent();
            foreach (IdentityReference role in cur.Groups)
            {
                if (role.IsValidTargetType(typeof(SecurityIdentifier)))
                {
                    SecurityIdentifier sid = (SecurityIdentifier)role.Translate(typeof(SecurityIdentifier));
                    if (sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) || sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid))
                        return true;
                }
            }
            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsRunningAsAdmin())
            {
                MessageBox.Show("This app needs administrator rights. Please run it as administrator.", "Disk locker", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            else
            {
                Title = "[Adminstrator] " + Title;
                disk_lstbx.DataContext = data_binding;
                GetDisk(true);
            }
        }

        private void Action_btm_Click(object sender, RoutedEventArgs e)
        {
            if (disk_lstbx.SelectedIndex > -1)
                SetAttribute(!(disk_lstbx.SelectedItem as ListItem).IsReadonly);
        }

        private void SetAttribute(bool _readonly)
        {
            if (disk_lstbx.SelectedIndex > -1)
            {
                //IsEnabled = false;
                log.AppendLine("**********************************************");
                log.AppendLine();
                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                ListItem selected_disk = disk_lstbx.SelectedItem as ListItem;
                bw.DoWork += (object sender, DoWorkEventArgs args) =>
                {
                    try
                    {
                        Process process = new Process();
                        ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                        psi.RedirectStandardOutput = true;
                        psi.RedirectStandardInput = true;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        process.StartInfo = psi;
                        process.Start();
                        process.StandardInput.WriteLine("diskpart");
                        process.StandardInput.WriteLine("list disk");
                        int disk_id = -1, i = 0;
                        while (!process.StandardOutput.EndOfStream)
                        {
                            string x = process.StandardOutput.ReadLine();
                            log.AppendLine(x);
                            if (x.Length > 9 && x.ToLower().Contains("online"))
                            {
                                if (CompareDisks(x.ToUpper(), selected_disk))
                                {
                                    disk_id = i;
                                    process.StandardInput.WriteLine("select" + x.Substring(0, 10));

                                    while (!process.StandardOutput.EndOfStream)
                                    {
                                        x = process.StandardOutput.ReadLine();
                                        log.AppendLine(x);
                                        if (x.Length > 0 && x.ToLower().Contains("disk "))
                                        {
                                            break;
                                        }
                                    }
                                    process.StandardInput.WriteLine(string.Format("attribute disk {0} readonly", _readonly ? "set" : "clear"));

                                    while (!process.StandardOutput.EndOfStream)
                                    {
                                        x = process.StandardOutput.ReadLine();
                                        log.AppendLine(x);
                                        if (x.Length > 0 && x.ToLower().Contains("attributes"))
                                        {
                                            MessageBox.Show(x, "Disk locker", MessageBoxButton.OK, x.Contains("successfully") ? MessageBoxImage.Information : MessageBoxImage.Error);
                                            log.AppendLine(process.StandardOutput.ReadToEnd());
                                        }
                                    }

                                    process.StandardOutput.ReadLine();


                                    break;
                                }
                                i++;
                            }
                        }
                        process.StandardInput.WriteLine("exit");
                        process.Start();
                    }
                    catch (Exception) { }
                };

                bw.RunWorkerAsync();


            }
            else
                MessageBox.Show("Please select disk first.", "Disk locker", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void InfoBtm_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hello Human", "Disk locker", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private ListItem MakeListItem(string str)
        {
            var multi_lines = str.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (multi_lines.Length > 10)
            {
                ListItem item = new ListItem();
                item.ID = multi_lines[0].Substring(1, 8);
                item.Status = multi_lines[0].Substring(12, 13);
                item.Size = multi_lines[0].Substring(28, 7);
                item.Free = multi_lines[0].Substring(38, 7);
                item.Name = multi_lines[1];
                item.IsReadonly = multi_lines[10].ToLower().Contains("yes");
                item.ReadOnly = item.IsReadonly ? Colors.Red : Colors.Green;
                item.LockedTooltip = item.IsReadonly ? "Disk is Readonly" : "Disk is not Readonly";
                item.Type = multi_lines[1].ToUpper().Contains("SSD") ? DiskType.SSD : multi_lines[3].ToUpper().Contains("SATA") ? DiskType.HDD : DiskType.Flash;
                item.Img = new BitmapImage(new Uri("pack://application:,,,/Images/" + (item.Type == DiskType.SSD ? "SSD" : item.Type == DiskType.Flash ? "Flash" : "HDD") + ".png"));
                item.DiskInfo = str;
                return item;
            }
            return null;
        }

        private bool CompareDisks(string str, ListItem item)
        {
            int num = Math.Abs(item.ID.ToUpper().CompareTo(str.Substring(1, 8))) +
             Math.Abs(item.Status.ToUpper().CompareTo(str.Substring(12, 13))) +
             Math.Abs(item.Size.ToUpper().CompareTo(str.Substring(28, 7))) +
             Math.Abs(item.Free.ToUpper().CompareTo(str.Substring(38, 7)));
            return num == 0;
        }

        private void LockerTab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Selector = TabItem.Locker;
        }

        private void LogoTab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Selector = TabItem.Log;
        }

        private void InfoTab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Selector = TabItem.Info;
        }

        private void DiskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (disk_lstbx.SelectedIndex > -1)
                if ((disk_lstbx.SelectedItem as ListItem).IsReadonly)
                {
                    action_btm_img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/unlock.png"));
                    action_btm_txt.Content = "Unlock";
                    action_btm.ToolTip = "Remove readonly mark up";
                }
                else
                {
                    action_btm_img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/lock.png"));
                    action_btm_txt.Content = "Lock";
                    action_btm.ToolTip = "Mark as readonly";
                }
        }

        private void ViewDiskInfo_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (disk_lstbx.SelectedIndex > -1)
                MessageBox.Show((disk_lstbx.SelectedItem as ListItem).DiskInfo, "Disk locker", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Please select disk first.", "Disk locker", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
    }

    enum TabItem
    {
        Locker = 1,
        Log = 2,
        Info = 3
    }

    enum DiskType
    {
        UnKnown = 0,
        HDD = 1,
        SSD = 2,
        Flash = 3
    }

    class ListItem
    {
        public ListItem()
        {
        }

        string id;
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        string status;
        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        string size;
        public string Size
        {
            get { return size; }
            set { size = value; }
        }

        string free;
        public string Free
        {
            get { return free; }
            set { free = value; }
        }

        BitmapImage img;
        public BitmapImage Img
        {
            get { return img; }
            set { img = value; }
        }

        Color read_only;
        public Color ReadOnly
        {
            get { return read_only; }
            set { read_only = value; }
        }

        string locked_tooltip;
        public string LockedTooltip
        {
            get { return locked_tooltip; }
            set { locked_tooltip = value; }
        }

        bool is_readonly;
        public bool IsReadonly
        {
            get { return is_readonly; }
            set { is_readonly = value; }
        }

        DiskType type;
        public DiskType Type
        {
            get { return type; }
            set { type = value; }
        }

        string disk_info;
        public string DiskInfo
        {
            get { return disk_info; }
            set { disk_info = value; }
        }
    }
}
