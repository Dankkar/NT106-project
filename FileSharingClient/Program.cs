using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSharingClient
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainContext());
        }

        public class MainContext : ApplicationContext
        {
            public MainContext()
            {
                ShowLoginForm();
            }

            private void ShowLoginForm()
            {
                var loginForm = new Login();
                loginForm.FormClosed += (s, e) =>
                {
                    if (loginForm.Tag as string == "register")
                    {
                        ShowRegisterForm();
                    }
                    else
                    {
                        ExitThread();
                    }
                };
                loginForm.Show();
            }
            
            private void ShowRegisterForm()
            {
                var registerForm = new Register();
                registerForm.FormClosed += (s, e) =>
                {
                    if (registerForm.Tag as string == "login")
                    {
                        ShowLoginForm();
                    }
                    else
                    {
                        ExitThread();
                    }
                };
                registerForm.Show();
            }
        }
    }
}
