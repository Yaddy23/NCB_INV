using System.Configuration;

namespace NCB_INV
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        public static void EncryptConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection("connectionStrings");

            if (section != null && !section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                config.Save();
            }
        }

        [STAThread]
        static void Main()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            ApplicationConfiguration.Initialize();
            using (Login log = new Login())
            {
                if (log.ShowDialog() == DialogResult.OK)
                {
                    EncryptConfig();
                    Application.Run(new ImportBook());
                }
            }
        }
    }
}