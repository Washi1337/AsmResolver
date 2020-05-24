using System;

namespace AsmResolver.Tests.Runners
{
    public class RunnerException : Exception
    {
        public RunnerException(int exitCode, string errorOutput)
        {
            ExitCode = exitCode;
            ErrorOutput = errorOutput;
        }

        public int ExitCode
        {
            get;
        }

        public string ErrorOutput
        {
            get;
        }

        public override string Message
        {
            get
            {
                string truncatedOutput = ErrorOutput.Length > 255 ? ErrorOutput.Remove(252) + "... (truncated)" : ErrorOutput;
                return "The program exited with code " + ExitCode + ". Error output: " + truncatedOutput;
            }
        }
    }
}