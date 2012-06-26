/* -----------------------------------------------------------------
    Weather Shield 1 communication sample sketch
                               www.EtherMania.com - Signorini Marco
   
   This is a sample sketch that shows how to communicate to the
   Weather Shield and an Arduino 2009 board and to publish 
   received into a web page.
   Web client part is based on example provided by the Arduino IDE.
  ------------------------------------------------------------------- */
  
#include <WeatherShield1.h>
#include <SPI.h>
#include <Ethernet.h>
#include "weathershield1_weatherdata.h"

byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };
byte ip[] = { 192, 168, 0, 150 };

#define RXBUFFERLENGTH          4

WeatherShield1 weatherShield;
EthernetServer server(80);

/* This is the prototype function able to communicate with the WeatherShield1 */
boolean handleWeatherShieldValues(WeatherData &weatherData);

/* ----------------------------------------------------------------- */

void setup()
{
  Ethernet.begin(mac, ip);
  server.begin();
}

/* ----------------------------------------------------------------- */

void loop() {

  EthernetClient client = server.available();
  if (client) {
    // an http request ends with a blank line
    boolean current_line_is_blank = true;
    while (client.connected()) {
      if (client.available()) {
        char c = client.read();
        // if we've gotten to the end of the line (received a newline
        // character) and the line is blank, the http request has ended,
        // so we can send a reply
        if (c == '\n' && current_line_is_blank) {
          // send a standard http response header
          client.println("HTTP/1.1 200 OK");
          client.println("Content-Type: text/html");
          client.println();
          
          WeatherData weatherData;
          if (handleWeatherShieldValues(weatherData)) {
            client.print("Temperature: ");
            client.print(weatherData.fTemperature);
            client.print("C (");
            client.print(weatherData.shTemperature);
            client.println(")<br />");
            
            client.print("Humidity: ");
            client.print(weatherData.fHumidity);
            client.print("% (");
            client.print(weatherData.shHumidity);
            client.println(")<br />");
            
            client.print("Pressure: ");
            client.print(weatherData.fPressure);
            client.print("kPa (");
            client.print(weatherData.shPressure);
            client.println(")<br />");
            
          } else
            client.print("Error reading from the WeatherShield1");
            
          client.println("<br/>");
          break;
        }
        if (c == '\n') {
          // we're starting a new line
          current_line_is_blank = true;
        } else if (c != '\r') {
          // we've gotten a character on the current line
          current_line_is_blank = false;
        }
      }
    }
    // give the web browser time to receive the data
    delay(1);
    client.stop();
  }  
}

/* ----------------------------------------------------------------- */

/* This function will read weather shield values and store in the
provided container. It returns true if the values are ready to be used, 
false if something goes wrong with the WeatherShield1 */
boolean handleWeatherShieldValues(WeatherData &weatherData) {
  
  /* This is the buffer for the answers */
  unsigned char ucBuffer[RXBUFFERLENGTH];
  
  /* Check for the weather shield connection and read values */
  if (weatherShield.sendCommand(CMD_ECHO_PAR, 100, ucBuffer)) {
    
    /* Start reading temperature */
    if (weatherShield.sendCommand(CMD_GETTEMP_C_AVG, 0, ucBuffer))
      weatherData.fTemperature = weatherShield.decodeFloatValue(ucBuffer);
    if (weatherShield.sendCommand(CMD_GETTEMP_C_RAW, 0, ucBuffer))
      weatherData.shTemperature = weatherShield.decodeShortValue(ucBuffer);
    
    /* Read pressure values */
    if (weatherShield.sendCommand(CMD_GETPRESS_AVG, 0, ucBuffer))
      weatherData.fPressure = weatherShield.decodeFloatValue(ucBuffer);
    if (weatherShield.sendCommand(CMD_GETPRESS_RAW, 0, ucBuffer))
      weatherData.shPressure = weatherShield.decodeShortValue(ucBuffer);
    
    /* Read humidity values */
    if (weatherShield.sendCommand(CMD_GETHUM_AVG, 0, ucBuffer))
      weatherData.fHumidity = weatherShield.decodeFloatValue(ucBuffer);
    if (weatherShield.sendCommand(CMD_GETHUM_RAW, 0, ucBuffer))
      weatherData.shHumidity = weatherShield.decodeShortValue(ucBuffer);

    return true;
  }  
  
  return false;
}


