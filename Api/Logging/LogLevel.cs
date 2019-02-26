namespace ColiseeSharp.Api.Logging {
    public enum LogLevel {
        /// <summary>For messages intended to trace which line of code the program is on and what the state of the program is.</summary>
        Trace,
        
        /// <summary>For messages intended to debug issues in the code.</summary>
        Debug,

        /// <summary>For non-issue messages intended for the user.</summary>
        Info,

        /// <summary>For issue messages that aren't necessarily errors but may result in errors.</summary>
        Warn,

        /// <summary>For errors that occur during execution, but do not necessarily cause the program to break.</summary>
        Error,

        /// <summary>For fatal errors in the program.</summary>
        Critical
    }
}