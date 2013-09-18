
/* -----------------------------------------------------------------
    Weather Shield 1 recovery sample sketch
                               www.EtherMania.com - Signorini Marco

    This sketch could be used when you have problem communicating 
    with the WeatherShield1 because you (or for an invalid message
    sent to the shield, changed the address stored in the Weather
    Shield internal EEPROM. It restores the address to the factory
    value (1) and set the sampling period to 30 seconds   
  ------------------------------------------------------------------- */

#include <WeatherShield1.h>

#define RXBUFFERLENGTH          4

#define WEATHERSHIELD_DEFAULTADDRESS   0x01
#define WEATHERSHIELD_DEFAULTSAMPLETIME  29
#define IODATA_PIN    2
#define CLOCK_PIN     7

char done;

/* This is the main sketch setup handler */
void setup(){

  /* Initialize the serial port */
  Serial.begin(9600);
  
  done = 0;
}


/* This is the main sketch loop handler */
void loop() {
  
  /* This is the buffer for the answers */
  unsigned char ucBuffer[RXBUFFERLENGTH];
 
  /* Loop on every address only if not already found a valid shield */
  for (unsigned char ucN = 0; (ucN <= 255) && (done == 0); ucN++) {
    
    Serial.print("Checking with address");
    Serial.println(ucN);
    
    WeatherShield1 weatherShield(CLOCK_PIN, IODATA_PIN, ucN);
  
    /* Check for the weather shield connection */
    if (weatherShield.sendCommand(CMD_ECHO_PAR, 100, ucBuffer)) {
      Serial.println("Connection With Shield Performed OK");
      
      Serial.print("Set default sample time...");
      if (weatherShield.sendCommand(CMD_SET_SAMPLETIME, WEATHERSHIELD_DEFAULTSAMPLETIME, ucBuffer)) {
        Serial.println("Ok");
      }
      
      Serial.print("Going back to default address...");
      
      if (weatherShield.sendCommand(CMD_SETADDRESS, WEATHERSHIELD_DEFAULTADDRESS, ucBuffer)) {
          Serial.println("Ok");
      }
      
      Serial.println("Stop the skecth, power off your board and remove the WeatherShield");
      done++;
    }
  }
}
