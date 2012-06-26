/* -----------------------------------------------------------------
    Weather Shield 1 communication sample sketch
                               www.EtherMania.com - Signorini Marco
   
   This is a sample sketch that shows how to communicate to the
   Weather Shield and an Arduino 2009 board
  ------------------------------------------------------------------- */
  
#include <WeatherShield1.h>


#define RXBUFFERLENGTH          4

WeatherShield1 weatherShield;

// --- from here ----
// You can use next lines instead of the previous one if you need to specify a
// shield address different from the default factory programmed one 
// (because you already changed issuing the proper command) or if you
// connected the shield in a custom way.

//#define IODATA_PIN    2
//#define CLOCK_PIN     7
//#define MY_WEATHERSHIELD_ADDRESS   10
//WeatherShield1 weatherShield(CLOCK_PIN, IODATA_PIN, MY_WEATHERSHIELD_ADDRESS);

// ---- till here ----

/* This is the main sketch setup handler */
void setup(){

  /* Initialize the serial port */
  Serial.begin(9600);
}

/* This is the main sketch loop handler */
void loop() {
  
  /* This is the buffer for the answers */
  unsigned char ucBuffer[RXBUFFERLENGTH];
  
  /* Check for the weather shield connection */
  if (weatherShield.sendCommand(CMD_ECHO_PAR, 100, ucBuffer)) {
    Serial.println("Connection With Shield Performed OK");
  }
  
  /* Start reading temperature */
  float fTemperature = 0.0f;
  unsigned short shTemperature = 0;
  if (weatherShield.sendCommand(CMD_GETTEMP_C_AVG, 0, ucBuffer))
    fTemperature = weatherShield.decodeFloatValue(ucBuffer);
  if (weatherShield.sendCommand(CMD_GETTEMP_C_RAW, PAR_GET_LAST_SAMPLE, ucBuffer))
    shTemperature = weatherShield.decodeShortValue(ucBuffer);
    

  /* Read pressure values */
  float fPressure = 0;
  unsigned short shPressure = 0;
  if (weatherShield.sendCommand(CMD_GETPRESS_AVG, 0, ucBuffer))
    fPressure = weatherShield.decodeFloatValue(ucBuffer);
  if (weatherShield.sendCommand(CMD_GETPRESS_RAW, PAR_GET_LAST_SAMPLE, ucBuffer))
    shPressure = weatherShield.decodeShortValue(ucBuffer);
    
  
  /* Read humidity values */
  float fHumidity = 0;
  unsigned short shHumidity = 0;
  if (weatherShield.sendCommand(CMD_GETHUM_AVG, 0, ucBuffer))
    fHumidity = weatherShield.decodeFloatValue(ucBuffer);
  if (weatherShield.sendCommand(CMD_GETHUM_RAW, PAR_GET_LAST_SAMPLE, ucBuffer))
    shHumidity = weatherShield.decodeShortValue(ucBuffer);
    
    
  /* Send all data through the serial line */
  Serial.print("Temperature ");
  Serial.print(fTemperature);
  Serial.print(" Celsius (");
  Serial.print(shTemperature);
  Serial.println(")");
  
  Serial.print ("Pressure: ");
  Serial.print(fPressure);
  Serial.print(" kPa (");
  Serial.print(shPressure);
  Serial.println(")");
  
  Serial.print("Humidity: ");
  Serial.print(fHumidity);
  Serial.print(" % (");
  Serial.print(shHumidity);
  Serial.println(")");
    
  /* Wait some time before running again */
  delay(1000);
}

