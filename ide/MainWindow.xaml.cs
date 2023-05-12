using Microsoft.Win32;
using System;
using ICSharpCode.AvalonEdit.Utils;
using System.Reflection;
using System.Windows.Resources;
using System.Resources;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;

namespace ide
{
    [Serializable]
    public class Settings
    {
        public int SizeFontEditor;
        public int SizeFontConsole;
        public string NameFontEditor;
        public string NameFontConsole;
        public string EncodingOpen;
        public string EncodingSave;
        public bool ShowLineNumbers;
        public string ConsolePath;
        public string ProcessName;
        public string Arguments;
        public string Part;
    }

    public partial class MainWindow : Window
    {
        public Settings settings = new Settings();

        private ConsoleContent dc = new ConsoleContent();

        private static string workPath = string.Empty;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = dc;
            Loaded += MainWindow_Loaded;
           	
            IHighlightingDefinition c;
            
            using(Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("ide.MathSyntax.xshd"))
            {
            	using(XmlReader r = new XmlTextReader(s))
            	{
            		c = HighlightingLoader.Load(r, HighlightingManager.Instance);
            	}
            }
            
            HighlightingManager.Instance.RegisterHighlighting("MathSyntax", new string[] { ".m" }, c);
            textEditor.SyntaxHighlighting = c;
        }
        
        private void Setup()
        {
            textEditor.FontFamily = new FontFamily(settings.NameFontEditor);
            textEditor.FontSize = settings.SizeFontEditor;
            textEditor.ShowLineNumbers = settings.ShowLineNumbers;

            InputBlock.FontFamily = new FontFamily(settings.NameFontConsole);
            PathBlock.FontFamily = new FontFamily(settings.NameFontConsole);
            ConsoleItemsControl.FontFamily = new FontFamily(settings.NameFontConsole);
            InputBlock.FontSize = settings.SizeFontConsole;
            PathBlock.FontSize = settings.SizeFontConsole;
            ConsoleItemsControl.FontSize = settings.SizeFontConsole;
        }

        public void UpdateEdit()
        {
            textEditor.FontFamily = new FontFamily(settings.NameFontEditor);
            textEditor.FontSize = settings.SizeFontEditor;
            textEditor.ShowLineNumbers = settings.ShowLineNumbers;
        }

        public void UpdateConsole()
        {
            InputBlock.FontFamily = new FontFamily(settings.NameFontConsole);
            PathBlock.FontFamily = new FontFamily(settings.NameFontConsole);
            ConsoleItemsControl.FontFamily = new FontFamily(settings.NameFontConsole);
            InputBlock.FontSize = settings.SizeFontConsole;
            PathBlock.FontSize = settings.SizeFontConsole;
            ConsoleItemsControl.FontSize = settings.SizeFontConsole;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            dc.Start(workPath);
            PathBlock.Text = dc.WorkPath;
            textEditor.TextChanged += TextEditor_TextChanged;
            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
        }

