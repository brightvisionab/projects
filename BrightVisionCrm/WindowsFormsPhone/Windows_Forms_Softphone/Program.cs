using System;
using System.Windows.Forms;

namespace Windows_Forms_Softphone
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PhoneMain());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ExceptionHandling(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
                ExceptionHandling(e.ExceptionObject as Exception);
            else
            {
                MessageBox.Show("Unknown unhandledException " + e);
            }

        }

        static void ExceptionHandling(Exception exception)
        {
            if (exception != null)
            {
                MessageBox.Show(exception.Message);
                MessageBox.Show(exception.StackTrace);

                if (exception.InnerException != null)
                    MessageBox.Show(exception.InnerException.Message);
            }
        }
    }
}
