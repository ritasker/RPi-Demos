using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.Devices.Spi;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ADC_Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;

            InitApp();
        }

        private async void InitApp()
        {
            try
            {
                //InitGpio();         /* Initialize GPIO to toggle the LED                          */
                await InitSPI();      /* Initialize the SPI bus for communicating with the ADC      */

            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
                return;
            }

            /* Now that everything is initialized, create a timer so we read data every 500mS */
            periodicTimer = new Timer(this.Timer_Tick, null, 0, 100);

            StatusText.Text = "Status: Running";
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (periodicTimer != null)
            {
                periodicTimer.Dispose();
            }            

            if (SpiADC != null)
            {
                SpiADC.Dispose();
            }
        }

        private async Task InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;   /* 0.5MHz clock rate                                        */
                settings.Mode = SpiMode.Mode0;      /* The ADC expects idle-low clock polarity so we use Mode0  */

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiADC = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private void Timer_Tick(object state)
        {
            int adcValue = ReadADC();

            /* UI updates must be invoked on the UI thread */
            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textPlaceHolder.Text = adcValue.ToString();         /* Display the value on screen                      */
            });
        }

        public int ReadADC()
        {
            byte[] readBuffer = new byte[3]; /* Buffer to hold read data*/
            byte[] writeBuffer = new byte[3] { 0x00, 0x00, 0x00 };

            writeBuffer[0] = MCP3008_CONFIG[0];
            writeBuffer[1] = MCP3008_CONFIG[1];

            SpiADC.TransferFullDuplex(writeBuffer, readBuffer); /* Read data from the ADC                           */
            return convertToInt(readBuffer);                    /* Convert the returned bytes into an integer value */            
        }

        /* Convert the raw ADC bytes to an integer */
        public int convertToInt(byte[] data)
        {
            int result = 0;
            result = data[1] & 0x03;
            result <<= 8;
            result += data[2];
            return result;
        }

        private const string SPI_CONTROLLER_NAME = "SPI0";  /* Friendly name for Raspberry Pi 2 SPI controller          */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
        private SpiDevice SpiADC;

        private readonly byte[] MCP3008_CONFIG = { 0x01, 0x80 }; /* 00000001 10000000 channel configuration data for the MCP3008 */

        private Timer periodicTimer;
    }
}
