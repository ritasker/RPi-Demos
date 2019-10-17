using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using Windows.Devices.Pwm;
using System.Threading;

namespace ThereminPi
{
    public sealed class StartupTask : IBackgroundTask
    {
        private Timer timer;
        private PwmPin buzzer;
        private SpiDevice SpiADC;
        private const int BUZZER_PIN = 27;
        private PwmController pwmController;
        private double RestingPulseLegnth = 0.5;
        private BackgroundTaskDeferral deferral;        
        private const Int32 SPI_CHIP_SELECT_LINE = 0;
        private const string SPI_CONTROLLER_NAME = "SPI0";
        private readonly byte[] MCP3008_CONFIG = { 0x01, 0x80 };


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            await InitSensor();
            await InitBuzzer();
            timer = new Timer(Timer_Tick, null, 0, 50);
        }

        private async Task InitSensor()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;
                settings.Mode = SpiMode.Mode0;

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiADC = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        private async Task InitBuzzer()
        {
            pwmController = (await PwmController.GetControllersAsync(PwmSoftware.PwmProviderSoftware.GetPwmProvider()))[0];
            pwmController.SetDesiredFrequency(40);
            buzzer = pwmController.OpenPin(BUZZER_PIN);
            buzzer.SetActiveDutyCyclePercentage(RestingPulseLegnth);            
        }

        private void Timer_Tick(object state)
        {
            int result = ReadADC();

            double buzzFreq = result * 0.9375;

            if (buzzFreq < 150)
                buzzer.Stop();

            if (buzzFreq >= 150)
                buzzer.Start();

            if (buzzFreq > 1000)
                buzzFreq = 1000;

            pwmController.SetDesiredFrequency(buzzFreq);
        }

        private int ReadADC()
        {
            byte[] readBuffer = new byte[3];
            byte[] writeBuffer = new byte[3] { 0x00, 0x00, 0x00 };

            writeBuffer[0] = MCP3008_CONFIG[0];
            writeBuffer[1] = MCP3008_CONFIG[1];

            SpiADC.TransferFullDuplex(writeBuffer, readBuffer);
            return convertToInt(readBuffer);
        }

        private int convertToInt(byte[] data)
        {
            int result = 0;
            result = data[1] & 0x03;
            result <<= 8;
            result += data[2];
            return result;
        }

        
    }
}
