using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using EtherMania.com;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace WShieldConsole
{
    public class Program
    {
        public static void Main()
        {
            // We use the LED to understand if the shield is properly connected to Netduino and valid average samples are counted 
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

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

    }
}
