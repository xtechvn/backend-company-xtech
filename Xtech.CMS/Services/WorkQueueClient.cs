using Entities.ViewModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using Utilities;

namespace APP_CHECKOUT.RabitMQ
{
    public class WorkQueueClient
    {
        private readonly QueueSettingViewModel queue_setting;
        private readonly ConnectionFactory factory;
        private readonly IConfiguration _configuration;

        public WorkQueueClient(IConfiguration configuration)
        {
            _configuration = configuration;

            queue_setting = new QueueSettingViewModel()
            {
                host = _configuration["Queue:Host"],
                port = Convert.ToInt32(_configuration["Queue:Port"]),
                v_host = _configuration["Queue:V_Host"],
                username = _configuration["Queue:Username"],
                password = _configuration["Queue:Password"],
            };

            factory = new ConnectionFactory()
            {
                HostName = queue_setting.host,
                UserName = queue_setting.username,
                Password = queue_setting.password,
                VirtualHost = queue_setting.v_host,
                Port = queue_setting.port,
            };
        }

        public async Task<bool> SyncES(long id, string store_procedure, string index_es, short project_id, string function_name = "")
        {
            try
            {
                var j_param = new Dictionary<string, object>
                {
                    { "store_name", store_procedure },
                    { "index_es", index_es },
                    { "project_type", project_id },
                    { "id", id }
                };

                var dataPush = JsonConvert.SerializeObject(j_param);

                // Push message vào queue
                return await InsertQueueSimpleDurableAsync(dataPush, _configuration["Queue:QueueSyncES"]);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(
                    $"WorkQueueClient - SyncES [{function_name}] [{id}][{store_procedure}] [{index_es}][{project_id}] ERROR: {ex}"
                );
                return false;
            }
        }

        public async Task<bool> InsertQueueSimpleAsync(string message, string queueName)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var body = Encoding.UTF8.GetBytes(message);

                // FIX: tạo props đúng kiểu để compiler suy ra TProperties
                var props = new BasicProperties();
                // Nếu không cần durable message thì không set gì thêm

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );

                return true;
            }
            catch (Exception ex)
            {
                // bạn có thể log thêm nếu muốn
                // LogHelper.InsertLogTelegram($"InsertQueueSimpleAsync ERROR: {ex}");
                return false;
            }
        }

        public async Task<bool> InsertQueueSimpleDurableAsync(string message, string queueName)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var body = Encoding.UTF8.GetBytes(message);

                // FIX + Durable message: set DeliveryMode = Persistent
                var props = new BasicProperties
                {
                    DeliveryMode = DeliveryModes.Persistent
                };

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );

                return true;
            }
            catch (Exception ex)
            {
                // LogHelper.InsertLogTelegram($"InsertQueueSimpleDurableAsync ERROR: {ex}");
                return false;
            }
        }
    }
}