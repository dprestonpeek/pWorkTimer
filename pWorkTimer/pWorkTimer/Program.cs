using System;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

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
        private static int offTime = 0;
        private static Point currPoint = new Point();
        private static Point lastPoint = new Point();
        private static bool updateDisplay = true;
        private static bool saveBreak = false;
        private static int numBreaks = 0;
        private static List<int> breaks = new List<int>();

        private static DateTime today;
        private static int fileItems = 10;
        private static int[] weekdays = new int[5];
        private static string prevLine = "";
        private static string weekHistory = "";

        private static string infofile = "../../../Resources/timerinfo.txt";
        private static string logfile = "../../../Resources/logfile.txt";
        static void Main(string[] args)
        {
            string input = "";
            Console.SetWindowSize(50, 10);
            today = DateTime.Today;
            LoadInfoFile();
            try
            {
                timerObj = new Timer(TimerCallback, null, 0, 1000);
                do
                {
                    input = Console.ReadKey().KeyChar.ToString();
                    switch (input)
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
                            SetWeekInfo();
                            SaveInfoFile();
                            updateDisplay = true;
                            break;
                        case "1":
                        case "2":
                        case "3":
                        case "4":
                        case "5":
                            int day = int.Parse(input);
                            string tab = "\t";
                            if (day == 3)
                                tab = "";
                            prevLine = "Time Elapsed " + (DayOfWeek)int.Parse(input) + ": " + tab + GetTimeFromSeconds(weekdays[day - 1]) + "\n"
                                + "Time Left " + (DayOfWeek)int.Parse(input) + ": \t" + GetTimeFromSeconds(hours - weekdays[day - 1]);
                            break;
                        case "w":
                            int total = 0;
                            total = GetWeekTime();
                            prevLine = "Week Time Elapsed: \t" + GetTimeFromSeconds(total) + "\nWeek Time Left: \t" + GetTimeFromSeconds((hours * 5) - total - offTime);
                            break;
                        case "b":
                            string line = "";
                            for (int i = 0; i < breaks.Count; i++)
                            {
                                line += i + ". \t" + GetTimeFromSeconds(breaks[i]) + "\n";
                            }
                            prevLine = line;
                            break;
                        case "m":
                            string[] split = weekHistory.Split('~');
                            prevLine = "";
                            for (int i = 0; i < split.Length; i++)
                            {
                                prevLine += i + ". \t" + GetTimeFromSeconds(int.Parse(split[i])) + "\n";
                            }
                            break;
                        case "o":
                            int offHours;
                            do
                            {
                                updateDisplay = false;
                                Console.WriteLine("\n\nHow many hours do you have off this week?");
                            } while (!int.TryParse(Console.ReadLine(), out offHours));
                            offTime = offHours * 3600;
                            updateDisplay = true;
                            break;
                        case "t":
                            Process.Start("notepad.exe", infofile);
                            break;
                        case "l":
                            Process.Start("notepad.exe", logfile);
                            break;
                        case "c":
                            prevLine = "";
                            break;
                    }
                } while (input != "x");
            }
            catch(Exception e)
            {
                LogFile(e.Message);
            }
        }

        private static void TimerCallback(Object o)
        {
            try
            {
                currPoint = new Point();
                GetCursorPos(ref currPoint);

                if (currPoint == lastPoint)
                {
                    idleTime++;
                }
                else
                {
                    if (saveBreak)
                    {
                        breaks.Add(idleTime);
                        saveBreak = false;
                        timer -= 60 * 15; //subtract 15 minutes spent idle
                    }
                    idleTime = 0;
                }
                lastPoint = currPoint;

                if (idleTime < 60 * 15) // under 15 minutes
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

                        SetWeekInfo();
                        SaveInfoFile();
                    }
                }
                else //over 15 minutes
                {
                    saveBreak = true;
                }
            }
            catch (Exception e)
            {
                LogFile(e.Message);
            }
        }

        public static string GetTimeFromSeconds(int seconds)
        {
            const int daySeconds = 28801;
            const int hourSeconds = 3600;
            const int minuteSeconds = 60;

            int minutes = seconds / minuteSeconds;
            int hours = seconds / hourSeconds;
            int days = seconds / daySeconds;

            hours = hours % 8;
            minutes = minutes % 60;
            seconds = seconds % 60;
            return days + ":" + hours + ":" + minutes + ":" + seconds;
        }

        private static void LoadInfoFile()
        {
            int retries = 3;
            string[] toRead = File.ReadAllLines(infofile);
            for (int i = 0; i < fileItems; i++)
            {
                string[] split = toRead[i].Split('=');
                if (i == 0)
                    timer = int.Parse(split[1]);
                if (i > 0 && i < 6)
                    weekdays[i - 1] = int.Parse(split[1]);
                if (i == 6)
                    SetBreakInfo(toRead[i].Split('~'));
                if (i == 7)
                {
                    CheckDayRestart(toRead[i]);
                }
                if (i == 8)
                {
                    weekHistory = toRead[i];
                }
                if (i == 9)
                {
                    offTime = int.Parse(toRead[i]);
                }
            }
        }

        private static void SaveInfoFile()
        {
            int retries = 3;

            string[] toWrite = new string[fileItems];
            toWrite[0] = "timer=" + timer;
            toWrite[1] = "monday=" + weekdays[0];
            toWrite[2] = "tuesday=" + weekdays[1];
            toWrite[3] = "wednesday=" + weekdays[2];
            toWrite[4] = "thursday=" + weekdays[3];
            toWrite[5] = "friday=" + weekdays[4];
            toWrite[6] = GetBreakInfo();
            toWrite[7] = today.ToString();
            toWrite[8] = weekHistory;
            toWrite[9] = offTime.ToString();
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    File.WriteAllLines(infofile, toWrite);
                }
                catch (Exception e)
                {

                }
            }
        }

        private static void SetWeekInfo()
        {
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    CheckWeekRestart();
                    weekdays[0] = timer;
                    break;
                case DayOfWeek.Tuesday:
                    CheckWeekRestart();
                    weekdays[1] = timer;
                    break;
                case DayOfWeek.Wednesday:
                    CheckWeekRestart();
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

        private static int GetWeekTime()
        {
            int total = 0;
            for (int i = 0; i < 5; i++)
            {
                total += weekdays[i];
            }
            return total;
        }

        private static void CheckWeekRestart()
        {
            if (weekdays[3] > 0 || weekdays[4] > 0)
            {
                weekHistory += "~" + GetWeekTime();
                timer = 0;
                offTime = 0;
                weekdays[0] = 0;
                weekdays[1] = 0;
                weekdays[2] = 0;
                weekdays[3] = 0;
                weekdays[4] = 0;
            }
        }

        private static void CheckDayRestart(string previousDate)
        {
            if (today != DateTime.Parse(previousDate))
            {
                timer = 0;
                breaks.Clear();
                today = DateTime.Today;
            }
        }

        private static string GetBreakInfo()
        {
            string breakInfo = "";
            foreach (int line in breaks)
            {
                breakInfo += line + "~";
            }
            return breakInfo;
        }

        private static void SetBreakInfo(string[] split)
        {
            foreach (string line in split)
            {
                if (line != "")
                    breaks.Add(int.Parse(line));
            }
        }

        private static void LogFile(string error)
        {
            // Create a writer and open the file:
            StreamWriter log;

            if (!File.Exists(logfile))
            {
                log = new StreamWriter(logfile);
            }
            else
            {
                log = File.AppendText(logfile);
            }

            // Write to the file:
            log.WriteLine(DateTime.Now);
            log.WriteLine(error);
            log.WriteLine();

            // Close the stream:
            log.Close();
        }
    }
}
