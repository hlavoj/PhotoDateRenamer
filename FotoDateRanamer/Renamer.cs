using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace FotoDateRanamer
{
    public class Renamer
    {
        private readonly bool _debug;
        private readonly int _level;
        private readonly int _interval;
        private const string DateFormat = "yyyy_MM_dd -";

        ILog _logger = LogManager.GetLogger(typeof(Renamer));

        public Renamer(bool debug, int level, int interval)
        {
            _debug = debug;
            _level = level;
            _interval = interval;
        }

        public List<string> ReadDirectories(string path)
        {
            //path = @"C:\School\fotos\test2\";
            foreach (string s in Directory.GetDirectories(path))
            {
                string dirPath = s.Substring(0, s.LastIndexOf("\\")+1);
                string dirName = s.Substring(s.LastIndexOf("\\")+1, s.Length - s.LastIndexOf("\\")-1);
                try
                {
                    Tuple<DateTime, string> result;
                    try
                    {
                        bool hasCorrectFormat = HasCorrectFormat(dirName);
                        if (hasCorrectFormat)
                        {
                            _logger.Debug("Directory has correct format.");
                            continue;
                        }

                        result = TryFindDate(dirName);
                        DateTime fileDateTimeAvg = GetAvgFileTime(s);

                        int tmpInterval = _interval;
                        if (result.Item1.Day == 1 && result.Item1.Month == 1)
                        {
                            _logger.Debug("Directory date is only year.");
                            tmpInterval = 365;
                        }

                        if (Math.Abs(fileDateTimeAvg.Date.Ticks - result.Item1.Ticks) > 864000000000 * tmpInterval)
                        {
                            
                            _logger.ErrorFormat("Date dont match avrage date {0}", dirName);
                            if (_level==2)
                                RenameDir(s, dirPath + fileDateTimeAvg.ToString(DateFormat) + " " + dirName);
                            if (_level==3)
                                RenameDir(s, dirPath + result.Item1.ToString(DateFormat) + " " + result.Item2);

                        }
                        else
                        {
                            _logger.DebugFormat("Avrage datetime files is: {0} for folder: {1}", fileDateTimeAvg.ToString("dd.MM.yyyy"), dirName);
                            RenameDir(s, dirPath + fileDateTimeAvg.ToString(DateFormat) + " " + result.Item2);
                        }
                    }
                    catch (Exception)
                    {

                        _logger.Warn("Folder has not date. folder: " + dirName);
                        DateTime fileDateTimeAvg = GetAvgFileTime(s);
                        _logger.DebugFormat("Avrage datetime files is: {0} for folder: {1}", fileDateTimeAvg.ToString("dd.MM.yyyy"), dirName);
                        if(_level==1)
                            RenameDir(s, dirPath + fileDateTimeAvg.ToString(DateFormat) + " " + dirName);
                    }


                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Dir {0} failed to rename.", s);

                }

            }
            return null;
        }

        private bool HasCorrectFormat(string dirName)
        {
            if (dirName.Substring(10,1) == " " 
                && dirName.Substring(11, 1) == "-"
                && dirName.Substring(12, 1) == " "
                )
            {
               string date = dirName.Substring(0, 12);
                try
                {
                    DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        private DateTime GetAvgFileTime(string s)
        {
            double sum = 0;
            List<string> files = Directory.GetFiles(s,"*.jpg").ToList();
            for (int i = 0; i < files.Count; i+=1)
            {
                sum += GetDateFromPhoto(files[i]).Ticks / files.Count;
            }

            return new DateTime((long)sum);
        }

        private static DateTime GetDateFromPhoto(string path)
        {
            DateTime? dateTime = GetDateTakenFromPhoto(path);
            if(dateTime== null)
            {
                dateTime = File.GetLastWriteTime(path);
            }
            return dateTime.Value;
        }


        private static DateTime? GetDateTakenFromPhoto(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (Image myImage = Image.FromStream(fs, false, false))
                {
                    // Check if the photo has a property item with ID 36867, which stores the date taken
                    if (myImage.PropertyIdList.Contains(36867))
                    {
                        PropertyItem propItem = myImage.GetPropertyItem(36867);
                        string dateTaken = Encoding.UTF8.GetString(propItem.Value).Trim();
                        return DateTime.ParseExact(dateTaken.Substring(0, 19), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception)
            {
                // Handle exception
            }

            return null;
        }


        private void RenameDir(string sourceDirName, string destDirName)
        {
            _logger.InfoFormat("destDirName: {0} -> sourceDirName: {1}", destDirName, sourceDirName);
            if (!_debug) {
                Directory.Move(sourceDirName, destDirName);
            }
        }

        private Tuple<DateTime, string> TryFindDate(string s)
        {
            try
            {
                return TryFindDate1(s);
            }
            catch (Exception) { }

            try
            {
                return TryFindDate2(s);
            }
            catch (Exception) { }
            throw new Exception("Date not found");
        }

        private Tuple<DateTime, string> TryFindDate1(string s)
        {

            string oldDate = s.Substring(s.LastIndexOf(" "), s.Length - s.LastIndexOf(" "));
            DateTime dt = TryGetDateTime(oldDate.Trim());

            DateTime newDate = dt;
            string nameWithoutDate = s.Substring(0, s.LastIndexOf(" "));
            return new Tuple<DateTime, string>( newDate, nameWithoutDate);
        }

        private Tuple<DateTime, string> TryFindDate2(string s)
        {

            string oldDate = s.Substring(s.LastIndexOf(" "), s.Length - s.LastIndexOf(" "));
            DateTime dt = new DateTime(Convert.ToInt32(oldDate.Trim()), 1, 1);

            DateTime newDate = dt;
            string nameWithoutDate = s.Substring(0, s.LastIndexOf(" "));
            return new Tuple<DateTime, string>( newDate, nameWithoutDate);
        }

        private DateTime TryGetDateTime(string date)
        {
            try
            {
                return DateTime.Parse(date.Trim());
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
