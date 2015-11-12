
namespace StitcherBoy.Logging
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// ILogging implementation for Task
    /// </summary>
    public class TaskLogging : ILogging
    {
        private readonly TaskLoggingHelper _logging;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskLogging"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        public TaskLogging(ITask task)
        {
            _logging = new TaskLoggingHelper(task);
        }

        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void Write(string format, params object[] parameters)
        {
            _logging.LogMessage(MessageImportance.High, format, parameters);
        }

        /// <summary>
        /// Writes the warning.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteWarning(string format, params object[] parameters)
        {
            _logging.LogWarning(format, parameters);
        }

        /// <summary>
        /// Writes the error.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteError(string format, params object[] parameters)
        {
            _logging.LogError(format, parameters);
        }

        /// <summary>
        /// Writes, debug level (does not show in release).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteDebug(string format, params object[] parameters)
        {
#if DEBUG
            _logging.LogMessage(MessageImportance.High, format, parameters);
#endif
        }
    }
}
