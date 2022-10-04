using System;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace pWorkTimer
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        private static Timer timerObj = null;
        private static int hours = 8 * 3600;
        private static int myHours = 0;
        private static int timer = 0;
        private static int idleTime = 0;
        private static Point currPoint = new Point();
        private static Point lastPoint = new Point();
        private static bool updateDisplay = true;

        private static int fileItems = 6;
        private static int[] weekdays = new int[5];
        private static string prevLine = "";

        private static string infofile = "../../../Resources/timerinfo.txt";
        static void Main(string[] args)
        {
            string input = "";
            LoadInfoFile();
            timerObj = new Timer(TimerCallback, null, 0, 1000);
            do
            {
                input = Console.ReadKey().KeyChar.ToString();
                switch(input)
                {
                    case "+":
                        do
                        {
                            updateDisplay = false;
                            Console.WriteLine("\n\nHow long have you worked today?");
                        } while (!int.TryParse(Console.ReadLine(), out myHours));
                        if (myHours < 8)
                        {
                            myHours *= 3600;    //turn seconds into hours
                        }
                        else
                        {
                            myHours *= 60;  //turn seconds into minutes
                        }
                        timer += myHours;
                        SaveWeekInfo();
                        SaveInfoFile();
                        updateDisplay = true;
                        break;
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                        int day = int.Parse(input);
                        prevLine = "Time Elapsed " + (DayOfWeek)int.Parse(input) + ": " + GetTimeFromSeconds(weekdays[day - 1]) + "\n"
                            + "Time Left " + (DayOfWeek)int.Parse(input) + ": " + GetTimeFromSeconds(hours - weekdays[day - 1]);
                        break;
                    case "w":
                        int total = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            total += weekdays[i];
                        }
                        prevLine = "Week Time Elapsed: " + GetTimeFromSeconds(total) + "\nWeek Time Left: " + GetTimeFromSeconds((hours * 5) - total);
                        break;
                    case "c":
                        prevLine = "";
                        break;
                }
            } while (input != "x");
        }

        private static void TimerCallback(Object o)
        {
            currPoint = new Point();
            GetCursorPos(ref currPoint);
            if (currPoint == lastPoint) 
                idleTime++;
            else 
                idleTime = 0;
            lastPoint = currPoint;

            if (idleTime < 60 * 15) //15 minutes
            {
                timer++;
                if (updateDisplay)
                {
                    Console.Clear();
                    Console.Write("Time Elapsed: \t\t");
                    Console.WriteLine(GetTimeFromSeconds(timer));
                    Console.Write("Time Left today: \t");
                    Console.WriteLine(GetTimeFromSeconds(hours - timer));
                    Console.WriteLine(prevLine);

                    SaveWeekInfo();
                    SaveInfoFile();
                    //Console.WriteLine("\"+\" to add time to Today");
                }
            }
        }

        public static string GetTimeFromSeconds(int seconds)
        {
            const int daySeconds = 28800;
            const int hourSeconds = 3600;
            const int minuteSeconds = 60;

            int minutes = seconds / minuteSeconds;
            int hours = seconds / hourSeconds;
            int days = seconds / daySeconds;

            hours = hours % 24;
            minutes = minutes % 60;
            seconds = seconds % 60;
            return days + ":" + hours + ":" + minutes + ":" + seconds;
        }

        private static void LoadInfoFile()
        {
            string[] toRead = File.ReadAllLines(infofile);
            for (int i = 0; i < fileItems; i++)
            {
                string[] split = toRead[i].Split('=');
                if (i == 0)
                    timer = int.Parse(split[1]);
                if (i > 0)
                    weekdays[i - 1] = int.Parse(split[1]);
            }
        }

        private static void SaveInfoFile()
        {
            string[] toWrite = new string[fileItems];
            toWrite[0] = "timer=" + timer;
            toWrite[1] = "monday=" + weekdays[0];
            toWrite[2] = "tuesday=" + weekdays[1];
            toWrite[3] = "wednesday=" + weekdays[2];
            toWrite[4] = "thursday=" + weekdays[3];
            toWrite[5] = "friday=" + weekdays[4];
            File.WriteAllLines(infofile, toWrite);
        }

        private static void SaveWeekInfo()
        {
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    weekdays[0] = + timer;
                    break;
                case DayOfWeek.Tuesday:
                    weekdays[1] = timer;
                    break;
                case DayOfWeek.Wednesday:
                    weekdays[2] = timer;
                    break;
                case DayOfWeek.Thursday:
                    weekdays[3] = timer;
                    break;
                case DayOfWeek.Friday:
                    weekdays[4] = timer;
                    break;
            }
        }
    }
}
