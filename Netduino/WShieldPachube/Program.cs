using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using EtherMania.com;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace WShieldPachube
{
    public class Program
    {
        private static string keyAPI = "YOURKEYAPIHERE";
        private static string URL = "http://api.pachube.com/v2/feeds/YOURFEED.csv";
        private static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

        public static void Main()
        {
            // This is the main WeatherShield object
            WeatherShield1 wShield1 = new WeatherShield1(Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D2, WeatherShield1.DEFAULTADDRESS);

            // Wait until valid samples are ready on the shield
            while (!wShield1.averageValuesReady())
            {
                led.Write(true);
                Thread.Sleep(1000);
                led.Write(false);
                Thread.Sleep(1000);
            }

            // Then start sending data to Console
            while (true)
            {
                // Read values from the shield
                float temperature = wShield1.readAveragedValue(WeatherShield1.units.TEMPERATURE);
                float pressure = wShield1.readAveragedValue(WeatherShield1.units.PRESSURE);
                float humidity = wShield1.readAveragedValue(WeatherShield1.units.HUMIDITY);

                if ((temperature != float.MinValue) &&
                    (pressure != float.MinValue) &&
                    (humidity != float.MinValue))
                {
                    // Connection performed OK.
                    led.Write(true);

                    Debug.Print("Temperature: " + temperature.ToString() + "\n");
                    Debug.Print("Pressure: " + pressure.ToString() + "\n");
                    Debug.Print("Humidity: " + humidity.ToString() + "\n");

                    PachubeGlue pachube = new PachubeGlue(URL, keyAPI);
                    if (pachube.sendMeasures(pressure, temperature, humidity))
                        blinkLed();
                }
                else
                {
                    // Something went wrong
                    led.Write(false);

                    // Try to resync the WeatherShield connection
                    wShield1.resetConnection();
                }

                // Wait for 30 seconds
                Thread.Sleep(30000);
            }
        }

        // Blink led 10 times
        private static void blinkLed()
        {
            for (int n = 0; n < 10; n++)
            {
                led.Write(true);
                Thread.Sleep(100);
                led.Write(false);
                Thread.Sleep(100);
            }

            led.Write(true);
        }
    }
}
