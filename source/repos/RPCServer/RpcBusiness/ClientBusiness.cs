using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Xml;

namespace RpcBusiness
{
    public class ClientBusiness
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;

        string[] values;
        string[] ReceivedCurveName= new string[3];
        double[] MinIndex;
        double[] MaxIndex;
        int Row = 0;



        /// <summary>
        /// Used to close the RabbitQueueMessaging channels.
        /// </summary>
        public void Close()
        {
            connection.Close();
        }

        /// <summary>
        /// This is where the connection from client to server is created. RPC Model is involved for data transfer between client and server.
        /// </summary>
        public ClientBusiness()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
        }



        /// <summary>
        /// Channel Connections between Client and Server implementation
        /// </summary>
        /// <param name="message">String received from Server</param>
        /// <returns>String to client from Server</returns>
        public string Call(string message)
        {
            var tcs = new TaskCompletionSource<string>();
            var resultTask = tcs.Task;
            var correlationId = Guid.NewGuid().ToString();
            IBasicProperties props = channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
            EventHandler<BasicDeliverEventArgs> handler = null;
            handler = (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    consumer.Received -= handler;

                    var body = ea.Body;
                    var response = Encoding.UTF8.GetString(body);
                    tcs.SetResult(response);
                }
            };
            consumer.Received += handler;
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return resultTask.Result;
        }



        /// <summary>
        /// Converting the Comma separated file into XML
        /// </summary>
        /// <param name="CurveValues"> List of Curves to be converted</param>
        /// <param name="difff">difference between consecutive indexes as received from server</param>
        /// <param name="indexx">First value of the index series</param>
        /// <returns></returns>
        public string csvtoxml(string[] CurveValues, int difff, int indexx)
        {
            ////////////////////////////////////////////////////////////
            ////Logic for finding max and minimum of each received curve
            ////////////////////////////////////////////////////////////
            Row = 0;
            int fairlength = 0,found=0;
            for (int i = 0; i < CurveValues.Length; i++)
            {
                CurveValues[i] = CurveValues[i].Trim();
                if (CurveValues[i] == "" || CurveValues[i] == null)
                {
                    continue;
                }
                else
                {
                    if (found == 0)
                    {
                        values = CurveValues[fairlength].Split(',');
                        found = 1;
                    }
                    fairlength = fairlength + 1;
                }

            }
            ReceivedCurveName = new string[fairlength];
            int nullcount = 0, arraycount = 0;
            MinIndex = new double[fairlength];
            MaxIndex = new double[fairlength];
            string[,] CurvesArray = new string[fairlength, values.Length];
            for (int i = 0; i < fairlength; i++)
            {
                nullcount = 0; arraycount = 0;
                values = CurveValues[i].Split(',');
                if (values[0] == "")
                {
                    continue;
                }
                ReceivedCurveName[i] = (values[0].Split(':'))[0];
                values[0] = (values[0].Split(':'))[1];
                for (int j = 0; j < values.Length; j++)
                {
                    values[j] =values[j].Trim();
                    if (values[j] == "null")
                    {
                        nullcount = nullcount + 1;
                        CurvesArray[Row, j] = "";
                    }
                    else
                    {
                        CurvesArray[Row, j] = values[j];
                    }

                }
                Row = Row + 1;

                double[] CurveArray = new double[values.Length - nullcount];
                for (int k = 0; k < values.Length; k++)
                {
                    if (values[k] == "null") { }
                    else
                    {
                        CurveArray[arraycount] = Convert.ToDouble(values[k]);
                        arraycount = arraycount + 1;
                    }

                }

                MinIndex[i] = CurveArray.Min();
                MaxIndex[i] = CurveArray.Max();


            }
            ////////////////////////////////////////////
            ////XML CREATION////////////////////////////
            ////////////////////////////////////////////
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlNode CurvesNode = doc.CreateElement("log");

            XmlNode indexnode = doc.CreateElement("logCurveInfo");
            XmlAttribute indexattribute = doc.CreateAttribute("id");
            indexattribute.Value = "index";
            indexnode.Attributes.Append(indexattribute);
            XmlNode indextype = doc.CreateElement("typeLogData");
            indextype.AppendChild(doc.CreateTextNode("long"));
            indexnode.AppendChild(indextype);
            CurvesNode.AppendChild(indexnode);
            doc.AppendChild(CurvesNode);

            for (int i = 0; i < fairlength; i++)
            {
                XmlNode logCurveInfo = doc.CreateElement("logCurveInfo");

                XmlAttribute idAttribute = doc.CreateAttribute("id");
                if (i == 0)
                {
                    
                    XmlNode typeLogData = doc.CreateElement("typeLogData");
                    idAttribute.Value = ReceivedCurveName[i];
                    logCurveInfo.Attributes.Append(idAttribute);
                    XmlNode minIndex = doc.CreateElement("minIndex");
                    minIndex.AppendChild(doc.CreateTextNode(MinIndex[i].ToString()));
                    logCurveInfo.AppendChild(minIndex);
                    XmlNode maxIndex = doc.CreateElement("maxIndex");
                    maxIndex.AppendChild(doc.CreateTextNode(MaxIndex[i].ToString()));
                    logCurveInfo.AppendChild(maxIndex);

                    XmlNode typeLogData2 = doc.CreateElement("typeLogData");
                    typeLogData2.AppendChild(doc.CreateTextNode("double"));
                    logCurveInfo.AppendChild(typeLogData2);
                    CurvesNode.AppendChild(logCurveInfo);
                    doc.AppendChild(CurvesNode);
                }
                else
                {
                    idAttribute.Value = ReceivedCurveName[i];
                    logCurveInfo.Attributes.Append(idAttribute);

                    XmlNode minIndex = doc.CreateElement("minIndex");
                    minIndex.AppendChild(doc.CreateTextNode(MinIndex[i].ToString()));
                    logCurveInfo.AppendChild(minIndex);

                    XmlNode maxIndex = doc.CreateElement("maxIndex");
                    maxIndex.AppendChild(doc.CreateTextNode(MaxIndex[i].ToString()));
                    logCurveInfo.AppendChild(maxIndex);

                    XmlNode typeLogData = doc.CreateElement("typeLogData");
                    typeLogData.AppendChild(doc.CreateTextNode("double"));
                    logCurveInfo.AppendChild(typeLogData);
                    CurvesNode.AppendChild(logCurveInfo);
                    doc.AppendChild(CurvesNode);
                }

            }

            XmlNode logData = doc.CreateElement("logData");
            CurvesNode.AppendChild(logData);
            string header = string.Join(",", ReceivedCurveName);
            string data = string.Empty;
            XmlNode headerNode = doc.CreateElement("header");
            headerNode.AppendChild(doc.CreateTextNode("index," + header));
            logData.AppendChild(headerNode);

           for (int j = 0; j < values.Length; j++)
            {
                data = string.Empty;
                data = Convert.ToString(indexx);
                for (int i = 0; i < fairlength; i++)
                {
                    data = data + "," + Convert.ToString(CurvesArray[i, j]);
                }
                XmlNode datanode = doc.CreateElement("data");


                datanode.AppendChild(doc.CreateTextNode(data));
                logData.AppendChild(datanode);
                indexx = indexx + difff; ;
            }
            doc.Save("CurveData.xml");
            Console.WriteLine("Conversion Success");
            return doc.OuterXml;
        }        
    }
}
