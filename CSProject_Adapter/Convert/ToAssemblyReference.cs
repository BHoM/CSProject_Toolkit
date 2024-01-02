/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BH.oM.Adapters.CSProject;

using Buildalyzer;

namespace BH.Adapter.CSProject
{
    public static partial class Convert
    {
        public static List<AssemblyReference> ToAssemblyReference(List<IProjectItem> references)
        {
            List<AssemblyReference> assemblyReferences = new List<AssemblyReference>();

            foreach(IProjectItem reference in references)
            {
                string refName = reference.ItemSpec.ToLower();
                if (!refName.StartsWith("bhom") && !refName.Contains("_om") && !refName.Contains("_engine") && !refName.Contains("_adapter") && !refName.Contains("_ui"))
                    continue; //Not a BHoM DLL so no point worrying

                AssemblyReference assemblyReference = new AssemblyReference();
                string[] splitData = reference.ItemSpec.Split(',');
                if (splitData.Length > 0)
                    assemblyReference.Name = splitData[0];
                if (splitData.Length > 1)
                    assemblyReference.Version = splitData[1];
                if (splitData.Length > 2)
                    assemblyReference.Culture = splitData[2];
                if (splitData.Length > 3)
                    assemblyReference.ProcessorArchitecture = splitData[3];

                if (reference.Metadata.ContainsKey("HintPath"))
                    assemblyReference.HintPath = reference.Metadata["HintPath"];

                if (reference.Metadata.ContainsKey("Private"))
                    assemblyReference.CopyLocal = System.Convert.ToBoolean(reference.Metadata["Private"]);

                if (reference.Metadata.ContainsKey("SpecificVersion"))
                    assemblyReference.SpecificVersion = System.Convert.ToBoolean(reference.Metadata["SpecificVersion"]);

                assemblyReferences.Add(assemblyReference);
            }

            return assemblyReferences;
        }
    }
}



