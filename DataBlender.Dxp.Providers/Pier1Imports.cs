using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataBlender.Dxp.Imports;

namespace DataBlender.Dxp.Providers
{
    public class Pier1Imports : ImportProvider
    {
        /// <summary>
        /// Imports the specified data using the specified intructions.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="data">The data.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The import context information.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Import(System.Xml.Linq.XElement instructions, System.Xml.Linq.XElement[] data, Action<int, string> log = null, dynamic context = null) {
            throw new NotImplementedException();
        }
    }
}
