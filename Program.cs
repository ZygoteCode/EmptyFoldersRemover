using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Filesystem.Ntfs;
using System.IO;
using System.Security.Principal;
using System;
using System.Linq;
using FileUnlockingSharp;

public class Program
{
    public static void Main()
    {
        Console.Title = "EmptyFoldersRemover | Made by https://github.com/GabryB03/";

        if (!(new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator))
        {
            Console.WriteLine("Please, run the program with Administrator privileges.");
            Console.WriteLine("Press the ENTER key in order to exit from the program.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Checking for empty folders, please wait a while...");
        DriveInfo driveToAnalyze = new DriveInfo(Environment.SystemDirectory[0].ToString());
        NtfsReader ntfsReader = new NtfsReader(driveToAnalyze, RetrieveMode.Minimal);
        IEnumerable<INode> nodes = ntfsReader.GetNodes(driveToAnalyze.Name);

        List<string> paths = new List<string>();

        foreach (INode node in nodes)
        {
            paths.Add(node.FullName);
        }

        List<string> directories = paths.Where(p => !p.Contains(".")).ToList();
        HashSet<string> nonEmptyDirectories = new HashSet<string>();

        foreach (string path in paths)
        {
            string parentDir = Path.GetDirectoryName(path);

            while (!string.IsNullOrEmpty(parentDir))
            {
                nonEmptyDirectories.Add(parentDir);
                parentDir = Path.GetDirectoryName(parentDir);
            }
        }

        IEnumerable<string> emptyDirectories = directories.Where(dir => !nonEmptyDirectories.Contains(dir));
        Console.WriteLine($"{emptyDirectories.Count()} empty directories have been found. Are you sure you want to delete them? Press ENTER key for confirmation.");
        Console.ReadLine();

        foreach (string dir in emptyDirectories)
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
                try
                {
                    FileUnlocker.ForcefullyCompleteDeletePath(dir);
                }
                catch
                {

                }
            }
        }
    }
}