        private void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
            	RunCommand(InputBlock.Text);
            }
        }
        
        private void RunCommand(string command)
        {
        	PathBlock.Text = dc.WorkPath;
			dc.ConsoleInput = command;
			
			dc.RunCommand();
			
			Scroller.ScrollToTop();
			InputBlock.Focus();
        }

        private string currentFileName;
        
        private void OpenFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            
            if (dlg.ShowDialog() ?? false)
            {
                if(currentFileName == dlg.FileName)
                {
                    MessageBox.Show("Помилка даний файл вже відкритий!");
                    return;
                }
                
                currentFileName = dlg.FileName;
                TabList.Items.Add(currentFileName);
                textEditor.Load(currentFileName);
                TabList.SelectedIndex++;
                string extension = Path.GetExtension(currentFileName);
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(extension);
            }
        }

        private void SaveFile()
        {
            if (currentFileName == null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".m";
                if(dlg.ShowDialog() ?? false)
                {
                    currentFileName = dlg.FileName;
                    TabList.Items.Add(currentFileName);
                }
                else
                {
                    return;
                }
            }
            string temp = textEditor.Text;
            
            textEditor.Save(currentFileName);
            textEditor.Text = temp;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TabList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        	if(currentFileName != null)
        	{
        		textEditor.Save(currentFileName);
        	}
        	
            currentFileName = TabList.SelectedItem as String;
            
            textEditor.Load(currentFileName);
            string extension = Path.GetExtension(currentFileName);
            
            if(extension.Length != 0)
            {
            	textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(extension);
            }
        }
        
        private bool openInfo = false;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!openInfo)
            {
                InfoWindow info = new InfoWindow();
                info.Show();
                openInfo = true;
                info.Closing += (s, a) => openInfo = false;
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Redo();
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            UndoMenuItem.IsEnabled = textEditor.CanUndo;
            RedoMenuItem.IsEnabled = textEditor.CanRedo;
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Copy();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Cut();
        }
        
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
        	RunCommand("clear");
        }
        
        private void Past_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Paste();
        }

        private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile();
        }

        private bool openFindAndReplace = false;

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (!openFindAndReplace)
            {
                FindAndReplaceWindow find = new FindAndReplaceWindow(textEditor, 0);
                find.Show();
                openFindAndReplace = true;
                find.Closing += (s, a) => openFindAndReplace = false;
            }
        }

        private void FindAndReplace_Click(object sender, RoutedEventArgs e)
        {
            if (!openFindAndReplace)
            {
                FindAndReplaceWindow replace = new FindAndReplaceWindow(textEditor, 1);
                replace.Show();
                openFindAndReplace = true;
                replace.Closing += (s, a) => openFindAndReplace = false;
            }
        }
        
        private bool openSettingsEditor = false;

        private void SettingsEditor_Click(object sender, RoutedEventArgs e)
        {
            if (!openSettingsEditor)
            {
                SettingsEditorWindow settings = new SettingsEditorWindow(this);
                settings.Show();
                openSettingsEditor = true;
                settings.Closing += (s, a) => openSettingsEditor = false;
            }
        }

        private void SaveFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile();
        }

        private void NewFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            textEditor.Clear();
            currentFileName = null;
            SaveFile();
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            textEditor.Undo();
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            textEditor.Redo();
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            
            if (saveFileDialog.ShowDialog() == true)
            {
                currentFileName = saveFileDialog.FileName;
                File.WriteAllText(currentFileName, textEditor.Text);
                textEditor.Load(currentFileName);
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFileName));
            }
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
        	SaveFile();
        	dc.ConsoleInput = "start " + currentFileName;
        	dc.RunCommand();
        }

        private void SettingsTask_Click(object sender, RoutedEventArgs e)
        {
        	
        }

        private bool openSettingsConsole = false;

        private void SettingsConsole_Click(object sender, RoutedEventArgs e)
        {
            if (!openSettingsConsole)
            {
                SettingsConsoleWindow console = new SettingsConsoleWindow(this);
                console.Show();
                openSettingsConsole = true;
                console.Closing += (s, a) => openSettingsConsole = false;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (textEditor.CanUndo)
            {
                string messageBoxText = "Ви хочете зберегти зміни?";
                string caption = "Зберегти зміни?";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        SaveFile();
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                }
            }
            SaveSettings();
            dc.Finish();
        }

        private BinaryFormatter formatter = new BinaryFormatter();

        private const string FileNameSettings = "Settings";

        private void SaveSettings()
        {
            settings.NameFontConsole = InputBlock.FontFamily.ToString();
            settings.SizeFontConsole = (int)InputBlock.FontSize;

            settings.NameFontEditor = textEditor.FontFamily.ToString();
            settings.SizeFontEditor = (int)textEditor.FontSize;
            settings.ShowLineNumbers = textEditor.ShowLineNumbers;

            using(FileStream fs = new FileStream(FileNameSettings, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, settings);
            }
        }

        private void LoadSettings()
        {
            if(File.Exists(FileNameSettings))
            {
                using (FileStream fs = new FileStream(FileNameSettings, FileMode.OpenOrCreate))
                {
                    Settings deserilizeSettings = (Settings)formatter.Deserialize(fs);
                    settings = deserilizeSettings;
                }
                Setup();
            }
        }
    }

    public class ConsoleContent : INotifyPropertyChanged
    {
        private static string consoleInput = string.Empty;
        private static Process cmd = new Process();
        private static ObservableCollection<string> consoleOutput = new ObservableCollection<string>();
        public string WorkPath = string.Empty;

        public void Start(string part)
        {
            WorkPath = part + ">";

            cmd.StartInfo.FileName = "lang.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.WorkingDirectory = part;

            cmd.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            cmd.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            cmd.ErrorDataReceived += Cmd_ErrorDataReceived;
            cmd.OutputDataReceived += Cmd_OutputDataReceived;

            cmd.Start();

            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
        }

        private void Cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            WorkPath = ">";
            Application.Current.Dispatcher.Invoke(() => consoleOutput.Add(e.Data));
        }

        private void Cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            WorkPath = ">";
            Application.Current.Dispatcher.Invoke(() => consoleOutput.Add(e.Data));
        }

        public string ConsoleInput
        {
            get
            {
                return consoleInput;
            }
            set
            {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ObservableCollection<string> ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }
        
        public void Finish()
        {
        	if(!cmd.HasExited)
        	{
        		cmd.Kill();
        	}
        }

        public void RunCommand()
        {
        	cmd.StandardInput.WriteLine(ConsoleInput);
        	
        	if(string.Compare(ConsoleInput, "clear") == 0)
        	{
        		ConsoleOutput.Clear();
        	}
        	
        	ConsoleInput = String.Empty;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}