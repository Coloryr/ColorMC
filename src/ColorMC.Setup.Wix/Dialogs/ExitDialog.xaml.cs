using Caliburn.Micro;
using System.Diagnostics;
using System.IO;
using WixSharp;
using WixSharp.UI.Forms;
using WixSharp.UI.WPF;
using IO = System.IO;

namespace ColorMC.Setup.Wix
{
    /// <summary>
    /// The standard ExitDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro View (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ExitDialog : WpfDialog, IWpfDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExitDialog"/> class.
        /// </summary>
        public ExitDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            UpdateTitles(ManagedFormHost.Runtime.Session);

            ViewModelBinder.Bind(new ExitDialogModel { Host = ManagedFormHost }, this, null);
        }

        /// <summary>
        /// Updates the titles of the dialog depending on the success of the installation action.
        /// </summary>
        /// <param name="session">The session.</param>
        public void UpdateTitles(ISession session)
        {
            this.Localize();
        }
    }

    /// <summary>
    /// ViewModel for standard ExitDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro ViewModel (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="Caliburn.Micro.Screen" />
    internal class ExitDialogModel : Caliburn.Micro.Screen
    {
        public ManagedForm Host { get; set; }
        ISession session => Host?.Runtime.Session;
        IManagedUIShell shell => Host?.Shell;

        public void GoExit()
            => shell?.Exit();

        public void ViewLog()
        {
            if (shell != null)
                try
                {
                    string logFile = session.LogFile;

                    if (logFile.IsEmpty())
                    {
                        string wixSharpDir = Path.GetTempPath().PathCombine("WixSharp");

                        if (!Directory.Exists(wixSharpDir))
                            Directory.CreateDirectory(wixSharpDir);

                        logFile = wixSharpDir.PathCombine(Host.Runtime.ProductName + ".log");
                        IO.File.WriteAllText(logFile, shell.Log);
                    }
                    Process.Start("notepad.exe", logFile);
                }
                catch
                {
                    // Catch all, we don't want the installer to crash in an
                    // attempt to view the log.
                }
        }
    }
}