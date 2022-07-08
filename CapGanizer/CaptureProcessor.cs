using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CapGanizer
{
    class CaptureProcessor
    {

        public static Boolean ProcessDirectories(IEnumerable<string> targetDirs)
        {
            Boolean isSuccesfull = true;
            foreach (String targetDir in targetDirs)
            {
                isSuccesfull = ProcessDirectory(targetDir);
                if (!isSuccesfull)
                {
                    return false;
                }
            }
            return true;
        }

        public static Boolean ProcessDirectory(String targetDir)
        {
            var captureDir = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos), "Captures");
            if(!System.IO.Directory.Exists(targetDir))
            {
                Console.Write($"Could not find the target directory {targetDir}");
                return false;
            }
            var files = System.IO.Directory.EnumerateFiles(captureDir);
            var filenamePattern = new Regex(@"^(.*?)(\d{1,2}.\d{1,2}.\d{4}.\d{1,2}.\d{1,2}.\d{1,2}.PM)(\.[a-zA-Z]+)$", RegexOptions.Compiled);
            int filesProcessed = 0;
            var directoriesAlreadyAvailable = new HashSet<String> { };
            foreach (var fileFullPath in files)
            {
                var fileName = System.IO.Path.GetFileName(fileFullPath);
                var match = filenamePattern.Match(fileName);
                if (match.Success)
                {
                    var gameName = match.Groups[1].Value;
                    var timeStampPart = match.Groups[2].Value;
                    gameName = gameName.Replace(" ", "_");
                    timeStampPart = Sanitize(timeStampPart);
                    gameName = Sanitize(gameName);
                    if (gameName.Length < 1)
                    {
                        continue;
                    }
                    var targetFileDir = System.IO.Path.Combine(targetDir, gameName);
                    var targetFilepath = System.IO.Path.Combine(targetFileDir, fileName);
                    if (!directoriesAlreadyAvailable.Contains(targetFileDir))
                    {
                        if (!System.IO.Directory.Exists(targetFileDir))
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(targetFileDir);
                            }
                            catch (Exception e)
                            {
                                Console.Write($"Could not create the directory {targetFileDir}");
                                return false;
                            }

                        }
                        directoriesAlreadyAvailable.Add(targetFileDir);
                    }
                    if(System.IO.File.Exists(targetFilepath))
                    {
                        continue;
                    }
                    try
                    {
                        System.IO.File.Copy(fileFullPath, targetFilepath);
                        filesProcessed += 1;

                    }
                    catch (Exception e)
                    {
                        Console.Write($"Could not copy the file to {targetFilepath}");
                        return false;
                    }
                }
            }
            return true;
        }

        private static String Sanitize(String text)
        {
            text = text.Replace(' ', '_');
            return text.Trim(new char[] { '.', '*', '_' });
        }
    }
}
