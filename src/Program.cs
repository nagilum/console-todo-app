using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Todo
{
    public class Program
    {
        /// <summary>
        /// List of all todo's.
        /// </summary>
        private static List<TodoEntry> TodoEntries = new();

        /// <summary>
        /// Init all the things..
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {
            // Load config from disk.
            if (!Config.Load())
            {
                return;
            }

            // Load todo's from disk.
            LoadTodos();

            // Display the list.
            if (args.Length == 0)
            {
                ShowList();
                return;
            }

            // Display the help screen?
            if (args[0] == "-h" ||
                args[0] == "--help")
            {
                ShowHelp();
                return;
            }

            // Mark todos as completed?
            if (args[0] == "-c" &&
                args.Length > 1)
            {
                ToggleTodoAsCompleted(args.Skip(1));
                SaveTodos();

                return;
            }

            // Delete todos?
            if (args[0] == "-d" &&
                args.Length > 1)
            {
                DeleteTodos(args.Skip(1));
                SaveTodos();

                return;
            }

            // Add todo.
            //var text = string.Join(" ", args);
            AddTodo(string.Join(" ", args));
            SaveTodos();
        }

        /// <summary>
        /// Add todo.
        /// </summary>
        /// <param name="text">Text of the todo.</param>
        private static void AddTodo(string text)
        {
            string id;

            while(true)
            {
                id = Guid.NewGuid().ToString().Substring(0, 2);

                if (TodoEntries.All(n => n.Id != id))
                {
                    break;
                }
            }

            var entry = new TodoEntry
            {
                Id = id,
                Created = DateTimeOffset.Now,
                Text = text
            };

            TodoEntries.Add(entry);
        }

        /// <summary>
        /// Delete todos permanently.
        /// </summary>
        /// <param name="ids">List of ids.</param>
        private static void DeleteTodos(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                var todo = TodoEntries
                    .FirstOrDefault(n => n.Id == id);

                if (todo == null)
                {
                    Console.Write("Todo with id ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(id);
                    Console.ResetColor();
                    Console.WriteLine(" not found.");

                    return;
                }

                TodoEntries.Remove(todo);
            }
        }

        /// <summary>
        /// Load todo's from disk.
        /// </summary>
        private static void LoadTodos()
        {
            try
            {
                var path = Config.Get(
                    "path",
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "todos.json"));

                if (!File.Exists(path))
                {
                    return;
                }

                TodoEntries = JsonSerializer.Deserialize<List<TodoEntry>>(
                    File.ReadAllText(path, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteException(ex);
            }
        }

        /// <summary>
        /// Save todos to disk.
        /// </summary>
        private static void SaveTodos()
        {
            try
            {
                var path = Config.Get(
                    "path",
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "todos.json"));

                if (path == null)
                {
                    // This null check will never be true, but since Visual Studio is a piece of
                    // crap software sometimes and Microsoft can go fuck themselves, I have to add
                    // this extra stupid null check to make the fucking code happy.

                    throw new Exception(
                        "Fuck Microsoft and Visual Studio for not letting " +
                        "me supress ALL warnings OUTSIDE the source code!");
                }

                File.WriteAllText(
                    path,
                    JsonSerializer.Serialize(TodoEntries),
                    Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteException(ex);
            }
        }

        /// <summary>
        /// Display the help screen.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine($"Todo v{Assembly.GetExecutingAssembly().GetName().Version}");
            Console.WriteLine();

            Console.WriteLine("Usage: todo [-option list-of-ids] [new-todo-text]");
            Console.WriteLine();

            Console.WriteLine("Add todo:");
            Console.WriteLine(" todo This will be the new todo text");
            Console.WriteLine();

            Console.WriteLine("Toggle completed on todos:");
            Console.WriteLine(" todo -c id");
            Console.WriteLine(" todo -c id1 id2 id3");
            Console.WriteLine();

            Console.WriteLine("Delete a todo permanently:");
            Console.WriteLine(" todo -d id");
            Console.WriteLine(" todo -d id1 id2 id3");
            Console.WriteLine();
        }

        /// <summary>
        /// Display the list of all todo's.
        /// </summary>
        private static void ShowList()
        {
            var todos = TodoEntries
                .Where(n => !n.Completed.HasValue)
                .ToList();

            var completedTodos = TodoEntries
                .Where(n => n.Completed.HasValue)
                .ToList();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Id Text ");
            Console.ResetColor();

            if (todos.Any())
            {
                foreach (var todo in todos)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($" {todo.Id} ");

                    Console.ResetColor();

                    var texts = SplitTextWithBuffer(todo.Text, 5);

                    Console.WriteLine(texts[0]);

                    if (texts.Count == 1)
                    {
                        continue;
                    }

                    foreach (var text in texts.Skip(1))
                    {
                        Console.WriteLine($"    {text}");
                    }
                }
            }
            else
            {
                Console.WriteLine("  -- no todos -- ");
            }

            if (!completedTodos.Any())
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Completed todos");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Id Completed        Text ");
            Console.ResetColor();

            foreach (var todo in completedTodos)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($" {todo.Id} ");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{todo.Completed:yyyy-MM-dd HH:mm} ");

                Console.ResetColor();

                var texts = SplitTextWithBuffer(todo.Text, 21);

                Console.WriteLine(texts[0]);

                if (texts.Count == 1)
                {
                    continue;
                }

                foreach (var text in texts.Skip(1))
                {
                    Console.WriteLine($"                     {text}");
                }
            }
        }

        /// <summary>
        /// Split a text into several pieces based on console width.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="buffer">Buffer width.</param>
        /// <returns>List of text pieces.</returns>
        private static List<string> SplitTextWithBuffer(string text, int buffer)
        {
            var width = Console.WindowWidth - buffer;

            if (text.Length < width)
            {
                return new List<string> {
                    text.Trim()
                };
            }

            var list = new List<string>();

            while(true)
            {
                var partial = text[..width];
                var point = partial.LastIndexOf(" ");

                if (point == -1)
                {
                    list.Add(partial.Trim());
                    point = width;
                }
                else
                {
                    partial = partial[..point];
                    list.Add(partial.Trim());
                }

                text = text[point..];

                if (text.Length < width)
                {
                    list.Add(text.Trim());
                    break;
                }
            }

            return list;
        }

        /// <summary>
        /// Toggle todos as completed or not.
        /// </summary>
        /// <param name="ids">List of ids.</param>
        private static void ToggleTodoAsCompleted(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                var todo = TodoEntries
                    .FirstOrDefault(n => n.Id == id);

                if (todo == null)
                {
                    Console.Write("Todo with id ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(id);
                    Console.ResetColor();
                    Console.WriteLine(" not found.");

                    return;
                }

                todo.Completed = todo.Completed.HasValue
                    ? null
                    : DateTimeOffset.Now;
            }
        }
    }
}