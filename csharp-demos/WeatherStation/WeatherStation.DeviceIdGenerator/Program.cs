using System;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Threading.Tasks;

namespace WeatherStation.DeviceIdGenerator
{
    public class Program
    {
        static RegistryManager registryManager;

        public static void Main(string[] args)
        {
            Console.WriteLine("Please enter your IoT Hub connection string:");
            string connectionString = Console.ReadLine();

            Console.WriteLine("Please enter your device Id:");
            string deviceId = Console.ReadLine();

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync(deviceId).Wait();
            Console.ReadLine();
        }

        private static async Task AddDeviceAsync(string deviceId)
        {
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
