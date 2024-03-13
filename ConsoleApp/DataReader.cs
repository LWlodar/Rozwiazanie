using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace ConsoleApp
{
    /// <summary>
    /// Class to operate on.
    /// </summary>
    /// <param name="values">Dependency injection [type,name,schema,parentname,parenttype,datatype,isnullable].</param>
    class ImportedObject(string[] values) : ImportedObjectBaseClass(values.Take(2).ToArray())
    {
        public string Schema { get; set; } = values[2];
        public string ParentName { get; set; } = values[3];
        public string ParentType { get; set; } = values[4];
        public string DataType { get; set; } = values[5];
        public string IsNullable { get; set; } = values[6];
        public List<ImportedObject> Children = [];
        public void Add(ImportedObject obj) { Children.Add(obj); }
    }

    /// <summary>
    /// Base class (for further project "evolution"?).
    /// </summary>
    /// <param name="values">Dependency injection [type,name].</param>
    class ImportedObjectBaseClass(string[] values)
    {
        public string Name { get; set; } = values[1];
        public string Type { get; set; } = values[0];
    }

    /// <summary>
    /// Read a file.
    /// </summary>
    internal class DataReader
    {
        /// <summary>
        /// Read data.
        /// </summary>
        public List<ImportedObject> ImportedObjects { get; private set; } = new List<ImportedObject>();

        /// <summary>
        /// Read a file and possibly print it to the console.
        /// </summary>
        /// <param name="fileToImport">File name.</param>
        /// <param name="printData">Print the data to the console?</param>
        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            StreamReader streamReader;
            try
            {
                // try to load file
                streamReader = new StreamReader(fileToImport);
            } catch {
                // error loading file
                return;
            }

            // clear the data
            if (ImportedObjects.Count > 0)
                ImportedObjects.Clear();
            // if file loaded successfully - go on
            var importedLines = new List<string>();
            string? line;
            while (!streamReader.EndOfStream)
            {
                line = streamReader.ReadLine();
                if (line != null)
                    importedLines.Add(line);
            }

            foreach (var imported in importedLines)
            {
                try
                {
                    // assume all whitespaces can be removed...
                    var values = imported.Trim().Replace(" ", "").Split(';');
                    values[0] = values[0].ToUpper();
                    ImportedObjects.Add(new ImportedObject(values));
                } catch {
                    // input data(line) not correct -> go on with the loop
                }
            }

            // find children of each object
            foreach (var posParent in ImportedObjects)
                foreach (var posChild in ImportedObjects)
                    if (posParent != posChild && posChild.ParentType == posParent.Type && posChild.ParentName == posParent.Name)
                        posParent.Add(posChild);

            // print the loaded data
            foreach (var imported in ImportedObjects)
                if (imported.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{imported.Name}' ({imported.Children.Count} tables):");

                    // print all database's tables
                    foreach (var table in imported.Children)
                    {
                        Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.Children.Count} columns)");

                        // print all table's columns
                        foreach (var column in table.Children)
                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                    }
                }

            Console.ReadLine();
        }
    }
}