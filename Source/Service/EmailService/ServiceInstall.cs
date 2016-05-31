using System.ComponentModel;
using System.Configuration.Install;


namespace EmailService
{
    [RunInstaller(true)]
    public partial class ServiceInstall : Installer
    {
        public ServiceInstall()
        {
            InitializeComponent();
        }
    }
}
