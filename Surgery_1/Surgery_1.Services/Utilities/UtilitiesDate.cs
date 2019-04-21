using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Utilities
{
    public static class UtilitiesDate
    {
        public static int ConvertDateToNumber(DateTime day)
        {
            string dayNum = day.Day < 10 ? "0" + day.Day.ToString() : day.Day.ToString();
            string monthNum = day.Month < 10 ? "0" + day.Month.ToString() : day.Month.ToString(); ;
            string yearNum = day.Year.ToString();
            string date = yearNum + monthNum + dayNum;
            return int.Parse(date);
        }
        public static string ConvertDateToString(DateTime day)
        {
            string dayNum = day.Day < 10 ? "0" + day.Day.ToString() : day.Day.ToString();
            string monthNum = day.Month < 10 ? "0" + day.Month.ToString() : day.Month.ToString(); ;
            string yearNum = day.Year.ToString();
            string dateString = yearNum + "-" + monthNum + "-" + dayNum;
            return dateString;
        }
        public static string FormatDateShow(DateTime day)
        {
            string dayNum = day.Day < 10 ? "0" + day.Day.ToString() : day.Day.ToString();
            string monthNum = day.Month < 10 ? "0" + day.Month.ToString() : day.Month.ToString(); ;
            string yearNum = day.Year.ToString();
            string dateString = dayNum + "/" + monthNum + "/" + yearNum;
            return dateString;
        }
        public static string GetTimeFromDate(DateTime day)
        {
            string hour = day.Hour < 10 ? "0" + day.Hour.ToString() : day.Hour.ToString();
            string minute = day.Minute < 10 ? "0" + day.Minute.ToString() : day.Minute.ToString();
            return hour + ":" + minute;
        }
        public static DateTime GetDateTimeNoSecond(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }

    }
}
