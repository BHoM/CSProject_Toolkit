/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using BH.oM.Base;
using BH.oM.Adapter;
using BH.Engine.Adapter;

using BH.oM.Adapters.CSProject;

using Buildalyzer;
using Buildalyzer.Environment;
using Microsoft.Extensions.Logging;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;

using System.IO;

namespace BH.Adapter.CSProject
{
    public partial class CSProjectAdapter : BHoMAdapter
    {
        protected override IEnumerable<IBHoMObject> IRead(Type type, IList indices = null, ActionConfig actionConfig = null)
        {
            StringBuilder logBuilder = new StringBuilder();
            
            ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Normal, x => logBuilder.Append(x), null, null);

            ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            StringWriter log = new StringWriter();

            AnalyzerManagerOptions opt = new AnalyzerManagerOptions() { LoggerFactory = factory, LogWriter = log };
            AnalyzerManager analyzerManager = new AnalyzerManager(opt);
            ProjectAnalyzer projectAnalyzer = analyzerManager.GetProject(m_fileSettings.GetFullFileName()) as ProjectAnalyzer;

            ProjectFile projectFile = new ProjectFile();
            projectFile.TargetNETVersions = projectAnalyzer.ProjectFile.TargetFrameworks.ToList();
            projectFile.OutputPaths = ExtractOutputPaths();

            try
            {
                EnvironmentOptions options = new EnvironmentOptions();
                options.DesignTime = true;
                /*options.Preference = EnvironmentPreference.Core;
                if (!projectFile.TargetNETVersions.Contains("netstandard2.0"))
                    options.Preference = EnvironmentPreference.Framework;*/
                    
                AnalyzerResult result = (projectAnalyzer.Build(options) as AnalyzerResults).First() as AnalyzerResult;

                var refs = result.Items.Where(x => x.Key == "Reference").FirstOrDefault();
                if(refs.Value != null) //If the refs.value is null then it means there was no references in the csproject file
                    projectFile.References = Convert.ToAssemblyReference(refs.Value.ToList());

                projectFile.TargetNETVersions = projectAnalyzer.ProjectFile.TargetFrameworks.ToList();
            }
            catch(Exception e)
            {
                projectFile.AnalysisErrors = e.ToString();
            }

            return new List<IBHoMObject>() { projectFile };
        }

        private List<string> ExtractOutputPaths()
        {
            List<string> lines = new List<string>();
            using (StreamReader sr = new StreamReader(m_fileSettings.GetFullFileName()))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                    lines.Add(line);

                sr.Close();
            }

            List<string> outputPaths = new List<string>();
            string outputPathStart = "<OutputPath>";
            string outputPathEnd = "</OutputPath>";
            foreach(string s in lines)
            {
                if(s.Trim().StartsWith(outputPathStart))
                {
                    string path = s.Trim().Substring(outputPathStart.Length);
                    path = path.Substring(0, path.IndexOf(outputPathEnd));
                    outputPaths.Add(path);
                }
            }

            return outputPaths;
        }
    }
}

