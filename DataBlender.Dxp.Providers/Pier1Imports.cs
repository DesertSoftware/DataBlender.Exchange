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

using DataBlender.Dxp.Imports;
using DesertSoftware.Solutions.Dynamix;
using DesertSoftware.Solutions.Extensions;

namespace DataBlender.Dxp.Providers
{
    public class LabResult
    {
        public int ResultID { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
    }

    public class Importer
    {
        public virtual void Import(ValueBag data) {
            throw new NotImplementedException();
        }

        public virtual void Import(ValueBag data, int rowNumber) {
            throw new NotImplementedException();
        }
    }

    public class LabResultImporter : Importer
    {
        Action<int, string> log;
        private List<Setter> setters;

        public LabResultImporter(List<Setter> setters, Action<int, string> log) {
            this.log = log;
            this.setters = setters;
        }

        public override void Import(ValueBag data, int rowNumber) {
            var labResult = new LabResult();
            var targetValues = new ValueBag(labResult);

            //                labResult.EquipmentID = asset.EquipmentID;

            // perform the value assignments
            foreach (var setter in setters) {
                try {
                    setter.Assign(data, targetValues);
                } catch (Exception ex) {
                    log.Error(ex.Message);
                }
            }

            log.Info("{0:0000#}: {1} (ResultID: {2})", rowNumber, labResult.Name, labResult.ResultID);
            //// update the lab result using control number as index if it already exists
            //// otherwise just insert it into the database
            //try {
            //    LabManager.ImportLabResult(labResult);
            //    log.Info(string.Format("{0:00000#}: Imported lab result {1}: {2} {3} @ {4}",
            //        rowNumber, labResult.ControlNum, labResult.EquipType, asset.Name, asset.LocationName));
            //} catch (Exception ex) {
            //    log.Error(string.Format("{0:00000#}: Lab result {1}: import failed. {2} {3} @ {4}",
            //        rowNumber, labResult.ControlNum, labResult.EquipType, asset.Name, asset.LocationName));
            //    log.Error(ex.Message);
            //}
        }
    }

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
        public override void Import(XElement instructions, XElement[] data, Action<int, string> log = null, dynamic context = null) {
            log.Info("Running ...");
            if (instructions == null) throw new ArgumentNullException("instructions");
            if (data == null) throw new ArgumentNullException("data");
            if (log == null) throw new ArgumentNullException("log");

            var console = new Terminal(log);

            // compile into an actionable list
            List<Action> actions = new List<Action>();

            foreach (var statement in instructions.Elements()) {
                switch (statement.Name.LocalName.ToLower()) {
                    case "console.print":
                        actions.Add(() => console.Print(statement.Value));
                        break;

                    case "console.warn":
                        actions.Add(() => console.Warn(statement.Value));
                        break;

                    case "console.error":
                        actions.Add(() => console.Error(statement.Value));
                        break;

                    case "console.debug":
                        actions.Add(() => console.Debug(statement.Value));
                        break;

                    case "foreach":
                        actions.Add(() => {
                            List<Action> forEachActions = new List<Action>();
                            List<Importer> importers = new List<Importer>();

                            // the "foreach" statment is a fairly complex definition to parse
                            // it contains the instructions on which fields and method are to be used to lookup the thing
                            // it also specifies the data element to be used as input

                            //// the "asset" attribute specifies which fields and method to use to lookup the equipment item
                            //if (foreachStatement.Attribute("asset") == null) {
                            //    log.Error("Missing asset attribute. A foreach element must contain an asset attribute.");
                            //    continue;
                            //}

                            //var assetFinder = new EquipmentFinder(foreachStatement.Attribute("asset").Value, organizationID);

                            // the "in" or "rowIn" attribute specifies which data element contains the source data to update from
                            var inAttribute = statement.Attribute("in", "rowIn");

                            if (inAttribute == null) {
                                log.Warn("Missing 'in' attribute. Defaulting to first data element");
                            }

                            // locate the data element that contains the source data to update from
                            XElement dataSource = null;

                            if (inAttribute != null) {
                                foreach (var item in data) {
                                    if (item.Attribute("id") != null && item.Attribute("id").Value.ToLower() == inAttribute.Value.ToLower()) {
                                        dataSource = item;
                                        break;
                                    }
                                }
                            } else
                                dataSource = data.FirstOrDefault();

                            if (dataSource == null) {
                                log.Error(string.Format("Unable to determine the data set to use. '{0}'", inAttribute != null ? inAttribute.Value : ""));
                                return; // continue;
                            }

                            // A foreach element contains a collection of import elements that provide instructions to perform an import operation
                            foreach (var stmt in statement.Elements("console.print", "console.warn", "console.error", "console.debug", "import")) {
                                switch (stmt.Name.LocalName.ToLower()) {
                                    case "console.print":
                                        forEachActions.Add(() => console.Print(stmt.Value));
                                        break;

                                    case "import":
                                        forEachActions.Add(() => {
                                            var importStatement = stmt;
                                            // every import element must minimally define an into attribute
                                            var intoAttribute = importStatement.Attribute("into");

                                            if (intoAttribute == null) {
                                                log.Error("Missing into attribute. An import element must contain an 'into' attribute.");
                                                return; // continue;
                                            }

                                            switch (intoAttribute.Value.ToLower()) {
                                                case "alarm":
                                                case "labresult":
                                                    // import lab result
                                                    // An import element contains a collection of let elements that provide instructions as to which data columns
                                                    // are to be assigned to which lab result properties
                                                    var setters = new List<Setter>();

                                                    // a setter takes the form of 
                                                    //   <let propertyname="column name" default="default value"><values><add text="if this" value="then this value" /></values></let>
                                                    // we need to validate that the setters propertyname is valid for a lab result
                                                    // review each setter and ensure that the setter is within the vocabulary of a lab result definition 
                                                    var labResultAttributes = new ValueBag(new LabResult());

                                                    foreach (var item in importStatement.Elements("let")) {
                                                        var assignment = new Setter(item);

                                                        if (labResultAttributes.ContainsKey(assignment.Target))
                                                            setters.Add(new Setter(item));
                                                        else
                                                            log.Warn(string.Format("'{0}' is not a valid lab result attribute. Assignment skipped.", assignment.Target));
                                                    }

                                                    importers.Add(new LabResultImporter(setters, log));
                                                    break;

                                                //case "values":
                                                //    // import gassing values
                                                //    importers.Add(new ValuesImporter(ValueHarvester.Load(importStatement), log));
                                                //    break;

                                                default:
                                                    log.Warn(string.Format("'{0}' is not a recognized import subject. Ignoring import operation.", intoAttribute.GetValueOrDefault()));
                                                    break;
                                            }
                                        });
                                        break;
                                }
                            }

                            foreach (var act in forEachActions)
                                act();

                            // Prepare to read the source data
                            int rowNumber = 0;
                            DataReader reader = DataReader.Load(dataSource);

                            // import each row in the data source using the defined let statements
                            foreach (var row in reader.DataEnumerator()) {
                                //Equipment asset = assetFinder.Find(row);
                                var sourceValues = new ValueBag(row);

                                rowNumber++;

                                ////if (asset == null)
                                ////    log.Warn(string.Format("RowNumber: {1:00000#} equipment not found. Failed to resolve {0}.", assetFinder.Statement(row) as string, rowNumber));
                                ////else
                                foreach (var importer in importers)
                                    try {
                                        importer.Import(sourceValues, rowNumber);
                                    } catch (Exception ex) {
                                        log.Error(ex.Message);
                                    }
                            }
                        });
                        break;

                    default:
                        break;
                }
            }

            foreach (var action in actions)
                action();

            // first up, parse the instructions into something actionable here
            // execute the instructions

            foreach (var foreachStatement in instructions.Elements("foreach")) {


            }

            log.Info("Running ... Done.");
        }
    }
}
