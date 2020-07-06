using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace JsonCodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter json schema file:");
            string jsonPath = Console.ReadLine();
            if (string.IsNullOrEmpty(jsonPath))
            {
                Console.WriteLine("Please enter a vaild value");
                Main(args);
            }

            FileInfo info = new FileInfo(jsonPath);
            if (info.Exists == false)
            {
                Console.WriteLine("File not exists! try again...");
                Main(args);
            }

            string schema = File.ReadAllText(jsonPath);
            if (string.IsNullOrEmpty(schema))
            {
                Console.WriteLine("Invalid Schema file! try again...");
                Main(args);
            }

            try
            {
                string output = $"{AppContext.BaseDirectory}{info.Name.Replace(info.Extension, "")}.cs";
                Generate(schema, output);
                Console.WriteLine("Code Generated successfuly! output path: " + output);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating code! Error Details: \n" + ex);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        public static void Generate(string schema, string outputPath)
        {
            JsonSchema jsonSchema = JsonSchema.FromSampleJson(schema);
            var generator = new CSharpGenerator(jsonSchema);
            var file = generator.GenerateFile();

            using (StreamWriter writer = new StreamWriter(outputPath, false))
            {
                writer.Write(file);
            }
        }
    }
}
