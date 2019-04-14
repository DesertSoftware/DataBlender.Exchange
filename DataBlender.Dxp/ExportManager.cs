/* 
//  Copyright Desert Software Solutions, Inc 2014
//  Data Exchange Platform - Data Blender exports

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

using DataBlender.Dxp.Exports;
using DesertSoftware.Solutions.Extensions;

namespace DataBlender.Dxp
{
    /// <summary>
    /// Provides functionality to run an export package
    /// </summary>
    public class ExportManager
    {
        static private void NullLogger(int severity, string message) { }

        /// <summary>
        /// Exports the specified export package.
        /// </summary>
        /// <param name="exportPackage">The export package.</param>
        /// <param name="writer">The writer to which the data is exported to.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The export context information.</param>
        /// <exception cref="System.ArgumentNullException">exportPackage</exception>
        /// <exception cref="System.Exception"></exception>
        static public void Export(XElement exportPackage, TextWriter writer, Action<int, string> log = null, dynamic context = null) {
            if (exportPackage == null) throw new ArgumentNullException("exportPackage");

            log = log ?? NullLogger;    // ensure we have a logger of some sort
            log.Info("Export started");

            try {
                var package = ExportPackage.Load(exportPackage);
                Type providerType = Type.GetType(package.ProviderType);
                ExportProvider provider = providerType != null
                    ? Activator.CreateInstance(providerType) as ExportProvider
                    : null;

                if (provider == null) {
                    if (providerType == null)
                        throw new Exception(string.Format("Provider does not exist. '{0}'", package.ProviderType));

                    throw new Exception(string.Format("Invalid export provider: '{0}'. ", package.ProviderType));
                }

                provider.Export(package.Instructions, writer, log, context);
                log.Info("Export completed");
            } catch (Exception ex) {
                log.Error(ex.Message);
                log.Info("Export aborted");
                return;
            }
        }

        /// <summary>
        /// Exports the specified stream package.
        /// </summary>
        /// <param name="stream">The stream to load the package from.</param>
        /// <param name="writer">The writer to which the data is exported to.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The export context information.</param>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        static public void Export(Stream stream, TextWriter writer, Action<int, string> log = null, dynamic context = null) {
            if (stream == null) throw new ArgumentNullException("stream");

            log = log ?? NullLogger;

            try {
                Export(XElement.Load(stream), writer, log, context);
            } catch (Exception ex) {
                log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Exports the specified reader package.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="writer">The writer to which the data is exported to.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The export context information.</param>
        /// <exception cref="System.ArgumentNullException">reader</exception>
        static public void Export(TextReader reader, TextWriter writer, Action<int, string> log = null, dynamic context = null) {
            if (reader == null) throw new ArgumentNullException("reader");

            log = log ?? NullLogger;

            try {
                Export(XElement.Load(reader), writer, log, context); 
            } catch (Exception ex) {
                log.Error(ex.Message);
            }
        }

    }
}
