using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Frixel.VersionBumper {
    class Program {
        static void Main(string[] args) {
            UpdateVersions("0.1.0.0");
        }

        public static void UpdateVersions(string version) {
            List<string> assemblies = new List<string>();

            List<string> projNames = new List<string>();

            string solutionDir = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\";
            string assem = @"\Properties\AssemblyInfo.cs";

            projNames.Add("Frixel.Core");

            foreach (var projName in projNames) {
                assemblies.Add(solutionDir + projName + assem);
            }

            foreach (string assemblyInfo in assemblies) {
                var lines = System.IO.File.ReadAllLines(assemblyInfo);
                List<string> newContent = new List<string>();

                foreach (string line in lines) {
                    string newLine = line;
                    if (line.StartsWith("[assembly: AssemblyVersion"))
                        newLine = String.Format("[assembly: AssemblyVersion(\"{0}\")]", version);
                    if (line.StartsWith("[assembly: AssemblyFileVersion"))
                        newLine = String.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version);

                    newContent.Add(newLine);

                }

                System.IO.File.WriteAllLines(assemblyInfo, newContent);
            }
        }



    }


}
