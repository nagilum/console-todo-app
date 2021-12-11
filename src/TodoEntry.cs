namespace Todo
{
    public class TodoEntry
    {
        /// <summary>
        /// Unique Id.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// When was the todo created.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// When was the todo marked as completed.
        /// </summary>
        public DateTimeOffset? Completed { get; set; }

        /// <summary>
        /// Todo main text.
        /// </summary>
        public string Text { get; set; } = null!;
    }
}