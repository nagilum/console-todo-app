namespace Todo
{
    public class ConsoleEx
    {
        /// <summary>
        /// Write an exception error to console.
        /// </summary>
        /// <param name="ex">Exception to write.</param>
        public static void WriteException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR: ");

            Console.ResetColor();
            Console.WriteLine(ex.Message);
        }
    }
}