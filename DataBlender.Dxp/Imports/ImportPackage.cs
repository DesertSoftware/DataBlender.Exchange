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
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataBlender.Dxp.Imports
{
    /// <summary>
    /// The ImportPackage class represents the known parts of an import package presented
    /// to an import process.
    /// </summary>
    public class ImportPackage
    {
        private XElement package = null;
        private List<ImportAction> actions = new List<ImportAction>();
        private Dictionary<string, ImportAction> indexedActions = new Dictionary<string, ImportAction>(StringComparer.CurrentCultureIgnoreCase);
        private List<XElement> data = new List<XElement>();

        /// <summary>
        /// Loads the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">package</exception>
        /// <exception cref="System.Exception">
        /// Import package does not contain an instructions element
        /// or
        /// Import package does not contain any data elements
        /// </exception>
        static public ImportPackage Load(XElement package) {
            if (package == null) throw new ArgumentNullException("package");
            var instance = new ImportPackage();
            XAttribute providerAttribute = null;
            XElement instructionsElement = null;

            instance.package = package;

            // sniff the package element to determine if we are importing a singular package
            // a singular package is indicated when a provider is specified at the root element
            // e.g. <import provider="provider.type, provider">
            providerAttribute = package.Attribute("provider");
            instructionsElement = package.Element("instructions");

            if (providerAttribute != null && instructionsElement != null)
                instance.actions.Add(new ImportAction(package));

            if (instance.actions.Count == 0)
               instance.actions.AddRange(package.Elements()
                   .Where(e => e.Name.LocalName.ToLower() != "data")
                   .Select(e => new ImportAction(e)));

            // count up the dependency references. 
            foreach (var action in instance.actions)
                instance.indexedActions.Add(action.Name, action);

            foreach (var action in instance.actions)
                foreach (var dependency in action.Dependencies)
                    if (instance.indexedActions.ContainsKey(dependency))
                        instance.indexedActions[dependency].References += 1;
                    else
                        throw new Exception(string.Format("Action dependency '{0}::{1}' is not defined", action.Name, dependency));

            // find the data payloads
            instance.data.AddRange(package.Elements("data"));
            if (instance.data.Count == 0) throw new Exception("Import package does not contain any data elements");

            return instance;
        }

        private ImportPackage() {}

        /// <summary>
        /// Gets the <see cref="ImportAction"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="ImportAction"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public ImportAction this[string index] {
            get { try { return this.indexedActions[index]; } catch { return null; } }
        }

        /// <summary>
        /// Gets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public IEnumerable<ImportAction> Actions { get { return this.actions;  } }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public XElement[] Data { get { return this.data.ToArray(); } }
    }
}
