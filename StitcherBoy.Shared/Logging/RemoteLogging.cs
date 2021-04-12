// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Logging
{
    using System;

    internal class RemoteLogging : MarshalByRefObject, ILogging
    {
        private readonly ILogging _logging;

        public RemoteLogging(ILogging logging)
        {
            _logging = logging;
        }

        public void Write(string format, params object[] parameters) => _logging.Write(format, parameters);

        public void WriteWarning(string format, params object[] parameters) => _logging.WriteWarning(format, parameters);

        public void WriteError(string format, params object[] parameters) => _logging.WriteError(format, parameters);

        public void WriteDebug(string format, params object[] parameters) => _logging.WriteDebug(format, parameters);
    }
}
