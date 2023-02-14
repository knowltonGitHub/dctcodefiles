using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading;
//using System.Threading.Tasks;

namespace DCT
{
    public static class Helper
    {        
        public static void L(System.Windows.Forms.TextBox tb, string message)
        {
            tb.Text += System.Environment.NewLine + message;
        }

        public static void CreateCopyOfCurrentDB(int rowcount, string message)
        {
            //to create an artificial one second gap between copies
            //in order to keep the file names from overlapping
            //when the end-user creates manual back ups when clicking super-fast
            Thread.Sleep(1000);

            DateTime dt = new DateTime();
            dt = DateTime.Now;
            string datestampstring = ReturnDateTimeStampAsPseudoJulian(dt);

            string tempfname = "CBDB" +
                datestampstring + "_" + message + "_" + rowcount.ToString() + ".db";

            System.IO.File.Copy("CBDB.db", tempfname);

            System.IO.File.SetLastWriteTime(tempfname, dt);
        }

        public static string ReturnDateTimeStampAsPseudoJulian(DateTime dt)
        {
           
            string Year = dt.Year.ToString();
            string Month = CreateDoubleDigit(dt.Month);
            string Day = CreateDoubleDigit(dt.Day);
            string Hour = CreateDoubleDigit(dt.Hour);
            string Minute = CreateDoubleDigit(dt.Minute);
            string Second = CreateDoubleDigit(dt.Second);
            string Millisecond = CreateDoubleDigit(dt.Millisecond);
            return Year + Month + Day + Hour + Minute + Second + Millisecond;
        }

        public static string CreateDoubleDigit(int digit)
        {
            string temp = digit < 10 ? "0" + digit.ToString() : digit.ToString();

            return temp;
        }
    }    
}//end of namespace
