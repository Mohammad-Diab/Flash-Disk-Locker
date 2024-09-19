using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
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
using Microsoft.Win32.SafeHandles;

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
                string output = RunDiskPart("list disk");
                bw.ReportProgress(0, output);
                List<int> columns;
                List<string> rows = GetDiskRows(output, out columns);
                if (rows.Count == 0)
                    return;

                List<string> commands = new List<string>();
                foreach (var row in rows)
                {
                    commands.Add("select disk " + GetDiskNumber(row));
                    commands.Add("detail disk");
                }
                output = RunDiskPart(commands.ToArray());
                bw.ReportProgress(0, output);

                var disks = output.Split(new[] { "DISKPART> " }, StringSplitOptions.None);
                for (int i = 0; i < rows.Count && 2 * i + 2 < disks.Length; i++)
                {
                    var item = MakeListItem(rows[i], columns, disks[2 * i + 2]);
                    if (item != null)
                        bw.ReportProgress(1, item);
                }
            };

            bw.ProgressChanged += (object sender, ProgressChangedEventArgs args) =>
            {
                if (args.ProgressPercentage == 0)
                    log.Append(args.UserState.ToString());
                else
                {
                    var item = args.UserState as ListItem;
                    item.Img = new BitmapImage(new Uri("pack://application:,,,/Images/" + (item.Type == DiskType.SSD ? "SSD" : item.Type == DiskType.Flash ? "Flash" : "HDD") + ".png"));
                    data_binding.Add(item);
                }
            };

            bw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs args) =>
              {
                  if (args.Error != null)
                      MessageBox.Show("Error\n" + args.Error.Message, "Disk locker", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                  IsEnabled = true;
              };

            bw.RunWorkerAsync();
        }

        static string RunDiskPart(params string[] commands)
        {
            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo("diskpart.exe");
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            process.StartInfo = psi;
            process.Start();
            foreach (var command in commands)
                process.StandardInput.WriteLine(command);
            process.StandardInput.WriteLine("exit");
            process.StandardInput.Close();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        static int RunDiskPartScript(out string output, params string[] commands)
        {
            string script = System.IO.Path.GetTempFileName();
            File.WriteAllLines(script, commands);
            try
            {
                Process process = new Process();
                ProcessStartInfo psi = new ProcessStartInfo("diskpart.exe", "/s \"" + script + "\"");
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                process.StartInfo = psi;
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode;
            }
            finally
            {
                File.Delete(script);
            }
        }

        static List<string> GetDiskRows(string output, out List<int> columns)
        {
            List<string> rows = new List<string>();
            columns = new List<int>();
            var lines = output.Replace("\r", "").Split('\n');
            int start = Array.FindIndex(lines, x => x.Trim().Length > 0 && x.Trim(' ', '-').Length == 0);
            if (start < 0)
                return rows;

            for (int i = 0; i < lines[start].Length; i++)
                if (lines[start][i] == '-' && (i == 0 || lines[start][i - 1] != '-'))
                    columns.Add(i);

            for (int i = start + 1; i < lines.Length && lines[i].Trim().Length > 0; i++)
                if (GetDiskNumber(lines[i]) > -1)
                    rows.Add(lines[i]);
            return rows;
        }

        static int GetDiskNumber(string row)
        {
            var match = Regex.Match(row, "[0-9]+");
            return match.Success ? int.Parse(match.Value) : -1;
        }

        static string GetColumn(string row, List<int> columns, int index)
        {
            if (index >= columns.Count || columns[index] >= row.Length)
                return "";
            int end = index + 1 < columns.Count ? Math.Min(columns[index + 1], row.Length) : row.Length;
            return row.Substring(columns[index], end - columns[index]).Trim();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern SafeFileHandle CreateFile(string file_name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(SafeFileHandle device, uint code, IntPtr in_buffer, uint in_size, out DiskAttributes out_buffer, uint out_size, out uint returned, IntPtr overlapped);

        static bool IsDiskReadonly(int number, string[] detail)
        {
            using (SafeFileHandle handle = CreateFile(@"\\.\PhysicalDrive" + number, 0, 3, IntPtr.Zero, 3, 0, IntPtr.Zero))
            {
                DiskAttributes attributes;
                uint returned;
                if (!handle.IsInvalid && DeviceIoControl(handle, 0x700F0, IntPtr.Zero, 0, out attributes, (uint)Marshal.SizeOf(typeof(DiskAttributes)), out returned, IntPtr.Zero))
                    return (attributes.Attributes & 2) != 0;
            }
            return detail.Length > 9 && detail[9].ToLower().Contains("yes");
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
                bool done = false;
                string message = "";
                bw.DoWork += (object sender, DoWorkEventArgs args) =>
                {
                    string output = RunDiskPart("select disk " + selected_disk.Number, "detail disk");
                    bw.ReportProgress(0, output);
                    var disks = output.Split(new[] { "DISKPART> " }, StringSplitOptions.None);
                    if (disks.Length < 3 || !CompareDisks(disks[2], selected_disk))
                    {
                        message = "The disk list has changed. Please refresh it and try again.";
                        return;
                    }

                    int exit_code = RunDiskPartScript(out output, "select disk " + selected_disk.Number, string.Format("attributes disk {0} readonly", _readonly ? "set" : "clear"));
                    bw.ReportProgress(0, output);
                    done = exit_code == 0;
                    message = string.Join("\n", output.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(3).ToArray());
                    if (message.Length == 0)
                        message = "DiskPart exit code: " + exit_code;
                };

                bw.ProgressChanged += (object sender, ProgressChangedEventArgs args) =>
                {
                    log.Append(args.UserState.ToString());
                };

                bw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs args) =>
                {
                    if (args.Error != null)
                        MessageBox.Show("Error\n" + args.Error.Message, "Disk locker", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    else
                        MessageBox.Show(message, "Disk locker", MessageBoxButton.OK, done ? MessageBoxImage.Information : MessageBoxImage.Error);
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

        static ListItem MakeListItem(string row, List<int> columns, string detail)
        {
            var multi_lines = detail.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int number = GetDiskNumber(row);
            if (number > -1 && multi_lines.Length > 2)
            {
                ListItem item = new ListItem();
                item.Number = number;
                item.ID = GetColumn(row, columns, 0);
                item.Status = GetColumn(row, columns, 1);
                item.Size = GetColumn(row, columns, 2);
                item.Free = GetColumn(row, columns, 3);
                item.Name = multi_lines[0];
                item.Signature = multi_lines[1];
                item.IsReadonly = IsDiskReadonly(number, multi_lines);
                item.ReadOnly = item.IsReadonly ? Colors.Red : Colors.Green;
                item.LockedTooltip = item.IsReadonly ? "Disk is Readonly" : "Disk is not Readonly";
                item.Type = multi_lines[0].ToUpper().Contains("SSD") ? DiskType.SSD : multi_lines[2].ToUpper().Contains("SATA") ? DiskType.HDD : DiskType.Flash;
                item.DiskInfo = row.Trim() + "\n" + detail.Trim();
                return item;
            }
            return null;
        }

        static bool CompareDisks(string detail, ListItem item)
        {
            var multi_lines = detail.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return multi_lines.Length > 1 && multi_lines[0] == item.Name && multi_lines[1] == item.Signature;
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

    [StructLayout(LayoutKind.Sequential)]
    struct DiskAttributes
    {
        public uint Version;
        public uint Reserved;
        public ulong Attributes;
    }

    class ListItem
    {
        public ListItem()
        {
        }

        int number;
        public int Number
        {
            get { return number; }
            set { number = value; }
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

        string signature;
        public string Signature
        {
            get { return signature; }
            set { signature = value; }
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
