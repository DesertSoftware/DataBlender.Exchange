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
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataBlender.Dxp.Exports
{
    /// <summary>
    /// The ExportPackage class represents the known parts of an export package presented
    /// to an export process.
    /// </summary>
    public class ExportPackage
    {
        private XElement package = null;
        private XAttribute providerAttribute = null;
        private XElement instructionsElement = null;

        /// <summary>
        /// Loads the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">package</exception>
        /// <exception cref="System.Exception">Export element is missing the provider attribute
        /// or
        /// Export package does not contain an instructions element
        static public ExportPackage Load(XElement package) {
            if (package == null) throw new ArgumentNullException("package");
            var instance = new ExportPackage();

            instance.package = package;
            instance.providerAttribute = package.Attribute("provider");
            instance.instructionsElement = package.Element("instructions");

            if (instance.providerAttribute == null) throw new Exception("Export element is missing the provider attribute");
            if (instance.instructionsElement == null) throw new Exception("Export package does not contain an instructions element");

            return instance;
        }

        private ExportPackage() { }

        /// <summary>
        /// Gets the type of the provider.
        /// </summary>
        /// <value>
        /// The type of the provider.
        /// </value>
        public string ProviderType { get { return this.providerAttribute.Value; } }

        /// <summary>
        /// Gets the instructions.
        /// </summary>
        /// <value>
        /// The instructions.
        /// </value>
        public XElement Instructions { get { return this.instructionsElement; } }
    }
}
