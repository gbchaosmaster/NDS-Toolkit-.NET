using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace NDSToolkit
{
    /// <summary>
    /// Interaction logic for CheatDownload.xaml
    /// </summary>
    public partial class CheatDownload : Window
    {
        public CheatDownload()
        {
            InitializeComponent();
        }

        const string usrcheat = "http://syntechx.com/downloads/nds_cheat_database/usrcheat.rar";
        private byte[] usrcheatDownloaded; //set the current size (in bytes)
        SaveFileDialog saveUsrcheat = new SaveFileDialog();
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        private void usrcheatDownload(string usrcheat) //connects and tries to download the usrcheat
        {
            Title = "Downloading...";
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

            Title = "Download Complete";
        }

        private void DownloadLatest_Click(object sender, RoutedEventArgs e)
        {
            //set our file name to the usrcheat name
            saveUsrcheat.FileName = usrcheat.Substring(usrcheat.LastIndexOf('/') + 1);
            //filter file types to save as
            saveUsrcheat.Filter = "RAR Archive (*.rar)|*.rar|All Files (*.*)|*.*";

            //show them the dialog and only continue if they choose a location
            if (saveUsrcheat.ShowDialog() == true)
            {
                usrcheatDownload(usrcheat);
                if (usrcheatDownloaded != null)
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
}
