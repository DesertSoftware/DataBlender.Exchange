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
    /// The ImportAction class represents the known parts of an import action plan described
    /// in an import package.
    /// </summary>
    public class ImportAction
    {
        private XElement source = null;
        private bool breakOnError = true;
        private List<string> dependencies = new List<string>();
        private XAttribute providerAttribute = null;    // must exist
        private XElement instructionsElement = null;    // must exist
        private int references = 0;
        private bool executed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportAction"/> class.
        /// </summary>
        internal ImportAction() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportAction"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="System.ArgumentNullException">source</exception>
        /// <exception cref="System.Exception">
        /// Import action does not contain an instructions element
        /// </exception>
        public ImportAction(XElement source) {
            if (source == null) throw new ArgumentNullException("source");
            this.source = source;

            this.providerAttribute = source.Attribute("provider");
            this.instructionsElement = source.Element("instructions");

            if (this.providerAttribute == null) throw new Exception(string.Format("{0} element is missing the provider attribute", source.Name.LocalName));
            if (this.instructionsElement == null) throw new Exception("Import action does not contain an instructions element");

            XAttribute breakOnErrorAttribute = this.source.Attribute("breakOnError");
            if (breakOnErrorAttribute != null)
                bool.TryParse(breakOnErrorAttribute.Value, out this.breakOnError);

            XAttribute dependsOnAttribute = this.source.Attribute("dependsOn");
            if (dependsOnAttribute != null)
                dependencies.AddRange(dependsOnAttribute.Value
                    .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()));
        }

        /// <summary>
        /// Gets a value indicating whether to break on fatal error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if break on error; otherwise, <c>false</c>.
        /// </value>
        public bool BreakOnError { get { return this.breakOnError; } }

        /// <summary>
        /// Gets the dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        public IEnumerable<string> Dependencies { get { return this.dependencies; } }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public XElement Description { get { return this.source.Element("description"); } }

        /// <summary>
        /// Gets the instructions.
        /// </summary>
        /// <value>
        /// The instructions.
        /// </value>
        public XElement Instructions { get { return this.instructionsElement; } }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get { return this.source.Name.LocalName; } }

        /// <summary>
        /// Gets the type of the provider.
        /// </summary>
        /// <value>
        /// The type of the provider.
        /// </value>
        public string ProviderType { get { return this.providerAttribute.Value; } }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ImportAction"/> is executed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if executed; otherwise, <c>false</c>.
        /// </value>
        internal bool Executed {
            get { return this.executed; }
            set { this.executed = true; }
        }

        /// <summary>
        /// Gets or sets the number times other actions reference this action typically via a dependsOn attribute.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        internal int References { 
            get { return this.references; }
            set { this.references = value; }
        }
    }
}
