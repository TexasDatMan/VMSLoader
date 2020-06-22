using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

using InjectionLibrary;

namespace VMS
{
    internal class Inject
    {
        public static void inj()
        {
            if (InjectForm.Process == "CSGO")
            {
                CSINJ();
            }
        }

        public static void Download()
        {
            InjectForm.status = "Downloading";

                HttpWebRequest.DefaultWebProxy = new WebProxy();
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                client.DownloadDataCompleted += DownloadDataCompleted;
                client.DownloadDataAsync(new Uri(InjectForm.url));

        }

        public static void next()
        {
            CSINJ();
        }

        public static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            InjectForm.status = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
            InjectForm.value = int.Parse(Math.Truncate(percentage).ToString());
        }

        public static void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            InjectForm.dll = e.Result;
            InjectForm.status = "Download Finished";
            inj();
        }

        public static void CSINJ()
        {
            while (System.Diagnostics.Process.GetProcessesByName(InjectForm.Process).Length == 0) //if csgo isnt started
            {
                InjectForm.status = "Scanning for CSGO";
                InjectForm.value = 1;
                Thread.Sleep(500); //sleeps for .5 seconds
            }
            
            bool Enginedll_Found = false; //initialize engine_found with false
            bool Serverdll_Found = false;
            

            do
            {
                InjectForm.status = "Scanning for Engine";
                Process[] CheckModules = System.Diagnostics.Process.GetProcessesByName(InjectForm.Process);

                foreach (ProcessModule m in CheckModules[0].Modules)
                {
                    if (m.ModuleName == "engine.dll") //this is to check if engine.dll is loaded
                    {
                        InjectForm.value = 50;
                        Enginedll_Found = true;
                    }
                }
            } while (Enginedll_Found == false); //loop while not loaded
            do
            {
                InjectForm.status = "Scanning for Server Browser ";
                Process[] CheckModules = System.Diagnostics.Process.GetProcessesByName(InjectForm.Process);

                foreach (ProcessModule m in CheckModules[0].Modules)
                {
                    if (m.ModuleName == "serverbrowser.dll") //this is to check if engine.dll is loaded
                    {
                        InjectForm.value = 75;
                        Serverdll_Found = true;
                    }
                }
            } while (Serverdll_Found == false);

            if (Enginedll_Found == true && Serverdll_Found == true) //if its loaded
            {
                Thread.Sleep(10000);
                
                var injectionMethod = InjectionMethod.Create(InjectionMethodType.Standard);
                IntPtr zero = IntPtr.Zero;
                using (JLibrary.PortableExecutable.PortableExecutable executable = new JLibrary.PortableExecutable.PortableExecutable(InjectForm.dll))
                {
                    InjectForm.value = 100;
                    zero = injectionMethod.Inject(executable, Process.GetProcessesByName("csgo").FirstOrDefault().Id);
                }
                if (zero != IntPtr.Zero)
                {
                    //BAIL HERE - ERROR
                }
                else if (injectionMethod.GetLastError() != null)
                {
                    //ERROR OCCURED
                    System.Windows.Forms.MessageBox.Show(injectionMethod.GetLastError().Message);
                }

            }
            Application.Exit();
        }
    }
}