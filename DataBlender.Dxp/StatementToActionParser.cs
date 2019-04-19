/* 
//  Copyright Desert Software Solutions, Inc 2018
//  Data Exchange Platform - Data Blender parsers

// Copyright (c) 2014 Desert Software Solutions Inc. All rights reserved.

// THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, NON-INFRINGEMENT AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  

// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
// OF THE USE OF THIS SOFTWARE, WHETHER OR NOT SUCH DAMAGES WERE FORESEEABLE AND EVEN IF THE AUTHOR IS ADVISED 
// OF THE POSSIBILITY OF SUCH DAMAGES. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using DesertSoftware.Solutions.Dynamix;
using DesertSoftware.Solutions.Dynamix.Extensions;

namespace DataBlender.Dxp
{
    static public class StatementToActionParser
    {
        private const string LOG_STATEMENT = "console.log";
        private const string PRINT_STATEMENT = "console.print";
        private const string WARN_STATEMENT = "console.warn";
        private const string ERROR_STATEMENT = "console.error";
        private const string DEBUG_STATEMENT = "console.debug";

        /// <summary>
        /// Returns the action expressed in the statement.
        /// </summary>
        /// <param name="stmt">The statement.</param>
        /// <param name="console">The console.</param>
        /// <param name="defaultAction">The default action.</param>
        /// <returns></returns>
        static public Action ToAction(this XElement stmt, Terminal console, Func<XElement, Terminal, Action> defaultAction) {
            if (stmt == null) return null;

            switch (stmt.Name.LocalName.ToLower()) {
                case LOG_STATEMENT:
                case PRINT_STATEMENT:
                    return () => console.Print(stmt.Value);

                case WARN_STATEMENT:
                    return () => console.Warn(stmt.Value);

                case ERROR_STATEMENT:
                    return () => console.Error(stmt.Value);

                case DEBUG_STATEMENT:
                    return () => console.Debug(stmt.Value);

                default: return defaultAction(stmt, console);
            }
        }

        /// <summary>
        /// Returns the typed action expressed in the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stmt">The statement.</param>
        /// <param name="console">The console.</param>
        /// <param name="defaultAction">The default action.</param>
        /// <returns></returns>
        static public Action<T> ToAction<T>(this XElement stmt, Terminal console, Func<XElement, Terminal, Action<T>> defaultAction) {
            if (stmt == null) return null;

            switch (stmt.Name.LocalName.ToLower()) {
                case LOG_STATEMENT:
                case PRINT_STATEMENT:
                    return (data) => console.Print(stmt.Value, data.ToValueBag());

                case WARN_STATEMENT:
                    return (data) => console.Warn(stmt.Value, data.ToValueBag());

                case ERROR_STATEMENT:
                    return (data) => console.Error(stmt.Value, data.ToValueBag());

                case DEBUG_STATEMENT:
                    return (data) => console.Debug(stmt.Value, data.ToValueBag());

                default: return defaultAction(stmt, console);
            }
        }
    }
}
