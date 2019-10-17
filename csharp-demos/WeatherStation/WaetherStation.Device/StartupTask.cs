using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace WeatherStation.Device
{
    public sealed class StartupTask : IBackgroundTask
    {
        private static DeviceClient deviceClient;
        private static string deviceKey = "BHySgxQifz2qdYFVZs2fFPLyNOx0/l+PmxLLZcwteGQ=";
        private static string deviceId = "weatherStation";
        private static string iotHubUri = "WeatherStationDemo.azure-devices.net";

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
            await SendDeviceToCloudMessagesAsync();

            deferral.Complete();
        }

        private static async Task SendDeviceToCloudMessagesAsync()
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
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);

                await Task.Delay(500);
            }
        }
    }
}
