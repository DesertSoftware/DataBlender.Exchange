/* 
//  Copyright Desert Software Solutions, Inc 2014
//  Data Exchange Platform - Data Blender imports

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
using System.Dynamic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Linq;

using DataBlender.Dxp.Imports;
using DesertSoftware.Solutions.Extensions;

namespace DataBlender.Dxp
{
    /// <summary>
    /// Provides functionality to run an import package
    /// </summary>
    public class ImportManager
    {
        static private void NullLogger(int severity, string message) { }

        // execute a given import action
        static private void Import(ImportAction action, ImportPackage package, Action<int, string> log, dynamic context) {
            try {
                // run dependencies
                foreach (var dependency in action.Dependencies.Select(d => package[d]).Where(a => !(a ?? new ImportAction()).Executed))
                    try {
                        if (dependency != null) 
                            Import(dependency, package, log, context);
                    } catch {
                        if (action.BreakOnError) throw;
                    }

                // run the action
                log.Info("{0} started", action.Name);
                Type providerType = Type.GetType(action.ProviderType);
                ImportProvider provider = providerType != null
                    ? Activator.CreateInstance(providerType) as ImportProvider
                    : null;

                if (provider == null) {
                    if (providerType == null)
                        throw new Exception(string.Format("Import action provider does not exist. '{0}'", action.ProviderType));

                    throw new Exception(string.Format("Invalid import action provider: '{0}'. ", action.ProviderType));
                }

                // perform the actions outlined in the instructions
                provider.Import(action.Instructions, package.Data, log, context);
                action.Executed = true; // Provide an indicator for other references, update the action executed to true.
                log.Info("{0} completed", action.Name);
            } catch (Exception aex) {
                log.Error(aex.Message);
                log.Debug(aex, "");
                log.Info("{0} aborted", action.Name);
                if (action.BreakOnError) throw;
            }
        }

        /// <summary>
        /// Imports the specified import package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException">package</exception>
        static public void Import(ImportPackage package, Action<int, string> log = null, dynamic context = null) {
            if (package == null) throw new ArgumentNullException("package");

            log = log ?? NullLogger;    // ensure we have a logger of some sort
            log.Info("Import started");

            try {
                // instantiate each action actor and perform the import
                foreach (var action in package.Actions.Where(a => a.References == 0)) {
                    try {
                        Import(action, package, log, context);
                    } catch {
                        // exceptions are being handled in the Import method. Logging them here would duplicate information
                        if (action.BreakOnError) break;
                    }
                }

                log.Info("Import completed");
            } catch (Exception ex) {
                log.Error(ex.Message);
                log.Info("Import aborted");
            }
        }

        /// <summary>
        /// Imports the specified import package.
        /// </summary>
        /// <param name="importPackage">The import package.</param>
        /// <param name="dataSource">The data source.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException">importPackage</exception>
        /// <exception cref="System.ArgumentException">dataSource</exception>
        static public void Import(XElement importPackage, TextReader dataSource, Action<int, string> log = null, dynamic context = null) {
            if (importPackage == null) throw new ArgumentNullException("importPackage");
            if (dataSource == null) throw new ArgumentException("dataSource");

            log = log ?? NullLogger;    // ensure we have a logger of some sort

            try {
                Import(ImportPackage.Load(importPackage, dataSource), log, context);
            } catch (Exception ex) {
                log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Imports the specified import package.
        /// </summary>
        /// <param name="importPackage">The import package.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The import context information.</param>
        /// <exception cref="System.ArgumentNullException">intakePackage</exception>
        /// <exception cref="System.Exception"></exception>
        static public void Import(XElement importPackage, Action<int, string> log = null, dynamic context = null) {
            if (importPackage == null) throw new ArgumentNullException("importPackage");

            log = log ?? NullLogger;    // ensure we have a logger of some sort

            try {
                Import(ImportPackage.Load(importPackage), log, context);
            } catch (Exception ex) {
                log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Imports the specified stream package.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The import context information.</param>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        static public void Import(Stream stream, Action<int, string> log = null, dynamic context = null) {
            if (stream == null) throw new ArgumentNullException("stream");

            log = log ?? NullLogger;

            try {
                Import(XElement.Load(stream), log, context);
            } catch (Exception ex) {
                log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Imports the specified reader package.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The import context information.</param>
        /// <exception cref="System.ArgumentNullException">reader</exception>
        static public void Import(TextReader reader, Action<int, string> log = null, dynamic context = null) {
            if (reader == null) throw new ArgumentNullException("reader");

            log = log ?? NullLogger;

            try {
                Import(XElement.Load(reader), log, context);
            } catch (Exception ex) {
                log.Error(ex.Message);
            }
        }
    }
}
