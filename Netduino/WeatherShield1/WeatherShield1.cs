/* ------------------------------------------------------------------------------------- 
         - Weather Shield 1 communication library for Netduino - 
                                         - www.EtherMania.com - Marco Signorini - 
   -------------------------------------------------------------------------------------- */

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace EtherMania.com
{
    public class WeatherShield1
    {
        public static Cpu.Pin DEFAULTCLOCK_PIN = Cpu.Pin.GPIO_Pin7;
        public static Cpu.Pin DEFAULTIODATA_PIN = Cpu.Pin.GPIO_Pin2;
        public static Byte DEFAULTADDRESS = 0x01;

        public enum units
        {
            TEMPERATURE,
            PRESSURE,
            HUMIDITY
        };

        public enum sample
        {
            SAMPLE_ONE = 0x00,
            SAMPLE_TWO = 0x01,
            SAMPLE_THREE = 0x02,
            SAMPLE_FOUR = 0x03,
            SAMPLE_FIVE = 0x04,
            SAMPLE_SIX = 0x05,
            SAMPLE_SEVEN = 0x06,
            SAMPLE_EIGHT = 0x07,
            LAST_SAMPLE = 0x80,
            AVG_SAMPLE = 0x81
        };

        private enum commands
        {
            CMD_UNKNOWN = 0x00,
            CMD_SETADDRESS = 0x01,
            CMD_ECHO_PAR = 0x02,
            CMD_SET_SAMPLETIME = 0x03,
            CMD_GETTEMP_C_AVG = 0x04,
            CMD_GETTEMP_C_RAW = 0x05,
            CMD_GETPRESS_AVG = 0x06,
            CMD_GETPRESS_RAW = 0x07,
            CMD_GETHUM_AVG = 0x08,
            CMD_GETHUM_RAW = 0x09,
        };

        private static int RXCOMMANDPOS = 3;
        private static int RXPAR1POS = 2;
        private static int RXPAR2POS = 1;
        private static int RXPAR3POS = 0;
        private static int RXBUFFERLENGTH = 4;

        private OutputPort m_clockPort = new OutputPort(DEFAULTCLOCK_PIN, false);
        private TristatePort m_dataPort = new TristatePort(DEFAULTIODATA_PIN, false, true, Port.ResistorMode.Disabled);
        private Byte m_deviceAddress = DEFAULTADDRESS;
        private Byte[] m_tempBuffer = new Byte[RXBUFFERLENGTH];
        private bool averageValuesValid = false;
        private bool averageValuesChecked = false;

        public WeatherShield1() { resetConnection(); }
        public WeatherShield1(Cpu.Pin clockPin, Cpu.Pin dataPin, Byte deviceAddress)
        {
            m_clockPort = new OutputPort(clockPin, false);
            m_dataPort = new TristatePort(dataPin, false, true, Port.ResistorMode.Disabled);
            m_deviceAddress = deviceAddress;

            resetConnection();
        }

        /* Initialize the connection with the WeatherShield1 */
        public void resetConnection()
        {
            m_clockPort.Write(false);

            /* We start sending a high level bit (start bit) */
            if (!m_dataPort.Active)
                m_dataPort.Active = true;
            m_dataPort.Write(true);
            pulseClockPin();

            /* Then we send a sequence of "fake" low level bits */
            for (int ucN = 0; ucN < 200; ucN++)
            {
                m_dataPort.Write(false);
                pulseClockPin();
            }
        }

        /* Assign a new address to the WeatherShield1 */
        public void setBoardAddress(Byte newAddress)
        {
            sendCommand(commands.CMD_SETADDRESS, newAddress);
        }

        /* Set a new sample time (in seconds from 1 to 256) */
        public void setSampleTime(Byte seconds)
        {
            sendCommand(commands.CMD_SET_SAMPLETIME, seconds);
        }

        /* Send back the parameter through the WeatherShield1 */
        public Byte echo(Byte parameter)
        {
            sendCommand(commands.CMD_ECHO_PAR, parameter);
            if (readAnswer(commands.CMD_ECHO_PAR))
                return m_tempBuffer[RXPAR1POS];

            return 0;
        }

        /* Read an averaged value of specified unit (or MinValue if fails) */
        public float readAveragedValue(units unitType)
        {
            float result = float.MinValue;
            commands command = commands.CMD_UNKNOWN;

            switch (unitType)
            {
                case units.TEMPERATURE:
                    command = commands.CMD_GETTEMP_C_AVG;
                    break;

                case units.HUMIDITY:
                    command = commands.CMD_GETHUM_AVG;
                    break;

                case units.PRESSURE:
                    command = commands.CMD_GETPRESS_AVG;
                    break;
            }

            sendCommand(command, 0);
            if (readAnswer(command))
                result = decodeFloatValue();

            return result;
        }

        /* Read a specific sample for a specified unit in a RAW format */
        /* Returns MinValue if fails */
        public short readRawValue(units unitType, sample sampleNum)
        {
            short result = short.MinValue;
            commands command = commands.CMD_UNKNOWN;

            switch (unitType)
            {
                case units.TEMPERATURE:
                    command = commands.CMD_GETTEMP_C_RAW;
                    break;

                case units.HUMIDITY:
                    command = commands.CMD_GETHUM_RAW;
                    break;

                case units.PRESSURE:
                    command = commands.CMD_GETPRESS_RAW;
                    break;
            }

            sendCommand(command, (Byte) sampleNum);
            if (readAnswer(command))
                result = decodeShortValue();

            return result;
        }

        /* Averaged values are calculated with last 8 raw samples */
        /* This function returns true if the shield contains at least */
        /* 8 valid raw samples in the buffer */
        public bool averageValuesReady()
        {
            if (!averageValuesChecked || !averageValuesValid)
            {
                averageValuesValid = false;

                /* Check for valid connection */
                if (echo(0x55) != 0x55)
                    return averageValuesValid;

                /* Read the last 8 raw temperature samples 
                 ans check they're not zero */
                averageValuesValid = true;
                for (int n = 0; n < 8; n++)
                {
                    short value = readRawValue(units.HUMIDITY, (sample)n);
                    averageValuesValid &= (value != 0);
                }

                averageValuesChecked = true;
            }

            return averageValuesValid;
        }

        /* Generate a clock pulse */
        private void pulseClockPin()
        {
            m_clockPort.Write(true);
            Thread.Sleep(5);
            m_clockPort.Write(false);
            Thread.Sleep(5);
        }

        /* Send a byte through the synchronous serial line */
        private void sendByte(Byte ucData)
        {
            for (int n = 0; n < 8; n++)
            {

                bool bit = (ucData & 0x80) != 0;
                m_dataPort.Write(bit);

                pulseClockPin();
                ucData = (Byte)(ucData << 1);
            }
        }

        /* Read a byte from the synchronous serial line */
        private Byte readByte()
        {
            Byte result = 0;

            for (int n = 0; n < 8; n++)
            {

                m_clockPort.Write(true);
                Thread.Sleep(5);

                result = (Byte)(result << 1);
                bool input = m_dataPort.Read();
                result |= (Byte)((input) ? 1 : 0);

                m_clockPort.Write(false);
                Thread.Sleep(5);
            }

            return result;
        }

        /* Send a command request to the WeatherShield1 */
        private void sendCommand(commands command, Byte parameter)
        {
            /* We start sending the first high level bit */
            if (!m_dataPort.Active)
                m_dataPort.Active = true;
            m_dataPort.Write(true);
            pulseClockPin();

            /* The first byte is always 0xAA... */
            sendByte(0xAA);

            /* ... then is the address... */
            sendByte(m_deviceAddress);

            /* ... then is the command ... */
            sendByte((Byte)command);

            /* ... and the parameter ... */
            sendByte(parameter);

            /* And this is the last low level bit required by the protocol */
            m_dataPort.Write(false);
            pulseClockPin();
        }

        /* Read the answer back from the Weather Shield 1 and fill the provided
        buffer with the result. Depending on the type of command associated
        to this answer the buffer contents should be properly decoded.
        The function returns true if the read answer contain the expected 
        command */
        private bool readAnswer(commands command)
        {
            m_dataPort.Active = false;

            for (int n = RXBUFFERLENGTH; n > 0; n--)
                m_tempBuffer[n-1] = readByte();

            m_dataPort.Active = true;

            return (m_tempBuffer[RXCOMMANDPOS] == (Byte)command);
        }

        /* Convert read bytes in a float value */
        private float decodeFloatValue()
        {

            Byte cMSD = m_tempBuffer[RXPAR1POS];
            Byte cLSD = m_tempBuffer[RXPAR2POS];

            float fVal = cMSD + (((float)cLSD) / 100.0f);

            return fVal;
        }

        /* Convert read bytes in a short value */
        private short decodeShortValue()
        {

            Byte cMSD = m_tempBuffer[RXPAR1POS];
            Byte cLSD = m_tempBuffer[RXPAR2POS];

            short shResult = (short)((cMSD << 8) | cLSD);

            return shResult;
        }
    }
}
