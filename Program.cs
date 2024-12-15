
using System.CommandLine;


// הגדרת אפשרויות לפקודה bundle
var languageOption = new Option<string>("--language", "List of programming languages to include (comma-separated or 'all')")
{
    IsRequired = true // אפשרות זו היא חובה
};

var outputOption = new Option<FileInfo>("--output", "Output bundle file path and name")
{
    IsRequired = true // אפשרות זו היא חובה
};

var noteOption = new Option<bool>("--note", "Include source file path as comments in the bundle");
var sortOption = new Option<string>("--sort", "Sorting order: 'alphabetical' or 'by-type'");
var removeEmptyLinesOption = new Option<bool>("--remove", "Remove empty lines from source files");
var authorOption = new Option<string>("--author", "Author name to include in the bundle header");

// הגדרת פקודת bundle
var bundleCommand = new Command("bundle", "Bundle code files to a single file");
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(authorOption);
bool removeEmptyLines = false;


// הגדרת הפעולה לפקודת bundle
bundleCommand.SetHandler((language, output, note, sort, remove, author) =>
{
    try
    {

        Console.WriteLine("aaaaaaaaaaaaa", output.Directory.Exists);
        // בדיקת תקינות הקובץ
        if (!output.Directory.Exists)
        {
            Console.WriteLine("error: file path is invalid");
            return;
        }

        var languages = language == "all" ? null : language

        .Split(',').Select(l => l.Trim().ToLower()).ToList();
        var directoryPath = Directory.GetCurrentDirectory();

        // איתור קבצים מתאימים לפי סוג
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
              .Where(file => languages == null || languages.Contains(Path.GetExtension(file).TrimStart('.').ToLower()))
              .ToList();

        // סינון וסידור הקבצים
        files = sort == "by-type" ? files.OrderBy(file => Path.GetExtension(file)).ToList() : files.OrderBy(file => file).ToList();

        using (var writer = new StreamWriter(output.FullName))
        {
            // כתיבת מחבר
            if (!string.IsNullOrEmpty(author))
            {
                writer.WriteLine($"// Author: {author}");
            }

            foreach (var file in files)
            {
                // כתיבת ניתוב המקור אם נדרש
                if (note)
                {
                    writer.WriteLine($"// Source: {Path.GetFileName(file)}");
                }

                var lines = File.ReadLines(file);
                foreach (var line in lines)
                {

                    if (!(removeEmptyLines && string.IsNullOrWhiteSpace(line)))
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }

        Console.WriteLine($"Files bundled successfully into {output.FullName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}, languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

// הגדרת פקודת create-rsp
var rspFileOption = new Option<string>("--file", "Path to save the .rsp file");

var createRspCommand = new Command("create-rsp", "Create a response file for the bundle command")
    {
      rspFileOption
    };

createRspCommand.SetHandler(async (string file) =>
{
    Console.Write("Enter languages (comma separated or 'all'): ");
    var languagesInput = Console.ReadLine();


    // קלט עבור נתיב הפלט
    Console.Write("Enter output file path: ");
    var outputFilePath = Console.ReadLine();

    // קלט עבור אם לרשום הערות מקור
    Console.Write("Do you want to note the source? (true/false): ");
    var noteInput = Console.ReadLine();
    bool note = bool.TryParse(noteInput, out bool parsedNote) && parsedNote;

    // קלט עבור מיון
    Console.Write("Sort by (name/language): ");
    var sort = Console.ReadLine();

    // קלט עבור אם למחוק שורות ריקות
    Console.Write("Do you want to remove empty lines? (true/false): ");
    var removeInput = Console.ReadLine();
    bool remove = bool.TryParse(removeInput, out bool parsedRemove) && parsedRemove;

    // קלט עבור שם היוצר
    Console.Write("Enter author name: ");
    var author = Console.ReadLine();

    // בנה פקודת bundle
    string command = $"fib bundle --language {languagesInput} --output {outputFilePath} " +
          $"{(note ? "--note " : "")}" +
          $"--sort {sort} " +
          $"{(remove ? "--remove " : "")}" +
          $"{(string.IsNullOrWhiteSpace(author) ? "" : $"--author \"{author}\"")}".Trim();

    // שמירה לקובץ תגובה
    string responseFilePath = "response.rsp";
    await File.WriteAllTextAsync(responseFilePath, command);

    Console.WriteLine($"Response file created: {responseFilePath}");
}, rspFileOption);

// הוספת פקודות ל-Root Command
var rootCommand = new RootCommand("CLI tool for bundling code files")
    {
      bundleCommand,
      createRspCommand
    };

// הרצת הפקודה
return await rootCommand.InvokeAsync(args);


