using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MySql.Data.MySqlClient;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ConsumerService
{
    private readonly IModel _channel;
    private readonly string _connectionString;

    public ConsumerService(IModel channel, string connectionString)
    {
        _channel = channel;
        _connectionString = connectionString;
    }

    public void StartConsuming()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var lines = message.Split('\n');
            Console.WriteLine("Chunk Received");

            var valuesList = new List<string>();

            foreach (var line in lines)
            {
                var values = line.Split(',');
                if (values.Length == 14)
                {
                    var valuesFormatted = string.Join(",", values.Select(v => $"'{v}'"));
                    valuesList.Add($"({valuesFormatted})");
                }
            }

            if (valuesList.Count > 0)
            {
                using (var dbConnection = new MySqlConnection(_connectionString))
                {
                    await dbConnection.OpenAsync();

                    var query = $"INSERT INTO employeeinfo (`1`, `2`, `3`, `4`, `5`, `6`, `7`, `8`, `9`, `10`, `11`, `12`, `13`, `14`) VALUES {string.Join(",", valuesList)};";

                    using (var command = new MySqlCommand(query, dbConnection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    await dbConnection.CloseAsync();
                }
            }
        };
        _channel.BasicConsume(queue: "csv_queue", autoAck: true, consumer: consumer);
    }
}
