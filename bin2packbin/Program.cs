using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace bin2packbin
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Syntax:\r\n\r\nbin2packbin source-directory target-directory header-directory\r\n\r\n");
                return;
            }

            string source_path;
            string target_path;
            string header_path;

            source_path = args[0];
            target_path = args[1];
            header_path = args[2];

            if (!Directory.Exists(source_path))
            {
                Console.WriteLine("Source path not found\r\n");
                return;
            }

            if (!Directory.Exists(target_path))
            {
                Console.WriteLine("Target path not found\r\n");
                return;
            }

            DirectoryInfo sourceDir = new DirectoryInfo(source_path);

            DirectoryInfo targetDir = new DirectoryInfo(target_path);

            int counter = 0;

            string name1 = "";
            string name2 = "";
            string name3 = "";

            int n = 0;

            const string NumberFormat = "0";

            ArrayGenerator ag = null;

            using (StreamWriter swDefine = new StreamWriter(header_path + "\\binaries.h"))
            {

                swDefine.WriteLine("");
                swDefine.WriteLine();
                
                using (StreamWriter swArrays = new StreamWriter(header_path + "\\binaries.cpp"))
                {
                    swArrays.WriteLine("#include \"binaries.h\"");
                    swArrays.WriteLine();

                    foreach (FileInfo file in sourceDir.GetFiles("*.bin"))
                    {

                        string[] c1 = file.Name.Split('.');
                        string[] c2 = c1[0].Split('_');

                        string nname1;
                        string nname2;
                        string nname3;

                        nname1 = c2[0];
                        if (c2.Length > 1) nname2 = c2[1]; else nname2 = "";
                        if (c2.Length > 2) nname3 = c2[2]; else nname3 = "";

                        if ((nname1 != name1) || (nname2 != name2) || (nname3 != name3) || (n != c2.Length))
                        {

                            if (name1 != "")
                            {
                                if (ag != null)
                                {
                                    ag.PrintHeader(swDefine);
                                    ag.Print(swArrays);
                                    ag = null;
                                }
                            }

                            name1 = nname1;
                            name2 = nname2;
                            name3 = nname3;
                            n = c2.Length;

                            if (c2.Length == 4)
                            {
                                ag = new ArrayGenerator(name1, name2, name3);
                            }

                        }

                        if (ag != null) ag.AddLine(int.Parse(c2[3]), counter.ToString(NumberFormat));


                        // Console.WriteLine(target_path + "\\" + counter.ToString(NumberFormat));
                        file.CopyTo(target_path + "\\" + counter.ToString(NumberFormat) + ".bin", true);

                        swDefine.WriteLine("#define " + file.Name.Replace('.', '_') + " " + counter.ToString(NumberFormat) + "");

                        counter++;
                    }


                    if (ag != null)
                    {
                        ag.Print(swArrays);
                        ag = null;
                    }

                    Console.WriteLine("bin2packbin: " + counter.ToString() + " files written to " + target_path);


                }
                Console.ReadLine();
            }

            
        }
    }


    class ArrayGenerator
    {
        private ArrayList list = null;
        private string n1;
        private string n2;
        private string n3;

        public ArrayGenerator(string name1, string name2, string name3)
        {
            n1 = name1;
            n2 = name2;
            n3 = name3;
            list = new ArrayList();
        }

        public void AddLine(int id, string value)
        {
            list.Add(new ArrayGeneratorLine(id,value));
        }

        public void Print(StreamWriter sw)
        {
            if (list != null)
            {
                list.Sort();
                sw.WriteLine("unsigned short int ar_" + n1 + "_" + n2 + "_" + n3 + "_count = " + list.Count.ToString() + ";");
                sw.Write("unsigned short int ar_" + n1 + "_" + n2 + "_" + n3 + " [" + list.Count.ToString() + "] = { ");
                foreach (object obj in list)
                {
                    sw.Write("" + ((ArrayGeneratorLine)obj).Filename + ",");
                }
                sw.WriteLine("};");
            }
        }

        public void PrintHeader(StreamWriter sw)
        {
            if (list != null)
            {
                list.Sort();
                sw.WriteLine("extern unsigned short int ar_" + n1 + "_" + n2 + "_" + n3 + " [" + list.Count.ToString() + "];");
                sw.WriteLine("extern unsigned short int ar_" + n1 + "_" + n2 + "_" + n3 + "_count;");
            }
        }

    }

    class ArrayGeneratorLine : IComparable
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _filename;

        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        public ArrayGeneratorLine(int id, string filename)
        {
            _id = id;
            _filename = filename;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return _id.CompareTo(((ArrayGeneratorLine)obj).Id);
        }

        #endregion
    }
}
