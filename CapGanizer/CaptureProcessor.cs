using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CapGanizer
{
    class CaptureProcessor
    {
        Program.Options o;

        public CaptureProcessor(Program.Options o)
        {
            this.o = o;
        }

        public Boolean ProcessDirectories(IEnumerable<string> targetDirs)
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

        private String GetProcessFilesLogFilePath()
        {
            String exeDir = AppDomain.CurrentDomain.BaseDirectory;
            return System.IO.Path.Combine(exeDir, "files.capganizer");
        }

        private HashSet<String> GetProcessedFiles()
        {
            var logFilePath = GetProcessFilesLogFilePath();
            if (System.IO.File.Exists(logFilePath))
            {
                return new HashSet<string>(System.IO.File.ReadAllLines(logFilePath));
            }
            else
            {
                return new HashSet<string> { };
            }
        }

        private Boolean WriteProcessedFiles(HashSet<string> lines)
        {
            var logFilePath = GetProcessFilesLogFilePath();
            var linesToWrite = new String[lines.Count];
            lines.CopyTo(linesToWrite);
            try
            {
                System.IO.File.WriteAllLines(logFilePath, linesToWrite);
                Trace.TraceInformation($"Saved {linesToWrite.Length} files to archive");
                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError($"Failed to save logs to {logFilePath}");
                return false;
            }
        }

        public Boolean ProcessDirectory(String targetDir)
        {
            Trace.TraceInformation($"Starting to process {targetDir}");
            var captureDir = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos), "Captures");
            if (!System.IO.Directory.Exists(targetDir))
            {
                Trace.TraceError($"Could not find the target directory {targetDir}");
                return false;
            }
            var files = System.IO.Directory.EnumerateFiles(captureDir);
            var filenamePattern = new Regex(@"^(.*?)(\d{1,2}.\d{1,2}.\d{4}.\d{1,2}.\d{1,2}.\d{1,2}.PM)(\.[a-zA-Z0-9]+)$", RegexOptions.Compiled);
            var videoFilenamePattern = new Regex(@"^(.*?)(\d{1,4}.\d{1,2}.\d{1,2}.\d{1,2}.\d{1,2}.\d{1,2})(\.[a-zA-Z0-9]+)$", RegexOptions.Compiled);
            int filesProcessed = 0;
            var directoriesAlreadyAvailable = new HashSet<String> { };
            var directoriesFailedToBeCreated = new HashSet<string> { };
            var processedFilesSet = GetProcessedFiles();
            Trace.TraceInformation($"{processedFilesSet.Count} files found in the archive");
            foreach (var fileFullPath in files)
            {
                var fileName = System.IO.Path.GetFileName(fileFullPath);
                if (processedFilesSet.Contains(fileName))
                {
                    continue;
                }
                Match match;
                if (fileName.Substring(fileName.Length - 4) == ".mp4")
                {
                    match = videoFilenamePattern.Match(fileName);
                }
                else
                {
                    match = filenamePattern.Match(fileName);
                }
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
                    if (directoriesFailedToBeCreated.Contains(targetFileDir))
                    {
                        continue;
                    }
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
                                Trace.TraceError($"Could not create the directory {targetFileDir} - will not retry this folder");
                                directoriesFailedToBeCreated.Add(targetFileDir);
                                continue;
                            }

                        }
                        directoriesAlreadyAvailable.Add(targetFileDir);
                    }
                    if (System.IO.File.Exists(targetFilepath))
                    {
                        processedFilesSet.Add(fileName);
                        continue;
                    }
                    try
                    {
                        System.IO.File.Copy(fileFullPath, targetFilepath);
                        filesProcessed += 1;
                        processedFilesSet.Add(fileName);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"Could not copy the file to {targetFilepath}");
                    }
                }
            }
            WriteProcessedFiles(processedFilesSet);
            Trace.TraceInformation($"Completed processing {targetDir}");
            return true;
        }

        private static String Sanitize(String text)
        {
            text = text.Replace(' ', '_');
            return text.Trim(new char[] { '.', '*', '_' });
        }
    }
}
