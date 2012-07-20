using System;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NDS_Toolkit
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private byte[] usrcheatDownloaded; //set the current size (in bytes)
        SaveFileDialog saveUsrcheat = new SaveFileDialog();
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        private void usrcheatDownload(string usrcheat) //connects and tries to download the usrcheat
        {
            progressBar1.Value = 0; //empty the progress bar
            usrcheatDownloaded = new byte[0];

            try
            {
                //Fetch the usrcheat from Syntechx.com
                WebRequest req = WebRequest.Create(usrcheat);
                WebResponse res = req.GetResponse();
                Stream stream = res.GetResponseStream();

                byte[] buffer = new byte[1024];

                //Get the length, in bytes, of content sent by the client.
                //usrcheatLength = MaxLength of the usrcheat file
                int usrcheatLength = (int)res.ContentLength;

                //set the progressbar's max value to the usrcheatLength
                progressBar1.Maximum = usrcheatLength;

                if (progressBar1.Value != progressBar1.Maximum)
                    Title = "Downloading...";

                //Time to download...
                MemoryStream memStream = new MemoryStream();
                UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar1.SetValue);

                while (true)
                {
                    //Try to read the data
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0) //Are we finished downloading? 
                    {
                        progressBar1.Value = progressBar1.Maximum;
                        break;
                    }
                    else //Eh, I guess not...
                    {
                        //Write the downloaded data to the current stream
                        memStream.Write(buffer, 0, bytesRead);

                        //Update the progress bar
                        if (progressBar1.Value + bytesRead <= progressBar1.Maximum)
                        {
                            progressBar1.Value += bytesRead;
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, 
                                new object[] { ProgressBar.ValueProperty, progressBar1.Value });
                        }
                    }
                }

                usrcheatDownloaded = memStream.ToArray(); //Convert the downloaded stream to a byte array
                stream.Close(); //Closes the current stream and releases everything associated with it
                memStream.Close(); //Closes the stream for reading and writing.
            }
            catch (System.Exception) //No internet connection? 
            {
                MessageBox.Show("A problem occurred while trying to download the latest usrcheat. Please check your internet settings and retry.",
                    "Download Incomplete", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadLatest_Click(object sender, RoutedEventArgs e)
        {
            string usrcheat = "http://syntechx.com/downloads/usrcheat.rar";
            usrcheatDownload(usrcheat);

            if (usrcheatDownloaded != null && usrcheatDownloaded.Length != 0)
            {
                string urlName = usrcheat;
                urlName = urlName.Substring(urlName.LastIndexOf('/') + 1); //get the file name
                saveUsrcheat.FileName = urlName; //sets our file name to the usrcheat name
            }

            saveUsrcheat.Filter = "WinRAR archive |*.rar";

            if (progressBar1.Value == progressBar1.Maximum && usrcheatDownloaded != null && saveUsrcheat.ShowDialog() == true)
            {
                //Write the bytes to the newly created file
                FileStream newFile = new FileStream(saveUsrcheat.FileName, FileMode.Create);
                newFile.Write(usrcheatDownloaded, 0, usrcheatDownloaded.Length);
                newFile.Close();
                MessageBox.Show("Successfully downloaded the latest usrcheat.", "Download Complete");
                this.Close();
            }
        }
    }
}
