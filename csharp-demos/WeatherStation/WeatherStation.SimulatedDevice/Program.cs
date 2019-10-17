using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Text;

namespace WeatherStation.SimulatedDevice
{
    public class Program
    {
        private static Device device;
        private static string deviceKey;
        private static DeviceClient deviceClient;
        private static RegistryManager registryManager;
        private static string deviceId = "simulatedDevice";
        private static string iotHubUri = "HubTest.azure-devices.net";
        private static string connectionString = "HostName=HubTest.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xRPqhrJpAaGLCcw2HO1Nsjhgz0RG5N2nic/DFGM1+sI=";

        public static void Main(string[] args)
        {
            // Register Device with Azure
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            RegisterDeviceAsync().Wait();

            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();

        }

        private static async Task RegisterDeviceAsync()
        {
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            deviceKey = device.Authentication.SymmetricKey.PrimaryKey;
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            var weatherDataprovider = await SimulatedWeatherSensorProvider.Create();

            for (int i = 0; i < 288; i++)
            {
                double currentHumidity = weatherDataprovider.GetHumidity();
                double currentTemperature = weatherDataprovider.GetTemperature();
                double currentWindSpeed = weatherDataprovider.GetWindSpeed();

                var telemetryDataPoint = new
                {
                    time = DateTime.Now.ToString(),
                    deviceId = deviceId,
                    currentHumidity = currentHumidity,
                    currentTemperature = currentTemperature,
                    currentWindSpeed = currentWindSpeed
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(500);
            }
        }
    }
}
