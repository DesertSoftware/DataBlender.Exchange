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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using DesertSoftware.Solutions.Extensions;
using DesertSoftware.Solutions.IO;

namespace DataBlender.Dxp.Imports
{
    /// <summary>
    /// The DataReader class provides functionality to read data declared in a data element of
    /// an import document.
    /// 
    /// <!-- A snippet of data from a TOA export -->
    /// <code>
    ///  <data id="{id}" content="System.Data.Csv" source="inline">
    ///  <![CDATA[
    ///    ID,EQUIPNUM,APPRTYPE,TANK,SAMPLEDATE,SAMPLENUM
    ///    0,1234,LTC,1,1/1/2000,3
    ///  ]]>
    ///  </data>
    /// </code>
    /// </summary>
    public class DataReader
    {
        protected IEnumerable<dynamic> data;

        private DataReader() { }

        /// <summary>
        /// Loads the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="detectDateTimeValues">if set to <c>true</c> detect date time values.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">data</exception>
        static public DataReader Load(XElement data) {
            if (data == null) throw new ArgumentNullException("data");

            var dataReader = new DataReader();

            // a "data" element is comprised of the following attributes (id, content, source, and options) and the data contents
            // the "id" attribute assigns a name to the data element. It is optional and should be unique when multiple data 
            // elements are defined. 

            // <data id="{id}" content="System.Data.Csv" source="inline" options="DetectDateTime">
            //   <!-- A snippet of data from a TOA export -->
            //   <![CDATA[
            //     ID,EQUIPNUM,APPRTYPE,TANK,SAMPLEDATE,SAMPLENUM
            //     0,1234,LTC,1,1/1/2000,3
            //   ]]>
            // </data>

            // the "content" attribute specifies the format of the data, such as System.Data.Csv. Csv is assumed if not present
            string content = data.Attribute("content") != null ? data.Attribute("content").Value : "System.Data.Csv";

            // the "source" attribute specifies where to obtain the data from. If not declared, inline is assumed
            string source = data.Attribute("source") != null ? data.Attribute("source").Value : "inline";

            // the options attribute provides functionality to pass additional behavior options to provider reading the data
            string options = data.Attribute("options") != null ? data.Attribute("options").Value : "";

            switch (source.ToLower()) {
                case "":
                case "inline":
                    switch (content.ToLower()) {
                        case "system.data.csv":
                        case "csv":
                            dataReader.data = new CsvReader(new StringReader(data.Value.Trim())).ReadAllLines(options);
                            break;

                        default:
                            dataReader.data = new CsvReader(new StringReader(data.Value.Trim())).ReadAllLines(options);
                            break;
                    }
                    break;

                default:
                    switch (content.ToLower()) {
                        case "system.data.csv":
                        case "csv":
                            dataReader.data = new CsvReader(source).ReadAllLines(options);
                            break;

                        default:
                            dataReader.data = new CsvReader(source).ReadAllLines(options);
                            break;
                    }
                    break;
            }

            return dataReader;
        }

        /// <summary>
        /// Returns an enumerator containing each row of data.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<dynamic> DataEnumerator() {
            foreach (var item in this.data)
                yield return item;
        }
    }
}
