﻿using System;
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
        //File Dialogs
        OpenFileDialog openFileOne = new OpenFileDialog();
        OpenFileDialog openFileTwo = new OpenFileDialog();

        //Code types
        const string D2 = "D2000000 00000000";
        const string DC = "DC000000 ", C0 = "C0000000 ";
        const string D4 = "D4000000 ", D5 = "D5000000 ";
        const string D6 = "D6000000 ", D7 = "D7000000 ";
        const string D8 = "D8000000 ";

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
        private int HexStrToInt(string str)
        {
            return int.Parse(str, NumberStyles.AllowHexSpecifier);
        }

        private bool Valid(string str, string re)
        {
            return Regex.IsMatch(str, re, RegexOptions.IgnoreCase);
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

            //This is really, really ugly. But it gets the job done,
            //removing the rest of the whitespace.
            string final = String.Join(
                "\n", lines
            ).Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

            return final.Length % 16 == 0 && Valid(final, @"[0-9A-F]+");
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

            BTN.ButtonCode(GBATotal, NDSTotal, GBA, NDS, ButtonInput, ButtonOutput, NDSTst, GBATst);
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
                    foreach (UIElement elementTwo in chkGrid.Children)
                        if (elementTwo is CheckBox)
                        {
                            CheckBox buttonChk = (CheckBox)elementTwo;
                            buttonChk.IsChecked = false;
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
        const string binFilter = "Binary Files (*.bin)|*.bin|All Files|*";

        long Length1 = 0, Length2 = 0;

        private void FileOne_Click(object sender, RoutedEventArgs e)
        {
            openFileOne.Filter = binFilter;

            if ((openFileOne.ShowDialog() == true) && (openFileOne.OpenFile() != null))
            {
                FileOneRead.Text = Path.GetFileName(openFileOne.FileName);
                Length1 = openFileOne.OpenFile().Length;
                ParseFileName(FileOneRead.Text, AddressOne);
            }

            ParseFileName(FileOneRead.Text, AddressOne);
        }
        private void FileTwo_Click(object sender, RoutedEventArgs e)
        {
            openFileTwo.Filter = binFilter;

            if ((openFileTwo.ShowDialog() == true) && (openFileTwo.OpenFile() != null))
            {
                FileTwoRead.Text = Path.GetFileName(openFileTwo.FileName);
                Length2 = openFileTwo.OpenFile().Length;
                ParseFileName(FileTwoRead.Text, AddressTwo);
            }

            ParseFileName(FileTwoRead.Text, AddressTwo);
        }
        private void ParseFileName(string filename, TextBox address)
        {
            //Helder's requested Pointer Searcher feature:
            //Auto-fill the address box if the name of the .bin file
            //contains the code's address before ".bin"
            if (filename.Length >= 12)
            {
                string last8 = filename.Substring(filename.Length - 12, 8);
                address.Text = Valid(last8, @"[0-9A-F]{8}") ? last8.ToUpper() : "";
            }
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

            //Compare the length of the files (in bytes/size)
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
            int hc = HexStrToInt(HexValue.Text);
            int MaxOffsetTest = int.Parse(MaxOffset.Text, NumberStyles.HexNumber);
            int DesiredOffset = Positive.IsChecked == true ? Math.Abs(MaxOffsetTest) : -Math.Abs(MaxOffsetTest);

            //Read the contents of the files
            BinaryReader File1 = new BinaryReader(openFileOne.OpenFile());
            BinaryReader File2 = new BinaryReader(openFileTwo.OpenFile());

            //Parse the text to an integer
            int Addy1 = int.Parse(AddressOne.Text, NumberStyles.HexNumber);
            int Addy2 = int.Parse(AddressTwo.Text, NumberStyles.HexNumber);

            //Loop through the file, from 0 to file size in bytes (32-bit aligned)
            for (int i = 0; i < Length1; i += 4)
            {
                int Offset1 = Addy1 - File1.ReadInt32();
                int Offset2 = Addy2 - File2.ReadInt32();

                /*Based on the checkbox that was checked:
                 *When positive is checked, it'll check to see if the offset is less than or
                 *equal to the positive max offset the user entered and it will then check if the
                 *offset is actually positive (greater than 0).
                 *
                 *When negative is checked, it'll check to see if the offset is greater than or
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

            for (int i = 0; i < ptrResults.Items.Count; i++)
            {
                string Address = ptrResults.Items[i].ToString();

                //Make sure we didn't grab a blank
                if (Address.Trim() == "")
                    continue;

                //Parse the offset value
                int SmallCheck = int.Parse(Address.Substring(29, 8),
                                    NumberStyles.AllowHexSpecifier);

                //If the result is the best so far, save some data about it
                if (SmallestOffset == 0 || SmallCheck < SmallestOffset)
                {
                    SmallestIndex = i;
                    SmallestOffset = SmallCheck;
                }
            }

            //Select the smallest one and focus on the listbox
            //This selection change will trigger the event that gets the pointer code
            ptrResults.SelectedIndex = SmallestIndex;
            ptrResults.Focus();
        }

        private void ptrResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ptrResults.SelectedIndex > -1)
            {
                string ptrLine = ptrResults.SelectedItem.ToString();
                setPtrCode(ptrLine.Substring(3, 7), ptrLine.Substring(29, 8));
            }
        }

        private void setPtrCode(string addressStr, string offsetStr)
        {
            StringBuilder ptrCode = new StringBuilder();

            int hc = HexStrToInt(HexValue.Text);
            int address = HexStrToInt(addressStr);
            int offset = HexStrToInt(offsetStr);

            ptrCode.Append(String.Format("6{0:X7} 00000000\nB{0:X7} 00000000\n", address));
            ptrCode.Append(
                offset < 0
                //Negative offset
                ? String.Format(
                "DC000000 {0:X8}\n" +
                "{1:X}8000000 {2:X8}\n" +
                "D2000000 00000000",
                offset - 0x08000000,
                getCodeType(), hc
                )
                //Positive offset
                : String.Format(
                "{0:X}{1:X7} {2:X8}\n" +
                "D2000000 00000000",
                getCodeType(),
                offset, hc
                )
            );

            PtARDS.Text = ptrCode.ToString();
        }

        //Pointer Searcher Helper Functions

        private string getCodeType()
        {
            int hc = HexStrToInt(HexValue.Text);

            //Get the code type based on the Hex Value input
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
            //Elixirdream's requested feature
            CodeOutput.IsReadOnly = String.IsNullOrEmpty(CodeOutput.Text);
        }

        private void CodePort_Click(object sender, RoutedEventArgs e)
        {
            string Ported = "";
            CodeOutput.Clear();
            StringBuilder cb = new StringBuilder();

            //No input? Alert the user.
            if (String.IsNullOrEmpty(CodeInput.Text))
            {
                MessageBox.Show(this, "There is no code input.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //No offset? Alert the user.
            if (String.IsNullOrEmpty(CodeOffset.Text))
            {
                MessageBox.Show(this, "No offset has been specified.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (string line in CodeInput.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (Valid(line, @"[0-9A-F]{8}\s+[0-9A-F]{8}") &&
                    !Valid(line, @"[CD][C0-24-5]0{6}\s+[0-9A-F]{8}|[3-9A]4[0-9A-F]{6}\s+[0-9A-F]{8}|927[0-9A-F]{5}\s+[0-9A-F]{8}"))
                {
                    string AddyOnly = line.Substring(0, 8);
                    string ValyOnly = line.Substring(9, 8);
                    int AddyConvert = HexStrToInt(AddyOnly);
                    int ValyConvert = HexStrToInt(ValyOnly);
                    int OffyConvert = HexStrToInt(CodeOffset.Text);

                    if (Valid(line, @"D[36-9A-B]0{6}\s+[0-9A-F]{8}")) //Dx Lines
                    {
                        Ported = CodeAdd.IsChecked == true 
                            ? (ValyConvert + OffyConvert).ToString("X8") 
                            : (ValyConvert - OffyConvert).ToString("X8");
                        cb.AppendLine(AddyOnly.ToUpper() + " " + Ported.ToUpper());
                    }
                    else //Non-Dx lines
                    {
                        Ported = CodeAdd.IsChecked == true 
                            ? (AddyConvert + OffyConvert).ToString("X8") 
                            : (AddyConvert - OffyConvert).ToString("X8");
                        cb.AppendLine(Ported.ToUpper() + " " + ValyOnly.ToUpper());
                    }
                }
                else cb.AppendLine(line); //Everything else
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

        #region LoopCodeGenerator
        private void LoopBase_KeyDown(object sender, KeyEventArgs e)
        {
            /*KeyDown event on LoopBase to-
             *1. Make sure only hex digits can be pressed
             *2. Add a space for the user on after they enter digit 8
             */

            HexOnly_KeyDown(sender, e);

            if (LoopBase.Text.Length == 8 && e.Key != Key.Back)
            {
                LoopBase.Text += " ";
                LoopBase.SelectionStart = 10;
            }
        }

        private void LoopGen_Click(object sender, RoutedEventArgs e)
        {
            bool run = true;
            string check = "";

            //Check if the Base, Increment, or Offset text boxes are empty
            //If they are, return/exit
            if (String.IsNullOrEmpty(LoopBase.Text) || 
                String.IsNullOrEmpty(LoopCount.Text) || 
                String.IsNullOrEmpty(LoopOffset.Text))
                return;

            string FullOffset = HexStrToInt(LoopOffset.Text).ToString("X8");
            string ConvCount = (Convert.ToInt32(LoopCount.Text) - 1).ToString("X8"); //Subtract 1 to get the loop offset

            //Check the offset
            switch (HexStrToInt(LoopOffset.Text))
            {
                case 1:
                    check = D8;
                    break;
                case 2:
                    check = D7;
                    break;
                case 4:
                    check = D6;
                    break;
                default:
                    run = false;
                    break;
            }

            //Did the user enter an invalid code?
            if (!Valid(LoopBase.Text, @"[0-9A-F]{8} [0-9A-F]{8}"))
            {
                LoopOutput.Clear();
                MessageBox.Show(this, "Please enter a full code.", "Base Code Error", 
                                         MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (run)
            {
                if (LoopBase.Text[0] == '0')
                {
                    LoopOutput.Text = String.Format(
                        "{0}{1}\n{2}{3}\n{4}{5}\n",
                        D5, LoopBase.Text.Substring(9, 8),
                        C0, ConvCount,
                        check, LoopBase.Text.Substring(0, 8)
                    );

                    LoopOutput.Text += !String.IsNullOrEmpty(LoopInc.Text)
                                       //If there's text in the increment textbox, add the value increment.
                                       ? D4 + HexStrToInt(LoopInc.Text).ToString("X8") + '\n' + D2
                                       //Else, don't add the value increment.
                                       : D2;
                }
                else
                {
                    LoopOutput.Clear();
                    MessageBox.Show(this, "Please check to see if your base code starts with a '0' and if you've entered " +
                                    "an offset increment of 1, 2, or 4.", "Value Increment Error", MessageBoxButton.OK,
                                                                                                MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (LoopBase.Text[0] >= '0' && LoopBase.Text[0] < '3')
                    LoopOutput.Text = String.Format(
                        "{0}{1}\n{2}\n{3}{4}\n{5}",
                        C0, ConvCount,
                        LoopBase.Text,
                        DC, FullOffset,
                        D2
                    );
                else
                {
                    MessageBox.Show(this, "Please start your code off with a 0, 1, or 2.", "Data Input Error",
                                                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void LoopCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(LoopOutput.Text);
        }

        private void LoopClear_Click(object sender, RoutedEventArgs e)
        {
            LoopInc.Clear();
            LoopBase.Clear();
            LoopCount.Clear();
            LoopOffset.Clear();
            LoopOutput.Clear();
        }
        #endregion

        #region PatchCodeBuilder
        private void PatchBuild_Click(object sender, RoutedEventArgs e)
        {
            PatchInput.Text = PatchInput.Text.Trim();
            StringBuilder pb = new StringBuilder();
            String[] PatchCode = PatchInput.Text.Split('\n');

            //Multiply number of lines by 4 to get the offset
            int CodeOffset = PatchInput.LineCount * 4;

            //If the user didn't enter a full code, return/exit
            if (PatchInput.Text.Length < 17)
                return;

            pb.AppendLine(String.Format("E{0} {1:X8}",
            PatchCode[0].Substring(1, 7), CodeOffset));

            for (int i = 0; i < PatchCode.Length; i++)
            {
                string CodeValues = PatchCode[i].Substring(9, 8);

                if (i % 2 == 0)
                    pb.Append(CodeValues + ' ');
                else pb.AppendLine(CodeValues);
            }

            if (PatchInput.LineCount % 2 != 0)
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

        #region CodeBeautifier
        private void CodeBeautify_Click(object sender, RoutedEventArgs e)
        {
            //First verify the input, or risk death.
            if (!Verify(txtCodeInput.Text))
            {
                MessageBox.Show(this, "The code input is invalid.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Build the code onto here.
            List<string> processed = new List<string>();

            foreach (string curline in txtCodeInput.Text.Split(new char[] { '\n' }))
            {
                //Trim everything.
                string line = curline.Trim();

                //Skip comments/blank lines if they aren't allowed.
                if ((chkStripComments.IsChecked == true && line.StartsWith(":")) ||
                    (chkStripBlankLines.IsChecked == true && String.IsNullOrEmpty(line)))
                    continue;

                //Add comments and blank lines directly...
                if (line.StartsWith(":") || String.IsNullOrEmpty(line))
                    processed.Add(line);
                //Otherwise it's a line of code...
                else
                {
                    //Enforce the Uppercase Hex option.
                    if (chkUpperHex.IsChecked == true)
                        line = line.ToUpper();

                    //Remove the whitespace.
                    line = line.Replace(" ", "").Replace("\t", "");

                    //Condense all hunks of code into one line.
                    //If processed is empty, or the last line in in processed is
                    //either a blank line or a comment, we need to start a new
                    //code line.
                    if (processed.Count == 0 ||
                        String.IsNullOrEmpty(processed[processed.Count - 1]) ||
                        processed[processed.Count - 1].StartsWith(":"))
                        processed.Add(line);
                    else processed[processed.Count - 1] += line;
                }
            }

            //Build the final result onto here.
            List<string> final = new List<string>();

            foreach (string curline in processed)
            {
                string line = curline;

                if (line.StartsWith(":") || String.IsNullOrEmpty(line))
                    final.Add(line);
                else
                {
                    //If we don't have the right number of characters, uh-oh...
                    //bail out and call it invalid.
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
