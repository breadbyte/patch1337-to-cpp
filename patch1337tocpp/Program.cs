using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;

namespace patch1337tocpp {
    class Program {
        static void Main(string[] args) {
            CommandLineOpts opts = null;
            
            Parser.Default.ParseArguments<CommandLineOpts>(args)
                  .WithParsed<CommandLineOpts>(o => opts = o);

            var lines = File.ReadAllLines(opts.patchFile);

            List<Patch> patches = new();
            StringBuilder finalOutput = new StringBuilder();

            Patch? currentPatch = null;
            
            foreach (var line in lines) {
                if (line.StartsWith('>')) {
                    var p = new Patch() {ModuleName = line.Substring(1, line.Length - 1)};
                    p.Patches = new();
                    currentPatch = p;
                    patches.Add(p);
                }
                else {
                    if (!currentPatch.HasValue)
                        throw new InvalidDataException("Patch does not start with a valid module identifier!");
                    
                    currentPatch.Value.Patches.Add(new PatchAddress() {
                        Address = int.Parse(line.Substring(0, 8), NumberStyles.HexNumber),
                        OldByte = byte.Parse(line.Substring(9, 2), NumberStyles.HexNumber),
                        NewByte = byte.Parse(line.Substring(13, 2), NumberStyles.HexNumber)
                    });
                }
            }

            using (StreamWriter w = new StreamWriter("output.cpp")) {
                
                w.WriteLine("#include <vector>");
                w.Write("struct PatchAddress { ");
                w.Write("int Address; ");
                w.Write("unsigned char OldByte; ");
                w.Write("unsigned char NewByte; ");
                w.WriteLine("};");
                w.Write("struct Patch { ");
                w.Write("char* ModuleName; ");
                w.Write("std::vector<PatchAddress> Patches;");
                w.WriteLine("};");

                foreach (var patch in patches) {
                    var addressableName = Regex.Replace(patch.ModuleName, "([^A-Za-z0-9])+", "_");
                    w.WriteLine($"Patch {addressableName};");
                    w.WriteLine($"{addressableName}.ModuleName = \"{patch.ModuleName}\";");
                    w.WriteLine($"{addressableName}.Patches = {{");

                    foreach (var addr in patch.Patches) { 
                        w.WriteLine($"PatchAddress{{ {addr.Address}, {addr.OldByte}, {addr.NewByte} }},");
                    }

                    w.WriteLine("};");
                }

                w.Flush();
            }
        }

        struct Patch {
            public string ModuleName;
            public List<PatchAddress> Patches;
        }

        struct PatchAddress {
            public int Address;
            public byte OldByte;
            public byte NewByte;
        }
    }
    
    public class CommandLineOpts
    {
        [Option('f', "patch_file", Required = true, HelpText = "The patch file.")]
        public string patchFile { get; set; }
    }
}