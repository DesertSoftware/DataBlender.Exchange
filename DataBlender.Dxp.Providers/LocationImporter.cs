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
    public class LocationImporter : ImportProvider
    {
        private XElement[] data;
        private Terminal console;
        private XElement instructions;
        private Action<int, string> log;

        private List<Action<ValueBag>> CompileImportActions(XElement statement, ValueBag validAssignments) {
            List<Action<ValueBag>> actions = new List<Action<ValueBag>>();

            foreach(var stmt in statement.Elements()) 
                switch(stmt.Name.LocalName.ToLower()) {
                    case "console.print":
                        actions.Add((data) => this.console.Print(stmt.Value, data));
                        break;

                    case "console.warn":
                        actions.Add((data) => this.console.Warn(stmt.Value, data));
                        break;

                    case "console.error":
                        actions.Add((data) => this.console.Error(stmt.Value, data));
                        break;

                    case "console.debug":
                        actions.Add((data) => this.console.Debug(stmt.Value, data));
                        break;

                    case "let":
                        // a setter takes the form of 
                        //   <let propertyname="column name" default="default value"><values><add text="if this" value="then this value" /></values></let>
                        // we need to validate that the setters propertyname is valid
                        var assignment = new Setter(stmt);

                        if (validAssignments.ContainsKey(assignment.Target))
                            actions.Add((data) => assignment.Assign(data["sourceValues"], data["targetValues"]));
                        else
                            this.log.Warn(string.Format("'{0}' is not a valid assignment attribute. Assignment skipped.", assignment.Target));
                        break;

                }

            return actions;
        }

        private Action CompileImportAction(XElement statement) {
            List<Action<ValueBag>> actions = new List<Action<ValueBag>>();
            string into = statement.Attribute("into").GetValueOrDefault();
            string from = statement.Attribute("from").GetValueOrDefault();

            // every import element must minimally define an into attribute
            if (string.IsNullOrWhiteSpace(into)) {
                this.log.Error("Missing into attribute. An import element must contain an 'into' attribute.");
                return () => { };
            }

            // locate the data element that contains the source data to update from
            XElement dataSource = null;

            if (!string.IsNullOrWhiteSpace(from)) {
                foreach (var item in this.data) {
                    if (item.Attribute("id").GetValueOrDefault().ToLower() == from.ToLower()) {
                        dataSource = item;
                        break;
                    }
                }
            } else
                dataSource = this.data.FirstOrDefault();

            if (dataSource == null) {
                log.Error(string.Format("Unable to determine the data set to use. '{0}'", from));
                return () => { };
            }

            switch (into.ToLower()) {
                case "company":
                    // compile the statements within the import element into a set of actionable actions
                    actions = CompileImportActions(statement, new ValueBag(new LocationPath()));
                    return () => {
                        ImportCompanies(actions, dataSource);
                    };

                case "region":
                    // compile the statements within the import element into a set of actionable actions
                    actions = CompileImportActions(statement, new ValueBag(new LocationPath()));
                    return () => {
                        ImportRegions(actions, dataSource);
                    };

                case "site":
                    // compile the statements within the import element into a set of actionable actions
                    actions = CompileImportActions(statement, new ValueBag(new LocationPath()));
                    return () => {
                        ImportSites(actions, dataSource);
                    };

                case "location":
                    actions = CompileImportActions(statement, new ValueBag(new LocationPath()));
                    return () => {
                        ImportLocations(actions, dataSource);
                    };

                default:
                    log.Warn(string.Format("'{0}' is not a recognized import subject. Ignoring import operation.", into));
                    break;
            }

            return () => { };
        }

        private List<Action> CompileInstructions() {
            List<Action> actions = new List<Action>();

            foreach (var statement in this.instructions.Elements()) {
                switch (statement.Name.LocalName.ToLower()) {
                    case "console.print":
                        actions.Add(() => this.console.Print(statement.Value));
                        break;

                    case "console.warn":
                        actions.Add(() => this.console.Warn(statement.Value));
                        break;

                    case "console.error":
                        actions.Add(() => this.console.Error(statement.Value));
                        break;

                    case "console.debug":
                        actions.Add(() => this.console.Debug(statement.Value));
                        break;

                    case "import":
                        // into 'company|region|site'
                        // from <datasourceid>
                        actions.Add(CompileImportAction(statement));
                        break;
                }
            }

            return actions;
        }

        private void ImportLocations(List<Action<ValueBag>> actions, XElement dataSource) {
            // Prepare to read the source data
            int rowNumber = 0;
            DataReader reader = DataReader.Load(dataSource);
            List<LocationPath> locations = new List<LocationPath>();

            // collect each row in the data source using the defined let statements
            foreach (var row in reader.DataEnumerator()) {
                //Equipment asset = assetFinder.Find(row);
                var sourceValues = new ValueBag(row);

                rowNumber++;

                var path = new LocationPath();
                var targetValues = new ValueBag(path);

                var data = new ValueBag(new { Source = row, sourceValues = sourceValues, Target = path, targetValues = targetValues });

                foreach (var act in actions)
                    try {
                        act(data);
                    } catch (Exception ex) {
                        this.log.Error(ex.Message);
                    }

                locations.Add(path);
            }

            int currentSiteID = 0;
            int currentRegionID = 0;
            int currentCompanyID = 0;
            LocationPath currentLocation = new LocationPath();

            // reduce the collected locations into a distinct ordered set of data and import
            log.Info("Collected: {0} items into a distinct count of {1} items", locations.Count, locations.Distinct().Count());
            foreach (var location in locations.Distinct().OrderBy(x => x.CompanyName).ThenBy(x => x.RegionName).ThenBy(x => x.SiteName)) {
                log.Info("{0} / {1} / {2}", location.CompanyName, location.RegionName, location.SiteName);

                if (!(currentLocation.CompanyName ?? "").Equals(location.CompanyName ?? "", StringComparison.CurrentCultureIgnoreCase)) {
                    currentLocation.CompanyName = location.CompanyName ?? "";
                    // Location.OrganizationID = context.OrganizationID
                    // Location.LocationTypeID = COMPANY_LOCATION_TYPE    // (1)
                    // Location.Name = currentLocation.CompanyName
                    // currentCompanyID = ConfigurationSystem.SaveLocation(Location).LocationID
                }

                if (!(currentLocation.RegionName ?? "").Equals(location.RegionName ?? "", StringComparison.CurrentCultureIgnoreCase)) {
                    currentLocation.RegionName = location.RegionName ?? "";
                    // Location.OrganizationID = context.OrganizationID
                    // Location.LocationTypeID = REGION_LOCATION_TYPE    // (2)
                    // Location.Name = currentLocation.RegionName
                    // Location.ParentLocations = new int[] { currentCompanyID }
                    // currentRegionID = ConfigurationSystem.SaveLocation(Location).LocationID
                }

                if (!(currentLocation.SiteName ?? "").Equals(location.SiteName ?? "", StringComparison.CurrentCultureIgnoreCase)) {
                    currentLocation.SiteName = location.SiteName ?? "";
                    // Location.OrganizationID = context.OrganizationID
                    // Location.LocationTypeID = SITE_LOCATION_TYPE    // (4)
                    // Location.Name = currentLocation.SiteName
                    // Location.ParentLocations = new int[] { currentRegionID }
                    // currentSiteID = ConfigurationSystem.SaveLocation(Location).LocationID
                }
            }
        }

        private void ImportCompanies(List<Action<ValueBag>> actions, XElement dataSource) {
            // Prepare to read the source data
            int rowNumber = 0;
            DataReader reader = DataReader.Load(dataSource);
            List<LocationPath> locations = new List<LocationPath>();

            // collect each row in the data source using the defined let statements
            foreach (var row in reader.DataEnumerator()) {
                //Equipment asset = assetFinder.Find(row);
                var sourceValues = new ValueBag(row);

                rowNumber++;

                var path = new LocationPath();
                var targetValues = new ValueBag(path);

                var data = new ValueBag(new { Source = row, sourceValues = sourceValues, Target = path, targetValues = targetValues });

                foreach (var act in actions)
                    try {
                        act(data);
                    } catch (Exception ex) {
                        this.log.Error(ex.Message);
                    }

                locations.Add(path);
            }

            // reduce the collected company locations into a distinct set of data and import
            foreach (var location in locations.Select((loc) => loc.CompanyName).Distinct())

                ;
        }

        private void ImportRegions(List<Action<ValueBag>> actions, XElement dataSource) {
            // Prepare to read the source data
            int rowNumber = 0;
            DataReader reader = DataReader.Load(dataSource);

            // TODO: reduce the data into a distinct set of company/region names

            // import each row in the data source using the defined let statements
            foreach (var row in reader.DataEnumerator()) {
                //Equipment asset = assetFinder.Find(row);
                var sourceValues = new ValueBag(row);

                rowNumber++;

                var labResult = new LabResult();
                var targetValues = new ValueBag(labResult);

                var data = new ValueBag(new { Source = row, sourceValues = sourceValues, Target = labResult, targetValues = targetValues });

                foreach (var act in actions)
                    try {
                        act(data);
                    } catch (Exception ex) {
                        this.log.Error(ex.Message);
                    }
            }
        }

        private void ImportSites(List<Action<ValueBag>> actions, XElement dataSource) {
            // Prepare to read the source data
            int rowNumber = 0;
            DataReader reader = DataReader.Load(dataSource);

            // TODO: reduce the data into a distinct set of company/region names

            // import each row in the data source using the defined let statements
            foreach (var row in reader.DataEnumerator()) {
                //Equipment asset = assetFinder.Find(row);
                var sourceValues = new ValueBag(row);

                rowNumber++;

                var labResult = new LabResult();
                var targetValues = new ValueBag(labResult);

                var data = new ValueBag(new { Source = row, sourceValues = sourceValues, Target = labResult, targetValues = targetValues });

                foreach (var act in actions)
                    try {
                        act(data);
                    } catch (Exception ex) {
                        this.log.Error(ex.Message);
                    }
            }
        }

        /// <summary>
        /// Imports the specified data using the specified intructions.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="data">The data.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">The import context information.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Import(XElement instructions, XElement[] data, Action<int, string> log, dynamic context) {
            log.Info("Running ...");
            if (instructions == null) throw new ArgumentNullException("instructions");
            if (data == null) throw new ArgumentNullException("data");
            if (log == null) throw new ArgumentNullException("log");

            this.log = log;
            this.data = data;
            this.instructions = instructions;
            this.console = new Terminal(log);

            // first up, parse the instructions into something actionable here
            // compile into an actionable list
            List<Action> actions = CompileInstructions();

            // execute the instructions
            foreach (var action in actions)
                action();

            log.Info("Running ... Done.");
        }
    }
}
