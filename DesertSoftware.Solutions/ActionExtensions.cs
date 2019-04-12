//
//  Copyright 2015, Desert Software Solutions Inc.
//    ActionExtensions.cs:
//      https://github.com/DesertSoftware/Solutions/blob/master/Solutions/Extensions/ActionExtensions.cs
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesertSoftware.Solutions.Extensions
{
    /// <summary>
    /// Provides extended functions to decorate an action method having the form of
    /// Action<int, string> with logging verbs. These are typically used when using
    /// loosely coupled libraries that support emitting information that is typically
    /// going to be logged by the provider of the action without imposing a specific
    /// logging dependency.
    /// </summary>
    static public class ActionExtensions
    {
        private const int DEBUG = 0;
        private const int INFO = 1;
        private const int WARN = 2;
        private const int ERROR = 3;
        private const int FATAL = 4;

        /// <summary>
        /// Invokes the specified action with the DEBUG verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        static public void Debug(this Action<int, string> action, string message) {
            action(DEBUG, message);
        }

        /// <summary>
        /// Invokes the specified action with the DEBUG verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        static public void Debug(this Action<int, string> action, Exception exception, string message) {
            action(DEBUG, string.Format("{0} ({1})",  message, exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the DEBUG verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Debug(this Action<int, string> action, string format, params object[] args) {
            action(DEBUG, string.Format(format, args));
        }

        /// <summary>
        /// Invokes the specified action with the DEBUG verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Debug(this Action<int, string> action, Exception exception, string format, object[] args) {
            action(DEBUG, string.Format("{0} ({1})", string.Format(format, args), exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the INFO verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        static public void Info(this Action<int, string> action, string message) {
            action(INFO, message);
        }

        /// <summary>
        /// Invokes the specified action with the INFO verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        static public void Info(this Action<int, string> action, Exception exception, string message) {
            action(INFO, string.Format("{0} ({1})",  message, exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the INFO verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Info(this Action<int, string> action, string format, params object[] args) {
            action(INFO, string.Format(format, args));
        }

        /// <summary>
        /// Invokes the specified action with the INFO verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Info(this Action<int, string> action, Exception exception, string format, object[] args) {
            action(INFO, string.Format("{0} ({1})", string.Format(format, args), exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the WARN verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        static public void Warn(this Action<int, string> action, string message) {
            action(WARN, message);
        }

        /// <summary>
        /// Invokes the specified action with the WARN verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        static public void Warn(this Action<int, string> action, Exception exception, string message) {
            action(WARN, string.Format("{0} ({1})",  message, exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the WARN verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Warn(this Action<int, string> action, string format, params object[] args) {
            action(WARN, string.Format(format, args));
        }

        /// <summary>
        /// Invokes the specified action with the WARN verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Warn(this Action<int, string> action, Exception exception, string format, object[] args) {
            action(WARN, string.Format("{0} ({1})", string.Format(format, args), exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the ERROR verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        static public void Error(this Action<int, string> action, string message) {
            action(ERROR, message);
        }

        /// <summary>
        /// Invokes the specified action with the ERROR verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        static public void Error(this Action<int, string> action, Exception exception, string message) {
            action(ERROR, string.Format("{0} ({1})",  message, exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the ERROR verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Error(this Action<int, string> action, string format, params object[] args) {
            action(ERROR, string.Format(format, args));
        }

        /// <summary>
        /// Invokes the specified action with the ERROR verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Error(this Action<int, string> action, Exception exception, string format, object[] args) {
            action(ERROR, string.Format("{0} ({1})", string.Format(format, args), exception.ToString()));
        }
        
        /// <summary>
        /// Invokes the specified action with the FATAL verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        static public void Fatal(this Action<int, string> action, string message) {
            action(FATAL, message);
        }

        /// <summary>
        /// Invokes the specified action with the FATAL verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        static public void Fatal(this Action<int, string> action, Exception exception, string message) {
            action(DEBUG, string.Format("{0} ({1})",  message, exception.ToString()));
        }

        /// <summary>
        /// Invokes the specified action with the FATAL verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Fatal(this Action<int, string> action, string format, params object[] args) {
            action(DEBUG, string.Format(format, args));
        }

        /// <summary>
        /// Invokes the specified action with the FATAL verb. 
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        static public void Fatal(this Action<int, string> action, Exception exception, string format, object[] args) {
            action(DEBUG, string.Format("{0} ({1})", string.Format(format, args), exception.ToString()));
        }
    }
}
