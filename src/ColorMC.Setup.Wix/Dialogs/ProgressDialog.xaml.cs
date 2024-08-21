using Caliburn.Micro;
using System.Security.Principal;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.UI.Forms;
using WixSharp.UI.WPF;
using WixToolset.Dtf.WindowsInstaller;

namespace ColorMC.Setup.Wix
{
    /// <summary>
    /// The standard ProgressDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro View (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ProgressDialog : WpfDialog, IWpfDialog, IProgressDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog" /> class.
        /// </summary>
        public ProgressDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is invoked by WixSharp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            UpdateTitles(ManagedFormHost.Runtime.Session);

            model = new ProgressDialogModel { Host = ManagedFormHost };
            ViewModelBinder.Bind(model, this, null);

            model.StartExecute();
        }

        /// <summary>
        /// Updates the titles of the dialog depending on what type of installation action MSI is performing.
        /// </summary>
        /// <param name="session">The session.</param>
        public void UpdateTitles(ISession session)
        {
            if (session.IsUninstalling())
            {
                DialogTitleLabel.Text = "ColorMC Uninstall";
            }
            else if (session.IsRepairing())
            {
                DialogTitleLabel.Text = "ColorMC Repair";
            }
            else if (session.IsInstalling())
            {
                DialogTitleLabel.Text = "ColorMC Installer";
            }

            // `Localize` resolves [...] titles and descriptions into the localized strings stored in MSI resources tables
            this.Localize();
        }

        ProgressDialogModel model;

        /// <summary>
        /// Processes information and progress messages sent to the user interface.
        /// <para> This method directly mapped to the
        /// <see cref="T:Microsoft.Deployment.WindowsInstaller.IEmbeddedUI.ProcessMessage" />.</para>
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageRecord">The message record.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="defaultButton">The default button.</param>
        /// <returns></returns>
        public override MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
            => model?.ProcessMessage(messageType, messageRecord, CurrentStatus.Text) ?? MessageResult.None;

        /// <summary>
        /// Called when MSI execution is complete.
        /// </summary>
        public override void OnExecuteComplete()
            => model?.OnExecuteComplete();

        /// <summary>
        /// Called when MSI execution progress is changed.
        /// </summary>
        /// <param name="progressPercentage">The progress percentage.</param>
        public override void OnProgress(int progressPercentage)
        {
            if (model != null)
                model.ProgressValue = progressPercentage;
        }
    }

    /// <summary>
    /// ViewModel for standard ProgressDialog.
    /// <para>Follows the design of the canonical Caliburn.Micro ViewModel (MVVM).</para>
    /// <para>See https://caliburnmicro.com/documentation/cheat-sheet</para>
    /// </summary>
    /// <seealso cref="Caliburn.Micro.Screen" />
    internal class ProgressDialogModel : Caliburn.Micro.Screen
    {
        public ManagedForm Host;

        ISession session => Host?.Runtime.Session;
        IManagedUIShell shell => Host?.Shell;

        public bool UacPromptIsVisible => (!WindowsIdentity.GetCurrent().IsAdmin() && Uac.IsEnabled() && !uacPromptActioned);

        public string CurrentAction { get => currentAction; set { currentAction = value; base.NotifyOfPropertyChange(() => CurrentAction); } }
        public int ProgressValue { get => progressValue; set { progressValue = value; base.NotifyOfPropertyChange(() => ProgressValue); } }

        bool uacPromptActioned = false;
        string currentAction;
        int progressValue;

        public string UacPrompt
        {
            get
            {
                if (Uac.IsEnabled())
                {
                    return
                        "Please agree to administrator privileges to continue installation\n" +
                        "It is usually located in the taskbar";
                }
                else
                    return null;
            }
        }

        public void StartExecute()
            => shell?.StartExecute();

        public void Cancel()
        {
            if (shell.IsDemoMode)
                shell.GoNext();
            else
                shell.Cancel();
        }

        public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, string currentStatus)
        {
            switch (messageType)
            {
                case InstallMessage.InstallStart:
                case InstallMessage.InstallEnd:
                    {
                        uacPromptActioned = true;
                        base.NotifyOfPropertyChange(() => UacPromptIsVisible);
                    }
                    break;

                case InstallMessage.ActionStart:
                    {
                        try
                        {
                            if (messageRecord.FieldCount >= 3)
                                CurrentAction = messageRecord[2].ToString();
                            else
                                CurrentAction = null;
                        }
                        catch
                        {
                            //Catch all, we don't want the installer to crash in an attempt to process message.
                        }
                    }
                    break;
            }
            return MessageResult.OK;
        }

        public void OnExecuteComplete()
        {
            CurrentAction = null;
            shell?.GoNext();
        }
    }
}