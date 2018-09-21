using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcBusiness
{
    /// <summary>
    /// Server Related Business Logic implemented in this module class
    /// </summary>
    public class ServerBusiness
    {   
        /// <summary>
        /// Read CSV file and gather the curves as requested by client.
        /// </summary>
        /// <param name="message">String received from client</param>
        /// <returns>Returns either Curve details</returns>
        public string ReadCSV(string message)
        {
            string commaSeperatedValues = "";
            int found = 0, firstindex = 0,diff = 0; ;
            Console.WriteLine("Message received is {0}", message);
            string[] RequestedCurves = message.Split(',');
            
            int readcolumns = 0;
            DataTable csvdatatable = new DataTable();
            using (var reader = new StreamReader(@"D:\testfile.txt"))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] values = line.Split(';');
                    string[] commaseparated = values[0].Split(',');
                    if (readcolumns == 0)
                    {
                        readcolumns = 1;
                        for (int i = 0; i < commaseparated.Length; i++)
                        {
                            if (commaseparated[i] == "index")
                            {
                                commaseparated[i] = commaseparated[i].Replace(" ", "");
                                csvdatatable.Columns.Add(Convert.ToString(commaseparated[i]), typeof(string));
                            }
                            else
                            {

                                commaseparated[i] = commaseparated[i].Replace(" ", "");
                                csvdatatable.Columns.Add(commaseparated[i], typeof(string));
                            }
                        }
                    }
                    else
                    {
                        DataRow dr1 = csvdatatable.NewRow();
                        for (int j = 0; j < commaseparated.Length; j++)
                        {
                            if (commaseparated[j] == " ")
                                dr1[j] = "null";
                            else
                            {
                                commaseparated[j] = commaseparated[j].Replace(" ", "");
                                dr1[j] = commaseparated[j];
                            }

                        }
                        csvdatatable.Rows.Add(dr1);

                    }           

                    
                }
                firstindex = Convert.ToInt32(csvdatatable.Rows[0][0]);
                diff = Convert.ToInt32(csvdatatable.Rows[1][0]) - Convert.ToInt32(csvdatatable.Rows[0][0]);
            }


            DataColumnCollection columns = csvdatatable.Columns;

            for (int j = 0; j < RequestedCurves.Length; j++)
            {
                if (found == 1)
                    commaSeperatedValues = commaSeperatedValues + ";";
                if (columns.Contains(RequestedCurves[j]))
                {
                    found = 1;
                    commaSeperatedValues = commaSeperatedValues + firstindex+"*"+diff+'@'+RequestedCurves[j] + ":";
                    var SelectedValues = csvdatatable.AsEnumerable().Select(s => s.Field<string>(RequestedCurves[j])).ToArray();
                    commaSeperatedValues = commaSeperatedValues + string.Join(",", SelectedValues);

                }

            }
            if (found == 0)
                commaSeperatedValues = "NOTFOUND";

            return commaSeperatedValues;
        }

        /// <summary>
        /// Returns first row of CSV file.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>first row of CSV File</returns>
        public string ReadCurveNames(string message)
        {
            using (var reader = new StreamReader(@"D:\testfile.txt"))
            {
                var line = reader.ReadLine();
                return line;

            }

        }
    }
}
