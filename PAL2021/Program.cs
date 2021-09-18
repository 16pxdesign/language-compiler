using System;
using System.IO;
using AllanMilne.Ardkit;

namespace PAL2021
{
    class Program
    {
        public static void Main(String[] args)
        {
            Prologue(); //info header
            //check is file path provided
            if (args.Length != 1) 
            {
                Console.WriteLine("Error: No file path provided as argument");
                return;
            }

            //Open the input source file.
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(args[0]);
            }
            catch (IOException e)
            {
                IoError("opening", args[0], e);
                return;
            }

            //Start program
            Program program = new Program(reader);
            program.Start();

            //Close reader
            try
            {
                reader.Close();
            }
            catch (IOException e)
            {
                IoError("closing", args[0], e);
                return;
            }

        }

        //Declare Program variables
        private TextReader _source; // the input source file stream.
        private static readonly ComponentInfo Info = new ComponentInfo("PAL", "1.0", "2021", "A.R.Ruszala", "PAL Compiler"); //Module info

        //Constructor
        private Program(TextReader reader)
        {
            _source = reader;
        }

        //Start compiler
        private void Start()
        {
            PALParser parser = new PALParser();

            parser.Parse(_source);

            foreach (ICompilerError err in parser.Errors)
            {
                Console.WriteLine(err);
            }

            Console.WriteLine("{0:d} errors found.", parser.Errors.Count);
        }


        private static void Prologue()
        {
            Console.WriteLine(Ardkit.Info.Copyright);
            Console.WriteLine(Program.Info);
        } 

        //--- An I/O exception has been caught.
        private static void IoError(String function, String filename, IOException e)
        {
            Console.WriteLine("An I/O error occurred {0:s} file {1:s}.", function, filename);
            Console.WriteLine(e);
        } // end ioError method.

    }
}
