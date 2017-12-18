using DiffPlex;
using DiffPlex.DiffBuilder;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using static System.ConsoleColor;
using TrackChanges.CodePlayer;

namespace TrackChanges
{
    internal class CodeDiffer
    {
        private const string PATH_TO_WATCH = @"C:\Users\marce\Documents\Visual Studio 2017\Projects\ConsoleApp1\ConsoleApp1\";
        private const string FILE_NAME_TO_WATCH = "Program.cs";
        private Session session;
        private readonly ISideBySideDiffBuilder diffBuilder;
        private IDiffer differ = new Differ();

        public CodeDiffer()
        {
            // instantiate the object
            var fileSystemWatcher = new FileSystemWatcher();

            // Associate event handlers with the events
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            // tell the watcher where to look
            fileSystemWatcher.Path = PATH_TO_WATCH;

            // You must add this line - this allows events to fire.
            fileSystemWatcher.EnableRaisingEvents = true;

            StoreNewVersion();

            session = new CodePlayer.Session("csharp", GetNewVersionPath());

            diffBuilder = new SideBySideDiffBuilder(differ);

            GenerateSessionJson();
        }

        private void GenerateSessionJson()
        {
            GenerateSessionJson(@"C:\Users\marce\Downloads\codeplayer-master\codeplayer-master\demo\", "demo.json");
        }

        private void GenerateSessionJson(string path, string file)
        {
            using (var sw = new StreamWriter(Path.Combine(path, file)))
            {
                sw.Write(JsonConvert.SerializeObject(session));
            }
        }

        private static void StoreNewVersion()
        {
            File.Copy(GetNewVersionPath(), GetOldVersionPath(), overwrite: true);
        }

        private static string GetNewVersionPath()
        {
            return Path.Combine(PATH_TO_WATCH, FILE_NAME_TO_WATCH);
        }

        private static string GetOldVersionPath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FILE_NAME_TO_WATCH);
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Console.ForegroundColor = Yellow;
            //Console.WriteLine($"A new file has been renamed from {e.OldName} to {e.Name}");

            if (e.Name == FILE_NAME_TO_WATCH)
            {
                string oldText = File.ReadAllText(GetOldVersionPath());
                string newText = File.ReadAllText(GetNewVersionPath());
                var model = diffBuilder.BuildDiffModel(oldText ?? string.Empty, newText ?? string.Empty);
                int charCount = 0;
                for (int i = 0; i < model.OldText.Lines.Count; i++)
                {
                    var oldLine = model.OldText.Lines[i];
                    var newLine = model.NewText.Lines[i];

                    if (oldLine.Type != DiffPlex.DiffBuilder.Model.ChangeType.Imaginary)
                    {
                        charCount += oldLine.Text.Length + 1;
                    }
                    switch (oldLine.Type)
                    {
                        case DiffPlex.DiffBuilder.Model.ChangeType.Unchanged:
                            break;
                        case DiffPlex.DiffBuilder.Model.ChangeType.Deleted:
                            Console.ForegroundColor = Red;
                            Console.WriteLine($"{oldLine.Position}: Removida: {oldLine.Text}");
                            session.AddActionDelete(charCount, oldLine.Text);
                            break;
                        case DiffPlex.DiffBuilder.Model.ChangeType.Inserted:
                            Console.ForegroundColor = Green;
                            Console.WriteLine($"{oldLine.Position}: Inserida: {oldLine.Text}");
                            //session.AddActionTyping(oldLine.Text);
                            break;
                        case DiffPlex.DiffBuilder.Model.ChangeType.Imaginary:
                            session.AddActionTyping(charCount, "\n");
                            session.AddActionTyping(charCount, newLine.Text);
                            break;
                        case DiffPlex.DiffBuilder.Model.ChangeType.Modified:
                            Console.ForegroundColor = Yellow;
                            Console.WriteLine($"{oldLine.Position}: Modificada: {oldLine.Text}");
                            break;
                        default:
                            break;
                    }
                }
                GenerateSessionJson();

                StoreNewVersion();
            }

        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.ForegroundColor = Red;
            Console.WriteLine($"A new file has been deleted - {e.Name}");
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.ForegroundColor = Green;
            Console.WriteLine($"A new file has been changed - {e.Name}");
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.ForegroundColor = Blue;
            Console.WriteLine($"A new file has been created - {e.Name}");
        }
    }
}
