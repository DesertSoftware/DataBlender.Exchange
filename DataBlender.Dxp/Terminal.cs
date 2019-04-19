/* 
//  Copyright Desert Software Solutions, Inc 2018
//  Data Exchange Platform - Data Blender console

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

using DesertSoftware.Solutions.Dynamix;
using DesertSoftware.Solutions.Extensions;

namespace DataBlender.Dxp
{
    /// <summary>
    /// The Terminal class represents a resource to which a data exchange process can write to
    /// that ultimately would be surfaced to a log or console display
    /// </summary>
    public class Terminal
    {
        private Action<int, string> logger = (severity, message) => { };    // default logger

        /// <summary>
        /// Initializes a new instance of the <see cref="Terminal"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public Terminal(Action<int, string> logger) {
            this.logger = logger;
        }

        /// <summary>
        /// Prints the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="logger">The logger.</param>
        static public void Print(string text, Action<int, string> logger) {
            logger.Info(text);
        }

        /// <summary>
        /// Prints the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        /// <param name="logger">The logger.</param>
        static public void Print(string text, ValueBag values, Action<int, string> logger) {
            logger.Info(Interpolator.Interpolate(text, values));
        }

        /// <summary>
        /// Prints the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Print(string text) {
            Terminal.Print(text, this.logger);
        }

        /// <summary>
        /// Prints the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        public void Print(string text, ValueBag values) {
            Terminal.Print(text, values, this.logger);
        }

        /// <summary>
        /// Prints the specified text warning.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="logger">The logger.</param>
        static public void Warn(string text, Action<int, string> logger) {
            logger.Warn(text);
        }

        /// <summary>
        /// Prints the specified text warning.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        /// <param name="logger">The logger.</param>
        static public void Warn(string text, ValueBag values, Action<int, string> logger) {
            logger.Warn(Interpolator.Interpolate(text, values));
        }

        /// <summary>
        /// Prints the specified text warning.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Warn(string text) {
            Terminal.Warn(text, this.logger);
        }

        /// <summary>
        /// Prints the specified text warning.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        public void Warn(string text, ValueBag values) {
            Terminal.Warn(text, values, this.logger);
        }

        /// <summary>
        /// Prints the specified debug text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="logger">The logger.</param>
        static public void Debug(string text, Action<int, string> logger) {
            logger.Debug(text);
        }

        /// <summary>
        /// Prints the specified debug text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        /// <param name="logger">The logger.</param>
        static public void Debug(string text, ValueBag values, Action<int, string> logger) {
            logger.Debug(Interpolator.Interpolate(text, values));
        }

        /// <summary>
        /// Prints the specified debug text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Debug(string text) {
            Terminal.Debug(text, this.logger);
        }

        /// <summary>
        /// Prints the specified debug text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        public void Debug(string text, ValueBag values) {
            Terminal.Debug(text, values, this.logger);
        }

        /// <summary>
        /// Prints the specified error text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="logger">The logger.</param>
        static public void Error(string text, Action<int, string> logger) {
            logger.Error(text);
        }

        /// <summary>
        /// Prints the specified error text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        /// <param name="logger">The logger.</param>
        static public void Error(string text, ValueBag values, Action<int, string> logger) {
            logger.Error(Interpolator.Interpolate(text, values));
        }

        /// <summary>
        /// Prints the specified error text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Error(string text) {
            Terminal.Error(text, this.logger);
        }

        /// <summary>
        /// Prints the specified error text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="values">The values.</param>
        public void Error(string text, ValueBag values) {
            Terminal.Error(text, values, this.logger);
        }
    }
}
