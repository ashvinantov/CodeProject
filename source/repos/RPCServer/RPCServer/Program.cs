using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Data;
using RpcBusiness;

class RPCServer
{
    public static void Main()
    {
        ServerBusiness servBus = new ServerBusiness();
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);
            Console.WriteLine("Server Awaiting RPC requests");

            consumer.Received += (model, ea) =>
            {

                string response = null;
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Client Recieves {0}", message);
                    if (message == "SendCurveNames")
                    {
                        string ExtractedColumns = servBus.ReadCurveNames(message);
                        response = ExtractedColumns;
                        Console.WriteLine("Client Returns {0}", response);
                    }
                    else
                    {
                        string Extractedcurves = servBus.ReadCSV(message);
                        response = Extractedcurves;
                        Console.WriteLine("Client Returns {0}", response);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }       
    }
}