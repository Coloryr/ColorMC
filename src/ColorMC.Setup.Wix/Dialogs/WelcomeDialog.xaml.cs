using Caliburn.Micro;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using WixSharp;
using WixSharp.UI.Forms;
using WixSharp.UI.WPF;

namespace ColorMC.Setup.Wix
{
    /// <summary>
    /// The standard WelcomeDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro View (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="WpfDialog" />
    /// <seealso cref="IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class WelcomeDialog : WpfDialog, IWpfDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeDialog" /> class.
        /// </summary>
        public WelcomeDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            ViewModelBinder.Bind(new WelcomeDialogModel { Host = ManagedFormHost }, this, null);
        }
    }

    /// <summary>
    /// ViewModel for standard WelcomeDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro ViewModel (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="Caliburn.Micro.Screen" />
    internal class WelcomeDialogModel : Caliburn.Micro.Screen
    {
        public ManagedForm Host;
        IManagedUIShell shell => Host?.Shell;
        ISession session => Host?.Runtime.Session;

        public BitmapImage Banner => session?.GetResourceBitmap("WixSharpUI_Bmp_Banner").ToImageSource();

        string installDirProperty => session?.Property("WixSharp_UI_INSTALLDIR");

        public bool CreateShortcut
        {
            get
            {
                if (Host != null)
                {
                    return session["INSTALLDESKTOPSHORTCUT"] == "yes";
                }
                else
                    return false;
            }
            set
            {
                session["INSTALLDESKTOPSHORTCUT"] = value ? "yes" : "no";
            }
        }

        public string InstallDirPath
        {
            get
            {
                if (Host != null)
                {
                    string installDirPropertyValue = session.Property(installDirProperty);

                    if (installDirPropertyValue.IsEmpty())
                    {
                        // We are executed before any of the MSI actions are invoked so the INSTALLDIR (if set to absolute path)
                        // is not resolved yet. So we need to do it manually
                        var installDir = session.GetDirectoryPath(installDirProperty);

                        if (installDir == "ABSOLUTEPATH")
                            installDir = session.Property("INSTALLDIR_ABSOLUTEPATH");

                        return installDir;
                    }
                    else
                    {
                        //INSTALLDIR set either from the command line or by one of the early setup events (e.g. UILoaded)
                        return installDirPropertyValue;
                    }
                }
                else
                    return null;
            }
            set
            {
                session[installDirProperty] = value;
                NotifyOfPropertyChange(() => InstallDirPath);
            }
        }

        public void ChangeInstallDir()
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = InstallDirPath })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var path = dialog.SelectedPath;
                    if (!path.EndsWith(@"\ColorMC"))
                    {
                        InstallDirPath = path + @"\ColorMC";
                    }
                    else
                    {
                        InstallDirPath = path;
                    }
                }
            }
        }

        public bool CanGoPrev => false;

        public void GoNext()
            => shell?.GoNext();

        public void Cancel()
            => shell?.Cancel();
    }
}