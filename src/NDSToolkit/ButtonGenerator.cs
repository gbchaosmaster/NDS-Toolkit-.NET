using System;
using System.Windows.Controls;

namespace NDSToolkit
{
    class ButtonGenerator
    {
        public void ButtonCode(int GBATotal, int NDSTotal, bool GBA, bool NDS, TextBox ButtonInput, TextBox ButtonOutput, TextBox NDSTst, TextBox GBATst)
        {
            //NOT_GBA and NOT_NDS will hold the 
            //16-bit values of NOT_GBATotal and NOT_NDSTotal
            string D2 = "D2000000 00000000";
            ushort NOT_GBA = (ushort)~GBATotal;
            ushort NOT_NDS = (ushort)~NDSTotal; 

            if (GBATotal != 0)
                GBA = true;
            if (NDSTotal != 0)
                NDS = true;

            if (GBA == true)
            {
                ButtonOutput.Text = "94000130 " + NOT_GBA.ToString("X") + "0000\n" + ButtonInput.Text + '\n' + D2;
                GBATst.Text = "GBA tst Value: 0x" + GBATotal.ToString("X4");
            }
            else GBATst.Clear();

            if (NDS == true)
            {
                ButtonOutput.Text = "927FFFA8 " + NOT_NDS.ToString("X") + "0000\n" + ButtonInput.Text + '\n' + D2;
                NDSTst.Text = "NDS tst Value: 0x" + NDSTotal.ToString("X4");
            }
            else NDSTst.Clear();

            if (GBA == true && NDS == true)
            {
                ButtonOutput.Text = "927FFFA8 " + NOT_NDS.ToString("X") + "0000\n94000130 " +
                             NOT_GBA.ToString("X") + "0000\n" + ButtonInput.Text + '\n' + D2;
            }
            else if (GBA == false && NDS == false)
            {
                ButtonOutput.Clear();
                GBATst.Clear();
                NDSTst.Clear();
            }
        }
    }
}
