using System;
using System.Threading.Tasks;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Xml;
using RpcBusiness;
using System.Linq;

public class Rpc
{
    public static void Main()
    {
        ClientBusiness rpcBus = new ClientBusiness();
        int Indexfromserver,Index = 0; ;
        char xmloption, option;
        string response = rpcBus.Call(Convert.ToString("SendCurveNames"));
        Console.WriteLine("Response is {0}", response);
        string curvenames, ReceivedCurveName;
        string[] values = new string[] { };
        string[] CurvesValues = new string[] { };        
        string[] splitresponse = response.Split(',');
        string[] requestedcurvenames;
        do
        {
            Console.Clear();
            int diff = 0;
            Console.WriteLine("\n\t\t\t********************\n");
            Console.WriteLine("\t\t\t   WELCOME CLIENT\n");
            Console.WriteLine("\t\t\t********************\n");
            Console.WriteLine("\t\t\tSl#\tCurve Names\n");
            for (int i = 0; i < splitresponse.Length; i++)
            {
                if (splitresponse[i] == "index")
                    continue;
                else
                    Console.WriteLine("\t\t\t{0}\t\t{1}", i, splitresponse[i].Trim());
            }

            Console.WriteLine("\nEnter the name(s) of the required curve in comma separated format from the above available curves!\n");
            curvenames = Console.ReadLine();
           
            requestedcurvenames = curvenames.Split(',');
            Console.WriteLine("\n");
            string curveresponse = rpcBus.Call(Convert.ToString(curvenames));
            //string curveresponse = "100*10@A:150.0,750.0,750.0,150.0,null,222.0;";
            if (curveresponse == "NOTFOUND")
            {
                Console.WriteLine("Sorry, you have entered the wrong entry.\n");
            }
            else
            {
                Indexfromserver = Convert.ToInt32(curveresponse.Split('*')[0]);
                curveresponse = curveresponse.Substring(curveresponse.IndexOf('*') + 1);
                diff = Convert.ToInt32(curveresponse.Split('@')[0]);
                curveresponse = curveresponse.Substring(curveresponse.IndexOf('@') + 1);
                CurvesValues = curveresponse.Split(';');
                for (int i = 0; i < CurvesValues.Length; i++)
                {
                    CurvesValues[i] = CurvesValues[i].Trim();
                }

                Console.WriteLine("\t\tThe Received Curves in Tabular Format:-\n");
                for (int j = 0; j < CurvesValues.Length; j++)
                {
                    if (j != 0)
                    {
                        CurvesValues[j] = CurvesValues[j].Substring(CurvesValues[j].IndexOf('*') + 1);
                        CurvesValues[j] = CurvesValues[j].Substring(CurvesValues[j].IndexOf('@') + 1);
                        

                    }
                    Index = Indexfromserver;
                    values = CurvesValues[j].Split(',');
                    values[0] = values[0].Trim();
                    ReceivedCurveName = (values[0].Split(':'))[0];
                    
                    if (ReceivedCurveName == "" || ReceivedCurveName == null)
                    {
                        Console.WriteLine("\n\t\t       Curve {0} not found in Server. \n", requestedcurvenames[j]);
                        continue;
                    }
                    Console.WriteLine("\t\t\t\tCurve {0}\n", ReceivedCurveName.ToUpper());
                    Console.WriteLine("\t\t\tIndex\t\tCurveValues");

                    for (int i = 0; i < values.Length; i++)
                    {

                        if (i == 0)
                        {
                            values[i] = values[i].Trim();
                            if (values[i] == "null")
                            {
                                values[i] = "";
                                Console.WriteLine("\t\t\t{0}\t\t{1}", Index, (values[0].Split(':'))[1]);
                                Index = Index + diff;
                            }
                            else
                            {
                                Console.WriteLine("\t\t\t{0}\t\t{1}",  Index, (values[0].Split(':'))[1]);
                                Index = Index + diff;
                            }
                        }
                        else
                        {
                            values[i] = values[i].Trim();
                            if (values[i] == "null")
                            {
                                values[i] = "";
                                Console.WriteLine("\t\t\t{0}\t\t{1}",  Index, values[i].TrimStart());
                                Index = Index + diff;
                            }
                            else
                            {
                                Console.WriteLine("\t\t\t{0}\t\t{1}",  Index, values[i].TrimStart());
                                Index = Index + diff;
                            }
                        }
                    }
                    Console.WriteLine("\n\n");
                }

                
                Console.WriteLine("Export Datatable to XML File (Y/N)??");
                xmloption = Convert.ToChar(Console.ReadLine());
                if (xmloption == 'y' || xmloption == 'Y')
                {
                    CurvesValues = CurvesValues.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    string exportxml = rpcBus.csvtoxml(CurvesValues, diff, Indexfromserver);
                    Console.WriteLine("XML File successfully saved in CurveData.XML");
                }
            }
            Console.WriteLine("Display another curve? (Y/N)");
            option = Convert.ToChar(Console.ReadLine());
        }
        while (option == 'Y' || option == 'y');

        if (option != 'y' || option != 'Y')
            Environment.Exit(0);
        rpcBus.Close();
    }
}



