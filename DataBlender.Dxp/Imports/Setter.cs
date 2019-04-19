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
using System.Text.RegularExpressions;
using System.Xml.Linq;

using DesertSoftware.Solutions.Dynamix;
using DesertSoftware.Solutions.Extensions;

namespace DataBlender.Dxp.Imports
{
    /// <summary>
    /// The Setter class provides functionality to parse and execute an import let statement
    /// <code>
    ///   
    ///   <!-- provide support for *required* assignments -->
    ///   
    ///   <let TargetColumn="sourceColumn" default="Yes">
    ///     <case>  <!-- when source attribute is not declared assume source of TargetColumn=SourceColumn -->
    ///       <when source="TRUE" value="Yes" />
    ///       <when source="YES" value="Yes" />
    ///       <when source="FALSE" value="No" />
    ///       <when source="NO" value="No" />
    ///     </case>
    ///   </let>
    ///   
    ///   <let TargetColumn="{Bank Phase} ({Serial Number})" default="">
    ///     <case source="Serial Number">
    ///       <when source="" value="" />
    ///     </case>
    ///     <case source="Bank Phase">
    ///       <when source="" value="" />
    ///     </case>
    ///   </let>
    ///   
    ///   <let HaveMotor="Equipment Type" default="false">
    ///     <case>
    ///       <when source="Motor" value="true" />
    ///       <else value="false">
    ///     </case>
    ///   </let>
    ///   
    ///   <let BankName="'Bank A'" />
    ///   
    ///   <!-- classic expression (same functionality just doesn't read as well) -->
    ///   <let TargetColumn="sourceColumn" default="Yes">
    ///     <values>
    ///       <add text="TRUE" value="Yes" />
    ///       <add text="YES" value="Yes" />
    ///       <add text="FALSE" value="No" />
    ///       <add text="NO" value="No" />
    ///     </values>
    ///   </let>
    /// 
    /// </code>
    /// </summary>
    public class Setter
    {
        static private Regex expression = new Regex(@"\{(.*?)\}");
        private XElement letStatement;
        private Dictionary<string, List<string[]>> valueLists = new Dictionary<string, List<string[]>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Setter"/> class.
        /// </summary>
        /// <param name="statement">The let statement.</param>
        public Setter(XElement statement) {
            this.letStatement = statement;

            var valueItem = "add";
            var valueSource = "text";
            var thenValue = "value";
            IEnumerable<XElement> values = statement.Elements("values");

            // a values list can be expressed as either a values or case element
            if (values == null || values.Count() == 0) {
                valueItem = "when";
                valueSource = "source";
                values = statement.Elements("case");
            }

            // gather the value items
            // we may have more than one case/values element
            if (values != null)
                foreach (var caseStatement in values) {
                    var source = caseStatement.Attribute(valueSource).GetValueOrDefault("*");

                    foreach (var value in caseStatement.Elements(valueItem)) {
                        // when source="expression" value="value"
                        // add text="expression" value="value"
                        if (!this.valueLists.ContainsKey(source))
                            this.valueLists[source] = new List<string[]>();

                        this.valueLists[source].Add(new string[2] { value.Attribute(valueSource).GetValueOrDefault(), value.Attribute(thenValue).GetValueOrDefault() });
                    }
                }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setter"/> class.
        /// </summary>
        /// <param name="statement">The let statement.</param>
        public Setter(string statement) : this(XElement.Parse(statement)) { }

        private XAttribute Assignment {
            get { return this.letStatement.Attributes().Where((x) => { return !x.Name.LocalName.Equals("default"); }).FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the declared default value. 
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public string Default {
            get {
                XAttribute def = this.letStatement.Attribute("default");

                return def != null ? def.Value : "";
            }
        }

        /// <summary>
        /// Gets the target attribute to assign to.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public string Target {
            get {
                XAttribute property = Assignment;

                return property != null ? property.Name.LocalName : "";
            }
        }

        /// <summary>
        /// Gets the source attribute to assign from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source {
            get {
                XAttribute property = Assignment;

                return property != null ? property.Value : "";
            }
        }

        private dynamic ResolveToConstrainedValue(string source, dynamic sourceValue) {
            if (this.valueLists.ContainsKey(source)) {
                bool lookupFound = false;
                string strValue = sourceValue.ToString();

                foreach (var listItem in this.valueLists[source])
                    if (listItem[0].Equals(strValue, StringComparison.CurrentCultureIgnoreCase)) {
                        lookupFound = true;
                        sourceValue = listItem[1];
                    }

                // <let targetColumn="{sourceColumn1} ({sourceColumn2})"
                if (!lookupFound)
                    sourceValue = this.Default;
            }

            return sourceValue;
        }

        /// <summary>
        /// Sets the value of the targetValues.Target attribute equal to the value of the sourceValues.Source attribute.
        /// </summary>
        /// <param name="sourceValues">The source values to assign from.</param>
        /// <param name="targetValues">The target values to assin to.</param>
        /// <param name="evaluator">An evaluator function providing an opportunity to inspect or modify the final value to assign.</param>
        public void Assign(ValueBag sourceValues, ValueBag targetValues, Func<string, string, dynamic, dynamic> evaluator = null) {
            evaluator = evaluator ?? ((source, target, value) => value);

            // find all of the source names delimited within {} braces. 
            // scan the tokens, if source element ends in } then we have a column source value
            if (Interpolator.Interpolations(this.Source).Count == 0) {
                // if this is a literal assignment the source will begin with a single quote
                var sourceValue = this.Source.StartsWith("'")
                    ? this.Source.Trim("'".ToCharArray())
                    : sourceValues.GetValue(this.Source) ?? this.Default;

                // if we have a global values clause resolve the sourceValue to a value in the list
                sourceValue = ResolveToConstrainedValue("*", sourceValue);

                // if we have a source specific values clause; resolve the sourceValue to a value in the list
                sourceValue = ResolveToConstrainedValue(this.Source, sourceValue);

                // finally bind the result of the evaluator to our sources 
                targetValues.SetValue(this.Target, evaluator(this.Source, this.Target, sourceValue));
            } else
                targetValues.SetValue(this.Target, evaluator(this.Source, this.Target, 
                    Interpolator.Interpolate(this.Source, sourceValues, (src, value) => {
                        // if we have a global values clause resolve the sourceValue to a value in the list
                        value = ResolveToConstrainedValue("*", value ?? this.Default);

                        // if we have a source specific values clause; resolve the sourceValue to a value in the list
                        value = ResolveToConstrainedValue(src, value);

                        // finally bind the result of the evaluator 
                        return evaluator(src, this.Target, value);
                    })));

            //// {colA} -- {colB} ==> colA}, -- , colB}
            //var sources = new Dictionary<string, dynamic>();
            //var results = expression.Matches(this.Source);
            //foreach (Match match in results) {
            //    var token = match.Value.Trim('{', '}');
                
            //    if (!sources.ContainsKey(token))
            //        sources.Add(token, null);
            //} 
            
            //// If no braces present then use the source statement as is
            //if (sources.Count == 0)
            //    sources.Add(this.Source, null);

            //// bind the source values to our sources
            //foreach (var source in sources.Keys.ToList()) {
            //    // if this is a literal assignment the source will begin with a single quote
            //    var sourceValue = source.StartsWith("'") 
            //        ? source.Trim("'".ToCharArray()) 
            //        : sourceValues.GetValue(source) ?? this.Default;

            //    // if we have a global values clause resolve the sourceValue to a value in the list
            //    sourceValue = ResolveToConstrainedValue("*", sourceValue);

            //    // if we have a source specific values clause; resolve the sourceValue to a value in the list
            //    sourceValue = ResolveToConstrainedValue(source, sourceValue);

            //    // finally bind the result of the evaluator to our sources 
            //    sources[source] = evaluator(source, this.Target, sourceValue);
            //}

            //// assign the value
            //if (sources.Count > 1) {
            //    string interpolation = this.Source;

            //    foreach (var source in sources.Keys)
            //        interpolation = interpolation.Replace(string.Concat("{", source, "}"), string.Format("{0}", sources[source]));

            //    targetValues.SetValue(this.Target, interpolation);
            //} else
            //    targetValues.SetValue(this.Target, sources[this.Source]);

            //targetValues.SetValue(this.Target, Console.Interpolate(this.Source, sourceValues, (src, value) => {
            //        // if we have a global values clause resolve the sourceValue to a value in the list
            //        value = ResolveToConstrainedValue("*", value ?? this.Default);

            //        // if we have a source specific values clause; resolve the sourceValue to a value in the list
            //        value = ResolveToConstrainedValue(src, value);

            //        // finally bind the result of the evaluator 
            //        return evaluator(src, this.Target, value);
            //    }));
        }
    }
}
