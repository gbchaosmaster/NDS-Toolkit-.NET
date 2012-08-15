using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace NDSToolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        long Length1 = 0, Length2 = 0;

        //File Dialogs
        OpenFileDialog openFileOne = new OpenFileDialog();
        OpenFileDialog openFileTwo = new OpenFileDialog();

        //Code types
        string D2 = "D2000000 00000000";
        string DC = "DC000000 ", C0 = "C0000000 ";
        string D4 = "D4000000 ", D5 = "D5000000 ";
        string D6 = "D6000000 ", D7 = "D7000000 ";
        string D8 = "D8000000 "; 

        public MainWindow()
        {
            InitializeComponent();
        }

        #region MenuItems
        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            new AboutBox().Show();
        }

        private void CheatMenu_Click(object sender, RoutedEventArgs e)
        {
            new CheatDownload().Show();
        }
        #endregion

        #region GlobalFunctions
        private bool Valid_Code(string code, string pattern)
        {
            return Regex.IsMatch(code, pattern, RegexOptions.IgnoreCase);
        }

        private bool Verify(string code)
        {
            string[] raw = code.Split(new char[] { '\n' });

            List<string> lines = new List<string>();
            foreach (string curline in raw)
            {
                string line = curline.Trim();

                if (!String.IsNullOrEmpty(line) && !line.StartsWith(":"))
                    lines.Add(line);
            }

            // This is really, really ugly. But it gets the job done,
            // removing the rest of the whitespace.
            string final = String.Join(
                "\n", lines
            ).Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

            return final.Length % 16 == 0 && Valid_Code(final, @"[0-9a-fA-F]+");
        }
        #endregion

        #region GlobalEvents
        private void HexOnly_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !(e.Key >= Key.A && e.Key <= Key.F ||
                          e.Key >= Key.D0 && e.Key <= Key.D9 ||
                          e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
        }

        private void DecOnly_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 ||
                          e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
        }
        #endregion

        #region ButtonGenerator
        ButtonGenerator BTN = new ButtonGenerator();
        private void ButtonGen_Click(object sender, RoutedEventArgs e)
        {
            int GBATotal = 0, NDSTotal = 0;
            bool GBA = false, NDS = false;
         
            if (chkA.IsChecked == true)
                GBATotal |= 0x0001;
            if (chkB.IsChecked == true)
                GBATotal |= 0x0002;
            if (chkS.IsChecked == true)
                GBATotal |= 0x0004;
            if (chkT.IsChecked == true)
                GBATotal |= 0x0008;
            if (chkI.IsChecked == true)
                GBATotal |= 0x0010;
            if (chkE.IsChecked == true)
                GBATotal |= 0x0020;
            if (chkU.IsChecked == true)
                GBATotal |= 0x0040;
            if (chkD.IsChecked == true)
                GBATotal |= 0x0080;
            if (chkR.IsChecked == true)
                GBATotal |= 0x0100;
            if (chkL.IsChecked == true)
                GBATotal |= 0x0200;
            if (chkX.IsChecked == true)
                NDSTotal |= 0x0400;
            if (chkY.IsChecked == true)
                NDSTotal |= 0x0800;
            if (chkG.IsChecked == true)
                NDSTotal |= 0x2000;
            if (chkF.IsChecked == true)
                NDSTotal |= 0x8000;
            
            BTN.ButtonCode(GBATotal, NDSTotal, GBA, NDS,ButtonInput, ButtonOutput, NDSTst, GBATst);
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement elementOne in grid1.Children)
            {
                if (elementOne is TextBox)
                {
                    TextBox buttonCodeIO = (TextBox)elementOne;
                    buttonCodeIO.Clear();
                }
                if (elementOne is GroupBox)
                {
                    foreach (UIElement elementTwo in chkGrid.Children)
                    {
                        if (elementTwo is CheckBox)
                        {
                            CheckBox buttonChk = (CheckBox)elementTwo;
                            buttonChk.IsChecked = false;
                        }
                    }
                }
            }
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ButtonOutput.Text);
        }

        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            ButtonInput.Paste();
        }
        #endregion

        #region PointerSearcher
        const string binFilter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*";

        private void FileOne_Click(object sender, RoutedEventArgs e)
        {
            openFileOne.Filter = binFilter;

            if ((openFileOne.ShowDialog() == true) && (openFileOne.OpenFile() != null))
            {
                FileOneRead.Text = Path.GetFileName(openFileOne.FileName);
                Length1 = openFileOne.OpenFile().Length;
            }

            string ReadToAddy1 = FileOneRead.Text.Substring(FileOneRead.Text.IndexOf(".bin") - 8, 8);
            AddressOne.Text = Valid_Code(ReadToAddy1, "[0-F]{8}") ? ReadToAddy1 : "";
        }

        private void FileTwo_Click(object sender, RoutedEventArgs e)
        {
            openFileTwo.Filter = binFilter;

            if ((openFileTwo.ShowDialog() == true) && (openFileTwo.OpenFile() != null))
            {
                FileTwoRead.Text = Path.GetFileName(openFileTwo.FileName);
                Length2 = openFileTwo.OpenFile().Length;
            }

            string ReadToAddy2 = FileTwoRead.Text.Substring(FileTwoRead.Text.IndexOf(".bin") - 8, 8);
            AddressTwo.Text = Valid_Code(ReadToAddy2, "[0-F]{8}") ? ReadToAddy2 : "";
        }

        private void PointerSearch_Click(object sender, RoutedEventArgs e)
        {
            //Check if both files have been opened
            if (Length1 == 0 || Length2 == 0 || Length1 != Length2)
            {
                MessageBox.Show(this, "Please upload two files of the same size.", 
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //compare the length of the files (in bytes/size)
            if (openFileOne.OpenFile().Length != openFileTwo.OpenFile().Length)
            {
                MessageBox.Show(this, "The files you uploaded are different sizes!",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Check if the addresses are the same
            if (AddressOne.Text == AddressTwo.Text)
            {
                MessageBox.Show(this, "Please input two different addresses.", 
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Check if the user entered an address with less than 7 chars
            if (AddressOne.Text.Length < 7 || AddressTwo.Text.Length < 7)
            {
                MessageBox.Show(this, "Please check to see if both addresses are 7-8 characters long.", 
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PtARDS.Clear();
            ptrResults.Items.Clear();
            StringBuilder ptrCode = new StringBuilder();
            int hc = int.Parse(HexValue.Text, NumberStyles.AllowHexSpecifier);
            int MaxOffsetTest = int.Parse(MaxOffset.Text, NumberStyles.HexNumber);
            int DesiredOffset = Positive.IsChecked == true ? Math.Abs(MaxOffsetTest) : -Math.Abs(MaxOffsetTest);

            //read the contents of the files
            BinaryReader File1 = new BinaryReader(openFileOne.OpenFile());
            BinaryReader File2 = new BinaryReader(openFileTwo.OpenFile());

            //parse the text to an integer
            int Addy1 = int.Parse(AddressOne.Text, NumberStyles.HexNumber);
            int Addy2 = int.Parse(AddressTwo.Text, NumberStyles.HexNumber);

            //loop through the file, from 0 to file size in bytes (32-bit aligned)
            for (int i = 0; i < Length1; i += 4)
            {
                int Offset1 = Addy1 - File1.ReadInt32();
                int Offset2 = Addy2 - File2.ReadInt32();

                /*Based on the checkbox that was checked:
                 *when positive is checked, it'll check to see if the offset is less than or
                 *equal to the positive max offset the user entered and it will then check if the
                 *offset is actually positive (greater than 0).
                 *
                 *when negative is checked, it'll check to see if the offset is greater than or
                 *equal to the negative max offset the user entered and it will then check if the
                 *offset is actually negative (less than 0)
                 */

                if ((Offset1 == Offset2) &&
                    ((Offset1 <= DesiredOffset && Offset1 > 0) ||
                     (Offset1 >= DesiredOffset && Offset1 < 0)))
                    ptrResults.Items.Add(String.Format("0x{0:X8} : 0x{1:X8} :: 0x{2:X8}",
                                                   i + 0x02000000, Addy1 - Offset1, Offset1));
            }

            if (ptrResults.Items.Count == 0)
            {
                MessageBox.Show(this, "No results were found.", "No Results",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
  
            /*Smallest = smallest value, 
             *SmallestAddy1 = smallest address 1
             */

            int SmallestIndex = 0;
            int SmallestOffset = 0;

            for (int i = 0; i < ptrResults.Items.Count; ++i)
            {
                string Address = ptrResults.Items[i].ToString();

                //make sure we didn't grab a blank
                if (Address.Trim() == "")
                    continue;

                //parse the offset value
                int SmallCheck = int.Parse(Address.Substring(29, 8),
                                    NumberStyles.AllowHexSpecifier);

                //if the result is the best so far, save some data about it
                if (SmallestOffset == 0 || SmallCheck < SmallestOffset)
                {
                    SmallestIndex = i;
                    SmallestOffset = SmallCheck;
                }
            }

            //select the smallest one and focus on the listbox
            //this selection change will trigger the event that gets the pointer code
            ptrResults.SelectedIndex = SmallestIndex;
            ptrResults.Focus();
        }

        private void ptrResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ptrLine = ptrResults.SelectedItem.ToString();
            setPtrCode(ptrLine.Substring(3, 7), ptrLine.Substring(29, 8));
        }

        private void setPtrCode(string addressStr, string offsetStr)
        {
            StringBuilder ptrCode = new StringBuilder();

            int hc = int.Parse(HexValue.Text, NumberStyles.AllowHexSpecifier);
            int address = int.Parse(addressStr, NumberStyles.AllowHexSpecifier);
            int offset = int.Parse(offsetStr, NumberStyles.AllowHexSpecifier);

            ptrCode.Append(String.Format("6{0:X7} 00000000\nB{0:X7} 00000000\n", address));
            ptrCode.Append(
                offset < 0
                //negative offset
                ? String.Format(
                "DC000000 {0:X8}\n" +
                "{1:X}8000000 {2:X8}\n" +
                "D2000000 00000000",
                offset - 0x08000000,
                getCodeType(), hc
                )
                //positive offset
                : String.Format(
                "{0:X}{1:X7} {2:X8}\n" +
                "D2000000 00000000",
                getCodeType(),
                offset, hc
                )
            );
            
            PtARDS.Text = ptrCode.ToString();
        }

        /** Pointer Searcher Helper Functions **/

        private string getCodeType()
        {
            int hc = int.Parse(HexValue.Text, NumberStyles.AllowHexSpecifier);

            //get the code type based on the Hex Value input
            int ct = hc >= 0 && hc <= 255
                     ? 2
                     : hc > 255 && hc <= 65535
                       ? 1
                       : 0;

            return ct.ToString("X");
        }
        #endregion

        #region CodePorter
        private void CodeOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //elixirdream's requested feature 
            CodeOutput.IsReadOnly = String.IsNullOrEmpty(CodeOutput.Text);
        }

        private void CodePort_Click(object sender, RoutedEventArgs e)
        {
            string Ported = "";
            CodeOutput.Clear(); 
            StringBuilder cb = new StringBuilder();

            //no input? alert the user.
            if (String.IsNullOrEmpty(CodeInput.Text))
            {
                MessageBox.Show(this, "There is no code input.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            //no offset? alert the user.
            if (String.IsNullOrEmpty(CodeOffset.Text))
            {
                MessageBox.Show(this, "No offset has been specified.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (string line in CodeInput.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (Valid_Code(line, @"[0-F]{8}\s[0-F]{8}") &&
                    !Valid_Code(line, @"[CD][0-2C4-5]0{6}\s[0-F]{8}|[3-A]4[0-F]{6}\s[0-F]{8}|927[0-F]{5}\s[0-F]{8}"))
                {
                    string AddyOnly = line.Substring(0, 8);
                    string ValyOnly = line.Substring(9, 8);
                    int AddyConvert = int.Parse(AddyOnly, NumberStyles.AllowHexSpecifier);
                    int ValyConvert = int.Parse(ValyOnly, NumberStyles.AllowHexSpecifier);
                    int OffyConvert = int.Parse(CodeOffset.Text, NumberStyles.AllowHexSpecifier);

                    if (Valid_Code(line, @"D[36-B]0{6}\s[0-F]{8}")) //Dx Lines
                    {
                        Ported = CodeAdd.IsChecked == true ? (ValyConvert + OffyConvert).ToString("X8") :
                            (ValyConvert - OffyConvert).ToString("X8");
                        cb.Append(AddyOnly.ToUpper() + " " + Ported.ToUpper() + '\n');
                    }
                    else //non-Dx lines
                    {
                        Ported = CodeAdd.IsChecked == true ? (AddyConvert + OffyConvert).ToString("X8") :
                            (AddyConvert - OffyConvert).ToString("X8");
                        cb.Append(Ported.ToUpper() + " " + ValyOnly.ToUpper() + '\n');
                    }
                }
                else cb.Append(line + '\n'); //everything else
            }
            CodeOutput.Text = cb.ToString();
        }

        private void CodeCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CodeOutput.Text);
        }

        private void CodePaste_Click(object sender, RoutedEventArgs e)
        {
            CodeInput.Paste();
        }

        private void CodeClear_Click(object sender, RoutedEventArgs e)
        {
            CodeInput.Clear();
            CodeOutput.Clear();
            CodeOffset.Clear();
        }
        #endregion

        #region PatchCodeBuilder
        private void PatchBuild_Click(object sender, RoutedEventArgs e)
        {
            PatchOutput.Clear();
            PatchInput.Text = PatchInput.Text.Trim();
            StringBuilder pb = new StringBuilder();
            String[] PatchCode = PatchInput.Text.Split(new String[] { Environment.NewLine }, StringSplitOptions.None);

            int CodeOffset = PatchInput.LineCount; //get the number of lines
            int CodeCheck = CodeOffset;
            CodeOffset *= 4; //multiply by 4 to get the offset

            string CodeAddress = PatchCode[0].Substring(1, 8);
            pb.Append("E" + CodeAddress + CodeOffset.ToString("X8") + '\n');

            for (int y = 0; y < PatchCode.Length; y++)
            {
                string CodeValues = PatchCode[y].Substring(9, 8);

                if (y % 2 == 0)
                    pb.Append(CodeValues + " ");
                else
                    pb.Append(CodeValues + '\n');
            }

            if (CodeCheck % 2 != 0)
                pb.Append("00000000");

            PatchOutput.Text = pb.ToString();
        }

        private void PatchPaste_Click(object sender, RoutedEventArgs e)
        {
            PatchInput.Paste();
        }

        private void PatchCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(PatchOutput.Text);
        }

        private void PatchClear_Click(object sender, RoutedEventArgs e)
        {
            PatchInput.Clear();
            PatchOutput.Clear();
        }
        #endregion

        #region LoopCodeGenerator
        private void LoopBase_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Loop Code Generator 
            if (LoopBase.Text.Length == 8) 
            {
                LoopBase.Text += " ";
                LoopBase.SelectionStart = 10;
            }
        }

        private void LoopGen_Click(object sender, RoutedEventArgs e)
        {
            bool run = true;
            string check = "";

            string TempCount = Convert.ToInt32(LoopCount.Text).ToString("X8");
            int HalfOffset = int.Parse(LoopOffset.Text, NumberStyles.AllowHexSpecifier);
            string FullOffset = HalfOffset.ToString("X8");

            //get the correct block offset by subtracting 1 and convert it to hex
            int nBlock = int.Parse(TempCount, NumberStyles.AllowHexSpecifier); nBlock -= 1;
            string ConvCount = nBlock.ToString("X8");

            //check offset
            int TempOffset = int.Parse(LoopOffset.Text, NumberStyles.AllowHexSpecifier);

            if (TempOffset == 1) check = D8;
            else if (TempOffset == 2) check = D7;
            else if (TempOffset == 4) check = D6;
            else run = false;

            //Did the user enter a full code?
            if (LoopBase.Text.Length != 17)
            {
                LoopOutput.Clear();
                MessageBox.Show(this, "Please enter a full code (XXXXXXXX YYYYYYYY).",
                    "Base Code Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (LoopBase.Text.Length == 17 && run)
            {
                string BaseAddy = LoopBase.Text.Substring(0, 8); //grab the address
                string BaseValy = LoopBase.Text.Substring(9, 8); //grab the value

                if (LoopBase.Text[0] == '0')
                {
                    LoopOutput.Text = D5 + BaseValy + '\n' + C0 + ConvCount + '\n' + check + BaseAddy + '\n';

                    if (!String.IsNullOrEmpty(LoopInc.Text)) //if there's text in the increment textbox, add the value increment.
                    {
                        int HalfInc = int.Parse(LoopInc.Text, NumberStyles.AllowHexSpecifier);
                        string FullInc = HalfInc.ToString("X8");
                        LoopOutput.Text += D4 + FullInc + '\n' + D2;
                    }
                    else LoopOutput.Text += D2; //else, don't add the value increment.
                }
                else
                {
                    LoopOutput.Clear();
                    MessageBox.Show(this, "Please check to see if your base code starts with a '0' and if you've entered " + 
                                    "an offset increment of 1, 2, or 4.", "Value Increment Error", MessageBoxButton.OK, 
                                                                                                MessageBoxImage.Error);
                }
            }
            else if (LoopBase.Text.Length == 17 && !run)
            {
                if (LoopBase.Text[0] >= '0' && LoopBase.Text[0] < '3')
                    LoopOutput.Text = C0 + ConvCount + '\n' + LoopBase.Text + '\n' + DC + FullOffset + '\n' + D2;
                else MessageBox.Show(this, "Invalid Data! Please start your code off with a 0, 1, or 2.", "Data Input Error",
                                                                                 MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoopCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(LoopOutput.Text);
        }

        private void LoopClear_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in loopGrid.Children)
            {
                if (element is TextBox)
                {
                    TextBox txtFields = (TextBox)element;
                    txtFields.Clear();
                }
            }
        }
        #endregion

        #region CodeBeautifier
        private void CodeBeautify_Click(object sender, RoutedEventArgs e)
        {
            // First verify the input, or risk death.
            if (!Verify(txtCodeInput.Text))
            {
                MessageBox.Show(this, "The code input is invalid.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Build the code onto here.
            List<string> processed = new List<string>();

            foreach (string curline in txtCodeInput.Text.Split(new char[] { '\n' }))
            {
                // Trim everything.
                string line = curline.Trim();

                // Skip comments/blank liens if they aren't allowed.
                if ((chkStripComments.IsChecked == true && line.StartsWith(":")) ||
                    (chkStripBlankLines.IsChecked == true && String.IsNullOrEmpty(line)))
                    continue;

                // Add comments and blank lines directly...
                if (line.StartsWith(":") || String.IsNullOrEmpty(line))
                    processed.Add(line);
                // Otherwise it's a line of code...
                else
                {
                    // Enforce the Uppercase Hex option.
                    if (chkUpperHex.IsChecked == true)
                        line = line.ToUpper();

                    // Remove the whitespace.
                    line = line.Replace(" ", "").Replace("\t", "");

                    // Condense all hunks of code into one line.
                    // If processed is empty, or the last line in in processed is
                    // either a blank line or a comment, we need to start a new
                    // code line.
                    if (processed.Count == 0 ||
                        String.IsNullOrEmpty(processed[processed.Count - 1]) ||
                        processed[processed.Count - 1].StartsWith(":"))
                        processed.Add(line);
                    else processed[processed.Count - 1] += line;
                }
            }

            // Build the final result onto here.
            List<string> final = new List<string>();

            foreach (string curline in processed)
            {
                string line = curline;

                if (line.StartsWith(":") || String.IsNullOrEmpty(line))
                    final.Add(line);
                else
                {
                    // If we don't have the right number of characters, uh-oh...
                    // bail out and call it invalid.
                    if (line.Length % 16 != 0)
                    {
                        MessageBox.Show(this, "The code input is invalid.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    while (!String.IsNullOrEmpty(line))
                    {
                        final.Add(line.Substring(0, 16).Insert(8, " "));
                        line = line.Remove(0, 16);
                    }
                }
            }

            txtCodeOutput.Text = String.Join("\n", final);
        }

        private void BeautifyCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtCodeOutput.Text);
        }

        private void BeautifyPaste_Click(object sender, RoutedEventArgs e)
        {
            txtCodeInput.Paste();
        }

        private void BeautifyClear_Click(object sender, RoutedEventArgs e)
        {
            txtCodeInput.Clear();
            txtCodeOutput.Clear();
        }
        #endregion
    }
}
